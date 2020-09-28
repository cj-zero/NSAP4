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
              show-overflow-tooltip
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
                    <el-button @click="item.handleClick" size="mini" type="primary">{{ item.btnText }}</el-button>
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
        width="1000px"
        :onClosed="closeDialog"
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
  </div>
</template>

<script>
import TabList from '@/components/TabList'
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import Order from './common/components/order'
import { tableMixin, categoryMixin } from './common/js/mixins'

export default {
  name: 'processed',
  mixins: [tableMixin, categoryMixin],
  components: {
    Search,
    Sticky,
    // CommonTable,
    Pagination,
    MyDialog,
    Order,
    TabList
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { type: 'search' }
      ]
    } // 搜索配置
  },
  data () {
    return {
      initialName: '3', // 初始标签的值
      texts: [ // 标签数组
        { label: '同意', name: '3' },
        { label: '撤回', name: '4' },
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
    onChangeForm (val) {
      this.currentFormQuery = val
      Object.assign(this.listQuery, val)
    },
    onSearch () {
      this._getList()
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
.my-submission-wrapper {
  ::v-deep .el-tabs__header {
    background-color: #fff;
    margin-bottom: 0;
  }
}
</style>