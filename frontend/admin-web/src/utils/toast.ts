import { createApp } from 'vue'
import Toast from '@/components/Toast.vue'

const MAX_TOASTS = 5
const activeToasts: ReturnType<typeof createApp>[] = []

function getContainer(): HTMLDivElement {
  let container = document.querySelector<HTMLDivElement>('.toast-container')
  if (!container) {
    container = document.createElement('div')
    container.className = 'toast-container'
    document.body.appendChild(container)
  }
  return container
}

function showToast(message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info', duration = 3000) {
  const container = getContainer()

  while (activeToasts.length >= MAX_TOASTS) {
    const oldest = activeToasts.shift()!
    oldest.unmount()
    ;(oldest as any)._el?.remove()
  }

  const el = document.createElement('div')
  container.appendChild(el)

  const app = createApp(Toast, {
    message,
    type,
    duration,
    onClose() {
      const idx = activeToasts.indexOf(app)
      if (idx !== -1) activeToasts.splice(idx, 1)
      app.unmount()
      el.remove()
    }
  })

  ;(app as any)._el = el
  app.mount(el)
  activeToasts.push(app)
}

const toast = {
  info: (message: string, duration?: number) => showToast(message, 'info', duration),
  success: (message: string, duration?: number) => showToast(message, 'success', duration),
  warning: (message: string, duration?: number) => showToast(message, 'warning', duration),
  error: (message: string, duration?: number) => showToast(message, 'error', duration)
}

export { showToast }
export default toast
