import apiClient from '@/utils/api-client'
import type { User } from '@/types/user'

export async function getNearbyUsers(
  lat: number,
  lng: number,
  radiusMeters: number
): Promise<User[]> {
  const response = await apiClient.get<User[]>('/user/nearby', {
    params: { lat, lng, radiusMeters },
  })
  console.log('response data', response.data)
  return response.data
}

export async function sendHi(targetTelegramId: number): Promise<void> {
  await apiClient.post('/message/hi', {
    to: targetTelegramId,
  })
}
