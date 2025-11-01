import apiClient from '@/utils/api-client'
import type { NearbyUser, User } from '@/types/user'

interface SubmitUserOptions {
  formData: FormData
  isEditMode: boolean
  telegramId?: number
}

export async function submitUser({
  formData,
  isEditMode,
  telegramId,
}: SubmitUserOptions): Promise<User> {
  if (isEditMode) {
    if (!telegramId)
      throw new Error('telegramId is required for updating a user')
    const response = await apiClient.put<User>(`/user/${telegramId}`, formData)
    return response.data
  } else {
    const response = await apiClient.post<User>('/user/register', formData)
    return response.data
  }
}

export async function getUserById(telegramId: number): Promise<User> {
  const response = await apiClient.get<User>(`/user/${telegramId}`)
  return response.data
}

export async function updateUserLocation(
  telegramId: number,
  latitude: number,
  longitude: number,
): Promise<User> {
  const response = await apiClient.put<User>(`/user/${telegramId}/location`, {
    latitude,
    longitude,
  })
  return response.data
}

export async function getNearbyUsers(
  lat: number,
  lng: number,
  radiusMeters: number,
): Promise<NearbyUser[]> {
  const response = await apiClient.get<NearbyUser[]>('/user/nearby', {
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
