namespace MyTodos.SharedKernel.Contracts;

public interface IEntity
{
    void SetCreatedInfo(string username);

    void SetUpdatedInfo(string username);
}