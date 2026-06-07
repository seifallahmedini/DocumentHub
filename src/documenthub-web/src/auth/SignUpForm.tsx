import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Anchor, Button, PasswordInput, Stack, Text, TextInput } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { AuthLayout } from '../components/AuthLayout'
import { PasswordStrength } from './PasswordStrength'
import { useDocumentTitle } from '../hooks/useDocumentTitle'

interface FormErrors {
  companyName?: string
  email?: string
  password?: string
}

function validateField(field: keyof FormErrors, value: string): string | undefined {
  if (field === 'companyName') {
    return value.trim() ? undefined : 'Company name is required.'
  }
  if (field === 'email') {
    if (!value.trim()) return 'Email is required.'
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())) return 'Please enter a valid email address.'
  }
  if (field === 'password') {
    if (!value.trim()) return 'Password is required.'
    if (value.length < 8) return 'Password must be at least 8 characters.'
  }
}

export function SignUpForm() {
  useDocumentTitle('Sign up')
  const navigate = useNavigate()
  const [companyName, setCompanyName] = useState('')
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
    if (field === 'companyName') setCompanyName(value)
    if (field === 'email') setEmail(value)
    if (field === 'password') setPassword(value)
  }

  function validateAll(): FormErrors {
    return {
      companyName: validateField('companyName', companyName),
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
      const res = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ companyName, email: email.trim().toLowerCase(), password }),
      })

      if (res.ok) {
        const { token } = await res.json() as { token: string }
        localStorage.setItem('token', token)
        notifications.show({
          title: 'Welcome to DocumentHub!',
          message: 'Your account has been created.',
          color: 'green',
          autoClose: 3000,
        })
        navigate('/dashboard')
      } else if (res.status === 409) {
        setFieldError('email', 'A user with this email already exists.')
      } else {
        notifications.show({ title: 'Error', message: 'Registration failed. Please try again.', color: 'red' })
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout title="Create your account" subtitle="Start your free trial — no credit card required.">
      <form onSubmit={handleSubmit} noValidate>
        <Stack>
          <TextInput
            label="Company Name"
            value={companyName}
            onChange={e => handleChange('companyName', e.target.value)}
            onBlur={() => handleBlur('companyName', companyName)}
            error={errors.companyName}
            autoComplete="organization"
            required
            autoFocus
          />
          <TextInput
            label="Email"
            type="email"
            value={email}
            onChange={e => handleChange('email', e.target.value)}
            onBlur={() => handleBlur('email', email)}
            error={errors.email}
            autoComplete="email"
            required
          />
          <Stack gap="xs">
            <PasswordInput
              label="Password"
              value={password}
              onChange={e => handleChange('password', e.target.value)}
              onBlur={() => handleBlur('password', password)}
              error={errors.password}
              autoComplete="new-password"
              required
            />
            <PasswordStrength password={password} />
          </Stack>
          <Button type="submit" loading={isSubmitting} fullWidth mt="xs">
            {isSubmitting ? 'Signing up…' : 'Sign up'}
          </Button>
        </Stack>
      </form>

      <Text size="sm" ta="center" mt="md">
        Already have an account?{' '}
        <Anchor href="/login">Log in</Anchor>
      </Text>
    </AuthLayout>
  )
}
