/**
 * 修仙问道 - 主入口文件
 *
 * 东方玄幻风格的网页回合制挂机修仙游戏前端
 * 使用 Vue 3 + Vite 构建
 */

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

// 引入全局样式
import './styles/index.css'

// 初始化主题
const initTheme = () => {
  const savedTheme = localStorage.getItem('xiuxian-theme') || 'light'
  const validThemes = ['dark', 'light', 'elegant']
  const theme = validThemes.includes(savedTheme) ? savedTheme : 'light'
  document.documentElement.setAttribute('data-theme', theme)
}
initTheme()

const app = createApp(App)

app.use(router)
app.mount('#app')
