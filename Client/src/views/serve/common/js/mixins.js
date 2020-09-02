import { getReportDetail } from '@/api/serve/callservesure'
export let reportMixin = {
  data () {
    return {
      dialogReportVisible: false, // 完工报告弹窗标识
      reportData: [] // 完工报告数据
    }
  },
  methods: {
    onReportClosed () {
      this.reportData = []
      this.$refs.report.reset()
    },
    handleReport (serviceOrderId, data) {
      console.log(serviceOrderId, 'serviceOrderId')
      console.log(data, 'list')
      let hasFinished = data.some(workOrder => {
        let { status, fromType } = workOrder
        return Number(status) === 7 && Number(fromType) !== 2 // 所有工单状态为已解决且呼叫类型不为在线解答
      })
      if (hasFinished) {
        getReportDetail({
          serviceOrderId
        }).then(res => {
          console.log(res, 'reportData')
          this.reportData = res.result.data
          this.dialogReportVisible = true
        })
      } else {
        this.$message({
          type: 'warning',
          message: '工单未解决或在线解答方式无完工报告'
        })
      }
    }
  }
}