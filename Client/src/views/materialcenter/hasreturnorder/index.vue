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
        @row-click="onRowClick"
        height="100%"
        ref="returnOrderTable" 
        :data="tableData" 
        :columns="returnOrderColumns" 
        :loading="tableLoading">
        <template v-slot:notClearAmoun="{ row }">
          <p v-infotooltip.ellipsis.top-start>{{ row.notClearAmount | toThousands }}</p>
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
      ref="returnOrderDialog"
      width="1100px"
      title="物料结算单"
      :btnList="btnList"
      :destroy-on-close="true"
    >
      <detail 
        ref="returnOrder" 
        :formData="formData"
      ></detail>
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
import Search from '@/components/Search'
// import ReturnOrder from '../common/components/ReturnOrder'
import Detail from './components/Detail'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { chatMixin } from '../common/js/mixins'
import { getClearReturnNoteList, getClearReturnNoteDetail } from '@/api/material/returnMaterial'
const W_100 = { width: '100px' }
const W_150 = { width: '100px' }
export default {
  name: 'materialHasReturnOrder',
  mixins: [chatMixin],
  components: {
    Search,
    zxchat,
    zxform,
    Detail
  },
  data () {
    return {
      listQuery: {
        page: 1,
        limit: 50,
        sapId: '',
        customer: '',
        createName: '',
        beginDate: '',
        endDate: ''
      },
      tableLoading: false,
      tableData: [],
      total: 0,
      currnetRow: null,
      returnOrderColumns: [
        { label: '结算订单', type: 'link', handleClick: this.preview, prop: 'id' },
        { label: '服务ID', type: 'link', handleClick: this._openServiceOrder, prop: 'serviceOrderSapId' },
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '申请人', prop: 'createUser' },
        { label: '创建时间', prop: 'createDate' },
        { label: '总金额', prop: 'notClearAmount', slotName: 'notClearAmoun', align: 'right' },
        { label: '备注', prop: 'remark' },
        // { label: '状态', prop: 'status' },
      ],
      status: '', // 报价单状态
      formData: null, // 详情信息
      searchConfig: [     
        {  prop: 'sapId', component: { attrs: { placeholder: '服务ID', style: W_100 } } },   
        {  prop: 'customer', component: { attrs: { placeholder: '客户名称', style: W_100 } } },   
        {  prop: 'createrName', component: { attrs: { placeholder: '申请人', style: W_100 } } },   
        {  prop: 'beginDate', component: { tag: 'date', attrs: { placeholder: '创建开始日期', style: W_150 } } },   
        {  prop: 'endDate', component: { tag: 'date', attrs: { placeholder: '创建结束日期', style: W_150 } } },
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } }
        // { prop: 'sapId', placeholder: '服务ID', width: 100 },
        // { prop: 'customer', placeholder: '客户名称', width: 100 },
        // { prop: 'createrName', placeholder: '申请人', width: 100 },
        // { prop: 'beginDate', placeholder: '创建开始日期', type: 'date', width: 150 },
        // { prop: 'endDate', placeholder: '创建结束日期', type: 'date', width: 150 },
        // { type: 'search' }
      ],
      btnList: [
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  methods: {
    _openServiceOrder (row) {
      this.openServiceOrder(row.serviceOrderId, () => this.tableLoading = true, () => this.tableLoading = false)
    },
    onRowClick (row) {
      this.currnetRow = row
    },
    _getList () {
      this.tableLoading = true
      getClearReturnNoteList(this.listQuery).then(res => {
        this.tableLoading = false
        let { data, count } = res
        console.log(data, 'data')
        this.tableData = this._normalizeList(data)
        console.log(this.tableData)
        this.total = count
        this.currnetRow = null
      }).catch(err => {
        this.$message(err.message)
        this.tableLoading = false
      })
    },
    _normalizeList (list) {
      return list.map(item => {
        return { ...item.detail[0] }
      })
    },
    close () {
      this.$refs.returnOrderDialog.close()
    },
    preview (row) {
      this.tableLoading = true
      getClearReturnNoteDetail({ id: row.id }).then(res => {
        this.formData = res.data
        this.$refs.returnOrderDialog.open()
        this.tableLoading = false
        console.log(res, 'detail')
      }).catch(err => {
        this.tableLoading = false
        this.$message.error(err.message)
      })
    },
    onSearch () {
      this.listQuery.page = 1
      this._getList()
    },
    handleCurrentChange ({ page, limit }) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    }
  },
  mounted () {
    this._getList()
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