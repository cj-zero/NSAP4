<template>
  <div class="daily-wrapper">
    <!-- 日期选择器 -->
    <MyDatePicker 
      v-model="dateList" 
      @input="onInput" 
      :isSingleClick="true" 
      :isShowInput="true" 
      :isRange="true"
      :markedDateList="markedDateList"
      style="width: 282px !important;"
    />
    <!-- 时间段 -->
    <p class="date-text">{{ dateStr }}</p>
    <template v-if="dailyList && !dailyList.length">
      <div class="empty-text">
        今日，暂无填写日报
      </div>
    </template>
    <!-- 日报列表 -->
    <el-scrollbar class="scrollbar" wrapClass="scroll-wrap-class" v-loading="loading">
      <template v-if="dailyList && dailyList.length">
        <ul class="content-wrapper">
          <li class="list-item" v-for="item in dailyList" :key="item.id">
            <el-row type="flex" justify="space-between">
              <span class="serialNumber">{{ item.manufacturerSerialNumber }}</span>
              <el-icon class="pointer" @click.native="handleClick(item)" :class="['el-icon-arrow-down', 'icon', item.isActive ? 'active' : '']"></el-icon>
            </el-row>
            <el-row class="content-list" type="flex">
              <div class="title">Q:</div>
              <el-scrollbar class="content-scrollbar" wrapClass="scroll-wrap-class">
                <div class="list question">
                  <div v-for="(question, index) in item.troubleDescription" :key="index">{{ question }}</div>
                </div>
              </el-scrollbar>
            </el-row>
            <el-row class="content-list" type="flex">
              <div class="title">A:</div>
              <el-scrollbar class="content-scrollbar" wrapClass="scroll-wrap-class">
                <div class="list answer">
                  <div v-for="(answer, index) in item.processDescription.slice(0, 1)" :key="index">{{ answer }}</div>
                  <transition-group name="el-zoom-in-top">
                    <div v-show="item.isActive" v-for="(answer, index) in item.processDescription.slice(1)" :key="answer + index">{{ answer }}</div>
                  </transition-group>
                </div>
              </el-scrollbar>
            </el-row>
          </li>
        </ul>
      </template>
    </el-scrollbar>
  </div>
</template>

<script>
import MyDatePicker from '@/components/MyDatePicker'
import { formatDate, toDate } from '@/utils/date'
import { getDailyReport } from '@/api/serve/callservesure'
import { findIndex } from '@/utils/process'
const NOW_DATE = new Date()
export default {
  components: {
    MyDatePicker
  },
  computed: {
    dateStr () {
      return this.dateList[1] ? formatDate(this.dateList[1]) : ''
    }
  },
  props: {
    serviceOrderId: {
      type: [String, Number]
    },
    isFinish: {
      type: Boolean
    }
  },
  watch: {
    serviceOrderId: {
      immediate: true,
      handler (val) {
        if (val) {
          this._getDailyReport({ serviceOrderId: val })
        }
      }
    }
  },
  data () {
    return {
      dateList: [NOW_DATE, NOW_DATE],
      dailyList: [],
      markedDateList: [], //  日报日期
      loading: false,
      cancelRequestFn: null
    }
  },
  methods: {
    onInput (val) {
      console.log(val, 'date-val')
      // this.dailyList = [formatDate(val[0]), formatDate(val[1])]
      const [startDate, endDate] = [formatDate(val[0]), formatDate(val[1])]
      this._getDailyReport({ serviceOrderId: this.serviceOrderId, startDate, endDate })
    },
    handleClick (item) {
      if (item.processDescription.length > 1) {
        item.isActive = !item.isActive
      }
    },
    async _getDailyReport (params) {
      if (this.cancelRequestFn) {
        this.cancelRequestFn()
      }
      this.dailyList = []
      this.loading = true
      const { endDate, startDate } = params
      try {
        const res = await getDailyReport(params, this)
        const { dailyDates, reportResults } = res.data
        this.markedDateList = dailyDates
        if (!dailyDates.length && !endDate && !startDate) { // 没有工作日报
          this.dateList = [NOW_DATE, NOW_DATE]
        } else { 
          // 有工作日报
          if (!endDate && !startDate) {
            const firstDate = dailyDates[dailyDates.length - 1]
            console.log(this.isFinish, 'get daily before')
            const lastDate = this.isFinish ? dailyDates[0] : NOW_DATE
            this.dateList = [toDate(firstDate), toDate(lastDate)]
          }
        }
        const list = []
        const index = findIndex(reportResults, item => {
          return item.dailyDate === formatDate(this.dateList[1])
        })
        if (index > - 1) {
          const { reportDetails } = reportResults[index]
          reportDetails.forEach(materialItem => {
            list.push({
              ...materialItem,
              isActive: false
            })
          })
        }
        this.dailyList = list  
        // 如果服务单已经完成 日报初始-日报结束
        // 服务单未完成 日报初始日期-当天
      } catch (err) {
        this.$message.error(err.message)
      } finally {
        this.loading = false
        console.log('finally')
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
.daily-wrapper {
  position: relative;
  font-size: 12px;
  min-height: 500px;
  .content-wrapper {
    padding: 0 10px;
  }
  .empty-text {
    position: absolute;
    top: 100px;
    left: 0;
    right: 0;
    margin: 0 auto;
    font-size: 18px;
    text-align: center;
    font-weight: bold;
  }
  .date-text {
    margin: 5px 0;
    text-align: center;
    font-size: 15px;
    font-weight: bold; 
  }
  /* 问题scrollbar样式 */
  ::v-deep .content-scrollbar {
    flex: 1;
      &.el-scrollbar {
        & > .scroll-wrap-class {
        max-height: 150px !important; // 最大高度
        overflow-x: hidden; // 隐藏横向滚动栏
        margin-bottom: 0 !important;
      }
    }
  }
  ::v-deep .scrollbar {
    position: relative;
    &.el-scrollbar {
      & > .scroll-wrap-class {
        max-height: 400px; // 最大高度
        overflow-x: hidden; // 隐藏横向滚动栏
        margin-bottom: 0 !important;
      }
    }
    .list-item {
      padding: 3px 0;
      border-bottom: 1px #c6c6c6 solid;
      .icon {
        cursor: pointer;
        transition: all .5s;
        &.active {
          transform: rotate(180deg);
        }
      }
      & > div {
        margin-top: 3px;
      }
      .content-list {
        flex: 1;
        .title {
          width: 20px;
          text-align: center;
          line-height: 22px;
        }
        .list {
          flex: 1;
          &.question div {
            display: inline-block;
            padding: 0 2px;
            margin: 1px 3px;
            height: 20px;
            line-height: 20px;
            text-align: center;
            background-color: #169bd5;
            color: #fff;
            border-radius: 10px;
          }
          &.answer div {
            padding: 0 2px;
            margin: 2px 11px 2px 0;
            background-color: #d7d7d7;
            border-radius: 5px;
            line-height: 20px;
          }
        }
      }
    }
  }
}
</style>
