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
  console.log(list[a], list[b])
  let temp = list[a]
  list[a] = list[b]
  list[b] = temp
  console.log(list, 'list')
}