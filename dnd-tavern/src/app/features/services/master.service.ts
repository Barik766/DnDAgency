import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, tap, throwError } from 'rxjs';
import { Campaign } from '../interfaces/campaign.interface';
import { Master } from '../interfaces/master.interface';

@Injectable({
  providedIn: 'root'
})
export class MasterService {
  private apiUrl = '/api/masters'; 

  constructor(private http: HttpClient) {}

  // назначить кампании мастеру (массово)
  assignCampaigns(masterId: string, campaignIds: string[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/${masterId}/campaigns/assign`, { campaignIds });
  }


  // Получить все кампании для мастера
  getMasterCampaigns(masterId: string): Observable<Campaign[]> {
  return this.http.get<{ Success: boolean; Data: Campaign[]; Message: string }>(
    `/api/masters/${masterId}/campaigns`
  ).pipe(map(res => res.Data));
}


  // Добавить кампанию мастеру
  addCampaignToMaster(masterId: string, campaignId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${masterId}/campaigns`, { campaignId });
  }

  // Удалить кампанию мастера (на будущее, если нужно будет убрать галочку)
  removeCampaignFromMaster(masterId: string, campaignId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${masterId}/campaigns/${campaignId}`);
  }

  // Получить список всех доступных кампаний (для чекбоксов)
  getAllCampaigns(): Observable<Campaign[]> {
    return this.http.get<{ Success: boolean; Data: Campaign[]; Message: string }>('/api/campaigns')
      .pipe(
        map(res => res.Data)
      );
  }

  // Получить всех мастеров
  getAllMasters(): Observable<Master[]> {
    return this.http.get<{ Success: boolean; Data: Master[]; Message: string }>(`${this.apiUrl}`)
      .pipe(map(res => res.Data));
  }

  // Получить ограниченное количество мастеров
  getMasters(limit?: number): Observable<Master[]> {
    const url = limit ? `${this.apiUrl}?limit=${limit}` : this.apiUrl;
    return this.http.get<any>(url)
      .pipe(map(res => res.Data || res));
  }

}
