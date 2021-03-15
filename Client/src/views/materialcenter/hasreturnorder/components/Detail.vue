<template>
  <div class="has-return-wrapper">
    <!-- 头部信息 -->
    <el-row type="flex" align="middle" class="head-title-wrapper">
      <p><span>结算订单</span><span>{{ formData.ReturnNoteId }}</span></p>
      <p><span>申请人</span> <span>{{ formData.CreateUser }}</span></p>
      <p><span>创建时间</span><span>{{ formData.CreateTime | formatDateFilter }}</span></p>
      <p><span>销售员</span><span>{{ formData.SalesMan }}</span></p>
    </el-row>
    <!-- 表单 -->
    <common-form
      ref="form"
      class="my-form-view"
      :model="formData" 
      label-width="60px" 
      :formItems="formItems" 
      :columnNumber="5"
      :disabled="true"
    ></common-form> 
    <div class="divider"></div>
    <!-- 表格信息 -->
    <common-table
      class="material-table-wrapper"
      style="margin-top: 10px;"
      :data="formData.DetailList"
      :columns="materialColumns"
      max-height="300px"
    >
      <template v-slot:notClearAmount="{ row }">
        <p v-infotooltip.top-start.ellipsis>{{ row.notClearAmount | toThousands }}</p>
      </template>
    </common-table>
    <el-row class="money-wrapper" type="flex" align="middle" justify="end">
      <span>合计</span>
      <span>{{ totalMoney | toThousands }}</span>
    </el-row>
  </div>
</template>

<script>
import { accAdd } from '@/utils/process'
import { isNumber } from '@/utils/validate'
import { formatDate } from '@/utils/date'
export default {
  props: {
    formData: {
      type: Object,
      default: () => {}
    }
  },
  filters: {
    formatDateFilter (val) {
      return val ? formatDate(val, 'YYYY.MM.DD HH:mm:ss') : ''
    }
  },
  computed: {
    totalMoney () { // 计算每一个物料表格的总金额
      if (this.formData.DetailList && this.formData.DetailList.length) {
        return this.formData.DetailList.filter(item => isNumber(item.notClearAmount))
          .reduce((prev, next) => accAdd(prev, next.notClearAmount), 0)
      }
      return 0        
    }
  },
  data () {
    return {
      formItems: [
        { tag: 'text', span: 4, attrs: { prop: 'U_SAP_ID', disabled: true }, itemAttrs: { label: '服务ID' } },
        { tag: 'text', span: 4, attrs: { prop: 'CustomerId', disabled: true }, itemAttrs: { label: '客户代码' } },
        { tag: 'text', span: 8, attrs: { prop: 'CustomerName', disabled: true }, itemAttrs: { label: '客户名称' } },
        { tag: 'text', span: 4, attrs: { prop: 'Contacter', disabled: true }, itemAttrs: { label: '联系人' } },
        { tag: 'text', span: 4, attrs: { prop: 'ContactTel', disabled: true }, itemAttrs: { label: '电话' } },
      ],
      materialColumns: [
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materDescription' },
        { label: '已退数量', prop: 'alreadyReturnQty', align: 'right' },
        { label: '需退总计', prop: 'totalReturnCount', align: 'right' },
        { label: '未清小计(￥)', prop: 'notClearAmount', align: 'right', slotName: 'notClearAmount' },
        { label: '状态', prop: 'status' }
      ]
    }
  },
  methods: {

  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.has-return-wrapper {
  position: relative;
  font-size: 12px;
  .divider {
    height: 1px;
    margin: 15px auto;
    background: #E6E6E6;
  }
  /* 头部信息 */
  .head-title-wrapper {
    position: absolute;
    top: -40px;
    left: 116px;
    p { 
      margin-right: 10px;
      font-size: 12px;
      span:nth-child(1) {
        color: #BFBFBF;
        margin-right: 10px;
      }
      span:nth-child(2) {
        color: #222222;
      }
    }
  }
  /* 物料表格 */
  .material-table-wrapper {
    ::v-deep .el-input.is-disabled .el-input__inner {
      background-color: #fff;
      cursor: default;
      color: #606266;
      border-color: #DCDFE6;
    }
    ::v-deep .el-textarea.is-disabled .el-textarea__inner {
      background-color: #fff;
      cursor: default;
      color: #606266;
      border-color: #DCDFE6;
    }
  }
  /* 合计 */
  .money-wrapper {
    margin-top: 10px;
    span:nth-child(1) {
      color: #BFBFBF;
      margin-right: 10px;
    }
    span:nth-child(2) {
      color: #222222;
      font-weight: bold;
    }
  }
}
</style>