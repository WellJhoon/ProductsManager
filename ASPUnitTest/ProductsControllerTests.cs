using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductsManager.Controllers;
using ProductsManager.Context;
using ProductsManager.Models;
using Xunit;

namespace ProductsManager.Tests
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task PostProducts_ReturnsCreatedAtAction()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var controller = new ProductsController(mockContext.Object);
            var product = new Products { Id = 1, ProductName = "Test Product", ProductDescription = "Test Description", price = 10, quantity = 5 };

            // Act
            var result = await controller.PostProducts(product);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetProducts", createdAtActionResult.ActionName);
            Assert.Equal(product.Id, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(product, createdAtActionResult.Value);
        }

        [Fact]
        public async Task Getproducts_ReturnsListOfProducts()
        {
            // Arrange
            var products = new List<Products>
            {
                new Products { Id = 1, ProductName = "Product1", ProductDescription = "Description1", price = 10, quantity = 5 },
                new Products { Id = 2, ProductName = "Product2", ProductDescription = "Description2", price = 20, quantity = 10 }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Products>>();
            mockDbSet.As<IQueryable<Products>>().Setup(m => m.Provider).Returns(products.Provider);
            mockDbSet.As<IQueryable<Products>>().Setup(m => m.Expression).Returns(products.Expression);
            mockDbSet.As<IQueryable<Products>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockDbSet.As<IQueryable<Products>>().Setup(m => m.GetEnumerator()).Returns(() => products.GetEnumerator());

            var mockContext = new Mock<AppDbContext>();
            mockContext.Setup(c => c.products).Returns(mockDbSet.Object);

            var controller = new ProductsController(mockContext.Object);

            // Act
            var result = await controller.Getproducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Products>>(okResult.Value);
            Assert.Equal(2, returnedProducts.Count());
        }

        [Fact]
        public async Task GetProducts_ReturnsNotFoundWhenProductNotExists()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var controller = new ProductsController(mockContext.Object);

            // Act
            var result = await controller.GetProducts(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutProducts_ReturnsBadRequestWhenIdMismatch()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var controller = new ProductsController(mockContext.Object);
            var product = new Products { Id = 1 };

            // Act
            var result = await controller.PutProducts(2, product);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteProducts_ReturnsNotFoundWhenProductNotExists()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var controller = new ProductsController(mockContext.Object);

            // Act
            var result = await controller.DeleteProducts(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Aquí puedes agregar más pruebas unitarias para otros métodos del controlador, como PutProducts, DeleteProducts, etc.
    }
}
