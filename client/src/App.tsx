import { useEffect } from 'react'
import { useTelegram } from './hooks/useTelegram'

import './App.css'

function App() {
  const { tg, user, closeApp } = useTelegram()

  useEffect(() => {
    tg.ready()
    tg.expand()
  }, [tg])

  return (
    <>
      <div style={{ padding: 20 }}>
        <h1>Hello, {user?.first_name ?? 'Guest'} 👋</h1>
        <p>This is running inside Telegram!</p>

        <button onClick={closeApp}>Close Mini App</button>
      </div>
    </>
  )
}

export default App
