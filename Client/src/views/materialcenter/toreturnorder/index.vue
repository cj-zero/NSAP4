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
          <common-table 
            ref="returnOrderTable" 
            :data="tableData" 
            :columns="returnOrderColumns" 
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
      ref="returnOrderDialog"
      width="1100px"
      :loading="dialogLoading"
      title="退料单详情"
      :btnList="btnList"
      :onClosed="close"
    >
      <return-Order 
        ref="returnOrder" 
        :detailInfo="detailInfo"
        :status="status"
        :isReceive="true"></return-Order>
    </my-dialog>
  </div>
</template>

<script>
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
import returnOrder from './components/returnorder'
// import { getQuotationDetail } from '@/api/material/quotation'
import { getReturnNoteList, getReturnNoteDetail } from '@/api/material/returnMaterial'
import {  quotationTableMixin } from '../common/js/mixins'
export default {
  name: 'quotation',
  mixins: [quotationTableMixin],
  components: {
    Search,
    Sticky,
    CommonTable,
    Pagination,
    MyDialog,
    returnOrder
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'id', placeholder: '退料单号', width: 100 },
        { prop: 'customer', placeholder: '客户名称', width: 100 },
        { prop: 'sapId', placeholder: '服务ID', width: 100 },
        { prop: 'createName', placeholder: '申请人', width: 100 },
        { prop: 'beginDate', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endDate', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '退料', handleClick: this._getReturnNoteDetail, options: { status: 'toReturn'} },
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '验收', handleClick: this.checkOrSave, isShow: !this.isPreviewing },
        { btnText: '保存', handleClick: this.checkOrSave, isShow: this.isPreviewing, options: { isSave: true } },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      formQuery: {
        id: '', // 退料单ID
        customer: '', // 客户
        sapId: '', // 服务Id
        createName: '', // 申请人
        beginDate: '', // 创建开始
        endDate: '' // 创建结束
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
      returnOrderColumns: [
        { label: '退料单号', prop: 'id', handleClick: this._getReturnNoteDetail, options: { status: 'view' }, type: 'link'},
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '服务ID', prop: 'serviceOrderId', handleClick: this.getDetail, type: 'link' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createDate' },
      ],
      customerList: [], // 用户服务单列表
      status: 'create', // 报价单状态
      isPreviewing: false, // 处于预览状态
      currentRow: null, // 当前点击行
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    _getList () {
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
    onSearch () {
      this.listQuery.page = 1
      this._getList()
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
      this.onSearch()
    },
    submit (options) {
      let isDraft = !!options.isDraft
      this.dialogLoading = true
      let isEdit = this.status === 'edit'
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
    close () {
      this.$refs.outboundOrder.resetInfo()
      this.$refs.returnOrderTable.close()
    },
    _getReturnNoteDetail (data) {
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
        // this.detailInfo = this._normalizeDetail(res.data)
        this.$refs.returnOrderDialog.open()
        this.tableLoading = false
        this.status = status
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    checkOrSave (value) {
      let { isSave } = value
      this.$refs.returnOrder.checkOrSave(isSave).then(() => {
        //
      })
    },
    handleCurrentChange ({ page, limit }) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    }
  },
  mounted () {
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