import { useEffect, useState } from 'react'
import { useTelegram } from './hooks/useTelegram'
import UserMap from './components/UserMap'
import ProfileSettings from './components/ProfileSettings'
import BottomNav from './components/BottomNav'
import type { User } from './types/user'
import Loader from './components/Loader'
import { telegramLogin } from './features/auth/api'
import { getUserById } from './features/users/api'

function App() {
  const { tg, user } = useTelegram()
  const [currentTab, setCurrentTab] = useState<'map' | 'profile'>('map')
  const [userData, setUserData] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [jwt, setJwt] = useState<string | null>(null)

  // Expand Telegram Web App
  useEffect(() => {
    tg?.expand()
  }, [tg])

  // Telegram login / auth
  useEffect(() => {
    const initAuth = async () => {
      if (!tg || !user) return

      try {
        const token = await telegramLogin(user.id)
        setJwt(token)
      } catch (err: any) {
        console.error('Login failed', err)
      }
    }

    initAuth()
  }, [tg, user])

  // Fetch existing user data
  useEffect(() => {
    const fetchUserData = async () => {
      if (!user?.id) {
        setIsLoading(false)
        return
      }

      try {
        const userData = await getUserById(user.id)

        debugger
        setUserData(userData)
      } catch (error: any) {
        if (error.response?.status === 404) {
          setUserData(null)
        } else {
          console.error('Error fetching user data:', error)
        }
      } finally {
        setIsLoading(false)
      }
    }

    fetchUserData()
  }, [user?.id])

  // Redirect unregistered users to ProfileSettings
  useEffect(() => {
    if (!isLoading) {
      setCurrentTab(userData ? 'map' : 'profile')
    }
  }, [isLoading, userData])

  if (isLoading) return <Loader />

  if (!user?.id) {
    return (
      <div className="h-screen w-screen flex items-center justify-center bg-white p-6">
        <div className="text-center">
          <p className="text-black text-lg mb-2">Error</p>
          <p className="text-gray-500">Please open this app through Telegram</p>
        </div>
      </div>
    )
  }

  return (
    <div className="h-screen w-screen flex flex-col">
      {/* Content section */}
      <div className="flex-1 overflow-hidden">
        {currentTab === 'map' && <UserMap />}
        {currentTab === 'profile' && (
          <ProfileSettings telegramId={user.id} existingUser={userData} />
        )}
      </div>

      {/* Bottom navigation */}
      <BottomNav currentTab={currentTab} setCurrentTab={setCurrentTab} />
    </div>
  )
}

export default App
