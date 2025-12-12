export type User = {
  telegramId: number
  displayName: string
  age: number
  bio?: string
  avatar: {
    url: string
    messageId: string
  }
  profilePhotos: Array<{
    url: string
    messageId: string
    slotIndex: number
  }>
  interests: string[]
  lookingFor: string[]
  languages: string[]
  location: LocationPoint
  createdAt: string
  updatedAt: string
  locationConsent: boolean
}

export type NearbyUser = {
  telegramId: number
  displayName: string
  avatarUrl: string | null
  location: LocationPoint
  lastActive: string
}

export type LocationPoint = {
  latitude: number
  longitude: number
}

export type RegisterUserDto = {
  displayName: string
  age: number
  bio?: string
}
