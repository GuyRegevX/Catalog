using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

//Get => /items
[ApiController]
[Route("items")]
public class ItemsController: ControllerBase
{
        private readonly IItemRepository _repository;
        private readonly ILogger<ItemsController> logger;

        public ItemsController(IItemRepository repository, ILogger<ItemsController> logger)
        {
                _repository = repository;
                this.logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetItemsAsync()
        {
                var items = (await _repository.GetItemsAsync())
                                        .Select(item => item.AsDto());
                logger.LogInformation($"{DateTime.UtcNow:hh:mm:ss} : Retrieves {items.Count()} items");
                return items;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id)
        {
                var item = await _repository.GetItemAsync(id) ;
                if (item is null)
                {
                        return NotFound();
                }
                return Ok(item.AsDto());
        }
        
        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto createItemDto)
        {
                Item item = new()
                {
                        Id = Guid.NewGuid(),
                        Name = createItemDto.Name,
                        Price = createItemDto.Price,
                        CreatedDate = DateTimeOffset.UtcNow
                };
                await _repository.CreateItemAsync(item) ;
                return CreatedAtAction( nameof(GetItemAsync), new { id = item.Id }, item.AsDto());
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItem(Guid id, UpdateItemDto updateItemDto)
        {
                var existingItem = await _repository.GetItemAsync(id);
                if (existingItem is null)
                {
                        return NotFound();
                }
                Item item = existingItem with
                {
                        Name = updateItemDto.Name,
                        Price = updateItemDto.Price
                };
                await _repository.UpdateItemAsync(item) ;
                return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
                var existingItem = await _repository.GetItemAsync(id);
                if (existingItem is null)
                {
                        return NotFound();
                }
                await _repository.DeleteItemAsync(id);
                return NoContent();
        }

}