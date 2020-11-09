export const quotationOrderMixin = {
  computed: {
    formConfig () { // 头部表单配置
      return [
          { label: '服务ID', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false, readonly: true },
          { label: '客户代码', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false },
          { label: '客户名称', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false },
          { label: '销售员', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false },
          { label: '发货方式', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false },
          { label: '开票单位', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false, type: 'select', options: [] },
          { label: '收款地址', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false },
          { label: '收货地址', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false },
          { label: '备注', prop: 'serviceOrderSapId', palceholder: '请选择', col: this.ifFormEdit ? 5 : 6, disabled: false },
      ]
    },
    materialConfig () {
      let config = [ // 交通配置
        { label: '物料编码', prop: 'trafficType', type: 'select', options: this.transportTypeList, width: 105 },
        { label: '物料描述', prop: 'transport', type: 'select', options: this.transportationList, width: 135 },
        { label: '数量', prop: 'from', type: 'input', width: 125, readonly: true },
        { label: '单价', prop: 'to', type: 'input', width: 125, readonly: true },
        { label: '总计', prop: 'mone', type: 'number', align: 'right', width: 120, placeholder: '大于0' },
        { label: '当前库存量', prop: 'yremark', type: 'input', width: 100 },
        { label: '仓库', prop: 'invoiceNumber', width: 155, placeholder: '7-11位字母数字' },
        { label: '备注', prop: 'invoiceAttachment', width: 150 },
      ]
      return (this.ifFormEdit !== undefined && !this.ifFormEdit) || this.ifFormEdit === undefined // 不可编辑状态并且是报销单页面(不影响我的费用配置)
        ? config 
        : [...config, { type: 'operation', iconList: [{ icon: 'el-icon-delete', handleClick: this.deleteMaterialItem }], width: 130 }] // 交通配置        
    },
  }
}

