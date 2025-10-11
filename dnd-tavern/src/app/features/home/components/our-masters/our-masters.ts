import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Master } from '../../../interfaces/master.interface';
import { MasterService } from '../../../services/master.service';
import { MasterCard } from '../../../interfaces/masterCard.interface';



@Component({
  selector: 'app-our-masters',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './our-masters.html',
  styleUrl: './our-masters.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OurMastersComponent implements OnInit {
  masters: MasterCard[] = []; 
  isLoading = false;
  error: string | null = null;
 
  private masterService = inject(MasterService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.loadMasters();
  }

  loadMasters() {
    this.isLoading = true;
    this.error = null;
   
    this.masterService.getAllMasters().subscribe({
      next: (masters) => {
        this.masters = masters.slice(0, 3).map(m => this.mapMasterToCard(m)); // Маппим и берём первые 3
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading masters:', error);
        this.error = 'Failed to load masters';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  private mapMasterToCard(master: Master): MasterCard {
    return {
      id: master.id,
      userId: master.userId,
      name: master.name,
      bio: master.bio,
      isActive: master.isActive,
      campaignCount: master.campaignCount,
      averageRating: master.averageRating,
      reviewCount: master.reviewCount,
      photoUrl: master.photoUrl ? `${master.photoUrl}` : '/img/default-master.jpg',
      createdAt: master.createdAt,
      updatedAt: master.updatedAt
    };
  }

  onMasterClick(master: MasterCard): void {
    console.log('Selected master:', master);
  }

  onViewAllMasters(): void {
    console.log('Show all masters');
  }
}