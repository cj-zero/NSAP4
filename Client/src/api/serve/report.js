
import request from '@/utils/request'

export function exportExcel(params) {
  return request({
    url: '/serve/ServiceOrder/ExportExcel',  // 表格导出为excel
    method: 'get',
    params
  })
}

export function getReport(params) {
  return request({
    url: '/serve/ServiceOrder/ServiceWorkOrderReport',  // 查询分析报表
    method: 'get',
    params
  })
}
