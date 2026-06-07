import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

interface FormErrors {
  email?: string
  password?: string
  server?: string
}

export function LoginForm() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<FormErrors>({})
  const [isSubmitting, setIsSubmitting] = useState(false)

  function validate(): FormErrors {
    const errs: FormErrors = {}
    if (!email.trim()) errs.email = 'Email is required.'
    if (!password.trim()) errs.password = 'Password is required.'
    return errs
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length > 0) {
      setErrors(errs)
      return
    }

    setIsSubmitting(true)
    setErrors({})

    try {
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })

      if (res.ok) {
        const { token } = await res.json() as { token: string }
        localStorage.setItem('token', token)
        navigate('/dashboard')
      } else if (res.status === 401) {
        setErrors({ server: 'Invalid email or password.' })
      } else {
        setErrors({ server: 'Login failed. Please try again.' })
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      <div>
        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={e => setEmail(e.target.value)}
        />
        {errors.email && <span>{errors.email}</span>}
      </div>

      <div>
        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
        />
        {errors.password && <span>{errors.password}</span>}
      </div>

      {errors.server && <span>{errors.server}</span>}

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Logging in…' : 'Log in'}
      </button>
    </form>
  )
}
