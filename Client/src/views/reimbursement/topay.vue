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
                v-for="item in columns"
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
                    <img :src="rightImg" @click="item.handleJump({ ...scope.row, ...{ type: 'view' }})" class="pointer">
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
        :center="true"
        width="1316px"
        :onClosed="closeDialog"
        :title="textMap[title]"
        :btnList="btnList"
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
      <my-dialog
        ref="reportDialog"
        width="983px"
        title="服务行为报告单"
        :onClosed="resetReport">
        <Report :data="reportData" ref="report"/>
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
              :form="temp"
              formName="查看"
              labelposition="right"
              labelwidth="100px"
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
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import Order from './common/components/order'
import Report from './common/components/report'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { tableMixin, categoryMixin, reportMixin, chatMixin } from './common/js/mixins'

export default {
  name: 'toPay',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin],
  components: {
    Search,
    Sticky,
    // CommonTable,
    Pagination,
    MyDialog,
    Order,
    Report,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { type: 'search' },
        { type: 'button', handleClick: this.getDetail, btnText: '支付', isSpecial: true, options:  { type: 'toPay' } }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '确认支付', handleClick: this.toPay, isShow: this.title !== 'view' },
        { btnText: '关闭', handleClick: this.closeDialog, isShow: this.title !== 'view' }
      ]
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
    toPay () {
      this.$refs.order.openRemarkDialog('pay')
    },
    closeDialog () {
      this.$refs.order.resetInfo()
      this.$refs.myDialog.close()
    }
  },
  created () {
    this.listQuery.pageType = 5
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