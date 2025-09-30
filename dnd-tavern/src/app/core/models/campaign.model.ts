import { Booking } from "./booking.model";
import { GameMaster } from "./gm.model";
import { Review } from "./review.model";

export interface Campaign {
  id: string;
  title: string;
  description: string;
  synopsis: string;
  coverImage: string;
  gmId: string;
  gm: GameMaster;
  system: string;
  genre: string[];
  level: {
    min: number;
    max: number;
  };
  duration: number; // в часах
  language: string[];
  pricePerSession: number;
  deposit: number;
  maxPlayers: number;
  minPlayers: number;
  isOnline: boolean;
  location?: string;
  tags: string[];
  safetyTools: string[];
  tableRules: string;
  whatToBring: string;
  sessionZero: boolean;
  status: 'draft' | 'published' | 'hidden';
  slots: CampaignSlot[];
  reviews: Review[];
  averageRating: number;
  totalSessions: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CampaignSlot {
  id: string;
  campaignId: string;
  startTime: Date;
  endTime: Date;
  timezone: string;
  availableSeats: number;
  bookedSeats: number;
  status: 'available' | 'full' | 'cancelled';
  price: number;
  bookings: Booking[];
}