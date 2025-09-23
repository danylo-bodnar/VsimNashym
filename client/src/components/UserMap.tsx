import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet'
import L from 'leaflet'
import { useEffect, useState } from 'react'
import { getNearbyUsers, sendHi } from '@/features/users/api'
import type { User } from '@/types/user'

export default function UserMap() {
  const [position, setPosition] = useState<[number, number] | null>(null)
  const [users, setUsers] = useState<User[]>([])

  useEffect(() => {
    if ('geolocation' in navigator) {
      navigator.geolocation.getCurrentPosition(
        (pos) => setPosition([pos.coords.latitude, pos.coords.longitude]),
        (err) => {
          console.error('Geo error', err)
          setPosition([58.1467, 7.9956]) // fallback to Kristiansand
        }
      )
    } else {
      setPosition([58.1467, 7.9956])
    }
  }, [])

  // // Fetch nearby users whenever position is available
  useEffect(() => {
    if (!position) return

    const fetchUsers = async () => {
      try {
        const nearby = await getNearbyUsers(position[0], position[1], 5000)
        setUsers(nearby)
      } catch (err) {
        console.error('Failed to fetch nearby users', err)
      }
    }

    fetchUsers()
  }, [position])
  useEffect(() => {
    console.log('Updated users state:', users)
  }, [users])

  const avatarIcon = (photoId: string | null) =>
    L.icon({
      iconUrl:
        'https://static.vecteezy.com/system/resources/previews/039/609/002/non_2x/funny-clown-avatar-png.png',
      iconSize: [40, 40],
      className: 'rounded-full border-2 border-white shadow-md',
    })

  return (
    <div style={{ height: '400px', width: '100%' }}>
      {position && (
        <MapContainer
          center={position}
          zoom={13}
          style={{ height: '100%', width: '100%' }}
        >
          <TileLayer
            attribution="&copy; OpenStreetMap contributors"
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />

          {/* Current user marker */}
          <Marker
            position={position}
            icon={avatarIcon(null)} // Use fallback for current user
          >
            <Popup>You are here</Popup>
          </Marker>

          {/* Other users */}
          {users.map((u) => (
            <Marker
              key={u.profilePhotoFileId}
              position={[u.location.latitude, u.location.longitude]}
              icon={avatarIcon(u.profilePhotoFileId)}
            >
              <Popup>
                <div>
                  <p>{u.displayName}</p>
                  <button
                    onClick={() => sendHi(u.telegramId)}
                    className="bg-blue-500 text-white px-2 py-1 rounded"
                  >
                    Say Hi 👋
                  </button>
                </div>
              </Popup>
            </Marker>
          ))}
        </MapContainer>
      )}
    </div>
  )
}
