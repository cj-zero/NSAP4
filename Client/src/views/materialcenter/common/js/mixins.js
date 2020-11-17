export const quotationTableMixin = {
  data () {
    return {
      textMap: Object.freeze({
        create: '新建',
        edit: '编辑',
        approve: '审批'
      })
    }
  },
  updated () {
    console.log('updated')
  }
}
export const quotationOrderMixin = {
  data () {
    return {
      invoiceCompany: [
        { label: '新威尔', value: 1 },
        { label: '新能源', value: 2 },
        { label: '东莞新威', value: 3 }
      ],
      deliveryMethod: [
        { label: '货到发款', value: 1 },
        { label: '先票后款', value: 2 }
      ],
      createUser: this.$store.state.user.name
    }
  },
  computed: {
    formConfig () { // 头部表单配置
      return [
        { label: '服务ID', prop: 'serviceOrderSapId', placeholder: '请选择', col: 6, readonly: true },
        { label: '客户代码', prop: 'terminalCustomerId', placeholder: '请选择', col: 6, disabled: true },
        { label: '客户名称', prop: 'terminalCustomer', placeholder: '请选择', col: 12, disabled: true, isEnd: true },
        { label: '开票地址', prop: 'shippingAddress', placeholder: '请选择', col: 18 },
        { label: '开票单位', prop: 'invoiceCompany', placeholder: '请选择', col: 6, type: 'select', options: this.invoiceCompany, isEnd: true },
        { label: '收货地址', prop: 'collectionAddress', placeholder: '请选择', col: 18 },
        { label: '发货方式', prop: 'deliveryMethod', placeholder: '请选择', col: 6, type: 'select', options: this.deliveryMethod, isEnd: true },
        { label: '备注', prop: 'remark', placeholder: '请填写', col: 18 }
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
    materialConfig () {
      return [
        { label: '序号', type: 'order' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '数量', prop: 'count', type: 'number' },
        { label: '最大数量', prop: 'maxCount' },
        { label: '单价', prop: 'unitPrice' },
        { label: '总计', prop: 'totalPrice', disabled: true },
        { label: '备注', prop: 'remark', type: 'input' },
        { label: '操作', type: 'operation', iconList: [{ handleClick: this.deleteMaterialItem, icon: 'el-icon-delete' }] }
      ]       
    }
  }
}

