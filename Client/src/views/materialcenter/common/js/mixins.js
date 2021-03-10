import { getCategoryNameList } from '@/api/directory'
import { getQuotationDetail } from '@/api/material/quotation' // 报价单详情
// import { GetDetails } from '@/api/serve/callservesure' // 服务单
import { getReturnNoteList, getReturnNoteDetail } from '@/api/material/returnMaterial' // 退料
import { normalizeFormConfig } from '@/utils/format'
import { processDownloadUrl } from '@/utils/file'
import { chatMixin } from '@/mixins/serve'
export { chatMixin }
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
        pay: '审批'
      }),
      isReceive: true, // 用来区分  报价单->true /(待处理、销售订单模块->false)
      categoryList: [], // 分类列表
      rolesList: this.$store.state.user.userInfoAll.roles, // 当前用户的角色列表
      originUserId: this.$store.state.user.userInfoAll.userId // 当前用户的ID
    }
  },
  computed: {
    isMaterialFinancial () { // 判断是不是物料财务
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '物料财务')
        : false
    }
  },
  methods: {
    _openServiceOrder (row) {
      this.openServiceOrder(row.serviceOrderId, () => this.tableLoading = true, () => this.tableLoading = false)
    },
    _getQuotationDetail (data) {
      let quotationId
      let { status, isReceive, isSalesOrder, quotationStatus } = data
      this.isReceive = !!isReceive // 判断销售订单还是报价单
      if (isSalesOrder && !data.salesOrderId) {
        return this.$message.warning('无销售单号')
      }
       if (status !== 'create') { // 要么编辑要么查看报价单
        quotationId = data.id
        console.log(quotationStatus, 'quotationStatus')
        quotationStatus = +quotationStatus
        this.isShowEditBtn = true
        if (quotationStatus >= 4) { // 4开始就已经提交物料单 不可编辑 销售订单已经成立
          status = 'view'
          this.isShowEditBtn = false // 新建报价单页面，判断是不是可以编辑报价单
          if (this.isSales) {
            if (quotationStatus === 4) { // 技术员回传文件
              status = 'upload'
              console.log(quotationStatus)
            } else if (quotationStatus === 5) { // 财务审批阶段
              status = 'pay'
              console.log(quotationStatus)
            }
          }
          if (this.isOutbound) {
            status = 'outbound'
          }
        } else {
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
        quotationId
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

const IDS = ['SYS_QuotationStatus', 'SYS_InvoiceCompany', 'SYS_DeliveryMethod', 'SYS_MaterialDiscount', 'SYS_AcquisitionWay', 'SYS_MoneyMeans']
const SYS_QuotationStatus = 'SYS_QuotationStatus'
const SYS_InvoiceCompany = 'SYS_InvoiceCompany'
const SYS_DeliveryMethod = 'SYS_DeliveryMethod'
const SYS_MaterialDiscount = 'SYS_MaterialDiscount'
const SYS_AcquisitionWay = 'SYS_AcquisitionWay'
const SYS_MoneyMeans = 'SYS_MoneyMeans'
export const categoryMixin = {
  computed: {
    quotationStatusMap () {
      return this.buildMap(this.categoryList.filter(item => item.typeId === SYS_QuotationStatus))
    },
    invoiceCompanyList () {
      return this.buildSelectList(this.categoryList.filter(item => item.typeId === SYS_InvoiceCompany))
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
    buildSelectList (list) {
      return list.map(item => {
        let { name: label, dtValue: value } = item
        return {
          label,
          value
        }
      })
    },
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
    ifEdit () {
      return this.status === 'create' || this.status === 'edit'
    },
    isMaterialFinancial () { // 判断是不是物料财务
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '物料财务')
        : false
    },
    isStorekeeper () { // 判断是不是仓库管理人员
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '仓库')
        : false
    },
    formItems () { // 头部表单配置
      return [
        { tag: 'text', span: 3, attrs: { prop: 'serviceOrderSapId', readonly: true }, itemAttrs: { prop: 'serviceOrderSapId', label: '服务ID' }, on: { focus: this.onServiceIdFocus }},
        { tag: 'text', span: 3, attrs: { prop: 'terminalCustomerId', disabled: true }, itemAttrs: { prop: 'terminalCustomerId', label: '客户代码' } },
        { tag: 'text', span: 6, attrs: { prop: 'terminalCustomer', disabled: true }, itemAttrs: { prop: 'terminalCustomer', label: '客户名称' } },
        { tag: 'text', span: 3, attrs: { prop: 'newestContacter', disabled: true }, itemAttrs: { prop: 'newestContacter', label: '联系人',  } },
        { tag: 'text', span: 3, attrs: { prop: 'newestContactTel', disabled: true }, itemAttrs: { prop: 'newestContactTel', label: '电话', 'label-width': '80px' } },
        { tag: 'date', span: 3, attrs: { prop: 'deliveryDate', disabled: !this.ifEdit, 'value-format': 'yyyy-MM-dd' }, itemAttrs: { prop: 'deliveryDate', label: '交货日期' } },
        { tag: 'number', span: 3, attrs: { prop: 'acceptancePeriod', disabled: !this.ifEdit, min: 7, max: 30, controls: false }, itemAttrs: { prop: 'acceptancePeriod', label: '验收期限' }, isEnd: true },
        { tag: 'area', span: 12, attrs: { prop: 'shippingAddress', disabled: !this.ifEdit }, itemAttrs: { prop: 'shippingAddress', label: '客户地址' } },
        { tag: 'select', span: 3, attrs: { prop: 'acquisitionWay', disabled: !this.ifEdit, options: this.acquisitionWayList }, itemAttrs: { prop: 'acquisitionWay', label: '领料方式' } },
        { tag: 'select', span: 3, attrs: { prop: 'moneyMeans', disabled: !this.ifEdit, options: this.moneyMeansList }, itemAttrs: { prop: 'moneyMeans', label: '业务伙伴货币', 'label-width': '80px' } },
        { tag: 'select', span: 3, attrs: { prop: 'invoiceCompany', disabled: !this.ifEdit, options: this.invoiceCompanyList }, 
          itemAttrs: { prop: 'invoiceCompany', label: '开票单位' } },
        { tag: 'select', span: 3, attrs: { prop: 'deliveryMethod', disabled: !this.ifEdit, options: this.deliveryMethodList,  }, itemAttrs: { prop: 'deliveryMethod', label: '付款条件' }, isEnd: true },
        { tag: 'area', span: 12, attrs: { prop: 'collectionAddress', disabled: !this.ifEdit }, itemAttrs: { prop: 'collectionAddress', label: '交货地址' } },
        { tag: 'text', span: 12, attrs: { prop: 'remark', disabled: !this.ifEdit }, itemAttrs: { prop: 'remark', label: '备注' } }
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
      formItems: [
        { span: 4, attrs: { prop: 'U_SAP_ID' }, itemAttrs: { label: '服务ID' } },
        { span: 4, attrs: { prop: 'CustomerId' }, itemAttrs: { label: '客户代码' } },
        { span: 8, attrs: { prop: 'CustomerName' }, itemAttrs: { label: '客户名称' } },
        { span: 4, attrs: { prop: 'contact' }, itemAttrs: { label: '联系人' } },
        { span: 4, attrs: { prop: 'number' }, itemAttrs: { label: '电话' } },
      ],
      returnOrderColumns: [
        { label: '退料单号', prop: 'id', handleClick: this._getReturnNoteDetail, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createDate' },
        { label: '总金额', prop: 'totalMoney', slotName: 'totalMoney', align: 'right' },
        { label: '备注', prop: 'remark' },
        // { label: '状态', slotName: 'status' }
      ],
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
    }
  },
  methods: {
    _getList () { // 获取涂料单列表信息
      this.tableLoading = true
      getReturnNoteList(this.listQuery).then(res => {
        let { count, data } = res
        this.tableData = data
        this.total = count
        this.tableLoading = false
        this.$refs.returnOrderTable.resetCurrentRow()
        console.log('_getList', this.$refs.returnOrderTable.getCurrentRow())
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    _getReturnNoteDetail (data) { // 获取退料单详情
      let id
      let { status } = data
      id = data.id
      console.log(status, 'status', id)
      this.tableLoading = true
      getReturnNoteDetail({
        id
      }).then(res => {
        this.detailInfo = this._normalizeDetail(res.data)
        this.$refs.returnOrderDialog.open()
        this.tableLoading = false
        this.status = status
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    _normalizeDetail (data) {
      let { expressList, mainInfo, materialList } = data
      expressList.forEach(expressInfo => {
        let { id, expressInformation } = expressInfo
        let subMaterialList = materialList.filter(item => item.key === id)[0].detail
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