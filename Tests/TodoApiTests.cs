using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.ViewModels;

namespace MinimalApi.Tests;

public class TodoApiTests
{

    [Fact]
    public async Task GetAllTodoItems_ReturnsOkResultOfIEnumerableTodoItems()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemsResult = await TodoApi.GetAllTodoItems(testDbContextFactory, user);

        Assert.IsType<Ok<PagedResults<TodoItemOutput>>>(todoItemsResult);
    }

    [Fact]
    public async Task GetTodoItemById_ReturnsOkResultOfTodoItem()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemResult = await TodoApi.GetTodoItemById(testDbContextFactory, user, 1);

        Assert.IsType<Ok<TodoItemOutput>>(todoItemResult);
    }

    [Fact]
    public async Task GetTodoItemById_ReturnsNotFound()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemResult = await TodoApi.GetTodoItemById(testDbContextFactory, user, 10);

        Assert.IsType<NotFound>(todoItemResult);
    }

    [Fact]
    public async Task CreateTodoItem_ReturnsCreatedStatusWithLocation()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));
        var title = "This todo item from Unit test";
        var todoItemInput = new TodoItemInput() { IsCompleted = false, Title = title };
        var todoItemOutputResult = await TodoApi.CreateTodoItem(
            testDbContextFactory, user, todoItemInput, new TodoItemInputValidator(testDbContextFactory));

        Assert.IsType<Created<TodoItemOutput>>(todoItemOutputResult);
        var createdTodoItemOutput = todoItemOutputResult as Created<TodoItemOutput>;
        Assert.Equal(201, createdTodoItemOutput!.StatusCode);
        var actual = createdTodoItemOutput!.Value!.Title;
        Assert.Equal(title, actual);
        var actualLocation = createdTodoItemOutput!.Location;
        var expectedLocation = $"/todoitems/4";
        Assert.Equal(expectedLocation, actualLocation);
    }

    [Fact]
    public async Task CreateTodoItem_ReturnsProblem()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemInput = new TodoItemInput();
        var todoItemOutputResult = await TodoApi.CreateTodoItem(testDbContextFactory, user, todoItemInput, new TodoItemInputValidator(testDbContextFactory));

        Assert.IsType<ValidationProblem>(todoItemOutputResult);
    }

    [Fact]
    public async Task UpdateTodoItem_ReturnsNoContent()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemInput = new TodoItemInput() { IsCompleted = true };
        var result = await TodoApi.UpdateTodoItem(testDbContextFactory, user, 1, todoItemInput);

        Assert.IsType<NoContent>(result);
        var updateResult = result as NoContent;
        Assert.NotNull(updateResult);
    }

    [Fact]
    public async Task UpdateTodoItem_ReturnsNotFound()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemInput = new TodoItemInput() { IsCompleted = true };
        var result = await TodoApi.UpdateTodoItem(testDbContextFactory, user, 5, todoItemInput);

        Assert.IsType<NotFound>(result);
        var updateResult = result as NotFound;
        Assert.NotNull(updateResult);
    }

    [Fact]
    public async Task DeleteTodoItem_ReturnsNoContent()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemInput = new TodoItemInput() { IsCompleted = true };
        var result = await TodoApi.DeleteTodoItem(testDbContextFactory, user, 1);

        Assert.IsType<NoContent>(result);
        var deleteResult = result as NoContent;
        Assert.NotNull(deleteResult);
    }

    [Fact]
    public async Task DeleteTodoItem_ReturnsNotFound()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "admin"));

        var todoItemInput = new TodoItemInput() { IsCompleted = true };
        var result = await TodoApi.DeleteTodoItem(testDbContextFactory, user, 5);

        Assert.IsType<NotFound>(result);
        var deleteResult = result as NotFound;
        Assert.NotNull(deleteResult);
    }

}

public class TestDbContextFactory : IDbContextFactory<TodoDbContext>
{
    private DbContextOptions<TodoDbContext> _options;

    public TestDbContextFactory(string databaseName = "InMemoryTest")
    {
        _options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    public TodoDbContext CreateDbContext()
    {
        var todoDbContext = new TodoDbContext(_options);
        todoDbContext.Database.EnsureCreated();
        return todoDbContext;
    }
}