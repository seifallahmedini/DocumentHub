import { Group, Text } from '@mantine/core'

interface LogoProps {
  size?: 'sm' | 'md' | 'lg'
  variant?: 'auto' | 'light' | 'dark'
}

const SIZES = { sm: 24, md: 32, lg: 48 }
const FONT_SIZES: Record<string, 'md' | 'lg' | 'xl'> = { sm: 'md', md: 'lg', lg: 'xl' }

export function Logo({ size = 'md', variant = 'auto' }: LogoProps) {
  const px = SIZES[size]
  const iconColor = variant === 'light' ? '#ffffff' : '#7c3aed'
  const docFill = variant === 'light' ? 'rgba(255,255,255,0.9)' : 'white'
  const lineFill = variant === 'light' ? 'rgba(255,255,255,0.5)' : 'rgba(124,58,237,0.45)'
  const textColor = variant === 'light' ? 'white' : undefined

  return (
    <Group gap="xs" align="center" wrap="nowrap">
      <svg width={px} height={px} viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
        <rect width="32" height="32" rx="8" fill={iconColor} />
        <path d="M8 6 L20 6 L26 12 L26 26 L8 26 Z" fill={docFill} />
        <path d="M20 6 L26 12 L20 12 Z" fill="rgba(0,0,0,0.15)" />
        <rect x="11" y="15" width="11" height="1.5" rx="0.75" fill={lineFill} />
        <rect x="11" y="19" width="8" height="1.5" rx="0.75" fill={lineFill} />
      </svg>
      <Text
        fw={700}
        size={FONT_SIZES[size]}
        style={{ color: textColor, letterSpacing: '-0.3px' }}
      >
        DocumentHub
      </Text>
    </Group>
  )
}
