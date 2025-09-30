export interface CatalogGame {
  isActive: boolean;
  id: string;             // GUID из сервера
  title: string;
  image: string;
  level: number;
  price: number;
  tags: string[];
  hasAvailableSlots: boolean;
  isInactive?: boolean;
}