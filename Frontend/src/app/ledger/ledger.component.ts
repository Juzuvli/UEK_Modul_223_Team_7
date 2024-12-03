import { Component, OnInit } from '@angular/core';
import { LedgerService } from '../../services/ledger.service';
import { Ledger } from '../../models/ledger.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-ledger',
  templateUrl: './ledger.component.html',
  styleUrls: ['./ledger.component.css'],
  imports: [CommonModule, FormsModule],
  standalone: true,
  providers:  [ LedgerService, HttpClient ]
})
export class LedgerComponent implements OnInit {
  ledgers: Ledger[] = [];
  fromLedgerId: number | null = null;
  toLedgerId: number | null = null;
  amount: number | null = null;
  transferMessage: string = '';

  // Neue Variablen für das Erstellen eines Ledgers
  newLedgerName: string = '';
  newLedgerBalance: number | null = null;
  createMessage: string = '';

  constructor(private ledgerService: LedgerService) {}

  ngOnInit(): void {
    this.loadLedgers();
  }

  loadLedgers(): void {
    this.ledgerService.getLedgers().subscribe(
      (data) => (this.ledgers = data),
      (error) => console.error('Error fetching ledgers', error)
    );
  }

  makeTransfer(): void {
    if (
      this.fromLedgerId !== null &&
      this.toLedgerId !== null &&
      this.amount !== null &&
      this.amount > 0
    ) {
      this.ledgerService
        .transferFunds(this.fromLedgerId, this.toLedgerId, this.amount)
        .subscribe(
          () => {
            this.transferMessage = 'Transfer successful!';
            this.loadLedgers(); // Refresh ledger balances
          },
          (error) => {
            this.transferMessage = `Transfer failed: ${error.error.message}`;
            console.error('Transfer error', error);
          }
        );
    } else {
      this.transferMessage = 'Please fill in all fields with valid data.';
    }
  }

  createLedger(): void {
    if (this.newLedgerName && this.newLedgerBalance !== null) {
      const newLedger: Ledger = {
        id: 0, // ID wird vom Backend generiert, daher hier 0
        name: this.newLedgerName,
        balance: this.newLedgerBalance,
      };

      this.ledgerService.createLedger(newLedger).subscribe(
        () => {
          this.createMessage = 'Ledger created successfully!';
          this.loadLedgers(); // Ledgers neu laden, um das neue Ledger anzuzeigen
        },
        (error) => {
          this.createMessage = `Failed to create ledger: ${error.error.message}`;
          console.error('Create ledger error', error);
        }
      );
    } else {
      this.createMessage = 'Please provide valid data for the new ledger.';
    }
  }
}
