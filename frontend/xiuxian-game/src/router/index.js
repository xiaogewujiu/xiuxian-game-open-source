/**
 * 游戏端路由配置。
 * 当前只分登录页和主游戏页，但登录态判断必须统一收口在这里。
 */

import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import MainView from '../views/MainView.vue'

const routes = [
  {
    path: '/',
    name: 'Login',
    component: LoginView,
    meta: { guestOnly: true }
  },
  {
    path: '/game',
    name: 'Game',
    component: MainView,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  // 中文注释：
  // 路由层统一判断是否允许进入主游戏页，
  // 避免页面先挂载、再因为未登录被动跳回登录页，产生无意义的首屏请求。
  const hasSession = Boolean(localStorage.getItem('xiuxian-access-token'))

  if (to.meta.requiresAuth && !hasSession) {
    return { name: 'Login' }
  }

  if (to.meta.guestOnly && hasSession) {
    return { name: 'Game' }
  }

  return true
})

export default router
