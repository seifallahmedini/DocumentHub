import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { SignUpForm } from './SignUpForm'

function renderWithRouter(ui: React.ReactElement) {
  return render(
    <MemoryRouter initialEntries={['/register']}>
      <Routes>
        <Route path="/register" element={ui} />
        <Route path="/dashboard" element={<div>Dashboard</div>} />
      </Routes>
    </MemoryRouter>
  )
}

beforeEach(() => {
  vi.restoreAllMocks()
})

test('shows validation errors when submitting empty form', async () => {
  renderWithRouter(<SignUpForm />)

  await userEvent.click(screen.getByRole('button', { name: /sign up/i }))

  expect(screen.getByText(/company name is required/i)).toBeInTheDocument()
  expect(screen.getByText(/email is required/i)).toBeInTheDocument()
  expect(screen.getByText(/password is required/i)).toBeInTheDocument()
})

test('redirects to dashboard on successful registration', async () => {
  vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
    new Response(JSON.stringify({ token: 'fake-jwt' }), { status: 200 })
  )

  renderWithRouter(<SignUpForm />)

  await userEvent.type(screen.getByLabelText(/company name/i), 'TitanCore SUARL')
  await userEvent.type(screen.getByLabelText(/email/i), 'admin@titancore.tn')
  await userEvent.type(screen.getByLabelText(/password/i), 'SecurePass123!')
  await userEvent.click(screen.getByRole('button', { name: /sign up/i }))

  await waitFor(() => {
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
  })
})

test('shows error message when email is already taken', async () => {
  vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
    new Response(JSON.stringify({ error: 'A user with this email already exists.' }), { status: 409 })
  )

  renderWithRouter(<SignUpForm />)

  await userEvent.type(screen.getByLabelText(/company name/i), 'Acme Corp')
  await userEvent.type(screen.getByLabelText(/email/i), 'taken@example.com')
  await userEvent.type(screen.getByLabelText(/password/i), 'SecurePass123!')
  await userEvent.click(screen.getByRole('button', { name: /sign up/i }))

  await waitFor(() => {
    expect(screen.getByText(/email already exists/i)).toBeInTheDocument()
  })
})
