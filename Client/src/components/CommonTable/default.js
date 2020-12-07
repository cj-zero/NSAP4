export const defaultTableConfig = {
  height: '100%',
  stripe: true,
  border: true,
  fit: true,
  size: 'mini',
  'row-class-name': tableRowClassName,
  'highlight-current-row': true
}


function tableRowClassName ({ row, rowIndex }) {
  // 把每一行的index加到row中
  row.index = rowIndex
}

export const defaultColumnConfig = {
  'show-overflow-tooltip': true
}