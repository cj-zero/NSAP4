<template>
  <div class="date-picker-wrapper">
    <input type="text" :value="formatValue" style="display: none;">
    <div class="control-pannel">
      <span class="prev" @click="change('prev', 'year')">&lt;&lt;</span>
      <span class="prev" @click="change('prev', 'month')">&lt;</span>
      <span>{{ value.getFullYear() }}年{{ value.getMonth() + 1 }}月</span>
      <span class="next" @click="change('next', 'month')">&gt;</span>
      <span class="next" @click="change('next', 'year')">&gt;&gt;</span>
    </div>
    <div class="week-days-list">
      <span v-for="item in weekList" :key="item">{{ item }}</span>
    </div>
    <div class="divider"></div>
    <div class="date-content-wrapper">
      <div v-for="i in 6" :key="i">
        <span v-for="j in 7" :key="j" :class="{ 'marked': isMarked(dateList[(i - 1) * 7 + (j - 1)]) }">
          {{ dateList[(i - 1) * 7 + (j - 1)] | showDate }}
        </span>
      </div>
    </div>
  </div>
</template>

<script>
import Day from 'dayjs'
import { formatDate } from '@/utils/date'
// const WEEK_LIST = ['日', '一', '二', '三', '四', '五', '六']
export default {
  props: {
    value: {
      type: Date,
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
  watch: { // 6 * 7
    value: {
      immediate: true,
      handler (val) {
        this.formatValue = formatDate(val, 'YYYY-MM-DD')
        let day = Day(val).date() // 获取当前是几号
        let week = Day(val).day() // 获取当前是第几周
        let diff = (day - 1) + week// 当前日期，跟第一个的差值个数
        for (let i = 0; i < diff; i++) { // 计算出第一个日期的值
          val = new Date(+val - (24 * 60 * 60 * 1000)) 
        }
        let startDate = val
        this._generateDateList(startDate)
      }
    }
  },
  data () {
    return {
      formatValue: '',
      weekList: ['日', '一', '二', '三', '四', '五', '六'],
      dateList: []
    }
  },
  methods: {
    _generateDateList (startDate) {
      let dateList = []
      for (let i = 0; i < 42; i++) {
        dateList.push(new Date(+startDate + i * (24 * 60 * 60 * 1000)))
      }
      this.dateList = dateList
    },
    isMarked (date) {
      let formDate = formatDate(date, 'YYYY-MM-DD')
      return this.markedDateList.includes(formDate)
    },
    change (action, dateType) {
      let date = new Date(this.formatValue)
      let diff = (action === 'next' ? 1 : -1 )
      if (dateType === 'month') {
        let month = Day(this.value).month() + diff
        date.setMonth(month)
      } else {
        let year = Day(this.value).year() + diff
        date.setYear(year)
      }
      this.$emit('input', new Date(date))
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
  width: 200px;
  box-sizing: border-box;
  padding: 0 10px;
  font-size: 12px;
  border-radius: 2px;
  box-shadow: 0 0 1px 2px #eee;
  background-color: #fff;
  .control-pannel {
    display: flex;
    justify-content: space-between;
    line-height: 20px;
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
      line-height: 20px;
      span {
        width: 25px;
        display: flex;
        align-items: center;
        justify-content: center;
        &.marked {
          color: #fff;
          background-color: #0078d7;
          border-radius: 50%;
        }
      }
    }
  }
}
</style>