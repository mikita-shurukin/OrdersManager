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
    public class CategoryControllerTests
    {
        private readonly IMapper _mapper;

        public CategoryControllerTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateCategory, Category>();
                cfg.CreateMap<UpdateCategory, Category>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Get_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            };
            var dbContext = CreateDbContext(categories);
            var controller = new CategoryController(dbContext, _mapper);

            // Act
            var result = await controller.Get() as ViewResult;
            var model = result.Model as List<Category>;

            // Assert
            Assert.NotNull(model);
            Assert.Equal(categories.Count, model.Count);
            Assert.Equal(categories.Select(c => c.Name), model.Select(c => c.Name));
        }


        [Fact]
        public async Task Update_UpdatesExistingCategory()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" }
            };
            var dbContext = CreateDbContext(categories);
            var controller = new CategoryController(dbContext, _mapper);
            var request = new UpdateCategory { Id = 1, Name = "Updated Category" };

            // Act
            var result = await controller.Update(1, request) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Get", result.ActionName);
            var updatedCategory = dbContext.Categories.FirstOrDefault(c => c.Id == 1);
            Assert.NotNull(updatedCategory);
            Assert.Equal(request.Name, updatedCategory.Name);
        }

        [Fact]
        public async Task Delete_RemovesCategory()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" }
            };
            var dbContext = CreateDbContext(categories);
            var controller = new CategoryController(dbContext, _mapper);

            // Act
            var result = await controller.Delete(1) as NoContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, dbContext.Categories.Count());
        }

        private MainDbContext CreateDbContext(List<Category> categories)
        {
            var options = new DbContextOptionsBuilder<MainDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new MainDbContext(options);

            dbContext.Categories.AddRange(categories);
            dbContext.SaveChanges();

            return dbContext;
        }
    }
}
