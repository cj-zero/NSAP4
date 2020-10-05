import { toThousands } from '@/utils/format'
const data = []
for (let i = 0; i < 10; i++) {
  data.push({
    expenseCategory: 1,
    totalMoney: toThousands(1000),
    invoiceNumber: 0,
    invoiceAttachment: 123,
    date: '2020.7.26 09:15',
    remark: '最奇热'
  })
}
export default data