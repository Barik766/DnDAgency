interface Slot {
  id: string;
  campaignId: string;
  startTime: string; // ISO string from backend
  currentPlayers: number;
  availableSlots: number;
  isFull: boolean;
  isInPast: boolean;
}