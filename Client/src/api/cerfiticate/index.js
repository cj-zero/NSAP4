import request from '@/utils/request'

export function queryLoad(params) { // 证书查询列表
  return request({
    url: '/Certinfos/Load',
    method: 'get',
    params
  })
}

export function loadApprover (params) { // 证书审批流程列表 (送审/审核)
  return request({
    url: '/Certinfos/LoadApprover',
    method: 'get',
    params
  })
}

export function certVerificate (data) { // 证书审批操作 (送审/审核)
  return request({
    url: '/Certinfos/CertVerification',
    method: 'post',
    data
  })
}

export function getCertOperationHistory (params) { // 获取证书操作记录列表
  return request({
    url: '/CertOperationHistory/GetCertOperationHistory',
    method: 'get',
    params
  })
}


