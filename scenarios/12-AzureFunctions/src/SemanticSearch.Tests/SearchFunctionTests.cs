using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SemanticSearchFunction.Functions;

namespace SemanticSearch.Tests;

[TestClass]
public class SearchFunctionTests
{
    private Mock<ILogger<SearchFunction>> _loggerMock;
    private SearchFunction _function;
    private Mock<HttpRequestData> _requestMock;
    private Mock<FunctionContext> _contextMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<SearchFunction>>();
        _function = new SearchFunction(_loggerMock.Object, _repositoryMock.Object);
        
        _contextMock = new Mock<FunctionContext>();
        _requestMock = new Mock<HttpRequestData>(_contextMock.Object);
    }

    [TestMethod]
    public async Task Run_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var searchRequest = new SearchRequest { Query = "test", Top = 5 };
        var json = JsonSerializer.Serialize(searchRequest);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        
        _requestMock.Setup(r => r.Body).Returns(stream);
        
        var expectedResults = new List<SearchResult>
        {
            new SearchResult
            {
                Id = 1,
                Title = "Test Product",
                Score = 0.9,
                Snippet = "Test description",
                Metadata = new Dictionary<string, string> { ["price"] = "$10.00" }
            }
        };

        _repositoryMock.Setup(r => r.SearchAsync("test", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var responseMock = new Mock<HttpResponseData>(_contextMock.Object);
        _requestMock.Setup(r => r.CreateResponse(System.Net.HttpStatusCode.OK))
            .Returns(responseMock.Object);

        // Act
        var result = await _function.Run(_requestMock.Object);

        // Assert
        Assert.IsNotNull(result);
        _repositoryMock.Verify(r => r.SearchAsync("test", 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Run_WithEmptyBody_ShouldReturnBadRequest()
    {
        // Arrange
        var stream = new MemoryStream();
        _requestMock.Setup(r => r.Body).Returns(stream);

        var responseMock = new Mock<HttpResponseData>(_contextMock.Object);
        _requestMock.Setup(r => r.CreateResponse(System.Net.HttpStatusCode.BadRequest))
            .Returns(responseMock.Object);

        // Act
        var result = await _function.Run(_requestMock.Object);

        // Assert
        Assert.IsNotNull(result);
        _repositoryMock.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        _requestMock.Setup(r => r.Body).Returns(stream);

        var responseMock = new Mock<HttpResponseData>(_contextMock.Object);
        _requestMock.Setup(r => r.CreateResponse(System.Net.HttpStatusCode.BadRequest))
            .Returns(responseMock.Object);

        // Act
        var result = await _function.Run(_requestMock.Object);

        // Assert
        Assert.IsNotNull(result);
        _repositoryMock.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}