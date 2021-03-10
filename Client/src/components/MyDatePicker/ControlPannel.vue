<template>
  <transition :name="animationName" @after-leave="handleLeave">
    <div class="content-wrapper" v-show="visible" 
      :class="{ 
        'hidden-single': !isRange && !this.isShowInput, 
        'hidden-double': isRange && !this.isShowInput,
        'show-input': this.isShowInput,
      }"
    >
      <div class="left">
        <div class="control-pannel">
          <div class="prev">
            <el-icon class="el-icon-d-arrow-left" @click.native="change('prev', 'year')"></el-icon>
            <el-icon class="el-icon-arrow-left" @click.native="change('prev', 'month')"></el-icon>
          </div>
          <span class="date-text">{{ prevSource.getFullYear() }}年{{ prevSource.getMonth() + 1 }}月</span>
          <div class="next" v-if="!isRange">
            <el-icon class="el-icon-d-arrow-right" @click.native="change('next', 'year')"></el-icon>
            <el-icon class="el-icon-arrow-right" @click.native="change('next', 'month')"></el-icon>
          </div>
        </div>
        <div class="week-days-list">
          <span v-for="item in weekList" :key="item">{{ item }}</span>
        </div>
        <div class="date-content-wrapper">
          <div v-for="i in 6" :key="i">
            <div
              class="cell"
              @click="clickItem(dateList[(i - 1) * 7 + (j - 1)], 'left')"
              v-for="j in 7" :key="j" 
              >
              <span
                :class="{ 
                  'selected': isSelected(dateList[(i - 1) * 7 + (j - 1)], 'left'),
                  'current-date': isCurrentDate(dateList[(i - 1) * 7 + (j - 1)]),
                  'marked': isMarked(dateList[(i - 1) * 7 + (j - 1)]),
                  'not-current-month': !isLeftCurrentMonth (dateList[(i - 1) * 7 + (j - 1)]) 
                }"
              >{{ dateList[(i - 1) * 7 + (j - 1)] | showDate }}</span>
            </div>
          </div>
        </div>
      </div>
      <div class="line" v-if="isRange"></div>
      <div class="right" v-if="isRange">
        <div class="control-pannel">
          <span class="date-text">{{ nextSource.getFullYear() }}年{{ nextSource.getMonth() + 1 }}月</span>
          <div class="next">
            <el-icon class="el-icon-arrow-right" @click.native="change('next', 'month')"></el-icon>
            <el-icon class="el-icon-d-arrow-right" @click.native="change('next', 'year')"></el-icon>
          </div>
        </div>
        <div class="week-days-list">
          <span v-for="item in weekList" :key="item">{{ item }}</span>
        </div>
        <div class="date-content-wrapper">
          <div v-for="(i, row) in 6" :key="row">
            <div
              class="cell"
              @click="clickItem(nextDateList[(i - 1) * 7 + (j - 1)], 'right')"
              v-for="(j, column) in 7" :key="column" 
              >
              <span 
                :class="{ 
                'selected': isSelected(nextDateList[(i - 1) * 7 + (j - 1)], 'right'),
                'current-date': isCurrentDate(nextDateList[(i - 1) * 7 + (j - 1)]),
                'marked': isMarked(nextDateList[(i - 1) * 7 + (j - 1)]),
                'not-current-month': !isRightCurrentMonth (nextDateList[(i - 1) * 7 + (j - 1)]) 
                }"
              >
                {{ nextDateList[(i - 1) * 7 + (j - 1)] | showDate }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </transition>
</template>

<script>
import Day from 'dayjs'
import { formatDate } from '@/utils/date'
const prevDate = new Date()
// const nextDate = new Date(Day().add(1, 'month'))
export default {
  filters: {
    showDate (date) {
      return date ? Day(date).date() : ''
    }
  },
  computed: {
    animationName () {
      return this.isShowInput ? 'el-zoom-in-top' : ''
    }
  },
  data () {
    return {
      visible: false,
      isShowInput: false,
      isSingleClick: false, // 是否单次点击关闭弹窗
      value: '',
      isRange: false,
      weekList: ['日', '一', '二', '三', '四', '五', '六'],
      dateList: [], // right
      nextDateList: [], // 右边
      prevSource: '',
      nextSource: '',
      leftValue: '',
      rightValue: '',
      markedDateList: []
    }
  },
  watch: {
    value: {
      immediate: true,
      handler (val) {
        console.log('controlpannel', val)
        if (Array.isArray(val)) {
          let [prev] = val
          this.prevSource = prev || prevDate
          console.log(this.prevSource.getFullYear(), this.prevSource.getMonth() + 1)
          this.nextSource = new Date(Day(this.prevSource).add(1, 'month'))
          console.log(this.nextSource.getFullYear(), this.nextSource.getMonth() + 1)
          console.log(this.prevSource, this.nextSource, 'watch copy')
          this.dateList = this._generateDateList(this.prevSource)
          this.nextDateList = this._generateDateList(this.nextSource)
        } else {
          this.prevSource = val || prevDate
          this.dateList = this._generateDateList(this.prevSource)
        }
      }
    },
    prevSource (val) {
      console.log('prevDate', val)
      this.dateList = this._generateDateList(this.prevSource)
    },
    nextSource (val) {
      console.log('nextDate', val)
      this.nextDateList = this._generateDateList(this.nextSource)
    }
  },
  methods: {
    handleLeave() {
      console.log('destroy')
      // this.$emit('dodestroy')
    },
    isCurrentDate (date) {
      return formatDate(this.currentDate) === formatDate(date)
    },
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
    isSelected (val, type) {
      return formatDate(type === 'left' ? this.leftValue : this.rightValue) === formatDate(val)
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
    setTag (type) {
      if (this.isShowInput && this.isRange) {
        if (!this.selectList.length) {
          this.selectList.push(type)
        } else {
          if (this.selectList[0] !== type) {
            this.selectList.push(type)
          }
        }
      }
    },
    setCurrentValue (val, type) { // 设置当前选中的时间
      if (type === 'left') {
        this.leftValue = val
      } else {
        this.rightValue = val
      }
    },
    clickItem (val, type) {
      this.setTag(type)
      this.setCurrentValue(val, type)
      if (this.isSingleClick) {
        this.leftValue = this.rightValue = val
        this.$emit('pick', [val, val])
      } else if (this.isShowInput && this.isRange) {
        if (this.selectList.length === 2) {
          this.selectList = []
          this.$emit('pick', [this.leftValue, this.rightValue])
        }
      }
      this.$emit('click', val)
    },
    getMonth (date, diff = 0) {
      return date.month() + diff
    },
    getYear (date, diff = 0) {
      return date.year() + diff
    },
    change (action, dateType) {
      console.log('change', action, dateType)
      let prevDate = Day(this.prevSource)
      let nextDate = null
      if (this.isRange) {
        nextDate = Day(this.nextSource)
      }
      let diff = (action === 'next' ? 1 : -1 )
      // 月份
      if (dateType === 'month') {
        this.prevSource = new Date(prevDate.month(this.getMonth(prevDate, diff)))
        if (this.isRange) {
          this.nextSource = new Date(nextDate.month(this.getMonth(nextDate, diff)))
        }
      } else {
        // 年份
        let year = this.getYear(prevDate, diff)
        this.prevSource = new Date(prevDate.year(year))
         if (this.isRange) {
          let year = this.getYear(nextDate, diff)
          this.nextSource = new Date(nextDate.year(year))
        }
      }
    },
  },
  created () {
    this.selectList = []
  }
}
</script>

<style lang="scss" scoped>
.content-wrapper {
  display: flex;
  position: relative;
  // width: 500px;
  // padding: 0 10px;
  font-size: 12px;
  border-radius: 2px;
  box-shadow: 0 0 1px 2px #eee;
  background-color: #fff;
  line-height: 30px;
  &.hidden-single {
    width: 260px;
  }
  &.hidden-double {
    width: 520px;
  }
  &.show-input {
    // position: absolute;
    width: 515px;
  //   position: absolute;
  //   top: 40px;
  //   left: 0;
  }
    & > div {
    flex: 1;
    &.left, &.right {
      padding: 0 15px;
    }
    &.line {
      flex: 0 0 2px;
    }
  }
  .line {
    flex: 1px;
    width: 1px;
    margin: 0 3px;
    background-color: #eee;
  }
  .control-pannel {
    text-align: center;
    &.date-text {
      font-size: 15px;
      font-weight: 500;
    }
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
      padding: 5px;
      color: #606266;
      border-bottom: 1px solid #ebeef5;
    }
  }
  .date-content-wrapper {
    div {
      display: flex;
      align-items: center;
      justify-content: space-between;
      // margin-top: 5px;
      .cell {
        padding: 6px 4px;
        cursor: pointer;
          span {
          box-sizing: content-box;
          display: flex;
          align-items: center;
          justify-content: center;
          width: 24px;
          height: 24px;
          
          line-height: 24px;
          &.selected {
            border-radius: 50%;
            background-color: rgba(248, 181, 0, 1);
          }
          &.current-date {
            color: #0078d7;
          }
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
}
  
  
</style>