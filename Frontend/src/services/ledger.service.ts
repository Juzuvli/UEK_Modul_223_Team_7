import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Ledger } from '../models/ledger.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class LedgerService {
  private apiUrl = 'http://localhost:5000/api/v1';

  constructor(private http: HttpClient, private authService: AuthService) {}

  getLedgers(): Observable<Ledger[]> {
    const token = this.authService.getToken();
    if (token) {
      return this.http.get<Ledger[]>(`${this.apiUrl}/ledgers`);
    }

    return new Observable<Ledger[]>();
  }

  createLedger(ledger: Ledger): Observable<Ledger> {
    return this.http.post<Ledger>(`${this.apiUrl}/ledgers`, ledger);
  }

  deleteLedger(id: number): Observable<Ledger> {
    return this.http.delete<Ledger>(`${this.apiUrl}/ledgers/${id}`);
  }

  transferFunds(
    fromLedgerId: number,
    toLedgerId: number,
    amount: number
  ): Observable<any> {
    const payload = {
      sourceId:fromLedgerId,
      destinationId:toLedgerId,
      amount,
    };
    return this.http.post(`${this.apiUrl}/bookings`, payload);
  }

}
