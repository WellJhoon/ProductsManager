using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.EntityFrameworkCore;
using ProductsManager.Context;
using ProductsManager.Controllers;
using ProductsManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;

namespace TestProject1
{
    public class ProductTesting
    {
        [Fact]
        public async Task PostTasks_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var controller = new ProductsController(mockContext.Object);

            // Set up the mocked context behavior
            mockContext.Setup(context => context.products.Add(It.IsAny<Products>()));
            mockContext.Setup(context => context.SaveChangesAsync(default)).ReturnsAsync(1);

            // Create an invalid model state
            controller.ModelState.AddModelError("Description about product", "Description is required");

            // Act
            var result = await controller.PostProducts(new Products { ProductDescription = "Sample Description" });

            // Assert
            Assert.IsType<ActionResult<Products>>(result);
            Assert.Null((result as ActionResult<Products>).Value);
        }

        [Fact]
        public async Task GetTasks_ReturnsListOfProducts()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var controller = new ProductsController(mockContext.Object);

            // Set up the mocked context behavior
            var productsData = new List<Products>
            {
                new Products { Id = 1, ProductName = "Product 1", ProductDescription = "Description 1" },
                new Products { Id = 2, ProductName = "Product 2", ProductDescription = "Description 2" },
            };

            var mockDbSet = new Mock<DbSet<Products>>();

            mockDbSet.As<IAsyncEnumerable<Products>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Products>(productsData.GetEnumerator()));

            mockDbSet.As<IQueryable<Products>>().Setup(m => m.Provider).Returns(productsData.AsQueryable().Provider);
            mockDbSet.As<IQueryable<Products>>().Setup(m => m.Expression).Returns(productsData.AsQueryable().Expression);
            mockDbSet.As<IQueryable<Products>>().Setup(m => m.ElementType).Returns(productsData.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<Products>>().Setup(m => m.GetEnumerator()).Returns(() => productsData.GetEnumerator());

            mockContext.Setup(context => context.products).Returns(mockDbSet.Object);

            // Act
            var result = await controller.Getproducts();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Products>>>(result);
            var products = Assert.IsType<List<Products>>(actionResult.Value);

            Assert.Equal(2, products.Count);
            Assert.Equal("Product 1", products[0].ProductName);
            Assert.Equal("Product 2", products[1].ProductName);
        }

        [Fact]
        public async Task DeleteProducts_existingProducts_ReturnsNoContentResult()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var controller = new ProductsController(mockContext.Object);

            var existingProducts = new Products { Id = 1, ProductName = "Existing Task", ProductDescription = "Existing Description" };

            mockContext.Setup(context => context.products.FindAsync(It.IsAny<int>())).ReturnsAsync(existingProducts);

            // Act
            var result = await controller.DeleteProducts(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }

    public class AuthTesting
    {
        [Fact]
        public void Register_ReturnsOkResult()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new AuthController(mockConfiguration.Object);

            var newUserRequest = new UserDTO
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                Password = "testpassword"
            };

            // Act
            var result = controller.Register(newUserRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            var returnedUser = Assert.IsType<User>(okResult.Value);

            Assert.Equal(newUserRequest.Email, returnedUser.Email);
            // UserName comparison is not needed because it's now hashed
            // Assert.Equal(newUserRequest.UserName, returnedUser.UserName);
            Assert.NotNull(returnedUser.PasswordHash); // Check that Password is not null
                                                       // Optionally, you can assert other properties as needed.
        }
    }

    // Helper classes for asynchronous operations
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public ValueTask DisposeAsync() => default;
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression) =>
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression) =>
            _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) =>
            _inner.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) =>
            new TestAsyncEnumerable<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) =>
            Execute<TResult>(expression);
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
}
