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
        :loading="tableLoading">
        <template v-slot:contract="{ row }">
          <span class="show-btn" @click="showContractPictures(row)">查看</span>
        </template>
        <template v-slot:totalMoney="{ row }">
          <p v-infotooltip.top-start.ellipsis>{{ row.totalMoney | toThousands }}</p>
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
      :title="`${textMap[status]}销售订单`"
      :btnList="btnList"
      @closed="close"
    >
      <quotation-order 
        ref="quotationOrder" 
        :detailInfo="detailInfo"
        :categoryList="categoryList"
        :isSales="true"
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
    <el-image-viewer
      v-if="previewVisible"
      :url-list="previewImageUrlList"
      :on-close="closeViewer"
    >
    </el-image-viewer>
  </div>
</template>

<script>
import Search from '@/components/Search'
import QuotationOrder from '../common/components/QuotationOrder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { getQuotationList, getServiceOrderList } from '@/api/material/quotation'
import {  quotationTableMixin, categoryMixin, chatMixin } from '../common/js/mixins'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import { processDownloadUrl } from '@/utils/file'
export default {
  name: 'materialSalesOrder',
  mixins: [quotationTableMixin, categoryMixin, chatMixin],
  components: {
    Search,
    QuotationOrder,
    zxform,
    zxchat,
    ElImageViewer
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'quotationId', placeholder: '销售单号', width: 100 },
        { prop: 'cardCode', placeholder: '客户名称', width: 100 },
        { prop: 'serviceOrderSapId', placeholder: '服务ID', width: 100 },
        { prop: 'createUser', placeholder: '申请人', width: 100 },
        { prop: 'startCreateTime', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endCreateTime', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        // { type: 'button', btnText: this.searchBtnText, handleClick: this._getQuotationDetail, options: { status: 'pay' }, isSpecial: true },
      ]
    }, // 搜索配置
    btnList () {
      // 弹窗按钮
      return [
        // this.isMaterialFinancial 
        {  btnText: '提交', isShow: this.status === 'upload', handleClick: this.pay },
        { btnText: '确认收款', handleClick: this.pay, isShow: this.status === 'pay' && this.isMaterialFinancial,
          options: { type: 'pay' }
        },
        { btnText: '打印', handleClick: this.showContract, isShow: this.status !== 'upload', className: 'outline' },
        { btnText: '查看合同', handleClick: this.showContract, isShow: this.status !== 'upload', className: 'outline' },
        // { btnText: '驳回', handleClick: this.pay, options: { type: 'reject' }, isShow: !this.isMaterialFinancial },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ] 
    },
    searchBtnText () {
      return this.isMaterialFinancial ? '收款' : '审批'
    }
  },
  data () {
    return {
      previewVisible: false,
      previewImageUrlList: [],
      isSales: true,
      formQuery: {
        quotationId: '', // 领料单号
        cardCode: '', // 客户
        serviceOrderSapId: '', // 服务Id
        createUser: '', // 申请人
        startCreateTime: '', // 创建开始
        endCreateTime: '' // 创建结束
      },
      listQuery: {
        startType: 2,
        page: 1,
        limit: 50,
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
      quotationColumns: [
        { label: '销售订单', prop: 'salesOrderId', handleClick: this._getQuotationDetail, options: { status: 'pay', isSalesOrder: true }, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createTime' },
        { label: '总金额（￥）', prop: 'totalMoney', align: 'right', slotName: 'totalMoney' },
        { label: '合同', slotName: 'contract' },
        { label: '备注', prop: 'remark' },
        { label: '状态', prop: 'quotationStatusText' }
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
        this.tableData = this._normalizeList(data)
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
    pay (options) {
      this.$refs.quotationOrder.beforeApprove(options.type)
    },
    showContractPictures (row) { // 查看表格合同
      console.log(row, 'row', processDownloadUrl)
      let files = row.files.map(pictureItem => {
        return processDownloadUrl(pictureItem.fileId)
      })
      if (!files.length) {
        return this.$message.warning('暂无图片')
      }
      this.previewImageUrlList = files
      this.previewVisible = true
    },
    closeViewer () {
      this.previewVisible = false
    },
    showContract () { // 查看弹窗合同
      this.$refs.quotationOrder.showContract()
    },
    close () {
      this.$refs.quotationOrder.resetInfo()
      this.$refs.quotationDialog.close()
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