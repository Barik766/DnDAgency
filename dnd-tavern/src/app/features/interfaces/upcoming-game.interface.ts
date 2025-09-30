export interface UpcomingGame {
  campaignId: string;
  campaignTitle: string;
  campaignImageUrl: string | null;
  level: number;
  slotId: string;
  startTime: string;
  maxPlayers: number;
  bookedPlayers: number;
  availableSlots: number;
  isFull: boolean;
}