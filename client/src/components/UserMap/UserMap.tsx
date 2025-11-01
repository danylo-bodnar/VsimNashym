import { MapContainer, TileLayer, Marker, Popup, Circle } from 'react-leaflet'
import L from 'leaflet'
import { useEffect, useState } from 'react'
import {
  getNearbyUsers,
  getUserById,
  sendHi,
  updateUserLocation,
} from '@/features/users/api'
import type { NearbyUser, User } from '@/types/user'
import UserProfile from '../UserProfile/UserProfile'

const radius = 500000

type Props = {
  existingUser: User
}

export default function UserMap({ existingUser }: Props) {
  const [position, setPosition] = useState<[number, number] | null>(null)
  const [nearbyUsers, setNearbyUsers] = useState<NearbyUser[]>([])
  const [selectedUser, setSelectedUser] = useState<User | null>(null)
  const [isLoadingProfile, setIsLoadingProfile] = useState(false)

  useEffect(() => {
    if (!('geolocation' in navigator)) return

    let lastSentPosition: [number, number] | null = null

    const watchId = navigator.geolocation.watchPosition(
      async (pos) => {
        const currentPos: [number, number] = [
          pos.coords.latitude,
          pos.coords.longitude,
        ]
        setPosition(currentPos)

        if (
          !lastSentPosition ||
          getDistance(lastSentPosition, currentPos) > 200
        ) {
          await updateUserLocation(
            existingUser.telegramId,
            currentPos[0],
            currentPos[1],
          )
          lastSentPosition = currentPos
          console.log('Position updated in DB')
        }
      },
      (err) => console.error('Geo error', err),
      {
        enableHighAccuracy: false,
        maximumAge: 60000,
        timeout: 10000,
      },
    )

    return () => navigator.geolocation.clearWatch(watchId)
  }, [])

  useEffect(() => {
    if (!position) return

    const fetchNearbyUsers = async () => {
      try {
        const nearby = await getNearbyUsers(position[0], position[1], radius)
        setNearbyUsers(nearby)
      } catch (err) {
        console.error('Failed to fetch nearby users', err)
      }
    }

    fetchNearbyUsers()
  }, [position])

  function getDistance(
    [lat1, lon1]: [number, number],
    [lat2, lon2]: [number, number],
  ) {
    const toRad = (value: number) => (value * Math.PI) / 180
    const R = 6371000 // Earth radius in meters

    const dLat = toRad(lat2 - lat1)
    const dLon = toRad(lon2 - lon1)
    const a =
      Math.sin(dLat / 2) ** 2 +
      Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon / 2) ** 2
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
    return R * c
  }

  const handleViewProfile = async (telegramId: number) => {
    setIsLoadingProfile(true)
    try {
      const fullUser = await getUserById(telegramId)
      setSelectedUser(fullUser)
    } catch (err) {
      console.error('Failed to fetch user profile', err)
    } finally {
      setIsLoadingProfile(false)
    }
  }

  const avatarIcon = (avatar: string | null) =>
    L.icon({
      iconUrl: avatar
        ? avatar
        : 'https://static.vecteezy.com/system/resources/previews/039/609/002/non_2x/funny-clown-avatar-png.png',
      iconSize: [40, 40],
      className: 'rounded-full border-2 border-black shadow-md',
    })

  const handleSayHi = async (telegramId: number) => {
    try {
      await sendHi(telegramId)
      setSelectedUser(null)
    } catch (err) {
      console.error('Failed to send hi', err)
    }
  }

  return (
    <div className="h-full w-full relative">
      {position ? (
        <MapContainer center={position} zoom={13} className="h-full w-full">
          <TileLayer
            attribution="&copy; OpenStreetMap contributors &copy; CARTO"
            url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
          />
          {/* Current user marker */}
          <Marker
            position={position}
            icon={avatarIcon(existingUser?.avatar.url || null)}
          >
            <Popup>You are here</Popup>
          </Marker>
          <Circle
            center={position}
            radius={radius}
            color="rgba(128,128,128,0.5)"
            fillColor="gray"
            fillOpacity={0.05}
            weight={2}
            interactive={false}
          />
          {/* Nearby users */}
          {nearbyUsers.map((u) => (
            <Marker
              key={u.telegramId}
              position={[u.location.latitude, u.location.longitude]}
              icon={avatarIcon(u.avatarUrl)}
            >
              <Popup>
                <div className="flex flex-col items-center gap-0 p-0 min-w-[150px]">
                  <div className="w-16 h-16 rounded-full overflow-hidden border-2 border-black shadow-lg">
                    <img
                      src={
                        u.avatarUrl ||
                        'https://static.vecteezy.com/system/resources/previews/039/609/002/non_2x/funny-clown-avatar-png.png'
                      }
                      alt={u.displayName}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  <p className="font-bold text-lg text-center text-black">
                    {u.displayName}
                  </p>
                  <button
                    onClick={() => handleViewProfile(u.telegramId)}
                    disabled={isLoadingProfile}
                    className="w-full bg-black hover:bg-gray-800 text-white px-4 py-2 rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
                  >
                    {isLoadingProfile ? 'Loading...' : 'View Profile'}
                  </button>
                </div>
              </Popup>
            </Marker>
          ))}
        </MapContainer>
      ) : (
        <div className="flex items-center justify-center h-full w-full">
          <p>Locating you...</p>
        </div>
      )}

      {/* User Profile Modal */}
      {selectedUser && (
        <UserProfile
          user={selectedUser}
          onClose={() => setSelectedUser(null)}
          onSayHi={handleSayHi}
        />
      )}
    </div>
  )
}
