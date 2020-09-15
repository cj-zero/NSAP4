import { getReportDetail, GetDetails } from '@/api/serve/callservesure'
import { AllowSendOrderUser } from "@/api/serve/callservepushm"
import { STATUS_COLOR_MAP } from '@/utils/declaration'
export let reportMixin = {
  data () {
    return {
      dialogReportVisible: false, // 完工报告弹窗标识
      reportData: [], // 完工报告数据
      serviceModeMap: {
        1: '电话服务',
        2: '返厂服务',
        3: '上门服务'
      }
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
          this.dialogReportVisible = true
        })
      } else {
        this.$message({
          type: 'warning',
          message: '工单未解决或在线解答方式无完工报告'
        })
      }
    },
    _normalizeReportData (data) {
      return data.map(item => {
        let { serviceMode } = item
        item.serviceText = serviceMode ? this.serviceModeMap[serviceMode] : serviceMode
        item.isPhoneService = Number(serviceMode) === 1
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
      orderRadio: '' //接单员单选
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
          this.dataForm1 = res.result;
          this.serveId = serviceOrderId
          this.dialogFormView = true;
        }
      })
    },
    openDetail() {
      this.dataForm = this.dataForm1;
    },
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
    }
  }
}