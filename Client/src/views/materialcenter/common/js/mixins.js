import { getCategoryNameList } from '@/api/directory'
import { getQuotationDetail } from '@/api/material/quotation'
import { GetDetails } from '@/api/serve/callservesure'
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
      categoryList: [] // 分类列表
    }
  },
  methods: {
    _getQuotationDetail (data) {
      let quotationId
      let { status } = data
      if (status === 'view') {
        quotationId = data.id
      } else {
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
      let { serviceOrders, quotations } = data
      let { terminalCustomer, terminalCustomerId, salesMan } = serviceOrders
      // result
      return { ...quotations, terminalCustomer, terminalCustomerId, salesMan }
    },
    _normalizeList (list) { // 格式化表格数据
      return list.map(item => {
        item.quotationStatus = this.quotationStatusMap[item.quotationStatus]
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
    isMaterialTreasurer () { // 判断是不是物料财务
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '物料财务')
        : false
    },
    formConfig () { // 头部表单配置
      return this.status === 'return' || this.isReturn
        ? [
            { label: '服务ID', prop: 'serviceOrderSapId', placeholder: '请选择', col: 6, readonly: true },
            { label: '客户代码', prop: 'terminalCustomerId', placeholder: '请选择', col: 6, disabled: true },
            { label: '客户名称', prop: 'terminalCustomer', placeholder: '请选择', col: 12, disabled: true, isEnd: true },
          ]
        : [
            { label: '服务ID', prop: 'serviceOrderSapId', placeholder: '请选择', col: 6, readonly: true, disabled: !this.ifEdit },
            { label: '客户代码', prop: 'terminalCustomerId', placeholder: '请选择', col: 6, disabled: true },
            { label: '客户名称', prop: 'terminalCustomer', placeholder: '请选择', col: 12, disabled: true, isEnd: true },
            { label: '开票地址', prop: 'shippingAddress', placeholder: '请选择', col: 18, disabled: !this.ifEdit },
            { label: '开票单位', prop: 'invoiceCompany', placeholder: '请选择', col: 6, 
              type: 'select', options: this.invoiceCompanyList, isEnd: true, disabled: this.isMaterialTreasurer },
            { label: '收货地址', prop: 'collectionAddress', placeholder: '请选择', col: 18, disabled: !this.ifEdit },
            { label: '发货方式', prop: 'deliveryMethod', placeholder: '请选择', col: 6, type: 'select', options: this.deliveryMethodList, disabled: !this.ifEdit, isEnd: true },
            { label: '备注', prop: 'remark', placeholder: '请填写', col: 18, disabled: !this.ifEdit },
            { label: '总计', type: 'money', col: 6 }
          ]
    },
    formatFormConfig () {
      let noneSlotConfig = this.formConfig
      let result = [], j = 0
      for (let i = 0; i < noneSlotConfig.length; i++) {
        if (!result[j]) {
          result[j] = []
        }
        result[j].push(noneSlotConfig[i])
        if (noneSlotConfig[i].isEnd) {
          j++
        }
      }
      return result
    },
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

export const chatMixin = { // 服务单
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
      let { serviceWorkOrders } = data
      if (serviceWorkOrders && serviceWorkOrders.length) {
        serviceWorkOrders.forEach(serviceOrder => {
          let { warrantyEndDate, bookingDate, visitTime, liquidationDate, completeDate } = serviceOrder
          serviceOrder.warrantyEndDate = this.deleteSeconds(warrantyEndDate)
          serviceOrder.bookingDate = this.deleteSeconds(bookingDate)
          serviceOrder.visitTime = this.deleteSeconds(visitTime)
          serviceOrder.liquidationDate = this.deleteSeconds(liquidationDate)
          serviceOrder.completeDate = this.deleteSeconds(completeDate)
        })
      }
      return data
    },
  }
}



