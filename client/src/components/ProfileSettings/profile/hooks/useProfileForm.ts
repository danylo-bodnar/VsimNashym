import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import type { User, RegisterUserDto } from '@/types/user'
import type { PhotoMeta } from '../constants'

export function useProfileForm(existingUser: User | null) {
  const isEditMode = !!existingUser

  // Avatar state
  const [avatarUrl, setAvatarUrl] = useState<string | null>(
    existingUser?.avatar.url || null
  )
  const [avatarFile, setAvatarFile] = useState<File | null>(null)
  const [initialAvatarUrl] = useState<string | null>(
    existingUser?.avatar.url || null
  )

  // Photos state - Initialize with proper PhotoMeta structure
  const [photos, setPhotos] = useState<PhotoMeta[]>([
    { url: null, file: null, messageId: null },
    { url: null, file: null, messageId: null },
    { url: null, file: null, messageId: null },
  ])
  const [initialPhotos, setInitialPhotos] = useState<PhotoMeta[]>([
    { url: null, file: null, messageId: null },
    { url: null, file: null, messageId: null },
    { url: null, file: null, messageId: null },
  ])

  // Multi-select states
  const [selectedInterests, setSelectedInterests] = useState<string[]>([])
  const [selectedLookingFor, setSelectedLookingFor] = useState<string[]>([])
  const [selectedLanguages, setSelectedLanguages] = useState<string[]>([])

  const form = useForm<RegisterUserDto>({
    defaultValues: {
      displayName: existingUser?.displayName || '',
      age: existingUser?.age || 18,
      bio: existingUser?.bio || '',
    },
  })

  useEffect(() => {
    if (existingUser) {
      // Load existing photos
      const loadedPhotos: PhotoMeta[] = existingUser.profilePhotos.map(
        (photo) => ({
          url: photo.url,
          messageId: photo.messageId || null,
          file: null,
        })
      )

      // Pad with empty slots
      while (loadedPhotos.length < 3) {
        loadedPhotos.push({ url: null, file: null, messageId: null })
      }

      setPhotos(loadedPhotos)
      setInitialPhotos(JSON.parse(JSON.stringify(loadedPhotos)))

      // Load selections
      setSelectedInterests(existingUser.interests || [])
      setSelectedLookingFor(existingUser.lookingFor || [])
      setSelectedLanguages(existingUser.languages || [])
    }
  }, [existingUser])

  const handleAvatarChange = (file: File, croppedUrl: string) => {
    setAvatarFile(file)
    setAvatarUrl(croppedUrl)
  }

  const handleRemoveAvatar = () => {
    setAvatarFile(null)
    setAvatarUrl(null)
  }

  const handlePhotoChange = (index: number, file: File | null) => {
    if (file) {
      const url = URL.createObjectURL(file)
      const newPhotos = [...photos]
      newPhotos[index] = { file, url, messageId: null }
      setPhotos(newPhotos)
    }
  }

  const removePhoto = (index: number) => {
    const newPhotos = [...photos]
    if (newPhotos[index]?.url && newPhotos[index]?.file) {
      URL.revokeObjectURL(newPhotos[index].url!)
    }
    newPhotos[index] = { url: null, file: null, messageId: null }
    setPhotos(newPhotos)
  }

  const toggleSelection = (
    item: string,
    selected: string[],
    setSelected: (items: string[]) => void
  ) => {
    if (selected.includes(item)) {
      setSelected(selected.filter((i) => i !== item))
    } else {
      setSelected([...selected, item])
    }
  }

  const hasChanges = (): boolean => {
    if (!isEditMode) return true

    const formValues = form.getValues()

    // Check avatar changes
    if (avatarFile !== null) return true
    if (avatarUrl !== initialAvatarUrl) return true

    // Check form field changes
    if (formValues.displayName !== existingUser?.displayName) return true
    if (formValues.age !== existingUser?.age) return true
    if (formValues.bio !== existingUser?.bio) return true

    // Check photos changes
    const photosChanged =
      JSON.stringify(photos) !== JSON.stringify(initialPhotos)
    if (photosChanged) return true

    // Check multi-selects
    if (
      JSON.stringify(selectedInterests.sort()) !==
      JSON.stringify((existingUser?.interests || []).sort())
    )
      return true
    if (
      JSON.stringify(selectedLookingFor.sort()) !==
      JSON.stringify((existingUser?.lookingFor || []).sort())
    )
      return true
    if (
      JSON.stringify(selectedLanguages.sort()) !==
      JSON.stringify((existingUser?.languages || []).sort())
    )
      return true

    return false
  }

  return {
    form,
    avatarUrl,
    avatarFile,
    handleAvatarChange,
    handleRemoveAvatar,
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
  }
}
