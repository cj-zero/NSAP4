import componentMap from './componentMap'
/* 合并配置项 */
export function mergeConfig (defaultConfig = {}, config = {}) {
  return Object.assign({}, defaultConfig, config)
}

/* 拿到对应的component组件 */
export function getComponentName (component, isSearch = false) {
  component.tag = component.tag ? component.tag : (isSearch ? 's-input' : 'text')
  return componentMap[component.tag].component
}

/* 合并属性 */
export function mergeComponentAttrs (component, key = 'attrs') {
  let defaultComponent = componentMap[component.tag]
  if (component[key] && component[key].style) {
    component[key].style = mergeConfig(defaultComponent[key].style || {}, component[key].style)
  }
  return mergeConfig(defaultComponent[key] || {}, component[key] || {})
}