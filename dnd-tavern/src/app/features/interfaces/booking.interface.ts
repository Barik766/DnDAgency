export interface Booking {
  id: string;
  userId: string;
  slotId: string;
  createdAt: string;
  user: {
    id: string;
    username: string;
    email: string;
    role: string;
    isMaster: boolean;
    isAdmin: boolean;
  };
  slot: {
    id: string;
    campaignId: string;
    startTime: string;
    currentPlayers: number;
    availableSlots: number;
    isFull: boolean;
    isInPast: boolean;
    roomType: number;
    campaignTitle: string;
  };
}