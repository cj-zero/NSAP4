
import request from '@/utils/request'

export function getList(params) {
    return request({
      url: '/serve/AttendanceClock/Load',  //加载考勤列表
      method: 'get',
      params
    })
  }

  export function getRightList(params) {
    return request({
      url: '/serve/ServiceOrder/UnsignedWorkOrderList',  //呼叫服务未派单页面右侧树数据源
      method: 'get',
      params
    })
  }

export function getForm(data) {
    return request({
      url: `/serve/ServiceOrder/GetUnConfirmedServiceOrderDetails?serviceOrderId=${data}`,
      method: 'get'
      
    })
  }
  export function getImgUrl(data) {     
    return request({
      url:`/Files/Download/${data}`,
      method: 'get',
    })
  }

  export function update(data) {    
    return request({
      url: '/Applications/AddOrUpdate',
      method: 'post',
      data
    })
  }
  
  export function del(data) {
    return request({
      url: '/certinfos/delete',
      method: 'post',
      data
    })}

    export function add(data) {
      return request({
        url: '/certinfos/delete',
        method: 'post',
        data
      })}