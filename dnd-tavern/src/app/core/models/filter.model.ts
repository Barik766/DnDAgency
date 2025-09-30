export interface CampaignFilters {
  search?: string;
  city?: string;
  isOnline?: boolean;
  dateFrom?: Date;
  dateTo?: Date;
  levelMin?: number;
  levelMax?: number;
  priceMin?: number;
  priceMax?: number;
  languages?: string[];
  duration?: number[];
  partySize?: number[];
  genres?: string[];
  systems?: string[];
  gmRating?: number;
  availableSeatsOnly?: boolean;
  sortBy?: 'date' | 'rating' | 'price' | 'newest';
  sortOrder?: 'asc' | 'desc';
}

export interface GMFilters {
  search?: string;
  rating?: number;
  experience?: number;
  priceMin?: number;
  priceMax?: number;
  languages?: string[];
  systems?: string[];
  genres?: string[];
  isOnline?: boolean;
  isOffline?: boolean;
  availableDates?: Date[];
}