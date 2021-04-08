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
        class="table-wrapper"
        height="100%"
        ref="quotationTable" 
        :data="tableData" 
        :columns="quotationColumns"
        @row-click="onRowClick" 
        :loading="tableLoading">
        <template v-slot:contract="{ row }">
          <span class="show-btn" @click="showContractPictures(row)">查看</span>
        </template>
        <template v-slot:balance="{ row }">
          <p v-infotooltip.top.ellipsis>{{ row.balance | toThousands }}</p>
        </template>
        <template v-slot:totalMoney="{ row }">
          <p v-infotooltip.top.ellipsis>{{ row.totalMoney | toThousands }}</p>
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
      ref="quotationDialog"
      width="1180px"
      :loading="dialogLoading"
      :title="`${title}物料领料单`"
      :btnList="btnList"
      @closed="closed"
    >
      <quotation-order 
        ref="quotationOrder" 
        :detailInfo="detailInfo"
        :categoryList="categoryList"
        :status="status"
        :isReceive="isReceive"></quotation-order>
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
    <!-- 预览合同图片 -->
    <!-- <el-image-viewer
      v-if="previewVisible"
      :url-list="previewImageUrlList"
      :on-close="closeViewer"
    >
    </el-image-viewer> -->
    <file-viewer 
      v-if="previewVisible"
      :file-list="previewFileList"
      :on-close="closeViewer"
    >
    </file-viewer>
  </div>
</template>

<script>
import Search from '@/components/Search'
import QuotationOrder from '../common/components/QuotationOrder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { getQuotationList } from '@/api/material/quotation'
import {  quotationTableMixin, categoryMixin, chatMixin, rolesMixin } from '../common/js/mixins'
// import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
const statusOptions = [
  { label: '全部', value: '' },
  { label: '未审批', value: '1' },
  { label: '审批', value: '2' }
]
export default {
  name: 'materialSalesOrder',
  mixins: [quotationTableMixin, categoryMixin, chatMixin, rolesMixin],
  components: {
    Search,
    QuotationOrder,
    zxform,
    zxchat
    // ElImageViewer
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'startType', placeholder: '请选择', type: 'select', options: statusOptions },
        { prop: 'quotationId', placeholder: '领料单号', width: 100 },
        { prop: 'cardCode', placeholder: '客户名称', width: 100 },
        { prop: 'serviceOrderSapId', placeholder: '服务ID', width: 100 },
        { prop: 'createUser', placeholder: '申请人', width: 100 },
        { prop: 'startCreateTime', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endCreateTime', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' }
      ]
    }, // 搜索配置
    btnList () {
      // 弹窗按钮
      return [
        // this.isMaterialFinancial 
        {  btnText: '同意', isShow: this.isValid, handleClick: this.agree },
        { btnText: '驳回', handleClick: this.reject, isShow: this.isValid, options: { type: 'reject' }},     
      ] 
    },
    searchBtnText () {
      return this.isMaterialFinancial ? '收款' : '审批'
    },
    isValid () {
      return (this.isMaterialsEngineer && this.quotationStatus === 4) || (this.isGeneralManager && this.quotationStatus === 5)
    },
    title () {
      return this.isValid ? '审批' : '查看'
    },
    quotationColumns () {
      return this.isGeneralManager ? [
        { label: '领料单号', prop: 'id', handleClick: this._getQuotationDetail, options: { status: 'approve' }, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createTime', width: 150 },
        { label: '总金额（￥）', prop: 'totalMoney', align: 'right', slotName: 'totalMoney' },
        { label: '科目余额（￥）', slotName: 'balance', align: 'right' },
        { label: '备注', prop: 'remark' },
        { label: '状态', prop: 'quotationStatusText' }
      ] : [
        { label: '领料单号', prop: 'id', handleClick: this._getQuotationDetail, options: { status: 'approve' }, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createTime', width: 150 },
        { label: '总金额（￥）', prop: 'totalMoney', align: 'right', slotName: 'totalMoney' },
        { label: '备注', prop: 'remark' },
        { label: '状态', prop: 'quotationStatusText' }
      ]
    }
  },
  data () {
    return {
      quotationStatus: 0, // 当前物料单状态
      previewVisible: false,
      previewFileList: [],
      formQuery: {
        startType: '1',
        quotationId: '', // 领料单号
        cardCode: '', // 客户
        serviceOrderSapId: '', // 服务Id
        createUser: '', // 申请人
        startCreateTime: '', // 创建开始
        endCreateTime: '' // 创建结束
      },
      listQuery: {
        startType: '1',
        pageStart: 1,
        page: 1,
        limit: 50,
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
      // quotationColumns: ,
      status: 'approve', // 报价单状态
      detailInfo: null, // 详情信息
      currentRow: null // 当前选中的行数据
    } 
  },
  methods: {
    _getList () {
      this.tableLoading = true
      getQuotationList(this.listQuery).then(res => {
        let { count, data } = res
        this.tableData = this._normalizeList(data)
        this.total = count
        this.tableLoading = false
        this.currentRow = null
        this.$refs.quotationTable.resetCurrentRow()
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    agree (options) {
      this.$refs.quotationOrder.beforeApprove(options.type)
    },
    reject (options) {
      this.$refs.quotationOrder.beforeApprove(options.type)
    },
    onRowClick (row) {
      this.currentRow = row
    },
    closeViewer () {
      this.previewVisible = false
    },
    handleClose () {
      this.$refs.quotationDialog.close()
    },
    closed () {
      this.$refs.quotationOrder.resetInfo()
    }
  },
  created () {
    this._getList()
    this._getCategoryNameList()
  }
}
</script>
<style lang='scss' scoped>
.my-submission-wrapper {
  ::v-deep .el-tabs__header {
    background-color: #fff;
    margin-bottom: 0;
  }
  .table-wrapper {
    .show-btn {
      color: #F8B500;
      cursor: pointer;
    }
  }
}
</style>