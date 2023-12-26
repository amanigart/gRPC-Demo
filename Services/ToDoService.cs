using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _dbContext;

    public ToDoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if (request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var todoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(todoItem);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = todoItem.Id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));

        var item = await _dbContext.ToDoItems.FirstOrDefaultAsync(item => item.Id == request.Id);
        if (item is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));

        return await Task.FromResult(new ReadToDoResponse
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            ToDoStatus = item.ToDoStatus
        });
    }

    public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();

        var items = await _dbContext.ToDoItems.ToListAsync();
        if (items is null)
            throw new RpcException(new Status(StatusCode.NotFound, "No items found ."));

        foreach (var item in items)
        {
            response.ToDo.Add(new ReadToDoResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                ToDoStatus = item.ToDoStatus
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var itemToUpdate = await _dbContext.ToDoItems.FirstOrDefaultAsync(item => item.Id == request.Id);
        if (itemToUpdate is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));

        itemToUpdate.Title = request.Title;
        itemToUpdate.Description = request.Description;
        itemToUpdate.ToDoStatus = request.ToDoStatus;

        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse
        {
            Id = itemToUpdate.Id,
        });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));

        var itemToDelete = await _dbContext.ToDoItems.FirstOrDefaultAsync(item => item.Id == request.Id);
        if (itemToDelete is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));

        _dbContext.Remove(itemToDelete);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse
        {
            Id = request.Id
        });
    }
}