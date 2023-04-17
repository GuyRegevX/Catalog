using Catalog.Entities;

namespace Catalog.Repositories;

public interface IItemRepository
{
    Task<Item> GetItemAsync(Guid id);
    Task<IEnumerable<Item>> GetItemsAsync();

    Task CreateItemAsync(Item item);

    Task UpdateItemAsync(Item item);

    Task DeleteItemAsync(Guid id);
}