export type User = {
  telegramId: number
  displayName: string
  age: number
  bio?: string
  interests: string[]
  lookingFor: string[]
  languages: string[]
  profilePhotos: string[]
  location: LocationPoint
}

export interface LocationPoint {
  latitude: number
  longitude: number
}
