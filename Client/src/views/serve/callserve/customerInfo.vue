<template>
  <el-form 
    :model="formData"
    disabled
    label-width="100px"
    class="form-wrapper"
    size="mini"
  >
    <el-row type="flex" justify="space-around">
      <el-form-item label="客户代码">
        <el-input v-model="formData.cardCode"></el-input>
      </el-form-item>
      <el-form-item label="客户名称">
        <el-input v-model="formData.cardName"></el-input>
      </el-form-item>
      <el-form-item label="更新时间">
        <el-input v-model="formData.updateDate"></el-input>
      </el-form-item>
    </el-row>
    <el-row type="flex" justify="space-around">
      <el-form-item label="状态冻结">
        <el-input v-model="status"></el-input>
      </el-form-item>
      <el-form-item label="客户联系人">
        <el-input v-model="formData.cntctPrsn"></el-input>
      </el-form-item>
      <el-form-item label="客户电话">
        <el-input v-model="formData.cellular"></el-input>
      </el-form-item>
    </el-row>
    <el-row type="flex" justify="space-around">
      <el-form-item label="销售员">
        <el-input v-model="formData.slpName"></el-input>
      </el-form-item>
      <el-form-item label="售后技术员">
        <el-input v-model="formData.technician"></el-input>
      </el-form-item>
      <el-form-item label="科目余额">
        <el-input v-model="formData.balance" :class="getColor(formData.balance)"></el-input>
      </el-form-item>
    </el-row>
    <el-row>
      <el-form-item label="客户地址">
        <el-input v-model="formData.address"></el-input>
      </el-form-item>
    </el-row>
  </el-form>
</template>

<script>

export default {
  props: {
    formData: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {

    }
  },
  computed: {
    status () {
      if (this.formData.frozenFor) {
        return this.formData.frozenFor === 'N' ? '正常' : '冻结'
      }
      return ''
    }
  },
  methods: {
    getColor (num) {
      return num >= 0 ? '' : 'red-color'
    }
  }
}
</script>
<style lang='scss' scoped>
.form-wrapper {
  ::v-deep .el-form-item{
    margin-bottom:5px;
  }
  ::v-deep .el-input.is-disabled .el-input__inner {
    background-color: #fff;
    cursor: default;
    color: #606266;
    border-color: #DCDFE6;
  }
}
.red-color {
  color: #ff0000;
}
</style>