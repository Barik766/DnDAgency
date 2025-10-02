export interface Campaign {
  isActive: boolean;
  id: string;
  title: string;
  description: string;
  imageUrl?: string;
  level: number;
  maxPlayers: number;
  price: number;
  durationHours: number;
  supportedRoomTypes: string[];
  status: 'Draft' | 'Published' | 'Archived';
  masterId: string;
  masterName?: string;
  createdAt: string;
  updatedAt: string;
  tags: string[];
  slots: Slot[];              // <--- this will hold dates and slots
  hasAvailableSlots: boolean;
}