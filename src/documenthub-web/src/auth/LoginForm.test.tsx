import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { vi, describe, it, expect, beforeEach } from 'vitest'
import { LoginForm } from './LoginForm'

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

beforeEach(() => {
  mockNavigate.mockReset()
  localStorage.clear()
  vi.restoreAllMocks()
})

describe('LoginForm', () => {
  it('shows validation errors when fields are empty', async () => {
    render(<LoginForm />, { wrapper: MemoryRouter })

    fireEvent.click(screen.getByRole('button', { name: /log in/i }))

    expect(await screen.findByText(/email is required/i)).toBeInTheDocument()
    expect(screen.getByText(/password is required/i)).toBeInTheDocument()
  })

  it('stores token and redirects to dashboard on success', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
      new Response(JSON.stringify({ token: 'jwt-token-123' }), { status: 200 })
    )

    render(<LoginForm />, { wrapper: MemoryRouter })

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'admin@titancore.tn' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'SecurePass123!' } })
    fireEvent.click(screen.getByRole('button', { name: /log in/i }))

    await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith('/dashboard'))
    expect(localStorage.getItem('token')).toBe('jwt-token-123')
  })

  it('shows error message on invalid credentials (401)', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
      new Response(null, { status: 401 })
    )

    render(<LoginForm />, { wrapper: MemoryRouter })

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'admin@titancore.tn' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'wrong' } })
    fireEvent.click(screen.getByRole('button', { name: /log in/i }))

    expect(await screen.findByText(/invalid email or password/i)).toBeInTheDocument()
  })
})
