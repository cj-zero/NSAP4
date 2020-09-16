<template>
  <div class="my-submission-wrapper">
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="formQuery" 
          :config="searchConfig"
        ></Search>
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
      </div>
  </div>
</template>

<script>
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import CommonTable from '@/components/CommonTable'
import Pagination from '@/components/Pagination'
import tableData from './mock'
import { isSameObjectByValue } from '@/utils/validate'
import { deepClone } from '@/utils'
import { tableMixin } from './common/js/mixins' 
export default {
  mixins: [tableMixin],
  components: {
    Search,
    Sticky,
    CommonTable,
    Pagination
  },
  data () {
    return {
      searchConfig: [ // 搜索配置
        { placeholder: '汇总单号', prop: 'summaryId', width: 100 },
        { placeholder: '劳务关系', prop: 'laborRelation', width: 100 },
        { placeholder: '汇总起始时间', prop: 'dateFrom', type: 'date', width: 150 },
        { placeholder: '汇总结束事件', prop: 'dateTo', type: 'date', width: 150 },
        { type: 'button', btnText: '查询', handleClick: this.search, icon: 'el-icon-search' }
      ],
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
    openTree (row) { // 打开详情
      console.log(row, 'row')
    },
    approval () { // 添加
      this.$refs.myDialog.open()
      console.log('add')
    },
    submit () {}, // 提交
    saveAsDraft () {}, // 存为草稿
    reset () {}, // 重置
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