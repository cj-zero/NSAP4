import store from '@/store'
import { serializeParams } from './process'

export function print (url, params) {
  const printUrl = `${process.env.VUE_APP_BASE_API}${url}?${serializeParams(params)}&X-token=${store.state.user.token}`
  window.open(printUrl, '_blank')
}
