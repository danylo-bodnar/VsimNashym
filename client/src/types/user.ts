export interface User {
  id: number
  displayName: string
  profilePhotoFileId: string
  location: Point
}

interface Point {
  latitude: number
  longitude: number
}
