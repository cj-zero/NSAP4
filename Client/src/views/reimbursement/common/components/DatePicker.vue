<template>
  <div class="date-picker-wrapper">
    <input type="text" :value="prevValue" style="display: none;">
    <input type="text" :value="nextValue" style="display: none;" v-if="isRange">
    <template>
      <el-row type="flex" align="middle" class="title-wrapper">
        <p>开始日期</p>
        <p>结束日期</p>
        <svg-icon iconClass="flower" class="pointer icon" @click.native="$emit('expand-click')"></svg-icon>
      </el-row>
      <div class="content-wrapper">
        <div class="left">
          <div class="control-pannel">
            <span class="prev" @click="change('prev', 'year')">&lt;&lt;</span>
            <span class="prev" @click="change('prev', 'month')" style="margin-left: 5px;">&lt;</span>
            <span>{{ prevSource.getFullYear() }}年{{ prevSource.getMonth() + 1 }}月</span>
            <span class="next" @click="change('next', 'year')" v-if="!isRange">&gt;&gt;</span>
            <span class="next" @click="change('next', 'month')" v-if="!isRange" style="margin-right: 5px;">&gt;</span>
          </div>
          <div class="week-days-list">
            <span v-for="item in weekList" :key="item">{{ item }}</span>
          </div>
          <div class="divider"></div>
          <div class="date-content-wrapper">
            <div v-for="i in 6" :key="i">
              <span
                @click="clickItem(dateList[(i - 1) * 7 + (j - 1)], 'left')"
                 v-for="j in 7" :key="j" 
                 :class="{ 
                    'marked': isMarked(dateList[(i - 1) * 7 + (j - 1)]),
                    'not-current-month': !isLeftCurrentMonth (dateList[(i - 1) * 7 + (j - 1)]) 
                  }">
                {{ dateList[(i - 1) * 7 + (j - 1)] | showDate }}
              </span>
            </div>
          </div>
        </div>
        <div class="line"></div>
        <div class="right" v-if="isRange">
          <div class="control-pannel">
            <span>{{ nextSource.getFullYear() }}年{{ nextSource.getMonth() + 1 }}月</span>
            <span class="next" @click="change('next', 'year')">&gt;&gt;</span>
            <span class="next" @click="change('next', 'month')" style="margin-right: 5px;">&gt;</span>
          </div>
          <div class="week-days-list">
            <span v-for="item in weekList" :key="item">{{ item }}</span>
          </div>
          <div class="divider"></div>
          <div class="date-content-wrapper">
            <div v-for="i in 6" :key="i">
              <span 
                 @click="clickItem(nextDateList[(i - 1) * 7 + (j - 1)], 'right')"
                v-for="j in 7" :key="j" 
                :class="{ 
                  'marked': isMarked(nextDateList[(i - 1) * 7 + (j - 1)]),
                  'not-current-month': !isRightCurrentMonth (nextDateList[(i - 1) * 7 + (j - 1)]) 
                }">
                {{ nextDateList[(i - 1) * 7 + (j - 1)] | showDate }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script>
import Day from 'dayjs'
import { formatDate } from '@/utils/date'
// const WEEK_LIST = ['日', '一', '二', '三', '四', '五', '六']
export default {
  props: {
    value: {
      type: [Date, Array],
      default: () => new Date()
    },
    markedDateList: {
      type: Array,
      default: () => []
    }
  },
  filters: {
    showDate (date) {
      return Day(date).date()
    }
  },
  computed: {
    isRange () {
      return Array.isArray(this.value)
    }
  },
  watch: { // 6 * 7
    value: {
      immediate: true,
      handler (val) {
        if (Array.isArray(val)) {
          let [prev, next] = val
          this.prevSource = prev
          this.nextSource = next
          this.prevValue = formatDate(prev)
          this.nextValue = formatDate(next)
          this.dateList = this._generateDateList(prev)
          this.nextDateList = this._generateDateList(next)
        } else {
          this.prevSource = val
          this.prevValue = formatDate(val)
          this.dateList = this._generateDateList(val)
        }
      }
    }
  },
  data () {
    return {
      prevSource: '',
      nextSource: '',
      prevValue: '',
      nextValue: '',
      weekList: ['日', '一', '二', '三', '四', '五', '六'],
      dateList: [], // right
      nextDateList: [], // 右边
    }
  },
  methods: {
    caculateFristDate (val) {
      let newDate = Day(val).date(1) // 设置当前月份的1号
      let week = newDate.day() // 获取当前月份一号所在的星期
      let diff = week // 当前日期，跟第一个的差值个数
      let startDate = newDate
      for (let i = 0; i < diff; i++) { // 计算出第一个日期的值
        startDate = new Date(+new Date(newDate) - (24 * 60 * 60 * 1000 * (i + 1))) 
      }
      return startDate instanceof Date ? startDate : new Date(startDate)
    },
    _generateDateList (val) { // 生成日历表
      let startDate = this.caculateFristDate(val)
      let dateList = []
      for (let i = 0; i < 42; i++) {
        dateList.push(new Date(+startDate + i * (24 * 60 * 60 * 1000)))
      }
      return dateList
    },
    isMarked (date) { // 标记markList里存在的数据
      let formDate = formatDate(date)
      return this.markedDateList.includes(formDate)
    },
    isLeftCurrentMonth (val) { // 判断是不是当前月份 左侧
      let year = Day(val).year()
      let month = Day(val).month()
      let prevYear = Day(this.prevSource).year()
      let prevMonth = Day(this.prevSource).month()
      return year === prevYear && month === prevMonth
    },
    isRightCurrentMonth (val) { // 判断是不是当前月份 右侧
      let year = Day(val).year()
      let month = Day(val).month()
      let nextYear = Day(this.nextSource).year()
      let nextMonth = Day(this.nextSource).month()
      return nextYear === year && nextMonth === month
    },
    clickItem (val, type) {
      let date = Day(val)
      let newDate = Day(type === 'left' ? this.nextSource : this.prevSource)
      let dateList = []
      if (this.isRange) {
        // type会prev 说明是向前走(点击的是左侧表格，右侧表格也要随之月份变化), 反之亦然
        let diff = 0
        if (type === 'left' && !this.isLeftCurrentMonth(val)) {
          diff = this.isRightCurrentMonth(val) ? 1 : -1 // 如果选中的日期在右侧日期的范围里
        } else if (type === 'right' && !this.isRightCurrentMonth(val)) {
          diff = this.isLeftCurrentMonth(val) ? -1 : 1 // 如果选中的日期在左侧日期的范围里
        }
        let month = this.getMonth(newDate, diff) // 拿到当前点击的月份
        newDate = newDate.month(month)
        dateList = type === 'left' ? [new Date(date), new Date(newDate)] : [new Date(newDate), new Date(date)]
      }
      this.isRange 
        ? this.$emit('input', dateList)
        : this.$emit('input', new Date(date))
      this.$emit('click', val)
    },
    getMonth (date, diff = 0) {
      return date.month() + diff
    },
    getYear (date, diff = 0) {
      return date.year() + diff
    },
    change (action, dateType) {
      let prevDate = Day(this.prevSource)
      let nextDate = null
      if (this.isRange) {
        nextDate = Day(this.nextSource)
      }
      let diff = (action === 'next' ? 1 : -1 )
      // 月份
      if (dateType === 'month') {
        prevDate = prevDate.month(this.getMonth(prevDate, diff))
        if (this.isRange) {
          nextDate = nextDate.month(this.getMonth(nextDate, diff))
        }
      } else {
        // 年份
        let year = this.getYear(prevDate, diff)
        prevDate = prevDate.year(year)
         if (this.isRange) {
          let year = this.getYear(nextDate, diff)
          nextDate = nextDate.year(year)
        }
      }
      this.isRange 
        ? this.$emit('input', [new Date(prevDate), new Date(nextDate)])
        : this.$emit('input', new Date(prevDate))
    },
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.date-picker-wrapper {
  // width: 200px;
  position: relative;
  width: 500px;
  box-sizing: border-box;
  padding: 0 10px;
  font-size: 12px;
  border-radius: 2px;
  box-shadow: 0 0 1px 2px #eee;
  background-color: #fff;
  .title-wrapper {
    height: 20px;
    line-height: 20px;
    p {
      flex: 1;
      text-align: center;
    }
    .icon {
      position: absolute;
      right: 5px;
      top: 5px;
      font-size: 13px;
    }
  }
  .content-wrapper {
    display: flex;
    position: relative;
      & > div {
      flex: 1;
      &.line {
        flex: 0 0 2px;
      }
    }
  }
  
  .line {
    // position: absolute;
    // top: 0;
    // bottom: 0;
    // left: 50%;
    flex: 1px;
    width: 1px;
    margin: 0 3px;
    background-color: #eee;
  }
  .control-pannel {
    // display: flex;
    // justify-content: space-between;
    text-align: center;
    line-height: 20px;
    .prev {
      float: left;
    }
    .next {
      float: right;
    }
    .prev, .next {
      cursor: pointer;
    }
  }
  .week-days-list {
    display: flex;
    align-items: center;
    justify-content: space-between;
    span {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }
  }
  .divider {
    box-sizing: border-box;
    padding: 0 10px;
    margin-top: 3px;
    border-bottom: 2px solid #eee;
  }
  .date-content-wrapper {
    div {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: 5px;
      line-height: 30px;
      cursor: pointer;
      span {
        width: 30px;
        display: flex;
        align-items: center;
        justify-content: center;
        &.marked {
          color: #fff;
          background-color: #0078d7;
          border-radius: 50%;
        }
        &.not-current-month {
          color: #c0c4cc;
          background-color: white;
          border-radius: none;
        }
      }
    }
  }
}
</style>