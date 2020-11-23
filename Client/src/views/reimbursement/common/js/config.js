export let accommodationConfig = [ // 住房配置
  { label: '天数', prop: 'days', type: 'number', width: 100, align: 'right' },
  { label: '金额', prop: 'money', type: 'number', width: 120, disabled: true, align: 'right' },
  { label: '总金额', prop: 'totalMoney', type: 'number', width: 120, align: 'right', placeholder: '大于0' },
  { label: '备注', prop: 'remark', type: 'input', width: 100 },
  { label: '发票号码', type: 'input', prop: 'invoiceNumber', width: 155, placeholder: '7-11位字母数字' },
  { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
  { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
]


export let customerColumns = [ // 用户信息表单配置
  { type: 'radio', prop: 'id', width: '50px' },
  { label: '服务Id', prop: 'u_SAP_ID', width: '70px' },
  { label: '客户名称', prop: 'terminalCustomer', width: '180px' },
  { label: '呼叫主题', prop: 'fromTheme', width: '300px' }
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