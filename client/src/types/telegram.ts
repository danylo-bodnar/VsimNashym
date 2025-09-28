// types/telegram.ts
export interface TelegramWebAppUser {
  id: number
  first_name: string
  last_name?: string
  username?: string
  language_code?: string
  photo_url?: string
}

export interface Point {
  latitude: number
  longitude: number
}

export function mapTelegramUserToDto(
  tgUser: TelegramWebAppUser,
  location: Point
) {
  return {
    telegramId: tgUser.id,
    displayName:
      tgUser.username ||
      `${tgUser.first_name} ${tgUser.last_name || ''}`.trim(),
    profilePhotoFileId: '',
    location,
  }
}
