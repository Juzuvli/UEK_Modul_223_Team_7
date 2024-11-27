namespace L_Bank_W_Backend.Models;

public interface IBookingRepository
{
    bool Book(int sourceLedgerId, int destinationLedgerId, decimal amount);
}