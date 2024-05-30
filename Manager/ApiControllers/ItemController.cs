using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Manager.DAL;
using Manager.DAL.Models;
using AutoMapper;
using Manager.Models.Requests;

namespace Manager.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : Controller
    {
        private readonly MainDbContext _dbContext;
        private readonly IMapper _mapper;

        public ItemController(MainDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var items = await _dbContext.Products.AsNoTracking().ToListAsync();
            return View(items);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateItem request)
        {
            var newItemGroup = _mapper.Map<Item>(request);
            _dbContext.Products.Add(newItemGroup);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("Update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var item = await _dbContext.Products.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var updateItem = _mapper.Map<UpdateItem>(item);
            return View(updateItem);
        }

        [HttpPost("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateItem request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            var item = await _dbContext.Products.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _mapper.Map(request, item);

            try
            {
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var itemToDelete = await _dbContext.Products.FindAsync(id);

            if (itemToDelete == null)
            {
                return NotFound();
            }

            _dbContext.Products.Remove(itemToDelete);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(int id)
        {
            return _dbContext.Products.Any(e => e.Id == id);
        }
    }
}
