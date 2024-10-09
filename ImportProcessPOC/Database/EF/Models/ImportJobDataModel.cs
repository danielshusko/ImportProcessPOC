namespace ImportProcessPOC.Database.EF.Models;

public class ImportJobDataModel
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string Importer { get; set; }
    public int ItemCount { get; set; }
    public bool IsOrdered { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }

    public virtual ICollection<ImportJobHeaderDataModel> Headers { get; set; } = [];
    public virtual ICollection<ImportJobLineDataModel> Lines { get; set; } = [];
    public virtual ICollection<ImportJobLineQueueDataModel> LineQueue { get; set; } = [];
    public virtual ICollection<ImportJobSpanModel> Spans { get; set; } = [];
}