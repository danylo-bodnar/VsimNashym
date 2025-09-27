import apiClient from '@/utils/api-client'
import { jwtDecode } from 'jwt-decode'

interface TelegramLoginRequest {
  TelegramId: number
}

interface TelegramLoginResponse {
  token: string
}

interface JwtPayload {
  exp: number
}

export async function telegramLogin(telegramId: number): Promise<string> {
  try {
    console.log(`Attempting Telegram login for ID: ${telegramId}`)

    const response = await apiClient.post<TelegramLoginResponse>(
      '/Auth/telegram-login',
      { TelegramId: telegramId } as TelegramLoginRequest
    )

    const token = response.data.token
    if (!token) throw new Error('No token received from server')

    localStorage.setItem('accessToken', token)
    console.log('Telegram login successful')
    return token
  } catch (error: any) {
    if (error.response?.status === 401) {
      const errorMessage = error.response.data || 'User not registered'
      console.error('Telegram login failed:', errorMessage)
      throw new Error(`Registration required: ${errorMessage}`)
    }

    console.error('Telegram login error:', error)
    throw new Error('Login failed. Please try again.')
  }
}

export function isTokenValid(): boolean {
  const token = localStorage.getItem('accessToken')
  if (!token) return false

  try {
    const decoded = jwtDecode<JwtPayload>(token)
    if (!decoded.exp) return false

    const now = Date.now() / 1000
    return decoded.exp > now
  } catch (err) {
    console.error('Invalid token', err)
    return false
  }
}

export function getStoredToken(): string | null {
  const token = localStorage.getItem('accessToken')
  return isTokenValid() ? token : null
}

export function clearStoredToken(): void {
  localStorage.removeItem('accessToken')
}
