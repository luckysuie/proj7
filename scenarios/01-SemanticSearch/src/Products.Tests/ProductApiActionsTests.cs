using Microsoft.EntityFrameworkCore;
using Products.Models;
using DataEntities;
using Products.Endpoints;

namespace Products.Tests
{
    [TestClass]
    public sealed class ProductApiActionsTests
    {
        private DbContextOptions<Context> _dbOptions;

        [TestInitialize]
        public void TestInit()
        {
            _dbOptions = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetAllProducts")
                .Options;
        }

        [TestMethod]
        public async Task GetAllProducts_ReturnsAllSeededProducts()
        {
            // Arrange
            using (var context = new Context(_dbOptions))
            {
                context.Product.AddRange(new List<Product>
                {
                    new Product { Id = 1, Name = "Test1", Description = "Desc1", Price = 10, ImageUrl = "img1" },
                    new Product { Id = 2, Name = "Test2", Description = "Desc2", Price = 20, ImageUrl = "img2" }
                });
                context.SaveChanges();
            }

            using (var context = new Context(_dbOptions))
            {
                // Act
                var result = await ProductApiActions.GetAllProducts(context);
                var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<List<Product>>;
                Assert.IsNotNull(okResult, "Result should be Ok with a list of products");
                var products = okResult.Value;
                Assert.AreEqual(2, products.Count, "Should return all seeded products");
                Assert.IsTrue(products.Any(p => p.Name == "Test1"));
                Assert.IsTrue(products.Any(p => p.Name == "Test2"));
            }
        }
    }
}
