<template>
  <el-form :inline="true" :model="formInline" class="demo-form-inline" label-width="60px">
    <el-form-item label="资产ID">
      <el-input v-model="formInline.user" placeholder="审批人"></el-input>
    </el-form-item>
    <el-form-item label="活动区域">
      <el-select v-model="formInline.region" placeholder="活动区域">
        <el-option label="区域一" value="shanghai"></el-option>
        <el-option label="区域二" value="beijing"></el-option>
      </el-select>
    </el-form-item>
    <el-form-item>
      <el-button type="primary" @click="search" size="mini" icon="el-icon-search">查询</el-button>
    </el-form-item>
  </el-form>
</template>

<script>

export default {
  components: {},
  data () {
    return {
      form: {
        id: '',
        assetCategory: '',
        assetType: '',
        assetCCNumber: '',
        assetZCNumber: '',
        orgId: '',
        assetSJType: '',
        assetStatus: '',
        assetJZData: '',
        assetSXDate: ''
      }
    }
  },
  methods: {
    search () {
      //
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