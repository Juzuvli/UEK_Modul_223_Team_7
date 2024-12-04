using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Data.SqlClient;
using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.Data; // For DatabaseSettings
using L_Bank_W_Backend.DbAccess.Repositories; // For BookingRepository, ILedgerRepository
using L_Bank_W_Backend.Core.Models; // For Ledger
using System.Collections.Generic; // Needed for List
using System.Linq; // Needed for Any


public class BookingRepositoryUnitTests
{
    private readonly Mock<ILedgerRepository> _mockLedgerRepository;
    private readonly Mock<ILogger<BookingRepository>> _mockLogger;
    private readonly BookingRepository _bookingRepository;

    public BookingRepositoryUnitTests()
    {
        _mockLedgerRepository = new Mock<ILedgerRepository>();
        _mockLogger = new Mock<ILogger<BookingRepository>>();

        var databaseSettings = new Mock<IOptions<DatabaseSettings>>();
        databaseSettings.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "Server=localhost;Database=TestDb;User Id=sa;Password=YourPassword123;"
        });

        _bookingRepository = new BookingRepository(databaseSettings.Object, _mockLedgerRepository.Object, _mockLogger.Object);
    }

    // Helper method to capture log messages
    private List<string> GetLoggedMessages(Mock<ILogger<BookingRepository>> loggerMock)
    {
        var logMessages = new List<string>();

        loggerMock.Setup(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>())
            )
            .Callback((LogLevel level, EventId eventId, object state, Exception exception, Delegate formatter) =>
            {
                logMessages.Add(state.ToString()!);
            });

        return logMessages;
    }

    [Fact]
    public void Book_ShouldTransferFunds_WhenFundsAreSufficient()
    {
        // Arrange
        var sourceLedger = new Ledger { Id = 1, Balance = 1000m };
        var destinationLedger = new Ledger { Id = 2, Balance = 500m };
        decimal amount = 200m;

        _mockLedgerRepository.Setup(repo => repo.SelectOne(1, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(sourceLedger);

        _mockLedgerRepository.Setup(repo => repo.SelectOne(2, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(destinationLedger);

        var logMessages = GetLoggedMessages(_mockLogger);

        // Act
        bool result = _bookingRepository.Book(1, 2, amount);

        // Assert
        Assert.True(result);
        Assert.Equal(800m, sourceLedger.Balance);
        Assert.Equal(700m, destinationLedger.Balance);
        Assert.Contains(logMessages, msg => msg.Contains("Booking successful"));
    }
    [Fact]
    public void Book_ShouldThrowException_WhenFundsAreInsufficient()
    {
        // Arrange
        var sourceLedger = new Ledger { Id = 1, Balance = 100m };
        var destinationLedger = new Ledger { Id = 2, Balance = 500m };
        decimal amount = 200m;

        _mockLedgerRepository.Setup(repo => repo.SelectOne(1, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(sourceLedger);

        _mockLedgerRepository.Setup(repo => repo.SelectOne(2, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(destinationLedger);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _bookingRepository.Book(1, 2, amount));
        Assert.Equal("Booking failed: Insufficient funds.", exception.Message); // Match updated message
    }

    [Fact]
    public void Book_ShouldThrowException_WhenSourceLedgerNotFound()
    {
        // Arrange
        var sourceLedger = (Ledger)null;
        var destinationLedger = new Ledger { Id = 2, Balance = 500m };
        decimal amount = 200m;

        _mockLedgerRepository.Setup(repo => repo.SelectOne(1, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(sourceLedger);

        _mockLedgerRepository.Setup(repo => repo.SelectOne(2, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(destinationLedger);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _bookingRepository.Book(1, 2, amount));
        Assert.Equal("Booking failed: Source ledger not found.", exception.Message); // Match the full error message
    }

    [Fact]
    public void Book_ShouldThrowException_WhenDestinationLedgerNotFound()
    {
        // Arrange
        var sourceLedger = new Ledger { Id = 1, Balance = 1000m };
        var destinationLedger = (Ledger)null;
        decimal amount = 200m;

        _mockLedgerRepository.Setup(repo => repo.SelectOne(1, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(sourceLedger);

        _mockLedgerRepository.Setup(repo => repo.SelectOne(2, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(destinationLedger);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _bookingRepository.Book(1, 2, amount));
        Assert.Equal("Booking failed: Destination ledger not found.", exception.Message); // Corrected message to match full error message format
    }

    public class DeadlockException : Exception
    {
        public int ErrorCode { get; }

        public DeadlockException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    [Fact]
    public void Book_ShouldRetry_WhenDeadlockOccurs()
    {
        // Arrange
        var sourceLedger = new Ledger { Id = 1, Balance = 1000m };
        var destinationLedger = new Ledger { Id = 2, Balance = 500m };
        decimal amount = 200m;

        _mockLedgerRepository.Setup(repo => repo.SelectOne(1, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(sourceLedger);

        _mockLedgerRepository.Setup(repo => repo.SelectOne(2, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(destinationLedger);

        // Simulate a deadlock exception with custom exception class
        _mockLedgerRepository.Setup(repo => repo.SelectOne(It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Throws(new DeadlockException("Deadlock occurred.", 1205)); // Simulate deadlock error code 1205

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _bookingRepository.Book(1, 2, amount));
        Assert.Contains("Booking failed: Deadlock occurred.", exception.Message); // Check the deadlock log message
    }

    [Fact]
    public void Book_ShouldThrowException_WhenGeneralErrorOccurs()
    {
        // Arrange
        var sourceLedger = new Ledger { Id = 1, Balance = 1000m };
        var destinationLedger = new Ledger { Id = 2, Balance = 500m };
        decimal amount = 200m;

        _mockLedgerRepository.Setup(repo => repo.SelectOne(1, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(sourceLedger);

        _mockLedgerRepository.Setup(repo => repo.SelectOne(2, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Returns(destinationLedger);

        // Simulate a general error
        _mockLedgerRepository.Setup(repo => repo.SelectOne(It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
            .Throws(new Exception("Some general error"));

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _bookingRepository.Book(1, 2, amount));
        Assert.Equal("Booking failed: Some general error", exception.Message);
    }

}
