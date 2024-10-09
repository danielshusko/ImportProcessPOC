namespace ImportProcessPOC.Database.EF.Models;

public class ImportJobLineDataModel
{
    public int JobId { get; set; }

    public int Index { get; set; }
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Line { get; set; }

    public virtual ImportJobDataModel Job { get; set; }
}