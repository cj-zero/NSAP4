import { formatDate } from '@/utils/date'
import { GetDetails } from '@/api/serve/callservesure'
import { isFunction } from '@/utils/validate'
export const chatMixin = {
  data () {
    return {
      serveId: '',
      dataForm: {} //传递的表单props
    }
  },
  methods: {
    openServiceOrder (serviceOrderId, before, after) {
       // 判断服务单详情是否在表格页面打开,否则就是在报销单查看页面打开  isOpenInTable
      if (!serviceOrderId) {
        return this.$message.error('无服务单ID')
      }
      if (isFunction(before)) {
        before()
      }
      GetDetails(serviceOrderId).then(res => {
        if (res.code == 200) {
          if (isFunction(after)) {
            after()
          }
          this.dataForm = this._normalizeOrderDetail(res.result);
          this.serveId = serviceOrderId
          this.$refs.serviceDetail.open()
        }
      }).catch((err) => {
        console.error(err)
        if (isFunction(after)) {
          after()
        }
        this.$message.error('获取服务单详情失败')
      })
    },
    _normalizeOrderDetail (data) {
      let reg = /[\r|\r\n|\n\t\v]/g
      let { serviceWorkOrders } = data
      if (serviceWorkOrders && serviceWorkOrders.length) {
        serviceWorkOrders.forEach(serviceOrder => {
          let { warrantyEndDate, bookingDate, visitTime, liquidationDate, completeDate } = serviceOrder
          serviceOrder.warrantyEndDate = formatDate(warrantyEndDate, 'YYYY-MM-DD HH:mm')
          serviceOrder.bookingDate = formatDate(bookingDate, 'YYYY-MM-DD HH:mm')
          serviceOrder.visitTime = formatDate(visitTime, 'YYYY-MM-DD HH:mm')
          serviceOrder.liquidationDate = formatDate(liquidationDate, 'YYYY-MM-DD HH:mm')
          serviceOrder.completeDate = formatDate(completeDate, 'YYYY-MM-DD HH:mm')
          serviceOrder.themeList = JSON.parse(serviceOrder.fromTheme.replace(reg, ''))
        })
      }
      return data
    },
  }
}