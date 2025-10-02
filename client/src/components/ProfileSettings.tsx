import { useForm } from 'react-hook-form'
import { useState, useEffect } from 'react'
import apiClient from '@/utils/api-client'
import type { User, LocationPoint } from '@/types/user'

type RegisterUserForm = {
  displayName: string
  age: number
  bio?: string
  interests: string[]
  lookingFor: string[]
  language: string[]
}

type ProfileSettingsProps = {
  existingUser: User | null
  telegramId: number
}

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
  'Deutch',
]

export default function ProfileSettings({
  existingUser,
  telegramId,
}: ProfileSettingsProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<RegisterUserForm>({
    defaultValues: existingUser
      ? {
          displayName: existingUser.displayName,
          age: existingUser.age,
          bio: existingUser.bio || '',
          interests: existingUser.interests,
          lookingFor: existingUser.lookingFor,
          language: existingUser.languages,
        }
      : {
          interests: [],
          lookingFor: [],
          language: [],
        },
  })

  const [location, setLocation] = useState<LocationPoint | null>(
    existingUser
      ? {
          latitude: existingUser.location.latitude,
          longitude: existingUser.location.longitude,
        }
      : null
  )

  const [photos, setPhotos] = useState<(string | null)[]>([
    existingUser?.profilePhotos?.[0] || null,
    existingUser?.profilePhotos?.[1] || null,
    existingUser?.profilePhotos?.[2] || null,
  ])

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

  const handleGetLocation = () => {
    return new Promise<LocationPoint>((resolve, reject) => {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          const newLocation = {
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
          }
          setLocation(newLocation)
          resolve(newLocation)
        },
        (err) => {
          alert('Не вдалося отримати локацію: ' + err.message)
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
        language: existingUser.languages,
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
    if (file) {
      const newPhotos = [...photos]
      newPhotos[index] = URL.createObjectURL(file)
      setPhotos(newPhotos)

      const newPhotoFiles = [...photoFiles]
      newPhotoFiles[index] = file
      setPhotoFiles(newPhotoFiles)
    }
  }

  const removePhoto = (index: number) => {
    const newPhotos = [...photos]
    newPhotos[index] = null
    setPhotos(newPhotos)

    const newPhotoFiles = [...photoFiles]
    newPhotoFiles[index] = null
    setPhotoFiles(newPhotoFiles)
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

  const onSubmit = async (data: RegisterUserForm) => {
    if (!isEditMode && !photos.some((p) => p !== null)) {
      alert('Будь ласка, додайте хоча б одне фото')
      return
    }

    setIsSubmitting(true)

    try {
      const currentLocation = location || (await handleGetLocation())

      const formData = new FormData()
      formData.append('telegramId', telegramId.toString())
      formData.append('displayName', data.displayName)
      formData.append('age', data.age.toString())
      if (data.bio) formData.append('bio', data.bio)

      selectedInterests.forEach((interest) =>
        formData.append('interests', interest)
      )
      selectedLookingFor.forEach((item) => formData.append('lookingFor', item))
      selectedLanguages.forEach((lang) => formData.append('language', lang))

      photoFiles.forEach((file, index) => {
        if (file) {
          formData.append(`photo${index}`, file)
        }
      })

      if (currentLocation) {
        formData.append('lat', currentLocation.latitude.toString())
        formData.append('lng', currentLocation.longitude.toString())
      }

      if (isEditMode) {
        await apiClient.put(`/api/users/${existingUser.telegramId}`, formData)

        alert('Профіль успішно оновлено!')
      } else {
        await apiClient.post('/api/users', formData)

        alert('Профіль успішно створено!')
      }
    } catch (error) {
      console.error('Помилка збереження профілю:', error)

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
                          src={photos[index]!}
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
