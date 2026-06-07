import { useState, type ReactNode } from 'react'
import { BrowserRouter, Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import {
  ActionIcon,
  AppShell,
  Avatar,
  Box,
  Burger,
  Button,
  Container,
  Group,
  Menu,
  NavLink,
  Paper,
  SimpleGrid,
  Stack,
  Text,
  ThemeIcon,
  Title,
  useMantineColorScheme,
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import {
  IconFileInvoice,
  IconFilePlus,
  IconFiles,
  IconLogout,
  IconMoon,
  IconSun,
  IconUsers,
} from '@tabler/icons-react'
import { SignUpForm } from './auth/SignUpForm'
import { LoginForm } from './auth/LoginForm'
import { clearToken, isAuthenticated } from './auth/useAuth'
import { Logo } from './components/Logo'
import { useDocumentTitle } from './hooks/useDocumentTitle'

function ProtectedRoute({ children }: { children: ReactNode }) {
  return isAuthenticated() ? <>{children}</> : <Navigate to="/login" replace />
}

const STATS = [
  { label: 'Documents', icon: IconFiles, color: 'violet' },
  { label: 'Invoices', icon: IconFileInvoice, color: 'indigo' },
  { label: 'Clients', icon: IconUsers, color: 'blue' },
]

function Dashboard() {
  useDocumentTitle('Dashboard')
  const navigate = useNavigate()
  const [opened, { toggle }] = useDisclosure()
  const { colorScheme, toggleColorScheme } = useMantineColorScheme()

  function logout() {
    clearToken()
    navigate('/login')
  }

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 220, breakpoint: 'sm', collapsed: { mobile: !opened } }}
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Group>
            <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
            <Logo size="sm" />
          </Group>
          <Group gap="xs">
            <ActionIcon
              variant="subtle"
              onClick={() => toggleColorScheme()}
              aria-label="Toggle color scheme"
              size="lg"
            >
              {colorScheme === 'dark' ? <IconSun size={18} /> : <IconMoon size={18} />}
            </ActionIcon>
            <Menu shadow="md" width={180}>
              <Menu.Target>
                <Avatar
                  size="sm"
                  color="violet"
                  style={{ cursor: 'pointer' }}
                  aria-label="User menu"
                >
                  A
                </Avatar>
              </Menu.Target>
              <Menu.Dropdown>
                <Menu.Item disabled>Profile</Menu.Item>
                <Menu.Divider />
                <Menu.Item
                  color="red"
                  leftSection={<IconLogout size={14} />}
                  onClick={logout}
                >
                  Log out
                </Menu.Item>
              </Menu.Dropdown>
            </Menu>
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="md">
        <Stack gap={4}>
          <NavLink
            label="Documents"
            leftSection={<IconFiles size={16} />}
            active
          />
          <NavLink
            label="Invoices"
            leftSection={<IconFileInvoice size={16} />}
            disabled
          />
          <NavLink
            label="Clients"
            leftSection={<IconUsers size={16} />}
            disabled
          />
        </Stack>
      </AppShell.Navbar>

      <AppShell.Main>
        <Container size="lg" py="xl">
          {/* Stats row */}
          <SimpleGrid cols={{ base: 1, sm: 3 }} mb="xl">
            {STATS.map(({ label, icon: Icon, color }) => (
              <Paper key={label} withBorder p="md" radius="md">
                <Group justify="space-between" mb="xs">
                  <Text size="sm" c="dimmed" fw={500}>{label}</Text>
                  <ThemeIcon size={36} radius="md" color={color} variant="light">
                    <Icon size={20} />
                  </ThemeIcon>
                </Group>
                <Title order={2} fw={700}>0</Title>
              </Paper>
            ))}
          </SimpleGrid>

          {/* Empty state */}
          <Paper withBorder p="xl" radius="md">
            <Stack align="center" py="xl" gap="md">
              <ThemeIcon size={64} radius="xl" color="violet" variant="light">
                <IconFilePlus size={32} />
              </ThemeIcon>
              <Title order={3} ta="center">No documents yet</Title>
              <Text c="dimmed" size="sm" ta="center" maw={360}>
                Upload your first document to get started. You can organise, tag, and search all your company files from here.
              </Text>
              <Button leftSection={<IconFilePlus size={16} />} disabled mt="xs">
                Upload document
              </Button>
            </Stack>
          </Paper>
        </Container>
      </AppShell.Main>
    </AppShell>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/register" element={<SignUpForm />} />
        <Route path="/login" element={<LoginForm />} />
        <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
