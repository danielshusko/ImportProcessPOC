namespace ImportProcessPOC.Models;

public record Item(Guid TenantId, int Id, int? ParentId, string Code, string Name);