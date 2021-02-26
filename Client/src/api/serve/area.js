import request from '@/utils/request'

export function getAreaList(params, _this) {
  return request({
    url: '/GlobalArea/Load',  // 获取地图信息
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestFn = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}

export function searchAreaList(params, _this) {
  return request({
    url: '/GlobalArea/GetArea',  // 获取地图信息
    method: 'get',
    params,
    cancelToken: new request.cancelToken(function executor(c) {
      _this.cancelRequestFn = c // 用于取消上一次未响应的请求,已经响应的请求无法取消
    })
  })
}