<template>
  <div class="report-wrapper">
    <div 
      class="chart-item" 
      style="width:100%;height:400px;"
      ref="chart-item"
      >
    </div>
  </div>
</template>

<script>
// import echarts from 'echarts'
export default {
  props: {

  },
  data () {
    return {

    }
  },
  methods: {
    _initCharts () {
      // todo
    }
  },
  created () {

  },
  mounted () {
    this._initCharts()
  },
}
</script>
<style lang='scss' scoped>
</style>