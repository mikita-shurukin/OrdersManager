using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Manager.DAL;
using System.Text.RegularExpressions;
using Manager.DAL.Models;
using AutoMapper;
using Manager.Models.Requests;

namespace Manager.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly MainDbContext _dbContext;
        private readonly IMapper _mapper;

        public CategoryController(MainDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> Get(int id)
        //{
        //    var result = await _dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        //    if (result is null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var itemGroup = await _dbContext.Categories.AsNoTracking().ToListAsync();
            return View(itemGroup);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateCategory request)
        {
            var newItemGroup = _mapper.Map<Category>(request);
            _dbContext.Categories.Add(newItemGroup);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("Update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var updateCategory = new UpdateCategory
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(updateCategory);
        }

        [HttpPost("Update/{id}")]
        public async Task<IActionResult> Update(int id,[FromForm] UpdateCategory request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = request.Name;

            try
            {
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Get));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CategoryExists(int id)
        {
            return _dbContext.Categories.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoryToDelete = await _dbContext.Categories.FindAsync(id);

            if (categoryToDelete == null)
            {
                return NotFound();
            }

            _dbContext.Categories.Remove(categoryToDelete);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
