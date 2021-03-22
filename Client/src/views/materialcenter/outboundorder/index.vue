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
      title="物料出库单"
      :btnList="btnList"
      @closed="closed"
      :destroy-on-close="true"
      top="100px"
      @opened="onOpened"
    >
      <outbound-order 
        ref="outboundOrder" 
        :detailInfo="detailInfo"
        :categoryList="categoryList"
        :status="status">
      </outbound-order>
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
import OutboundOrder from './components/outboundorder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { getQuotationList } from '@/api/material/quotation'
import {  quotationTableMixin, chatMixin, categoryMixin } from '../common/js/mixins'
import { print } from '@/utils/utils'
import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: 'outBoundOrder',
  directives: {
    elDragDialog
  },
  mixins: [quotationTableMixin, chatMixin, categoryMixin],
  components: {
    TabList,
    Search,
    OutboundOrder,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'quotationId', placeholder: '出库单号', width: 100 },
        { prop: 'cardCode', placeholder: '客户名称', width: 100 },
        { prop: 'serviceOrderSapId', placeholder: '服务ID', width: 100 },
        { prop: 'createUser', placeholder: '申请人', width: 100 },
        { prop: 'startCreateTime', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endCreateTime', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '打印', handleClick: this.print, isSpecial: true },   
        // { type: 'button', btnText: '打印', handleClick: this.print, isSpecial: true },     
        // { type: 'button', btnText: '出库', handleClick: this._getQuotationDetail, options: { status: 'outbound'}, isSpecial: true },
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '关闭', handleClick: this.handleClose, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      initialName: '', // 初始标签的值
      texts: [ // 标签数组
        { label: '全部', name: '' },
        { label: '未出库', name: '1' },
        { label: '已出库', name: '2' }
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
        status: '2',
        startType: '',
        pageStart: 3,
        page: 1,
        limit: 50,
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
      quotationColumns: [
        { label: '出库单号', prop: 'id', handleClick: this._getQuotationDetail, options: { status: 'view' }, type: 'link'},
        { label: '销售单号', prop: 'salesOrderId' },
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '总金额(￥)', prop: 'totalMoney', align: 'right', slotName: 'totalMoney' },
        { label: '申请人', prop: 'createUser' },
        { label: '备注', prop: 'remark' },
        { label: '创建时间', prop: 'createTime' },
      ],
      customerList: [], // 用户服务单列表
      status: 'outbound', // 报价单状态
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    print () {
      let currentRow = this.$refs.quotationTable.getCurrentRow()
      if (!currentRow) {
        return this.$message.warning('请先选择数据')
      }
      const { id } = currentRow
      print('/Material/Quotation/PrintPickingList', { QuotationId: id })
    },
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
    onTabChange (name) {
      this.listQuery.startType = name
      this.listQuery.page = 1
      this._getList()
    },
    handleClose () {
      this.$refs.quotationDialog.close()
    },
    closed () {
      this.$refs.outboundOrder.resetInfo()
    },
    onOpened () {
      this.$refs.outboundOrder.openedFn()
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
}
</style>