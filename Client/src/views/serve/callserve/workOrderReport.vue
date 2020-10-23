<template>
  <div class="report-wrapper">
    <div 
      class="chart-item" 
      style="width:100%;height:350px;"
      ref="chart-item"
      v-for="i in data.length"
      :key="i"
      >
    </div>
  </div>
</template>

<script>
const titleList = {
  'Supervisor': { title: '售后部门呼叫量情况', legend: '售后部门排行' },
  'SalesMan': { title: '销售员呼叫量分布图', legend: '销售员排行' },
  'ProblemType': { title: '问题类型分析', legend: '问题类型排行' },
  'RecepUser': { title: '接单员分布图', legend: '接单员排行' }
}
export default {
  props: {
    data: {
      type: Array,
      default () {
        return []
      }
    }
  },
  watch: {
    data: {
      immediate: true,
      handler () {
        this._initCharts()
      }
    }
  },
  data () {
    return {

    }
  },
  methods: {
    _initCharts () {
      // todo
      let charts = document.getElementsByClassName('chart-item')
      for (let i = 0; i < charts.length; i++) {
        let myChart = global.echarts.init(charts[i])
        let data = this.data[i]
        let xAxisData = data.statList.map(item => item.statName)
        let yAxisData = data.statList.map(item => item.serviceCnt)
      
        let option = {
          title: {
            text: titleList[data.statType].title,
            textAlign: 'left',
            textStyle: {
              fontSize: 28
            },
            left: 'center'
          },
          legend: {
            data: [titleList[data.statType].legend],
            left: 'center',
            bottom: 0,
            tooltip: {
              show: true
            }
          },
          tooltip: {
            show: true
          },
          grid: {
            bottom: '22%'
          },
          xAxis: {
            type: 'category',
            data: xAxisData,
            splitLine: {
              show: false
            },
            offset: 10,
            axisTick: {
              show: false
            },
            axisLabel: {
              interval: 0,
              rotate: data.statType === 'ProblemType' ? -30 : 0
            }
          },
          yAxis: {
            type: 'value',
            axisLine: {
              show: false
            },
            axisTick: {
              show: false
            }
          },
          series: [{
            data: yAxisData,
            type: 'bar',
            name: titleList[data.statType].legend,
            itemStyle: {
              color: '#88c7ff'
            }
          }]
        }
        myChart.setOption(option)
      }
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
.report-wrapper {
  height: 600px;
  overflow: auto;
  .chart-item {
    margin-bottom: 10px;
  }
}
</style>