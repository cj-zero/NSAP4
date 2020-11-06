
import request from '@/utils/request'

export function getList(params) {
  return request({
    url: 'Asset/Load',  //加载资产列表
    method: 'get',
    params
  })
}

export function getSingleAsset(params) {
  return request({
    url: 'Asset/GetAsset', // 获取单个资产详情
    method: 'get',
    params
  })
}

export function add(data) {
  return request({
    url: 'Asset/Add', // 增加
    method: 'post',
    data
  })
}

export function update(data) {
  return request({
    url: 'Asset/Update', // 修改
    method: 'post',
    data
  })
}

export function getListCategoryName(params) {
  return request({
    url: 'Asset/GetListCategoryName', // 字典模糊查询
    method: 'get',
    params
  })
}

export function getListOrg(params) {
  return request({
    url: 'Asset/GetListOrg', // 模糊查询部门
    method: 'get',
    params
  })
}

export function getListUser(params) {
  return request({
    url: 'Asset/GetListUser', // 模糊查询人员
    method: 'get',
    params
  })
}
