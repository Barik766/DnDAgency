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
      title: 'Choose a campaign',
      description: 'Find a suitable game by genre, level, and schedule',
      icon: '🎲'
    },
    {
      number: 2,
      title: 'Pick a slot',
      description: 'Reserve a convenient time to play',
      icon: '📅'
    },
    {
      number: 3,
      title: 'Book',
      description: 'Pay and receive booking confirmation',
      icon: '✨'
    }
  ];
}
