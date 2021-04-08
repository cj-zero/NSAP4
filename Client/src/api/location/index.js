
import request from '@/utils/request'

export function getRealTimeLocations (params) { //  获取app实时定位
  return request({
    url: '/RealTimeLocations/Load',
    method: 'get',
    params
  })
}
