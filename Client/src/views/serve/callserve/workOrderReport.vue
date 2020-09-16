<template>
  <div class="report-wrapper">
    <div 
      class="chart-item" 
      style="width:100%;height:300px;"
      ref="chart-item"
      v-for="i in data.length"
      :key="i"
      >
    </div>
  </div>
</template>

<script>
// import echarts from 'echarts'
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
      console.log(charts, 'charts')
      for (let i = 0; i < charts.length; i++) {
        let myChart = window.echarts.init(charts[i])
        let data = this.data[i]
        let xAxisData = data.statList.map(item => item.statName)
        let yAxisData = data.statList.map(item => item.serviceCnt * 100)
      
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
          xAxis: {
            type: 'category',
            data: xAxisData,
            splitLine: {
              show: false
            },
            axisTick: {
              show: false
            },
            axisLabel: {
              rotate: data.statType === 'ProblemType' ? -45 : 0
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
    //       title: {
    //     text: '堆叠区域图'
    // },
    // tooltip: {
    //     trigger: 'axis',
    //     axisPointer: {
    //         type: 'cross',
    //         label: {
    //             backgroundColor: '#6a7985'
    //         }
    //     }
    // },
    // legend: {
    //     data: ['联盟广告', '视频广告', '直接访问', '搜索引擎']
    // },
    
   
    // xAxis: [
    //     {
    //         type: 'category',
    //         boundaryGap: false,
    //         data: ['周一', '周二', '周三', '周四', '周五', '周六', '周日']
    //     }
    // ],
    // yAxis: [
    //     {
    //         type: 'value'
    //     }
    // ],
    // series: [
    //     {
    //         // name: '邮件营销',
    //         type: 'line',
    //         stack: '总量',
    //         areaStyle: {},
    //         data: [120, 132, 101, 134, 90, 230, 210]
    //     },
    //     {
    //         // name: '联盟广告',
    //         type: 'line',
    //         stack: '总量',
    //         areaStyle: {},
    //         data: [220, 182, 191, 234, 290, 330, 310]
    //     },
    //     {
    //         name: '视频广告',
    //         type: 'line',
    //         stack: '总量',
    //         areaStyle: {},
    //         data: [150, 232, 201, 154, 190, 330, 410]
    //     },
    //     {
    //         name: '直接访问',
    //         type: 'line',
    //         stack: '总量',
    //         areaStyle: {},
    //         data: [320, 332, 301, 334, 390, 330, 320]
    //     },
    //     {
    //         name: '搜索引擎',
    //         type: 'line',
    //         stack: '总量',
    //         label: {
    //             normal: {
    //                 show: true,
    //                 position: 'top'
    //             }
    //         },
    //         areaStyle: {},
    //         data: [820, 932, 901, 934, 1290, 1330, 1320]
    //     }
    // ]
        }
        console.log('setOptoins', option)
        myChart.setOption(option)
      }
    }
  },
  created () {

  },
  mounted () {
    console.log(titleList)
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