import { Pipe, PipeTransform } from '@angular/core';
import { Game } from '../../home/components/upcoming-games/game-card/game-card';

@Pipe({
  name: 'gameDisplay'
})
export class GameDisplayPipe implements PipeTransform {
  transform(game: Game): string {
    if (!game) return '';

    const date = new Date(game.date);
    const options: Intl.DateTimeFormatOptions = { weekday: 'long', month: 'long', day: 'numeric' };
    const formattedDate = date.toLocaleDateString(undefined, options); // Saturday, May 4

    const time = new Date(game.date).toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' }); // 6:00 pm

    return `${formattedDate}\n${time}\n${game.currentPlayers}/${game.maxPlayers} players • Level ${game.level}`;
  }
}
