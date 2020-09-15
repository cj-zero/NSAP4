<template>
  <div class="my-submission-wrapper">
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="formQuery" 
          :config="searchConfig"
          @changeForm="onChangeForm" 
          @search="onSearch">
        </Search>
        <!-- <permission-btn moduleName="callservesure" size="mini" v-on:btn-event="onBtnClicked"></permission-btn> -->
      </div>
    </sticky>
      <div class="app-container">
        <div class="bg-white">
          <div class="content-wrapper">
            <common-table :data="tableData" :columns="columns" :loading="tableLoading"></common-table>
            <pagination
              v-show="total>0"
              :total="total"
              :page.sync="listQuery.page"
              :limit.sync="listQuery.limit"
              @pagination="handleCurrentChange"
            />
            </div>
        </div>
        <my-dialog
          ref="myDialog"
          :center="true"
          width="800px"
          :btnList="btnList"
        >
          <order></order>
        </my-dialog>
      </div>
  </div>
</template>

<script>
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import CommonTable from '@/components/CommonTable'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import Order from './common/components/order'
import tableData from './mock'
import { isSameObjectByValue } from '@/utils/validate'
import { deepClone } from '@/utils'
export default {
  components: {
    Search,
    Sticky,
    CommonTable,
    Pagination,
    MyDialog,
    Order
  },
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
      statusOptions: [ // 审核状态
        { label: '全部', value: '' },
        { label: '审批中', value: '1' },
        { label: '已支付', value: '2' },
        { label: '已撤回', value: '3' },
        { label: '草稿箱', value: '4' }
      ],
      searchConfig: [ // 搜索配置
        { placeholder: '报销单号', prop: 'accountId', width: 100 },
        { placeholder: '报销状态', prop: 'status', width: 100 },
        { placeholder: '报销人', prop: 'people', width: 100 },
        { placeholder: '客户代码/名称', prop: 'customer', width: 150 },
        { placeholder: '服务ID', prop: 'serviceId', width: 100 },
        { placeholder: '报销部门', prop: 'org', width: 100 },
        { placeholder: '费用承担', prop: 'expense', width: 100 },
        { placeholder: '责任承担', prop: 'responsibility', width: 100 },
        { placeholder: '填报起始时间', prop: 'dateFrom', type: 'date', width: 150 },
        { placeholder: '填报结束事件', prop: 'dateTo', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '审批', handleClick: this.approval},
      ],
      listQuery: { // 分页参数
        page: 1,
        limit: 30
      },
      tableLoading: false,
      btnList: [
        { btnText: '提交', handleClick: this.submit },
        { btnText: '存为草稿', handleClick: this.saveAsDraft },
        { btnText: '重置', handleClick: this.reset },
        { btnText: '关闭', handleClick: this.closeDialog }
      ]
    }
  },
  methods: {
    _getList (type) {
      let { page, limit } = this.listQuery
      this.totalTableData = tableData
      if (type) {
        if (this.isCurrentChange) {
          this.tableData = tableData.slice((page - 1) * limit, limit * page - 1)
          this.isCurrentChange = false // 将翻页标识置为false
          console.log('current change')
        } else if (this.currentFormQuery && !isSameObjectByValue(this.formQuery, this.currentFormQuery)) { // 判断
          this.listQuery.page = 1
          this.tableData = tableData.slice((page - 1) * limit, limit * page - 1)
          this.formQuery = deepClone(this.currentFormQuery)
        }
      } else {
        console.log('no search')
        this.tableData = tableData.slice((page - 1) * limit, limit * page - 1)
      }
      console.log('getList')
    },
    handleCurrentChange ({page, limit}) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this.isCurrentChange = true
      this._getList('search')
    },
    onChangeForm (val) {
      this.currentFormQuery = val
      Object.assign(this.listQuery, val)
    },
    onSearch () {
      this._getList('search')
    },
    openTree (row) { // 打开详情
      console.log(row, 'row')
    },
    approval () { // 添加
      this.$refs.myDialog.open()
      console.log('add')
    },
    submit () {}, // 提交
    saveAsDraft () {}, // 存为草稿
    reset () {}, // 重置
    closeDialog () {
      this.$refs.myDialog.close()
    }
  },
  computed: {
    total () {
      return this.totalTableData.length
    }
  },
  created () {
    this.oldListQuery = JSON.parse(JSON.stringify(this.listQuery))
    this._getList()
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.my-submission-wrapper {
  ::v-deep .el-tabs__header {
    background-color: #fff;
    margin-bottom: 0;
  }
}
</style>