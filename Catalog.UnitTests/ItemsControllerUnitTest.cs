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
    public async Task GetItemAsync_WithUnExistingItem_ReturnsNotFound()
    {
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))!.ReturnsAsync((Item)null!);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        var result = await controller.GetItemAsync(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundResult>();
    }
    
    

    [Fact]
    public async Task GetItemAsync_WithExistingItem_ReturnsExpectedItem()
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
        dto.Should().BeEquivalentTo(expectedItem);
            
    }

    [Fact]
    public async Task GetItemsAsync_WithExistingItems_ReturnsAllItems()
    {
        //Arrange
        var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };
        repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(expectedItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var actualItems = await controller.GetItemsAsync();
        
        //Assert
        actualItems.Should().BeEquivalentTo(expectedItems);
    }
    
    [Fact]
    public async Task GetItemsAsync_WithMatchingItems_ReturnsMatchingItems()
    {
        //Arrange
        var allItems  = new[]
        {
            new Item(){Name = "Potion"},
            new Item(){Name = "Antidote"},
            new Item(){Name = "Hi-Potion"},
        };

        var nameToMatch = "Potion";
        
        repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(allItems);
        
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMatch);

        //Assert
        foundItems.Should().OnlyContain(item => item.Name == allItems[0].Name || item.Name == allItems[2].Name);
    }

    [Fact]
    public Task CreateItemsAsync_WithItemToCreate_ReturnsCreatedItem()
    {
        var itemToCreate = new CreateItemDto(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            rand.Next(100));
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        var result = controller.CreateItemAsync(itemToCreate);
        var createdItem =  (result.Result.Result as CreatedAtActionResult).Value as ItemDto;
        createdItem.Should().BeEquivalentTo(itemToCreate);
        createdItem.Id.Should().NotBeEmpty();
        createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(3000));
        return Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateItemAsAsync_ItemExists_ReturnsNoContent()
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