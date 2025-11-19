using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Enums;

namespace MyTodos.Services.TodoService.Application.Tasks.Queries.GetPagedList;

public sealed class TaskPagedListFilter : Filter
{
    public Guid? TaskId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Domain.TaskAggregate.Enums.TaskStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public bool? IsRecurring { get; set; }
    public DateTime? DueAfter { get; set; }
    public DateTime? DueBefore { get; set; }
}
