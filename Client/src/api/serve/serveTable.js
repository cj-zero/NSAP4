
import request from '@/utils/request'

export function getTableList(params) {
    return request({
      url: '/serve/ServiceOrder/Load',  //服务单查询
      method: 'get',
      params
    })
  }

//   export function getSerialNumber(params) {
//     return request({
//       url: '/Sap/SerialNumber/Get',  //加载业务伙伴
//       method: 'get',
//       params
//     })
//   }