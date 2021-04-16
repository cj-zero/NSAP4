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
        height="100%"
        ref="returnOrderTable" 
        :data="tableData" 
        :columns="returnOrderColumns" 
        :loading="tableLoading">
        <template v-slot:totalMoney="{ row }">
          <p v-infotooltip.ellipsis>{{ Number(row.totalMoney) | toThousands }}</p>
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
      width="950px"
      :loading="dialogLoading"
      title="退料单详情"
      :btnList="btnList"
      @closed="closed"
      @opened="onOpened"
    >
      <return-Order 
        ref="returnOrder"
        orderType="returnOrder"
        :isCreated="isCreated"
        :detailInfo="detailInfo"
        :status="status"
        ></return-Order>
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
            :isCreate="isCreated"
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
import ReturnOrder from '../common/components/ReturnOrder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { quotationTableMixin, chatMixin, returnTableMixin } from '../common/js/mixins'
import { getServiceOrderInfo, getReturnNoteList, getReturnNoteDetail, } from '@/api/material/returnMaterial'
const W_100 = { width: '100px' }
const W_150 = { width: '150px' }
export default {
  name: 'materialToReturnOrder',
  mixins: [quotationTableMixin, chatMixin, returnTableMixin],
  components: {
    Search,
    ReturnOrder,
    zxform,
    zxchat
  },
  provide() {
    return {
      parentVm: this
    }
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'id', component: { attrs: { style: W_100, placeholder: '退料单号' } } },
        { prop: 'customer', component: { attrs: { style: W_100, placeholder: '客户名称' } } },
        { prop: 'sapId', component: { attrs: { style: W_100, placeholder: '服务ID' } } },
        { prop: 'createName', component: { attrs: { style: W_100, placeholder: '申请人' } } },
        { prop: 'beginDate', component: { tag: 'date', attrs: { style: W_150, placeholder: '创建开始日期' } } },
        { prop: 'endDate', component: { tag: 'date', attrs: { style: W_150, placeholder: '创建结束日期' } } },
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } },
        { component: { tag: 's-button', attrs: { btnText: '新建', class: ['customer-btn-class'] }, on: { click: this.createOrder } } }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '关闭', handleClick: this.handleClose, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      listQuery: {
        page: 1,
        limit: 50,
      },
      isCreated: false,
      status: '', // 报价单状态
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    async createOrder () { // 新建退料单
      this.isCreated = true
      try {
        const { data } = await getServiceOrderInfo({
          page: 1,
          limit: 1
        })
        if (data && data.length) {
          this.$refs.returnOrderDialog.open()
        } else {
          this.$message.warning('服务单列表为空')
        }
      } catch (err) {
        this.$message.error(err.message)
      }
    },
    _getList () { // 获取涂料单列表信息
      this.tableLoading = true
      getReturnNoteList(this.listQuery).then(res => {
        let { count, data } = res
        this.tableData = data
        this.total = count
        this.tableLoading = false
        this.$refs.returnOrderTable.resetCurrentRow()
        console.log('_getList', this.$refs.returnOrderTable.getCurrentRow())
      }).catch(err => {
        this.tableData = []
        this.total = 0
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    _getReturnNoteDetail (data) { // 获取退料单详情
      let id
      let { status } = data
      id = data.id
      console.log(status, 'status', id)
      this.tableLoading = true
      this.isCreated = false
      getReturnNoteDetail({
        id
      }).then(res => {
        this.detailInfo = this._normalizeDetail(res.data)
        this.$refs.returnOrderDialog.open()
        this.tableLoading = false
        this.status = status
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    checkOrSave (value) {
      let { isSave } = value
      this.dialogLoading = true
      this.$refs.returnOrder.checkOrSave(isSave).then((res) => {
        console.log('res', res)
        this.$message.success(isSave ? '保存成功' : '验收成功')
        this._getList()
        this.close()
        this.dialogLoading = false
      }).catch(err => {
        this.$message.error(err.message)
        // this.close()
        this.dialogLoading = false
      })
    },
    handleClose () {
      this.$refs.returnOrderDialog.close()
    },
    closed () {
      this.$refs.returnOrder.resetInfo()
    },
    onOpened () {
      this.$refs.returnOrder.openedFn()
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