import { Campaign } from "./campaign.model";
import { Review } from "./review.model";

export interface GameMaster {
  id: string;
  username: string;
  displayName: string;
  avatar: string;
  bio: string;
  slogan: string;
  experience: number; // количество проведенных сессий
  rating: number;
  totalPlayers: number;
  languages: string[];
  systems: string[];
  genres: string[];
  styles: string[];
  safetyTools: string[];
  isOnline: boolean;
  isOffline: boolean;
  location?: string;
  pricePerHour?: number;
  pricePerSession?: number;
  badges: string[];
  socialLinks: {
    platform: string;
    url: string;
  }[];
  availability: GMAvailability[];
  campaigns: Campaign[];
  reviews: Review[];
  createdAt: Date;
}

export interface GMAvailability {
  date: Date;
  timeSlots: {
    startTime: string;
    endTime: string;
    available: boolean;
  }[];
}