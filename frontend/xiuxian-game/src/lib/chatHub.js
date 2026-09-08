import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { buildApiUrl, getAccessToken } from './apiClient'

// 单例连接：整个前端只维护一条聊天 Hub 连接，避免重复握手和重复收消息。
let connection = null
let connectionPromise = null
let heartbeatTimer = null

// 事件订阅集合：由页面层按需注册，底层统一广播。
const messageListeners = new Set()
const onlineCountListeners = new Set()
const forceDisconnectListeners = new Set()
const stateListeners = new Set()

// 统一安全派发事件，避免单个监听器抛错把其他订阅链也打断。
function emit(listeners, ...args) {
  listeners.forEach((listener) => {
    try {
      listener(...args)
    } catch (error) {
      console.error('Chat hub listener error:', error)
    }
  })
}

// 停止心跳，通常在断线、重连或主动关闭连接时调用。
function stopHeartbeat() {
  if (heartbeatTimer) {
    clearInterval(heartbeatTimer)
    heartbeatTimer = null
  }
}

// 连接成功后定时向服务端发心跳，帮助服务端判断在线状态并及时剔除僵尸连接。
function startHeartbeat() {
  stopHeartbeat()
  heartbeatTimer = window.setInterval(() => {
    if (!connection || connection.state !== HubConnectionState.Connected) {
      return
    }

    connection.invoke('Heartbeat').catch(() => {})
  }, 20000)
}

// 把后端推送事件与 SignalR 生命周期事件统一绑定到本地订阅中心。
function bindConnectionEvents(hubConnection) {
  hubConnection.on('ReceiveMessage', (message) => emit(messageListeners, message))
  hubConnection.on('OnlineCountChanged', (count) => emit(onlineCountListeners, count))
  hubConnection.on('ForceDisconnect', (message) => emit(forceDisconnectListeners, message))

  hubConnection.onreconnecting((error) => {
    stopHeartbeat()
    emit(stateListeners, 'reconnecting', error)
  })

  hubConnection.onreconnected(() => {
    startHeartbeat()
    emit(stateListeners, 'connected')
  })

  hubConnection.onclose((error) => {
    stopHeartbeat()
    connection = null
    connectionPromise = null
    emit(stateListeners, 'closed', error)
  })
}

// 页面层通过订阅函数感知消息、在线人数、强制下线和连接状态变化。
export function subscribeChatHub({ onMessage, onOnlineCount, onForceDisconnect, onStateChange } = {}) {
  if (typeof onMessage === 'function') {
    messageListeners.add(onMessage)
  }

  if (typeof onOnlineCount === 'function') {
    onlineCountListeners.add(onOnlineCount)
  }

  if (typeof onForceDisconnect === 'function') {
    forceDisconnectListeners.add(onForceDisconnect)
  }

  if (typeof onStateChange === 'function') {
    stateListeners.add(onStateChange)
  }

  return () => {
    if (typeof onMessage === 'function') {
      messageListeners.delete(onMessage)
    }

    if (typeof onOnlineCount === 'function') {
      onlineCountListeners.delete(onOnlineCount)
    }

    if (typeof onForceDisconnect === 'function') {
      forceDisconnectListeners.delete(onForceDisconnect)
    }

    if (typeof onStateChange === 'function') {
      stateListeners.delete(onStateChange)
    }
  }
}

// 启动聊天连接。若已有可用连接或正在连接中的 Promise，则直接复用。
export async function startChatHub() {
  if (connection?.state === HubConnectionState.Connected) {
    return connection
  }

  if (connectionPromise) {
    return connectionPromise
  }

  connection = new HubConnectionBuilder()
    .withUrl(buildApiUrl('/hubs/chat'), {
      accessTokenFactory: () => getAccessToken()
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(LogLevel.Warning)
    .build()

  bindConnectionEvents(connection)

  connectionPromise = connection.start()
    .then(() => {
      startHeartbeat()
      emit(stateListeners, 'connected')
      return connection
    })
    .catch((error) => {
      stopHeartbeat()
      connection = null
      connectionPromise = null
      emit(stateListeners, 'error', error)
      throw error
    })

  return connectionPromise
}

// 供页面快速读取当前连接状态，用来显示“已连接/重连中/已断开”。
export function getChatHubState() {
  return connection?.state || HubConnectionState.Disconnected
}

// 通过实时 Hub 发送聊天消息。
export async function sendChatHubMessage(channelType, content) {
  const hub = await startChatHub()
  return hub.invoke('SendMessage', channelType, content)
}

// 在玩家宗门变化等需要重建频道归属的场景下，主动重连 Hub。
export async function restartChatHub() {
  await stopChatHub()
  return startChatHub()
}

// 主动关闭连接并清空本地引用，确保后续会重新走完整握手流程。
export async function stopChatHub() {
  stopHeartbeat()

  if (!connection) {
    connectionPromise = null
    return
  }

  const hub = connection
  connection = null
  connectionPromise = null
  await hub.stop()
}
