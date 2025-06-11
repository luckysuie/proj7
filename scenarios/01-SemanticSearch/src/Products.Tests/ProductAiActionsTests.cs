using Microsoft.EntityFrameworkCore;
using Products.Models;
using DataEntities;
using Products.Endpoints;
using Products.Memory;
using SearchEntities;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace Products.Tests
{
    [TestClass]
    public sealed class ProductAiActionsTests
    {
        private DbContextOptions<Context> _dbOptions;

        [TestInitialize]
        public void TestInit()
        {
            // Use a unique database name for each test run to ensure isolation
            _dbOptions = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [TestMethod]
        public async Task AISearch_WithValidSearch_ReturnsOkWithSearchResponse()
        {
            // Arrange
            var searchQuery = "test search";
            var expectedResponse = new SearchResponse
            {
                Response = "Found some products",
                Products = new List<Product>
                {
                    new Product { Id = 1, Name = "Test Product", Description = "Test Description", Price = 10.0m, ImageUrl = "test.jpg" }
                }
            };

            using var context = new Context(_dbOptions);
            var mockMemoryContext = new MockMemoryContext(expectedResponse);

            // Act
            var result = await ProductAiActions.AISearch(searchQuery, context, mockMemoryContext);

            // Assert
            var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<SearchResponse>;
            Assert.IsNotNull(okResult, "Result should be Ok with SearchResponse");
            Assert.AreSame(expectedResponse, okResult.Value, "Should return the same SearchResponse from MemoryContext");
            Assert.AreEqual("Found some products", okResult.Value.Response);
            Assert.AreEqual(1, okResult.Value.Products.Count);
            Assert.AreEqual("Test Product", okResult.Value.Products[0].Name);
        }

        [TestMethod]
        public async Task AISearch_WithEmptySearch_ReturnsOkWithSearchResponse()
        {
            // Arrange
            var searchQuery = "";
            var expectedResponse = new SearchResponse
            {
                Response = "No search query provided",
                Products = new List<Product>()
            };

            using var context = new Context(_dbOptions);
            var mockMemoryContext = new MockMemoryContext(expectedResponse);

            // Act
            var result = await ProductAiActions.AISearch(searchQuery, context, mockMemoryContext);

            // Assert
            var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<SearchResponse>;
            Assert.IsNotNull(okResult, "Result should be Ok with SearchResponse");
            Assert.AreSame(expectedResponse, okResult.Value, "Should return the same SearchResponse from MemoryContext");
            Assert.AreEqual("No search query provided", okResult.Value.Response);
            Assert.AreEqual(0, okResult.Value.Products.Count);
        }

        [TestMethod]
        public async Task AISearch_WithNullSearch_ReturnsOkWithSearchResponse()
        {
            // Arrange
            string searchQuery = null!;
            var expectedResponse = new SearchResponse
            {
                Response = "Invalid search query",
                Products = new List<Product>()
            };

            using var context = new Context(_dbOptions);
            var mockMemoryContext = new MockMemoryContext(expectedResponse);

            // Act
            var result = await ProductAiActions.AISearch(searchQuery, context, mockMemoryContext);

            // Assert
            var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<SearchResponse>;
            Assert.IsNotNull(okResult, "Result should be Ok with SearchResponse");
            Assert.AreSame(expectedResponse, okResult.Value, "Should return the same SearchResponse from MemoryContext");
        }

        [TestMethod]
        public async Task AISearch_WithSearchException_ThrowsException()
        {
            // Arrange
            var searchQuery = "test search";
            var exceptionMessage = "Search service unavailable";

            using var context = new Context(_dbOptions);
            var mockMemoryContext = new MockMemoryContextWithException(exceptionMessage);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await ProductAiActions.AISearch(searchQuery, context, mockMemoryContext));
            
            Assert.AreEqual(exceptionMessage, exception.Message);
        }

        [TestMethod]
        public async Task AISearch_CallsMemoryContextSearchWithCorrectParameters()
        {
            // Arrange
            var searchQuery = "specific search term";
            var expectedResponse = new SearchResponse();

            using var context = new Context(_dbOptions);
            var mockMemoryContext = new MockMemoryContextWithTracking(expectedResponse);

            // Act
            await ProductAiActions.AISearch(searchQuery, context, mockMemoryContext);

            // Assert
            Assert.IsTrue(mockMemoryContext.SearchWasCalled, "Search method should be called");
            Assert.AreEqual(searchQuery, mockMemoryContext.LastSearchQuery, "Search should be called with correct query");
            Assert.AreSame(context, mockMemoryContext.LastDbContext, "Search should be called with correct context");
        }
    }

    // Test double for MemoryContext
    internal class MockMemoryContext : MemoryContext
    {
        private readonly SearchResponse _responseToReturn;

        public MockMemoryContext(SearchResponse responseToReturn) 
            : base(new MockLogger(), null, null)
        {
            _responseToReturn = responseToReturn;
        }

        public override async Task<SearchResponse> Search(string search, Context db)
        {
            // Simulate async operation
            await Task.Delay(1);
            return _responseToReturn;
        }
    }

    // Test double that throws exception
    internal class MockMemoryContextWithException : MemoryContext
    {
        private readonly string _exceptionMessage;

        public MockMemoryContextWithException(string exceptionMessage) 
            : base(new MockLogger(), null, null)
        {
            _exceptionMessage = exceptionMessage;
        }

        public override async Task<SearchResponse> Search(string search, Context db)
        {
            await Task.Delay(1);
            throw new InvalidOperationException(_exceptionMessage);
        }
    }

    // Test double that tracks method calls
    internal class MockMemoryContextWithTracking : MemoryContext
    {
        private readonly SearchResponse _responseToReturn;

        public MockMemoryContextWithTracking(SearchResponse responseToReturn) 
            : base(new MockLogger(), null, null)
        {
            _responseToReturn = responseToReturn;
        }

        public bool SearchWasCalled { get; private set; }
        public string? LastSearchQuery { get; private set; }
        public Context? LastDbContext { get; private set; }

        public override async Task<SearchResponse> Search(string search, Context db)
        {
            SearchWasCalled = true;
            LastSearchQuery = search;
            LastDbContext = db;
            
            await Task.Delay(1);
            return _responseToReturn;
        }
    }

    // Mock logger for test doubles
    internal class MockLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}