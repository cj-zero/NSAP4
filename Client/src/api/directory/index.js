
import request from '@/utils/request'

export function getCategoryNameList(params) { //  根据标识获取字典
  return request({
    url: '/Categorys/GetCategoryNameList',
    method: 'get',
    params
  })
}
