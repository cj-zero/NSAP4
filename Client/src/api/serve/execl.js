
import request from '@/utils/request'

export function exportExcel(params) {
  return request({
    url: '/serve/ServiceOrder/ExportExcel',  // 表格导出为excel
    method: 'get',
    params
  })
}
