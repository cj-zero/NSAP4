export let checkInvoiceAttachment = (rule, value, callback) => {
  if (!value || !value.length) {
    return callback(new Error())
  }
  return callback()
}

export let checkMoney = (rule, value, callback) => {
  console.log(!value, typeof Number(value) !== 'number')
  if (!value || typeof Number(value) !== 'number') {
    return callback(new Error())
  }
  return callback()
}
