import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'confirmation-modal',
  standalone: true,
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ title }}</h5>
      <button type="button" class="btn-close" aria-label="Close" (click)="activeModal.dismiss()"></button>
    </div>
    <div class="modal-body">
      <p>{{ message }}</p>
    </div>
    <div class="modal-footer">
      <button class="btn btn-secondary" (click)="activeModal.dismiss()">Отмена</button>
      <button class="btn btn-danger" (click)="activeModal.close('ok')">Подтвердить</button>
    </div>
  `,
})
export class ConfirmationModal {
  @Input() title!: string;
  @Input() message!: string;

  constructor(public activeModal: NgbActiveModal) {}
}
