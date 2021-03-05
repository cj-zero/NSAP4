<template>
  <div class="daily-wrapper">
    <!-- 日期选择器 -->
    <MyDatePicker 
      v-model="dateList" 
      @input="onInput" 
      :isSingleClick="true" 
      :isShowInput="true" 
      :isRange="true" 
      style="width: 282px !important;"
    />
    <!-- 时间段 -->
    <p class="date-text">{{ dateStr }}</p>
    <!-- 日报列表 -->
    <el-scrollbar class="scrollbar" wrapClass="scroll-wrap-class">
      <ul class="content-wrapper">
        <li class="list-item" v-for="item in textData" :key="item.id">
          <el-row type="flex" justify="space-between">
            <span class="serialNumber">{{ item.number }}</span>
            <el-icon @click.native="handleClick(item)" :class="['el-icon-arrow-down', 'icon', item.isActive ? 'active' : '']"></el-icon>
          </el-row>
          <el-row class="content-list" type="flex">
            <div class="title">Q:</div>
            <el-scrollbar class="content-scrollbar" wrapClass="scroll-wrap-class">
              <div class="list question">
                <div v-for="question in item.questions" :key="question">{{ question }}</div>
              </div>
            </el-scrollbar>
          </el-row>
          <el-row class="content-list" type="flex">
            <div class="title">A:</div>
            <el-scrollbar class="content-scrollbar" wrapClass="scroll-wrap-class">
              <div class="list answer">
                <div v-for="answer in item.answers.slice(0, 1)" :key="answer">{{ answer }}</div>
                <transition-group name="el-zoom-in-top">
                  <div v-show="item.isActive" v-for="answer in item.answers.slice(1)" :key="answer">{{ answer }}</div>
                </transition-group>
              </div>
            </el-scrollbar>
          </el-row>
        </li>
      </ul>
    </el-scrollbar>
  </div>
</template>

<script>
import MyDatePicker from '@/components/MyDatePicker'
import { formatDate } from '@/utils/date'
const NOW_DATE = new Date()
const textData = []
for(let i = 0; i < 10; i++) {
  textData.push({
    id: i,
    number: '12321312321',
    isActive: false,
    questions: ['123', '123123123', '123213'],
    answers: [i, i + 1, i + 2, i + 3, i + 4, i + 5, i + 6, i + 7]
  })
}
export default {
  components: {
    MyDatePicker
  },
  computed: {
    dateStr () {
      return this.dateList[0] ? formatDate(this.dateList[0]) : ''
    }
  },
  data () {
    return {
      dateList: [NOW_DATE, NOW_DATE],
      textData
    }
  },
  methods: {
    onInput (val) {
      console.log(val, 'val')
    },
    handleClick (item) {
      if (item.answers.length > 1) {
        item.isActive = !item.isActive
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
  font-size: 12px;
  
  .content-wrapper {
    padding: 0 10px;
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
            margin: 0 3px;
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
