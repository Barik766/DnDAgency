import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Campaign } from '../interfaces/campaign.interface';
import { UpcomingGame } from '../interfaces/upcoming-game.interface';
import {PagedResult} from '../interfaces/paged-result.interface';

// API response type
interface ApiResponse<T> {
  Success: boolean;
  Data: T;
  Message: string;
}


@Injectable({
  providedIn: 'root'
})
export class CampaignService {
  private readonly apiUrl = '/api/Campaigns';

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

  getCampaignCatalogPaged(
    pageNumber: number = 1,
    pageSize: number = 12,
    search?: string,
    tag?: string,
    hasSlots?: boolean,
    sortBy: string = 'title'
  ): Observable<PagedResult<Campaign>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString())
      .set('sortBy', sortBy);

    if (search) params = params.set('search', search);
    if (tag) params = params.set('tag', tag);
    if (hasSlots !== undefined && hasSlots !== null) {
      params = params.set('hasSlots', hasSlots.toString());
    }

    return this.http
      .get<ApiResponse<PagedResult<Campaign>>>(`${this.apiUrl}/catalog/paged`, { params })
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

  getAvailableTimeSlots(campaignId: string, date: string, roomType: 'Online' | 'Physical'): Observable<AvailableTimeSlot[]> {
    return this.http
      .get<any>(`${this.apiUrl}/${campaignId}/available-slots?date=${date}&roomType=${roomType}`)
      .pipe(map(response => response.Data.data));
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
}