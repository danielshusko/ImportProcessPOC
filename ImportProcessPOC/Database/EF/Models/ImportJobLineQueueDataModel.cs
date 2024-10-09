namespace ImportProcessPOC.Database.EF.Models;

public class ImportJobLineQueueDataModel
{
    public int JobId { get; set; }

    public int Index { get; set; }
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Line { get; set; }
    public bool IsProcessed { get; set; }

    public virtual ImportJobDataModel Job { get; set; }
}