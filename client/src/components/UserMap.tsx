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
          // TODO: rewrite it
          console.error('Geo error', err)
          setPosition([58.1467, 7.9956]) // fallback to Kristiansand
        }
      )
    } else {
      setPosition([58.1467, 7.9956])
    }
  }, [])

  useEffect(() => {
    if (!position) return

    const fetchUsers = async () => {
      try {
        // TODO: avoid hardcoding distance value
        const nearby = await getNearbyUsers(position[0], position[1], 5000)
        setUsers(nearby)
      } catch (err) {
        console.error('Failed to fetch nearby users', err)
      }
    }

    fetchUsers()
  }, [position])

  const avatarIcon = (photoId: string | null) =>
    L.icon({
      iconUrl:
        'https://static.vecteezy.com/system/resources/previews/039/609/002/non_2x/funny-clown-avatar-png.png',
      iconSize: [40, 40],
      className: 'rounded-full border-2 border-white shadow-md',
    })

  return (
    <div className="h-full w-full">
      {position ? (
        <MapContainer center={position} zoom={13} className="h-full w-full">
          <TileLayer
            attribution="&copy; OpenStreetMap contributors &copy; CARTO"
            url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
          />

          {/* Current user marker */}
          <Marker position={position} icon={avatarIcon(null)}>
            <Popup>You are here</Popup>
          </Marker>

          {/* Nearby users */}
          {users.map((u) => (
            <Marker
              key={u.telegramId}
              position={[u.location.latitude, u.location.longitude]}
              icon={avatarIcon(u.profilePhotos[0])}
            >
              <Popup>
                <div className="flex flex-col gap-2">
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
      ) : (
        <div className="flex items-center justify-center h-full w-full">
          <p>Locating you...</p>
        </div>
      )}
    </div>
  )
}
