import { toThousands } from '@/utils/format'
const data = []
const statusText = ['已支付', '未支付',  '已撤回']
for (let i = 0; i < 1000; i++) {
  data.push({
    accountId: i + 1,
    fillDate: '2020.08.02',
    org: 'S2',
    people: '我',
    position: 'S2',
    totalMoney: toThousands(200000),
    status: statusText[i % 3],
    projectName: '我我',
    customerId: 'C00002',
    customerName: '深圳道',
    customerRefer: '欣慰',
    salMan: '正则',
    origin: '道乐',
    destination: '欣慰',
    originDate: '2020.03.04',
    endDate: '2020.03.04',
    totalDay: 54,
    serviceOrderId: i + 2,
    serialNumber: i + 3,
    theme: '我我我欧文',
    problemType: '软件问题',
    solution: '解决方案' + i + 1,
    responsibility: '无责任',
    expense: toThousands(30000),
    laborRelations: '丛书关系',
    category: '出差',
    remark: '无'
  })

}

export default data

