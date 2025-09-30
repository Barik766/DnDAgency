import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameDisplayPipe } from "../../../../campaigns/pipes/game-display.pipe";

export interface Game {
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
  selector: 'app-game-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './game-card.html',
  styleUrls: ['./game-card.scss']
})
export class GameCard {
  @Input({ required: true }) game!: Game;
}