import { 
  TRAVEL_MONEY, 
  TRAFFIC_TYPES, 
  TRAFFIC_WAY, 
  OTHER_EXPENSE_TYPES
} from './type'

export let travelConfig = [ // 出差配置
  { label: '天数', prop: 'days', type: 'number', fixed: true },
  { label: '金额', align: 'right', prop: 'money', type: 'select', options: TRAVEL_MONEY },
  { label: '备注', prop: 'remark', type: 'input' },
]

export let trafficConfig = [ // 交通配置
  { label: '序号', type: 'order', width: 60, fixed: true },
  { label: '交通类型', prop: 'trafficType', type: 'select', options: TRAFFIC_TYPES, width: 120 },
  { label: '交通工具', prop: 'transport', type: 'select', options: TRAFFIC_WAY, width: 120 },
  { label: '出发地', prop: 'from', type: 'input', width: 100 },
  { label: '目的地', prop: 'to', type: 'input', width: 100 },
  { label: '金额', prop: 'money', type: 'number', align: 'right', width: 150 },
  { label: '备注', prop: 'remark', type: 'input', width: 100 },
  { label: '发票号码', disabled: true, type: 'input', prop: 'invoiceNumber', width: 100 },
  { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
]

export let accommodationConfig = [ // 住房配置
  { label: '序号', type: 'order', width: 60, fixed: true },
  { label: '天数', prop: 'days', type: 'number', width: 100 },
  { label: '金额', prop: 'money', type: 'number', width: 120, disabled: true, align: 'right' },
  { label: '总金额', prop: 'totalMoney', type: 'number', width: 120, align: 'right' },
  { label: '备注', prop: 'remark', type: 'input', width: 100 },
  { label: '发票号码', disabled: true, type: 'input', prop: 'invoiceNumber', width: 100 },
  { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
]

export let otherConfig = [ // 其他配置
  { label: '序号', type: 'order', width: 60, fixed: true },
  { label: '费用类别', prop: 'expenseCategory', type: 'select', width: 150, options: OTHER_EXPENSE_TYPES },
  { label: '其他费用', prop: 'money', type: 'number', width: 120, align: 'right' },
  { label: '备注', prop: 'remark', type: 'input', width: 100 },
  { label: '发票号码', disabled: true, type: 'input', prop: 'invoiceNumber', width: 100 },
  { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
]

export let customerColumns = [ // 用户信息表单配置
  { type: 'radio', prop: 'id' },
  { label: '服务Id', prop: 'u_SAP_ID' },
  { label: '报销人', prop: 'userName' },
  { label: '客户代码', prop: 'terminalCustomerId' },
  { label: '客户名称', prop: 'terminalCustomer' },
  { label: '呼叫主题', prop: 'fromTheme' }
]