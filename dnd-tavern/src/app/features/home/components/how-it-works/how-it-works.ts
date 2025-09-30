import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';


@Component({
  selector: 'app-how-it-works',
  imports: [CommonModule],
  templateUrl: './how-it-works.html',
  styleUrl: './how-it-works.scss'
})

export class HowItWorksComponent {
  steps = [
    {
      number: 1,
      title: 'Выбери кампанию',
      description: 'Найди подходящую игру по жанру, уровню и расписанию',
      icon: '🎲'
    },
    {
      number: 2,
      title: 'Выбери слот',
      description: 'Забронируй удобное время для игры',
      icon: '📅'
    },
    {
      number: 3,
      title: 'Забронируй',
      description: 'Оплати и получи подтверждение брони',
      icon: '✨'
    }
  ];
}
