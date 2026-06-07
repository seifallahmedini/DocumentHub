import { type ReactNode } from 'react'
import { ActionIcon, Box, Group, Stack, Text, ThemeIcon, Title, useMantineColorScheme } from '@mantine/core'
import { IconFileInvoice, IconFiles, IconMoon, IconSun, IconUsers } from '@tabler/icons-react'
import { Logo } from './Logo'

const FEATURES = [
  { icon: IconFiles, text: 'Upload and search company documents' },
  { icon: IconFileInvoice, text: 'Create and export invoices with TVA' },
  { icon: IconUsers, text: 'Manage clients in one place' },
]

function BrandPanel() {
  return (
    <Box
      visibleFrom="sm"
      style={{
        width: '44%',
        background: 'linear-gradient(145deg, #7c3aed 0%, #4f46e5 100%)',
        position: 'relative',
        overflow: 'hidden',
        display: 'flex',
        flexDirection: 'column',
        padding: '2.5rem',
      }}
    >
      {/* Dot-grid texture */}
      <Box
        style={{
          position: 'absolute',
          inset: 0,
          backgroundImage: 'radial-gradient(circle, rgba(255,255,255,0.12) 1px, transparent 1px)',
          backgroundSize: '22px 22px',
          pointerEvents: 'none',
        }}
      />

      <Box style={{ position: 'relative', flex: 1, display: 'flex', flexDirection: 'column' }}>
        <Logo size="md" variant="light" />

        <Box style={{ flex: 1, display: 'flex', flexDirection: 'column', justifyContent: 'center', paddingTop: '3rem' }}>
          <Title order={2} c="white" lh={1.3} mb="xl">
            Your documents.<br />Your clients.<br />One place.
          </Title>

          <Stack gap="md">
            {FEATURES.map(({ icon: Icon, text }) => (
              <Group key={text} align="center">
                <ThemeIcon size={32} radius="sm" style={{ background: 'rgba(255,255,255,0.15)', flexShrink: 0 }}>
                  <Icon size={16} color="white" />
                </ThemeIcon>
                <Text c="rgba(255,255,255,0.88)" size="sm">{text}</Text>
              </Group>
            ))}
          </Stack>
        </Box>

        <Text c="rgba(255,255,255,0.4)" size="xs" mt="xl">
          Built for TitanCore SUARL · Tunis, Tunisia
        </Text>
      </Box>
    </Box>
  )
}

interface AuthLayoutProps {
  children: ReactNode
  title: string
  subtitle?: string
}

export function AuthLayout({ children, title, subtitle }: AuthLayoutProps) {
  const { colorScheme, toggleColorScheme } = useMantineColorScheme()

  return (
    <Box style={{ display: 'flex', minHeight: '100svh' }}>
      <BrandPanel />

      {/* Form panel */}
      <Box
        style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          background: colorScheme === 'dark' ? 'var(--mantine-color-dark-7)' : 'var(--mantine-color-gray-0)',
        }}
      >
        {/* Top bar */}
        <Group justify="space-between" p="md">
          <Box hiddenFrom="sm">
            <Logo size="sm" />
          </Box>
          <Box visibleFrom="sm" style={{ opacity: 0, pointerEvents: 'none' }}>
            <Logo size="sm" />
          </Box>
          <ActionIcon
            variant="subtle"
            onClick={() => toggleColorScheme()}
            aria-label="Toggle color scheme"
            size="lg"
          >
            {colorScheme === 'dark' ? <IconSun size={18} /> : <IconMoon size={18} />}
          </ActionIcon>
        </Group>

        {/* Form content */}
        <Box
          style={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            padding: '2rem',
            animation: 'auth-fade-in 0.25s ease',
          }}
        >
          <Box style={{ width: '100%', maxWidth: 400 }}>
            <Title order={2} mb={subtitle ? 4 : 'lg'}>{title}</Title>
            {subtitle && (
              <Text c="dimmed" size="sm" mb="lg">{subtitle}</Text>
            )}
            {children}
          </Box>
        </Box>
      </Box>
    </Box>
  )
}
