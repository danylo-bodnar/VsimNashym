import type { Dispatch, SetStateAction } from 'react'
import MapIcon from '@mui/icons-material/Map'
import AccountCircleIcon from '@mui/icons-material/AccountCircle'

interface BottomNavProps {
  currentTab: 'map' | 'profile'
  setCurrentTab: Dispatch<SetStateAction<'map' | 'profile'>>
  disableMap?: boolean
}

export default function BottomNav({
  currentTab,
  setCurrentTab,
  disableMap,
}: BottomNavProps & { className?: string }) {
  return (
    <div className="flex justify-around p-2 bg-white border-t border-gray-300">
      <button
        onClick={() => !disableMap && setCurrentTab('map')}
        className={`flex flex-col flex-1 items-center text-sm ${
          currentTab === 'map' ? 'text-black-600' : 'text-gray-600'
        }`}
      >
        <MapIcon fontSize="medium" />
        <span>Map</span>
      </button>

      <button
        onClick={() => setCurrentTab('profile')}
        className={`flex flex-col flex-1 items-center text-sm ${
          currentTab === 'profile' ? 'text-black-600' : 'text-gray-600'
        }`}
      >
        <AccountCircleIcon fontSize="medium" />
        <span>Profile</span>
      </button>
    </div>
  )
}
