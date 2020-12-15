const modules = {}

const getFileName = string => { // 获取文件名
  let result = string.match(/[^\\|/]*(?=[.][a-zA-Z]+$)/)
  return result ? result [0] : ''
}

const context = require.context('./modules', false, /\.js$/)

context.keys().forEach(file => {
  let fileName = getFileName(file)
  if (fileName) {
    modules[fileName] = context(file).default
  }
})

export default modules