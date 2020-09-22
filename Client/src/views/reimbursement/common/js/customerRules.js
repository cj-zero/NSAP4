let validateMoney = (rule, value, callback) => {
  value = Number(value)
  console.log(value, 'validaMoney')
  if (value === '' || isNaN(value) || typeof value !== 'number' || value <= 0) {
    callback(new Error())
  } else {
    callback();
  }
}

export let travelRules =  { // 出差补贴
  days: [ { required: true, trigger: 'blur', validator: validateMoney } ],
  money: [ { required: true, trigger: 'blur' } ],
}

export let trafficRules = { // 交通
  trafficType: [ { required: true, trigger: 'change' } ],
  transport: [ { required: true, trigger: 'change' } ],
  from: [ { required: true, trigger: 'blur' } ],
  to: [ { required: true, trigger: 'blur' } ],
  money: [ { required: true, trigger: ['blur', 'change'], validator: validateMoney } ],
  invoiceNumber: [ { required: true } ],
}

export let accRules = { // 住宿
  days: { required: true, trigger: 'change', validator: validateMoney } ,
  money: [ { required: true, trigger: 'change', disabled: true } ],
  totalMoney: [ { required: true, trigger: 'blur', validator: validateMoney } ],
  invoiceNumber: [ { required: true } ],
}

export let otherRules = { // 其他
  expenseCategory: [ { required: true, trigger: 'change' } ],
  money: [{ required: true, trigger: 'blur', validator: validateMoney }],
  invoiceNumber: [ { required: true } ],
}