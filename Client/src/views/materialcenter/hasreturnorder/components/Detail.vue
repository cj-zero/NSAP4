<template>
  <div class="has-return-wrapper">
    <!-- 头部信息 -->
    <el-row type="flex" align="middle" class="head-title-wrapper">
      <!-- <p class="bold red">服务ID: {{ formData.U_SAP_ID }}</p> -->
      <p>退库订单</p>
      <p class="bold">申请人: {{ formData.CreateUser }}</p>
      <p>创建时间: {{ formData.CreateTime }}</p>
      <p>销售员: {{ formData.SalesMan }}</p>
      <p>结算订单</p>
    </el-row>
    <!-- 表单 -->
    <common-form 
      :model="formData" 
      label-width="60px" 
      :formItems="formItems" 
      :columnNumber="5"
    > 
    </common-form>
    <!-- 表格信息 -->
    <common-table
      class="material-table-wrapper"
      style="margin-top: 10px;"
      :data="formData.MaterialList"
      :columns="materialColumns"
    >
    </common-table>
  </div>
</template>

<script>
export default {
  props: {
    formData: {
      type: Object,
      default: () => {}
    }
  },
  data () {
    return {
      materialColumns: [
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materDescription' },
        { label: '已退数量', prop: 'alreadyReturnQty', align: 'right' },
        { label: '需退总计', prop: 'totalReturnCount', align: 'right' },
        { label: '成本价', prop: 'costPrice', align: 'right' },
        { label: '未清总计', prop: '' },
      ],
      formItems: [
        { span: 4, attrs: { prop: 'U_SAP_ID', disabled: true }, itemAttrs: { label: '服务ID' } },
        { span: 4, attrs: { prop: 'CustomerId', disabled: true }, itemAttrs: { label: '客户代码' } },
        { span: 8, attrs: { prop: 'CustomerName', disabled: true }, itemAttrs: { label: '客户名称' } },
        { span: 4, attrs: { prop: 'contact', disabled: true }, itemAttrs: { label: '联系人' } },
        { span: 4, attrs: { prop: 'number', disabled: true }, itemAttrs: { label: '电话' } },
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
  /* 头部信息 */
  .head-title-wrapper {
    position: absolute;
    top: -37px;
    left: 93px;
    p {
      margin-right: 5px;
    }
    .red {
      color: red;
    }
    .bold {
      font-weight: bold;
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
}
</style>