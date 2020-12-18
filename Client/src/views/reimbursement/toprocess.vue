<template>
  <div class="my-submission-wrapper">
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :config="searchConfig"
          @changeForm="onChangeForm" 
          @search="onSearch">
        </Search>
      </div>
    </sticky>
      <div class="app-container">
        <div class="bg-white">
          <div class="content-wrapper">
            <el-table 
              ref="table"
              :data="tableData" 
              v-loading="tableLoading" 
              size="mini"
              border
              fit
              height="100%"
              style="width: 100%;"
              @row-click="onRowClick"
              highlight-current-row
              >
              <el-table-column
                v-for="item in processedColumns"
                :key="item.prop"
                :width="item.width"
                :label="item.label"
                :align="item.align || 'left'"
                :sortable="item.isSort || false"
                :type="item.originType || ''"
                show-overflow-tooltip
              >
                <template slot-scope="scope" >
                  <div class="link-container" v-if="item.type === 'link'">
                    <img :src="rightImg" @click="item.handleJump({ ...scope.row, ...{ type: 'approve' }})" class="pointer">
                    <span>{{ scope.row[item.prop] }}</span>
                  </div>
                  <template v-else-if="item.type === 'operation'">
                    <el-button 
                      v-for="btnItem in item.actions"
                      :key="btnItem.btnText"
                      @click="btnItem.btnClick(scope.row)" 
                      type="text" 
                      :icon="item.icon || ''"
                      :size="item.size || 'mini'"
                    >{{ btnItem.btnText }}</el-button>
                  </template>
                  <template v-else-if="item.label === '服务报告'">
                    <div class="link-container">
                      <img :src="rightImg" @click="item.handleClick(scope.row, 'table')" class="pointer">
                      <span>查看</span>
                    </div>
                  </template>
                  <template v-else>
                    {{ scope.row[item.prop] }}
                  </template>
                </template>    
              </el-table-column>
            </el-table>
            <!-- <common-table :data="tableData" :columns="columns" :loading="tableLoading"></common-table> -->
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
      <!-- 审核弹窗 -->
      <my-dialog
        ref="myDialog"
        top="10px"
        :width="dialogWidth"
        :btnList="btnList"
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
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import Order from './common/components/order'
// import Report from './common/components/report'
// import zxform from "@/views/serve/callserve/form";
// import zxchat from '@/views/serve/callserve/chat/index'
import { tableMixin, categoryMixin, reportMixin, chatMixin } from './common/js/mixins'

export default {
  name: 'toProcess',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin],
  components: {
    Search,
    Sticky,
    // CommonTable,
    Pagination,
    MyDialog,
    Order,
    // Report,
    // zxform,
    // zxchat
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { type: 'search' },
        // { type: 'button', btnText: '审批', isSpecial: true, handleClick: this.getDetail, options: { type: 'approve' } }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '同意', handleClick: this.agree, isShow: this.title !== 'view' },
        { btnText: '驳回到发起人', handleClick: this.reject, isShow: this.title !== 'view', className: 'danger' },
        { btnText: '关闭', handleClick: this.closeDialog, className: 'close' }
      ]
    },
    dialogWidth () {
      return this.isGeneralManager ? '1015px' : '1100px'
    }
  },
  data () {
    return {
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
    }
  },
  methods: {
    onChangeForm (val) {
      this.currentFormQuery = val
      Object.assign(this.listQuery, val)
    },
    async agree () { //同意
      this.$refs.order.openRemarkDialog('agree')
    }, 
    reject () { // 驳回
      this.$refs.order.openRemarkDialog('reject')
    },
    operate () { // 操作成功 驳回/同意
      this.closeDialog()
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
</style>