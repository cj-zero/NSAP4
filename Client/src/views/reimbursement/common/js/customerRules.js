let validateMoney = (rule, value, callback) => {
  value = Number(value)
  if (value === '' || isNaN(value) || typeof value !== 'number' || value <= 0) {
    callback(new Error())
  } else {
    callback();
  }
}

let validateInvoiceNumber = (rule, value, callback) => { // 校验发票号码8位数字
  console.log('validaInvo', /^[\d|a-z|A-Z]{8,11}$/.test(value), value)
  if (/^[\d|a-z|A-Z]{8,11}$/.test(value))  {
    callback()
  } else {
    // console.error('发票号码错误')
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
  // money: [ { required: true, trigger: 'change', disabled: true } ],
  totalMoney: [ { required: true, trigger: 'blur', validator: validateMoney } ],
  invoiceNumber: [ { required: true, trigger: ['blur', 'change'], validator: validateInvoiceNumber } ]
}

export let otherRules = { // 其他
  expenseCategory: [ { required: true, trigger: 'change' } ],
  money: [{ required: true, trigger: 'blur', validator: validateMoney }],
  invoiceNumber: [ { required: true, trigger: ['blur', 'change'], validator: validateInvoiceNumber } ]
}