namespace L_Bank_W_Backend.DbAccess.Repositories
{
    public interface IBookingRepository
    {
        bool Book(int sourceId, int destinationId, decimal amount);
    }
}
