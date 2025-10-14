import { useForm } from 'react-hook-form'
import { useState, useEffect } from 'react'
import type { User, LocationPoint, RegisterUserDto } from '@/types/user'
import { submitUser } from '@/features/users/api'
import { telegramLogin } from '@/features/auth/api'

const INTERESTS = [
  'Кава',
  'Спорт',
  'Музика',
  'Пиво',
  'Природа',
  'Мистецтво',
  'Ігри',
  'Книги',
]
const LOOKING_FOR = [
  'Друзів',
  'Практика мови',
  'Відносини',
  'Інтеграційні поради',
  'Cпілкування',
]
const LANGUAGES = [
  'Українська',
  'Norsk',
  'English',
  'Русский',
  'Polsk',
  'Deutsch',
]

type PhotoMeta = {
  url: string | null
  file?: File | null
  messageId?: string | null
}

type ProfileSettingsProps = {
  existingUser: User | null
  telegramId: number
  onRegister?: (userData: User, jwt: string) => void
}

export default function ProfileSettings({
  existingUser,
  telegramId,
  onRegister,
}: ProfileSettingsProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<RegisterUserDto>({
    defaultValues: existingUser
      ? {
          displayName: existingUser.displayName,
          age: existingUser.age,
          bio: existingUser.bio || '',
          interests: existingUser.interests,
          lookingFor: existingUser.lookingFor,
          languages: existingUser.languages,
        }
      : {
          interests: [],
          lookingFor: [],
          languages: [],
        },
  })

  const [photos, setPhotos] = useState<PhotoMeta[]>(
    existingUser?.profilePhotos.map((p) => ({
      url: p.url,
      messageId: p.messageId,
    })) || [
      { url: '', file: undefined },
      { url: '', file: undefined },
      { url: '', file: undefined },
    ]
  )

  const [photoFiles, setPhotoFiles] = useState<(File | null)[]>([
    null,
    null,
    null,
  ])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [selectedInterests, setSelectedInterests] = useState<string[]>(
    existingUser?.interests || []
  )
  const [selectedLookingFor, setSelectedLookingFor] = useState<string[]>(
    existingUser?.lookingFor || []
  )
  const [selectedLanguages, setSelectedLanguages] = useState<string[]>(
    existingUser?.languages || []
  )

  const isEditMode = !!existingUser

  const handleGetLocation = (): Promise<LocationPoint> => {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        return reject(new Error('Geolocation is not supported by your browser'))
      }

      navigator.geolocation.getCurrentPosition(
        (pos) => {
          const location: LocationPoint = {
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
          }
          resolve(location)
        },
        (err) => {
          console.error('Geo error', err)
          reject(err)
        }
      )
    })
  }

  useEffect(() => {
    if (existingUser) {
      reset({
        displayName: existingUser.displayName,
        age: existingUser.age,
        bio: existingUser.bio || '',
        interests: existingUser.interests,
        lookingFor: existingUser.lookingFor,
        languages: existingUser.languages,
      })
      setPhotos([
        existingUser?.profilePhotos?.[0] || null,
        existingUser?.profilePhotos?.[1] || null,
        existingUser?.profilePhotos?.[2] || null,
      ])
      setSelectedInterests(existingUser.interests || [])
      setSelectedLookingFor(existingUser.lookingFor || [])
      setSelectedLanguages(existingUser.languages || [])
    }
  }, [existingUser, reset])

  const handlePhotoChange = (index: number, file: File | null) => {
    const newPhotos = [...photos]
    if (file) {
      newPhotos[index] = {
        url: URL.createObjectURL(file),
        file,
        messageId: newPhotos[index]?.messageId,
      }
    } else {
      newPhotos[index] = { url: '', file: undefined, messageId: undefined }
    }
    setPhotos(newPhotos)
  }

  const removePhoto = (index: number) => {
    const newPhotos = [...photos]
    if (newPhotos[index]?.messageId) {
      newPhotos[index] = {
        url: '',
        file: null,
        messageId: newPhotos[index].messageId,
      }
    } else {
      newPhotos[index] = { url: '', file: null, messageId: undefined }
    }
    setPhotos(newPhotos)
  }

  const toggleSelection = (
    item: string,
    list: string[],
    setter: (val: string[]) => void
  ) => {
    if (list.includes(item)) {
      setter(list.filter((i) => i !== item))
    } else {
      setter([...list, item])
    }
  }

  const onSubmit = async (data: RegisterUserDto) => {
    let userLocation: LocationPoint

    try {
      userLocation = await handleGetLocation()
    } catch (err) {
      console.error('Failed to get location, using default', err)
      return
    }

    if (!isEditMode && !photos.some((p) => p.url)) {
      alert('Будь ласка, додайте хоча б одне фото')
      return
    }

    setIsSubmitting(true)
    try {
      const formData = new FormData()
      formData.append('telegramId', telegramId.toString())
      formData.append('displayName', data.displayName)
      formData.append('age', data.age.toString())
      if (data.bio) formData.append('bio', data.bio)

      selectedInterests.forEach((interest) =>
        formData.append('interests', interest)
      )
      selectedLookingFor.forEach((item) => formData.append('lookingFor', item))
      selectedLanguages.forEach((lang) => formData.append('languages', lang))

      photos.forEach((photo, index) => {
        if (photo.file) formData.append(`profilePhotos`, photo.file)
        if (!photo.file && photo.messageId)
          formData.append('existingPhotoMessageIds', photo.messageId)
      })

      formData.append('latitude', userLocation.latitude.toString())
      formData.append('longitude', userLocation.longitude.toString())

      const newUser = await submitUser({
        formData,
        isEditMode,
        telegramId: existingUser?.telegramId,
      })

      if (isEditMode) {
        alert('Профіль успішно оновлено!')
      } else {
        alert('Профіль успішно створено!')

        const token = await telegramLogin(telegramId)
        onRegister?.(newUser, token)
      }
    } catch (err) {
      console.error('Помилка збереження профілю:', err)
      alert('Помилка збереження профілю')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="h-full w-full flex items-center justify-center bg-white p-6 overflow-y-auto">
      <div className="w-full max-w-md my-auto pb-8">
        <div className="space-y-6">
          {/* Header */}
          <div>
            <h1 className="text-3xl font-light tracking-tight text-black mb-1">
              {isEditMode ? 'Редагувати профіль' : 'Створити профіль'}
            </h1>
            <p className="text-sm text-gray-500">
              Допоможіть іншим дізнатися про вас
            </p>
          </div>

          {/* Photo Grid */}
          <div>
            <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium mb-3">
              Фото (до 3)
            </label>
            <div className="grid grid-cols-3 gap-3">
              {[0, 1, 2].map((index) => (
                <div key={index} className="relative aspect-square">
                  <input
                    id={`photo-${index}`}
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={(e) => {
                      if (e.target.files?.[0]) {
                        handlePhotoChange(index, e.target.files[0])
                      }
                    }}
                  />
                  <label
                    htmlFor={`photo-${index}`}
                    className="block h-full w-full cursor-pointer"
                  >
                    {photos[index] ? (
                      <div className="relative h-full w-full group">
                        <img
                          src={photos[index].url!}
                          alt={`Фото ${index + 1}`}
                          className="h-full w-full object-cover border-2 border-black"
                        />
                        <button
                          type="button"
                          onClick={(e) => {
                            e.preventDefault()
                            removePhoto(index)
                          }}
                          className="absolute top-1 right-1 bg-black text-white w-6 h-6 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity text-lg leading-none"
                        >
                          ×
                        </button>
                      </div>
                    ) : (
                      <div className="h-full w-full border-2 border-dashed border-gray-300 flex items-center justify-center hover:border-black transition-colors">
                        <svg
                          className="w-8 h-8 text-gray-400"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={1.5}
                            d="M12 4v16m8-8H4"
                          />
                        </svg>
                      </div>
                    )}
                  </label>
                </div>
              ))}
            </div>
            {!isEditMode && !photos.some((p) => p !== null) && (
              <p className="text-gray-500 text-xs mt-2">
                Потрібне хоча б одне фото
              </p>
            )}
          </div>

          {/* Display Name */}
          <div className="space-y-2">
            <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium">
              Ім'я
            </label>
            <input
              {...register('displayName', {
                required: "Ім'я обов'язкове",
                maxLength: 50,
              })}
              placeholder="Як вас називати?"
              className="w-full px-0 py-3 border-0 border-b-2 border-gray-200 focus:border-black bg-transparent text-black placeholder-gray-400 outline-none transition-colors"
            />
            {errors.displayName && (
              <p className="text-black text-sm">{errors.displayName.message}</p>
            )}
          </div>

          {/* Age */}
          <div className="space-y-2">
            <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium">
              Вік
            </label>
            <input
              type="number"
              {...register('age', {
                required: "Вік обов'язковий",
                min: { value: 16, message: 'Має бути 16+' },
                max: { value: 100, message: 'Невірний вік' },
              })}
              placeholder="Ваш вік"
              className="w-full px-0 py-3 border-0 border-b-2 border-gray-200 focus:border-black bg-transparent text-black placeholder-gray-400 outline-none transition-colors"
            />
            {errors.age && (
              <p className="text-black text-sm">{errors.age.message}</p>
            )}
          </div>

          {/* Languages */}
          <div className="space-y-2">
            <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium">
              Мови, якими я розмовляю
            </label>
            <div className="flex flex-wrap gap-2">
              {LANGUAGES.map((lang) => (
                <button
                  key={lang}
                  type="button"
                  onClick={() =>
                    toggleSelection(
                      lang,
                      selectedLanguages,
                      setSelectedLanguages
                    )
                  }
                  className={`px-4 py-2 border-2 text-sm font-medium transition-colors ${
                    selectedLanguages.includes(lang)
                      ? 'border-black bg-black text-white'
                      : 'border-gray-300 text-black hover:border-black'
                  }`}
                >
                  {lang}
                </button>
              ))}
            </div>
          </div>

          {/* Bio */}
          <div className="space-y-2">
            <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium">
              Про мене
            </label>
            <textarea
              {...register('bio')}
              placeholder="Розкажіть трохи про себе..."
              rows={2}
              className="w-full px-0 py-3 border-0 border-b-2 border-gray-200 focus:border-black bg-transparent text-black placeholder-gray-400 outline-none transition-colors resize-none"
            />
          </div>

          {/* Interests */}
          <div className="space-y-2">
            <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium">
              Інтереси
            </label>
            <div className="flex flex-wrap gap-2">
              {INTERESTS.map((interest) => (
                <button
                  key={interest}
                  type="button"
                  onClick={() =>
                    toggleSelection(
                      interest,
                      selectedInterests,
                      setSelectedInterests
                    )
                  }
                  className={`px-3 py-2 border-2 text-sm font-medium transition-colors ${
                    selectedInterests.includes(interest)
                      ? 'border-black bg-black text-white'
                      : 'border-gray-300 text-black hover:border-black'
                  }`}
                >
                  {interest}
                </button>
              ))}
            </div>
          </div>

          {/* Looking For */}
          <div className="space-y-2">
            <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium">
              Шукаю
            </label>
            <div className="flex flex-wrap gap-2">
              {LOOKING_FOR.map((item) => (
                <button
                  key={item}
                  type="button"
                  onClick={() =>
                    toggleSelection(
                      item,
                      selectedLookingFor,
                      setSelectedLookingFor
                    )
                  }
                  className={`px-3 py-2 border-2 text-sm font-medium transition-colors ${
                    selectedLookingFor.includes(item)
                      ? 'border-black bg-black text-white'
                      : 'border-gray-300 text-black hover:border-black'
                  }`}
                >
                  {item}
                </button>
              ))}
            </div>
          </div>

          {/* Submit */}
          <button
            type="submit"
            onClick={handleSubmit(onSubmit)}
            disabled={isSubmitting}
            className="w-full py-4 bg-black text-white font-medium tracking-wide transition-all hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting
              ? 'Зберігаємо...'
              : isEditMode
              ? 'Оновити профіль'
              : 'Створити профіль'}
          </button>
        </div>
      </div>
    </div>
  )
}
