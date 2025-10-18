import { useState, useCallback } from 'react'
import Cropper from 'react-easy-crop'

type AvatarUploaderProps = {
  avatarUrl?: string
  onAvatarChange: (file: File, previewUrl: string) => void
  showValidation?: boolean
}

export default function AvatarUploader({
  avatarUrl,
  onAvatarChange,
  showValidation = false,
}: AvatarUploaderProps) {
  const [imageSrc, setImageSrc] = useState<string | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(avatarUrl || null)
  const [croppedAreaPixels, setCroppedAreaPixels] = useState<any>(null)
  const [crop, setCrop] = useState({ x: 0, y: 0 })
  const [zoom, setZoom] = useState(1)
  const [showCropper, setShowCropper] = useState(false)

  const onCropComplete = useCallback((_: any, croppedAreaPixels: any) => {
    setCroppedAreaPixels(croppedAreaPixels)
  }, [])

  async function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return

    const reader = new FileReader()
    reader.onload = () => {
      setImageSrc(reader.result as string)
      setShowCropper(true)
      setCrop({ x: 0, y: 0 })
      setZoom(1)
    }
    reader.readAsDataURL(file)
  }

  async function handleCropSave() {
    if (!imageSrc || !croppedAreaPixels) return

    const croppedFile = await getCroppedImage(imageSrc, croppedAreaPixels)
    const previewUrl = URL.createObjectURL(croppedFile)

    setPreviewUrl(previewUrl)
    setShowCropper(false)
    setImageSrc(null)

    onAvatarChange(croppedFile, previewUrl)
  }

  function handleCropCancel() {
    setShowCropper(false)
    setImageSrc(null)
    setCrop({ x: 0, y: 0 })
    setZoom(1)
  }

  return (
    <div>
      <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium mb-3">
        Аватар (фото профілю)
      </label>

      <div className="flex flex-col items-center gap-3">
        <div className="relative w-32 h-32 rounded-full overflow-hidden border-2 border-black bg-gray-100">
          {previewUrl ? (
            <img
              src={previewUrl}
              alt="Avatar preview"
              className="object-cover w-full h-full"
            />
          ) : (
            <div className="flex items-center justify-center h-full w-full">
              <svg
                className="w-16 h-16 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1.5}
                  d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                />
              </svg>
            </div>
          )}
        </div>

        <label className="bg-black hover:bg-gray-800 text-white px-4 py-2 text-sm font-medium transition-colors cursor-pointer">
          {previewUrl ? 'Змінити аватар' : 'Завантажити аватар'}
          <input
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handleFileSelect}
          />
        </label>

        {showValidation && !previewUrl && (
          <p className="text-gray-500 text-xs">Потрібен аватар</p>
        )}
      </div>

      {showCropper && imageSrc && (
        <div className="fixed inset-0 bg-black bg-opacity-90 flex flex-col items-center justify-center z-50 p-4">
          <div className="w-full max-w-md space-y-4">
            <div className="relative w-full h-80 bg-gray-900 rounded-lg overflow-hidden">
              <Cropper
                image={imageSrc}
                crop={crop}
                zoom={zoom}
                aspect={1}
                cropShape="round"
                showGrid={false}
                onCropChange={setCrop}
                onZoomChange={setZoom}
                onCropComplete={onCropComplete}
              />
            </div>

            <div className="space-y-3">
              <div>
                <label className="block text-white text-sm mb-2">Масштаб</label>
                <input
                  type="range"
                  min="1"
                  max="3"
                  step="0.1"
                  value={zoom}
                  onChange={(e) => setZoom(Number(e.target.value))}
                  className="w-full"
                />
              </div>

              <div className="flex gap-3">
                <button
                  onClick={handleCropSave}
                  className="flex-1 bg-blue-500 hover:bg-blue-600 text-white px-4 py-3 font-medium transition-colors rounded"
                >
                  Зберегти
                </button>
                <button
                  onClick={handleCropCancel}
                  className="flex-1 bg-gray-600 hover:bg-gray-700 text-white px-4 py-3 font-medium transition-colors rounded"
                >
                  Скасувати
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

/** Utility to crop the image to a File object */
async function getCroppedImage(imageSrc: string, crop: any): Promise<File> {
  const image = await createImage(imageSrc)
  const canvas = document.createElement('canvas')
  const ctx = canvas.getContext('2d')!

  const { width, height, x, y } = crop

  // Set canvas to desired output size (400x400 for avatars)
  const outputSize = 400
  canvas.width = outputSize
  canvas.height = outputSize

  // Draw the cropped portion scaled to output size
  ctx.drawImage(image, x, y, width, height, 0, 0, outputSize, outputSize)

  return new Promise((resolve) => {
    canvas.toBlob(
      (blob) => {
        if (blob) {
          resolve(new File([blob], 'avatar.jpg', { type: 'image/jpeg' }))
        }
      },
      'image/jpeg',
      0.9
    )
  })
}

function createImage(url: string): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const image = new Image()
    image.addEventListener('load', () => resolve(image))
    image.addEventListener('error', reject)
    image.src = url
  })
}
