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
  { label: '物流信息', prop: 'expressInformation', slotName: 'expressInformation', width: 200, 'show-overflow-tooltip': false },
  { label: '运费￥', prop: 'freight', slotName: 'freight', align: 'right', width: 100, 'show-overflow-tooltip': false },
  { label: '图片', type: 'slot', slotName: 'picture', prop: 'expressagePicture', width: 70 },
]

export const SHOW_RETURN_COLUMNS = [
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '需退总计', prop: 'count', align: 'right' },
  { label: '剩余需退', prop: 'unit', align: 'right' },
  { label: '退回数量', prop: 'returnCount', align: 'right' },
  { label: '图片', prop: 'quantity', slotName: 'upload' }
]
// 品质检测
export const ADD_QUALITY_COLUMNS = [
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '返回数量', prop: 'count', align: 'right' },
  { label: '图片', prop: 'unit', align: 'right' },
  { label: '良品', prop: 'sentQuantity', slotName: 'yliang'},
  { label: '次品', prop: 'quantity', slotName: 'quantity' }
]

export const EXPRESS_QUALITY_COLUMNS = [
  { label: '#', type: 'index', width: 50 },
  { label: '快递单号', prop: 'expressNumber', width: '100px' },
  { label: '物流信息', prop: 'expressInformation', slotName: 'expressInformation', width: 200, 'show-overflow-tooltip': false },
  { label: '运费￥', prop: 'freight', slotName: 'freight', align: 'right', width: 100, 'show-overflow-tooltip': false },
  { label: '图片', type: 'slot', slotName: 'picture', prop: 'expressagePicture', width: 70 },
  { label: '操作', slotName: 'qualityOperation' }
]

export const SHOW_QUALITY_COLUMNS = [
  { label: '序号', type: 'index' },
  { label: '物料编码', prop: 'materialCode' },
  { label: '物料描述', prop: 'materialDescription' },
  { label: '返回数量', prop: 'count', align: 'right' },
  { label: '图片', prop: 'unit', align: 'right' },
  { label: '良品', prop: 'sentQuantity', align: 'right' },
  { label: '次品', prop: 'quantity', align: 'right' }
]

// 入库
export const EXPRESS_STORAGE_COLUMNS = [
  { label: '#', type: 'index', width: 50 },
  { label: '快递单号', prop: 'expressNumber', width: '100px' },
  { label: '物流信息', prop: 'expressInformation', slotName: 'expressInformation', width: 200, 'show-overflow-tooltip': false },
  { label: '运费￥', prop: 'freight', slotName: 'freight', align: 'right', width: 100, 'show-overflow-tooltip': false },
  { label: '图片', type: 'slot', slotName: 'picture', prop: 'expressagePicture', width: 70 },
  { label: '操作', slotName: 'storageOperation' }
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
  { label: '图片', slotName: 'picture' },
  { label: '良品', prop: 'sentQuantity', slotName: 'yliang'},
  { label: '操作', prop: 'test', slotName: 'toPutGood' },
  { label: '次品', prop: 'quantity', slotName: 'quantity' },
  { label: '操作', prop: 'test', slotName: 'toPutBad' },
]