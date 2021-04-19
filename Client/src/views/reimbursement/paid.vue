<template>
  <div class="my-paid-wrapper">
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="listQuery"
          :config="searchConfig"
          @search="onSearch">
        </Search>
      </div>
    </sticky>
    <Layer>
      <common-table
        ref="table"
        height="100%"
        :data="tableData"
        :columns="paidColumns"
        :loading="tableLoading"
      ></common-table>
      <pagination
        v-show="total>0"
        :total="total"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleCurrentChange"
      />
    </Layer>
      <!-- 审核弹窗 -->
      <my-dialog
        ref="myDialog"
        width="1351px"
        @closed="closeDialog"
        :title="textMap[title]"
        :loading="dialogLoading"
      >
        <order 
          ref="order" 
          :title="title"
          :detailData="detailData"
          :categoryList="categoryList"
          :customerInfo="customerInfo">
        </order>
      </my-dialog>
      <!-- 完工报告 -->
      <!-- <my-dialog
        ref="reportDialog"
        width="983px"
        title="服务行为报告单"
        :onClosed="resetReport">
        <Report :data="reportData" ref="report"/>
      </my-dialog> -->
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
import Order from './common/components/order'
// import Report from './common/components/report'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { tableMixin, categoryMixin, reportMixin, chatMixin } from './common/js/mixins'

export default {
  name: 'paid',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin],
  components: {
    Search,
    Order,
    // Report,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } },
      ]
    }, // 搜索配置 
  },
  data () {
    return {
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
      isPaid: true, // 已经支付标识变量
      paidColumns: [ // 不同的表格配置(我的提交除外的其它模块表格)
        { label: '报销单号', prop: 'mainIdText', type: 'link', width: 70, handleClick: this.getDetail, options: { type: 'view' } },
        { label: '服务ID', prop: 'serviceOrderSapId', width: 80, type: 'link', handleClick: this._openServiceOrder, options: { type: 'view' } },
        { label: '客户代码', prop: 'terminalCustomerId', width: 75 },
        { label: '客户名称', prop: 'terminalCustomer', width: 170 },
        { label: '总金额', prop: 'totalMoney', width: 100, align: 'right' },
        { label: '总天数', prop: 'days', width: 60, align: 'right' },
        { label: '出发日期', prop: 'businessTripDate', width: 85 },
        { label: '结束日期', prop: 'endDate', width: 85 },
        { label: '报销部门', prop: 'orgName', width: 70 },
        { label: '报销人', prop: 'userName', width: 70 },
        { label: '业务员', prop: 'salesMan', width: 80 },
        { label: '填报日期', prop: 'fillTime', width: 85 }
      ]
    }
  },
  methods: {
    closeDialog () {
      this.$refs.order.resetInfo()
    }
  },
  created () {
    this.listQuery.pageType = 6
    this._getList()
    this._getCategoryName()
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.my-paid-wrapper {
  ::v-deep .el-tabs__header {
    background-color: #fff;
    margin-bottom: 0;
  }
}
</style>