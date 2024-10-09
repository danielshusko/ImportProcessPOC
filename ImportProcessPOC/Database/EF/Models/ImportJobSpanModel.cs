namespace ImportProcessPOC.Database.EF.Models;

public class ImportJobSpanModel
{
    public long Id { get; set; }
    public int JobId { get; set; }
    public string Name { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }

    public virtual ImportJobDataModel Job { get; set; }
}