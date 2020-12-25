const globalComponents = []

const context = require.context('./', true, /\.vue$/)

context.keys().forEach(file => {
  globalComponents.push(context(file).default)
})
console.log(context, globalComponents)

export default {
  // 注册全局组件
  install (Vue) {
    globalComponents.forEach(component => {
      Vue.component(component.name, component)
    })
  }
}
