import { Photo } from './photo';

export interface Member {
  Id: number;
  userName: string;
  photoUrl: string;
  age: number;
  knownAs: string;
  created: Date;
  lastActive: Date;
  introduction: string;
  lookingFor: string;
  city: string;
  country: string;
  photos: Photo[];
}
