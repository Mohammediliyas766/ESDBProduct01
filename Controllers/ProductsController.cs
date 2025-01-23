using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project1.Data;
using Project1.Models;
using Newtonsoft.Json;
using static Project1.Events.ProductEvent;
using EventStore.Client;
using System.Text;

namespace Project1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _context;
        private readonly IEventStoreService _eventStore;
        private readonly EventStoreClient _client;

        public ProductsController(ProductContext context, IEventStoreService eventStore)
        {
            _context = context;
            _eventStore = eventStore;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            product.Id = Guid.NewGuid();
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Store event
            await _eventStore.AppendEventAsync($"product-{product.Id}",
                new ProductCreated(product.Id, product.Name, product.Price, product.Description));

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _eventStore.AppendEventAsync($"product-{product.Id}",
                    new ProductUpdated(product.Id, product.Name, product.Price, product.Description));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await _eventStore.AppendEventAsync($"product-{id}", new ProductDeleted(id));

            return NoContent();
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        //[HttpGet("count")]
        //public async Task<ActionResult<int>> GetProductCount()
        //{
        //    var projectionState = await GetProjectionStateAsync<ProductCountState>("ProductCountProjection");
        //    return projectionState?.Count ?? 0;
        //}

        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllEvents()
        {
            try
            {
                var events = new List<object>();
                var productStreams = await _context.Products.Select(p => $"product-{p.Id}").ToListAsync();

                foreach (var streamName in productStreams)
                {
                    var streamEvents = await _eventStore.GetAllEventsAsync(streamName);
                    events.AddRange(streamEvents);
                }

                return Ok(events.OrderBy(e => ((dynamic)e).Created));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving events: {ex.Message}");
            }
        }



        [HttpGet("price-summary")]
        public async Task<ActionResult<ProductPriceSummaryState>> GetProductPriceSummary()
        {
            var projectionState = await GetProjectionStateAsync<ProductPriceSummaryState>("ProductPriceSummaryProjection");
            return projectionState ?? new ProductPriceSummaryState();
        }

        private async Task<T?> GetProjectionStateAsync<T>(string projectionName)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://localhost:2114/projection/{projectionName}/state");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        [HttpGet("grouped-events")]
        public async Task<ActionResult<IEnumerable<object>>> GetGroupedEvents()
        {
            try
            {
                var groupedEvents = await _eventStore.ReadGroupedEventsAsync();
                return Ok(groupedEvents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving grouped events: {ex.Message}");
            }
        }
    }

    public class ProductCountState
    {
        public int Count { get; set; }
    }

    public class ProductPriceSummaryState
    {
        public int Count { get; set; }
        public decimal Sum { get; set; }
        public decimal Avg { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        
    }
}