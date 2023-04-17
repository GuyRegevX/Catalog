using System;
using System.Threading.Tasks;
using Catalog.Controllers;
using Catalog.Dtos;
using Catalog.Entities;
using Catalog.Repositories;
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