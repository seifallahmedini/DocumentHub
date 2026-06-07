import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert, Anchor, Button, Center, Paper, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core'
import { IconAlertCircle } from '@tabler/icons-react'

export function SignUpForm() {
  const navigate = useNavigate()
  const [companyName, setCompanyName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<{ companyName?: string; email?: string; password?: string }>({})
  const [serverError, setServerError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  function validate() {
    const errs: typeof errors = {}
    if (!companyName.trim()) errs.companyName = 'Company name is required.'
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
    setServerError('')

    try {
      const res = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ companyName, email, password }),
      })

      if (res.ok) {
        const { token } = await res.json() as { token: string }
        localStorage.setItem('token', token)
        navigate('/dashboard')
      } else if (res.status === 409) {
        setServerError('A user with this email already exists.')
      } else {
        setServerError('Registration failed. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Center mih="100svh">
      <Paper withBorder shadow="md" p="xl" w={420}>
        <Title order={2} mb="lg">Create your account</Title>

        {serverError && (
          <Alert icon={<IconAlertCircle size={16} />} color="red" mb="md">
            {serverError}
          </Alert>
        )}

        <form onSubmit={handleSubmit} noValidate>
          <Stack>
            <TextInput
              label="Company Name"
              value={companyName}
              onChange={e => setCompanyName(e.target.value)}
              error={errors.companyName}
            />
            <TextInput
              label="Email"
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              error={errors.email}
            />
            <PasswordInput
              label="Password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              error={errors.password}
            />
            <Button type="submit" loading={isSubmitting} fullWidth mt="sm">
              Sign up
            </Button>
          </Stack>
        </form>

        <Text size="sm" ta="center" mt="md">
          Already have an account?{' '}
          <Anchor href="/login">Log in</Anchor>
        </Text>
      </Paper>
    </Center>
  )
}
