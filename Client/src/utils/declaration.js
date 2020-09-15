export function noop () {} // 空函数

export const STATUS_COLOR_MAP = { // 工单呼叫状态颜色
  1: 'status-red',
  2: 'status-yellow',
  3: 'status-yellow',
  4: 'status-yellow',
  5: 'status-red',
  6: 'status-yellow',
  7: 'status-green',
  8: 'status-gray'
}

export const EXPENSE_CATEGORY = [ // 报销类别
  { label: '请选择', value: '' },
  { label: '测试仪项目交流', value: '1' },
  { label: '测试仪项目开发', value: '2' },
  { label: '测试仪项目测试', value: '3' },
  { label: '测试仪售后安装维护', value: '4' },
  { label: '自动化项目交流', value: '5' },
  { label: '自动化项目开发', value: '6' },
  { label: '自动化项目测试', value: '7' },
  { label: '软件项目交流', value: '8' },
  { label: '软件项目开发', value: '9' },
  { label: '软件项目测试', value: '10' },
  { label: '软件售后安装维护', value: '11' }
]