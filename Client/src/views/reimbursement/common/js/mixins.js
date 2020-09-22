export let tableMixin = {
  data () {
    return {
      columns: [ // 表格配置
        { label: '报销单号', prop: 'accountId', type: 'link', width: 100, handleJump: this.openTree },
        { label: '填报日期', prop: 'fillDate', width: 100 },
        { label: '报销部门', prop: 'org', width: 100 },
        { label: '报销人', prop: 'people', width: 100 },
        { label: '职位', prop: 'position', width: 100 },
        { label: '总金额', prop: 'totalMoney', width: 100 },
        { label: '报销状态', prop: 'status', width: 100 },
        { label: '项目名称', prop: 'projectName', width: 100 },
        { label: '客户代码', prop: 'customerId', width: 100 },
        { label: '客户名称', prop: 'customerName', width: 100 },
        { label: '客户简称', prop: 'customerRefer', width: 100 },
        { label: '业务员', prop: 'saleMan', width: 100 },
        { label: '出发地', prop: 'origin', width: 100 },
        { label: '到达地', prop: 'destination', width: 100 },
        { label: '出发日期', prop: 'originDate', width: 100 },
        { label: '结束日期', prop: 'endDate', width: 100 },
        { label: '总天数', prop: 'totalDay', width: 100 },
        { label: '服务ID', prop: 'serviceOrderId', width: 100 },
        { label: '序列号', prop: 'serialNumber', width: 100 },
        { label: '呼叫主题', prop: 'theme', width: 100 },
        { label: '问题类型', prop: 'problemType', width: 100 },
        { label: '解决方案', prop: 'solution', width: 100 },
        { label: '责任承担', prop: 'responsibility', width: 100 },
        { label: '费用承担', prop: 'expense', width: 100 },
        { label: '劳务关系', prop: 'laborRelations', width: 100 },
        { label: '报销类别', prop: 'category', width: 100 },
        { label: '备注', prop: 'remark', width: 100 }
      ],
      tableData: [],
      totalTableData: [],
      formQuery: { // 查询字段参数
        accountId: '',
        status: '',
        people: '',
        customer: '',
        serviceId: '',
        org: '',
        expense: '',
        responsibility: '',
        dateFrom: '',
        dateTo: ''
      },
      listQuery: { // 分页参数
        page: 1,
        limit: 30
      },
      tableLoading: false,
    }
  },
}

export let searchMixin = {
  data () {
    return {
      // TODO
    }
  }
}