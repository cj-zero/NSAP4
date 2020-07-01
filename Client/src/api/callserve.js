
import request from '@/utils/request'

export function getPartner(params) {
    return request({
      url: '/Sap/BusinessPartner/Load',  //加载应用列表
      method: 'get',
      params
    })
  }
