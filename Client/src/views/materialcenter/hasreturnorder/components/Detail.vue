<template>
  <div class="has-return-wrapper">
    <!-- 头部信息 -->
    <el-row type="flex" align="middle" class="head-title-wrapper">
      <p class="bold red">服务ID: {{ formData.U_SAP_ID }}</p>
      <p class="bold">申请人: {{ formData.CreateUser }}</p>
      <p>销售员: {{ formData.SalesMan }}</p>
      <p>创建时间: {{ formData.CreateTime }}</p>
    </el-row>
    <!-- 表单 -->
    <el-form size="mini" label-width="100px" :module="formData">
      <el-row type="flex">
        <el-col :span="8">
          <el-form-item label="客户代码" required>
            <el-input v-model="formData.CustomerId" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="16">
          <el-form-item label="客户名称" required>
            <el-input v-model="formData.CustomerName" disabled></el-input>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
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
import CommonTable from '@/components/CommonTable' // 对于不可编辑的表格
export default {
  components: {
    CommonTable
  },
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