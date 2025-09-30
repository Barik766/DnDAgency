import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Campaign } from '../interfaces/campaign.interface';
import { UpcomingGame } from '../interfaces/upcoming-game.interface';

// Тип ответа от API
interface ApiResponse<T> {
  Success: boolean;
  Data: T;
  Message: string;
}



@Injectable({
  providedIn: 'root'
})
export class CampaignService {
  private readonly apiUrl = 'http://localhost:5195/api/Campaigns';

  constructor(private http: HttpClient) {}

  getAllCampaigns(): Observable<Campaign[]> {
    return this.http
      .get<ApiResponse<Campaign[]>>(this.apiUrl)
      .pipe(map(response => response.Data));
  }

  getCampaignById(id: string): Observable<Campaign> {
    return this.http
      .get<ApiResponse<Campaign>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.Data));
  }
 
  getCampaignCatalog(): Observable<Campaign[]> {
    return this.http
      .get<ApiResponse<Campaign[]>>(`${this.apiUrl}/catalog`)
      .pipe(map(response => response.Data));
  }

  getCampaignDetails(id: string): Observable<Campaign> {
    return this.http
      .get<ApiResponse<Campaign>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.Data));
  }

  getUpcomingGames(): Observable<UpcomingGame[]> {
    return this.http
      .get<ApiResponse<UpcomingGame[]>>(`${this.apiUrl}/upcoming-games`)
      .pipe(map(response => response.Data));
  }

  // НОВЫЙ МЕТОД: Получение доступных временных слотов для кампании на дату
  getAvailableTimeSlots(campaignId: string, date: string, roomType: 'Online' | 'Physical'): Observable<AvailableTimeSlot[]> {
    return this.http
      .get<any>(`${this.apiUrl}/${campaignId}/available-slots?date=${date}&roomType=${roomType}`)
      .pipe(map(response => response.Data.data)); // было response.data
  }

  createCampaign(formData: FormData): Observable<Campaign> {
    return this.http.post<Campaign>(this.apiUrl, formData);
  }

  updateCampaign(id: string, formData: FormData): Observable<Campaign> {
    return this.http.put<Campaign>(`${this.apiUrl}/${id}`, formData);
  }

  deleteCampaign(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  toggleCampaignStatus(id: string): Observable<Campaign> {
    return this.http.patch<Campaign>(`${this.apiUrl}/${id}/toggle-status`, null);
  }

  // УДАЛЕНЫ методы для управления слотами:
  // getCampaignSlots() - больше не нужен
  // addSlotToCampaign() - слоты создаются автоматически при бронировании
  // removeSlotFromCampaign() - слоты создаются автоматически при бронировании
}