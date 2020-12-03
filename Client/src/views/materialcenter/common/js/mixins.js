import { getCategoryNameList } from '@/api/directory'
import { getQuotationDetail } from '@/api/material/quotation' // 报价单详情
import { GetDetails } from '@/api/serve/callservesure' // 服务单
import { getReturnNoteList, getReturnNoteDetail } from '@/api/material/returnMaterial' // 退料
import { normalizeFormConfig } from '@/utils/format'
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
        view: '查看',
        approve: '审批',
        pay: '支付'
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
    _getQuotationDetail (data) {
      let quotationId
      let { status, isReceive, isSalesOrder } = data
      this.isReceive = !!isReceive
      if (isSalesOrder && !data.salesOrderId) {
        return this.$message.warning('无销售单号')
      }
       if (status === 'view') {
        quotationId = data.id
      } else {
        let currentRow = this.$refs.quotationTable.getCurrentRow()
        if (!currentRow) {
          return this.$message.warning('请先选择数据')
        }
        let { quotationStatus } = currentRow
        quotationStatus = +quotationStatus
        if (status === 'edit') {
          if (quotationStatus > 3) {
            return this.$message.warning('当前状态不可编辑')
          }
        } else if (status === 'pay') { // 财务付款审批
          console.log('pay')
          if (quotationStatus === 9 && this.isMaterialFinancial) {
            return this.$message.warning('需要客户签字')
          } else if (!this.isMaterialFinancial && quotationStatus === 10) {
            return this.$message.warning('当前状态不可审批')
          }
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
      let { terminalCustomer, terminalCustomerId, salesMan } = serviceOrders
      // result
      return { ...quotations, terminalCustomer, terminalCustomerId, salesMan, quotationMergeMaterials, expressages }
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

const IDS = ['SYS_QuotationStatus', 'SYS_InvoiceCompany', 'SYS_DeliveryMethod']
const SYS_QuotationStatus = 'SYS_QuotationStatus'
const SYS_InvoiceCompany = 'SYS_InvoiceCompany'
const SYS_DeliveryMethod = 'SYS_DeliveryMethod'
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
    }
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
  computed: {
    ifEdit () {
      return this.status === 'create' || this.status === 'edit'
    },
    isMaterialFinancial () { // 判断是不是物料财务
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '物料财务')
        : false
    },
    formConfig () { // 头部表单配置
      return this.isOutbound || this.status === 'outbound' // 出库单
        ? [
        { label: '服务ID', prop: 'serviceOrderSapId', placeholder: '请选择', col: 6, readonly: true, disabled: !this.ifEdit },
        { label: '客户代码', prop: 'terminalCustomerId', placeholder: '请选择', col: 6, disabled: true },
        { label: '客户名称', prop: 'terminalCustomer', placeholder: '请选择', col: 12, disabled: true, isEnd: true },
        { label: '开票地址', prop: 'shippingAddress', placeholder: '请选择', col: 24, disabled: !this.ifEdit, isEnd: true },
        { label: '收货地址', prop: 'collectionAddress', placeholder: '请选择', col: 24, disabled: !this.ifEdit, isEnd: true },
        { label: '备注', prop: 'remark', placeholder: '请填写', col: 24, disabled: !this.ifEdit, isEnd: true },
      ]
      : this.isSales || this.status === 'pay' ? // 销售单
      [
        { label: '服务ID', prop: 'serviceOrderSapId', placeholder: '请选择', col: 6, readonly: true, disabled: !this.ifEdit },
        { label: '客户代码', prop: 'terminalCustomerId', placeholder: '请选择', col: 6, disabled: true },
        { label: '客户名称', prop: 'terminalCustomer', placeholder: '请选择', col: 12, disabled: true, isEnd: true },
        { label: '开票地址', prop: 'shippingAddress', placeholder: '请选择', col: 18, disabled: !this.ifEdit },
        { label: '开票单位', prop: 'invoiceCompany', placeholder: '请选择', col: 6, 
          type: 'select', options: this.invoiceCompanyList, isEnd: true, disabled: true },
        { label: '收货地址', prop: 'collectionAddress', placeholder: '请选择', col: 18, disabled: !this.ifEdit },
        { label: '发货方式', prop: 'deliveryMethod', placeholder: '请选择', col: 6, type: 'select', options: this.deliveryMethodList, disabled: !this.ifEdit, isEnd: true },
        { label: '备注', prop: 'remark', placeholder: '请填写', col: 18, disabled: !this.ifEdit },
      ] 
      :
      [ // 报价单
        { label: '服务ID', prop: 'serviceOrderSapId', placeholder: '请选择', col: 6, readonly: true, disabled: !this.ifEdit },
        { label: '客户代码', prop: 'terminalCustomerId', placeholder: '请选择', col: 6, disabled: true },
        { label: '客户名称', prop: 'terminalCustomer', placeholder: '请选择', col: 12, disabled: true, isEnd: true },
        { label: '开票地址', prop: 'shippingAddress', placeholder: '请选择', col: 18, disabled: !this.ifEdit },
        { label: '开票单位', prop: 'invoiceCompany', placeholder: '请选择', col: 6, 
          type: 'select', options: this.invoiceCompanyList, isEnd: true, disabled: this.status === 'view' || this.status === 'pay' || (!this.isMaterialFinancial && this.status === 'approve') },
        { label: '收货地址', prop: 'collectionAddress', placeholder: '请选择', col: 18, disabled: !this.ifEdit },
        { label: '发货方式', prop: 'deliveryMethod', placeholder: '请选择', col: 6, type: 'select', options: this.deliveryMethodList, disabled: !this.ifEdit, isEnd: true },
        { label: '备注', prop: 'remark', placeholder: '请填写', col: 18, disabled: !this.ifEdit },
        { label: '总计', type: 'money', col: 6 }
      ] 
      
    },
    returnFormConfig () { // 退料单表单
      return this.isReturn 
        ? [
            { label: '客户代码', prop: 'terminalCustomerId', placeholder: '请选择', col: 8, disabled: true },
            { label: '客户名称', prop: 'terminalCustomer', placeholder: '请选择', col: 16, disabled: true, isEnd: true },
          ]
        : [
            { label: '客户代码', prop: 'terminalCustomerId', col: 8, disabled: true },
            { label: '客户名称', prop: 'terminalCustomer', col: 16, disabled: true, isEnd: true },
            // { label: '退货备注', prop: 'terminalCustomer', placeholder: '请输入', col: 24, disabled: true, isEnd: true },
            { label: '签收备注', prop: 'remark', placeholder: '请输入内容', col: 24, disabled: this.status !== 'toReturn' , isEnd: true },
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
      invoiceCompany: [
        { label: '新威尔', value: '1' },
        { label: '新能源', value: '2' },
        { label: '东莞新威', value: '3' }
      ],
      deliveryMethod: [
        { label: '货到发款', value: 1 },
        { label: '先票后款', value: 2 }
      ],
      createUser: this.$store.state.user.name
    }
  },
  // computed: {
  //   materialConfig () {
  //     return [
  //       { label: '序号', type: 'order' },
  //       { label: '物料编码', prop: 'materialCode' },
  //       { label: '物料描述', prop: 'materialDescription' },
  //       { label: '数量', prop: 'count', type: 'number', align: 'right' },
  //       { label: '最大数量', prop: 'maxCount', align: 'right' },
  //       { label: '单价', prop: 'unitPrice', align: 'right' },
  //       { label: '总计', prop: 'totalPrice', disabled: true, align: 'right' },
  //       { label: '备注', prop: 'remark', type: 'input' },
  //       { label: '操作', type: 'operation', iconList: [{ handleClick: this.deleteMaterialItem, icon: 'el-icon-delete' }] }
  //     ]       
  //   }
  // }
}

export const chatMixin = { // 服务单详情
  data () {
    return {
      serveId: '',
      dataForm: {}, //传递的表单props

    }
  },
  methods: {
    _openServiceOrder(row) {
       // 判断服务单详情是否在表格页面打开,否则就是在报销单查看页面打开  isOpenInTable
      let serviceOrderId = row.serviceOrderId
      let isInTable = row.isInTable
      if (!serviceOrderId) {
        return this.$message.error('无服务单ID')
      }
      isInTable ? this.tableLoading = true : this.contentLoading = true
      GetDetails(serviceOrderId).then(res => {
        if (res.code == 200) {
          this.dataForm = this._normalizeOrderDetail(res.result);
          this.serveId = serviceOrderId
          this.$refs.serviceDetail.open()
          isInTable ? this.tableLoading = false : this.contentLoading = false
        }
      }).catch((err) => {
        console.error(err)
        isInTable ? this.tableLoading = false : this.contentLoading = false
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
      returnOrderColumns: [
        { label: '退料单号', prop: 'id', handleClick: this._getReturnNoteDetail, options: { status: 'view' }, type: 'link'},
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link', options: { isInTable: true } },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createDate' },
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
      if (status === 'view') {
        id = data.id
      } else {
        let currentRow = this.$refs.returnOrderTable.getCurrentRow()
        console.log(currentRow, 'currentRow')
        if (!currentRow) {
          return this.$message.warning('请先选择数据')
        }
        id = currentRow.id
      }
      console.log(status, 'status', id)
      this.tableLoading = true
      getReturnNoteDetail({
        id
      }).then(res => {
        console.log(res,' res')
        this.detailInfo = res.data
        // this.detailInfo = this._normalizeDetail(res.data)
        this.$refs.returnOrderDialog.open()
        this.tableLoading = false
        this.status = status
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
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
      this.listQuery.page = 1
      this._getList()
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
      this.onSearch()
    },
  }
}

