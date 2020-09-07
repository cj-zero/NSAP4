
import request from '@/utils/request'

export function getServiceOrderLogs(params) {
  return request({
    url: '/serve/ServiceOrderLogs/Load',  //服务单查询
    method: 'get',
    params
  })
}