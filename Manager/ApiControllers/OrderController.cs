using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Manager.DAL;
using Manager.DAL.Models;
using AutoMapper;
using Manager.Models.Requests;

namespace Manager.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly MainDbContext _dbContext;
        private readonly IMapper _mapper;

        public OrderController(MainDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var orders = await _dbContext.Orders.Include(o => o.Items).AsNoTracking().ToListAsync();
            return View(orders);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateOrder request)
        {
            var itemIdsString = string.Join(",", request.ItemIds);

            var itemIds = itemIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(int.Parse)
                                       .ToList();

            var items = await _dbContext.Products.Where(i => itemIds.Contains(i.Id)).ToListAsync();

            var newOrder = new Order
            {
                OrderDate = request.OrderDate,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                Items = items
            };

            _dbContext.Orders.Add(newOrder);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("Update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Получаем список всех товаров из базы данных
            var allItems = await _dbContext.Products.ToListAsync();

            // Записываем список всех товаров в ViewBag, чтобы использовать его во вьювере
            ViewBag.Items = allItems;

            return View(order);
        }

        [HttpPost("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] Order request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Обновляем поля заказа
            order.OrderDate = request.OrderDate;
            order.CustomerName = request.CustomerName;
            order.CustomerEmail = request.CustomerEmail;
            order.Items = request.Items;

            // Сохраняем изменения в базе данных
            try
            {
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Get"); // Перенаправляем на страницу просмотра заказов после успешного обновления
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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
            var orderToDelete = await _dbContext.Orders.FindAsync(id);
            if (orderToDelete == null)
            {
                return NotFound();
            }

            _dbContext.Orders.Remove(orderToDelete);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _dbContext.Orders.Any(e => e.Id == id);
        }
    }
}
