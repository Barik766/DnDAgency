import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Booking } from '../interfaces/booking.interface';

interface CreateBookingRequest {
  campaignId: string;
  startTime: string;
  playersCount: number;
}

interface ApiResponse<T> {
  Success: boolean;
  Data: T;
  Message: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly apiUrl = '/api/Bookings';

  constructor(private http: HttpClient) {}

  createBooking(bookingData: CreateBookingRequest): Observable<Booking> {
    return this.http
      .post<ApiResponse<Booking>>(this.apiUrl, bookingData)
      .pipe(map(response => response.Data));
  }

  getUserBookings(): Observable<Booking[]> {
    return this.http
      .get<ApiResponse<Booking[]>>(`${this.apiUrl}/my`)
      .pipe(map(response => response.Data));
  }

  getBookingById(bookingId: string): Observable<Booking> {
    return this.http
      .get<ApiResponse<Booking>>(`${this.apiUrl}/${bookingId}`)
      .pipe(map(response => response.Data));
  }

  cancelBooking(bookingId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${bookingId}`);
  }

  getAvailableSlots(campaignId: string): Observable<any[]> {
    return this.http
      .get<ApiResponse<any[]>>(`${this.apiUrl}/slots/${campaignId}/available`)
      .pipe(map(response => response.Data));
  }
}