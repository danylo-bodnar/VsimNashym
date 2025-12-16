import type { User } from '@/types/user'
import { AxiosError } from 'axios'
import { useState } from 'react'
import { FaTimes } from 'react-icons/fa'

type Props = {
  user: User
  onClose: () => void
  onSayHi: (telegramId: number) => void
}

export default function UserProfile({ user, onClose, onSayHi }: Props) {
  const [isSendingHi, setIsSendingHi] = useState(false)
  const [hiSent, setHiSent] = useState(false)

  const handleSayHi = async () => {
    try {
      setIsSendingHi(true)
      await onSayHi(user.telegramId)
      setHiSent(true)
    } catch (e: unknown) {
      const err = e as AxiosError
      if (err.response?.status === 429) {
        alert('Please wait a bit before sending another Hi 👋')
      } else {
        console.error('Failed to send hi', err)
      }
    } finally {
      setIsSendingHi(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/50 z-[1000] flex items-end md:items-center justify-center">
      <div className="bg-white w-full md:w-96 md:rounded-2xl rounded-t-2xl max-h-[97vh] overflow-y-auto">
        {/* Header with close button */}
        <div className="sticky top-0 bg-white border-b border-gray-200 p-4 flex justify-between items-center">
          <h2 className="text-xl font-semibold">Profile</h2>
          <button
            onClick={onClose}
            className="p-2 hover:bg-gray-100 rounded-full transition-colors"
          >
            <FaTimes size={24} />
          </button>
        </div>

        {/* Profile content */}
        <div className="p-6 max-w-2xl mx-auto">
          {/* Avatar */}
          <div className="flex justify-center mb-6">
            <img
              src={
                user.avatar?.url ||
                'https://static.vecteezy.com/system/resources/previews/039/609/002/non_2x/funny-clown-avatar-png.png'
              }
              alt={user.displayName}
              className="w-32 h-32 rounded-full object-cover border-4 border-black"
            />
          </div>

          {/* Name & Age */}
          <div className="text-center mb-2">
            <h3 className="text-2xl font-bold">
              {user.displayName}, {user.age}
            </h3>
          </div>

          {/* Bio */}
          {user.bio && (
            <div className="mb-6">
              <p className="text-gray-800 text-center">{user.bio}</p>
            </div>
          )}

          {/* Additional photos */}
          {user.profilePhotos && user.profilePhotos.length > 0 && (
            <div className="mb-6">
              <h4 className="text-sm font-bold mb-3">Photos</h4>
              <div className="grid grid-cols-3 gap-2">
                {user.profilePhotos.map((photo, idx) => (
                  <img
                    key={photo.messageId}
                    src={photo.url}
                    alt={`${user.displayName} ${idx + 1}`}
                    className="w-full h-24 object-cover rounded-lg border border-black"
                  />
                ))}
              </div>
            </div>
          )}

          {/* Interests */}
          {user.interests && user.interests.length > 0 && (
            <div className="mb-6">
              <h4 className="text-sm font-bold mb-3">Interests</h4>
              <div className="flex flex-wrap gap-2">
                {user.interests.map((interest, idx) => (
                  <span
                    key={idx}
                    className="px-3 py-1 bg-black text-white rounded-full text-sm"
                  >
                    {interest}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Looking For */}
          {user.lookingFor && user.lookingFor.length > 0 && (
            <div className="mb-6">
              <h4 className="text-sm font-bold mb-3">Looking For</h4>
              <div className="flex flex-wrap gap-2">
                {user.lookingFor.map((item, idx) => (
                  <span
                    key={idx}
                    className="px-3 py-1 bg-gray-800 text-white rounded-full text-sm"
                  >
                    {item}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Languages */}
          {user.languages && user.languages.length > 0 && (
            <div className="mb-6">
              <h4 className="text-sm font-bold mb-3">Languages</h4>
              <div className="flex flex-wrap gap-2">
                {user.languages.map((lang, idx) => (
                  <span
                    key={idx}
                    className="px-3 py-1 bg-gray-200 text-black rounded-full text-sm border border-black"
                  >
                    {lang}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Say Hi button */}
          <button
            disabled={isSendingHi || hiSent}
            onClick={handleSayHi}
            className={`w-full py-3 px-6 rounded-lg font-bold transition
      ${
        isSendingHi || hiSent
          ? 'bg-gray-400 cursor-not-allowed'
          : 'bg-black hover:bg-gray-800 text-white'
      }`}
          >
            {isSendingHi
              ? 'Відправляю…'
              : hiSent
                ? '✓ Привіт надіслано'
                : 'Привітатись 👋'}
          </button>
        </div>
      </div>
    </div>
  )
}
