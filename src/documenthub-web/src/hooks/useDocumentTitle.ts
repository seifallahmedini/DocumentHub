import { useEffect } from 'react'

export function useDocumentTitle(title: string) {
  useEffect(() => {
    document.title = `${title} — DocumentHub`
    return () => { document.title = 'DocumentHub' }
  }, [title])
}
