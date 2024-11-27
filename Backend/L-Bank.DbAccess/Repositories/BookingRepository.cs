using L_Bank_W_Backend.Models;
using Microsoft.Extensions.Options;

namespace L_Bank_W_Backend.DbAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        // private readonly DatabaseSettings databaseSettings;
        // public BookingRepository(IOptions<DatabaseSettings> databaseSettings)
        // {
        //     this.databaseSettings = databaseSettings.Value;
        // }
        
        // public bool Book(int sourceLedgerId, int destinationLedgerId, decimal amount)
        // {
        //     
        //     // using (SqlConnection conn = new SqlConnection(this.databaseSettings.ConnectionString))
        //     // {
        //     //     conn.Open();
        //     //     using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.Serializable))
        //     //     {
        //     //         try
        //     //         {
        //     //             amount = 10;
        //     //         }
        //     //     }
        //     // }
        //     // // Machen Sie eine Connection und eine Transaktion
        //     // //
        //     // // In der Transaktion:
        //     // //
        //     // // Schauen Sie ob genügend Geld beim Spender da ist
        //     // // Führen Sie die Buchung durch und UPDATEn Sie die ledgers
        //     // // Beenden Sie die Transaktion
        //     // // Bei einem Transaktionsproblem: Restarten Sie die Transaktion in einer Schleife 
        //     // // (Siehe LedgersModel.SelectOne)
        //     //
        //     // return false;
        // }
        
        // public string Book(decimal amount, Ledger from, Ledger to)
        // {
        //     // try
        //     // {
        //     //     amount = 10;
        //     //     from.Balance = this.GetBalance(from.Id, conn, transaction) ?? throw new ArgumentNullException();
        //     //     from.Balance -= amount;
        //     //     this.Update(from, conn, transaction);
        //     //     // Complicate calculations
        //     //     Thread.Sleep(250);
        //     //     to.Balance = this.GetBalance(to.Id, conn, transaction) ?? throw new ArgumentNullException();
        //     //     to.Balance += amount;
        //     //     this.Update(to, conn, transaction);
        //     //
        //     //     // Console.WriteLine($"Booking {amount} from {from.Name} to {to.Name}");
        //     //
        //     //     transaction.Commit();
        //     //     return ".";
        //     // }
        //     // catch (Exception ex)
        //     // {
        //     //     //Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
        //     //     //Console.WriteLine("  Message: {0}", ex.Message);
        //     //
        //     //     // Attempt to roll back the transaction.
        //     //     try
        //     //     {
        //     //         transaction.Rollback();
        //     //         return "R";
        //     //     }
        //     //     catch (Exception ex2)
        //     //     {
        //     //         // Handle any errors that may have occurred on the server that would cause the rollback to fail.
        //     //         //Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
        //     //         //Console.WriteLine("  Message: {0}", ex2.Message);
        //     //         return "E";
        //     //     }
        //     }
    }
}

