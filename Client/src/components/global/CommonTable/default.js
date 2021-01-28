export const defaultTableConfig = {
  stripe: true,
  border: true,
  fit: true,
  size: 'mini',
  // 'row-class-name': tableRowClassName,
  'highlight-current-row': true,
  'header-row-style': headerRowStyle,
  'header-cell-style': headerRowStyle
}


// function tableRowClassName ({ row, rowIndex }) {
//   // 把每一行的index加到row中
//   row.index = rowIndex
// }
function headerRowStyle () {
  return {
    backgroundColor: '#DEDEDE'
  }
}
export const defaultColumnConfig = {
  'show-overflow-tooltip': true,
  'reserve-selection': true
}