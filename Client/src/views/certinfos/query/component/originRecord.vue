<template>
  <el-table :data="tableData">

  </el-table>
</template>

<script>

export default {
  props: {
    tableData: {
      type: Array,
      default () {
        return []
      }
    }
  },
  components: {},
  data () {
    return {
      // headOptoins
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
</style>