
import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/Serve/ServiceEvaluates/Load',  //加载考勤列表
    method: 'get',
    params
  })
}

export function add (params) {
  return request({
    method: 'get',
    params
  })
}

export function add (params) {
  return request({
    method: 'post',
    params
  })
}

export function edit (params) {
  return request({
    method: 'post',
    params
  })
}

// export functio
