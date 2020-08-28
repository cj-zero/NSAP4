import request from '@/utils/request'

export function getAreaList(params) {
  return request({
    url: '/GlobalArea',  // 获取地图信息
    method: 'get',
    params
  })
}