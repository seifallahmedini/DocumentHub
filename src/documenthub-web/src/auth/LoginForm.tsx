import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Anchor, Button, PasswordInput, Stack, Text, TextInput } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { AuthLayout } from '../components/AuthLayout'
import { useDocumentTitle } from '../hooks/useDocumentTitle'
import { apiUrl } from '../config'

interface FormErrors {
  email?: string
  password?: string
}

function validateField(field: keyof FormErrors, value: string): string | undefined {
  if (field === 'email') {
    if (!value.trim()) return 'Email is required.'
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())) return 'Please enter a valid email address.'
  }
  if (field === 'password') {
    return value.trim() ? undefined : 'Password is required.'
  }
}

export function LoginForm() {
  useDocumentTitle('Log in')
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<FormErrors>({})
  const [isSubmitting, setIsSubmitting] = useState(false)

  function setFieldError(field: keyof FormErrors, error: string | undefined) {
    setErrors(prev => ({ ...prev, [field]: error }))
  }

  function handleBlur(field: keyof FormErrors, value: string) {
    setFieldError(field, validateField(field, value))
  }

  function handleChange(field: keyof FormErrors, value: string) {
    if (errors[field]) setFieldError(field, undefined)
    if (field === 'email') setEmail(value)
    if (field === 'password') setPassword(value)
  }

  function validateAll(): FormErrors {
    return {
      email: validateField('email', email),
      password: validateField('password', password),
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const errs = validateAll()
    if (Object.values(errs).some(Boolean)) {
      setErrors(errs)
      return
    }

    setIsSubmitting(true)
    try {
      const res = await fetch(apiUrl('/auth/login'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: email.trim().toLowerCase(), password }),
      })

      if (res.ok) {
        const { token } = await res.json() as { token: string }
        localStorage.setItem('token', token)
        navigate('/dashboard')
      } else if (res.status === 401) {
        setFieldError('email', 'Invalid email or password.')
      } else {
        notifications.show({ title: 'Error', message: 'Login failed. Please try again.', color: 'red' })
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout title="Welcome back" subtitle="Sign in to your DocumentHub account.">
      <form onSubmit={handleSubmit} noValidate>
        <Stack>
          <TextInput
            label="Email"
            type="email"
            value={email}
            onChange={e => handleChange('email', e.target.value)}
            onBlur={() => handleBlur('email', email)}
            error={errors.email}
            autoComplete="email"
            required
            autoFocus
          />
          <PasswordInput
            label="Password"
            value={password}
            onChange={e => handleChange('password', e.target.value)}
            onBlur={() => handleBlur('password', password)}
            error={errors.password}
            autoComplete="current-password"
            required
          />
          <Button type="submit" loading={isSubmitting} fullWidth mt="xs">
            {isSubmitting ? 'Logging in…' : 'Log in'}
          </Button>
        </Stack>
      </form>

      <Text size="sm" ta="center" mt="md">
        Don't have an account?{' '}
        <Anchor href="/register">Sign up</Anchor>
      </Text>
    </AuthLayout>
  )
}
