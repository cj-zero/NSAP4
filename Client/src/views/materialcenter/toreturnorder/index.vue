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
import ReturnOrder from './components/Order'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { quotationTableMixin, chatMixin, returnTableMixin, afterReturnMixin } from '../common/js/mixins'
const W_100 = { width: '100px' }
const W_150 = { width: '150px' }
export default {
  name: 'materialToReturnOrder',
  mixins: [quotationTableMixin, chatMixin, returnTableMixin, afterReturnMixin],
  components: {
    Search,
    ReturnOrder,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'id', component: { attrs: { placeholder: '退料单号', style: W_100 } } },
        { prop: 'customer', component: { attrs: { placeholder: '客户名称', style: W_100 } } },
        { prop: 'sapId', component: { attrs: { placeholder: '服务ID', style: W_100 } } },
        { prop: 'createName', component: { attrs: { placeholder: '申请人', style: W_100 } } },
        { prop: 'beginDate', component: { tag: 'date', attrs: { placeholder: '创建开始日期', style: W_150 } } },
        { prop: 'endDate', component: { tag: 'date', attrs: { placeholder: '创建结束日期', style: W_150 } } },
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '验收', handleClick: this.checkOrSave },
        // { btnText: '保存', handleClick: this.checkOrSave, options: { isSave: true } },
        { btnText: '关闭', handleClick: this.handleClose, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      listQuery: {
        status: 0,
        page: 1,
        limit: 50,
      },
      status: '', // 报价单状态
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    submit (options) {
      let isDraft = !!options.isDraft
      this.dialogLoading = true
      let isEdit = this.status === 'edit'
      this.$refs.quotationOrder._operateOrder(isEdit, isDraft).then(() => {
        this.dialogLoading = false
        this._getList()
        this.handleClose()
        this.$message.success(isDraft ? '存为草稿成功' : '提交成功')
      }).catch(err => {
        this.$message.error(err.message)
        this.dialogLoading = false
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