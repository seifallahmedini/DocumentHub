export const config = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? '',
} as const

export function apiUrl(path: string): string {
  return `${config.apiBaseUrl}${path}`
}
