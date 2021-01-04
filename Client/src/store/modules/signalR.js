import Vue from 'vue'
export default {
  namespaced: true,
  state: {
    messageList: [],
    serviceOrderCount: 0,
    serviceWorkOrderCount: 0
  },
  mutations: {
    receiveMessage (state, payload) { // 
      let { user, message } = payload
      Vue.prototype.$notify.info({
        title: `来自${user}的消息`,
        message,
        duration: 0
      })
    },
    setMessageList (state, messageList) {
      state.messageList = messageList
    },
    setServiceOrderCount (state, serviceOrderCount) { // 待确认工单数量
      state.serviceOrderCount = serviceOrderCount
    },
    setServiceWordOrderCount (state, serviceWorkOrderCount) {// 未派单的工单那数量
      state.serviceWorkOrderCount = serviceWorkOrderCount
    }
  },
  actions: {
    initSignalR ({ commit }, payload) {
      let { token, callBack } = payload
      initSignalR(commit, token, callBack)
    }
  }
}

/**
 * 相关文档地址: https://docs.microsoft.com/zh-cn/aspnet/core/signalr/javascript-client?view=aspnetcore-3.1
 */

let hasConnected = false
function initSignalR (commit, token = '', callBack) { 
  if (hasConnected) {
    return
  }
  let signalR = window.signalR
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${process.env.VUE_APP_BASE}/MessageHub?x-token=${token}`, {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .configureLogging(signalR.LogLevel.Information)
    .build()

  async function start() {
    try {
      await connection.start()
    } catch (err) {
      // console.log(err)
      setTimeout(() => start(), 5000)
    }
  }

  connection.onclose(async () => {
    hasConnected = false
  })
  // 消息
  connection.on("ReceiveMessage", (user, message) => {
    commit('receiveMessage', { user, message })
  })
  // 系统消息
  connection.on("SystemMessage", (user, message) => {
    commit('receiveMessage', { user, message })
  })
  // ServiceOrderMessage
  connection.on('ServiceOrderMessage', (user, message) => {
    console.log('ServiceOrderMessage', user, message)
    commit('setMessageList', message)
  })
  connection.on('ServiceOrderCount', (user, message) => { // 服务待确认工单数量
    console.log('ServiceOrderCount')
    commit('setServiceOrderCount', message)
  })
  connection.on('ServiceWordOrderCount', (user, message) => { // 服务未呼叫工单数量
    console.log('ServiceWordOrderCount')
    commit('setServiceWordOrderCount', message)
    // this.message.serviceWorkOrderCount = messag
  })

  // 连接完成

  connection.on('Connected', () => {
    callBack && callBack()
    hasConnected = true
    console.log("websocket has connected", hasConnected)
  })
  
  // Start the connection.
  start()

  /* this is here to show an alternative to start, with a then
    connection.start().then(() => console.log("connected"))
    */

  /* this is here to show another alternative to start, with a catch
    connection.start().catch(err => console.error(err))
    */
}