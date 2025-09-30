export interface User {
  id: string;
  email: string;
  username: string;
  displayName: string;
  avatar?: string;
  role: 'player' | 'gm' | 'admin';
  languages: string[];
  experienceLevel: 'beginner' | 'intermediate' | 'advanced' | 'expert';
  preferredGenres: string[];
  preferredSystems: string[];
  notifications: {
    email: boolean;
    push: boolean;
    telegram: boolean;
  };
  createdAt: Date;
  isVerified: boolean;
}