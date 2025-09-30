import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { BookingService } from '../../services/booking.service';
import { Booking } from '../../interfaces/booking.interface';
import { ConfirmationModal } from '../../services/confirmation-modal';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, NgbModule],
  templateUrl: './my-bookings.component.html',
  styleUrl: './my-bookings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyBookingsComponent implements OnInit {
  bookings: Booking[] = [];
  isLoading = false;
  error: string | null = null;
 
  private bookingService = inject(BookingService);
  private modalService = inject(NgbModal);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.loadBookings();
  }

  loadBookings() {
    this.isLoading = true;
    this.error = null;
   
    this.bookingService.getUserBookings().subscribe({
      next: (bookings) => {
        this.bookings = bookings.sort((a, b) => {
          const dateA = new Date(a.slot.startTime).getTime();
          const dateB = new Date(b.slot.startTime).getTime();
          return dateB - dateA;
        });
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading bookings:', error);
        this.error = 'Не удалось загрузить бронирования';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onCancelBooking(bookingId: string) {
    const booking = this.bookings.find(b => b.id === bookingId);
    if (!booking) return;

    const modalRef = this.modalService.open(ConfirmationModal);
    modalRef.componentInstance.title = 'Отменить бронирование';
    modalRef.componentInstance.message = `Вы уверены, что хотите отменить бронирование на ${this.formatDate(booking.slot.startTime)}?`;

    modalRef.result.then((result) => {
      if (result === 'ok') {
        this.bookingService.cancelBooking(bookingId).subscribe({
          next: () => {
            this.bookings = this.bookings.filter(b => b.id !== bookingId);
            this.cdr.markForCheck();
          },
          error: (error) => {
            console.error('Error canceling booking:', error);
            this.error = 'Не удалось отменить бронирование';
            this.cdr.markForCheck();
          }
        });
      }
    }).catch(() => {});
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  isUpcoming(booking: Booking): boolean {
    return !booking.slot.isInPast;
  }
}