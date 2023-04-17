using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.UnitTests;

public class ItemsControllerUnitTest
{
    private readonly Mock<IItemRepository> repositoryStub = new();
    private readonly Mock<ILogger<ItemsController>> loggerStub = new();
    private readonly Random rand = new();
    [Fact]
    public async Task GetItemSync_WithUnExistingItem_ReturnsNotFound()
    {
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))!.ReturnsAsync((Item)null!);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        var result = await controller.GetItemAsync(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundResult>();
    }
    
    

    [Fact]
    public async Task GetItemSync_WithExistingItem_ReturnsExpectedItem()
    {
        //Arrange
        var expectedItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync((expectedItem));
        //Act
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        var result = await controller.GetItemAsync(Guid.NewGuid());
        
        //Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var dto = (ItemDto)(result.Result as OkObjectResult)?.Value!;
        dto.Should().BeEquivalentTo(
                expectedItem,
            options => options.ComparingByMembers<Item>()
                );
            
    }

    [Fact]
    public async Task GetItemsSync_WithExistingItems_ReturnsAllItems()
    {
        //Arrange
        var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };
        repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(expectedItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var actualItems = await controller.GetItemsAsync();
        
        //Assert
        actualItems.Should().BeEquivalentTo(
            expectedItems,
            options => options.ComparingByMembers<Item>()
        );
    }

    [Fact]
    public Task CreateItemsSync_WithItemToCreate_ReturnsCreatedItem()
    {
        var itemToCreate = new CreateItemDto()
        {
            Name = Guid.NewGuid().ToString(),
            Price = rand.Next(100)
        };
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        var result = controller.CreateItemAsync(itemToCreate);
        var createdItem =  (result.Result.Result as CreatedAtActionResult).Value as ItemDto;
        createdItem.Should().BeEquivalentTo(
            itemToCreate,
            options => options.ComparingByMembers<Item>().ExcludingMissingMembers()
        );
        createdItem.Id.Should().NotBeEmpty();
        createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(3000));
        return Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateItemSync_WithItemToUpdate_ReturnsNoContent()
    {
        var expectedItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync((expectedItem));

        var itemId = expectedItem.Id;
        var itemToUpdate = new UpdateItemDto()
        {
            Name = Guid.NewGuid().ToString(),
            Price = expectedItem.Price + 3
        };
            
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        var result = await controller.UpdateItem(itemId, itemToUpdate);
        result.Should().BeOfType<NoContentResult>();
    }
    
    [Fact]
    public async Task DeleteItemSync_ItemExists_ReturnsNoContent()
    {
        var expectedItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync((expectedItem));

            
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        var result = await controller.DeleteItem(expectedItem.Id);
        result.Should().BeOfType<NoContentResult>();
    }

    private Item CreateRandomItem()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Price = rand.Next(100),
            CreatedDate = DateTimeOffset.UtcNow
        };
    }
}