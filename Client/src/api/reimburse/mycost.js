// 我的费用
import request from '@/utils/request'

export function getList(params) { // 获取我的费用
  return request({
    url: '/serve/MyExpends/Load',
    method: 'get',
    params
  })
}

export function addCost(data) { // 添加我的费用
  return request({
    url: '/serve/MyExpends/Add',
    method: 'post',
    data
  })
}

export function updateCost(data) { // 修改我的费用
  return request({
    url: '/serve/MyExpends/UpDate',
    method: 'post',
    data
  })
}

export function getDetail (params) { // 获取费用详情
  return request({
    url: '/serve/MyExpends/Details',
    method: 'get',
    params
  })
}

export function deleteCost(data) { // 添加我的费用
  return request({
    url: '/serve/MyExpends/Delete',
    method: 'post',
    data
  })
}