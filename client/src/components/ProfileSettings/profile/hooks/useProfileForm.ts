import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import type { User, RegisterUserDto } from '@/types/user'
import type { PhotoMeta } from '../constants'

export function useProfileForm(existingUser: User | null) {
  const form = useForm<RegisterUserDto>({
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
      { url: null, file: null },
      { url: null, file: null },
      { url: null, file: null },
    ]
  )

  const [initialPhotos, setInitialPhotos] = useState<PhotoMeta[]>(photos)

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

  // Sync state when existingUser changes
  useEffect(() => {
    if (existingUser) {
      form.reset({
        displayName: existingUser.displayName,
        age: existingUser.age,
        bio: existingUser.bio || '',
        interests: existingUser.interests,
        lookingFor: existingUser.lookingFor,
        languages: existingUser.languages,
      })

      const existingPhotos = [
        existingUser?.profilePhotos?.[0]
          ? {
              url: existingUser.profilePhotos[0].url,
              messageId: existingUser.profilePhotos[0].messageId,
            }
          : { url: null, file: null },
        existingUser?.profilePhotos?.[1]
          ? {
              url: existingUser.profilePhotos[1].url,
              messageId: existingUser.profilePhotos[1].messageId,
            }
          : { url: null, file: null },
        existingUser?.profilePhotos?.[2]
          ? {
              url: existingUser.profilePhotos[2].url,
              messageId: existingUser.profilePhotos[2].messageId,
            }
          : { url: null, file: null },
      ]

      setPhotos(existingPhotos)
      setInitialPhotos(existingPhotos)
      setSelectedInterests(existingUser.interests || [])
      setSelectedLookingFor(existingUser.lookingFor || [])
      setSelectedLanguages(existingUser.languages || [])
    }
  }, [existingUser, form])

  const handlePhotoChange = (index: number, file: File | null) => {
    const newPhotos = [...photos]
    if (file) {
      newPhotos[index] = {
        url: URL.createObjectURL(file),
        file,
        messageId: null,
      }
    } else {
      newPhotos[index] = { url: null, file: null, messageId: null }
    }
    setPhotos(newPhotos)
  }

  const removePhoto = (index: number) => {
    const newPhotos = [...photos]
    newPhotos[index] = { url: null, file: null, messageId: null }
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

  const hasChanges = () => {
    if (!isEditMode) return true

    const formValues = form.watch()
    const formChanged =
      formValues.displayName !== existingUser?.displayName ||
      formValues.age !== existingUser?.age ||
      formValues.bio !== (existingUser?.bio || '')

    const interestsChanged =
      JSON.stringify(selectedInterests.sort()) !==
      JSON.stringify((existingUser?.interests || []).sort())

    const lookingForChanged =
      JSON.stringify(selectedLookingFor.sort()) !==
      JSON.stringify((existingUser?.lookingFor || []).sort())

    const languagesChanged =
      JSON.stringify(selectedLanguages.sort()) !==
      JSON.stringify((existingUser?.languages || []).sort())

    const photosChanged =
      JSON.stringify(photos) !== JSON.stringify(initialPhotos)

    return (
      formChanged ||
      interestsChanged ||
      lookingForChanged ||
      languagesChanged ||
      photosChanged
    )
  }

  return {
    form,
    photos,
    setPhotos,
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
