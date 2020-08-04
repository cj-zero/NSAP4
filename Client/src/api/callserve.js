
import request from '@/utils/request'

export function getPartner(params) {
    return request({
      url: '/Sap/BusinessPartner/Load',  //加载业务伙伴
      method: 'get',
      params
    })
  }

  export function getSerialNumber(params) {
    return request({
      url: '/Sap/SerialNumber/Get',  //加载序列号列表
      method: 'get',
      params
    })
  }

  // export function SendServiceOrderMessage(data) {
  //   return request({
  //     url: '/serve/ServiceOrder/SendServiceOrderMessage',  //发送聊天室消息
  //     method: 'post',
  //     data
  //   })
  // }

  // export function GetServiceOrderMessage(params) {
  //   return request({
  //     url: '/serve/ServiceOrder/GetServiceOrderMessage',  //获取服务单聊天记录
  //     method: 'get',
  //     params
  //   })
  // }
  
  // export function GetServiceOrderMessageList(params) {
  //   return request({
  //     url: '/serve/ServiceOrder/GetServiceOrderMessageList',  //获取服务单聊天列表
  //     method: 'get',
  //     params
  //   })
  // }

  // export function GetMessageCount(params) {
  //   return request({
  //     url: '/serve/ServiceOrder/GetMessageCount',  //获取未读消息个数
  //     method: 'get',
  //     params
  //   })
  // }
    export function GetServiceOrderMessages(params) {
    return request({
      url: '/serve/ServiceOrderMessage/GetServiceOrderMessages',  //获取聊天列表
      method: 'get',
      params
    })
  }
  
  export function SendMessageToTechnician(data) {
    return request({
      url: '/serve/ServiceOrderMessage/SendMessageToTechnician',  //发送聊天信息
      method: 'post',
      data
    })
  }
  
  export function GetUserProfile(params) {
    return request({
      url: `/Check/GetUserProfile?X-Token=${params}`,  //获取个人信息
      method: 'get',
    })
  }