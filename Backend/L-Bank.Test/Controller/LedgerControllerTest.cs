using Moq;
using Xunit;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.Controllers;
using L_Bank_W_Backend.Core.Models;
using Microsoft.AspNetCore.Mvc;

public class LedgerControllerTests
{
    private readonly Mock<ILedgerRepository> _mockLedgerRepository;
    private readonly LedgersController _controller;

    public LedgerControllerTests()
    {
        _mockLedgerRepository = new Mock<ILedgerRepository>();
        _controller = new LedgersController(_mockLedgerRepository.Object);
    }

    [Fact]
    public async Task Post_ShouldReturnBadRequest_WhenLedgerIsNull()
    {
        // Act
        var result = await _controller.Post(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Post_ShouldReturnCreatedResult_WhenLedgerIsValid()
    {
        // Arrange
        var ledger = new Ledger { Id = 0, Name = "Test Ledger", Balance = 1000.00M };
        _mockLedgerRepository.Setup(repo => repo.AddLedger(It.IsAny<Ledger>()))
            .ReturnsAsync(ledger.Id);

        // Act
        var result = await _controller.Post(ledger) as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        
        var returnedLedger = result.Value as Ledger;
        Assert.NotNull(returnedLedger);
        Assert.Equal(ledger.Id, returnedLedger.Id);
    }

    [Fact]
    public async Task Post_ShouldReturnServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var ledger = new Ledger { Id = 0, Name = "Test Ledger", Balance = 1000.00M };
        _mockLedgerRepository.Setup(repo => repo.AddLedger(It.IsAny<Ledger>()))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act
        var result = await _controller.Post(ledger) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("An error occurred while creating the ledger.", result.Value);
    }
}