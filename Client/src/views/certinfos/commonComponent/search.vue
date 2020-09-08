<template>
  <div class="search-wrapper w949">
    <el-form :model="form" inline label-width="90px" size="mini">
      <el-form-item label="证书编号">
        <el-input v-model="form.certNo" style="width: 100px;"></el-input>
      </el-form-item>
      <el-form-item label="型号规格">
        <el-input v-model="form.model" style="width: 100px;" ></el-input>
      </el-form-item>
      <el-form-item label="出厂编号">
        <el-input v-model="form.sn" style="width: 100px;"></el-input>
      </el-form-item>
      <el-form-item label="资产编号">
        <el-input v-model="form.assetNo" style="width: 100px;"></el-input>
      </el-form-item>
      <el-form-item label="校准人" v-if="type !== 'submit'">
        <el-input v-model="form.operator" style="width: 100px;"></el-input>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" class="el-icon-search" size="mini" @click="search">搜索</el-button>
      </el-form-item>
      <el-form-item label="校准日期">
        <el-date-picker
          style="width: 370px;"
          v-model="form.date"
          type="daterange"
          value-format="yyyy-MM-dd"
          format="yyyy 年 MM 月 dd 日"
          range-separator="至"
          start-placeholder="开始日期"
          end-placeholder="结束日期"
          @change="onDateChange">
        </el-date-picker>
      </el-form-item>
    </el-form>
  </div>
</template>

<script>

export default {
  props: {
    type: {
      type: String,
      default: ''
    }
  },
  components: {},
  data () {
    return {
      form: {
        certNo: '', // 证书编号
        model: '', // 设备型号
        sn: '', // 设备出厂编号
        assetNo: '', // 资产编号
        operator: '', // 校准人
        calibrationDateFrom: '', // 校准开始日期
        calibrationDateTo: '' // 校准结束日期
      }
    }
  },
  methods: {
    search () {
      this.$emit('search', this.form)
    },
    onDateChange (val) {
      if (val) {
        this.form.calibrationDateFrom = val[0]
        this.form.calibrationDateTo = val[1]
      }
      console.log(val, 'dateVal', this.form)
    }
  },
  watch: {
    'form.date' (newVal) {
      if (!newVal) {
        this.form.calibrationDateFrom = ''
        this.form.calibrationDateTo = ''
      }
      console.log(newVal, 'dateChange')
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.search-wrapper {
  margin-top: 10px;
}
</style>