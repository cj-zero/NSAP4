<template>
  <div class="my-submission-wrapper">
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
      <!-- 审核弹窗 -->
      <my-dialog
        class="dialog-clss"
        ref="myDialog"
        top="10px"
        :btnList="btnList"
        @closed="closeDialog"
        @opened="onOpened"
        :titleStyle="titleStyle"
        title="进程"
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
import Search from '@/components/Search'
import Order from './common/components/order'
// import Report from './common/components/report'
// import zxform from "@/views/serve/callserve/form";
// import zxchat from '@/views/serve/callserve/chat/index'
import { tableMixin, categoryMixin, reportMixin, chatMixin, processMixin } from './common/js/mixins'

export default {
  name: 'toProcess',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin, processMixin],
  components: {
    Search,
    Order,
    // Report,
    // zxform,
    // zxchat
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } },
        // { type: 'button', btnText: '审批', isSpecial: true, handleClick: this.getDetail, options: { type: 'approve' } }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '同意', handleClick: this.agree, isShow: this.title !== 'view', type: 'primary' },
        { btnText: '驳回到发起人', handleClick: this.reject, isShow: this.title !== 'view', type: 'danger' }
      ]
    },
    // dialogWidth () {
    //   return this.isGeneralManager ? '1050px' : '1130px'
    // },
    titleStyle () {
      return { height: '55px !important' }
    }
  },
  data () {
    return {
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
    }
  },
  methods: {
    async agree () { //同意
      this.$refs.order.openRemarkDialog('agree')
    }, 
    reject () { // 驳回
      this.$refs.order.openRemarkDialog('reject')
    },
    closeDialog () {
      this.$refs.order.resetInfo()
      this.$refs.myDialog.close()
    }
  },
  created () {
    this.listQuery.pageType = 2
    this._getList()
    this._getCategoryName()
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.my-submission-wrapper {
  ::v-deep .el-tabs__header {
    background-color: #fff;
    margin-bottom: 0;
  }
}
.dialog-clss {
  ::v-deep {
    .el-dialog {
      width: 100% !important;
      min-width: 1100px;
      // left: 240px;
    }
  }
}
</style>


