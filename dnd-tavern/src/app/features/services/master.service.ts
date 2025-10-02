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

  // Assign campaigns to a master (bulk)
  assignCampaigns(masterId: string, campaignIds: string[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/${masterId}/campaigns/assign`, { campaignIds });
  }


  // Get all campaigns for a master
  getMasterCampaigns(masterId: string): Observable<Campaign[]> {
  return this.http.get<{ Success: boolean; Data: Campaign[]; Message: string }>(
    `/api/masters/${masterId}/campaigns`
  ).pipe(map(res => res.Data));
}


  // Add a campaign to a master
  addCampaignToMaster(masterId: string, campaignId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${masterId}/campaigns`, { campaignId });
  }

  // Remove a campaign from a master (future use - e.g. uncheck)
  removeCampaignFromMaster(masterId: string, campaignId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${masterId}/campaigns/${campaignId}`);
  }

  // Get list of all available campaigns (for checkboxes)
  getAllCampaigns(): Observable<Campaign[]> {
    return this.http.get<{ Success: boolean; Data: Campaign[]; Message: string }>('/api/campaigns')
      .pipe(
        map(res => res.Data)
      );
  }

  // Get all masters
  getAllMasters(): Observable<Master[]> {
    return this.http.get<{ Success: boolean; Data: Master[]; Message: string }>(`${this.apiUrl}`)
      .pipe(map(res => res.Data));
  }

  // Get a limited number of masters
  getMasters(limit?: number): Observable<Master[]> {
    const url = limit ? `${this.apiUrl}?limit=${limit}` : this.apiUrl;
    return this.http.get<any>(url)
      .pipe(map(res => res.Data || res));
  }

}
