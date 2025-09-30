import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CampaignService } from '../../../services/campaign.service';
import { BookingService } from '../../../services/booking.service';

interface CampaignDetails {
  id: string;
  title: string;
  description: string;
  price: number;
  imageUrl?: string;
  level: number;
  maxPlayers: number;
  durationHours?: number;
  tags: string[];
  isActive: boolean;
  masterName?: string;
}

@Component({
  selector: 'app-game-card-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './game-card-details.html',
  styleUrls: ['./game-card-details.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GameCardDetails implements OnInit {
  campaign!: CampaignDetails | null;
  availableTimeSlots: AvailableTimeSlot[] = [];
  isLoading = true;
  isLoadingTimeSlots = false;
  error: string | null = null;

  selectedGameType: 'online' | 'offline' | null = null;
  selectedDate: string = '';
  selectedDateTime: string | null = null;
  reservedPlayers = 1;

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private campaignService = inject(CampaignService);
  private bookingService = inject(BookingService);
  private cdr = inject(ChangeDetectorRef);

  bookingForm = this.fb.group({
    gameType: [''],
    date: [''],
    dateTime: [''],
    players: [1]
  });

  ngOnInit() {
    this.availableTimeSlots = [];
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = 'ID кампании не указан';
      this.isLoading = false;
      return;
    }

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.selectedDate = tomorrow.toISOString().split('T')[0];

    this.loadCampaign(id);
  }

  private mapCampaignDetails(data: any): CampaignDetails {
    return {
      ...data,
      imageUrl: data.imageUrl
        ? `http://localhost:5195/${data.imageUrl}`
        : '/img/default-game.jpeg'
    };
  }

  loadCampaign(id: string) {
    console.log('Starting loadCampaign for id:', id);
    this.isLoading = true;
    this.campaignService.getCampaignDetails(id).subscribe({
      next: (data) => {
        console.log('Received data:', data); // Добавить
        this.campaign = this.mapCampaignDetails(data);
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.error = 'Не удалось загрузить кампанию';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onGameTypeChange() {
    this.selectedDate = '';
    this.selectedDateTime = null;
    this.availableTimeSlots = [];
  }

  onDateChange(event: Event) {
    this.selectedDate = (event.target as HTMLInputElement).value;
    this.selectedDateTime = null;
    this.availableTimeSlots = [];
    if (this.selectedDate && this.selectedGameType && this.campaign) {
      this.loadAvailableTimeSlots();
    }
  }

  loadAvailableTimeSlots() {
    if (!this.campaign || !this.selectedDate || !this.selectedGameType) return;

    this.isLoadingTimeSlots = true;
    
    const roomType = this.selectedGameType === 'online' ? 'Online' : 'Physical';
    
    this.campaignService.getAvailableTimeSlots(this.campaign.id, this.selectedDate, roomType).subscribe({
      next: (timeSlots) => {
        console.log('Received timeSlots:', timeSlots); // добавь эту строку
        console.log(roomType)
        this.availableTimeSlots = timeSlots || []; // добавь || []
        this.isLoadingTimeSlots = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.availableTimeSlots = []; // добавь эту строку
        this.error = 'Не удалось загрузить доступные времена';
        this.isLoadingTimeSlots = false;
        this.cdr.markForCheck();
      }
    });
  }

  onTimeSlotChange(event: Event) {
    this.selectedDateTime = (event.target as HTMLSelectElement).value;
  }

  getMaxPlayersForTimeSlot(): number {
    if (!this.selectedDateTime) return 1;
    const timeSlot = this.availableTimeSlots.find(ts => ts.dateTime === this.selectedDateTime);
    return timeSlot ? (timeSlot.maxPlayers - timeSlot.currentPlayers) : 1;
  }

  onReserve() {
    if (!this.selectedDateTime || !this.campaign) return;

    const bookingData = {
      campaignId: this.campaign.id,
      startTime: this.selectedDateTime,
      playersCount: this.reservedPlayers
    };

    this.bookingService.createBooking(bookingData).subscribe({
      next: (booking) => {
        console.log('Booking created:', booking);
        this.router.navigate(['/my-bookings']);
      },
      error: (err) => {
        console.error(err);
        this.error = err.error?.message || 'Не удалось создать бронирование';
        this.cdr.markForCheck();
      }
    });
  }

  getTomorrowDate(): string {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  }
}
