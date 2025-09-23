export interface User {
  telegramId: number
  displayName: string
  profilePhotoFileId: string
  location: Point
}

interface Point {
  latitude: number
  longitude: number
}
