export const on = (element, event, handler) => {
  if (document.addEventListener) {
    if (element && event && handler) {
      element.addEventListener(event, handler, false)
    }
  } else {
    if (element && event && handler) {
      element.attachEvent('on' + event, handler)
    }
  }
}

export const off = (element, event, handler) => {
  if (document.removeEventListener) {
    if (element && event && handler) {
      element.removeEventListener(event, handler, false)
    }
  } else {
    if (element && event && handler) {
      element.detachEvent('on' + event, handler)
    }
  }
}
