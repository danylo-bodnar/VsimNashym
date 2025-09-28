import { useEffect, useState } from 'react'
import { useTelegram } from './hooks/useTelegram'
import UserMap from './components/UserMap'
import { telegramLogin } from '@/features/auth/api'
import { mapTelegramUserToDto } from './types/telegram'

function App() {
  const { tg, user, closeApp } = useTelegram()
  const [jwt, setJwt] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const initAuth = async () => {
      if (!tg || !user) return

      try {
        const dto = mapTelegramUserToDto(user, {
          latitude: 58.1467,
          longitude: 7.9956,
        })
        const token = await telegramLogin(dto.telegramId)
        setJwt(token)
      } catch (err: any) {
        console.error('Login failed', err)
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }

    initAuth()
  }, [tg, user])

  useEffect(() => {
    console.log('jwt', jwt)
  }, [jwt])

  if (loading) return <p>Loading...</p>
  if (error) return <p>Error: {error}</p>

  return <div>{jwt ? <UserMap /> : <p>Please login to continue.</p>}</div>
}

export default App
