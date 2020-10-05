import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/Serve/KnowledgeBases/LoadTree',  // 加载知识库列表
    method: 'get',
    params
  })
}
export function getDetail(params) { // 
  return request({
    url: '/Serve/KnowledgeBases/Get',  // 获取单个知识库详情
    method: 'get',
    params
  })
}
export function add (data) {
  return request({
    url: '/Serve/KnowledgeBases/Add',  // 新增知识库
    method: 'post',
    data
  })
}
export function update(data) {
  return request({
    url: '/Serve/KnowledgeBases/Update',  // 更新单个知识库
    method: 'post',
    data
  })
}
export function deleteAll (data) {
  return request({
    url: '/Serve/KnowledgeBases/Load',  // 加载知识库列表
    method: 'post',
    data
  })
}
