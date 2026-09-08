import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { buildApiUrl, getAccessToken } from './apiClient'

// 队伍副本实时同步只维护一条 Hub 连接，避免重复握手和重复收事件。
let connection = null
let connectionPromise = null

const battleResolvedListeners = new Set()
const stateListeners = new Set()

function emit(listeners, ...args) {
  listeners.forEach((listener) => {
    try {
      listener(...args)
    } catch (error) {
      console.error('Party hub listener error:', error)
    }
  })
}

function bindConnectionEvents(hubConnection) {
  hubConnection.on('PartyBattleResolved', (payload) => emit(battleResolvedListeners, payload))

  hubConnection.onreconnecting((error) => {
    emit(stateListeners, 'reconnecting', error)
  })

  hubConnection.onreconnected(() => {
    emit(stateListeners, 'connected')
  })

  hubConnection.onclose((error) => {
    connection = null
    connectionPromise = null
    emit(stateListeners, 'closed', error)
  })
}

export function subscribePartyHub({ onPartyBattleResolved, onStateChange } = {}) {
  if (typeof onPartyBattleResolved === 'function') {
    battleResolvedListeners.add(onPartyBattleResolved)
  }

  if (typeof onStateChange === 'function') {
    stateListeners.add(onStateChange)
  }

  return () => {
    if (typeof onPartyBattleResolved === 'function') {
      battleResolvedListeners.delete(onPartyBattleResolved)
    }

    if (typeof onStateChange === 'function') {
      stateListeners.delete(onStateChange)
    }
  }
}

export async function startPartyHub() {
  if (connection?.state === HubConnectionState.Connected) {
    return connection
  }

  if (connectionPromise) {
    return connectionPromise
  }

  connection = new HubConnectionBuilder()
    .withUrl(buildApiUrl('/hubs/party'), {
      accessTokenFactory: () => getAccessToken()
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(LogLevel.Warning)
    .build()

  bindConnectionEvents(connection)

  connectionPromise = connection.start()
    .then(() => {
      emit(stateListeners, 'connected')
      return connection
    })
    .catch((error) => {
      connection = null
      connectionPromise = null
      emit(stateListeners, 'error', error)
      throw error
    })

  return connectionPromise
}

export async function syncPartyHubGroups() {
  const hub = await startPartyHub()
  return hub.invoke('SyncPartyGroups')
}

export async function stopPartyHub() {
  if (!connection) {
    connectionPromise = null
    return
  }

  const hub = connection
  connection = null
  connectionPromise = null
  await hub.stop()
}
