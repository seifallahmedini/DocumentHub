export function getToken(): string | null {
  return localStorage.getItem('token')
}

export function setToken(token: string): void {
  localStorage.setItem('token', token)
}

export function clearToken(): void {
  localStorage.removeItem('token')
}

export function isAuthenticated(): boolean {
  return getToken() !== null
}

export interface TokenClaims {
  userId: string
  tenantId: string
  role: string
}

export function parseClaims(): TokenClaims | null {
  const token = getToken()
  if (!token) return null
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return {
      userId: payload.userId,
      tenantId: payload.tenantId,
      role: payload.role,
    }
  } catch {
    return null
  }
}
