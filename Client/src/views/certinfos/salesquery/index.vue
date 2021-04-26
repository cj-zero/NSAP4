<template>
  <div class="sales-order-wrapper">
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="listQuery"
          :config="searchConfig"
          @search="onSearch">
        </Search>
        <!-- <el-popover
          size="mini"
          placement="bottom"
          width="200"
          trigger="click">
          <template v-if="downloadProcessList.length">
            <div v-for="item in downloadProcessList" :key="item.id">
              <div>{{ item.date }}</div>
              <el-progress :percentage="(item.count / item.totalCount * 100)" :format="formatText(item)" ></el-progress>
            </div>
          </template>
          <template v-else>暂无下载进度</template>
          <el-button size="mini" slot="reference">下载进度</el-button>
        </el-popover> -->
      </div>
    </sticky>
    <Layer>
      <common-table
        ref="table"
        height="100%"
        :data="tableData"
        :columns="columns"
        :loading="tableLoading"
      >
        <template v-slot:salesOrderId="{ row }">
          <el-link type="primary" @click="openOrder(row)">{{ row.salesOrderId }}</el-link>
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

      <!-- 审核弹窗 -->
      <my-dialog
        ref="myDialog"
        width="1250px"
        @closed="closeDialog"
        @opened="onOpened"
        :destroy-on-close="true"
      >
        <template v-slot:title>
          <el-row class="title" type="flex" align="middle">
            <p>销售订单<span>{{ currentRow.salesOrderId }}</span></p>
            <p>客户代码<span>{{ currentRow.cardCode }}</span></p>
            <p>客户名称<span>{{ currentRow.cardName }}</span></p>
            <p>创建时间<span>{{ currentRow.createDate }}</span></p>
          </el-row>
        </template>
        <equipment-order ref="order" :salesOrderId="salesOrderId" :currentInfo="currentRow"></equipment-order>
      </my-dialog>
  </div>
</template>

<script>
import Search from '@/components/Search'
import EquipmentOrder from './components/equipmentOrder'
import { querySalesLoad } from '@/api/cerfiticate'
import { mapState } from 'vuex'
import { formatDate } from '@/utils/date'
const W_370 = { width: '370px', display: 'inline-flex' }
const W_150 = { width: '150px' }
export default {
  name: 'salesquery',
  components: {
    Search,
    EquipmentOrder
  },
  computed: {
    ...mapState('certinfos', ['downloadProcessList'])
  },
  watch: {
    listQuery: {
      deep: true,
      handler (val) {
        console.log(val, 'listQuery')
      }
    }
  },
  data () {
    return {
      currentRow: {},
      salesOrderId: '',
      listQuery: {
        page: 1,
        limit: 50,
        pageStatus: 1
      },
      tableData: [],
      tableLoading: false,
      total: 0,
      columns: [ // 不同的表格配置(我的提交除外的其它模块表格)
        { label: '序号', type: 'index' },
        { label: '销售订单', slotName: 'salesOrderId', width: 100 },
        { label: '客户代码', prop: 'cardCode', width: 100 },
        { label: '客户名称', prop: 'cardName', width: 300 },
        { label: '创建时间', prop: 'createDate' }
      ],
      searchConfig: [
        { prop: 'salesOrderId', component: { attrs: { placeholder: '销售订单', style: W_150 } } },
        { prop: 'cardCode', component: { attrs: { placeholder: '客户代码', style: W_150 } } },
        { prop: 'cardName', component: { attrs: { placeholder: '客户名称', style: W_150 } } },
        { prop: 'manufacturerSerialNumbers', component: { attrs: { placeholder: '序列号', style: W_150 } } },
        { prop: 'date', 
          component: { 
            tag: 'date', on: { change: this.onDateChange },
            attrs: { 
              type: 'daterange', 'range-separator': '至', 'start-placeholder': '开始日期', 
              'end-placeholder': '结束日期', style: W_370, clearable: true 
            }, 
          } 
        },
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } },
      ]
    }
  },
  methods: {
    formatText (processItem) {
      const { count, totalCount } = processItem
      return () => `${count} / ${totalCount}`
    },
    async _getList () {
      this.tableLoading = true
      try {
        const { data, count } = await querySalesLoad(this.listQuery)
        console.log(data, count, 'methods')
        this.tableData = data.map(item => {
          item.createDate = formatDate(item.createDate)
          return item
        })
        this.total = count
        console.log(data, 'data')
      } catch (err) {
        this.$message.error(err.message)
        this.tableData = []
      } finally {
        this.tableLoading = false
      }
    },
    closeDialog () {
      this.$refs.order.reset()
    },
    onSearch () {
      this.listQuery.page = 1
      this._getList()
    },
    handleCurrentChange (val) {
      Object.assign(this.listQuery, val)
      this._getList()
    },
    onDateChange (val) {
      this.listQuery.saleStartCreatTime = val ? val[0] : ''
      this.listQuery.saleEndCreatTime = val ? val[1] : ''
    },
    openOrder (row) {
      console.log(row)
      this.currentRow = row
      this.salesOrderId = row.salesOrderId
      this.$refs.myDialog.open()
    },
    onOpened () {
      this.$refs.order._getList('eq')
    }
  },
  created () {
    this._getList()
    console.log(this, 'this')
    const { $vnode } = this
    const { componentInstance, context } = $vnode
    console.log(componentInstance === context)
  },
  mounted () {
    console.log('salesquery', this)
  },
}
</script>
<style lang='scss' scoped>
.sales-order-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  .title {
    p {
      margin-left: 10px;
      color: #a6a6a6;
      span {
        margin-left: 10px;
        color: #000;
      }
    }
  }
}
</style>