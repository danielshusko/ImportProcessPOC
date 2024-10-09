namespace ImportProcessPOC.Database.EF.Models;

public class ImportJobHeaderDataModel
{
    public int JobId { get; set; }

    public int Index { get; set; }
    public string Header { get; set; }

    public virtual ImportJobDataModel Job { get; set; }
}