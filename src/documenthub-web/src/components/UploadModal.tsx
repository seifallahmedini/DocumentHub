import { useState } from 'react'
import {
  Button,
  Group,
  Modal,
  Progress,
  Stack,
  TagsInput,
  Text,
  TextInput,
} from '@mantine/core'
import { Dropzone } from '@mantine/dropzone'
import '@mantine/dropzone/styles.css'
import { notifications } from '@mantine/notifications'
import { IconCloudUpload, IconFile, IconX } from '@tabler/icons-react'
import { uploadDocument } from '../api/documents'

interface UploadModalProps {
  opened: boolean
  onClose: () => void
  onUploaded: () => void
}

export function UploadModal({ opened, onClose, onUploaded }: UploadModalProps) {
  const [file, setFile] = useState<File | null>(null)
  const [name, setName] = useState('')
  const [tags, setTags] = useState<string[]>([])
  const [progress, setProgress] = useState(0)
  const [uploading, setUploading] = useState(false)

  function reset() {
    setFile(null)
    setName('')
    setTags([])
    setProgress(0)
    setUploading(false)
  }

  function handleClose() {
    reset()
    onClose()
  }

  function handleDrop(files: File[]) {
    const f = files[0]
    setFile(f)
    if (!name) setName(f.name.replace(/\.[^.]+$/, ''))
  }

  async function handleUpload() {
    if (!file || !name.trim()) return
    setUploading(true)
    try {
      await uploadDocument(file, name.trim(), tags, setProgress)
      notifications.show({ color: 'green', message: `"${name}" uploaded successfully.` })
      onUploaded()
      handleClose()
    } catch {
      notifications.show({ color: 'red', message: 'Upload failed. Please try again.' })
      setUploading(false)
      setProgress(0)
    }
  }

  return (
    <Modal opened={opened} onClose={handleClose} title="Upload document" size="md" centered>
      <Stack>
        {!file ? (
          <Dropzone onDrop={handleDrop} maxFiles={1}>
            <Group justify="center" gap="xl" mih={120} style={{ pointerEvents: 'none' }}>
              <Dropzone.Accept><IconCloudUpload size={48} stroke={1.5} color="var(--mantine-color-blue-6)" /></Dropzone.Accept>
              <Dropzone.Reject><IconX size={48} stroke={1.5} color="var(--mantine-color-red-6)" /></Dropzone.Reject>
              <Dropzone.Idle><IconCloudUpload size={48} stroke={1.5} color="var(--mantine-color-dimmed)" /></Dropzone.Idle>
              <Stack gap={4} align="center">
                <Text size="md" fw={500}>Drag a file here or click to browse</Text>
                <Text size="xs" c="dimmed">Any file type accepted</Text>
              </Stack>
            </Group>
          </Dropzone>
        ) : (
          <Group p="md" style={{ border: '1px solid var(--mantine-color-default-border)', borderRadius: 8 }}>
            <IconFile size={32} stroke={1.5} />
            <Stack gap={2} style={{ flex: 1, overflow: 'hidden' }}>
              <Text size="sm" fw={500} truncate>{file.name}</Text>
              <Text size="xs" c="dimmed">{(file.size / 1024).toFixed(1)} KB</Text>
            </Stack>
            {!uploading && (
              <Button variant="subtle" color="red" size="xs" onClick={() => setFile(null)}>Remove</Button>
            )}
          </Group>
        )}

        <TextInput
          label="Document name"
          value={name}
          onChange={e => setName(e.target.value)}
          placeholder="e.g. Q1 Invoice 2025"
          required
          disabled={uploading}
        />

        <TagsInput
          label="Tags"
          placeholder="Type a tag and press Enter"
          value={tags}
          onChange={setTags}
          disabled={uploading}
          description="Press Enter or comma to add a tag"
          splitChars={[',', ' ']}
        />

        {uploading && (
          <Stack gap={4}>
            <Text size="xs" c="dimmed">Uploading… {progress}%</Text>
            <Progress value={progress} animated />
          </Stack>
        )}

        <Group justify="flex-end" mt="xs">
          <Button variant="default" onClick={handleClose} disabled={uploading}>Cancel</Button>
          <Button
            onClick={handleUpload}
            loading={uploading}
            disabled={!file || !name.trim()}
            leftSection={<IconCloudUpload size={16} />}
          >
            Upload
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}
