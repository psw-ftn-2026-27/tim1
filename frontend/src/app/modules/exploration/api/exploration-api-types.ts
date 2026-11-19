export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export interface CurrentPositionDto {
  latitude: number;
  longitude: number;
  updatedAt: string;
}

export interface SaveCurrentPositionDto {
  latitude: number | null;
  longitude: number | null;
}

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface GroupSizeDto {
  min: number;
  max: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  transportTimes: TransportTimeDto[];
  groupSize: GroupSizeDto;
  length: number;
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  groupSize: GroupSizeDto;
}

export interface PublishTourDto {
  price: number;
}

export interface TourPreferenceDto {
  difficulties: TourDifficulty[];
  transport: TransportMode | null;
  tags: string[];
  opensSearch: boolean;
}

export interface CoordinateDto {
  latitude: number;
  longitude: number;
}

export interface KeypointDto {
  id: string;
  name: string;
  description: string;
  imageId: string;
  secret: string | null;
  location: CoordinateDto;
  publiclyVisible: boolean;
}

export interface AddKeypointDto {
  name: string;
  description: string;
  imageId: string;
  secret: string | null;
  location: CoordinateDto;
  lengthKm: number;
  publiclyVisible: boolean;
}

export interface UpdateKeypointDto {
  name: string;
  description: string;
  imageId: string;
  secret: string | null;
  location: CoordinateDto;
  lengthKm: number;
}

export interface SetTourLengthDto {
  lengthKm: number;
}

export interface SetKeypointVisibilityDto {
  publiclyVisible: boolean;
}
