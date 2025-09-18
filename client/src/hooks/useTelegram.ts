import type { User } from '@/types/user'

declare global {
  interface Window {
    Telegram: {
      WebApp: {
        initDataUnsafe?: { user?: User }
        close: () => void
        expand: () => void
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        [key: string]: any
      }
    }
  }
}

export function useTelegram() {
  const tg = window.Telegram.WebApp

  return {
    tg,
    user: tg.initDataUnsafe?.user,
    closeApp: () => tg.close(),
    expand: () => tg.expand(),
  }
}
