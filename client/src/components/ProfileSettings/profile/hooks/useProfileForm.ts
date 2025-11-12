import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import type { User, RegisterUserDto } from '@/types/user'
import type { PhotoMeta } from '../constants'

export function useProfileForm(existingUser: User | null) {
  const isEditMode = !!existingUser

  const [avatar, setAvatar] = useState<PhotoMeta>({
    url: null,
    file: null,
    messageId: null,
  })

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
      const mappedPhotos: PhotoMeta[] = [
        { url: null, file: null, messageId: null },
        { url: null, file: null, messageId: null },
        { url: null, file: null, messageId: null },
      ]
      existingUser.profilePhotos.forEach((photo) => {
        if (photo.slotIndex >= 0 && photo.slotIndex < 3) {
          mappedPhotos[photo.slotIndex] = {
            url: photo.url,
            file: null,
            messageId: photo.messageId,
          }
        }
      })

      setPhotos(mappedPhotos)
      setInitialPhotos(JSON.parse(JSON.stringify(mappedPhotos)))

      // Load avatar
      setAvatar({
        url: existingUser.avatar.url || null,
        file: null,
        messageId: existingUser.avatar.messageId || null,
      })

      // Load selections
      setSelectedInterests(existingUser.interests || [])
      setSelectedLookingFor(existingUser.lookingFor || [])
      setSelectedLanguages(existingUser.languages || [])
    }
  }, [existingUser])

  const handleAvatarChange = (file: File, croppedUrl: string) => {
    setAvatar({ url: croppedUrl, file: file, messageId: null })
  }

  const handleRemoveAvatar = () => {
    setAvatar({ url: null, file: null, messageId: null })
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
    setSelected: (items: string[]) => void,
  ) => {
    if (selected.includes(item)) {
      setSelected(selected.filter((i) => i !== item))
    } else {
      setSelected([...selected, item])
    }
  }

  const displayName = form.watch('displayName')
  const age = form.watch('age')
  const bio = form.watch('bio')

  const hasChanges = (): boolean => {
    if (!isEditMode) return true

    // Avatar
    if (
      avatar.messageId !== existingUser?.avatar?.messageId ||
      avatar.url !== existingUser?.avatar?.url
    )
      return true

    // Form fields
    if (displayName !== existingUser?.displayName) return true
    if (age !== existingUser?.age) return true
    if (bio !== existingUser?.bio) return true

    // Photos
    if (JSON.stringify(photos) !== JSON.stringify(initialPhotos)) return true

    // Multi-selects
    if (
      JSON.stringify([...selectedInterests].sort()) !==
      JSON.stringify([...(existingUser?.interests || [])].sort())
    )
      return true
    if (
      JSON.stringify([...selectedLookingFor].sort()) !==
      JSON.stringify([...(existingUser?.lookingFor || [])].sort())
    )
      return true
    if (
      JSON.stringify([...selectedLanguages].sort()) !==
      JSON.stringify([...(existingUser?.languages || [])].sort())
    )
      return true

    return false
  }

  return {
    form,
    avatar,
    setAvatar,
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
