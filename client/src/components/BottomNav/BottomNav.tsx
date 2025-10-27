import type { Dispatch, SetStateAction } from 'react'
import MapIcon from '@mui/icons-material/Map'
import AccountCircleIcon from '@mui/icons-material/AccountCircle'
import BlockIcon from '@mui/icons-material/Block' // 👈 added this

interface BottomNavProps {
  currentTab: 'map' | 'profile'
  setCurrentTab: Dispatch<SetStateAction<'map' | 'profile'>>
  disableMap?: boolean
}

export default function BottomNav({
  currentTab,
  setCurrentTab,
  disableMap = false,
}: BottomNavProps & { className?: string }) {
  return (
    <div className="flex justify-around p-2 bg-white border-t border-gray-300">
      {/* Map Button */}
      <button
        onClick={() => !disableMap && setCurrentTab('map')}
        disabled={disableMap}
        className={`flex flex-col flex-1 items-center text-sm transition relative
          ${
            disableMap
              ? 'text-gray-400 cursor-not-allowed opacity-60'
              : currentTab === 'map'
                ? 'text-black'
                : 'text-gray-600 hover:text-black'
          }
        `}
      >
        <div className="relative flex items-center justify-center">
          <MapIcon fontSize="medium" />
          {disableMap && (
            <BlockIcon
              fontSize="small"
              className="absolute -right-3 -top-1 text-gray-500"
            />
          )}
        </div>
        <span>Map</span>
      </button>

      {/* Profile Button */}
      <button
        onClick={() => setCurrentTab('profile')}
        className={`flex flex-col flex-1 items-center text-sm transition
          ${currentTab === 'profile' ? 'text-black' : 'text-gray-600 hover:text-black'}
        `}
      >
        <AccountCircleIcon fontSize="medium" />
        <span>Profile</span>
      </button>
    </div>
  )
}
