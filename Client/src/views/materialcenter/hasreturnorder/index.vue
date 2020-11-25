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
      </div>
    </sticky>
    <div class="app-container">
      <div class="bg-white">
        <div class="content-wrapper">
          <common-table :data="tableData" :columns="quotationColumns" :loading="tableLoading"></common-table>
          <pagination
            v-show="total>0"
            :total="total"
            :page.sync="listQuery.page"
            :limit.sync="listQuery.limit"
            @pagination="handleCurrentChange"
          />
        </div>
      </div>
    </div>    
    <my-dialog ref="myDialog">
    </my-dialog>
  </div>
</template>

<script>
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
const tableData = []
for (let i = 0; i < 100; i++) {
  tableData.push({
    pickNO: i,
    serviceOrderId: i,
    customerId: i,
    customerName: i,
    totalMoney: i,
    otherMoney: i,
    applicant: 'rookie',
    remark: 'rookie',
    createTime: '123',
    status: '审批中'
  })
}
export default {
  name: 'quotation',
  components: {
    Search,
    Sticky,
    CommonTable,
    Pagination,
    MyDialog
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'pickNO', placeholder: '退料单号', width: 100 },
        { prop: 'customerName', placeholder: '客户', width: 100 },
        { prop: '', placeholder: '服务ID', width: 100 },
        { prop: 'applicant', placeholder: '申请人', width: 100 },
        { prop: 'startDate', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endDate', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      formQuery: {
        pickNO: '',
        customerName: '',
        serviceOrderId: '',
        startDate: '',
        endDate: ''
      },
      listQuery: {
        page: 1,
        limit: 50,
      },
      tableLoading: false,
      tableData,
      total: 0,
      quotationColumns: [
        { label: '退料单号', prop: 'pickNO', handleClick: this.getDetail, options: { type: 'view' }, type: 'link'},
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '服务ID', prop: 'serviceOrderId', handleClick: this.getDetail, type: 'link' },
        { label: '申请人', prop: 'applicant' },
        { label: '创建时间', prop: 'createTime' }
      ]
    } 
  },
  methods: {
    submit () {},
    saveAsDraft () {},
    close () {},
    getDetail (data) {
      console.log(data, 'data detail')
    },
    onChangeForm () {},
    onSearch () {},
    handleCurrentChange () {}
  }
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