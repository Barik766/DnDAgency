import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CampaignService } from '../../services/campaign.service';

@Component({
  selector: 'app-create-campaign',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create.campaign.component.html',
  styleUrls: ['./create.campaign.component.scss']
})
export class CreateCampaignComponent {
  private fb = inject(FormBuilder);
  private campaignService = inject(CampaignService);
  router = inject(Router);

  isSubmitting = false;
  error: string | null = null;
  selectedFile: File | null = null;
  fileError = false;

  roomTypes = [
    { value: 'Physical', label: 'Физические игры' },
    { value: 'Online', label: 'Онлайн игры' }
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

  onRoomTypeChange(roomType: string, event: any) {
    if (event.target.checked) {
      this.supportedRoomTypesArray.push(this.fb.control(roomType));
    } else {
      const index = this.supportedRoomTypesArray.controls.findIndex(x => x.value === roomType);
      this.supportedRoomTypesArray.removeAt(index);
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.fileError = false;
    }
  }

  submit() {
    if (this.form.invalid || !this.selectedFile) {
      this.form.markAllAsTouched();
      if (!this.selectedFile) this.fileError = true;
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
    
    // Добавляем поддерживаемые типы комнат
    const roomTypes = this.supportedRoomTypesArray.value;
    roomTypes.forEach((type: string) => formData.append('supportedRoomTypes', type));

    formData.append('ImageFile', this.selectedFile);

    this.campaignService.createCampaign(formData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/campaigns']);
      },
      error: (err) => {
        console.error('Error creating campaign:', err);
        this.error = 'Не удалось создать кампанию. Попробуйте позже.';
        this.isSubmitting = false;
      }
    });
  }
}