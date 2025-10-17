// components/profile/constants.ts
export const INTERESTS = [
  'Кава',
  'Спорт',
  'Музика',
  'Пиво',
  'Природа',
  'Мистецтво',
  'Ігри',
  'Книги',
] as const

export const LOOKING_FOR = [
  'Друзів',
  'Практика мови',
  'Відносини',
  'Інтеграційні поради',
  'Cпілкування',
] as const

export const LANGUAGES = [
  'Українська',
  'Norsk',
  'English',
  'Русский',
  'Polsk',
  'Deutsch',
] as const

export type PhotoMeta = {
  url: string | null
  file?: File | null
  messageId?: string | null
}
