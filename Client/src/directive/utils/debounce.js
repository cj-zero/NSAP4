import { debounce } from '@/utils/process'

export default {
  update (el, binding) {
    let { fn, delay = 500, immediate = false, eventName = 'click' } = binding.value
    console.log(el, fn ,delay, immediate, eventName)
    if (el) {
      el.addEventListener(eventName, debounce(fn, delay, immediate))
    }
  }
}