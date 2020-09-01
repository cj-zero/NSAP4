
import request from '@/utils/request'

export function getList(params) {
    return request({
      url: '/Serve/ServiceEvaluates/Load',  //加载考勤列表
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
  })
}

export function add(data) {
  return request({
    url: '/certinfos/delete',
    method: 'post',
    data
  })
}

export function  addComment(data) { // 添加评价
  return request({
    url: '/Serve/ServiceEvaluates/AppAdd',
    method: 'post',
    data
  })
}

export function getComment (params) { // 获取评价
  return request({
    url: '/Serve/ServiceEvaluates/Get',
    method: 'get',
    params
  })
}

export function getTechnicianName (params) { // 获取技术员列表
  return request({
    url: '/Serve/ServiceEvaluates/GetTechnicianName',
    method: 'get',
    params
  })
}