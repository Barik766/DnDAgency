import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Master } from '../../interfaces/master.interface';
import { MasterService } from '../../services/master.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-masters',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './masters.component.html',
  styleUrl: './masters.component.scss'
})
export class MastersComponent implements OnInit {
  masters: Master[] = [];
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
  this.masters = masters; // Show all masters
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

  onMasterClick(master: Master): void {
    this.router.navigate(['/masters', master.id]);
  }
}