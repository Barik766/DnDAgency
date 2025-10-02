import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-newsletter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './newsletter.html',
  styleUrl: './newsletter.scss'
})
export class NewsletterComponent {
  email: string = '';
  isSubmitting: boolean = false;
  isSubscribed: boolean = false;

  onSubscribe() {
    if (!this.email || !this.isValidEmail(this.email)) {
      return;
    }

    this.isSubmitting = true;
    
      // Simulate sending request
    setTimeout(() => {
      this.isSubmitting = false;
      this.isSubscribed = true;
      this.email = '';
      
  // Hide success message after 3 seconds
      setTimeout(() => {
        this.isSubscribed = false;
      }, 3000);
    }, 1000);
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
}