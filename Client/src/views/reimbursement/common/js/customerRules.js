let validateMoney = (rule, value, callback) => {
  value = Number(value)
  if (value === '' || isNaN(value) || typeof value !== 'number' || value <= 0) {
    callback(new Error())
  } else {
    callback();
  }
}

let validateInvoiceNumber = (rule, value, callback) => { // 校验发票号码8位数字
  // if (/^[\d|a-z|A-Z]{7,11}$/.test(value))  {
  //   callback()
  // } else {
  //   callback(new Error())
  // }
  if (value.trim()) {
    callback()
  } else {
    callback(new Error())
  }
}

export let travelRules =  { // 出差补贴
  days: [ { required: true, trigger: 'blur', validator: validateMoney } ],
  money: [ { required: true, trigger: 'change' } ],
}

export let trafficRules = { // 交通
  trafficType: [ { required: true, trigger: 'change' } ],
  transport: [ { required: true, trigger: 'change' } ],
  from: [ { required: true, trigger: ['blur', 'change'] } ],
  to: [ { required: true, trigger: ['blur', 'change'] } ],
  money: [ { required: true, trigger: ['blur', 'change'], validator: validateMoney } ],
  invoiceNumber: [ { required: true, trigger: ['blur', 'change'], validator: validateInvoiceNumber } ]
}

export let accRules = { // 住宿
  days: { required: true, trigger: 'change', validator: validateMoney } ,
  totalMoney: [ { required: true, trigger: 'blur', validator: validateMoney } ],
  invoiceNumber: [ { required: true, trigger: ['blur', 'change'], validator: validateInvoiceNumber } ]
}

export let otherRules = { // 其他
  expenseCategory: [ { required: true, trigger: 'change' } ],
  money: [{ required: true, trigger: 'blur', validator: validateMoney }],
  invoiceNumber: [ { required: true, trigger: ['blur', 'change'], validator: validateInvoiceNumber } ]
}