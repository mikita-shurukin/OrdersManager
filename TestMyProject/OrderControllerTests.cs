using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Manager.ApiControllers;
using Manager.DAL;
using Manager.DAL.Models;
using Manager.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestMyProject
{
    public class OrderControllerTests
    {
        private readonly IMapper _mapper;

        public OrderControllerTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateOrder, Order>();
                cfg.CreateMap<UpdateOrder, Order>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Get_ReturnsAllOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderDate = DateTime.Now, CustomerName = "Customer 1", CustomerEmail = "customer1@example.com" },
                new Order { Id = 2, OrderDate = DateTime.Now.AddDays(-1), CustomerName = "Customer 2", CustomerEmail = "customer2@example.com" }
            };
            var dbContext = CreateDbContext(orders);
            var controller = new OrderController(dbContext, _mapper);

            // Act
            var result = await controller.Get() as ViewResult;
            var model = result.Model as List<Order>;

            // Assert
            Assert.NotNull(model);
            Assert.Equal(orders.Count, model.Count);
            Assert.Equal(orders.Select(o => o.CustomerName), model.Select(o => o.CustomerName));
        }

        [Fact]
        public async Task Create_CreatesNewOrder()
        {
            // Arrange
            var dbContext = CreateDbContext(new List<Order>());
            var controller = new OrderController(dbContext, _mapper);
            var request = new CreateOrder
            {
                OrderDate = DateTime.Now,
                CustomerName = "New Customer",
                CustomerEmail = "newcustomer@example.com",
                ItemIds = new List<int> { 1, 2 }
            };

            // Act
            var result = await controller.Create(request) as OkResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, dbContext.Orders.Count());
            var createdOrder = dbContext.Orders.FirstOrDefault();
            Assert.NotNull(createdOrder);
            Assert.Equal(request.CustomerName, createdOrder.CustomerName);
            Assert.Equal(request.CustomerEmail, createdOrder.CustomerEmail);
            Assert.Equal(request.ItemIds.Count, createdOrder.Items.Count);
        }

        [Fact]
        public async Task Update_UpdatesExistingOrder()
        {
            // Arrange
            var orders = new List<Order>
    {
        new Order { Id = 1, OrderDate = DateTime.Now, CustomerName = "Customer 1", CustomerEmail = "customer1@example.com", Items = new List<Item>() }
    };
            var dbContext = CreateDbContext(orders);
            var controller = new OrderController(dbContext, _mapper);
            var request = new UpdateOrder
            {
                Id = 1,
                OrderDate = DateTime.Now.AddDays(-1),
                CustomerName = "Updated Customer",
                CustomerEmail = "updatedcustomer@example.com",
                ItemIds = new List<int> { 1 }
            };

            // Act
            var result = await controller.Update(1, request) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Get", result.ActionName);
            var updatedOrder = dbContext.Orders.FirstOrDefault(o => o.Id == 1);
            Assert.NotNull(updatedOrder);
            Assert.Equal(request.CustomerName, updatedOrder.CustomerName);
            Assert.Equal(request.CustomerEmail, updatedOrder.CustomerEmail);
            Assert.Equal(request.ItemIds.Count, updatedOrder.Items.Count);

            var savedChanges = await dbContext.SaveChangesAsync();
            Assert.Equal(1, savedChanges);
        }


        [Fact]
        public async Task Delete_RemovesOrder()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderDate = DateTime.Now, CustomerName = "Customer 1", CustomerEmail = "customer1@example.com" }
            };
            var dbContext = CreateDbContext(orders);
            var controller = new OrderController(dbContext, _mapper);

            // Act
            var result = await controller.Delete(1) as NoContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, dbContext.Orders.Count());
        }

        private MainDbContext CreateDbContext(List<Order> orders)
        {
            var options = new DbContextOptionsBuilder<MainDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new MainDbContext(options);

            dbContext.Orders.AddRange(orders);
            dbContext.SaveChanges();

            return dbContext;
        }
    }
}
