import request from '@/utils/request'

export function getTableList(params) {
    return request({
      url: '/serve/ServiceOrder/GetCustomerNewestOrders',  ///api/serve/ServiceOrder/GetCustomerNewestOrders
      method: 'get',
      params
    })
  }

