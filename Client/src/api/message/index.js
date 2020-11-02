import request from '@/utils/request'

export function sendPendingNumber(params) { //推送服务模块工单消息(未处理和待派单消息)
  return request({
    url: '/serve/ServiceOrderPushNotification/SendPendingNumber',
    method: 'get',
    params
  })
}

export function getMessageWeb(params) { // 获取未读消息
  return request({
    url: '/serve/ServiceOrderPushNotification/GetMessageWeb',
    method: 'get',
    params
  })
}

export function readMessage (data) { // 消息已读
  return request({
    url: '/serve/ServiceOrder/ReadMsg',
    method: 'post',
    data
  })
}


