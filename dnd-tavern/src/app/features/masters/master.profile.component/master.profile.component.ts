import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { MasterService } from '../../services/master.service';
import { UserStateService } from '../../auth/services/user-state.service';
import { Campaign } from '../../interfaces/campaign.interface';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-master-profile',
  templateUrl: './master.profile.component.html',
  styleUrls: ['./master.profile.component.scss'],
  imports: [CommonModule]
})
export class MasterProfileComponent implements OnInit {
  private userStateService = inject(UserStateService);
  private masterService = inject(MasterService);
  private cdr = inject(ChangeDetectorRef);


  masterId: string | null = null;
  allCampaigns: Campaign[] = [];
  selectedCampaigns: Set<string> = new Set();

  loading = false;
  error: string | null = null;
  success: boolean = false;

  ngOnInit(): void {
    // Берём текущего пользователя
    this.masterId = this.userStateService.getUserId();
    if (!this.masterId) {
      this.error = 'Не удалось определить текущего мастера';
      return;
    }

    this.loadData();
  }

  private loadData() {
    if (!this.masterId) return;
    
    this.loading = true;
    
    this.masterService.getAllCampaigns().subscribe({
      next: (campaigns) => {
        this.allCampaigns = campaigns;
        this.cdr.detectChanges();
        
        this.masterService.getMasterCampaigns(this.masterId!).subscribe({
          next: (masterCampaigns) => {
            this.selectedCampaigns = new Set(masterCampaigns.map(c => c.id));
            this.loading = false;
            this.cdr.detectChanges();
          },
          error: (err) => {
            this.error = 'Ошибка загрузки кампаний мастера';
            this.loading = false;
          }
        });
      },
      error: (err) => {
        this.error = 'Ошибка загрузки списка кампаний';
        this.loading = false;
      }
    });
  }


  toggleSelection(campaignId: string, event: any) {
    if (event.target.checked) {
      this.selectedCampaigns.add(campaignId);
    } else {
      this.selectedCampaigns.delete(campaignId);
    }
  }

  saveSelections() {
    if (!this.masterId) return;

    this.loading = true;
    this.error = null;
    this.success = false;

    const campaignIds = Array.from(this.selectedCampaigns);

    this.masterService.assignCampaigns(this.masterId, campaignIds).subscribe({
      next: () => {
        this.success = true;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Ошибка сохранения';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
