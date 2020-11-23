import request from '@/utils/request'

export function getReturnNoteList(params) { // 获取退料单列表
  return request({
    url: 'ReturnNotes/GetReturnNoteList',
    method: 'get',
    params
  })
}

export function returnMaterials(data) { // 退料
  return request({
    url: '/ReturnNotes/ReturnMaterials',
    method: 'post',
    data
  })
}

export function getReturnNoteInfo(params) { // 获取退料详情
  return request({
    url: '/ReturnNotes/GetReturnNoteInfo',
    method: 'get',
    params
  })
}

export function getExpressInfo (params) { // 获取物流详情
  return request({
    url: '/ReturnNotes/GetExpressInfo',
    method: 'get',
    params
  })
}

export function saveReceiveInfo(data) { // 保存仓库验收记录
  return request({
    url: '/ReturnNotes/SaveReceiveInfo',
    method: 'post',
    data
  })
}

 


