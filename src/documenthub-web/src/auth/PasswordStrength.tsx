import { Box, Progress, Text } from '@mantine/core'

function getStrength(password: string): number {
  let s = 0
  if (password.length >= 8) s++
  if (/\d/.test(password)) s++
  if (/[!@#$%^&*(),.?":{}|<>_\-]/.test(password)) s++
  return s
}

const LEVELS = [
  { label: '', color: 'gray' },
  { label: 'Weak', color: 'red' },
  { label: 'Fair', color: 'yellow' },
  { label: 'Strong', color: 'green' },
]

export function PasswordStrength({ password }: { password: string }) {
  if (!password) return null
  const strength = getStrength(password)
  const { label, color } = LEVELS[strength]

  return (
    <Box mt={-4}>
      <Progress value={(strength / 3) * 100} color={color} size="xs" mb={4} />
      <Text size="xs" c={color}>{label}</Text>
    </Box>
  )
}
