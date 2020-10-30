/**
 * 相关文档地址: https://docs.microsoft.com/zh-cn/aspnet/core/signalr/javascript-client?view=aspnetcore-3.1
 */

let hasConnected = false
function initSignalR (token = '', callBack) { 
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
    // console.log(user, message, 'user')
    this.$notify.info({
      title: `来自${user}的消息`,
      message,
      duration: 0
    })
  })
  // 系统消息
  connection.on("SystemMessage", (user, message) => {
    // console.log(user, message, 'user')
    this.$notify.info({
      title: `来自${user}的消息`,
      message,
      duration: 0
    })
  })
  // ServiceOrderMessage
  connection.on('ServiceOrderMessage', (user, message) => {
    console.log('ServiceOrderMessage', user, message)
    this.message.messageList = message
  })
  // 工单数量 (服务呼叫模块)
  // connection.on('PendingNumber', (user, message) => {
  //   if (typeof message === 'string') {
  //     try {
  //       message = JSON.parse(message)
  //     } catch (e) {
  //       // TODO
  //     }
  //   }
  //   console.log('pendingNumber', message)
  //   this.message = message
  // })
  connection.on('ServiceOrderCount', (user, message) => {
    console.log('ServiceOrderCount')
    this.message.serviceOrderCount = message
  })
  connection.on('ServiceWordOrderCount', (user, message) => {
    console.log('ServiceWordOrderCount')
    this.message.serviceWorkOrderCount = message
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

// let messageReceive = {
//   // serviceWorkOrder
// }
// export messageReceive

export default initSignalR