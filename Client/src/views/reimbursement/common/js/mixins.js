import rightImg from '@/assets/table/right.png'
import { getReportDetail, GetDetails } from '@/api/serve/callservesure'
import { getCategoryName } from '@/api/reimburse'
import { accommodationConfig } from './config'
import { toThousands } from '@/utils/format'
import { getList, getDetails } from '@/api/reimburse'
import { identifyInvoice } from '@/api/reimburse' // 票据识别
// import imageConversion from 'image-conversion'
export let tableMixin = {
  provide () {
    return {
      parentVm: this
    }
  },
  data () {
    return {
      rightImg,
      textMap: {
        create: '新建',
        edit: '编辑',
        view: '查看',
        approve: '审核',
        toPay: '支付',
        paid: '已支付'
      },
      submissionColumns: [ // 表格配置(我的提交)
        { label: '报销单号', prop: 'mainIdText', type: 'link', width: 70, handleJump: this.getDetail },
        { label: '报销状态', prop: 'remburseStatusText', width: 100 },
        { label: '服务ID', prop: 'serviceOrderSapId', width: 80, type: 'link', handleJump: this.openTree },
        { label: '呼叫主题', prop: 'fromTheme', width: 250 },
        { label: '客户代码', prop: 'terminalCustomerId', width: 75 },
        { label: '客户名称', prop: 'terminalCustomer', width: 170 },
        { label: '总金额', prop: 'totalMoney', width: 100, align: 'right' },
        { label: '总天数', prop: 'days', width: 60, align: 'right' },
        { label: '出发日期', prop: 'businessTripDate', width: 85 },
        { label: '结束日期', prop: 'endDate', width: 85 },
        { label: '报销部门', prop: 'orgName', width: 70 },
        { label: '报销人', prop: 'userName', width: 70 },
        // { label: '劳务关系', prop: 'serviceRelations', width: 100 },
        { label: '业务员', prop: 'salesMan', width: 80 },
        { label: '服务报告', width: 70, handleClick: this.openReport, btnText: '查看' },
        { label: '填报日期', prop: 'fillTime', width: 85 },
        { label: '备注', prop: 'remark' }
      ],
      processedColumns: [ // 不同的表格配置(我的提交除外的其它模块表格)
        { label: '报销单号', prop: 'mainIdText', type: 'link', width: 70, handleJump: this.getDetail },
        // { label: '服务ID', prop: 'serviceOrderSapId', width: 80 },
        { label: '客户代码', prop: 'terminalCustomerId', width: 75 },
        { label: '客户名称', prop: 'terminalCustomer', width: 170 },
        { label: '总金额', prop: 'totalMoney', width: 100, align: 'right' },
        { label: '总天数', prop: 'days', width: 60, align: 'right' },
        { label: '出发日期', prop: 'businessTripDate', width: 85 },
        { label: '结束日期', prop: 'endDate', width: 85 },
        { label: '报销部门', prop: 'orgName', width: 70 },
        { label: '报销人', prop: 'userName', width: 70 },
        // { label: '劳务关系', prop: 'serviceRelations', width: 100 },
        { label: '业务员', prop: 'salesMan', width: 80 },
        // { label: '服务报告', width: 70, handleClick: this.openReport, btnText: '查看' },
        { label: '填报日期', prop: 'fillTime', width: 85 }
      ],
      tableData: [],
      total: 0, // 表格数据的总数量
      dialogLoading: false, 
      formQuery: { // 查询字段参数
        mainId: '', // 报销单ID
        createUserName: '',
        terminalCustomer: '',
        serviceOrderId: '',
        orgName: '',
        bearToPay: '',
        responsibility: '',
        staticDate: '',
        endDate: '',
        reimburseType: '',
        // serviceRelations: '' // 劳务关系
      },
      listQuery: { // 分页参数
        page: 1,
        limit: 50,
        remburseStatus: '',
        pageType: ''
      },
      tableLoading: false,
      currentRow: null, // 当前行的数据
      title: '', // 弹窗标题 同时也用于区别 报销单的状态
      detailData: {}, // 报销单详情
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download", // 图片基地址
      tokenValue: this.$store.state.user.token,
      originUserId: this.$store.state.user.userInfoAll.userId // 当前用户的ID
    }
  },
  methods: {
    openLoading () {
      this.dialogLoading = true
    },
    closeLoading () {
      this.dialogLoading = false
    },
    onRowClick (row) {
      this.currentRow = row
    },
    print () {
      if (!this.currentRow) {
        return this.$message.warning('请先选择报销单号')
      }
      // console.log([printOrder, 'printOrder'])
      window.open(`${process.env.VUE_APP_BASE_API}/serve/Reimburse/Print?ReimburseInfoId=${this.currentRow.id}&X-Token=${this.tokenValue}`)
    },
    _getList () { // 获取表格列表
      this.tableLoading = true
      getList({
        ...this.formQuery,
        ...this.listQuery
      }).then(res => {
        let { data, count } = res
        this.tableData = this._normalizeList(data)
        this.total = count
        this.tableLoading = false
        this.currentRow = null
      }).catch((err) => {
        console.log(err, 'err')
        this.$message.error('获取列表失败')
        this.tableLoading = false
      })
    },
    onSearch () {
      this.listQuery.page = 1
      this._getList()
    },
    handleCurrentChange ({page, limit}) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    },
    _normalizeList (data) { // 处理table数据
      let reg = /[\r|\r\n|\n\t\v]/g
      return data.map(item => {
        let { reimburseResp } = item
        delete item.reimburseResp
        item = Object.assign({}, item, { ...reimburseResp })
        item.mainIdText = item.mainId || ''
        item.projectName = this.projectNameMap[item.projectName]
        item.remburseStatusText = this.reimburseStatusMap[item.remburseStatus]
        item.responsibility = this.responsibilityMap[item.responsibility]
        item.totalMoney = toThousands(item.totalMoney)
        item.themeList = JSON.parse(item.fromTheme.replace(reg, '')).map(item => item.description)
        item.fromTheme = item.themeList.join(' ')
        return item
      })
    },
    getDetail (val) { // 获取服务单详情
      let id
      let tableClick = false
      if (val.type === 'view') { // 如果是点击底部表格里的箭头查看详情
        id = val.id
        tableClick = true
      } else {
        if (!this.currentRow) { // 编辑审核等操作
          return this.$message.warning('请先选择报销单')
        }
        if (val.name === 'mySubmit') { // 我的提交模块判断
          console.log(this.originUserId, this.currentRow.createUserId)
          if (this.currentRow.createUserId !== this.originUserId) {
            return this.$message.warning('当前用户与报销人不符，不可编辑')
          }
          let status = this.currentRow.remburseStatus
          if (Number(status) > 3|| Number(status) === -1) {
            return this.$message.warning('当前状态不可编辑')
          }
        }
        id = this.currentRow.id
      }
      this.tableLoading = true
      getDetails({
        reimburseInfoId: id
      }).then(res => {
        let { reimburseResp } = res.data
        delete res.data.reimburseResp
        this.detailData = Object.assign({}, res.data, { ...reimburseResp })
        // 如果是审核流程、则判断当前用户是不是客服主管
        this.title = tableClick ? 'view' : val.type
        try {
          if ((this.title === 'approve' || this.title === 'view') && this.isGeneralManager) {
            // 用于总经理审批页面的表格数据
            this._generateApproveTable(this.detailData)
          }
          this._normalizeDetail(this.detailData)
          
        } catch (err) {
          console.log(err, 'err')
        }
        
        this.tableLoading = false
        this.$refs.myDialog.open()
      }).catch(() => {
        this.tableLoading = false
        this.$message.error('获取详情失败')
      })
    },
    isValidInvoice (attachmentList) { // 判断有没有发票附件
      return attachmentList.some(item => {
        return item.attachmentType === 2
      })
    },
    getOtherFileList (attachmentList) { // 获取其它附件列表
      return attachmentList.filter(item => {
        return item.attachmentType === 1
      })
    },
    getInvoiceFileList (attachmentList) { // 获取发票附件
      return attachmentList.filter(item => {
        return item.attachmentType === 2
      })
    },
    processInvoiceTime (invoiceTime) { // 截取年月日
      if (!invoiceTime) { // 为空直接返回
        return invoiceTime
      }
      let date = new Date(invoiceTime)
      let hours = date.getHours()
      let min = date.getMinutes()
      if (hours <= 0 && min <= 0) { // 判断时分是否存在， 都不存在就只展示年月日
        return invoiceTime.split(' ')[0]
      }
      invoiceTime = invoiceTime.slice(0, -3)
      return invoiceTime
    },
    _generateApproveTable (data) { // 针对总经理审批页面
      console.log(data, 'generate')
      let result = []
      let { 
        reimburseTravellingAllowances,
        reimburseFares,
        reimburseAccommodationSubsidies,
        reimburseOtherCharges,
      } = data
      reimburseFares.forEach(item => {
        let { invoiceTime, transport, from, to, money, reimburseAttachments, invoiceNumber, remark } = item
        result.push({
          invoiceTime: this.processInvoiceTime(invoiceTime),
          expenseName: this.transportationMap[transport],
          expenseDetail: from + '-' + to,
          money,
          remark,
          invoiceNumber,
          isValidInvoice: this.isValidInvoice(reimburseAttachments),
          invoiceFileList: this.getInvoiceFileList(reimburseAttachments),
          otherFileList: this.getOtherFileList(reimburseAttachments)
        })
      })
      reimburseAccommodationSubsidies.forEach(item => {
        let { invoiceTime, days, totalMoney, reimburseAttachments, invoiceNumber, remark } = item
        result.push({
          invoiceTime: this.processInvoiceTime(invoiceTime),
          expenseName: '住宿补贴',
          expenseDetail: days + '天',
          money: totalMoney,
          remark,
          invoiceNumber,
          isValidInvoice: this.isValidInvoice(reimburseAttachments),
          invoiceFileList: this.getInvoiceFileList(reimburseAttachments),
          otherFileList: this.getOtherFileList(reimburseAttachments)
        })
      })
      
      reimburseTravellingAllowances.forEach(item => {
        let { invoiceTime, days, money, remark } = item
        result.push({
          invoiceTime: this.processInvoiceTime(invoiceTime),
          expenseName: '出差补贴',
          expenseDetail: days + '天',
          money: money * days,
          remark
        })
      })
      reimburseOtherCharges.forEach(item => {
        let { invoiceTime, money, expenseCategory, remark, reimburseAttachments, invoiceNumber } = item
        result.push({
          invoiceTime: this.processInvoiceTime(invoiceTime),
          expenseName: this.otherExpensesMap[expenseCategory],
          expenseDetail: '',
          money,
          remark,
          invoiceNumber,
          isValidInvoice: this.isValidInvoice(reimburseAttachments),
          invoiceFileList: this.getInvoiceFileList(reimburseAttachments),
          otherFileList: this.getOtherFileList(reimburseAttachments)
        })
      })
      /* 日期从小到大， 没日期的话，交通费用→住宿补贴→出差补贴→其他费用 */
      let dataWithInvoiceTime = result.filter(item => item.invoiceTime).sort((a, b) => {
        return new Date(b.invoiceTime).getTime() - new Date(a.invoiceTime).getTime()
      })
      let dataWithoutInvoiceTime = result.filter(item => !item.invoiceTime)
      // 交通-住宿-出差-其它
      data.expenseCategoryList = dataWithInvoiceTime.concat(dataWithoutInvoiceTime)
      console.log(result, 'result')
      // data.expenseCategoryList = result
    },
    _normalizeDetail (data) { 
      let reg = /[\r|\r\n|\n\t\v]/g
      let { 
        reimburseAttachments,
        reimburseTravellingAllowances,
        reimburseFares,
        reimburseAccommodationSubsidies,
        reimburseOtherCharges,
        remburseStatus 
      } = data
      data.themeList = JSON.parse(data.fromTheme.replace(reg, ''))
      data.reimburseTypeText = this.reimburseStatusMap[remburseStatus] // 处理报销状态
      data.attachmentsFileList = reimburseAttachments
        .map(item => {
          item.name = item.attachmentName
          item.url = `${this.baseURL}/${item.fileId}?X-Token=${this.tokenValue}`
          return item
        })
      data.reimburseAttachments = []
      if (reimburseTravellingAllowances && reimburseTravellingAllowances.length) {
        reimburseTravellingAllowances[0].isAdd = true
      }
      // 处理附件
      this._buildAttachment(reimburseFares)
      this._buildAttachment(reimburseAccommodationSubsidies)
      this._buildAttachment(reimburseOtherCharges)
    },
    _buildAttachment (data) { // 为了回显，并且编辑 目标是为了保证跟order.vue的数据保持相同的逻辑
      data.forEach(item => {
        let { reimburseAttachments } = item
        item.invoiceFileList = this.getTargetAttachment(reimburseAttachments, 2)
        item.otherFileList = this.getTargetAttachment(reimburseAttachments, 1)
        item.invoiceAttachment = [],
        item.otherAttachment = []
        item.reimburseAttachments = []
        item.maxMoney = item.totalMoney || item.money
        item.isAdd = true
        item.isValidInvoice = Boolean(item.invoiceFileList.length)
      })
    },
    getTargetAttachment (data, attachmentType) { // 用于el-upload 回显
      return data.filter(item => item.attachmentType === attachmentType)
        .map(item => {
          item.name = item.attachmentName
          item.url = `${this.baseURL}/${item.fileId}?X-Token=${this.tokenValue}`
          item.isAdd = true
          return item
        })
    },
  }
}

export let searchMixin = {
  data () {
    return {
      // TODO
    }
  }
}
export const reportMixin = {
  data () {
    return {
      reportData: [], // 完工报告数据
      serviceModeMap: {
        1: '上门服务',
        2: '电话服务',
        3: '返厂维修'
      },
      reportBtnLoading: false // 如果是报销单上点的
    }
  },
  methods: {
    openReport (data, type) {
      let { serviceOrderId, createUserId } = data
      if (!serviceOrderId) {
        return this.$message.error('请先选择服务ID')
      }
      if (type === 'table') { // 如果是在表格上点的
        this.tableLoading = true
      } else {
        // 如果是报销单上点的
        this.reportBtnLoading = true
      }
      getReportDetail({
        serviceOrderId,
        userId: createUserId
      }).then(res => {
        this.reportData = this._normalizeReportData(res.result.data)
        if (this.reportData.length) {
          this.$refs.reportDialog.open()
        } else {
          this.$message.error('暂无完工报告数据')
        }
        // this.$refs.reportDialog.open()
        this.reportBtnLoading = false
        this.tableLoading = false
      }).catch((err) => {
        console.log(err, 'err')
        this.reportBtnLoading = false
        this.tableLoading = false
        this.$message.error('获取完工报告失败')
      })
    },
    resetReport () {
      this.$refs.report.reset()
    },
    _normalizeReportData (data) {
      return data.filter(item => item.id).map(item => {
        let { serviceMode } = item
        item.serviceText = serviceMode ? this.serviceModeMap[serviceMode] : serviceMode
        item.isPhoneService = Number(serviceMode) === 2
        return item
      })
    }
  }
}

const SYS_ReimburseType = 'SYS_ReimburseType' // 报销类别
const SYS_RemburseStatus = 'SYS_RemburseStatus' // 报销状态
const SYS_ProjectName = 'SYS_ProjectName' // 项目名称
const SYS_EXPENSE = 'SYS_Expense' // 费用承担
const SYS_Responsibility = 'SYS_Responsibility' // 责任承担
const SYS_ServiceRelations = 'SYS_ServiceRelations' // 劳务关系
const SYS_TravellingAllowance = 'SYS_TravellingAllowance' // 出差补贴
const SYS_TransportationAllowance = 'SYS_TransportationAllowance' // 交通类型
const SYS_Transportation = 'SYS_Transportation' // 交通方式
const SYS_OtherExpenses = 'SYS_OtherExpenses' // 其它费用
const SYS_ReimburseAccraditation = 'SYS_ReimburseAccraditation' // 备注弹窗的标签按钮 报销审批常用语
const SYS_ReimburseAccommodation = 'SYS_ReimburseAccommodation' // 报销住宿标准
export let categoryMixin = {
  data () {
    return {
      iconList: [ // 操作配置
        { icon: 'el-icon-document-add', handleClick: this.addAndCopy, operationType: 'add' }, 
        { icon: 'el-icon-document-copy', handleClick: this.addAndCopy, operationType: 'copy' }, 
        { icon: 'el-icon-delete', handleClick: this.delete }
      ],
      rolesList: this.$store.state.user.userInfoAll.roles, // 当前用户的角色列表
      userOrgName: this.$store.state.user.userInfoAll.orgName, // 部门名称
      serviceRelations: this.$store.state.user.userInfoAll.serviceRelations // 劳务关系
    }
  },
  methods: {
    _getCategoryName () {
      getCategoryName().then(res => {
        this.categoryList = res.data
      }).catch(() => {
        this.$message.error('获取字典分类失败')
      })
    },
    buildSelectOptions (list, isName = false) {
      if (isName) {
        return list.map(item => {
          return {
            label: item.name,
            value: item.name
          }
        })
      } else {
        list.forEach(item => {
          let { name, dtValue } = item
          item.label = name
          item.value = dtValue
        })
        return list
      }
    },
    buildMap (list) { // 用来对应表格的名字
      let result = {}
      list.forEach(item => {
        let { name, dtValue } = item
        result[dtValue] = name
      })
      return result
    },
    buildReimburseAcc (list) { // 住宿标准费用格式 { cityName: { dtValue:.. , description: ..} } isName
      let result = {}
      list.forEach(item => {
        let { name, description, dtValue } = item
        result[name] = { description, dtValue }
      })
      return result
    }
  },
  computed: {
    reimburseTypeList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_ReimburseType))
    },
    reimburseStatusList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_RemburseStatus))
    },
    reimburseStatusMap () {
      return this.buildMap(this.categoryList.filter(item => item.typeId === SYS_RemburseStatus))
    },
    expenseList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_EXPENSE))
    },
    projectNameList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_ProjectName))
    },
    projectNameMap () {
      return this.buildMap(this.categoryList.filter(item => item.typeId === SYS_ProjectName))
    },
    responsibilityList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_Responsibility))
    },
    responsibilityMap () {
      return this.buildMap(this.categoryList.filter(item => item.typeId === SYS_Responsibility))
    },
    serviceRelationsList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_ServiceRelations), true)
    },
    travellingList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_TravellingAllowance))
    },
    transportTypeList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_TransportationAllowance))
    },
    transportationMap () {
      return this.buildMap(this.categoryList.filter(item => item.typeId === SYS_Transportation))
    },
    transportationList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_Transportation))
    },
    otherExpensesList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_OtherExpenses))
    },
    otherExpensesMap () {
      return this.buildMap(this.categoryList.filter(item => item.typeId === SYS_OtherExpenses))
    },
    reimburseTagList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_ReimburseAccraditation))
    },
    reimburseAccCityList () { // 住宿标准
      return this.buildReimburseAcc(this.categoryList.filter(item => item.typeId === SYS_ReimburseAccommodation))
    },
    isCustomerSupervisor () { // 判断是不是客服主管
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '客服主管')
        : false
    },
    isGeneralManager () { // 判断是不是总经理
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '总经理')
        : false
    },
    isGeneralStatus () { // 判断是不是处于总经理浏览
      return this.isGeneralManager && (this.title === 'approve' || this.title === 'view')
    },
    isEditItem () { // 审批的时候只有客服主管可以改 新增编辑都可以修改
      return (this.title === 'view' || (this.title === 'approve' && !this.isCustomerSupervisor) || this.title === 'toPay')
    },
    formConfig () {
      return [
        { label: '服务ID', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: this.title !== 'create', readonly: true },
        { 
          label: '报销类别', prop: 'reimburseType', palceholder: '请输入内容', 
          col: this.ifFormEdit ? 5 : 6, type: 'select', options: this.reimburseTypeList, 
          disabled: this.isEditItem, width: '100%'
        },
        // { label: '客户简称', prop: 'shortCustomerName', palceholder: '最长6个字', col: this.ifFormEdit ? 5 : 6, maxlength: 6, disabled: this.isEditItem, required: true },
        { label: '费用承担', prop: 'bearToPay', palceholder: '请输入内容', 
          disabled: this.title === 'view' || !(this.isCustomerSupervisor && (this.title === 'create' || this.title === 'edit' || this.title === 'approve')), 
          col: this.ifFormEdit ? 5 : 6, type: 'select', options: this.expenseList, width: '100%'
        },
        { label: '报销状态', prop: 'reimburseTypeText', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 5 : 6, isEnd: true },
        { label: '客户代码', prop: 'terminalCustomerId', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 5 : 6 },
        { label: '客户名称', prop: 'terminalCustomer', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 10 : 12 },
        { label: '支付时间', prop: 'payTime', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 5 : 6, isEnd: true },
        { label: '呼叫主题', prop: 'fromTheme', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 15 : 18 },
        { label: '服务报告', prop: 'report',  disabled: true, col: this.ifFormEdit ? 5 : 6, 
          type: 'button', btnText: '服务报告', handleClick: this.openReport, isEnd: true
        },
        { label: '出发地点', prop: 'becity', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 5 : 6 },
        { label: '到达地点', prop: 'destination', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 5 : 6 },
        { label: '开始时间', prop: 'businessTripDate', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 5 : 6, width: '100%' },
        { label: '结束时间', prop: 'endDate', palceholder: '请输入内容', disabled: true, col: this.ifFormEdit ? 5 : 6, isEnd: true, width: '100%' },
        { label: '备注', prop: 'remark', palceholder: '请输入内容', disabled: !this.ifFormEdit, col: this.ifFormEdit ? 15 : 18 }, 
        { label: '总金额', type: 'money', col: this.ifFormEdit ? 5 : 6 }
      ]
    },    
    travelConfig () {
      let config = [
        { label: '天数', prop: 'days', type: 'number', width: 100, align: 'right' },
        { label: '金额', prop: 'money', type: 'number', disabled: true, width: 150, align: 'right' },
        { label: '备注', prop: 'remark', type: 'input', width: 150 }
      ]
      return (this.ifFormEdit !== undefined && !this.ifFormEdit) || this.ifFormEdit === undefined
        ? config
        : [...config, { label: '操作', type: 'operation', iconList: [{ icon: 'el-icon-delete', handleClick: this.delete }], width: 150 }]
    },
    trafficConfig () {
      let config = [ // 交通配置
        { label: '交通类型', prop: 'trafficType', type: 'select', options: this.transportTypeList, width: 105 },
        { label: '交通工具', prop: 'transport', type: 'select', options: this.transportationList, width: 135 },
        { label: '出发地', prop: 'from', type: 'input', width: 125, readonly: true },
        { label: '目的地', prop: 'to', type: 'input', width: 125, readonly: true },
        { label: '金额', prop: 'money', type: 'number', align: 'right', width: 120, placeholder: '大于0' },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', type: 'input', prop: 'invoiceNumber', width: 155, placeholder: '不能为空' },
        { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
        { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 }
      ]
      return (this.ifFormEdit !== undefined && !this.ifFormEdit) || this.ifFormEdit === undefined // 不可编辑状态并且是报销单页面(不影响我的费用配置)
        ? config 
        : [...config, { label: '操作', type: 'operation', iconList: [{ icon: 'el-icon-sort rotate', handleClick: this.changeAddr }, ...this.iconList], width: 130 }] // 交通配置        
    },
    accommodationConfig () {
      return (this.ifFormEdit !== undefined && !this.ifFormEdit) || this.ifFormEdit === undefined
        ? accommodationConfig
        : [...accommodationConfig, { label: '操作', type: 'operation', iconList: this.iconList, width: 160 }]
    }, 
    otherConfig () { // 其他配置  
      let config = [
        { label: '费用类别', prop: 'expenseCategory', type: 'select', width: 150, options: this.otherExpensesList },
        { label: '其他费用', prop: 'money', type: 'number', width: 120, align: 'right', placeholder: '大于0' },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', type: 'input', prop: 'invoiceNumber', width: 155, placeholder: '不能为空' },
        { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
        { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 }
      ]
      return (this.ifFormEdit !== undefined && !this.ifFormEdit) || this.ifFormEdit === undefined
        ? config
        : [...config, { label: '操作', type: 'operation', iconList: this.iconList, width: 168 }]
    },
    commonSearch () { // 搜索配置
      return [
        { placeholder: '报销单号', prop: 'mainId', width: 100 },
        { placeholder: '报销人', prop: 'createUserName', width: 100 },
        { placeholder: '客户代码/名称', prop: 'terminalCustomer', width: 150 },
        { placeholder: '服务ID', prop: 'serviceOrderId', width: 100 },
        { placeholder: '报销部门', prop: 'orgName', width: 100 },
        { placeholder: '费用承担', prop: 'bearToPay', width: 100, type: 'select', options: this.expenseList },
        // { placeholder: '责任承担', prop: 'responsibility', width: 100, type: 'select', options: this.responsibilityList },
        { placeholder: '劳务关系', prop: 'serviceRelations', width: 120, type: 'select', options: this.serviceRelationsList },
        { placeholder: '填报起始时间', prop: 'staticDate', type: 'date', width: 150 },
        { placeholder: '填报结束时间', prop: 'endDate', type: 'date', width: 150 }
      ]
    }
  }
}

// 0：出租车发票 1：定额发票 2：火车票 3：增值税发票 5：机票行程单 8：通用机打发票 
// 9：汽车票 10：轮船票 11：增值税发票（卷票 ）12：购车发票 13：过路过桥费发票
const TRAFFIC_TYPE_LIST = [0, 2, 5, 9, 10, 13] // 交通类型发票
const ACC_TYPE = 3
const invoiceTimeReg = /(^\d{4}-\d{2}-\d{2}).*$/ // 发票时间正则
export const attachmentMixin = {
  data () {
    return {
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download", // 图片基地址
      tokenValue: this.$store.state.user.token,
    }
  },
  methods: {
    onAccept (file, { prop }) { // 限制发票文件上传的格式
      if (prop === 'invoiceAttachment') {
        let { type } = file
        // console.log(size, 'file size')
        let imgReg = /^image\/\w+/i
        let isFitType = imgReg.test(type) || type === 'application/pdf'    
        return new Promise((resolve, reject) => {
          if (!isFitType) {
            this.$message.error('文件格式只能为图片或者PDF')
            reject(false)
          } else {
            resolve()
          }
          // if (type !== 'application/pdf') { // 图片文件先进行压缩，再上传
          //   const reader = new FileReader()
          //   const image = new Image()
          //   reader.onload = (e => { 
          //     image.src = e.target.result
          //   });
          //   reader.readAsDataURL(file)
          //   image.onload = function() {
          //     resolve(_this.compressUpload(image, file.type))
          //   }
          // } else {
          //   resolve() // pdf文件直接resolve
          // }
        })
      }
      return true
    },
    compressUpload(image, type) {
      let canvas = document.createElement("canvas") //创建画布元素
      let ctx = canvas.getContext("2d")
      let { width, height } = image
      canvas.width = width
      canvas.height = height
      ctx.fillRect(0, 0, canvas.width, canvas.height)
      ctx.drawImage(image, 0, 0, width, height)
      let compressData = canvas.toDataURL(type, 0.8) //等比压缩
      let blobImg = this.dataURItoBlob(compressData, type)//base64转Blob
      return blobImg
    },
    dataURItoBlob(dataURI, type) {
      var binary = atob(dataURI.split(',')[1])
      var array = []
      for(var i = 0; i < binary.length; i++) {
        array.push(binary.charCodeAt(i))
      }
      return new Blob([new Uint8Array(array)], { type })
    },
    _setCurrentRow (currentRow, data) { // 识别发票凭据后，对表格行进行赋值
      console.log('setCurrentRow')
      let { invoiceNo, invoiceDate, money, isAcc, isValidInvoice } = data
      if (isAcc) { // 住宿表格行数据
        currentRow.totalMoney = money
        currentRow.money = (currentRow.totalMoney / (currentRow.days || 1)).toFixed(2)
        if (this.accMaxMoney && Number(currentRow.money) > Number(this.accMaxMoney)) {
          this.$message.warning(`所填金额大于住宿补贴标准(${this.accMaxMoney}元)`)
        }
      } else {
        currentRow.money = money
      }
      this.$set(currentRow, 'isValidInvoice', isValidInvoice) // 判断发票是否正确，如果是正确的话就不给修改，不正确就给修改
      currentRow.maxMoney = money
      currentRow.invoiceNumber = invoiceNo
      currentRow.invoiceTime = invoiceDate.match(invoiceTimeReg) ? RegExp.$_ : ''
    },
    _setAttachmentList ({ data, index, prop, reimburseType, val }) { // 设置通过上传获取到的附件列表
      let resultArr = []
      resultArr = this.createFileList(val, {
        reimburseType,
        attachmentType: prop === 'invoiceAttachment' ? 2 : 1
      })
      let currentRow = data[index]
      currentRow[prop] = resultArr
      if (currentRow[prop] && !currentRow[prop].length && prop === 'invoiceAttachment') { // 删除发票附件的时候把金额跟发票号码删除
        if (currentRow.totalMoney) {
          currentRow.totalMoney = ''
        }
        currentRow.money = ''
        currentRow.invoiceNumber = ''
        currentRow.isValidInvoice = false
      }
    },
    _isValidInvoiceType ({ tableType, type, extendInfo }) { // 判断表格对应的发票类型跟上传的发票类型是否一直
      if (tableType === 'traffic') { // 交通发票
        return TRAFFIC_TYPE_LIST.includes(type) || (type === ACC_TYPE && /运输/g.test(extendInfo.serviceName))
      }
      if (tableType === 'acc') { // 住宿发票
        return type === ACC_TYPE 
          ? /住宿/g.test(extendInfo.serviceName)
          : false
      }
      return true
    },
    _identifyInvoice (data, isAcc = false) { // 票据识别
      let { fileId, currentRow, uploadVm, tableType } = data
      return new Promise(resolve => {
        identifyInvoice({
          fileId
        }).then(res => {
          if (res.data && !res.data.length) {
            this.$nextTick(() => {
              this._setCurrentRow(currentRow, {
                invoiceNo: '',
                money: '',
                isAcc,
                invoiceDate: '',
                isValidInvoice: false
              })
            })
            uploadVm.clearFiles()
            this.$message.error('识别失败,请上传至其它附件列表')
            resolve(false)
          } else {
            let { invoiceNo, invoiceDate, amountWithTax, isValidate, isUsed, notPassReason, type, extendInfo } = res.data[0]
            if (!isValidate || (isValidate && isUsed)) { // 识别失败
              this.$nextTick(() => {
                this._setCurrentRow(currentRow, {
                  invoiceNo: '',
                  money: '',
                  isAcc,
                  invoiceDate: '',
                  isValidInvoice: false
                })
              })
              uploadVm.clearFiles()
              this.$message.error(notPassReason ? notPassReason : '识别失败,请上传至其它附件列表')
              resolve(false)
            } else {
              // 识别成功，但是需要判断当前的发票类型是否跟表格的发票类型是否一致
              let isValidInvoice = this._isValidInvoiceType({ tableType, type, extendInfo })
              isValidInvoice ? this.$message.success('识别成功') : this.$message.warning('发票归类错误!')
              this.$nextTick(() => {
                this._setCurrentRow(currentRow, {
                  invoiceNo,
                  money: amountWithTax,
                  isAcc,
                  invoiceDate,
                  isValidInvoice: true
                })
              })
              resolve(true)
            }
          }
        }).catch(err => {
          console.error(err, 'err')
          this.$nextTick(() => {
            this._setCurrentRow(currentRow, {
              invoiceNo:'',
              money: '',
              isAcc,
              invoiceDate: '',
              isValidInvoice: false
            })
          })
          uploadVm.clearFiles()
          this.$message.error(err.message || '识别失败,请上传至其它附件列表')
          resolve(false)
        })
      })
    },
    _buildAttachment (data, isImport = false) { // 为了回显，并且编辑 目标是为了保证跟order.vue的数据保持相同的逻辑
      data.forEach(item => {
        let { reimburseAttachments } = item
        // console.log(reimburseAttachments, 'foeEach', item)
        item.invoiceFileList = this.getTargetAttachment(reimburseAttachments, 2, isImport)
        item.otherFileList = this.getTargetAttachment(reimburseAttachments, 1, isImport)
        item.invoiceAttachment = [],
        item.otherAttachment = []
        item.reimburseAttachments = []
        item.maxMoney = item.totalMoney || item.money // 存在附件时，需要对金额进行限制
        item.isAdd = true
        item.isValidInvoice = Boolean(item.invoiceFileList.length)
        if (isImport) {
          item.myExpendsId = item.id // 吧当前的费用id赋值到myExpendsId
        }
      })
    },
    getTargetAttachment (data, attachmentType, isImport) { // 用于el-upload 回显
      return data.filter(item => item.attachmentType === attachmentType)
        .map(item => {
          item.name = item.attachmentName
          item.url = `${this.baseURL}/${item.fileId}?X-Token=${this.tokenValue}`
          item.isAdd = true
          if (isImport) { // 如果是通过我的费用单引入的模板，则需要删除对应的ID,避免新建时出错
            item.reimburseId = 0
          }
          return item
        })
    },
    buildAttachment (fileId, reimburseType, attachmentType = 1, reimburseId = 0, id = 0, isAdd = true) { // 构建附件的数据格式
      return {
        fileId,
        reimburseType,
        attachmentType,
        reimburseId,
        id,
        isAdd
      }
    },
    createFileIdArr (data) { // 附件ID列表
      return data.map(item => item.pictureId)
    },
    createFileList (data, { reimburseType, attachmentType, reimburseId, id }) { // 附件列表
      let fileIdList = this.createFileIdArr(data)
      let resultArr = fileIdList.map(fileId => {
        return this.buildAttachment(fileId, reimburseType, attachmentType, reimburseId, id)
      })
      return resultArr
    },
    mergeFileList (data) {   
      data.forEach(item => {
        let { invoiceAttachment, otherAttachment, invoiceFileList, otherFileList, isImport } = item
        if (isImport) {
          item.id = '' // 如果是导入费用的话， 要把id变成空, 这些数据是没有新增和修改的 行数据
        }
        this._setAttachmentId(invoiceAttachment, isImport)
        this._setAttachmentId(otherAttachment, isImport)
        this._setAttachmentId(invoiceFileList, isImport)
        this._setAttachmentId(otherFileList, isImport)
        item.reimburseAttachments = [...invoiceAttachment, ...otherAttachment, ...invoiceFileList, ...otherFileList]
      })
    },
    _setAttachmentId (data, isImport) { // 如果是导入的数据需要将附件ID变成零
      data.forEach(item => {
        if (isImport) {
          item.id = 0
        }
      })
    }
  }
}

export const chatMixin = {
  data () {
    return {
      serveId: '',
      dataForm: {}, //传递的表单props
      temp: {
        id: "", // Id
        sltCode: "", // SltCode
        subject: "", // Subject
        cause: "", // Cause
        symptom: "", // Symptom
        descriptio: "", // Descriptio
        status: "", // Status
        extendInfo: "" // 其他信息,防止最后加逗号，可以删除
      }
    }
  },
  methods: {
    openTree(row, isOpenInTable = true) {
       // 判断服务单详情是否在表格页面打开,否则就是在报销单查看页面打开  isOpenInTable
      let serviceOrderId = row.serviceOrderId
      if (!serviceOrderId) {
        return this.$message.error('无服务单ID')
      }
      isOpenInTable ? this.tableLoading = true : this.orderLoading = true
      GetDetails(serviceOrderId).then(res => {
        if (res.code == 200) {
          this.dataForm = this._normalizeOrderDetail(res.result);
          this.serveId = serviceOrderId
          this.$refs.serviceDetail.open()
          isOpenInTable ? this.tableLoading = false : this.orderLoading = false
        }
      }).catch((err) => {
        console.error(err)
        isOpenInTable ? this.tableLoading = false : this.orderLoading = false
        this.$message.error('获取服务单详情失败')
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
  }
}
