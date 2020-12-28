<template>
  <div class="my-processed-wrapper">
    <tab-list :initialName="initialName" :texts="texts" @tabChange="onTabChange"></tab-list>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :config="searchConfig"
          @changeForm="onChangeForm" 
          @search="onSearch">
        </Search>
      </div>
    </sticky>
    <Layer>
      <common-table
        ref="table"
        height="100%"
        :data="tableData"
        :columns="processedColumns"
        :loading="tableLoading"
        @row-click="onRowClick"
      ></common-table>
      <pagination
        v-show="total>0"
        :total="total"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleCurrentChange"
      />
    </Layer>
    <!-- <div class="app-container">
      <div class="bg-white">
        <div class="content-wrapper">
          <common-table
            ref="table"
            height="100%"
            :data="tableData"
            :columns="processedColumns"
            :loading="tableLoading"
            @row-click="onRowClick"
          ></common-table>
          <pagination
            v-show="total>0"
            :total="total"
            :page.sync="listQuery.page"
            :limit.sync="listQuery.limit"
            @pagination="handleCurrentChange"
          />
        </div>
      </div>
    </div> -->
    <!-- 审核弹窗 -->
    <my-dialog
      ref="myDialog"
      top="10px"
      :width="dialogWidth"
      @closed="closeDialog"
      @opened="onOpened"
      title="进程"
      :loading="dialogLoading"
      :btnList="btnList"
    >
      <order 
        ref="order" 
        :title="title"
        :isProcessed="true"
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
    <!-- <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
    >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
            :form="temp"
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
    </my-dialog> -->
  </div>
</template>

<script>
import TabList from '@/components/TabList'
import Search from '@/components/Search'
import Order from './common/components/order'
// import Report from './common/components/report'
// import zxform from "@/views/serve/callserve/form";
// import zxchat from '@/views/serve/callserve/chat/index'
import { tableMixin, categoryMixin, reportMixin, chatMixin, processMixin } from './common/js/mixins'

export default {
  name: 'processed',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin, processMixin],
  components: {
    Search,
    Order,
    TabList,
    // Report,
    // zxform,
    // zxchat
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { type: 'search' }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '驳回到发起人', handleClick: this.reject, 
          // 总经理并且当前状态为待支付
          isShow: this.isGeneralManager && this.reimburseStatus === 8, className: 'danger' },
        { btnText: '关闭', handleClick: this.closeDialog, className: 'close' }
      ]
    },
    dialogWidth () {
      return this.isGeneralManager ? '1015px' : '1130px'
    }
  },
  data () {
    return {
      initialName: '3', // 初始标签的值
      texts: [ // 标签数组
        { label: '同意', name: '3' },
        { label: '驳回', name: '4' },
      ],
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
    }
  },
  methods: {
    onTabChange (name) {
      this.listQuery.pageType = name
      this.listQuery.page = 1
      this._getList()
    },
    reject () { // 驳回
      this.$refs.order.openRemarkDialog('reject')
    },
    onChangeForm (val) {
      this.currentFormQuery = val
      Object.assign(this.listQuery, val)
    },
    closeDialog () {
      this.$refs.order.resetInfo()
      this.$refs.myDialog.close()
    }
  },
  created () {
    this.listQuery.pageType = 3
    this._getList()
    this._getCategoryName()
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.my-processed-wrapper {
  ::v-deep .el-tabs__header {
    background-color: #fff;
    margin-bottom: 0;
  }
}
</style>