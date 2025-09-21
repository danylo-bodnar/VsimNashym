import apiClient from '@/utils/api-client'

interface TelegramLoginRequest {
  TelegramId: number
}

interface TelegramLoginResponse {
  token: string
}

export async function telegramLogin(telegramId: number): Promise<string> {
  try {
    console.log(`Attempting Telegram login for ID: ${telegramId}`)

    const response = await apiClient.post<TelegramLoginResponse>(
      '/Auth/telegram-login',
      {
        TelegramId: telegramId,
      } as TelegramLoginRequest
    )

    const token = response.data.token

    if (!token) {
      throw new Error('No token received from server')
    }

    localStorage.setItem('accessToken', token)
    localStorage.setItem(
      'tokenExpiration',
      (Date.now() + 30 * 60 * 1000).toString()
    )

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

export function getStoredToken(): string | null {
  const token = localStorage.getItem('accessToken')
  if (!token) return null

  const expiration = localStorage.getItem('tokenExpiration')
  if (expiration && Date.now() > parseInt(expiration)) {
    console.log('Token expired, clearing storage')
    clearStoredToken()
    return null
  }

  return token
}

export function clearStoredToken(): void {
  localStorage.removeItem('accessToken')
  localStorage.removeItem('tokenExpiration')
}

export function isTokenValid(): boolean {
  return !!getStoredToken()
}
