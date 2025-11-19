using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.Services.TodoService.Domain.ProjectAggregate.Enums;

namespace MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;

public sealed class ProjectPagedListFilter : Filter
{
    public Guid? ProjectId { get; set; }
    public Guid? TenantId { get; set; }
    public string? CreatedBy { get; set; }
    public ProjectStatus? Status { get; set; }
    public DateTime? StartAfter { get; set; }
    public DateTime? StartBefore { get; set; }
    public DateTime? TargetAfter { get; set; }
    public DateTime? TargetBefore { get; set; }
}
