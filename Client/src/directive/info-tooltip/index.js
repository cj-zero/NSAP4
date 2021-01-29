import Tip from './infotooltip.js'
import { addClass } from 'element-ui/lib/utils/dom.js'

//清空事件
function clearEvent(el) {
  if (el._tipHandler) {
    el.removeEventListener('mouseenter', el._tipHandler)
  }
  if (el._tipMouseleaveHandler) {
    el.removeEventListener('mouseleave', el._tipMouseleaveHandler)
  }
}

export default {
  install(Vue) {
    // tip 的展示方向
    const allPlacements = ['middle', 'top-start', 'top', 'right', 'bottom', 'left']
    Vue.directive('infotooltip', {
      bind(el, binding) {
        clearEvent(el)
        el._tipInstance = Tip()
        let { value, modifiers, arg } = binding
        el._tipHandler = function(e) {
          e.stopPropagation()
          const tooltip = el._tipInstance
          const cellChild = e.target
          const limitPlacementQueue = allPlacements.filter(placement => modifiers[placement])
          //设置显示位置，默认为top
          tooltip.placement = limitPlacementQueue.length ? limitPlacementQueue[0] : 'top'
          console.log(tooltip.placement, 'placement')
          // 判断是否有限制最大宽度
          if (arg) {
            tooltip.tooltipStyle.maxWidth = arg + 'px'
          }
          //判断是否有...
          if (cellChild.scrollWidth > cellChild.offsetWidth && tooltip && (!value || (value && typeof value === 'string'))) {
            tooltip.tooltipContent = [(cellChild.textContent || cellChild.value).replace(/[' ']/g, '')] //替换显示文本中的空格
            tooltip.referenceElm = cellChild //定位元素
            tooltip.handleMouseEnter() //设置显示
          } else {
            //如果指定了tooltip要显示的内容
            if (value) {
              tooltip.tooltipContent = Array.isArray(value) ? value : [value] //显示内容及为设置的内容
              tooltip.referenceElm = cellChild //定位元素
              tooltip.handleMouseEnter() //显示tooltip
              return
            }
          }
        }
        //鼠标移除执行的函数
        el._tipMouseleaveHandler = function(e) {
          e.stopPropagation()
          if (el._tipInstance) {
            el._tipInstance.handleMouseLeave() //隐藏元素
          }
        }
        //添加鼠标移上，移除事件
        el.addEventListener('mouseover', el._tipHandler)
        el.addEventListener('mouseout', el._tipMouseleaveHandler)
      },
      inserted(el, binding) {
        let { modifiers } = binding
        if (modifiers && modifiers['ellipsis']) {
          addClass(el, 'text-overflow')
        }
      },

      update(el, binding) {
        //如果传的value值是一个变量，在更新的时候也会及时更新
        el._binding = binding
      },

      unbind(el) {
        //在卸载的时候注销相关事件
        const instance = el._tipInstance
        instance.handleMouseLeave()
        if (instance && instance.doDestroy) {
          instance.doDestroy()
        }
        clearEvent(el)
      }
    })
  }
}
