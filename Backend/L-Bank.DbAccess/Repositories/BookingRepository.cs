using L_Bank_W_Backend.Models;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Data;

namespace L_Bank_W_Backend.DbAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DatabaseSettings databaseSettings;

        public BookingRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            this.databaseSettings = databaseSettings.Value;
        }

        public bool Book(int sourceLedgerId, int destinationLedgerId, decimal amount)
        {
            using (SqlConnection conn = new SqlConnection(this.databaseSettings.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        // Abbuchen vom Quell-Ledger
                        string debitQuery =
                            "UPDATE Ledgers SET Balance = Balance - @Amount WHERE Id = @SourceLedgerId AND Balance >= @Amount";
                        using (SqlCommand debitCommand = new SqlCommand(debitQuery, conn, transaction))
                        {
                            debitCommand.Parameters.AddWithValue("@Amount", amount);
                            debitCommand.Parameters.AddWithValue("@SourceLedgerId", sourceLedgerId);

                            int rowsAffected = debitCommand.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                // Kein ausreichender Saldo oder Ledger nicht gefunden
                                throw new InvalidOperationException(
                                    "Debit operation failed. Insufficient funds or source ledger not found.");
                            }
                        }

                        // Gutschrift auf das Ziel-Ledger
                        string creditQuery =
                            "UPDATE Ledgers SET Balance = Balance + @Amount WHERE Id = @DestinationLedgerId";
                        using (SqlCommand creditCommand = new SqlCommand(creditQuery, conn, transaction))
                        {
                            creditCommand.Parameters.AddWithValue("@Amount", amount);
                            creditCommand.Parameters.AddWithValue("@DestinationLedgerId", destinationLedgerId);

                            int rowsAffected = creditCommand.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                // Ziel-Ledger nicht gefunden
                                throw new InvalidOperationException(
                                    "Credit operation failed. Destination ledger not found.");
                            }
                        }

                        // Transaktion abschließen
                        transaction.Commit();
                        return true; // Erfolgreich
                    }
                    catch
                    {
                        // Rollback der Transaktion bei Fehlern
                        transaction.Rollback();
                        return false; // Fehlerhaft
                    }
                }
            }
        }
    }
}


