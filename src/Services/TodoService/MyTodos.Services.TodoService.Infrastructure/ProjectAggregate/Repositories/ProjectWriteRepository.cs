using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.TodoService.Application.Projects;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Application.Projects.Queries;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;
using MyTodos.Services.TodoService.Infrastructure.Persistence;

namespace MyTodos.Services.TodoService.Infrastructure.ProjectAggregate.Repositories;

public sealed class ProjectWriteRepository
    : WriteEfRepository<Project, Guid, TodoServiceDbContext>
    , IProjectWriteRepository
{
    public ProjectWriteRepository(TodoServiceDbContext context, ICurrentUserService currentUserService)
        : base(context, new ProjectQueryConfiguration(), currentUserService)
    {
    }
}
