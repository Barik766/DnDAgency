import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Master } from '../../../interfaces/master.interface';
import { MasterService } from '../../../services/master.service';

@Component({
  selector: 'app-our-masters',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './our-masters.html',
  styleUrl: './our-masters.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OurMastersComponent implements OnInit {
  masters: Master[] = [];
  isLoading = false;
  error: string | null = null;
 
  private masterService = inject(MasterService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.loadMasters();
  }

  loadMasters() {
    this.isLoading = true;
    this.error = null;
   
    this.masterService.getAllMasters().subscribe({
      next: (masters) => {
        this.masters = masters.slice(0, 3);
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading masters:', error);
  this.error = 'Failed to load masters';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onMasterClick(master: Master): void {
  console.log('Selected master:', master);
  }

  onViewAllMasters(): void {
  console.log('Show all masters');
  }
}