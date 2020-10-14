export let accommodationConfig = [ // 住房配置
  // { label: '序号', type: 'order', width: 60 },
  { label: '天数', prop: 'days', type: 'number', width: 100 },
  { label: '金额', prop: 'money', type: 'number', width: 120, disabled: true, align: 'right' },
  { label: '总金额', prop: 'totalMoney', type: 'number', width: 120, align: 'right' },
  { label: '备注', prop: 'remark', type: 'input', width: 100 },
  { label: '发票号码', type: 'input', prop: 'invoiceNumber', width: 155 },
  { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
]


export let customerColumns = [ // 用户信息表单配置
  { type: 'radio', prop: 'id' },
  { label: '服务Id', prop: 'u_SAP_ID' },
  { label: '报销人', prop: 'userName' },
  { label: '客户代码', prop: 'terminalCustomerId' },
  { label: '客户名称', prop: 'terminalCustomer' },
  { label: '呼叫主题', prop: 'fromTheme' },
  { label: '出发地点', prop: 'becity' },
  { label: '到达地点', prop: 'destination' },
  { label: '出发日期', prop: 'businessTripDate' },
  { label: '结束日期', prop: 'endDate' },
]

export let costColumns = [ // 费用列表配置 
  { originType: 'selection' },
  { label: '费用类型', prop: 'feeType' },
  { label: '总金额', prop: 'moneyText' },
  { label: '发票号码', prop: 'invoiceNumber' },
  { label: '发票附件', prop: 'invoiceAttachment' },
  { label: '日期', prop: 'createTime', width: 140 },
  { label: '备注', prop: 'remark' }
]