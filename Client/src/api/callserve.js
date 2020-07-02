
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
      url: '/Sap/SerialNumber/Get',  //加载业务伙伴
      method: 'get',
      params
    })
  }