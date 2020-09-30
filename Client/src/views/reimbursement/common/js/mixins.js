import rightImg from '@/assets/table/right.png'
import { getReportDetail } from '@/api/serve/callservesure'
import { getCategoryName } from '@/api/reimburse'
import { accommodationConfig } from './config'
import { REIMBURSE_STATUS_MAP, PROJECT_NAME_MAP, RESPONSIBILITY_MAP, RELATIONS_MAP } from './map'
import { EXPENSE_LIST } from './type'
import { toThousands } from '@/utils/format'
import { getList, getDetails } from '@/api/reimburse'
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
        view: '查看'
      },
      columns: [ // 表格配置
        { label: '报销单号', prop: 'id', type: 'link', width: 100, handleJump: this.getDetail },
        { label: '填报日期', prop: 'createTime', width: 150 },
        { label: '报销部门', prop: 'orgName', width: 100 },
        { label: '报销人', prop: 'userName', width: 100 },
        { label: '报销状态', prop: 'remburseStatusText', width: 100 },
        { label: '总金额', prop: 'totalMoney', width: 100 },
        { label: '客户代码', prop: 'terminalCustomerId', width: 100 },
        { label: '客户名称', prop: 'terminalCustomer', width: 170 },
        { label: '客户简称', prop: 'shortCustomerName', width: 100 },
        { label: '业务员', prop: 'saleSMan', width: 100 },
        { label: '出发日期', prop: 'businessTripDate', width: 150 },
        { label: '结束日期', prop: 'endDate', width: 150 },
        { label: '总天数', prop: 'businessTripDays', width: 100 },
        { label: '服务ID', prop: 'serviceOrderSapId', width: 100, type: 'link' },
        { label: '呼叫主题', prop: 'theme', width: 100 },
        { label: '项目名称', prop: 'projectName', width: 100 },
        { label: '服务报告', width: 100, handleClick: this.openReport, btnText: '查看' },
        { label: '责任承担', prop: 'responsibility', width: 100 },
        { label: '劳务关系', prop: 'serviceRelations', width: 100 },
        { label: '备注', prop: 'remark', width: 100 }
      ],
      tableData: [],
      total: 0, // 表格总数量
      dialogLoading: false, 
      formQuery: { // 查询字段参数
        id: '', // 报销单ID
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
      }).catch(() => {
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
        item.projectName = PROJECT_NAME_MAP[item.projectName]
        item.remburseStatusText = REIMBURSE_STATUS_MAP[item.remburseStatus]
        item.responsibility = RESPONSIBILITY_MAP[item.responsibility]
        item.serviceRelations = RELATIONS_MAP[item.serviceRelations]
        item.totalMoney = toThousands(item.totalMoney)
        return item
      })
    },
    getDetail (val) { // 获取服务单详情
      let id
      if (val.type === 'view') { // 如果是点击底部表格里的查看详情
        id = val.id
      } else {
        if (!this.currentRow) { // 编辑审核等操作
          return this.$message({
            type: 'warning',
            message: '请先选择报销单'
          })
        }
        console.log(val, 'val')
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
      console.log(this.currentRow, 'currentRow')
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
        this.title = val.type === 'approve' ? (this.isCustomerSupervisor ? 'approve' : 'view') : val.type
        console.log(this.title, 'title')
        this.$refs.myDialog.open()
      }).catch(() => {
        this.$message.error('获取详情失败')
      })
    },
    _normalizeDetail (data) {
      let { 
        reimburseAttachments,
        // reimburseTravellingAllowances,
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
      // this._buildAttachment(reimburseTravellingAllowances)
      this._buildAttachment(reimburseFares)
      this._buildAttachment(reimburseAccommodationSubsidies)
      this._buildAttachment(reimburseOtherCharges)
    },
    _buildAttachment (data) { // 为了回显，并且编辑 目标是为了保证跟order.vue的数据保持相同的逻辑
      data.forEach(item => {
        let { reimburseAttachments } = item
        // console.log(reimburseAttachments, 'foeEach', item)
        item.invoiceFileList = this.getTargetAttachment(reimburseAttachments, 2)
        item.otherFileList = this.getTargetAttachment(reimburseAttachments, 1)
        item.invoiceAttachment = [],
        item.otherAttachment = []
        item.reimburseAttachments = []
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
    openReport (serviceOrderId, type) {
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
        serviceOrderId
      }).then(res => {
        this.reportData = this._normalizeReportData(res.result.data)
        this.$refs.reportDialog.open()
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
      return data.map(item => {
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
      reimburseStatusMap: REIMBURSE_STATUS_MAP,
      roleName: this.$store.state.user.name // 当前用户角色名字
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
    }
  },
  computed: {
    reimburseTypeList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_ReimburseType))
    },
    reimburseStatusList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_RemburseStatus))
    },
    projectNameList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_ProjectName))
    },
    responsibilityList () {
      return this.buildSelectOptions(this.categoryList.filter(item => item.typeId === SYS_Responsibility))
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
      console.log(this.roleName, 'roleN')
      return this.roleName === '客服主管'
    },
    formConfig () {
      return [
        { label: '报销单号', prop: 'mainId', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '报销人', prop: 'userName', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '部门', prop: 'orgName', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '职位', prop: 'position', palceholder: '请输入内容', disabled: true, col: 6, isEnd: true },
        { label: '服务ID', prop: 'serviceOrderSapId', palceholder: '请选择', col: 6, disabled: this.title !== 'create' },
        { label: '客户代码', prop: 'terminalCustomerId', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '客户名称', prop: 'terminalCustomer', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '客户简称', prop: 'shortCustomerName', palceholder: '最长5个字', col: 6, maxlength: 5, isEnd: true, disabled: this.title === 'view' },
        { label: '出发地点', prop: 'becity', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '到达地点', prop: 'destination', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '出发日期', prop: 'businessTripDate', palceholder: '请输入内容', disabled: true, col: 6, type: 'date', width: 157 },
        { label: '结束日期', prop: 'endDate', palceholder: '请输入内容', disabled: true, col: 6, type: 'date', isEnd: true, width: 157 },
        { 
          label: '报销类别', prop: 'reimburseType', palceholder: '请输入内容', 
          col: 6, type: 'select', options: this.reimburseTypeList, disabled: this.title === 'view' 
        },
        { 
          label: '项目名称', prop: 'projectName', palceholder: '请输入内容',  
          col: 6, type: 'select', options: this.projectNameList, disabled: this.title === 'view'
        },
        { label: '服务报告', prop: 'report',  disabled: true, col: 6, 
          type: 'button', btnText: '服务报告', handleClick: this.openReport
        },
        { label: '报销状态', prop: 'reimburseTypeText', palceholder: '请输入内容', disabled: true, col: 6, isEnd: true },
        { label: '呼叫主题', prop: 'fromTheme', palceholder: '请输入内容', disabled: true, col: 18 },
        { label: '填报时间', prop: 'fillDate', palceholder: '请输入内容', disabled: true, col: 6, isEnd: true },
        // { label: '设备类型', prop: 'materialType', palceholder: '请输入内容', disabled: true, col: 6 },
        // { label: '解决方案', prop: 'solution', palceholder: '请输入内容', disabled: true, col: 6 },
        { label: '费用承担', prop: 'bearToPay', palceholder: '请输入内容', disabled: (!this.isCustomerSupervisor && this.title !== 'approve') || this.title === 'view', 
          col: 6, type: 'select', options: EXPENSE_LIST
        },
        { label: '责任承担', prop: 'responsibility', palceholder: '请输入内容', 
          col: 6, type: 'select', options: this.responsibilityList, disabled: this.title === 'view' 
        },
        { label: '劳务关系', prop: 'serviceRelations', palceholder: '请输入内容',  
          col: 6, type: 'select', options: this.serviceRelationsList, disabled: this.title === 'view' 
        },
        { label: '支付时间', prop: 'payTime', palceholder: '请输入内容', disabled: true, col: 6, isEnd: true },
        { label: '备注', prop: 'remark', palceholder: '请输入内容', disabled: this.title !== 'create', col: 18 },
        { label: '总金额', prop: 'totalMoney', col: 6, isEnd: true, type: 'inline-slot', id: 'money' },
        { label: '附件', prop: 'reimburseAttachments', type: 'slot', id: 'attachment', showLabel: true },
        { label: '出差补贴', prop: 'reimburseTravellingAllowances', type: 'slot', id: 'travel' },
        { label: '交通费用', prop: 'reimburseFares', type: 'slot', id: 'traffic' },
        { label: '住宿补贴', prop: 'reimburseAccommodationSubsidies', type: 'slot', id: 'accommodation' },
        { label: '其他费用', prop: 'reimburseOtherCharges', type: 'slot', id: 'other' }
      ]
    },    
    travelConfig () {
      return [ // 出差配置
        { label: '天数', prop: 'days', type: 'number' },
        { label: '金额', align: 'right', prop: 'money', type: 'input', disabled: true },
        { label: '备注', prop: 'remark', type: 'input' },
        { label: '操作', type: 'operation', iconList: [{ icon: 'el-icon-delete', handleClick: this.delete }] }
      ]
    },
    trafficConfig () {
      return [ // 交通配置
        { label: '序号', type: 'order', width: 60 },
        { label: '交通类型', prop: 'trafficType', type: 'select', options: this.transportTypeList, width: 120 },
        { label: '交通工具', prop: 'transport', type: 'select', options: this.transportationList, width: 120 },
        { label: '出发地', prop: 'from', type: 'input', width: 100 },
        { label: '目的地', prop: 'to', type: 'input', width: 100 },
        { label: '金额', prop: 'money', type: 'number', align: 'right', width: 150 },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', disabled: true, type: 'input', prop: 'invoiceNumber', width: 100 },
        { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
        { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
        { label: '操作', type: 'operation', iconList: this.iconList, width: 160 }
      ]
    },
    accommodationConfig () {
      return [...accommodationConfig, { label: '操作', type: 'operation', iconList: this.iconList, width: 160 }]
    }, 
    otherConfig () {
      return [ // 其他配置
        { label: '序号', type: 'order', width: 60 },
        { label: '费用类别', prop: 'expenseCategory', type: 'select', width: 150, options: this.otherExpensesList },
        { label: '其他费用', prop: 'money', type: 'number', width: 120, align: 'right' },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', disabled: true, type: 'input', prop: 'invoiceNumber', width: 100 },
        { label: '发票附件', type: 'upload', prop: 'invoiceAttachment', width: 150 },
        { label: '其他附件', type: 'upload', prop: 'otherAttachment', width: 150 },
        { label: '操作', type: 'operation', iconList: this.iconList, width: 160 }
      ]
    },
    commonSearch () { // 搜索配置
      return [
        { placeholder: '报销单号', prop: 'id', width: 100 },
        { placeholder: '报销人', prop: 'createUserName', width: 100 },
        { placeholder: '客户代码/名称', prop: 'terminalCustomer', width: 150 },
        { placeholder: '服务ID', prop: 'serviceOrderId', width: 100 },
        { placeholder: '报销部门', prop: 'orgName', width: 100 },
        { placeholder: '费用承担', prop: 'bearToPay', width: 100, type: 'select', options: this.responsibilityList },
        { placeholder: '责任承担', prop: 'responsibility', width: 100, type: 'select', options: this.serviceRelationsList },
        { placeholder: '填报起始时间', prop: 'staticDate', type: 'date', width: 150 },
        { placeholder: '填报结束事件', prop: 'endDate', type: 'date', width: 150 }
      ]
    }
  }
}


export const attachmentMixin = {
  methods: {
    _buildAttachment (data, isImport = false) { // 为了回显，并且编辑 目标是为了保证跟order.vue的数据保持相同的逻辑
      data.forEach(item => {
        let { reimburseAttachments } = item
        // console.log(reimburseAttachments, 'foeEach', item)
        item.invoiceFileList = this.getTargetAttachment(reimburseAttachments, 2, isImport)
        item.otherFileList = this.getTargetAttachment(reimburseAttachments, 1, isImport)
        item.invoiceAttachment = [],
        item.otherAttachment = []
        item.reimburseAttachments = []
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
            item.id = 0
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
          item.id = ''
        }
        item.reimburseAttachments = [...invoiceAttachment, ...otherAttachment, ...invoiceFileList, ...otherFileList]
      })
    }
  }
}