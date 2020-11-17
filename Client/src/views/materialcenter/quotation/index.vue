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
          <common-table ref="quotationTable" :data="tableData" :columns="quotationColumns" :loading="tableLoading"></common-table>
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
      :title="`${textMap[title]}报价单`"
      :btnList="btnList"
    >
      <quotation-order 
        ref="quotationOrder" 
        :customerList="customerList"
        :detailInfo="detailInfo"
        :title="title"></quotation-order>
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
// const tableData = []
// for (let i = 0; i < 100; i++) {
//   tableData.push({
//     pickNO: i,
//     serviceOrderId: i,
//     customerId: i,
//     customerName: i,
//     totalMoney: i,
//     otherMoney: i,
//     applicant: 'rookie',
//     remark: 'rookie',
//     createTime: '123',
//     status: '审批中'
//   })
// }
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
        { btnText: '预览', handleClick: this.togglePreview, isShow: !this.isPreviewing },
        { btnText: '返回', handleClick: this.togglePreview, isShow: this.isPreviewing },
        { btnText: '提交', handleClick: this.submit },
        { btnText: '草稿', handleClick: this.submit, options: { isDraft: true } },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      initialName: '1', // 初始标签的值
      texts: [ // 标签数组
        { label: '全部', name: '' },
        { label: '草稿箱', name: '1' },
        { label: '申请中', name: '2' },
        { label: '已领料', name: '3' },
        { label: '已驳回', name: '4' }
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
        { label: '领料单号', prop: 'id', handleClick: this._getQuotationDetail, options: { type: 'view' }, type: 'link'},
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
      customerList: [], // 用户服务单列表
      title: 'create', // 报价单状态
      isPreviewing: false, // 处于预览状态
      currentRow: null, // 当前点击行
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
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    openMaterialOrder () {
      getServiceOrderList({ page: 1, limit: 1 }).then((res) => {
        this.customerList = res.data
        if (this.customerList && this.customerList.length) {
          this.title = 'create'
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
      let isDraft = !!options.isDraft
      this.dialogLoading = true
      let isEdit = this.title === 'edit'
      this.$refs.quotationOrder._operateOrder(isEdit, isDraft).then(() => {
        this.dialogLoading = false
        this._getList()
        this.close()
        this.$message.success(isDraft ? '存为草稿成功' : '提交成功')
      }).catch(err => {
        this.$message.error(err.message)
        this.dialogLoading = false
      })
    },
    togglePreview () {
      this.isPreviewing = !this.isPreviewing // 正在预览
      this.$refs.quotationOrder.togglePreview()
    },
    close () {
      this.$refs.quotationOrder.resetInfo()
      this.$refs.quotationDialog.close()
    },
    _getQuotationDetail (currentRow) {
      // let currentRow = this.$refs.quotationTable.getCurrentRow()
      // if (!currentRow) {
      //   this.$message.warning('请先选择数据')
      // }
      console.log(currentRow)
      this.tableLoading = true
      let { id } = currentRow
      getQuotationDetail({
        quotationId: id
      }).then(res => {
        console.log(res,' res')
        this.detailInfo = this._normalizeDetail(res.data)
        this.$refs.quotationDialog.open()
        this.tableLoading = false
        this.title = 'edit'
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