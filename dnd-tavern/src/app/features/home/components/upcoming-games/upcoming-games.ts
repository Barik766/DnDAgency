import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UpcomingGame } from '../../../interfaces/upcoming-game.interface';
import { CampaignService } from '../../../services/campaign.service';
import { GameCard } from "./game-card/game-card";

interface Game {
  id: string;
  title: string;
  image: string;
  date: string;
  time: string;
  currentPlayers: number;
  maxPlayers: number;
  level: number;
}

@Component({
  selector: 'app-upcoming-games',
  standalone: true,
  imports: [CommonModule, GameCard],
  templateUrl: './upcoming-games.html',
  styleUrls: ['./upcoming-games.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UpcomingGames implements OnInit {
  upcomingGames: Game[] = [];
  isLoading = false;
  error: string | null = null;

  private campaignService = inject(CampaignService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.loadUpcomingGames();
  }

  loadUpcomingGames() {
    this.isLoading = true;
    this.error = null;

    this.campaignService.getUpcomingGames().subscribe({
      next: (games) => {
    this.upcomingGames = games.map(this.mapToGame);
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading upcoming games:', error);
        this.error = 'Failed to load upcoming games';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  private mapToGame(upcomingGame: UpcomingGame): Game {
    const startTime = new Date(upcomingGame.startTime);
   
    return {
      id: upcomingGame.campaignId,
      title: upcomingGame.campaignTitle,
      image: upcomingGame.campaignImageUrl
        ? `http://localhost:5195/${upcomingGame.campaignImageUrl}`
        : '/img/default-game.jpeg',
      date: startTime.toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'long',
        day: 'numeric'
      }),
      time: startTime.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit'
      }),
      currentPlayers: upcomingGame.bookedPlayers,
      maxPlayers: upcomingGame.maxPlayers,
      level: upcomingGame.level
    };
  }
}