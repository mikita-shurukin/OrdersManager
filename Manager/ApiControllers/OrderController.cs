﻿using Microsoft.AspNetCore.Mvc;
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

            var allItems = await _dbContext.Products.ToListAsync();

            ViewBag.Items = allItems;

            var updateOrderModel = new UpdateOrder
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                ItemIds = order.Items.Select(i => i.Id).ToList()
            };

            return View(updateOrderModel);
        }

        [HttpPost("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateOrder request)
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

            var items = await _dbContext.Products.Where(p => request.ItemIds.Contains(p.Id)).ToListAsync();

            order.OrderDate = request.OrderDate;
            order.CustomerName = request.CustomerName;
            order.CustomerEmail = request.CustomerEmail;

            order.Items.Clear();

            order.Items.AddRange(items);

            try
            {
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Get");
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
            var itemToDelete = await _dbContext.Orders.FindAsync(id);

            if (itemToDelete == null)
            {
                return NotFound();
            }

            _dbContext.Orders.Remove(itemToDelete);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _dbContext.Orders.Any(e => e.Id == id);
        }
    }
}
