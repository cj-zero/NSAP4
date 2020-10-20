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
      columns: [ // 表格配置
        { label: '报销单号', prop: 'mainIdText', type: 'link', width: 70, handleJump: this.getDetail },
        { label: '填报日期', prop: 'fillTime', width: 85 },
        { label: '报销部门', prop: 'orgName', width: 70 },
        { label: '报销人', prop: 'userName', width: 70 },
        { label: '报销状态', prop: 'remburseStatusText', width: 100 },
        { label: '总金额', prop: 'totalMoney', width: 100 },
        { label: '客户代码', prop: 'terminalCustomerId', width: 75 },
        { label: '客户名称', prop: 'terminalCustomer', width: 170 },
        { label: '客户简称', prop: 'shortCustomerName', width: 85 },
        { label: '业务员', prop: 'salesMan', width: 100 },
        { label: '出发日期', prop: 'businessTripDate', width: 85 },
        { label: '结束日期', prop: 'endDate', width: 85 },
        { label: '总天数', prop: 'businessTripDays', width: 60 },
        { label: '服务ID', prop: 'serviceOrderSapId', width: 80, type: 'link', handleJump: this.openTree },
        { label: '呼叫主题', prop: 'fromTheme', width: 100 },
        { label: '项目名称', prop: 'projectName', width: 80 },
        { label: '服务报告', width: 70, handleClick: this.openReport, btnText: '查看' },
        { label: '责任承担', prop: 'responsibility', width: 75 },
        { label: '劳务关系', prop: 'serviceRelations', width: 80 },
        { label: '备注', prop: 'remark', width: 100 }
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
        reimburseType: ''
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
      roleName: this.$store.state.user.name // 当前用户角色名字
    }
  },
  computed: {
    isCustomerSupervisor () { // 判断是不是客服主管
      return this.roleName === '客服主管'
    }
  },
  methods: {
    onRowClick (row) {
      this.currentRow = row
    },
    print () {
      if (!this.currentRow) {
        return this.$message({
          type: 'warning',
          message: '请先选择报销单号'
        })
      }
      // console.log([printOrder, 'printOrder'])
      window.open(`${process.env.VUE_APP_BASE_API}/serve/Reimburse/Print?ReimburseInfoId=${this.currentRow.id}&X-Token=${this.tokenValue}`)
    },
    _getList () {
      this.tableLoading = true
      getList({
        ...this.formQuery,
        ...this.listQuery
      }).then(res => {
        let { data, count } = res
        this.tableData = this._normalizeList(data)
        this.total = count
        this.tableLoading = false
        // if (!data.length) {
        //   this.$message.error('用户列表为空')
        // }
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
      return data.map(item => {
        let { reimburseResp } = item
        delete item.reimburseResp
        item = Object.assign({}, item, { ...reimburseResp })
        item.mainIdText = item.mainId || ''
        item.projectName = this.projectNameMap[item.projectName]
        item.remburseStatusText = this.reimburseStatusMap[item.remburseStatus]
        item.responsibility = this.responsibilityMap[item.responsibility]
        item.totalMoney = toThousands(item.totalMoney)
        // item.createTime = item.createTime.split(' ')[0]
        // item.businessTripDate = item.businessTripDate.split(' ')[0].replace('/', '.')
        // item.endDate = item.endDate.split(' ')[0].replace('/', '.')
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
          return this.$message({
            type: 'warning',
            message: '请先选择报销单'
          })
        }
        if (val.name === 'mySubmit') { // 我的提交模块判断
          if (this.currentRow.remburseStatus > 3) {
            return this.$message({
              type: 'warning',
              message: '当前状态不可编辑'
            })
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
        try {
          this._normalizeDetail(this.detailData)
        } catch (err) {
          console.log(err, 'err')
        }
        // 如果是审核流程、则判断当前用户是不是客服主管
        this.title = tableClick
          ? 'view'
          : val.type
        this.tableLoading = false
        this.$refs.myDialog.open()
      }).catch(() => {
        this.tableLoading = false
        this.$message.error('获取详情失败')
      })
    },
    _normalizeDetail (data) {
      let { 
        reimburseAttachments,
        reimburseTravellingAllowances,
        reimburseFares,
        reimburseAccommodationSubsidies,
        reimburseOtherCharges,
        remburseStatus 
      } = data
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
        1: '电话服务',
        2: '上门服务',
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
        item.isPhoneService = Number(serviceMode) === 1
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

export let categoryMixin = {
  data () {
    return {
      iconList: [ // 操作配置
        { icon: 'el-icon-document-add', handleClick: this.addAndCopy, operationType: 'add' }, 
        { icon: 'el-icon-document-copy', handleClick: this.addAndCopy, operationType: 'copy' }, 
        { icon: 'el-icon-delete', handleClick: this.delete }
      ],
      rolesList: this.$store.state.user.roles // 当前用户的角色列表
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
    buildSelectOptions (list) {
      list.forEach(item => {
        let { name, dtValue } = item
        item.label = name
        item.value = dtValue
      })
      return list
    },
    buildMap (list) { // 用来对应表格的名字
      let result = {}
      list.forEach(item => {
        let { name, dtValue } = item
        result[dtValue] = name
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
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_ServiceRelations))
    },
    travellingList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_TravellingAllowance))
    },
    transportTypeList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_TransportationAllowance))
    },
    transportationList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_Transportation))
    },
    otherExpensesList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_OtherExpenses))
    },
    isCustomerSupervisor () { // 判断是不是客服主管
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item.name === '客服主管')
        : false
    },
    isEditItem () {
      return (this.title === 'view' || (this.title === 'approve' && !this.isCustomerSupervisor) || this.title === 'toPay')
    },
    formConfig () {
      return [
        { label: '报销单号', prop: 'mainId', palceholder: '请输入内容', disabled: true, col: 5 },
        { label: '报销人', prop: 'userName', palceholder: '请输入内容', disabled: true, col: 5 },
        { label: '部门', prop: 'orgName', palceholder: '请输入内容', disabled: true, col: 5 },
        { label: '职位', prop: 'position', palceholder: '请输入内容', disabled: true, col: 5, isEnd: true },
        { label: '服务ID', prop: 'serviceOrderSapId', palceholder: '请选择', col: 5, disabled: this.title !== 'create', readonly: true },
        { label: '客户代码', prop: 'terminalCustomerId', palceholder: '请输入内容', disabled: true, col: 5 },
        { label: '客户名称', prop: 'terminalCustomer', palceholder: '请输入内容', disabled: true, col: 5 },
        { label: '客户简称', prop: 'shortCustomerName', palceholder: '最长5个字', col: 5, maxlength: 5, isEnd: true, disabled: this.isEditItem, required: true },
        { label: '出发地点', prop: 'becity', palceholder: '请输入内容', disabled: true, col: 5 },
        { label: '到达地点', prop: 'destination', palceholder: '请输入内容', disabled: true, col: 5 },
        { label: '出发日期', prop: 'businessTripDate', palceholder: '请输入内容', disabled: true, col: 5, width: '100%' },
        { label: '结束日期', prop: 'endDate', palceholder: '请输入内容', disabled: true, col: 5, isEnd: true, width: '100%' },
        { 
          label: '报销类别', prop: 'reimburseType', palceholder: '请输入内容', 
          col: 5, type: 'select', options: this.reimburseTypeList, 
          disabled: this.isEditItem, width: '100%'
        },
        { 
          label: '项目名称', prop: 'projectName', palceholder: '请输入内容',  
          col: 5, type: 'select', options: this.projectNameList, 
          disabled: this.isEditItem, width: '100%'
        },
        { label: '服务报告', prop: 'report',  disabled: true, col: 5, 
          type: 'button', btnText: '服务报告', handleClick: this.openReport
        },
        { label: '报销状态', prop: 'reimburseTypeText', palceholder: '请输入内容', disabled: true, col: 5, isEnd: true },
        { label: '呼叫主题', prop: 'fromTheme', palceholder: '请输入内容', disabled: true, col: 15 },
        { label: '填报时间', prop: 'createTime', palceholder: '请输入内容', disabled: true, col: 5, isEnd: true },
        { label: '费用承担', prop: 'bearToPay', palceholder: '请输入内容', 
          disabled: this.title === 'view' || !(this.isCustomerSupervisor && (this.title === 'create' || this.title === 'edit' || this.title === 'approve'))
          , 
          col: 5, type: 'select', options: this.expenseList, width: '100%'
        },
        { label: '责任承担', prop: 'responsibility', palceholder: '请输入内容', 
          col: 5, type: 'select', options: this.responsibilityList, 
          disabled: this.isEditItem, width: '100%' 
        },
        { label: '劳务关系', prop: 'serviceRelations', palceholder: '请输入内容',  
          col: 5, disabled: true
        },
        { label: '支付时间', prop: 'payTime', palceholder: '请输入内容', disabled: true, col: 5, isEnd: true },
        { label: '备注', prop: 'remark', palceholder: '请输入内容', disabled: this.title !== 'create', col: 15 },
        // { label: '总金额', prop: 'totalMoney', col: 5, isEnd: true, type: 'inline-slot', id: 'money' },
        { label: '附件', prop: 'reimburseAttachments', type: 'slot', id: 'attachment', showLabel: true },
        { label: '出差补贴', prop: 'reimburseTravellingAllowances', type: 'slot', id: 'travel' },
        { label: '交通费用', prop: 'reimburseFares', type: 'slot', id: 'traffic' },
        { label: '住宿补贴', prop: 'reimburseAccommodationSubsidies', type: 'slot', id: 'accommodation' },
        { label: '其他费用', prop: 'reimburseOtherCharges', type: 'slot', id: 'other' }
      ]
    },    
    travelConfig () {
      return [ // 出差配置
        { label: '天数', prop: 'days', type: 'number', width: 150 },
        { label: '金额', align: 'right', prop: 'money', type: 'number', disabled: true, width: 150 },
        { label: '备注', prop: 'remark', type: 'input', width: 150 },
        { label: '操作', type: 'operation', iconList: [{ icon: 'el-icon-delete', handleClick: this.delete }], width: 150 }
      ]
    },
    trafficConfig () {
      return [ // 交通配置
        // { label: '序号', type: 'order', width: 60 },
        { label: '交通类型', prop: 'trafficType', type: 'select', options: this.transportTypeList, width: 105 },
        { label: '交通工具', prop: 'transport', type: 'select', options: this.transportationList, width: 135 },
        { label: '出发地', prop: 'from', type: 'input', width: 125, readonly: true },
        { label: '目的地', prop: 'to', type: 'input', width: 125, readonly: true },
        { label: '金额', prop: 'money', type: 'number', align: 'right', width: 120, placeholder: '大于0' },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', type: 'input', prop: 'invoiceNumber', width: 155, placeholder: '8-11位字母数字' },
        { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
        { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
        { label: '操作', type: 'operation', iconList: this.iconList, width: 110 }
      ]
    },
    accommodationConfig () {
      return [...accommodationConfig, { label: '操作', type: 'operation', iconList: this.iconList, width: 160 }]
    }, 
    otherConfig () {
      return [ // 其他配置
        // { label: '序号', type: 'order', width: 60 },
        { label: '费用类别', prop: 'expenseCategory', type: 'select', width: 150, options: this.otherExpensesList },
        { label: '其他费用', prop: 'money', type: 'number', width: 120, align: 'right', placeholder: '大于0' },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', type: 'input', prop: 'invoiceNumber', width: 155, placeholder: '8-11位字母数字' },
        { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
        { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
        { label: '操作', type: 'operation', iconList: this.iconList, width: 168 }
      ]
    },
    commonSearch () { // 搜索配置
      return [
        { placeholder: '报销单号', prop: 'mainId', width: 100 },
        { placeholder: '报销人', prop: 'createUserName', width: 100 },
        { placeholder: '客户代码/名称', prop: 'terminalCustomer', width: 150 },
        { placeholder: '服务ID', prop: 'serviceOrderId', width: 100 },
        { placeholder: '报销部门', prop: 'orgName', width: 100 },
        { placeholder: '费用承担', prop: 'bearToPay', width: 100, type: 'select', options: this.expenseList },
        { placeholder: '责任承担', prop: 'responsibility', width: 100, type: 'select', options: this.responsibilityList },
        { placeholder: '填报起始时间', prop: 'staticDate', type: 'date', width: 150 },
        { placeholder: '填报结束时间', prop: 'endDate', type: 'date', width: 150 }
      ]
    }
  }
}

// 0：出租车发票 1：定额发票 2：火车票 3：增值税发票 5：机票行程单 8：通用机打发票 
// 9：汽车票 10：轮船票 11：增值税发票（卷票 ）12：购车发票 13：过路过桥费发票
const TRAFFIC_TYPE_LIST = [0, 2, 5, 8, 9, 10, 13] // 交通类型发票
const ACC_TYPE = 3

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
        let { type, size } = file
        console.log(size, 'file size')
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
      console.log(type, 'type URI')
      return new Blob([new Uint8Array(array)], { type })
    },
    _setCurrentRow (currentRow, data) { // 识别发票凭据后，对表格行进行赋值
      let { invoiceNo, money, isAcc, isValidInvoice } = data
      if (isAcc) { // 住宿表格行数据
        currentRow.totalMoney = money
        currentRow.money = (currentRow.totalMoney / (currentRow.days || 1)).toFixed(2)
      } else {
        currentRow.money = money
      }
      this.$set(currentRow, 'isValidInvoice', isValidInvoice) // 判断发票是否正确，如果是正确的话就不给修改，不正确就给修改
      currentRow.maxMoney = money
      currentRow.invoiceNumber = invoiceNo
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
      console.log(tableType, type, extendInfo, 'check')
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
            this._setCurrentRow(currentRow, {
              invoiceNo: '',
              money: '',
              isAcc,
              isValidInvoice: false
            })
            uploadVm.clearFiles()
            this.$message.error('识别失败,请上传至其它附件列表')
            resolve(false)
          } else {
            let { invoiceNo, amountWithTax, isValidate, isUsed, notPassReason, type, extendInfo } = res.data[0]
            if (!isValidate || (isValidate && isUsed)) { // 识别失败
              this._setCurrentRow(currentRow, {
                invoiceNo: '',
                money: '',
                isAcc,
                isValidInvoice: false
              })
              uploadVm.clearFiles()
              this.$message.error(notPassReason ? notPassReason : '识别失败,请上传至其它附件列表')
              resolve(false)
            } else {
              // 识别成功，但是需要判断当前的发票类型是否跟表格的发票类型是否一致
              let isValidInvoice = this._isValidInvoiceType({ tableType, type, extendInfo })
              isValidInvoice ? this.$message({
                type: 'success',
                message: '识别成功'
              }) : this.$message({
                type: 'warning',
                message: '发票归类错误!'
              })
                
              this._setCurrentRow(currentRow, {
                invoiceNo,
                money: amountWithTax,
                isAcc,
                isValidInvoice: true
              })
              resolve(true)
              
            }
          }
        }).catch(err => {
          console.error(err, 'err')
          this._setCurrentRow(currentRow, {
            invoiceNo:'',
            money: '',
            isAcc,
            isValidInvoice: false
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
    openTree(row) {
      let serviceOrderId = row.serviceOrderId
      this.tableLoading = true
      GetDetails(serviceOrderId).then(res => {
        if (res.code == 200) {
          this.dataForm = res.result;
          this.serveId = serviceOrderId
          this.$refs.serviceDetail.open()
          this.tableLoading = false
        }
      }).catch((err) => {
        console.error(err)
        this.tableLoading = false
        this.$message.error('获取服务单详情失败')
      })
    }
  }
}