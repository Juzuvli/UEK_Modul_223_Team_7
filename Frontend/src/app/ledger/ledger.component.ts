import { Component, OnInit } from '@angular/core';
import { LedgerService } from '../../services/ledger.service';
import { Ledger } from '../../models/ledger.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

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
  message: string = '';

  // Neue Variablen für das Erstellen eines Ledgers
  newLedgerName: string = '';
  newLedgerBalance: number | null = null;

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
      this.amount > 0 &&
      this.fromLedgerId !== this.toLedgerId
    ) {
      this.ledgerService
        .transferFunds(this.fromLedgerId, this.toLedgerId, this.amount)
        .subscribe(
          () => {
            this.message = `${this.amount}$ have been successfully transferred.`;
            this.loadLedgers(); // Refresh ledger balances
          },
          (error) => {
            this.message = `An error occured while making the transfer.`;
            console.error('Transfer error', error);
          }
        );
    } else {
      this.message = 'Please fill in all fields with valid data.';
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
          this.message = this.message = `Ledger "${newLedger.name}" was successfully created!`;
          this.loadLedgers(); // Ledgers neu laden, um das neue Ledger anzuzeigen
        },
        (error) => {
          this.message = `Failed to create the new ledger named ${newLedger.name}. Error:  ${error.error.message}`;
          console.error('Create ledger error', error);
        }
      );
    } else {
      this.message = 'Please provide valid data for the new ledger.';
    }
  }

  deleteLedger(id:number): void {
    this.ledgerService.deleteLedger(id).subscribe(
      () => {
        this.loadLedgers(); // Ledgers neu laden, um das neue Ledger anzuzeigen
      },
      (error) => {
        this.message = `Failed to delete the ledger with the id ${id}. Error: ${error.error.message}`;
        console.error('Delete ledger error', error);
      }
    );
  }
}
