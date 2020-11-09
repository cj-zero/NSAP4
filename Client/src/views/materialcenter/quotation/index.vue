<template>
  <div class="my-submission-wrapper">
    <tab-list :initialName="initialName" :texts="texts" @tabChange="onTabChange"></tab-list>
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
    <my-dialog 
      ref="quotationDialog"
      width="800px"
      :btnList="btnList"
    >
      <quotation-order ref="quotationOrder" :customerList="customerList"></quotation-order>
    </my-dialog>
  </div>
</template>

<script>
import TabList from '@/components/TabList'
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
import QuotationOrder from '../common/components/quotationOrder'
import { getQuotationList, getServiceOrderList } from '@/api/material/quotation'
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
    TabList,
    Search,
    Sticky,
    CommonTable,
    Pagination,
    MyDialog,
    QuotationOrder
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'pickNO', placeholder: '领料单号', width: 100 },
        { prop: 'customerName', placeholder: '客户名称', width: 100 },
        { prop: '', placeholder: '服务ID', width: 100 },
        { prop: 'applicant', placeholder: '申请人', width: 100 },
        { prop: 'startDate', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endDate', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '新建', isSpecial: true, handleClick: this.openMaterialOrder },
        { type: 'button', btnText: '编辑', handleClick: this.getDetail, options: { type: 'edit', name: 'mySubmit' } },
        { type: 'button', btnText: '打印', handleClick: this.print },     
        { type: 'button', btnText: '删除', handleClick: this.deleteOrder },
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '提交', handleClick: this.submit },
        { btnText: '草稿', handleClick: this.saveAsDraft },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      initialName: '1', // 初始标签的值
      texts: [ // 标签数组
        { label: '全部', name: '1' },
        { label: '草稿箱', name: '2' },
        { label: '申请中', name: '3' },
        { label: '已领料', name: '4' },
        { label: '已驳回', name: '5' }
      ],
      formQuery: {
        quotationId: '', // 领料单号
        cardCode: '', // 客户
        serviceOrderSapId: '',
        startCraeteTime: '',
        endCreateTime: ''
      },
      listQuery: {
        startType: '',
        page: 1,
        limit: 50,
      },
      tableLoading: false,
      tableData,
      total: 100,
      quotationColumns: [
        { label: '领料单号', prop: 'quotationId', handleClick: this.getDetail, options: { type: 'view' }, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderId', handleClick: this.getDetail, type: 'link' },
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '单据总金额', prop: 'totalMoney' },
        { label: '未清金额', prop: 'otherMoney' },
        { label: '申请人', prop: 'applicant' },
        { label: '备注', prop: 'remark' },
        { label: '创建时间', prop: 'createTime' },
        { label: '状态', prop: 'status' }
      ],
      customerList: [] // 用户服务单列表
    } 
  },
  methods: {
    _getList () {
      getQuotationList(this.listQuery)
    },
    openMaterialOrder () {
      getServiceOrderList({ page: 1, limit: 1 }).then((res) => {
        this.customerList = res.data
        if (this.customerList && this.customerList.length) {
          return this.$refs.quotationDialog.open()
        } 
        this.$message.warning('无服务单数据')
      }).catch(err => {
        this.$message.error(err.message)
      })
      
    },
    onTabChange () {},
    submit () {},
    saveAsDraft () {},
    close () {},
    getDetail (data) {
      console.log(data, 'data detail')
    },
    onChangeForm () {},
    onSearch () {},
    handleCurrentChange () {}
  },
  created () {
    this._getList()
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