import { 
  TRAVEL_MONEY, 
  TRAFFIC_TYPES, 
  TRAFFIC_WAY, 
  OTHER_EXPENSE_TYPES,
  EXPENSE_CATEGORY 
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
  { label: '发票附件', disabled: true, type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', disabled: true, type: 'upload', prop: 'otherAttachment', width: 150 },
]

export let accommodationConfig = [ // 住房配置
  { label: '序号', type: 'order', width: 60, fixed: true },
  { label: '天数', prop: 'days', type: 'number', width: 100 },
  { label: '金额', prop: 'money', type: 'number', width: 120, disabled: true, align: 'right' },
  { label: '总金额', prop: 'totalMoney', type: 'number', width: 120, align: 'right' },
  { label: '备注', prop: 'remark', type: 'input', width: 100 },
  { label: '发票号码', disabled: true, type: 'input', prop: 'invoiceNumber', width: 100 },
  { label: '发票附件', disabled: true, type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', disabled: true, type: 'upload', prop: 'otherAttachment', width: 150 },
]

export let otherConfig = [ // 其他配置
  { label: '序号', type: 'order', width: 60, fixed: true },
  { label: '费用类别', prop: 'expenseCategory', type: 'select', width: 150, options: OTHER_EXPENSE_TYPES },
  { label: '其他费用', prop: 'money', type: 'number', width: 120, align: 'right' },
  { label: '备注', prop: 'remark', type: 'input', width: 100 },
  { label: '发票号码', disabled: true, type: 'input', prop: 'invoiceNumber', width: 100 },
  { label: '发票附件', disabled: true, type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', disabled: true, type: 'upload', prop: 'otherAttachment', width: 150 },
]

export let formConfig = [
  { label: '报销单号', prop: 'id', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '报销人', prop: 'currentUser', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '部门', prop: 'org', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '职位', prop: 'position', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true, width: 100 },
  { label: '服务ID', prop: 'serviceOrderId', palceholder: '请选择', required: true, col: 6, width: 100 },
  { label: '客户代码', prop: 'customerId', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '客户名称', prop: 'customerName', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '客户简称', prop: 'shortCustomerName', palceholder: '最长5个字', disabled: true, required: true, col: 6, maxlength: 5, isEnd: true, width: 100 },
  { label: '出发地点', prop: 'origin', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '到达地点', prop: 'destination', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '出发日期', prop: 'originDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, type: 'date', width: 100 },
  { label: '结束日期', prop: 'endDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, type: 'date', isEnd: true, width: 100 },
  { label: '报销类别', prop: 'remburseType', palceholder: '请输入内容', disabled: true, required: true, col: 6, type: 'select', options: EXPENSE_CATEGORY, width: 100 },
  { label: '项目名称', prop: 'projectName', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '报销状态', prop: 'remburseStatus', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true, width: 100 },
  { label: '呼叫主题', prop: 'theme', palceholder: '请输入内容', disabled: true, required: true, col: 18, width: 474 },
  { label: '填报事件', prop: 'fillDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true },
  { label: '设备类型', prop: 'materialType', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '解决方案', prop: 'solution', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '服务报告', prop: 'report',  disabled: true, required: true, col: 6, type: 'inline-slot', id: 'report', isEnd: true },
  { label: '费用承担', prop: 'bearToPay', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '责任承担', prop: 'responsibility', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '劳务关系', prop: 'serviceRelations', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
  { label: '支付时间', prop: 'payTime', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true },
  { label: '备注', prop: 'remark', palceholder: '请输入内容', disabled: true, required: true, col: 18, width: 474 },
  { label: '总金额', prop: 'totalMoney', col: 6, isEnd: true, type: 'inline-slot', id: 'money' },
  { label: '附件', prop: 'reimburseAttachments', type: 'slot', id: 'attachment', showLabel: true },
  { label: '出差补贴', prop: 'reimburseTravellingAllowances', type: 'slot', id: 'travel' },
  { label: '交通费用', prop: 'reimburseFares', type: 'slot', id: 'traffic' },
  { label: '住宿补贴', prop: 'reimburseAccommodationSubsidies', type: 'slot', id: 'accommodation' },
  { label: '其他费用', prop: 'reimburseOtherCharges', type: 'slot', id: 'other' }
]