import type { PhotoMeta } from './constants'

type PhotoUploaderProps = {
  photos: PhotoMeta[]
  onPhotoChange: (index: number, file: File | null) => void
  onRemovePhoto: (index: number) => void
  showValidation?: boolean
}

export default function PhotoUploader({
  photos,
  onPhotoChange,
  onRemovePhoto,
  showValidation = false,
}: PhotoUploaderProps) {
  return (
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
                  onPhotoChange(index, e.target.files[0])
                }
              }}
            />
            <label
              htmlFor={`photo-${index}`}
              className="block h-full w-full cursor-pointer"
            >
              {photos[index]?.url ? (
                <div className="relative h-full w-full">
                  <img
                    src={photos[index].url!}
                    alt={`Фото ${index + 1}`}
                    className="h-full w-full object-cover border-2 border-black"
                  />
                  <button
                    type="button"
                    onClick={(e) => {
                      e.preventDefault()
                      onRemovePhoto(index)
                    }}
                    className="absolute top-0 right-0 text-white w-7 h-7 flex items-center justify-center text-xl leading-none shadow-lg hover:bg-red-600 transition-colors"
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
      {showValidation && !photos.some((p) => p?.url) && (
        <p className="text-gray-500 text-xs mt-2">Потрібне хоча б одне фото</p>
      )}
    </div>
  )
}
