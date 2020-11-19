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
          <common-table 
            ref="quotationTable" 
            :data="tableData" 
            :columns="quotationColumns" 
            :loading="tableLoading">
          </common-table>
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
      width="1100px"
      :loading="dialogLoading"
      :title="`${textMap[status]}报价单`"
      :btnList="btnList"
      :onClosed="close"
    >
      <quotation-order 
        ref="quotationOrder" 
        :detailInfo="detailInfo"
        :status="status"></quotation-order>
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
import { getQuotationList, getServiceOrderList, getQuotationDetail } from '@/api/material/quotation'
import {  quotationTableMixin } from '../common/js/mixins'
export default {
  name: 'quotation',
  mixins: [quotationTableMixin],
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
        { prop: 'pickNO', placeholder: '报价单号', width: 100 },
        { prop: 'customerName', placeholder: '客户名称', width: 100 },
        { prop: '', placeholder: '服务ID', width: 100 },
        { prop: 'applicant', placeholder: '申请人', width: 100 },
        { prop: 'startDate', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endDate', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '审批', handleClick: this._getQuotationDetail, options: { status: 'approve' } },
      ]
    }, // 搜索配置
    btnList () {
      // 弹窗按钮
      return [
        { btnText: '同意', handleClick: this.submit, options: { isReject: true }, isShow: !this.isPreviewing && this.status !== 'view' },
        { btnText: '驳回', handleClick: this.submit, options: { isReject: false }, isShow: this.isPreviewing },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      initialName: '', // 初始标签的值
      texts: [ // 标签数组
        { label: '待处理', name: '' },
        { label: '已驳回', name: '1' },
        { label: '已通过', name: '2' }
      ],
      formQuery: {
        quotationId: '', // 领料单号
        cardCode: '', // 客户
        serviceOrderSapId: '', // 服务Id
        createUser: '', // 申请人
        startCreateTime: '', // 创建开始
        endCreateTime: '' // 创建结束
      },
      listQuery: {
        startType: '',
        page: 1,
        limit: 50,
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 100,
      quotationColumns: [
        { label: '领料单号', prop: 'id', handleClick: this._getQuotationDetail, options: { status: 'view' }, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this.getDetail, type: 'link' },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '单据总金额', prop: 'totalMoney' },
        { label: '未清金额', prop: 'otherMoney' },
        { label: '申请人', prop: 'createUser' },
        { label: '备注', prop: 'remark' },
        { label: '创建时间', prop: 'createTime' },
        { label: '状态', prop: 'quotationStatus' }
      ],
      status: 'create', // 报价单状态
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    _getList () {
      this.tableLoading = true
      getQuotationList(this.listQuery).then(res => {
        let { count, data } = res
        this.tableData = data
        this.total = count
        this.tableLoading = false
        this.$refs.quotationTable.resetCurrentRow()
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    openMaterialOrder () {
      getServiceOrderList({ page: 1, limit: 1 }).then((res) => {
        this.customerList = res.data
        if (this.customerList && this.customerList.length) {
          this.status = 'create'
          return this.$refs.quotationDialog.open()
        } 
        this.$message.warning('无服务单数据')
      }).catch(err => {
        this.$message.error(err.message)
      })
    },
    onSearch () {
      this.listQuery.page = 1
      this._getList()
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
      this.onSearch()
    },
    onTabChange (name) {
      this.listQuery.startType = name
      this.listQuery.page = 1
      this._getList()
    },
    submit (options) {
      let isReject = !!options.isReject
      this.dialogLoading = true
      this.$refs.quotationOrder._approve(isReject).then(() => {
        this.dialogLoading = false
        this._getList()
        this.close()
        this.$message.success(isReject ? '驳回成功' : '审批成功')
      }).catch(err => {
        this.$message.error(err.message)
        this.dialogLoading = false
      })
    },
    close () {
      this.$refs.quotationOrder.resetInfo()
      this.$refs.quotationDialog.close()
    },
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
      let { terminalCustomer, terminalCustomerId } = serviceOrders
      // result
      return { ...quotations, terminalCustomer, terminalCustomerId }
    },
    handleCurrentChange ({ page, limit }) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    }
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