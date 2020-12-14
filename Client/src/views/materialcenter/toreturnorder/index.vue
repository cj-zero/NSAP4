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
          <common-table 
            height="100%"
            ref="returnOrderTable" 
            :data="tableData" 
            :columns="returnOrderColumns" 
            :loading="tableLoading">
          </common-table>
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
    <my-dialog 
      ref="returnOrderDialog"
      width="1100px"
      :loading="dialogLoading"
      title="退料单详情"
      :btnList="btnList"
      @closed="close"
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
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
import ReturnOrder from '../common/components/ReturnOrder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import {  quotationTableMixin, chatMixin, returnTableMixin } from '../common/js/mixins'
export default {
  name: 'materialToReturnOrder',
  mixins: [quotationTableMixin, chatMixin, returnTableMixin],
  components: {
    Search,
    Sticky,
    CommonTable,
    Pagination,
    MyDialog,
    ReturnOrder,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'id', placeholder: '退料单号', width: 100 },
        { prop: 'customer', placeholder: '客户名称', width: 100 },
        { prop: 'sapId', placeholder: '服务ID', width: 100 },
        { prop: 'createName', placeholder: '申请人', width: 100 },
        { prop: 'beginDate', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endDate', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '退料', handleClick: this._getReturnNoteDetail, options: { status: 'toReturn'}, isSpecial: true },
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '验收', handleClick: this.checkOrSave, isShow: this.status !== 'view' },
        { btnText: '保存', handleClick: this.checkOrSave, isShow: this.status !== 'view', options: { isSave: true } },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      listQuery: {
        status: '1',
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
        this.close()
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
    close () {
      this.$refs.returnOrder.resetInfo()
      this.$refs.returnOrderDialog.close()
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