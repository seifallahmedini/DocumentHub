import { clearToken, getToken } from '../auth/useAuth'
import { apiUrl } from '../config'

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const token = getToken()
  const headers = new Headers(init.headers)
  headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const res = await fetch(apiUrl(path), { ...init, headers })

  if (res.status === 401) {
    clearToken()
    window.location.href = '/login'
  }

  return res
}
