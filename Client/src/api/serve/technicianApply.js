import request from '@/utils/request'

export function getTechnicianApplyDevices(params) { // 获取技术员提交/修改的设备信息
  return request({
    url: `/SeviceTechnicianApplyOrders/GetTechnicianApplyDevices`,
    method: 'get',
    params
  })
}
export function solveTechApplyDevices(data) { // 服务台处理技术员提交的设备信息
  return request({
    url: `/SeviceTechnicianApplyOrders/SolveTechApplyDevices`,
    method: 'post',
    data
  })
}

