import { createApp } from 'vue'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'

/**
 * 显示确认对话框，替代原生 window.confirm()。
 * @param {string} message - 确认消息
 * @param {object} [opts] - 可选配置
 * @param {string} [opts.title='确认'] - 标题
 * @param {string} [opts.type='warning'] - 类型: warning / danger / info
 * @param {string} [opts.confirmText='确认'] - 确认按钮文字
 * @param {string} [opts.cancelText='取消'] - 取消按钮文字
 * @param {string} [opts.icon='⚠'] - 图标
 * @returns {Promise<boolean>}
 */
export function confirm(message, opts = {}) {
  return new Promise(resolve => {
    const el = document.createElement('div')
    document.body.appendChild(el)

    const app = createApp(ConfirmDialog, {
      message,
      title: opts.title || '确认',
      type: opts.type || 'warning',
      confirmText: opts.confirmText || '确认',
      cancelText: opts.cancelText || '取消',
      icon: opts.icon || '⚠',
      onConfirm() { cleanup(); resolve(true) },
      onCancel() { cleanup(); resolve(false) }
    })

    function cleanup() {
      app.unmount()
      el.remove()
    }

    app.mount(el)
  })
}

/**
 * 显示提示对话框，替代原生 window.alert()。
 * @param {string} message - 提示消息
 * @param {object} [opts] - 可选配置
 * @returns {Promise<void>}
 */
export function alert(message, opts = {}) {
  return new Promise(resolve => {
    const el = document.createElement('div')
    document.body.appendChild(el)

    const app = createApp(ConfirmDialog, {
      message,
      title: opts.title || '提示',
      type: opts.type || 'info',
      confirmText: opts.confirmText || '知道了',
      cancelText: '',
      icon: opts.icon || 'ℹ',
      onConfirm() { cleanup(); resolve() },
      onCancel() { cleanup(); resolve() }
    })

    function cleanup() {
      app.unmount()
      el.remove()
    }

    app.mount(el)
  })
}

export default confirm
