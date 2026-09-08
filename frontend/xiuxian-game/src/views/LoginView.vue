<template>
  <!-- 修仙问道 - 登录页面 -->
  <div class="login-page">
    <!-- 背景动画层 -->
    <div class="bg-animation">
      <div class="stars"></div>
      <div class="spirit-flow"></div>
    </div>

    <!-- 登录容器 -->
    <div class="login-container">
      <!-- 标题区域 -->
      <div class="title-section">
        <h1 class="game-title">
          <span class="rune-decoration left"></span>
          修仙问道
          <span class="rune-decoration right"></span>
        </h1>
        <p class="game-subtitle">一念成仙，万法归宗</p>
      </div>

      <!-- 登录表单 -->
      <div class="login-form panel">
        <div class="form-header">
          <span class="active-tab">{{ isRegister ? '道友注册' : '道友登录' }}</span>
        </div>

        <div class="form-body">
          <!-- 账号输入 -->
          <div class="input-group">
            <label class="input-label">
              <span class="icon"><AssetIcon :source="ICON.misc_wizard" size="20" /></span>
              道号
            </label>
            <input
              v-model="form.username"
              type="text"
              placeholder="请输入道号"
              class="xiuxian-input"
              @keyup.enter="handleSubmit"
            />
          </div>

          <!-- 密码输入 -->
          <div class="input-group">
            <label class="input-label">
              <span class="icon"><AssetIcon :source="ICON.ui_lock" size="20" /></span>
              法印
            </label>
            <input
              v-model="form.password"
              type="password"
              placeholder="请输入法印"
              class="xiuxian-input"
              @keyup.enter="handleSubmit"
            />
            <p v-if="isRegister" class="password-hint">
              法印需为 8-32 位，且包含大写字母、小写字母和数字
            </p>
          </div>

          <!-- 确认密码 - 仅注册显示 -->
          <div v-if="isRegister" class="input-group">
            <label class="input-label">
              <span class="icon"><AssetIcon :source="ICON.ui_lock" size="20" /></span>
              确认法印
            </label>
            <input
              v-model="form.confirmPassword"
              type="password"
              placeholder="请再次输入法印"
              class="xiuxian-input"
              @keyup.enter="handleSubmit"
            />
          </div>

          <div v-if="isRegister" class="input-group">
            <label class="input-label">
              <span class="icon"><AssetIcon :source="ICON.stat_attack" size="20" /></span>
              修行职业
            </label>
            <div class="profession-selector">
              <button
                v-for="option in professionOptions"
                :key="option.value"
                type="button"
                class="profession-option"
                :class="{ active: form.profession === option.value }"
                @click="form.profession = option.value"
              >
                <span class="profession-option__title">{{ option.label }}</span>
                <span class="profession-option__desc">{{ option.description }}</span>
              </button>
            </div>
          </div>

          <!-- 记住密码 -->
          <div v-if="!isRegister" class="remember-row">
            <label class="checkbox-wrapper">
              <input v-model="form.remember" type="checkbox" />
              <span class="checkmark"></span>
              <span class="checkbox-label">铭记于心</span>
            </label>
            <a href="#" class="forgot-link" @click.prevent="showForgot">遗忘法印?</a>
          </div>

          <!-- 提交按钮 -->
          <button
            class="submit-btn"
            :class="{ loading: isLoading }"
            :disabled="isLoading"
            @click="handleSubmit"
          >
            <span v-if="isLoading" class="btn-spinner"></span>
            <span v-else>{{ isRegister ? '踏入仙途' : '登临仙境' }}</span>
          </button>
        </div>

        <!-- 底部切换 -->
        <div class="form-footer">
          <p class="toggle-text">
            {{ isRegister ? '已有道号?' : '尚无道号?' }}
            <a href="#" class="toggle-link" @click.prevent="toggleMode">
              {{ isRegister ? '立即登录' : '即刻修行' }}
            </a>
          </p>
        </div>

        <!-- 装饰符文 -->
        <div class="corner-runes">
          <span class="rune tl"></span>
          <span class="rune tr"></span>
          <span class="rune bl"></span>
          <span class="rune br"></span>
        </div>
      </div>

      <!-- 底部版权 -->
      <div class="copyright">
        <p>仙缘一线牵，珍惜这段缘</p>
      </div>
    </div>

    <!-- 提示弹窗 -->
    <Transition name="fade">
      <div v-if="toast.show" class="toast-message" :class="toast.type">
        {{ toast.message }}
      </div>
    </Transition>
  </div>
</template>

<script>
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useGameStore } from '../state/gameStore'
import { ICON } from '../icons'
import AssetIcon from '../components/common/AssetIcon.vue'

/**
 * LoginView - 登录页面
 *
 * 这里保留原来的登录页视觉结构，
 * 只把原先的本地模拟登录切换为真实后端鉴权。
 */
export default {
  name: 'LoginView',

  components: {
    AssetIcon
  },

  setup() {
    const router = useRouter()
    const gameStore = useGameStore()
    const strongPasswordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,32}$/

    // 表单数据
    const form = reactive({
      username: '',
      password: '',
      confirmPassword: '',
      profession: 'warrior',
      remember: false
    })

    const professionOptions = [
      { value: 'warrior', label: '战', description: '偏物攻与正面压制' },
      { value: 'mage', label: '法', description: '偏法攻与灵力爆发' },
      { value: 'body', label: '体', description: '偏血防与持续硬抗' }
    ]

    // 状态
    const isRegister = ref(false)
    const isLoading = ref(false)
    const toast = reactive({
      show: false,
      message: '',
      type: 'info'
    })

    let toastTimer = null

    // 显示提示
    const showToast = (message, type = 'info') => {
      toast.message = message
      toast.type = type
      toast.show = true

      if (toastTimer) {
        clearTimeout(toastTimer)
      }

      toastTimer = setTimeout(() => {
        toast.show = false
      }, 3000)
    }

    // 切换登录/注册模式
    const toggleMode = () => {
      isRegister.value = !isRegister.value
      form.password = ''
      form.confirmPassword = ''
      gameStore.clearFeedback()
    }

    // 显示忘记密码提示
    const showForgot = () => {
      showToast('当前版本暂不支持找回法印，请重新注册一个测试账号。', 'warning')
    }

    // 表单验证
    const validate = () => {
      if (!form.username.trim()) {
        showToast('请输入道号', 'error')
        return false
      }

      if (!form.password) {
        showToast('请输入法印', 'error')
        return false
      }

      if (isRegister.value) {
        if (!form.profession) {
          showToast('请选择修行职业', 'error')
          return false
        }

        if (!strongPasswordPattern.test(form.password)) {
          showToast('法印需为 8-32 位，且包含大写字母、小写字母和数字', 'error')
          return false
        }

        if (!form.confirmPassword) {
          showToast('请再次输入确认法印', 'error')
          return false
        }
      }

      if (isRegister.value && form.password !== form.confirmPassword) {
        showToast('两次法印不一致', 'error')
        return false
      }

      return true
    }

    // 提交表单
    const handleSubmit = async () => {
      if (!validate()) return

      isLoading.value = true
      gameStore.clearFeedback()

      try {
        if (isRegister.value) {
          await gameStore.register({
            account: form.username.trim(),
            password: form.password,
            confirmPassword: form.confirmPassword,
            name: form.username.trim(),
            profession: form.profession
          })
          showToast('注册成功，正在进入仙途。', 'success')
        } else {
          await gameStore.login({
            account: form.username.trim(),
            password: form.password
          })
          showToast('登录成功，正在进入游戏。', 'success')
        }

        setTimeout(() => {
          router.push('/game')
        }, 400)
      } catch (error) {
        showToast(error.message || (isRegister.value ? '注册失败' : '登录失败'), 'error')
      } finally {
        isLoading.value = false
      }
    }

    onMounted(() => {
      if (gameStore.state.accessToken) {
        router.replace('/game')
      }
    })

    return {
      ICON,
      form,
      isRegister,
      isLoading,
      toast,
      professionOptions,
      toggleMode,
      showForgot,
      handleSubmit
    }
  }
}
</script>

<style scoped>
/* 登录页面整体布局 */
.login-page {
  width: 100%;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
  background: var(--xiuxian-login-bg);
}

/* 背景动画 */
.bg-animation {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

/* 星空效果 */
.stars {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image:
    radial-gradient(2px 2px at 20px 30px, #eee, transparent),
    radial-gradient(2px 2px at 40px 70px, #ddd, transparent),
    radial-gradient(2px 2px at 50px 160px, #fff, transparent),
    radial-gradient(2px 2px at 90px 40px, #eee, transparent),
    radial-gradient(2px 2px at 130px 80px, #ddd, transparent),
    radial-gradient(2px 2px at 160px 120px, #fff, transparent);
  background-repeat: repeat;
  background-size: 200px 200px;
  animation: twinkle 10s linear infinite;
  opacity: 0.5;
}

@keyframes twinkle {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 0.8; }
}

/* 灵气流动效果 */
.spirit-flow {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: radial-gradient(ellipse at center, rgba(128, 128, 128, 0.06) 0%, transparent 70%);
  animation: pulse 4s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { transform: scale(1); opacity: 0.5; }
  50% { transform: scale(1.1); opacity: 0.8; }
}

/* 登录容器 */
.login-container {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 30px;
}

/* 标题区域 */
.title-section {
  text-align: center;
}

.game-title {
  font-size: 48px;
  font-weight: 700;
  color: var(--text-primary);
  letter-spacing: 12px;
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 10px;
}

.rune-decoration {
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 2px solid var(--border-color);
  transform: rotate(45deg);
}

.game-subtitle {
  font-size: 16px;
  color: var(--text-secondary);
  letter-spacing: 8px;
  opacity: 0.8;
}

/* 登录表单 */
.login-form {
  width: 400px;
  padding: 40px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  position: relative;
  backdrop-filter: blur(16px);
  box-shadow: var(--shadow-lg);
}

.form-header {
  text-align: center;
  margin-bottom: 30px;
}

.active-tab {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
  letter-spacing: 4px;
}

/* 表单输入 */
.form-body {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary);
}

.input-label .icon {
  font-size: 16px;
}

.xiuxian-input {
  width: 100%;
  padding: 12px 16px;
  background: var(--control-bg);
  border: 1px solid var(--control-border);
  border-radius: var(--radius-md);
  color: var(--control-text);
  font-size: 14px;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.xiuxian-input:focus {
  outline: none;
  border-color: var(--primary-color);
  background: var(--control-bg-hover);
  box-shadow: 0 0 0 2px oklch(from var(--primary-color) l c h / 0.15);
}

.xiuxian-input::placeholder {
  color: var(--control-placeholder);
}

.password-hint {
  margin: 0;
  font-size: 12px;
  line-height: 1.5;
  color: var(--text-muted);
}

.profession-selector {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
}

.profession-option {
  display: grid;
  gap: 4px;
  padding: 12px 10px;
  border: 1px solid var(--control-border);
  border-radius: var(--radius-md);
  background: var(--control-bg);
  color: var(--control-text);
  text-align: left;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.profession-option:hover {
  border-color: var(--text-secondary);
}

.profession-option.active {
  border-color: var(--primary-color);
  background: var(--highlight-soft-bg);
  box-shadow: 0 0 8px oklch(from var(--primary-color) l c h / 0.15);
}

.profession-option__title {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-primary);
}

.profession-option__desc {
  font-size: 12px;
  line-height: 1.45;
  color: var(--text-muted);
}

/* 记住密码行 */
.remember-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--font-size-base);
}

.checkbox-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.checkbox-wrapper input {
  display: none;
}

.checkmark {
  width: 16px;
  height: 16px;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.checkbox-wrapper input:checked + .checkmark {
  background: var(--primary-color);
  border-color: var(--primary-color);
}

.checkbox-wrapper input:checked + .checkmark::after {
  content: '✓';
  color: var(--button-primary-text);
  font-size: 12px;
  font-weight: bold;
}

.checkbox-label {
  color: var(--text-secondary);
  font-size: 13px;
}

.forgot-link {
  color: var(--primary-color);
  text-decoration: none;
  font-size: 13px;
  transition: color 0.15s ease;
}

.forgot-link:hover {
  filter: brightness(1.2);
}

/* 提交按钮 */
.submit-btn {
  width: 100%;
  padding: 14px;
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-lg);
  color: var(--button-primary-text);
  font-size: 16px;
  font-weight: 600;
  letter-spacing: 4px;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}

.submit-btn:hover:not(:disabled) {
  background: var(--button-primary-hover-start);
}

.submit-btn:active:not(:disabled) {
  transform: translateY(1px);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.btn-spinner {
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: var(--button-primary-text);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 表单底部 */
.form-footer {
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid var(--border-color);
  text-align: center;
}

.toggle-text {
  font-size: 13px;
  color: var(--text-secondary);
}

.toggle-link {
  color: var(--primary-color);
  text-decoration: none;
  margin-left: 5px;
  transition: all 0.15s ease;
}

.toggle-link:hover {
  filter: brightness(1.2);
  text-decoration: underline;
}

/* 角落符文装饰 */
.corner-runes {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.rune {
  position: absolute;
  width: 20px;
  height: 20px;
  border: 2px solid var(--border-color);
}

.rune.tl {
  top: -1px;
  left: -1px;
  border-right: none;
  border-bottom: none;
}

.rune.tr {
  top: -1px;
  right: -1px;
  border-left: none;
  border-bottom: none;
}

.rune.bl {
  bottom: -1px;
  left: -1px;
  border-right: none;
  border-top: none;
}

.rune.br {
  bottom: -1px;
  right: -1px;
  border-left: none;
  border-top: none;
}

/* 底部版权 */
.copyright {
  font-size: var(--font-size-base);
  color: var(--text-muted);
  text-align: center;
}

/* 提示消息 */
.toast-message {
  position: fixed;
  top: 50px;
  left: 50%;
  transform: translateX(-50%);
  padding: 12px 24px;
  border-radius: var(--radius-sm);
  font-size: 14px;
  z-index: 9999;
  animation: slide-down 0.15s ease;
}

.toast-message.success {
  background: rgba(30, 30, 30, 0.92);
  color: #8be0a8;
  box-shadow: var(--shadow-md);
}

.toast-message.error {
  background: rgba(30, 30, 30, 0.92);
  color: #e88;
  box-shadow: var(--shadow-md);
}

.toast-message.warning {
  background: rgba(30, 30, 30, 0.92);
  color: #f1d58b;
  box-shadow: var(--shadow-md);
}

.toast-message.info {
  background: rgba(30, 30, 30, 0.92);
  color: #8bc4e8;
  box-shadow: var(--shadow-md);
}

@keyframes slide-down {
  from {
    opacity: 0;
    transform: translateX(-50%) translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(-50%) translateY(0);
  }
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.15s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

@media (max-width: 720px) {
  .profession-selector {
    grid-template-columns: 1fr;
  }
}
</style>

