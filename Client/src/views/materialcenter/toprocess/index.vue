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
    <Layer>
      <common-table 
        height="100%"
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
    </Layer> 
    <my-dialog 
      ref="quotationDialog"
      width="1100px"
      :loading="dialogLoading"
      title="审批报价单"
      :btnList="btnList"
      @closed="close"
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
  </div>
</template>

<script>
import TabList from '@/components/TabList'
import Search from '@/components/Search'
import QuotationOrder from '../common/components/QuotationOrder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { getApprovePendingList, getServiceOrderList } from '@/api/material/quotation'
import {  quotationTableMixin, categoryMixin, chatMixin } from '../common/js/mixins'
export default {
  name: 'materialToProcess',
  mixins: [quotationTableMixin, categoryMixin, chatMixin],
  components: {
    TabList,
    Search,
    QuotationOrder,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'quotationId', placeholder: '领料单号', width: 100 },
        { prop: 'cardCode', placeholder: '客户名称', width: 100 },
        { prop: 'serviceOrderSapId', placeholder: '服务ID', width: 100 },
        { prop: 'createUser', placeholder: '申请人', width: 100 },
        { prop: 'startCreateTime', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endCreateTime', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        // { type: 'button', btnText: '审批', handleClick: this._getQuotationDetail, options: { status: 'approve' }, isSpecial: true, isShow: this.isToProcess },
      ]
    }, // 搜索配置
    btnList () {
      // 弹窗按钮
      return [
        { btnText: '同意', handleClick: this.approve, options: { type: 'agree' }, isShow: this.isToProcess },
        { btnText: '驳回', handleClick: this.approve, options: { type: 'reject' }, isShow: this.isToProcess },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      initialName: '1', // 初始标签的值
      texts: [ // 标签数组
        { label: '待处理', name: '1' },
        { label: '已驳回', name: '2' },
        { label: '已通过', name: '3' }
      ],
      isToProcess: true,
      formQuery: {
        quotationId: '', // 领料单号
        cardCode: '', // 客户
        serviceOrderSapId: '', // 服务Id
        createUser: '', // 申请人
        startCreateTime: '', // 创建开始
        endCreateTime: '' // 创建结束
      },
      listQuery: {
        startType: '1',
        page: 1,
        limit: 50,
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
      quotationColumns: [
        { label: '领料单号', prop: 'id', handleClick: this._getQuotationDetail, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '销售单号', prop: 'salesOrderId', handleClick: this._getQuotationDetail, options: { status: 'view', isSalesOrder: true }, type: 'link'},
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '单据总金额', prop: 'totalMoney', align: 'right' },
        { label: '申请人', prop: 'createUser' },
        { label: '备注', prop: 'remark' },
        { label: '创建时间', prop: 'createTime', width: 150 },
        { label: '状态', prop: 'quotationStatusText' }
      ],
      status: 'create', // 报价单状态
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    _getList () {
      this.tableLoading = true
      getApprovePendingList(this.listQuery).then(res => {
        let { count, data } = res
        this.tableData = this._normalizeList(data)
        this.total = count
        this.tableLoading = false
        console.log(this.$refs.quotationTable, 'ref')
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
    onTabChange (name) {
      this.listQuery.startType = name
      this.listQuery.page = 1
      this.isToProcess = name === '1'
      this._getList()
    },
    approve (options) {
      this.$refs.quotationOrder.beforeApprove(options.type)
    },
    close () {
      this.$refs.quotationOrder.resetInfo()
      this.$refs.quotationDialog.close()
    },  
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
}
</style>