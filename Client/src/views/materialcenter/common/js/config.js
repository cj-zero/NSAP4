// 退料单
export const ADD_RETURN_COLUMNS =  [
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription', width: 120 },
  { label: '需退总计', prop: 'count', align: 'right' },
  { label: '剩余需退', prop: 'surplusQty', align: 'right' },
  { label: '退回数量', slotName: 'returnQty' },
  { label: '图片', prop: 'quantity', slotName: 'upload' }
] 

export const EXPRESS_RETURN_COLUMNS = [
  { label: '#', type: 'index', width: 50 },
  { label: '快递单号', prop: 'expressNumber', width: '100px' },
  { label: '物流信息', prop: 'expressInformation', slotName: 'expressInformation', 'show-overflow-tooltip': false },
  { label: '运费￥', prop: 'freight', slotName: 'freight', align: 'right', width: 100, 'show-overflow-tooltip': false },
  { label: '图片', slotName: 'upload', width: 70 },
]

export const SHOW_RETURN_COLUMNS = [
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode', width: 150 },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '需退总计', prop: 'totalCount', align: 'right', width: 100 },
  { label: '剩余需退', prop: 'surplusQty', align: 'right', width: 100 },
  { label: '退回数量', prop: 'count', align: 'right', width: 100 },
  { label: '图片', slotName: 'upload', width: 50 }
]
// 品质检测
export const ADD_QUALITY_COLUMNS = [
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '返回数量', prop: 'count', align: 'right' },
  { label: '图片', slotName: 'upload' },
  { label: '良品', prop: 'goodQty', slotName: 'goodQty'},
  { label: '次品', prop: 'secondQty', slotName: 'secondQty' }
]

export const EXPRESS_QUALITY_COLUMNS = [
  { label: '#', type: 'index', width: 50 },
  { label: '快递单号', prop: 'expressNumber', width: '100px' },
  { label: '物流信息', prop: 'expressInformation', slotName: 'expressInformation', 'show-overflow-tooltip': false },
  { label: '运费￥', prop: 'freight', slotName: 'freight', align: 'right', width: 100, 'show-overflow-tooltip': false },
  { label: '图片', type: 'slot', slotName: 'upload', prop: 'expressagePicture', width: 70 },
  { label: '操作', slotName: 'operation', width: 50 }
]

export const SHOW_QUALITY_COLUMNS = [
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '需退总计', prop: 'totalCount', align: 'right' },
  { label: '剩余需退', prop: 'surplusQty', align: 'right' },
  { label: '退回数量', prop: 'count', align: 'right' },
  { label: '图片', slotName: 'upload' },
  { label: '良品', prop: 'goodQty', align: 'right' },
  { label: '次品', prop: 'secondQty', align: 'right' }
]

// 入库
export const EXPRESS_STORAGE_COLUMNS = [
  { label: '#', type: 'index', width: 50 },
  { label: '快递单号', prop: 'expressNumber', width: '100px' },
  { label: '物流信息', prop: 'expressInformation', slotName: 'expressInformation', 'show-overflow-tooltip': false },
  { label: '运费￥', prop: 'freight', slotName: 'freight', align: 'right', width: 100, 'show-overflow-tooltip': false },
  { label: '图片', type: 'slot', slotName: 'upload', prop: 'expressagePicture', width: 70 },
  // { label: '操作', slotName: 'operation', width: 50 }
]

export const SHOW_STORAGE_COLUMNS_TO_TEST = [ // 待测
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '返回数量', prop: 'count', align: 'right' },
  { label: '图片', prop: 'unit', align: 'right' },
  { label: '良品', prop: 'sentQuantity' },
  { label: '入库', prop: 'test' },
  { label: '次品', prop: 'quantity',  },
  { label: '入库', prop: 'test1' },
]

export const SHOW_STORAGE_COLUMNS_FINISHED = [ // 已完成
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '返回数量', prop: 'count', align: 'right' },
  { label: '图片', slotName: 'picture' },
  { label: '良品', prop: 'sentQuantity' },
  { label: '操作', prop: 'test' },
  { label: '次品', prop: 'quantity' },
  { label: '操作', prop: 'test1' },
]

export const SHOW_STORAGE_COLUMNS_TO_PUT = [ // 待入库
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '返回数量', prop: 'count', align: 'right' },
  { label: '图片', slotName: 'upload', width: 40 },
  { label: '良品', prop: 'goodQty', align: 'right', width: 100 },
  { label: '操作', slotName: 'toPutGood', width: 80 },
  { label: '次品', prop: 'secondQty', align: 'right', width: 100 },
  { label: '操作', slotName: 'toPutBad', width: 80 },
]


export const EXPRESS_COLUMNS_MAP = { // 快递表格
  returnOrder: EXPRESS_RETURN_COLUMNS,
  quality: EXPRESS_QUALITY_COLUMNS,
  storage: EXPRESS_STORAGE_COLUMNS
}

export const TABLE_COLUMNS_MAP = { // 物料动态变化的表格
  returnOrder: SHOW_RETURN_COLUMNS,
  quality: SHOW_QUALITY_COLUMNS
}

export const STORAGE_COLUMNS_MAP = {
  test: SHOW_STORAGE_COLUMNS_TO_TEST,
  finished: SHOW_STORAGE_COLUMNS_FINISHED,
  put: SHOW_STORAGE_COLUMNS_TO_PUT
}

export const QUALITY_TEXT_MAP = {
  0: '待测',
  1: '已测'
}

export const STORAGE_TEXT_MAP = {
  0: '待入库',
  1: '已入库'
}