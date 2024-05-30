using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Manager.ApiControllers;
using Manager.DAL;
using Manager.DAL.Models;
using Manager.Models.Requests;
using AutoMapper;

namespace TestMyProject
{
    public class ItemControllerTests
    {
        private readonly IMapper _mapper;

        public ItemControllerTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateItem, Item>();
                cfg.CreateMap<UpdateItem, Item>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Get_ReturnsAllItems()
        {
            // Arrange
            var items = new List<Item>
            {
                new Item { Id = 1, Name = "Item 1" },
                new Item { Id = 2, Name = "Item 2" }
            };
            var dbContext = CreateDbContext(items);
            var controller = new ItemController(dbContext, _mapper);

            // Act
            var result = await controller.Get() as ViewResult;
            var model = result.Model as List<Item>;

            // Assert
            Assert.NotNull(model);
            Assert.Equal(items.Count, model.Count);
            Assert.Equal(items.Select(i => i.Name), model.Select(i => i.Name));
        }

        [Fact]
        public async Task Update_UpdatesExistingItem()
        {
            // Arrange
            var items = new List<Item>
            {
                new Item { Id = 1, Name = "Item 1" }
            };
            var dbContext = CreateDbContext(items);
            var controller = new ItemController(dbContext, _mapper);
            var request = new UpdateItem { Id = 1, Name = "Updated Item" };

            // Act
            var result = await controller.Update(1, request) as NoContentResult;

            // Assert
            Assert.NotNull(result);
            var updatedItem = dbContext.Products.FirstOrDefault(i => i.Id == 1);
            Assert.NotNull(updatedItem);
            Assert.Equal(request.Name, updatedItem.Name);
        }

        [Fact]
        public async Task Delete_RemovesItem()
        {
            // Arrange
            var items = new List<Item>
            {
                new Item { Id = 1, Name = "Item 1" }
            };
            var dbContext = CreateDbContext(items);
            var controller = new ItemController(dbContext, _mapper);

            // Act
            var result = await controller.Delete(1) as NoContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, dbContext.Products.Count());
        }

        private MainDbContext CreateDbContext(List<Item> items)
        {
            var options = new DbContextOptionsBuilder<MainDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new MainDbContext(options);

            foreach (var item in items)
            {
                item.Description = "Sample description";
            }

            dbContext.Products.AddRange(items);
            dbContext.SaveChanges();

            return dbContext;
        }

    }
}
