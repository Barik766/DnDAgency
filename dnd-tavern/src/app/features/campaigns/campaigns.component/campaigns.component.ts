import { Component, OnInit, OnDestroy, inject, computed, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CampaignService } from '../../services/campaign.service';
import { GameCardCatalog } from './game-card-catalog/game-card-catalog';
import { UserStateService } from '../../auth/services/user-state.service';
import { CatalogGame } from '../../interfaces/catalogGame.interface';
import { Campaign } from '../../interfaces/campaign.interface';

@Component({
  selector: 'app-campaigns',
  standalone: true,
  imports: [CommonModule, FormsModule, GameCardCatalog],
  templateUrl: './campaigns.component.html',
  styleUrls: ['./campaigns.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CampaignsComponent implements OnInit, OnDestroy {
  filteredCampaigns: CatalogGame[] = [];
  paginatedCampaigns: CatalogGame[] = [];
  availableTags: string[] = [];
  isLoading = false;
  error: string | null = null;
  
  // Pagination
  currentPage = 1;
  pageSize = 12;
  totalPages = 1;
  
  filters = {
    search: '',
    selectedTag: '',
    hasSlots: ''
  };
  
  sortBy: string = 'title';
  
  private destroy$ = new Subject<void>();
  private router = inject(Router);
  private campaignService = inject(CampaignService);
  private userState = inject(UserStateService);
  private cdr = inject(ChangeDetectorRef);

  isMasterOrAdmin = computed(() =>
    this.userState.isUserRole('Master') || this.userState.isUserRole('Admin')
  );

  ngOnInit() {
    this.loadAllTags();
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
        ? `/api/${campaign.imageUrl}` 
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
        ? `/api/${updatedCampaign.image}` 
        : '/img/default-game.jpeg',
      level: updatedCampaign.level,
      price: updatedCampaign.price,
      tags: updatedCampaign.tags || [],
      hasAvailableSlots: updatedCampaign.hasAvailableSlots ?? false,
      isActive: updatedCampaign.isActive ?? false
    };
  }

  private loadAllTags() {
    // Загружаем все теги один раз для dropdown
    this.campaignService.getCampaignCatalog().subscribe({
      next: (catalog) => {
        const tagsSet = new Set<string>();
        catalog.forEach(campaign => {
          campaign.tags?.forEach(tag => tagsSet.add(tag));
        });
        this.availableTags = Array.from(tagsSet).sort();
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading tags:', error);
      }
    });
  }

  loadCampaigns() {
    this.isLoading = true;
    this.error = null;
  
    this.campaignService.getCampaignCatalogPaged(
      this.currentPage,
      this.pageSize,
      this.filters.search || undefined,
      this.filters.selectedTag || undefined,
      this.filters.hasSlots ? this.filters.hasSlots === 'true' : undefined,
      this.sortBy
    ).subscribe({
      next: (result) => {
        this.filteredCampaigns = result.items
          .map(c => this.mapCampaignToCatalogGame(c))
          .filter(c => c.isActive || this.isMasterOrAdmin());
        
        this.paginatedCampaigns = this.filteredCampaigns;
        this.totalPages = result.totalPages;  
        
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading campaigns:', error);
        this.error = 'Failed to load campaigns. Please try again later.';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadCampaigns();
  }

  resetFilters() {
    this.filters = {
      search: '',
      selectedTag: '',
      hasSlots: ''
    };
    this.sortBy = 'title';
    this.applyFilters();
  }

  changePage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadCampaigns();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    if (this.totalPages <= maxVisible) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (this.currentPage <= 3) {
        for (let i = 1; i <= maxVisible; i++) {
          pages.push(i);
        }
      } else if (this.currentPage >= this.totalPages - 2) {
        for (let i = this.totalPages - maxVisible + 1; i <= this.totalPages; i++) {
          pages.push(i);
        }
      } else {
        for (let i = this.currentPage - 2; i <= this.currentPage + 2; i++) {
          pages.push(i);
        }
      }
    }
    
    return pages;
  }

  onGameDeleted(gameId: string) {
    this.loadCampaigns();
  }

  onGameUpdated(updatedGame: any) {
    this.loadCampaigns();
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