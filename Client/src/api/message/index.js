import request from '@/utils/request'

export function sendPendingNumber(data = {}) { //推送服务模块工单消息
  return request({
    url: '/SignalR/Message/SendPendingNumber',
    method: 'post',
    data
  })
}


