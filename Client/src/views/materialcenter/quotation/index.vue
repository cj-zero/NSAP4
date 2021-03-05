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
        <!-- <el-upload
          v-if="isCustomerServiceSupervisor"
          style="display: inline-block"
          :action="importMaterialAction"
          name="files"
          :show-file-list="false"
          :on-success="onSuccess"
          :on-error="onError"
        >
          <el-button size="mini" type="primary">上传物料信息</el-button>
        </el-upload> -->
      </div>
    </sticky>
    <Layer> 
      <common-table 
        height="100%"
        ref="quotationTable" 
        :data="tableData" 
        :columns="quotationColumns" 
        :loading="tableLoading">
        <template v-slot:totalMoney="{ row }">
          <p v-infotooltip.ellipsis.top-start>{{ row.totalMoney | toThousands }}</p>
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
      ref="quotationDialog"
      width="1180px"
      :loading="dialogLoading"
      :title="`${textMap[status]}报价单`"
      :btnList="btnList"
      @closed="close"
    >
      <quotation-order 
        ref="quotationOrder" 
        :customerList="customerList"
        :detailInfo="detailInfo"
        :status="status"
        :categoryList="categoryList"
        :isReceive="isReceive"></quotation-order>
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
import QuotationOrder from '../common/components/QuotationOrder'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { getQuotationList, getServiceOrderList, deleteOrder } from '@/api/material/quotation'
import {  quotationTableMixin, categoryMixin, chatMixin } from '../common/js/mixins'
import { print } from '@/utils/utils'
export default {
  name: 'quotation',
  mixins: [quotationTableMixin, categoryMixin, chatMixin],
  components: {
    Search,
    QuotationOrder,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'quotationId', placeholder: '领料单号', width: 100 },
        { prop: 'cardCode', placeholder: '客户名称', width: 100 },
        { prop: 'serviceOrderSapId', placeholder: '服务ID', width: 100 },
        { prop: 'createUser', placeholder: '申请人', width: 100 },
        { prop: 'startCreateTime', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endCreateTime', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '新建', handleClick: this.openMaterialOrder, isSpecial: true, options: { isReceive: true } },
        // { type: 'button', btnText: '编辑', handleClick: this._getQuotationDetail, isSpecial: true, options: { status: 'edit', isReceive: true } },
        { type: 'button', btnText: '打印', handleClick: this.print, isSpecial: true },     
        { type: 'button', btnText: '删除', handleClick: this.deleteOrder, style: { backgroundColor: '#f56c6c', color: '#fff' } },
        { 
          type: 'upload',
          isShow: this.isCustomerServiceSupervisor,
          attrs: {
            style: { display: 'inline-block' },
            btnText: '上传物料信息', 
            name: 'files',
            headers: { "X-Token":this.$store.state.user.token },
            action:  `${process.env.VUE_APP_BASE_API}/Material/Quotation/ImportMaterialPrice`, 
            'show-file-list': false,
            'on-success': this.onSuccess,
            'on-error': this.onError
          } 
        }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '保存', handleClick: this.submit, options: { isDraft: true }, isShow: this.isShowOperateBtn },
        { btnText: '客户同意报价', handleClick: this.submit, isShow: this.isPreviewing && this.isShowEditBtn, className: 'outline' },
        { btnText: '编辑', handleClick: this.togglePreview, isShow: this.isPreviewing && this.isShowEditBtn },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    },
    isShowOperateBtn () { // 处于新增状态、或者在编辑的状态才展示
      return this.status === 'create' || (!this.isPreviewing && this.isShowEditBtn)
    },
    isCustomerServiceSupervisor () { // 判断是不是客服主管
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '客服主管')
        : false
    }
  },
  data () {
    return {
      rolesList: this.$store.state.user.userInfoAll.roles, // 当前用户的角色列表
      importMaterialAction: `${process.env.VUE_APP_BASE_API}/Material/Quotation/ImportMaterialPrice`,
      isShowEditBtn: true, // 是否出现编辑按钮
      formQuery: {
        quotationId: '', // 领料单号
        cardCode: '', // 客户
        serviceOrderSapId: '', // 服务Id
        createUser: '', // 申请人
        startCreateTime: '', // 创建开始
        endCreateTime: '' // 创建结束
      },
      listQuery: {
        page: 1,
        limit: 50,
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
      quotationColumns: [
        { label: '领料单号', prop: 'id', handleClick: this._getQuotationDetail, options: { status: 'view', isReceive: true }, type: 'link'},
        { label: '服务ID', prop: 'serviceOrderSapId', handleClick: this._openServiceOrder, type: 'link' },
        { label: '销售单号', prop: 'salesOrderId', handleClick: this._getQuotationDetail, options: { status: 'view', isSalesOrder: true }, type: 'link', width: 120 },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '总金额（￥）', prop: 'totalMoney', align: 'right', slotName: 'totalMoney'},
        { label: '申请人', prop: 'createUser' },
        { label: '备注', prop: 'remark' },
        { label: '创建时间', prop: 'createTime' },
        { label: '状态', prop: 'quotationStatusText' }
      ],
      customerList: [], // 用户服务单列表
      status: 'create', // 报价单状态
      isPreviewing: true, // 处于预览状态
      currentRow: null, // 当前点击行
      detailInfo: null // 详情信息
    } 
  },
  methods: {
    print () {
      let currentRow = this.$refs.quotationTable.getCurrentRow()
      if (!currentRow) {
        return this.$message.warning('请先选择数据')
      }
      const { id } = currentRow
      print('/Material/Quotation/PrintQuotation', { QuotationId: id })
    },
    onSuccess () {
      this.$message.success('上传成功')
    },
    onError () {
      this.$message.error('上传失败')
    },
    _getList () {
      this.tableLoading = true
      getQuotationList(this.listQuery).then(res => {
        let { count, data } = res
        this.tableData = this._normalizeList(data)
        this.total = count
        this.tableLoading = false
        this.$refs.quotationTable.resetCurrentRow()
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    openMaterialOrder () {
      getServiceOrderList({ page: 1, limit: 1 }).then((res) => {
        this.customerList = res.data
        if (this.customerList && this.customerList.length) {
          this.status = 'create'
          this.isShowEditBtn = false
          return this.$refs.quotationDialog.open()
        } 
        this.$message.warning('无服务单数据')
      }).catch(err => {
        this.$message.error(err.message)
      })
    },
    deleteOrder () {
      let currentRow = this.$refs.quotationTable.getCurrentRow()
      if (!currentRow) {
        return this.$message.warning('请先选择数据')
      }
      let { quotationStatus, id } = currentRow
      if (+quotationStatus !== 3) {
        return this.$message.warning('当前报价单状态不可删除')
      }
      this.$confirm('确定删除？', '提示消息', {
        confirmButtonText: '确定',
        cancelButtonText: '取消'
      }).then(() => {
        deleteOrder({ quotationId: id }).then(() => {
          this.$message.success('删除成功')
          this._getList()
        }).catch(err => {
          this.$message.error(err.message)
        })
      })
    },
    
    submit (options) {
      let isDraft = !!options.isDraft
      this.dialogLoading = true
      this.$refs.quotationOrder._operateOrder(isDraft).then((res) => {
        this.dialogLoading = false
        this._getList()
        this.close()
        if (res.message === '已存在多笔订单且库存数量不满足，请尽快付款。') {
          this.$message.warning(res.message)
        }
        this.$delay(this.$message.success('保存成功'))
      }).catch(err => {
        this.$message.error(err.message)
        this.dialogLoading = false
      })
    },
    togglePreview () {
      this.isPreviewing = !this.isPreviewing // 正在预览
      this.$refs.quotationOrder.togglePreview()
    },
    close () {
      this.isPreviewing = true
      this.$refs.quotationOrder.resetInfo()
      this.$refs.quotationDialog.close()
    }
  },
  created () {
    this._getList()
    this._getCategoryNameList()
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