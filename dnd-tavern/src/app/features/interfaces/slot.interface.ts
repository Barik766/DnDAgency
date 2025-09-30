interface Slot {
  id: string;
  campaignId: string;
  startTime: string; // ISO string с бэка
  currentPlayers: number;
  availableSlots: number;
  isFull: boolean;
  isInPast: boolean;
}