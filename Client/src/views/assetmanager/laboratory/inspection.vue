<template>
  <div>
    送检记录
    <el-table :data="tableData" :cell-style="cellStyle">
      <el-table-column label="序号">
        <template slot-scope="scope">
          {{ scope.row.$index }}
        </template>
      </el-table-column>
      <el-table-column label="校准日期">
        <template slot-scope="scope">
          {{ scope.row.$index }}
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>

export default {
  components: {},
  data () {
    return {

    }
  },
  methods: {
    cellStyle () {
      return {
        'text-align': 'center',
        'vertical-align': 'middle'
      }
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