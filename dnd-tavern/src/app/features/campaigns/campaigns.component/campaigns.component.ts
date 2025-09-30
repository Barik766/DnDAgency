import { Component, OnInit, OnDestroy, inject, computed, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { CampaignService } from '../../services/campaign.service';
import { GameCardCatalog } from './game-card-catalog/game-card-catalog';
import { UserStateService } from '../../auth/services/user-state.service';
import { CatalogGame } from '../../interfaces/catalogGame.interface';
import { Campaign } from '../../interfaces/campaign.interface';

@Component({
  selector: 'app-campaigns',
  standalone: true,
  imports: [CommonModule, GameCardCatalog],
  templateUrl: './campaigns.component.html',
  styleUrls: ['./campaigns.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CampaignsComponent implements OnInit, OnDestroy {
  campaigns: CatalogGame[] = [];
  isLoading = false;
  error: string | null = null;
  
  private destroy$ = new Subject<void>();
  private router = inject(Router);
  private campaignService = inject(CampaignService);
  private userState = inject(UserStateService);
  private cdr = inject(ChangeDetectorRef);

  isMasterOrAdmin = computed(() =>
    this.userState.isUserRole('Master') || this.userState.isUserRole('Admin')
  );

  ngOnInit() {
    this.loadCampaigns();
    this.subscribeToRouteChanges();
  }

  private subscribeToRouteChanges() {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.loadCampaigns());
  }

  private mapCampaignToCatalogGame(campaign: Campaign): CatalogGame {
    return {
      id: campaign.id,
      title: campaign.title,
      image: campaign.imageUrl 
        ? `http://localhost:5195/${campaign.imageUrl}` 
        : '/img/default-game.jpeg',
      level: campaign.level,
      price: campaign.price,
      tags: campaign.tags ?? [],
      hasAvailableSlots: campaign.hasAvailableSlots,
      isActive: campaign.isActive
    };
  }

  private mapUpdatedCampaignToCatalogGame(updatedCampaign: any): CatalogGame {
    return {
      id: updatedCampaign.id,
      title: updatedCampaign.title,
      image: updatedCampaign.image 
        ? `http://localhost:5195/${updatedCampaign.image}` 
        : '/img/default-game.jpeg',
      level: updatedCampaign.level,
      price: updatedCampaign.price,
      tags: updatedCampaign.tags || [],
      hasAvailableSlots: updatedCampaign.hasAvailableSlots ?? false,
      isActive: updatedCampaign.isActive ?? false
    };
  }

  loadCampaigns() {
    this.isLoading = true;
    this.error = null;
   
    this.campaignService.getCampaignCatalog().subscribe({
      next: (catalog) => {
        this.campaigns = catalog
          .map(c => this.mapCampaignToCatalogGame(c))
          .filter(c => c.isActive || this.isMasterOrAdmin());
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading campaigns:', error);
        this.error = 'Не удалось загрузить кампании. Попробуйте позже.';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onGameDeleted(gameId: string) {
    this.campaigns = this.campaigns.filter(game => game.id !== gameId);
    this.cdr.markForCheck();
  }

  onGameUpdated(updatedGame: any) {
    const catalogGame = this.mapUpdatedCampaignToCatalogGame(updatedGame);
    const index = this.campaigns.findIndex(game => game.id === catalogGame.id);
    
    if (index !== -1) {
      this.campaigns[index] = catalogGame;
      
      if (!catalogGame.isActive && !this.isMasterOrAdmin()) {
        this.campaigns = this.campaigns.filter(game => game.id !== catalogGame.id);
      }
    }
    this.cdr.markForCheck();
  }

  trackByGameId(index: number, game: CatalogGame): string {
    return game.id;
  }

  goToCreateCampaign() {
    this.router.navigate(['/campaigns/create']);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}