// 消息推送
import vm from './eventBus'
let hasConnected = false
function initSignalR (token = '') {
  if (hasConnected) {
    return
  }
  let signalR = window.signalR
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${process.env.VUE_APP_BASE}/MessageHub?x-token=${token}`, {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets
    })
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
    // console.log(user, message, 'user')
    vm.$notify.info({
      title: `来自${user}的消息`,
      message,
      duration: 0
    })
  })
  // 系统消息
  connection.on("SystemMessage", (user, message) => {
    // console.log(user, message, 'user')
    vm.$notify.info({
      title: `来自${user}的消息`,
      message,
      duration: 0
    })
  })

  // 工单数量 (服务呼叫模块)
  connection.on('PendingNumber', (user, message) => {
    if (typeof message === 'string') {
      try {
        message = JSON.parse(message)
      } catch (e) {
        // TODO
      }
    }
    vm.$emit('pendingNumber', message)
  })
  // 连接完成
  connection.on('Connected', () => {
    console.log("websocket has connected")
    hasConnected = true
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

// let messageReceive = {
//   // serviceWorkOrder
// }
// export messageReceive

export default initSignalR