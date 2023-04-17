using Catalog.Api.Entities;
using Catalog.Api.Entities;

namespace Catalog.Api.Repositories;

public interface IItemRepository
{
    Task<Item> GetItemAsync(Guid id);
    Task<IEnumerable<Item>> GetItemsAsync();

    Task CreateItemAsync(Item item);

    Task UpdateItemAsync(Item item);

    Task DeleteItemAsync(Guid id);
}