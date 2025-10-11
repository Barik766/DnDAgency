
export interface MasterCard {
  id: string;
  userId: string;
  name: string;
  bio: string;
  isActive: boolean;
  campaignCount: number;
  averageRating: number;
  reviewCount: number;
  photoUrl: string; 
  createdAt: string;
  updatedAt: string | null;
}