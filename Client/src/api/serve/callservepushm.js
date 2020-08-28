import request from "@/utils/request";

export function getLeftList(params) {
  return request({
    url: "/serve/ServiceOrder/UnsignedWorkOrderTree", //呼叫服务未派单页面左侧树数据源
    method: "get",
    params,
  });
}

export function getRightList(params) {
  //  let params =qs.stringify(a, { indices: false })
  return request({
    url: "/serve/ServiceOrder/UnsignedWorkOrderList", //呼叫服务未派单页面右侧树数据源
    method: "get",
    params,
    // paramsSerializer: params => {
    //   return qs.stringify(params, { indices: false })
    // }
  });
}

export function getForm(data) {
  return request({
    url: `/serve/ServiceOrder/GetUnConfirmedServiceOrderDetails?serviceOrderId=${data}`,
    method: "get",
  });
}

export function AllowSendOrderUser(params) {
  return request({
    url: "/serve/ServiceOrder/GetAllowSendOrderUser",
    method: "get",
    params,
  });
}

export function SendOrders(data) {
  return request({
    url: "/serve/ServiceOrder/SendOrders", //主管给技术员派单
    method: "post",
    data,
  });
}

export function getImgUrl(data) {
  return request({
    url: `/Files/Download/${data}`,
    method: "get",
    params: data,
  });
}

export function update(data) {
  return request({
    url: "/Applications/AddOrUpdate",
    method: "post",
    data,
  });
}

export function del(data) {
  return request({
    url: "/certinfos/delete",
    method: "post",
    data,
  });
}

export function add(data) {
  return request({
    url: "/certinfos/delete",
    method: "post",
    data,
  });
}

export function getServiceOrder(params) { // 根据服务单ID获取行为报告
  return request({
    url: "/serve/ServiceOrder/GetServiceOrder",
    method: "get",
    params,
  });
}