import { useState } from 'react'
import { getMyProfile, acceptLocationConsent } from '../../features/users/api'

interface Props {
  telegramId: number
  setUserData: (user: any) => void
}

export const LocationConsent: React.FC<Props> = ({
  telegramId,
  setUserData,
}) => {
  const [loading, setLoading] = useState(false)

  const consentTextUk =
    `Щоб показувати людей поруч, нам потрібна твоя згода на використання твоєї локації.\n\n` +
    `• Локація використовується тільки в застосунку\n` +
    `• Не передається третім сторонам\n` +
    `• Автоматично видаляється після 90 днів неактивності\n\n` +
    `Натисни «Прийняти», щоб продовжити 🚀`

  return (
    <div className="h-screen w-screen flex items-center justify-center bg-white px-8">
      <div className="text-center max-w-md font-sans">
        {/* Icon */}
        <div className="text-4xl mb-4">📍</div>

        {/* Title */}
        <h1 className="text-2xl font-bold text-black mb-3">
          Дозвіл на використання локації
        </h1>

        {/* Description */}
        <p className="text-gray-700 mb-6 whitespace-pre-line leading-relaxed text-base">
          {consentTextUk}
        </p>

        {/* Button */}
        <button
          className="w-full py-3 rounded-lg bg-black text-white font-medium text-lg hover:bg-gray-800 transition"
          disabled={loading}
          onClick={async () => {
            setLoading(true)
            await acceptLocationConsent(telegramId)
            const updatedUser = await getMyProfile()
            setUserData(updatedUser)
            setLoading(false)
          }}
        >
          {loading ? '⏳...' : '✅ Прийняти'}
        </button>
      </div>
    </div>
  )
}
