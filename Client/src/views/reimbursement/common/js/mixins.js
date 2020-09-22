import rightImg from '@/assets/table/right.png'
import { getReportDetail } from '@/api/serve/callservesure'
export let tableMixin = {
  data () {
    return {
      rightImg,
      columns: [ // 表格配置
        { label: '报销单号', prop: 'id', type: 'link', width: 100, handleJump: this.openTree },
        { label: '填报日期', prop: 'createTime', width: 150 },
        { label: '报销部门', prop: 'orgName', width: 100 },
        { label: '报销人', prop: 'userName', width: 100 },
        { label: '报销状态', prop: 'remburseStatus', width: 100 },
        { label: '总金额', prop: 'totalMoney', width: 100 },
        { label: '客户代码', prop: 'terminalCustomerId', width: 100 },
        { label: '客户名称', prop: 'terminalCustomer', width: 100 },
        { label: '客户简称', prop: 'shortCustomerName', width: 100 },
        { label: '业务员', prop: 'saleSMan', width: 100 },
        { label: '出发日期', prop: 'businessTripDate', width: 150 },
        { label: '结束日期', prop: 'endDate', width: 150 },
        { label: '总天数', prop: 'businessTripDays', width: 100 },
        { label: '服务ID', prop: 'serviceOrderId', width: 100, type: 'link' },
        { label: '呼叫主题', prop: 'theme', width: 100 },
        { label: '项目名称', prop: 'projectName', width: 100 },
        { label: '服务报告', width: 100, handleClick: this.openReport, btnText: '查看' },
        { label: '责任承担', prop: 'responsibility', width: 100 },
        { label: '劳务关系', prop: 'serviceRelations', width: 100 },
        { label: '备注', prop: 'remark', width: 100 }
      ],
      tableData: [],
      total: 0, // 表格总数量
      formQuery: { // 查询字段参数
        id: '', // 报销单ID
        remburseStatus: '',
        createUserName: '',
        terminalCustomer: '',
        serviceOrderId: '',
        orgName: '',
        bearToPay: '',
        responsibility: '',
        staticDate: '',
        endDate: '',
        isDraft: false,
        reimburseType: ''
      },
      listQuery: { // 分页参数
        page: 1,
        limit: 30
      },
      tableLoading: false,
      currentRow: null, // 当前行的数据
      title: '', // 弹窗标题 同时也用于区别 报销单的状态
      detailData: {}, // 报销单详情
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download",
      tokenValue: this.$store.state.user.token,
      dialogReportVisible: false, // 完工报告弹窗标识
      reportData: [], // 完工报告数据
      serviceModeMap: {
        1: '电话服务',
        2: '上门服务',
        3: '返厂维修'
      }
    }
  },
  methods: {
    onRowClick (row) {
      this.currentRow = row
    },
    openReport (serviceOrderId) {
      getReportDetail({
        serviceOrderId
      }).then(res => {
        this.reportData = this._normalizeReportData(res.result.data)
        this.dialogReportVisible = true
      })
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

export let searchMixin = {
  data () {
    return {
      // TODO
    }
  }
}