import { getReportDetail, GetDetails } from '@/api/serve/callservesure'
import { AllowSendOrderUser } from "@/api/serve/callservepushm"
import { STATUS_COLOR_MAP, PRIORITY_COLOR_MAP } from './map'
export let reportMixin = {
  data () {
    return {
      dialogReportVisible: false, // 完工报告弹窗标识
      reportData: [], // 完工报告数据
      serviceModeMap: {
        1: '上门服务',
        2: '电话服务',
        3: '返厂维修'
      },
    }
  },
  methods: {
    onReportClosed () {
      this.reportData = []
      this.$refs.report.reset()
    },
    handleReport (serviceOrderId, data) {
      let hasFinished = data.some(workOrder => {
        let { status, fromType } = workOrder
        return Number(status) >= 7 && Number(fromType) !== 2 // 所有工单状态为已解决且呼叫类型不为在线解答
      })
      if (hasFinished) {
        getReportDetail({
          serviceOrderId
        }).then(res => {
          this.reportData = this._normalizeReportData(res.result.data)
          if (this.reportData.length) {
            this.dialogReportVisible = true
          } else {
            this.$message.error('暂无完工报告数据')
          } 
        }).catch(() => {
          this.$message.error('获取完工报告失败')
        })
      } else {
        this.$message({
          type: 'warning',
          message: '工单未解决或在线解答方式无完工报告'
        })
      }
    },
    _normalizeReportData (data) {
      return  data.filter(item => item.id).map(item => {
        let { serviceMode } = item
        item.serviceText = serviceMode ? this.serviceModeMap[serviceMode] : serviceMode
        item.isPhoneService = Number(serviceMode) === 2
        return item
      })
    }
  }
}


export let dispatchMixin = { // 派单 转派
  data () {
    return {
      dataTree: [], // 问题类型数组
      listQuery2: {
        page: 1,
        limit: 10,
        currentUser: ''
      },
      orderRadio: '', //接单员单选
      loadingBtn: false // 按钮loading
    }
  },
  methods: {
    _getAllowSendOrderUser () { // 获取派单人员
      AllowSendOrderUser(this.listQuery2).then((res) => {
        this.tableData = res.data;
        this.total2 = res.count;
      });
    },
    onClosed () { // 关闭派单窗口
      this.listQuery2.currentUser = ''
      this.listQuery2.page = 1
      this.orderRadio = ''
    },
    onSearchUser () {
      this._getAllowSendOrderUser()
    },
    handleCurrentChange2(val) {
      this.listQuery2.page = val.page;
      this.listQuery2.limit = val.limit;
      this._getAllowSendOrderUser()
    },
    _normalizeRightList (data) {
      let reg = /[\r|\r\n|\n\t\v]/g
      data.forEach(item => {
        let { terminalCustomerId, terminalCustomer, customerId, customerName } = item
        item.customerId = terminalCustomerId || customerId
        item.customerName = terminalCustomer || customerName
        item.themeList = JSON.parse(item.fromTheme.replace(reg, '')).map(item => item.description)
        item.fromTheme = item.themeList.join(' ')
      })
      return data
    }
  },
  watch: {
    'listQuery2.currentUser' () {
      this.listQuery2.page = 1
    }
  }
}

export let chatMixin = { // 查看、编辑服务单时 右侧出现的聊天记录跟日志
  data () {
    return {
      serveId: '',
      dataForm: {}, //传递的表单props
      dataForm1: {}, //获取的详情表单
    }
  },
  methods: {
    openTree(serviceOrderId) {
      GetDetails(serviceOrderId).then(res => {
        if (res.code == 200) {
          this.dataForm1 = this._normalizeOrderDetail(res.result);
          this.serveId = serviceOrderId
          this.isFinish = res.result.isFinish
          this.dialogFormView = true;
        }
      })
    },
    deleteSeconds (date) { // yyyy-MM-dd HH:mm:ss 删除秒
      return date ? date.slice(0, -3) : date
    },
    _normalizeOrderDetail (data) {
      let reg = /[\r|\r\n|\n\t\v]/g
      let { serviceWorkOrders } = data
      if (serviceWorkOrders && serviceWorkOrders.length) {
        serviceWorkOrders.forEach(serviceOrder => {
          let { warrantyEndDate, bookingDate, visitTime, liquidationDate, completeDate } = serviceOrder
          serviceOrder.warrantyEndDate = this.deleteSeconds(warrantyEndDate)
          serviceOrder.bookingDate = this.deleteSeconds(bookingDate)
          serviceOrder.visitTime = this.deleteSeconds(visitTime)
          serviceOrder.liquidationDate = this.deleteSeconds(liquidationDate)
          serviceOrder.completeDate = this.deleteSeconds(completeDate)
          serviceOrder.themeList = JSON.parse(serviceOrder.fromTheme.replace(reg, ''))
        })
      }
      return data
    },
    openDetail() {
      this.dataForm = this.dataForm1;
    }
  }
}

export let tableMixin = {
  methods: {
    processStatus (val) {
      if (!val) {
        return
      }
      let { status } = val
      return STATUS_COLOR_MAP[status]
    },
    processFromType (val) {
      if (!val) {
        return
      }
      let { fromType } = val
      return fromType * 1 === 1 ? 'status-red' : 'status-green'
    },
    processPriorityStatus (val) {
      if (!val) {
        return
      }
      let { priority } = val
      return PRIORITY_COLOR_MAP[priority]
    },
    processServiceOrders (serviceWorkOrders) { // 返回第一项展示的值
      return serviceWorkOrders[0]
      // if (serviceWorkOrders && serviceWorkOrders.length === 1) {
      //   return serviceWorkOrders[0]
      // }
      // let processing = serviceWorkOrders.some(item => item.status <= 6) // 判断有没有正在处理的服务单
      // let result = []
      // result = processing // 优先展示正再处理的工单状态
      //   ? serviceWorkOrders.filter(item => item.status <= 6).sort((a, b) => b.status - a.status) // 优先级越大优先展示,大到小排序
      //   : serviceWorkOrders.filter(item => item.status >= 7).sort((a, b) => a.status - b.status) // 已完成(7) 优先于 已回访(8) 小到大排序
      // // 取出结果的第一个数据
      // return result[0]
    }
  }
}