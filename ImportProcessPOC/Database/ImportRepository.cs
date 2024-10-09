using ImportProcessPOC.Database.EF;
using ImportProcessPOC.Database.EF.Models;
using ImportProcessPOC.Models;
using ImportProcessPOC.Spans;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ImportProcessPOC.Database;

public class ImportRepository(SpanMetrics spanMetrics)
{
    private readonly ImportProcessDataContext _context = new();

    public int CreateJob(Guid tenantId, string importer, int itemCount, bool isOrdered)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.CreateJob");
        var job = new ImportJobDataModel
                  {
                      TenantId = tenantId,
                      Importer = importer,
                      ItemCount = itemCount,
                      IsOrdered = isOrdered,
                      StartDate = DateTimeOffset.UtcNow
                  };
        _context.ImportJobs.Add(job);
        _context.SaveChanges();
        return job.Id;
    }

    public void EndJob(int jobId)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.EndJob");
        var job = _context.ImportJobs.First(x => x.Id == jobId);
        job.EndDate = DateTimeOffset.UtcNow;
        _context.SaveChanges();
    }

    public void CreateSpans(int jobId)
    {
        var spanModels = spanMetrics.Metrics.Select(
            x => new ImportJobSpanModel
                 {
                     JobId = jobId,
                     Name = x.Name,
                     StartDate = x.StartDate,
                     EndDate = x.StartDate.Add(x.RunTime)
                 });
        _context.ImportJobSpans.AddRange(spanModels);
        _context.SaveChanges();
    }

    public void SaveImportHeaders(int jobId, Dictionary<int, string> headers)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.SaveImportHeaders");
        var headerModels = headers
            .Select(
                x => new ImportJobHeaderDataModel
                     {
                         JobId = jobId,
                         Index = x.Key,
                         Header = x.Value
                     });
        _context.ImportJobsHeaders.AddRange(headerModels);
        _context.SaveChanges();
    }

    public void SaveImportLines(int jobId, IEnumerable<FileLine> fileLines)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.SaveImportLines");
        var importLines = fileLines
            .Select(
                x => new ImportJobLineDataModel
                     {
                         JobId = jobId,
                         Index = x.Index,
                         Id = x.Id,
                         ParentId = x.ParentId,
                         Line = x.Line
                     });

        _context.ImportJobsLines.AddRange(importLines);
        _context.SaveChanges();
    }

    public void EnqueueImportLines(int jobId, IEnumerable<FileLine> fileLines)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.EnqueueImportLines");
        var importLines = fileLines
            .Select(
                x => new ImportJobLineQueueDataModel
                     {
                         JobId = jobId,
                         Index = x.Index,
                         Id = x.Id,
                         ParentId = x.ParentId,
                         Line = x.Line
                     });

        _context.ImportJobsLineQueue.AddRange(importLines);
        _context.SaveChanges();
    }

    public int GetJobImportLineCount(int jobId)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.SaveImportLines");
        return _context.ImportJobsLines.Count(x => x.JobId == jobId);
    }

    public Dictionary<int, string> GetImportLines(int jobId, int pageSize, int pageNumber)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.GetImportLines");

        var query = @"
WITH RECURSIVE nodes(JobId, Index, Id, ParentId, Line, Path) AS (
    select ijl.""JobId"", ijl.""Index"", ijl.""Id"", ijl.""ParentId"", ijl.""Line"", cast(ijl.""Id"" as text)
    from ""importProcessPoc"".""ImportJobLine"" ijl
    where ijl.""JobId"" = @jobId
    and ijl.""ParentId"" is null
  UNION all
  	select ijl2.""JobId"", ijl2.""Index"", ijl2.""Id"", ijl2.""ParentId"", ijl2.""Line"", concat(path, '->', cast(ijl2.""Id"" as text))
    from ""importProcessPoc"".""ImportJobLine"" ijl2
    join nodes n on n.id = ijl2.""ParentId""
    where ijl2.""JobId"" = @jobId
)
select JobId, Index, Id, ParentId, Line, Path
from nodes
where JobId = @jobId
order by path
limit @limit
offset @offset";

        var jobIdParameter = new NpgsqlParameter("jobId", jobId);
        var limitIdParameter = new NpgsqlParameter("limit", pageSize);
        var offsetIdParameter = new NpgsqlParameter("offset", pageSize * pageNumber);

        var items = _context.ImportJobsLines.FromSqlRaw(query, jobIdParameter, limitIdParameter, offsetIdParameter).ToList();
        return items.ToDictionary(x => x.Index, x => x.Line);
    }

    public Dictionary<int, string> GetImportLinesByJoin(int jobId, int pageSize)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.GetImportLines");

        var query = @"
select 
	ijl.*
from 
	""importProcessPoc"".""ImportJobLine"" ijl
	join ""importProcessPoc"".""ImportJob"" ij 
		on ijl.""JobId"" = ij.""Id"" 
	left join ""importProcessPoc"".""Item"" i
		on ijl.""Id"" = i.""Id"" 
		and ij.""TenantId"" = i.""TenantId"" 
	left join ""importProcessPoc"".""Item"" ip
		on ijl.""ParentId"" = ip.""Id"" 
		and ij.""TenantId"" = ip.""TenantId"" 
where 
	ijl.""JobId"" = @jobId
	and i.""Id"" is null
	and 
	(
		ijl.""ParentId"" is null
		or ip.""Id"" is not null
	)
limit @limit";

        var jobIdParameter = new NpgsqlParameter("jobId", jobId);
        var limitIdParameter = new NpgsqlParameter("limit", pageSize);

        var items = _context.ImportJobsLines.FromSqlRaw(query, jobIdParameter, limitIdParameter).ToList();
        return items.ToDictionary(x => x.Index, x => x.Line);
    }

    public Dictionary<int, string> DequeueImportLines(int jobId, int pageSize)
    {
        using var span = spanMetrics.CreateSpan("ImportRepository.DequeueImportLines");

        var query = @"
with cte as (
	select
		lq.""JobId"",
		lq.""Index"",
		lq.""Id"",
		lq.""ParentId"",
		lq.""Line"",
		lq.""IsProcessed"",
		j.""TenantId""
	from 
		""importProcessPoc"".""ImportJobLineQueue"" lq
		join ""importProcessPoc"".""ImportJob"" j
			on lq.""JobId"" = j.""Id"" 
		left join ""importProcessPoc"".""Item"" i 
			on lq.""ParentId"" = i.""Id"" 
			and j.""TenantId"" = i.""TenantId""
	where 
		lq.""JobId"" = @jobId
		and lq.""IsProcessed"" = false
		and 
		(
			lq.""ParentId"" is null
			or i.""Id"" is not null
		)
	for update of lq skip locked
	limit @limit
)
update ""importProcessPoc"".""ImportJobLineQueue"" lqu
set ""IsProcessed"" = true
from cte c
where 
	c.""Id"" = lqu.""Id""
	and c.""JobId"" = lqu.""JobId""
returning 
	c.""JobId"",
	c.""Index"",
	c.""Id"",
	c.""ParentId"",
	c.""Line"",
    c.""IsProcessed""
";

        var jobIdParameter = new NpgsqlParameter("jobId", jobId);
        var limitIdParameter = new NpgsqlParameter("limit", pageSize);

        var items = _context.ImportJobsLineQueue.FromSqlRaw(query, jobIdParameter, limitIdParameter).ToList();
        return items.ToDictionary(x => x.Index, x => x.Line);
    }

    public void SaveItems(ICollection<Item> items)
    {
        if (items.Any())
        {
            using var span = spanMetrics.CreateSpan("ImportRepository.SaveItems");
            var itemDataModels = items
                                 .Select(
                                     x => new ItemDataModel
                                          {
                                              TenantId = x.TenantId,
                                              Id = x.Id,
                                              ParentId = x.ParentId,
                                              Code = x.Code,
                                              Name = x.Name
                                          })
                                 .ToList();

            _context.Items.AddRange(itemDataModels);
            _context.SaveChanges();
        }
    }

    public void Reset() => _context.ChangeTracker.Clear();
}