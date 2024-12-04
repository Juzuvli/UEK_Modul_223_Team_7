using L_Bank_W_Backend.Controllers;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class BookingsControllerTests
{
    private readonly Mock<IBookingRepository> _mockBookingRepository;
    private readonly BookingsController _controller;

    public BookingsControllerTests()
    {
        _mockBookingRepository = new Mock<IBookingRepository>();
        _controller = new BookingsController(_mockBookingRepository.Object);
    }

    [Fact]
    public async Task Post_ShouldReturnBadRequest_WhenBookingIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("SourceId", "Required");
        var booking = new Booking();

        // Act
        var result = await _controller.Post(booking);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Equal("Invalid booking data.", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_ShouldReturnOk_WhenBookingIsSuccessful()
    {
        // Arrange
        var booking = new Booking { SourceId = 1, DestinationId = 2, Amount = 100 };
        _mockBookingRepository.Setup(repo => repo.Book(booking.SourceId, booking.DestinationId, booking.Amount))
                              .Returns(true);

        // Act
        var result = await _controller.Post(booking);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Post_ShouldReturnConflict_WhenBookingFails()
    {
        // Arrange
        var booking = new Booking { SourceId = 1, DestinationId = 2, Amount = 100 };
        _mockBookingRepository.Setup(repo => repo.Book(booking.SourceId, booking.DestinationId, booking.Amount))
                              .Returns(false);

        // Act
        var result = await _controller.Post(booking);

        // Assert
        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public async Task Post_ShouldReturnConflict_WhenExceptionIsThrown()
    {
        // Arrange
        var booking = new Booking { SourceId = 1, DestinationId = 2, Amount = 100 };
        _mockBookingRepository.Setup(repo => repo.Book(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
                              .Throws(new Exception("Database error"));

        // Act
        var result = await _controller.Post(booking);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        var conflictResult = result as ConflictObjectResult;
        Assert.Contains("An error occurred:", conflictResult.Value.ToString());
    }
}
