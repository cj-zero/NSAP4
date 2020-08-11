<template>
  <el-form :inline="true" :model="formData" class="demo-form-inline" label-width="60px">
    <el-row>
      <el-col :span="3">
        <el-form-item label="资产ID">
          <el-input v-model="form.user" placeholder="审批人"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="类别">
          <el-select v-model="assetCategoryRes" placeholder="请选择" @change="onTypeChange">
            <el-option v-for="item in assetCategory" :value="item" :key="item.name"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="型号">
          <el-input placeholder="请输入内容" v-model="formData.assetType"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="出厂编号">
          <el-input placeholder="请输入内容" v-model="formData.assetCCNumber"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="资产编号">
          <el-input placeholder="请输入内容" v-model="formData.assetZCNumber"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="部门">
          <el-input placeholder="请输入内容" v-model="formData.orgName"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row>
      <el-col :span="4">
        <el-form-item label="类型">
          <el-select v-model="formData.assetSJType" placeholder="请选择">
            <el-option v-for="item in assetSJType" :key="item"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="状态">
          <el-select v-model="formData.assetStatus" placeholder="请选择状态">
            <el-option v-for="item in item in assetStatus" :value="item" :key="item"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="6">
        <el-date-picker
          v-model="value1"
          type="daterange"
          range-separator="至"
          start-placeholder="开始日期"
          end-placeholder="结束日期">
        </el-date-picker>
      </el-col>
      <el-col :span="4">
        <el-form-item>
          <el-button type="primary" @click="search" size="mini" icon="el-icon-search">查询</el-button>
        </el-form-item>
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
import assetMixin from './mixin/mixin'
export default {
  mixins: [assetMixin],
  data () {
    return {
      formData: {
        id: '',
        assetCategory: '',
        assetType: '',
        assetCCNumber: '',
        assetZCNumber: '',
        orgName: '',
        assetSJType: '',
        assetStatus: '',
        assetJZData: '',
        assetSXDate: ''
      }
    }
  },
  methods: {
    search () {
      this.$emit('search', this.form)
    }
  },
  created () {

  },
  mounted () {
    
  },
}
</script>
<style lang='scss' scoped>
</style>