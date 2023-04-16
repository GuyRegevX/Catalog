using Catalog.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Catalog.Repositories;

public class MongoDbItemsRepository: IItemRepository
{
    private const string DATABASE_NAME   = "catalog";
    private const string COLLECTION_NAME = "items";

    private readonly IMongoCollection<Item> itemCollection;
    private readonly FilterDefinitionBuilder<Item> filterBuilder;
    public MongoDbItemsRepository(IMongoClient mongoClient )
    {
        IMongoDatabase database = mongoClient.GetDatabase(DATABASE_NAME);
        itemCollection = database.GetCollection<Item>(COLLECTION_NAME);
        filterBuilder = Builders<Item>.Filter;
    }

    public async Task<Item> GetItemAsync(Guid id)
    {
        var filter = filterBuilder.Eq(item => item.Id, id);
        return await itemCollection.Find(filter).SingleOrDefaultAsync();
    }
    
    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        return await itemCollection.Find(new BsonDocument()).ToListAsync();
    }

    public async Task CreateItemAsync(Item item)
    {
        await itemCollection.InsertOneAsync(item);
    }

    public async Task UpdateItemAsync(Item item)
    {
        var filter = filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
        await itemCollection.ReplaceOneAsync(filter, item);
    }

    public async Task DeleteItemAsync(Guid id)
    {
        var filter = filterBuilder.Eq(existingItem => existingItem.Id, id);
        await itemCollection.DeleteOneAsync(filter);
    }
}