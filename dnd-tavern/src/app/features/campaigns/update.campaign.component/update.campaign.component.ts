import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CampaignService } from '../../services/campaign.service';
import { Campaign } from '../../interfaces/campaign.interface';

@Component({
  selector: 'app-update-campaign',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './update.campaign.component.html',
  styleUrls: ['./update.campaign.component.scss']
})
export class UpdateCampaignComponent implements OnInit {
  private fb = inject(FormBuilder);
  private campaignService = inject(CampaignService);
  private route = inject(ActivatedRoute);
  router = inject(Router);

  isSubmitting = false;
  error: string | null = null;
  selectedFile: File | null = null;
  fileError = false;
  campaignId!: string;

  roomTypes = [
    { value: 'Physical', label: 'In-person games' },
    { value: 'Online', label: 'Online games' }
  ];

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    level: [1, [Validators.required, Validators.min(1)]],
    price: [0, [Validators.required, Validators.min(0)]],
    tags: [''],
    maxPlayers: [1, [Validators.required, Validators.min(1)]],
    durationHours: [1, [Validators.required, Validators.min(1)]],
    supportedRoomTypes: this.fb.array([], Validators.required)
  });

  get supportedRoomTypesArray() {
    return this.form.get('supportedRoomTypes') as FormArray;
  }

  ngOnInit() {
    this.campaignId = this.route.snapshot.paramMap.get('id')!;
    this.campaignService.getCampaignById(this.campaignId).subscribe({
      next: (campaign: Campaign) => {
        this.form.patchValue({
          title: campaign.title,
          description: campaign.description,
          level: campaign.level,
          price: campaign.price,
          tags: campaign.tags.join(', '),
          maxPlayers: campaign.maxPlayers,
          durationHours: campaign.durationHours
        });

  // Set supported room types
        const roomTypesArray = this.supportedRoomTypesArray;
        roomTypesArray.clear();
        campaign.supportedRoomTypes?.forEach(type => {
          roomTypesArray.push(this.fb.control(type));
        });
      },
      error: (err) => {
        console.error('Error loading campaign:', err);
        this.error = 'Failed to load campaign';
      }
    });
  }

  onRoomTypeChange(roomType: string, event: any) {
    if (event.target.checked) {
      this.supportedRoomTypesArray.push(this.fb.control(roomType));
    } else {
      const index = this.supportedRoomTypesArray.controls.findIndex(x => x.value === roomType);
      this.supportedRoomTypesArray.removeAt(index);
    }
  }

  isRoomTypeSelected(roomType: string): boolean {
    return this.supportedRoomTypesArray.value.includes(roomType);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.fileError = false;
    }
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const formData = new FormData();
    formData.append('title', this.form.value.title!);
    formData.append('description', this.form.value.description!);
    formData.append('level', this.form.value.level!.toString());
    formData.append('price', this.form.value.price!.toString());

    const tagsArray = this.form.value.tags
      ? this.form.value.tags
          .split(',')
          .map(t => t.trim())
          .filter(t => t.length > 0)
      : [];
    tagsArray.forEach(tag => formData.append('tags', tag));

    formData.append('maxPlayers', this.form.value.maxPlayers!.toString());
    formData.append('durationHours', this.form.value.durationHours!.toString());

  // Add supported room types
    const roomTypes = this.supportedRoomTypesArray.value;
    roomTypes.forEach((type: string) => formData.append('supportedRoomTypes', type));

    if (this.selectedFile) {
      formData.append('ImageFile', this.selectedFile);
    }

    this.campaignService.updateCampaign(this.campaignId, formData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/campaigns']);
      },
      error: (err) => {
        console.error('Error updating campaign:', err);
        this.error = 'Failed to update campaign. Please try again later.';
        this.isSubmitting = false;
      }
    });
  }
}