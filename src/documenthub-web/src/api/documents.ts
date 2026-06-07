import { getToken } from '../auth/useAuth'
import { apiUrl } from '../config'

export interface DocumentMeta {
  id: string
  name: string
  tags: string[]
  mimeType: string
  fileSize: number
  uploadedAt: string
}

export async function uploadDocument(
  file: File,
  name: string,
  tags: string[],
  onProgress?: (pct: number) => void
): Promise<string> {
  const token = getToken()
  const form = new FormData()
  form.append('file', file)
  form.append('name', name)
  if (tags.length > 0) form.append('tags', tags.join(','))

  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest()
    xhr.open('POST', apiUrl('/documents'))
    if (token) xhr.setRequestHeader('Authorization', `Bearer ${token}`)

    xhr.upload.onprogress = e => {
      if (e.lengthComputable && onProgress) onProgress(Math.round((e.loaded / e.total) * 100))
    }

    xhr.onload = () => {
      if (xhr.status === 201) {
        const body = JSON.parse(xhr.responseText)
        resolve(body.id)
      } else {
        reject(new Error(`Upload failed: ${xhr.status}`))
      }
    }

    xhr.onerror = () => reject(new Error('Network error'))
    xhr.send(form)
  })
}

export async function deleteDocument(id: string): Promise<void> {
  const token = getToken()
  await fetch(apiUrl(`/documents/${id}`), {
    method: 'DELETE',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })
}

export async function updateDocument(id: string, name: string, tags: string[]): Promise<void> {
  const token = getToken()
  await fetch(apiUrl(`/documents/${id}`), {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify({ name, tags }),
  })
}
