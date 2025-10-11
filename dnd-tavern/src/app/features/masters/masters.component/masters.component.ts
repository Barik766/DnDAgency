import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Master } from '../../interfaces/master.interface';
import { MasterService } from '../../services/master.service';
import { Router } from '@angular/router';
import { MasterCard } from '../../interfaces/masterCard.interface';

@Component({
  selector: 'app-masters',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './masters.component.html',
  styleUrl: './masters.component.scss'
})
export class MastersComponent implements OnInit {
  masters: MasterCard[] = []; // Измени тип
  isLoading = false;
  error: string | null = null;
  
  private masterService = inject(MasterService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);

  ngOnInit() {
    this.loadMasters();
  }

  loadMasters() {
    this.isLoading = true;
    this.error = null;
   
    this.masterService.getAllMasters().subscribe({
      next: (masters) => {
        this.masters = masters.map(m => this.mapMasterToCard(m)); // Маппим всех
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading masters:', error);
        this.error = 'Failed to load masters';
        this.isLoading = false;
        this.cdr.detectChanges();
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
      photoUrl: `/api/masters/${master.id}.png`, 
      createdAt: master.createdAt,
      updatedAt: master.updatedAt
    };
}

  onMasterClick(master: MasterCard): void {
    this.router.navigate(['/masters', master.id]);
  }
}