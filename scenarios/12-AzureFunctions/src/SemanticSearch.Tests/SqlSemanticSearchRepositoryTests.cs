using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SemanticSearchFunction.Repositories;

namespace SemanticSearch.Tests;

[TestClass]
public class SqlSemanticSearchRepositoryTests
{
    private Mock<IConfiguration> _configurationMock;
    private Mock<ILogger<SqlSemanticSearchRepository>> _loggerMock;
    private IConfiguration _configuration;

    [TestInitialize]
    public void Setup()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<SqlSemanticSearchRepository>>();
        
        // Mock connection string
        _configurationMock.Setup(c => c.GetConnectionString("productsDb"))
            .Returns("Server=localhost;Database=test;Integrated Security=true;TrustServerCertificate=true;");
        
        // Mock default top value
        _configurationMock.Setup(c => c["SEMANTIC_SEARCH_TOP_DEFAULT"])
            .Returns("10");

        _configuration = _configurationMock.Object;
    }

    [TestMethod]
    public void Constructor_WithValidConfiguration_ShouldCreateInstance()
    {
        // Act
        var repository = new SqlSemanticSearchRepository(_configuration, _loggerMock.Object);

        // Assert
        Assert.IsNotNull(repository);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Constructor_WithMissingConnectionString_ShouldThrowException()
    {
        // Arrange
        _configurationMock.Setup(c => c.GetConnectionString("productsDb"))
            .Returns((string)null!);

        // Act
        new SqlSemanticSearchRepository(_configuration, _loggerMock.Object);
    }

    [TestMethod]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var repository = new SqlSemanticSearchRepository(_configuration, _loggerMock.Object);

        // Act
        var results = await repository.SearchAsync("", 10);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count());
    }

    [TestMethod]
    public async Task SearchAsync_WithWhitespaceQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var repository = new SqlSemanticSearchRepository(_configuration, _loggerMock.Object);

        // Act
        var results = await repository.SearchAsync("   ", 10);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count());
    }
}