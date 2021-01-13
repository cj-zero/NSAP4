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
      title="出库单详情"
      :btnList="btnList"
      :onClosed="close"
    >
      <SalesOrder ref="salesOrder" />
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
import Mock from 'mockjs'
const tableData = Mock.mock({
  "array|10": [{
    id: '@word',
    terminalCustomer: '@word',
    terminalCustomerId: '@word',
    salesMan: '@word',
    deliveryDate: '@date',
    warrantyDate: '@date',
    remark: '@word'
  }]
})
console.log(tableData, 'date')
export default {
  name: 'saleswarranty',
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
        { prop: 'quotationId', placeholder: '销售单号', width: 100 },
        { prop: 'cardCode', placeholder: '客户', width: 100 },
        { prop: 'serviceOrderSapId', placeholder: '销售员', width: 100 },
        { type: 'search' }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '保存', handleClick: this.updateMaterial, isShow: this.status !== 'view' },
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
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: tableData.array,
      total: 0,
      salesColumns: [
        { label: '销售单号', prop: 'id', handleClick: this._getQuotationDetail, type: 'link', width: 140 },
        { label: '客户代码', prop: 'termianalCustomer', handleClick: this._openServiceOrder, type: 'link', options: { isInTable: true }, width: 100 },
        { label: '客户名称', prop: 'terminalCustomerId', width: 160 },
        { label: '销售员', prop: 'salesMan', width: 70 },
        { label: '交货日期', prop: 'deliveryDate', width: 100 },
        { label: '保修日期', prop: 'warrantyDate', width: 100 },
        { label: '备注', prop: 'remark', width: 200 },
        { label: '操作', type: 'slot', slotName: 'btnList' }
      ],
      customerList: [], // 用户服务单列表
      status: 'outbound', // 报价单状态
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    openDialog () {
      this.$refs.salesOrderDialog.open()
    },
    _getList () {
      
    },
    updateMaterial () {
      this.dialogLoading = true
      this.$refs.outboundOrder.updateMaterial().then(res => {
        console.log(res, 'res')
        this.$message.success('保存成功')
        this._getList()
        this.close()
        this.dialogLoading = false
      }).catch(err => {
        this.dialogLoading = false
        this.$message.error(typeof err === 'object' ? err.message : '保存失败')
      })
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