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
    <Layer>
      <common-table 
        height="100%"
        ref="quotationTable" 
        :data="tableData" 
        :columns="salesColumns" 
        @row-click="onRowClick"
        :loading="tableLoading">
        <template v-slot:btnList="{ row }">
          <el-button size="mini" class="customer-btn-class" @click="openDialog(row)">保修</el-button>
        </template>
      </common-table>
      <pagination
        v-show="total>0"
        :total="total"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleCurrentChange"
      />
    </Layer>
    <my-dialog 
      ref="salesOrderDialog"
      width="1100px"
      :loading="dialogLoading"
      :btnList="btnList"
      :onClosed="close"
    >
      <template v-slot:title>
        <div class="my-dialog-icon"></div>
        <span class="my-dialog-title">修改保修时间</span>
        <el-row type="flex" align="middle" v-if="currentRow" style="margin-left: 20px;">
          <p class="my-dialog-item">
            <span>销售单号</span>
            <span>{{ currentRow.salesOrderId }}</span>
          </p>
          <p class="my-dialog-item">
            <span>销售员</span>
            <span>{{ currentRow.salesOrderName }}</span>
          </p>
        </el-row>
      </template>
      <SalesOrder ref="salesOrder" :currentRow="currentRow" />
    </my-dialog>
    <!-- 只能查看的表单 -->
    <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
    >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
            formName="查看"
            labelposition="right"
            labelwidth="72px"
            max-width="800px"
            :isCreate="false"
            :refValue="dataForm"
          ></zxform>
        </el-col>
        <el-col :span="6" class="lastWord">   
          <zxchat :serveId='serveId' formName="报销"></zxchat>
        </el-col>
      </el-row>
    </my-dialog>
  </div>
</template>

<script>
import Search from '@/components/Search'
import SalesOrder from './components/SalesOrder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { chatMixin } from '../common/js/mixins'
import { getSalesOrderList, approve } from '@/api/material/warrantyDate'
import { formatDate } from '@/utils/date'
// import Day from 'dayjs'
// import Mock from 'mockjs'
// const tableData = Mock.mock({
//   "array|10": [{
//     id: '@word',
//     terminalCustomer: '@word',
//     terminalCustomerId: '@word',
//     salesMan: '@word',
//     deliveryDate: '@date',
//     warrantyDate: '@date',
//     remark: '@word'
//   }]
// })
// console.log(tableData, 'date')
export default {
  name: 'approvewarranty',
  mixins: [chatMixin],
  components: {
    Search,
    SalesOrder,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'salesOrderId', placeholder: '销售单号', width: 100 },
        { prop: 'customer', placeholder: '客户', width: 100 },
        { prop: 'salesMan', placeholder: '销售员', width: 100 },
        { type: 'search' }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '同意', handleClick: this.approveDateClick, options: { isPass: true } },
        { btnText: '驳回', handleClick: this.approveDateClick, className: 'danger' },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      formQuery: {
        quotationId: '', // 领料单号
        cardCode: '', // 客户
        salesMan: '', // 销售员
      },
      listQuery: {
        page: 1,
        limit: 50,
        state: 2
      },
      dialogLoading: false,
      tableLoading: false,
      tableData:  [],
      total: 0,
      salesColumns: [
        { label: '销售单号', prop: 'salesOrderId', handleClick: this.openSalesOrder, type: 'link', width: 100 },
        { label: '客户代码', prop: 'customerId', width: 100 },
        { label: '客户名称', prop: 'customerName', width: 200 },
        { label: '销售员', prop: 'salesOrderName', width: 70 },
        { label: '交货日期', prop: 'deliveryDate', width: 100 },
        { label: '保修日期', prop: 'warrantyPeriod', width: 100 },
        { label: '备注', prop: 'remark', width: 200 }
      ],
      currentRow: null
    } 
  },
  methods: {
    openSalesOrder (data) {
      this.currentRow = data
      this.$refs.salesOrderDialog.open()
    },
    _getList () {
      this.tableLoading = true
      getSalesOrderList(this.listQuery).then(res => {
        let { data, count } = res
        this.total = count
        this.tableData = data.map(item => {
          item.warrantyPeriod = formatDate(item.warrantyPeriod)
          item.deliveryDate = formatDate(item.deliveryDate)
          return item
        })
        this.tableData = data
        this.tableLoading = false
        console.log(res, 'res')
      }).catch(err => {
        this.tableData = []
        this.tableLoading = false
        this.$message.error(err.message)
      })
    },
    approveDateClick (optoins) {
      let isPass = !!optoins.isPass
      this.$refs.salesOrder.approveWarrantyDate(params => {
        params = {
          ...params,
          isPass
        }
        this.dialogLoading = true
        approve(params).then(() => {
          this.$message.success(params.isPass ? '审批成功' : '驳回成功')
          this.dialogLoading = false
          this._getList()
          this.close()
        }).catch(err => {
          this.$message.error(err.message)
          this.dialogLoading = false
        })
      })
    },
    onRowClick (row) {
      this.currentRow = JSON.parse(JSON.stringify(row))
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
    },
    onSearch () {
      this.listQuery.page = 1
      this._getList()
    },
    handleCurrentChange ({ page, limit }) {
        this.listQuery.page = page
        this.limit = limit
        this._getList()
    },
    close () {
      this.$refs.salesOrder.resetInfo()
      this.$refs.salesOrderDialog.close()
    },
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