export function debounce (fn, delay) {
  let timer = null
  return function (...args) {
    let context = this
    if (timer) {
      clearTimeout(timer)
    }
    timer = setTimeout(() => {
      fn.apply(context, args)
    }, delay)
  }
}

export function swap (list, a, b) {
  let temp = list[a]
  list[a] = list[b]
  list[b] = temp
}

export function findIndex (list, fn) {
  let index = list.findIndex(fn)
  return index
}

