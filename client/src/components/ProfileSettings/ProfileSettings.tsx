// components/ProfileSettings.tsx (Refactored)
import { useState } from 'react'
import type { User, LocationPoint, RegisterUserDto } from '@/types/user'
import { submitUser } from '@/features/users/api'
import { telegramLogin } from '@/features/auth/api'
import imageCompression from 'browser-image-compression'
import { useProfileForm } from './profile/hooks/useProfileForm'
import PhotoUploader from './profile/PhotoUploader'
import MultiSelectButtons from './profile/MultiSelectButtons'
import { INTERESTS, LOOKING_FOR, LANGUAGES } from './profile/constants'

type ProfileSettingsProps = {
  existingUser: User | null
  telegramId: number
  onRegister?: (userData: User, jwt: string | null) => void
}

export default function ProfileSettings({
  existingUser,
  telegramId,
  onRegister,
}: ProfileSettingsProps) {
  const {
    form,
    photos,
    setInitialPhotos,
    selectedInterests,
    setSelectedInterests,
    selectedLookingFor,
    setSelectedLookingFor,
    selectedLanguages,
    setSelectedLanguages,
    isEditMode,
    handlePhotoChange,
    removePhoto,
    toggleSelection,
    hasChanges,
  } = useProfileForm(existingUser)

  const [isSubmitting, setIsSubmitting] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = form

  const handleGetLocation = (): Promise<LocationPoint> => {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        return reject(new Error('Geolocation is not supported by your browser'))
      }

      navigator.geolocation.getCurrentPosition(
        (pos) => {
          resolve({
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
          })
        },
        (err) => {
          console.error('Geo error', err)
          reject(err)
        }
      )
    })
  }

  const onSubmit = async (data: RegisterUserDto) => {
    let userLocation: LocationPoint

    try {
      userLocation = await handleGetLocation()
    } catch (err) {
      console.error('Failed to get location', err)
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

      // Compress photos
      const compressionOptions = {
        maxSizeMB: 0.6,
        maxWidthOrHeight: 512,
        useWebWorker: true,
      }

      const compressedPhotos = await Promise.all(
        photos.map(async (photo) => {
          if (!photo?.file) return null
          return await imageCompression(photo.file, compressionOptions)
        })
      )

      compressedPhotos.forEach((file) => {
        if (file) formData.append('profilePhotos', file)
      })

      photos.forEach((photo) => {
        if (photo?.messageId) {
          formData.append('existingPhotoMessageIds', photo.messageId)
        }
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
        setInitialPhotos([...photos])
        onRegister?.(newUser, null)
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

  const isButtonDisabled = isSubmitting || (isEditMode && !hasChanges())

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

          {/* Photo Uploader */}
          <PhotoUploader
            photos={photos}
            onPhotoChange={handlePhotoChange}
            onRemovePhoto={removePhoto}
            showValidation={!isEditMode}
          />

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
          <MultiSelectButtons
            label="Мови, якими я розмовляю"
            options={LANGUAGES}
            selected={selectedLanguages}
            onToggle={(lang) =>
              toggleSelection(lang, selectedLanguages, setSelectedLanguages)
            }
          />

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
          <MultiSelectButtons
            label="Інтереси"
            options={INTERESTS}
            selected={selectedInterests}
            onToggle={(interest) =>
              toggleSelection(interest, selectedInterests, setSelectedInterests)
            }
          />

          {/* Looking For */}
          <MultiSelectButtons
            label="Шукаю"
            options={LOOKING_FOR}
            selected={selectedLookingFor}
            onToggle={(item) =>
              toggleSelection(item, selectedLookingFor, setSelectedLookingFor)
            }
          />

          {/* Submit */}
          <button
            type="submit"
            onClick={handleSubmit(onSubmit)}
            disabled={isButtonDisabled}
            className="w-full py-4 bg-black text-white font-medium tracking-wide transition-all hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting
              ? 'Зберігаємо...'
              : isEditMode
              ? hasChanges()
                ? 'Оновити профіль'
                : 'Немає змін'
              : 'Створити профіль'}
          </button>
        </div>
      </div>
    </div>
  )
}
