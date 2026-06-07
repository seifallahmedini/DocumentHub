import { useEffect, useState } from 'react'
import {
  ActionIcon,
  Badge,
  Button,
  Group,
  Modal,
  Paper,
  PasswordInput,
  Stack,
  Table,
  Text,
  TextInput,
  Title,
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { notifications } from '@mantine/notifications'
import { IconTrash, IconUserPlus } from '@tabler/icons-react'
import { apiFetch } from '../api/client'
import { parseClaims } from '../auth/useAuth'
import { useDocumentTitle } from '../hooks/useDocumentTitle'

interface Member {
  id: string
  email: string
  role: string
  createdAt: string
}

interface InviteErrors {
  email?: string
  password?: string
}

export function MembersPage() {
  useDocumentTitle('Members')
  const [members, setMembers] = useState<Member[]>([])
  const [loading, setLoading] = useState(true)
  const [inviteOpened, { open: openInvite, close: closeInvite }] = useDisclosure(false)
  const [inviteEmail, setInviteEmail] = useState('')
  const [invitePassword, setInvitePassword] = useState('')
  const [inviteErrors, setInviteErrors] = useState<InviteErrors>({})
  const [inviting, setInviting] = useState(false)

  const claims = parseClaims()
  const isAdmin = claims?.role === 'Admin'

  async function loadMembers() {
    setLoading(true)
    try {
      const res = await apiFetch('/users')
      if (res.ok) setMembers(await res.json())
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { loadMembers() }, [])

  function validateInvite(): boolean {
    const errors: InviteErrors = {}
    if (!inviteEmail.match(/^[^\s@]+@[^\s@]+\.[^\s@]+$/))
      errors.email = 'Please enter a valid email address.'
    if (invitePassword.length < 8)
      errors.password = 'Password must be at least 8 characters.'
    setInviteErrors(errors)
    return Object.keys(errors).length === 0
  }

  async function handleInvite() {
    if (!validateInvite()) return
    setInviting(true)
    try {
      const res = await apiFetch('/users/invite', {
        method: 'POST',
        body: JSON.stringify({ email: inviteEmail, password: invitePassword }),
      })
      if (res.ok) {
        notifications.show({ color: 'green', message: `${inviteEmail} has been invited.` })
        closeInvite()
        setInviteEmail('')
        setInvitePassword('')
        setInviteErrors({})
        loadMembers()
      } else if (res.status === 409) {
        setInviteErrors({ email: 'A user with this email already exists.' })
      } else {
        notifications.show({ color: 'red', message: 'Failed to invite member.' })
      }
    } finally {
      setInviting(false)
    }
  }

  async function handleRemove(id: string, email: string) {
    const res = await apiFetch(`/users/${id}`, { method: 'DELETE' })
    if (res.ok) {
      notifications.show({ color: 'green', message: `${email} has been removed.` })
      setMembers(m => m.filter(u => u.id !== id))
    } else {
      notifications.show({ color: 'red', message: 'Failed to remove member.' })
    }
  }

  return (
    <>
      <Group justify="space-between" mb="lg">
        <Title order={2}>Members</Title>
        {isAdmin && (
          <Button leftSection={<IconUserPlus size={16} />} onClick={openInvite}>
            Invite member
          </Button>
        )}
      </Group>

      <Paper withBorder radius="md">
        {loading ? (
          <Text p="xl" c="dimmed" ta="center">Loading…</Text>
        ) : members.length === 0 ? (
          <Text p="xl" c="dimmed" ta="center">No members yet.</Text>
        ) : (
          <Table striped highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Email</Table.Th>
                <Table.Th>Role</Table.Th>
                <Table.Th>Joined</Table.Th>
                {isAdmin && <Table.Th />}
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {members.map(member => (
                <Table.Tr key={member.id}>
                  <Table.Td>{member.email}</Table.Td>
                  <Table.Td>
                    <Badge color={member.role === 'Admin' ? 'violet' : 'gray'} variant="light">
                      {member.role}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    {new Date(member.createdAt).toLocaleDateString()}
                  </Table.Td>
                  {isAdmin && (
                    <Table.Td>
                      {member.id !== claims?.userId && (
                        <ActionIcon
                          color="red"
                          variant="subtle"
                          aria-label={`Remove ${member.email}`}
                          onClick={() => handleRemove(member.id, member.email)}
                        >
                          <IconTrash size={16} />
                        </ActionIcon>
                      )}
                    </Table.Td>
                  )}
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Paper>

      <Modal opened={inviteOpened} onClose={closeInvite} title="Invite member" centered>
        <Stack>
          <TextInput
            label="Email"
            placeholder="member@company.com"
            value={inviteEmail}
            onChange={e => { setInviteEmail(e.target.value); setInviteErrors(err => ({ ...err, email: undefined })) }}
            onBlur={() => {
              if (!inviteEmail.match(/^[^\s@]+@[^\s@]+\.[^\s@]+$/))
                setInviteErrors(err => ({ ...err, email: 'Please enter a valid email address.' }))
            }}
            error={inviteErrors.email}
            autoComplete="email"
            required
            autoFocus
          />
          <PasswordInput
            label="Temporary password"
            placeholder="Min. 8 characters"
            value={invitePassword}
            onChange={e => { setInvitePassword(e.target.value); setInviteErrors(err => ({ ...err, password: undefined })) }}
            onBlur={() => {
              if (invitePassword.length < 8)
                setInviteErrors(err => ({ ...err, password: 'Password must be at least 8 characters.' }))
            }}
            error={inviteErrors.password}
            autoComplete="new-password"
            required
          />
          <Button onClick={handleInvite} loading={inviting} mt="xs">
            Send invite
          </Button>
        </Stack>
      </Modal>
    </>
  )
}
