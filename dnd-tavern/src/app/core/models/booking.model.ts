export interface Booking {
  id: string;
  campaignId: string;
  slotId: string;
  playerId: string;
  seats: number;
  playerNames: string[];
  comment: string;
  status: 'pending' | 'confirmed' | 'paid' | 'cancelled' | 'completed';
  totalPrice: number;
  depositPaid: number;
  paymentUrl?: string;
  bookingDate: Date;
  lastUpdated: Date;
}