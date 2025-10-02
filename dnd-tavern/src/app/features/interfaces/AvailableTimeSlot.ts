interface AvailableTimeSlot {
  dateTime: string;
  isAvailable: boolean;
  currentPlayers: number;
  maxPlayers: number;
  availableSlots: number;
  roomType: number; // previously 'Online' | 'Physical'
}