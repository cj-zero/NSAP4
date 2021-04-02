import { getCategoryNameList } from '@/api/directory'
import { getQuotationDetail } from '@/api/material/quotation' // 报价单详情
// import { GetDetails } from '@/api/serve/callservesure' // 服务单
import { getReturnNoteListByExpress, getReturnNoteDetailByExpress } from '@/api/material/returnMaterial' // 退料
import { normalizeFormConfig } from '@/utils/format'
import { processDownloadUrl } from '@/utils/file'
import { isMatchRole } from '@/utils/utils'
import { chatMixin } from '@/mixins/serve'
export { chatMixin }
// import { noop } from '@/utils/declaration'
const statusMap = {
  4: 'approve',
  5: 'approve',
  6: 'custom', // 客户确认
  7: 'upload', // 技术员回传文件并且是保外
  8: 'pay', // 财务审批阶段
  9: 'approveSales' // 总经理审批阶段
}
export const quotationTableMixin = {
  provide () {
    return {
      parentVm: this
    }
  },
  data () {
    return {
      textMap: Object.freeze({
        create: '新建',
        edit: '编辑',
        upload: '审批',
        view: '查看',
        approve: '审批',
        pay: '审批',
        approveSales: '审批'
      }),
      isView: false, // 判断是不是查看的方式打开物料单弹窗
      isReceive: true, // 用来区分  报价单->true /(待处理、销售订单模块->false)
      categoryList: [], // 分类列表
      rolesList: this.$store.state.user.userInfoAll.roles, // 当前用户的角色列表
      originUserId: this.$store.state.user.userInfoAll.userId, // 当前用户的ID
      quotationStatus: -1 // 报价单状态
    }
  },
  computed: {
    dialogTitle () {
      return this.quotationStatus === 6 ? '审批' : (this.isView ? '查看' : this.textMap[this.status])
    }
  },
  methods: {
    _openServiceOrder (row) {
      this.openServiceOrder(row.serviceOrderId, () => this.tableLoading = true, () => this.tableLoading = false)
    },
    _getQuotationDetail (data) {
      let quotationId
      let { status, isReceive, isSalesOrder, quotationStatus, isView, isProtected, isUpdate } = data
      this.isReceive = !!isReceive // 判断销售订单还是报价单
      this.hasEditBtn = !!data.hasEditBtn
      this.isSalesOrder = !!isSalesOrder
      this.isView = !!isView
      this.isProtected = !!isProtected
      if (isSalesOrder && !data.salesOrderId) {
        return this.$message.warning('无销售单号')
      }
      this.quotationStatus = +quotationStatus
       if (status !== 'create') { // 要么编辑要么查看报价单d
        quotationId = data.id
        quotationStatus = +quotationStatus
        this.isShowEditBtn = true
        if (quotationStatus >= 4) { // 4开始就已经提交物料单 不可编辑 销售订单已经成立
          status = 'view'
          this.isShowEditBtn = false // 新建报价单页面，判断是不是可以编辑报价单
          status = statusMap[quotationStatus]
          if (this.isOutbound) { // 出库状态
            status = 'outbound'
          }
        } else if (quotationStatus <= 3) { // 未提交 撤回 驳回 阶段
          // 进行编辑报价物料单的
          // this.status = 'edit'
          status = 'edit'
        }
      } else {
        // 创建物料报价单
        this.isShowEditBtn = false
        let currentRow = this.$refs.quotationTable.getCurrentRow()
        if (!currentRow) {
          return this.$message.warning('请先选择数据')
        }
        quotationId = currentRow.id
      }
      console.log(status, 'status', quotationId)
      this.tableLoading = true
      getQuotationDetail({
        quotationId,
        isUpdate
      }).then(res => {
        console.log(res,' res')
        this.detailInfo = this._normalizeDetail(res.data)
        this.$refs.quotationDialog.open()
        this.tableLoading = false
        this.status = status
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    _normalizeDetail (data) {
      let { serviceOrders, quotations, quotationMergeMaterials = [], expressages } = data
      let quotationPictures = quotations.quotationPictures || [] // 技术员回传合同图片
      quotationPictures = quotationPictures.map(pictureItem => {
        let { id, pictureId } = pictureItem
        return processDownloadUrl(pictureId || id)
      })
      // result
      return { ...serviceOrders, ...quotations,  quotationMergeMaterials, expressages, quotationPictures }
    },
    _normalizeList (list) { // 格式化表格数据
      return list.map(item => {
        item.quotationStatusText = this.quotationStatusMap[item.quotationStatus]
        return item
      })
    },
    onSearch () {
      this.listQuery.page = 1
      this._getList()
    },
    onChangeForm (val) {
      this.listQuery.page = 1
      Object.assign(this.listQuery, val)
    },
    handleCurrentChange ({ page, limit }) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    }
  }
}

const IDS = ['SYS_QuotationStatus', 'SYS_InvoiceCompany', 'SYS_DeliveryMethod', 'SYS_MaterialDiscount', 
'SYS_AcquisitionWay', 'SYS_MoneyMeans', 'SYS_MaterialsCommonlyRejected', 'SYS_MaterialTaxRate', 'SYS_MaterialInvoiceCategory']
const SYS_QuotationStatus = 'SYS_QuotationStatus'
const SYS_InvoiceCompany = 'SYS_InvoiceCompany'
const SYS_DeliveryMethod = 'SYS_DeliveryMethod'
const SYS_MaterialDiscount = 'SYS_MaterialDiscount'
const SYS_AcquisitionWay = 'SYS_AcquisitionWay'
const SYS_MoneyMeans = 'SYS_MoneyMeans'
const SYS_MaterialsCommonlyRejected = 'SYS_MaterialsCommonlyRejected'
const SYS_MaterialInvoiceCategory = 'SYS_MaterialInvoiceCategory' // 发票类别
const SYS_MaterialTaxRate = 'SYS_MaterialTaxRate' // 税率
export const categoryMixin = {
  computed: {
    reimburseTagList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_MaterialsCommonlyRejected), true)
    },
    quotationStatusMap () {
      return this.buildMap(this.categoryList.filter(item => item.typeId === SYS_QuotationStatus))
    },
    invoiceCompanyList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_InvoiceCompany))
    },
    invoiceCategoryList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_MaterialInvoiceCategory))
    },
    taxRateList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_MaterialTaxRate))
    },
    deliveryMethodList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_DeliveryMethod))
    },
    discountList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_MaterialDiscount))
    },
    acquisitionWayList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_AcquisitionWay))
    },
    moneyMeansList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_MoneyMeans))
    },
  },
  methods: {
    _getCategoryNameList () { // 获取字典分类信息
      getCategoryNameList({
        ids: IDS
      }).then(res => {
        this.categoryList = res.data
        console.log(res, 'res')
      })
    },
    buildMap (list) {
      let result = {}
      console.log(list, 'list')
      for (let i = 0; i < list.length; i++) {
        let { name, dtValue } = list[i]
        result[dtValue] = name
      } 
      return result
    },
    buildSelectList (list, isName = false ) {
      return list.map(item => {
        let { name: label, dtValue: value } = item
        return {
          label,
          value: isName ? label : value
        }
      })
    },
  }
}
export const rolesMixin = {
  data () {
    return {
      rolesList: this.$store.state.user.userInfoAll.roles, // 当前用户的角色列表
      isCustomerServiceSupervisor: isMatchRole('客服主管'),
      isMaterialFinancial: isMatchRole('物料财务'),
      isStorekeeper: isMatchRole('仓库'),
      isTechnical: isMatchRole('售后技术员'),
      isGeneralManager: isMatchRole('总经理'),
      isMaterialsEngineer: isMatchRole('物料工程审批')
    }
  }
}

export const configMixin = { // 表单配置
  data () {
    return {
      rolesList: this.$store.state.user.userInfoAll.roles, // 当前用户的角色列表
    }
  },
  methods: {
    processValue ({ province, city, district }) {
      return `${province}--${city}--${district}`
    }
  },
  computed: {
    ifEdit () { // 表单是否可以编辑
      return this.status === 'create' || (this.status === 'edit' && this.hasEditBtn)
    },
    formItems () { // 头部表单配置
      return [
        // { tag: 'text', span: 3, slotName: 'serviceOrderSapId', attrs: { prop: 'serviceOrderSapId', readonly: true }, itemAttrs: { prop: 'serviceOrderSapId', label: '服务ID' }, on: { focus: this.onServiceIdFocus }},
        { span: 3, slotName: 'serviceOrderSapId' },
        { tag: 'text', span: 3, attrs: { prop: 'terminalCustomerId', disabled: true }, itemAttrs: { prop: 'terminalCustomerId', label: '客户代码' } },
        { tag: 'text', span: 6, attrs: { prop: 'terminalCustomer', disabled: true }, itemAttrs: { prop: 'terminalCustomer', label: '客户名称' } },
        { tag: 'text', span: 4, attrs: { prop: 'newestContacter', disabled: true }, itemAttrs: { prop: 'newestContacter', label: '联系人',  } },
        { tag: 'text', span: 4, attrs: { prop: 'newestContactTel', disabled: true }, itemAttrs: { prop: 'newestContactTel', label: '电话' } },
        { tag: 'select', span: 4, attrs: { prop: 'acquisitionWay', disabled: !this.ifEdit, options: this.acquisitionWayList }, itemAttrs: { prop: 'acquisitionWay', label: '领料方式' }, isEnd: true },
        { tag: 'area', span: 6, attrs: { prop: 'shippingAddress', disabled: !this.ifEdit }, itemAttrs: { prop: 'shippingAddress', label: '客户地址' } },
        { tag: 'text', span: 6, attrs: { prop: 'shippingDA', disabled: !this.ifEdit }, itemAttrs: { prop: 'shippingDA', label: '详细地址' } },
        { tag: 'date', span: 4, attrs: { prop: 'deliveryDate', disabled: !this.ifEdit, 'value-format': 'yyyy-MM-dd', format: 'yyyy.MM.dd' }, itemAttrs: { prop: 'deliveryDate', label: '交货日期' } },
        { tag: 'number', span: 4, attrs: { prop: 'acceptancePeriod', disabled: !this.ifEdit, min: 7, max: 30, controls: false }, itemAttrs: { prop: 'acceptancePeriod', label: '验收期限' } },
        { tag: 'select', span: 4, attrs: { prop: 'moneyMeans', disabled: !this.ifEdit, options: this.moneyMeansList }, itemAttrs: { prop: 'moneyMeans', label: '业务货币' }, isEnd: true },
        
        { tag: 'area', span: 6, attrs: { prop: 'collectionAddress', disabled: !this.ifEdit }, itemAttrs: { prop: 'collectionAddress', label: '交货地址' } },
        { tag: 'text', span: 6, attrs: { prop: 'collectionDA', disabled: !this.ifEdit }, itemAttrs: { prop: 'collectionDA', label: '详细地址' } },
        { tag: 'select', span: 8, attrs: { prop: 'invoiceCompany', disabled: !this.ifEdit, options: this.invoiceCompanyList }, 
          itemAttrs: { prop: 'invoiceCompany', label: '开票单位' } },
        { tag: 'select', span: 4, attrs: { prop: 'taxRate', disabled: !this.ifEdit, options: this.taxRateList }, 
          itemAttrs: { prop: 'taxRate', label: '税率' }, isEnd: true },
        { tag: 'text', span: 12, attrs: { prop: 'remark', disabled: !this.ifEdit }, itemAttrs: { prop: 'remark', label: '备注' } },
        { tag: 'select', span: 8, attrs: { prop: 'deliveryMethod', disabled: !this.ifEdit, options: this.deliveryMethodList,  }, itemAttrs: { prop: 'deliveryMethod', label: '付款条件' }, on: { change: this.onDeliveryMethodChange } },
        { tag: 'select', span: 4, attrs: { prop: 'invoiceCategory', disabled: !this.ifEdit, options: this.invoiceCategoryList,  }, itemAttrs: { prop: 'invoiceCategory', label: '发票类别' }, isEnd: true },
        { tag: 'select', span: 4, attrs: { prop: 'isMaterialType', disabled: !this.ifEdit, options: this.materialTypeList, }, itemAttrs: { prop: 'isMaterialType', label: '物料类型' }, on: { change: this.onFormMaterialTypeChange }}
      ]
    },
    returnFormConfig () { // 退料单表单
      return [
        { label: '服务ID', prop: 'serviceOrderId', col: 4, disabled: true },
        { label: '客户代码', prop: 'terminalCustomerId', col: 4, disabled: true },
        { label: '客户名称', prop: 'terminalCustomer', col: 8, disabled: true },
        { label: '联系人', prop: 'contact', col: 4, disabled: true },
        { label: '电话', prop: 'number', col: 4, disabled: true }
        // { label: '退货备注', prop: 'terminalCustomer', placeholder: '请输入', col: 24, disabled: true, isEnd: true },
        // { label: '签收备注', prop: 'remark', placeholder: '请输入内容', col: 24, disabled: this.status !== 'toReturn' , isEnd: true },
      ]
    },
    formatFormConfig () {
      return normalizeFormConfig(this.formConfig)
    },
    formatReturnConfig () {
      return normalizeFormConfig(this.returnFormConfig)
    }
  }
}

export const quotationOrderMixin = { // 报价单
  data () {
    return {
      createUser: this.$store.state.user.name
    }
  }
}

export const returnTableMixin = { // 退料表格
  data () {
    return {
      formQuery: { //  查询字段
        id: '', // 退料单ID
        customer: '', // 客户
        sapId: '', // 服务Id
        createName: '', // 申请人
        beginDate: '', // 创建开始
        endDate: '' // 创建结束
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
      returnOrderColumns: [
        { label: '退料单号', prop: 'id', handleClick: this._getReturnNoteDetail, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createDate' },
        // { label: '总金额', prop: 'totalMoney', slotName: 'totalMoney', align: 'right' },
        { label: '备注', prop: 'remark' },
        { label: '状态', prop: 'statusName' }
      ],
    }
  },
  methods: {
    _normalizeDetail (data) {
      let { expressList, mainInfo, materialList } = data
      const isArray = Array.isArray(expressList)
      expressList = isArray ? expressList : [expressList]

      expressList.forEach(expressInfo => {
        let { id, expressInformation } = expressInfo
        let subMaterialList = isArray ? materialList.filter(item => item.key === id)[0].detail : materialList
        try {
          let infoList = JSON.parse(expressInformation).data
          expressInfo.expressInformation = infoList[infoList.length - 1].context
        } catch (err) {
          expressInfo.expressInformation = ''
        }
        expressInfo.materialList = subMaterialList
      })
      return  { mainInfo, expressList }
    },
    close () { // 关闭弹窗
      this.$refs.returnOrder.resetInfo()
      this.$refs.returnOrderDialog.close()
    },
    handleCurrentChange ({ page, limit }) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    },
    onSearch () {
      this._getList()
    },
    onChangeForm (val) {
      this.listQuery.page = 1
      Object.assign(this.listQuery, val)
    }
  }
}

export const uploadFileMixin = {
  methods: {
    onAccept (file) { // 限制发票文件上传的格式
      let { type } = file
      let imgReg = /^image\/\w+/i
      let isFitType = imgReg.test(type) || type === 'application/pdf'
      return new Promise((resolve, reject) => {
        if (!isFitType) {
          this.$message.error('文件格式只能为图片或者为PDF文件')
          reject(false)
        } else {
          resolve()
        }
      })
    }
  }
}


export const afterReturnMixin = { // 退料之后 (仓库)
  data () {
    return {
      returnOrderColumns: [
        { label: '退料单号', prop: 'id', handleClick: this._getReturnNoteDetail, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '快递单号', prop: 'expressNumber' },
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createDate' },
        // { label: '总金额', prop: 'totalMoney', slotName: 'totalMoney', align: 'right' },
        { label: '备注', prop: 'remark' },
        // { label: '状态', slotName: 'status' }
      ]
    }
  },
  methods: {
    async _getList () {
      this.tableLoading = true
      try {
        const { data, count } = await getReturnNoteListByExpress(this.listQuery)
        this.tableData = data
        this.total = count
        console.log(data, count)
      } catch(err) {
        this.tableData = []
        this.total = 0
        this.$message.error(err.message)
      } finally {
        this.tableLoading = false
        console.log('finally')
      }
    },
    async _getReturnNoteDetail (row) {
      const { expressId } = row
      if (!expressId) {
        return this.$message.warning('无物流ID')
      }
      this.isCreated = false
      this.tableLoading = true
      try {
        const { data } = await getReturnNoteDetailByExpress({ expressageId: expressId })
        console.log(data, 'getDetail')
        this.detailInfo = this._normalizeDetail(data)
        this.$refs.returnOrderDialog.open()
        console.log(data, 'detail')
      } catch (err) {
        this.$message.error(err.message)
      } finally {
        this.tableLoading = false
      }
      
    }
  }
}