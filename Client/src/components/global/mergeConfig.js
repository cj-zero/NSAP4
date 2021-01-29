import componentMap from './componentMap'
/* 合并配置项 */
export function mergeConfig (defaultConfig = {}, config = {}) {
  return Object.assign({}, defaultConfig, config)
}

/* 拿到对应的component组件 */
export function getComponentName (component) {
  return componentMap[component.tag].component || 'text'
}

/* 合并属性 */
export function mergeComponentAttrs (component, key = 'attrs') {
  let defaultComponent = componentMap[component.tag]
  return mergeConfig(defaultComponent[key] || {}, component[key] || {})
}