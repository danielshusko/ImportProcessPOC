namespace ImportProcessPOC.Database.EF.Models;

public class ItemDataModel
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public Guid TenantId { get; set; }

    public virtual ItemDataModel Parent { get; set; }
    public virtual ICollection<ItemDataModel> Children { get; set; } = [];
}