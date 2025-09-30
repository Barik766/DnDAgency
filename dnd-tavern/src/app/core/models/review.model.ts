export interface Review {
  id: string;
  authorId: string;
  authorName: string;
  targetId: string; // campaign или gm ID
  targetType: 'campaign' | 'gm';
  rating: number;
  title: string;
  comment: string;
  pros: string[];
  cons: string[];
  wouldRecommend: boolean;
  isVerified: boolean;
  createdAt: Date;
  gmResponse?: {
    comment: string;
    createdAt: Date;
  };
}