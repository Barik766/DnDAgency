import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CatalogGame } from '../../../interfaces/catalogGame.interface';
import { CampaignService } from '../../../services/campaign.service';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationModal } from '../../../services/confirmation-modal';
import { Router } from '@angular/router';

@Component({
  selector: 'app-game-card-catalog',
  standalone: true,
  imports: [CommonModule, NgbModule],
  templateUrl: './game-card-catalog.html',
  styleUrls: ['./game-card-catalog.scss']
})
export class GameCardCatalog {
  @Input({ required: true }) game!: CatalogGame;
  @Input() canEditOrAdmin = false;

  @Output() gameDeleted = new EventEmitter<string>();
  @Output() gameUpdated = new EventEmitter<CatalogGame>();

  private campaignService = inject(CampaignService);
  private modalService = inject(NgbModal);
  private router = inject(Router);  

  onEdit() {
    this.router.navigate(['/campaigns/update', this.game.id]);
  }

  onDelete() {
    const modalRef = this.modalService.open(ConfirmationModal);
    modalRef.componentInstance.title = 'Подтвердите удаление';
    modalRef.componentInstance.message = `Вы уверены, что хотите удалить кампанию "${this.game.title}"?`;

    modalRef.result.then((result) => {
      if (result === 'ok') {
        this.campaignService.deleteCampaign(this.game.id).subscribe({
          next: () => {
            //console.log('Deleted', this.game.id);
            // Уведомляем родительский компонент об удалении
            this.gameDeleted.emit(this.game.id);
          },
          error: (err) => console.error('Delete failed', err)
        });
      }
    }).catch(() => {});
  }

  onToggleStatus()  {
  const modalRef = this.modalService.open(ConfirmationModal);
  modalRef.componentInstance.title = this.game.isActive ? 'Деактивировать кампанию?' : 'Активировать кампанию?';
  modalRef.componentInstance.message = `Вы уверены, что хотите ${this.game.isActive ? 'деактивировать' : 'активировать'} кампанию "${this.game.title}"?`;
  
  modalRef.result.then((result) => {
      if (result === 'ok') {
        this.campaignService.toggleCampaignStatus(this.game.id).subscribe({
          next: (updated: any) => {
            //console.log('Server response:', updated);
            
            const data = updated.Data; // Извлекаем данные из обертки
            
            // Маппинг из серверного ответа (данные уже в camelCase)
            const mappedUpdate = {
              id: data.id,
              title: data.title,
              image: data.imageUrl,
              level: data.level,
              price: data.price,
              tags: data.tags || [],
              hasAvailableSlots: data.hasAvailableSlots ?? false,
              isActive: data.isActive ?? false
            };
            
            //console.log('Mapped update:', mappedUpdate);
            this.gameUpdated.emit(mappedUpdate);
          },
          error: (err) => console.error('Toggle failed', err)
        });
      }
    }).catch(() => {});
  }

  onDetails() {
    this.router.navigate(['/campaigns', this.game.id]);
  }

}