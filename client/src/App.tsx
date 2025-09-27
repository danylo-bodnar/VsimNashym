import { useEffect, useState } from 'react'
import { useTelegram } from './hooks/useTelegram'
import UserMap from './components/UserMap'
import { telegramLogin } from '@/features/auth/api'

function App() {
  const { tg, user, closeApp } = useTelegram()
  const [jwt, setJwt] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const initAuth = async () => {
      if (!tg || !user) return

      try {
        // Here we assume `user.id` is Telegram user ID
        const token = await telegramLogin(user.telegramId)
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

  if (loading) return <p>Loading...</p>
  if (error) return <p>Error: {error}</p>

  return (
    <div style={{ padding: 20 }}>
      {jwt ? <UserMap /> : <p>Please login to continue.</p>}
      <button onClick={closeApp}>Close Mini App</button>
    </div>
  )
}

export default App
