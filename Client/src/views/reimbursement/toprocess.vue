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
        <!-- <permission-btn moduleName="callservesure" size="mini" v-on:btn-event="onBtnClicked"></permission-btn> -->
      </div>
    </sticky>
      <div class="app-container">
        <div class="bg-white">
          <div class="content-wrapper">
            <common-table :data="tableData" :columns="columns" :loading="tableLoading"></common-table>
            <pagination
              v-show="total>0"
              :total="total"
              :page.sync="listQuery.page"
              :limit.sync="listQuery.limit"
              @pagination="handleCurrentChange"
            />
            </div>
        </div>
        <my-dialog
          ref="myDialog"
          :center="true"
          width="800px"
          :btnList="btnList"
        >
          <order></order>
        </my-dialog>
      </div>
  </div>
</template>

<script>
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import CommonTable from '@/components/CommonTable'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import Order from './common/components/order'
import tableData from './mock'
import { isSameObjectByValue } from '@/utils/validate'
import { deepClone } from '@/utils'
import { tableMixin } from './common/js/mixins'
import { commonSearch } from './common/js/search'
export default {
  mixins: [tableMixin],
  components: {
    Search,
    Sticky,
    CommonTable,
    Pagination,
    MyDialog,
    Order
  },
  data () {
    return {
      statusOptions: [ // 审核状态
        { label: '全部', value: '' },
        { label: '审批中', value: '1' },
        { label: '已支付', value: '2' },
        { label: '已撤回', value: '3' },
        { label: '草稿箱', value: '4' }
      ],
      searchConfig: [ // 搜索配置
        ...commonSearch,
        { type: 'search' },
        { type: 'button', btnText: '审批', handleClick: this.approval},
      ],
      listQuery: { // 分页参数
        page: 1,
        limit: 30
      },
      tableLoading: false,
      btnList: [
        { btnText: '同意', handleClick: this.agree },
        { btnText: '驳回到发起人', handleClick: this.reject },
        { btnText: '关闭', handleClick: this.closeDialog }
      ]
    }
  },
  methods: {
    _getList (type) {
      let { page, limit } = this.listQuery
      this.totalTableData = tableData
      if (type) {
        if (this.isCurrentChange) {
          this.tableData = tableData.slice((page - 1) * limit, limit * page - 1)
          this.isCurrentChange = false // 将翻页标识置为false
          console.log('current change')
        } else if (this.currentFormQuery && !isSameObjectByValue(this.formQuery, this.currentFormQuery)) { // 判断
          this.listQuery.page = 1
          this.tableData = tableData.slice((page - 1) * limit, limit * page - 1)
          this.formQuery = deepClone(this.currentFormQuery)
        }
      } else {
        console.log('no search')
        this.tableData = tableData.slice((page - 1) * limit, limit * page - 1)
      }
      console.log('getList')
    },
    handleCurrentChange ({page, limit}) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this.isCurrentChange = true
      this._getList('search')
    },
    onChangeForm (val) {
      this.currentFormQuery = val
      Object.assign(this.listQuery, val)
    },
    onSearch () {
      this._getList('search')
    },
    openTree (row) { // 打开详情
      console.log(row, 'row')
    },
    approval () { // 添加
      this.$refs.myDialog.open()
      console.log('add')
    },
    agree () {},
    reject () {},
    closeDialog () {
      this.$refs.myDialog.close()
    }
  },
  computed: {
    total () {
      return this.totalTableData.length
    }
  },
  created () {
    this.oldListQuery = JSON.parse(JSON.stringify(this.listQuery))
    this._getList()
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