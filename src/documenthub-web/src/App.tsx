import { BrowserRouter, Routes, Route, Navigate, useNavigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { AppShell, Button, Container, Group, Text, Title } from '@mantine/core'
import { SignUpForm } from './auth/SignUpForm'
import { LoginForm } from './auth/LoginForm'
import { clearToken, isAuthenticated } from './auth/useAuth'

function ProtectedRoute({ children }: { children: ReactNode }) {
  return isAuthenticated() ? <>{children}</> : <Navigate to="/login" replace />
}

function Dashboard() {
  const navigate = useNavigate()
  function logout() {
    clearToken()
    navigate('/login')
  }
  return (
    <AppShell header={{ height: 60 }}>
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Title order={4}>DocumentHub</Title>
          <Button variant="subtle" onClick={logout}>Log out</Button>
        </Group>
      </AppShell.Header>
      <AppShell.Main>
        <Container mt="xl">
          <Text>Welcome to DocumentHub.</Text>
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
