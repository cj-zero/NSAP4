<template>
  <div class="report-wrapper">
    <el-scrollbar style="height: 100%;">
      <div 
        class="chart-item" 
        style="width:100%;height:350px;"
        ref="chart-item"
        v-for="i in data.length"
        :key="i"
        >
      </div>
    </el-scrollbar>
  </div>
</template>

<script>
const titleList = {
  'Supervisor': { title: '售后部门呼叫量情况', legend: '售后部门排行' },
  'SalesMan': { title: '销售员呼叫量分布图', legend: '销售员排行' },
  'ProblemType': { title: '问题类型分析', legend: '问题类型排行' },
  'RecepUser': { title: '接单员分布图', legend: '接单员排行' },
  'RecepStart': { title: '工单状态分析图', legend: '工单状态' }
}
const callStatus = [
  "待处理" ,
  "已排配" ,
  "已预约" ,
  "在上门" ,
  "在维修" ,
  "已寄回" ,
  "已完成" ,
  "已回访" 
]
const colorMap = {
  1: '#da0721',
  5: '#da0721',
  7: '#74c74b',
  8: '#cbcbcb'
}
// 其它呼叫状态 #f7b868
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
          } 
        }
        let { series, tooltip } = this._normalizeSerias(data)
        option.series = series
        option.tooltip = tooltip
        // console.log(series)
        myChart.setOption(option)
      }
    },
    _normalizeSerias (data) {
      let yAxisData = [], series = [], tooltip = {}
      if (data.statType === 'RecepStart') {
        for (let i = 0; i < data.statList.length; i++) { // 格式化柱状图数据
          let { reportList } = data.statList[i]
          for (let j = 0; j < reportList.length; j++) {
            let { statId, serviceCnt } = reportList[j]
            if (yAxisData[statId - 1]) {
              yAxisData[statId - 1].push(serviceCnt)
            } else {
              yAxisData[statId - 1] = []
              yAxisData[statId - 1].push(serviceCnt)
            }
          }
        }
        for (let i = 0; i < yAxisData.length; i++) {
          series.push({
            data: yAxisData[i],
            type: 'bar',
            name: callStatus[i],
            stack: 1,
            itemStyle: {
              color: colorMap[i + 1] || '#f7b868'
            },
            label: { 
              color: '#000',
              position: 'top'
            }
          })
        }
        const calcFn = function (params) {
          let result = 0
          for (let i = 0; i < series.length; i++) {
            result += series[i].data[params.dataIndex]
          }
          return result
        }
        // 给最后一项添加总量
        series[series.length - 1]['label']['formatter'] = calcFn 
        series[series.length - 1]['label']['show'] = true
        tooltip = {
          trigger: 'axis',
          axisPointer: {            // 坐标轴指示器，坐标轴触发有效
            type: 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
          }
        }
      } else {
        yAxisData = data.statList.map(item => item.serviceCnt)
        series = [{
          data: yAxisData,
          type: 'bar',
          name: titleList[data.statType].legend,
          itemStyle: {
            color: '#88c7ff'
          },
          label: { 
            color: '#000',
            position: 'top',
            show: true
          }
        }]
        tooltip = {
          show: true
        }
      }
      return { series, tooltip }
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
  // overflow: auto;
  overflow: hidden;
  ::v-deep .el-scrollbar__wrap {
    overflow-x: hidden;
  }
  .chart-item {
    margin-bottom: 10px;
  }
}
</style>