export type ProfilePhoto = {
  url: string
  messageId?: string
}

export type User = {
  telegramId: number
  displayName: string
  age: number
  bio?: string
  interests: string[]
  lookingFor: string[]
  languages: string[]
  profilePhotos: ProfilePhoto[]
  location: LocationPoint
}

export type RegisterUserDto = {
  displayName: string
  age: number
  bio?: string
  interests: string[]
  lookingFor: string[]
  languages: string[]
}

export interface LocationPoint {
  latitude: number
  longitude: number
}
