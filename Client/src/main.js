import Vue from 'vue'
import layer from 'vue-layer'
import VueAMap from 'vue-amap';
import 'normalize.css/normalize.css'// A modern alternative to CSS resets

import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
import '@/assets/custom-theme/index.css'
import locale from 'element-ui/lib/locale/lang/zh-CN' // lang i18n
import VueContextMenu from 'vue-contextmenu'

import '@/styles/index.scss' // global css

import App from './App'
import router from './router'
import store from './store'

// 全局mixin
import GlobalMixin from '@/mixins/global'

import '@/icons' // icon
import '@/permission' // permission control
import '@/assets/public/css/iconfont/iconfont.css'
import '@/assets/public/css/iconfont/iconfont.js'
import '@/assets/public/css/comIconfont/iconfont/iconfont.css'
import '@/assets/public/css/comIconfont/iconfont/iconfont.js'

// 请假条表单和详情
import FrmLeaveReqAdd from '@/views/forms/userDefine/frmLeaveReq/add'
import FrmLeaveReqDetail from '@/views/forms/userDefine/frmLeaveReq/detail'

// 全局过滤器
import { toThousands } from '@/filter/money'
import { m2DHM } from '@/filter/time'
Vue.filter('toThousands', toThousands)
Vue.filter('m2DHM', m2DHM)

// 全局指令
import elDragDialog from '@/directive/el-dragDialog'
import debounce from '@/directive/utils/debounce'
import infoTooltip from '@/directive/info-tooltip'
Vue.directive('elDragDialog', elDragDialog)
Vue.directive('debounce', debounce)
Vue.use(infoTooltip)
// 全局组件
import GlobalComponent from '@/components/global'
Vue.use(GlobalComponent)
// import MyDialog from '@/components/Dialog'
// import SvgIcon from '@/components/SvgIcon'
// import Layer from '@/components/Layer'
// import CommonTable from '@/components/CommonTable'
// import Pagination from '@/components/Pagination'
// Vue.component('Layer', Layer)
// Vue.component('CommonTable', CommonTable)
// Vue.component('Pagination', Pagination)
// Vue.component('MyDialog', MyDialog)
// Vue.component(SvgIcon.name, SvgIcon)
// 引入PDFJS
Vue.use(ElementUI, { locale })
Vue.use(VueContextMenu)
Vue.use(VueAMap);
VueAMap.initAMapApiLoader({
  key: '24cc78e9598a08b90c8a256a8ec30bee',
  plugin: ['AMap.Autocomplete', 'AMap.PlaceSearch', 'AMap.Scale', 'AMap.OverView', 'AMap.ToolBar', 'AMap.MapType', 'AMap.PolyEditor', 'AMap.CircleEditor'],
  // 默认高德 sdk 版本为 1.4.4
  v: '1.4.4'
});

Vue.mixin(GlobalMixin)
Vue.config.productionTip = false
Vue.prototype.$layer = layer(Vue, {
  msgtime: 3
})
Vue.prototype.$delay = function (cb) {
  setTimeout(cb, 0)
}
Vue.component('FrmLeaveReqAdd', FrmLeaveReqAdd)
Vue.component('FrmLeaveReqDetail', FrmLeaveReqDetail)
new Vue({
  el: '#app',
  store,
  router,
  render: h => h(App)
})
