using System;
using System.Data.SqlClient;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DatabaseSettings databaseSettings;
        private readonly ILedgerRepository ledgerRepository; // Hinzufügen der LedgerRepository-Referenz
        private readonly ILogger<BookingRepository> logger;

        public BookingRepository(IOptions<DatabaseSettings> databaseSettings, ILedgerRepository ledgerRepository, ILogger<BookingRepository> logger)
        {
            this.databaseSettings = databaseSettings.Value;
            this.ledgerRepository = ledgerRepository; // LedgerRepository instanziieren
            this.logger = logger;
        }

        public bool Book(int sourceLedgerId, int destinationLedgerId, decimal amount)
        {
            bool success = false;

            using (SqlConnection conn = new SqlConnection(this.databaseSettings.ConnectionString))
            {
                conn.Open();
                bool retry;

                do
                {
                    retry = false;

                    using (var transaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        try
                        {
                            // Get Source Ledger
                            var sourceLedger = ledgerRepository.SelectOne(sourceLedgerId, conn, transaction);
                            if (sourceLedger == null)
                            {
                                // Fehlerbehandlung, wenn das Quellkonto nicht gefunden wurde
                                logger.LogError($"Source ledger not found for ID: {sourceLedgerId}");
                                throw new Exception("Source ledger not found.");
                            }

                            // Überprüfen, ob das Guthaben ausreichend ist
                            if (sourceLedger.Balance < amount)
                            {
                                // Fehlerbehandlung bei unzureichendem Guthaben
                                logger.LogWarning($"Insufficient funds in source ledger {sourceLedgerId}. Requested amount: {amount}, Available balance: {sourceLedger.Balance}");
                                throw new Exception("Insufficient funds.");
                            }

                            // Get Destination Ledger
                            var destinationLedger = ledgerRepository.SelectOne(destinationLedgerId, conn, transaction);
                            if (destinationLedger == null)
                            {
                                // Fehlerbehandlung, wenn das Zielkonto nicht gefunden wurde
                                logger.LogError($"Destination ledger not found for ID: {destinationLedgerId}");
                                throw new Exception("Destination ledger not found.");
                            }

                            // Update der Salden
                            sourceLedger.Balance -= amount;
                            destinationLedger.Balance += amount;

                            // Aktualisieren der Konten in der Datenbank
                            ledgerRepository.Update(sourceLedger, conn, transaction);
                            ledgerRepository.Update(destinationLedger, conn, transaction);

                            // Commit der Transaktion
                            transaction.Commit();
                            logger.LogInformation($"Booking successful. Transferred {amount} from Ledger {sourceLedgerId} to Ledger {destinationLedgerId}");
                            success = true;
                        }
                        catch (SqlException ex) when (ex.Number == 1205) // Deadlock exception handling
                        {
                            // Fehlerbehandlung bei Deadlocks
                            retry = true;
                            transaction.Rollback();
                            logger.LogWarning("Deadlock detected. Retrying transaction.");
                        }
                        catch (Exception ex)
                        {
                            // Allgemeine Fehlerbehandlung
                            transaction.Rollback();
                            logger.LogError($"Booking transaction failed: {ex.Message}");

                            // Fehlernachricht mit spezifischer Fehlermeldung
                            throw new Exception($"Booking failed: {ex.Message}");
                        }
                    }
                } while (retry); // Wiederhole die Transaktion im Falle eines Deadlocks
            }

            return success;
        }
    }
}
