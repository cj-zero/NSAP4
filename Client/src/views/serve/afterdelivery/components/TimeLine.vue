<template>
  <div class="time-line-wrapper">
    <el-scrollbar class="scroll-bar-wrapper">
      <el-timeline>
        <el-timeline-item
          class="time-item"
          v-for="(expressInfo, index) in expressInfoList"
          :key="index"
        >
          <template v-slot:dot>
            <!-- 最后一条数据 -->
            <el-row class="dot-wrapper" type="flex" align="middle" justify="center">
              <i
                :class="[
                  { 'lastest': index === 0 },
                  getIconClass(index),
                  'icon'
                ]"
              ></i>
            </el-row>
          </template>
          <el-row type="flex" align="middle">
            <div class="time-wrapper"  :class="{ 'lastest': index === 0 }">
              <p class="text-overflow">{{ expressInfo.dateTime }}</p>
              <p class="text-overflow">{{ expressInfo.hourMins }} 星期{{ expressInfo.week }} </p>
              <p class="status text-overflow">{{ expressInfo.status }}</p>
            </div>
            <div class="context" :class="{ 'lastest': index === 0 }">
              <p>{{ expressInfo.context }}</p>
            </div>
          </el-row>
        </el-timeline-item>
      </el-timeline>
    </el-scrollbar>
    
  </div>
</template>

<script>
export default {
  components: {},
  props: {
    currentData: {
      type: Object,
      default: () => {}
    },
    expressInfoList: {
      type: Array,
      default: () => []
    }
  },
  data () {
    return {
     
    }
  },
  methods: {
    getIconClass (index) {
      let length = this.expressInfoList.length - 1
      console.log(index)
      return index === 0
        ? 'el-icon-success last'
          : (index === length ? 'first-icon' : 'el-icon-arrow-up middle')
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.scroll-bar-wrapper {
  font-size: 12px;
  color: #969696;
  &.el-scrollbar {
      ::v-deep {
      .el-scrollbar__wrap {
        max-height: 400px; // 最大高度
        margin-right: -17px;
        overflow-x: hidden; // 隐藏横向滚动栏
        margin-bottom: 0 !important;
      }
      .el-scrollbar__bar  {
        &.is-horizontal {
          display: none;
        }
      }
    }
  }
  ::v-deep .el-timeline {
    margin-left: 120px;
    font-size: 12px;
    .el-timeline-item__dot {
      background-color: #fff;
    }
    .el-timeline-item__wrapper {
      left: -110px;
    }
  }
  ::v-deep .time-item {
    .el-timeline-item__wrapper {
      padding-left: 0;
    }
    .dot-wrapper {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      border: 1px solid #cccccc;
      .icon {
        // width: 100%;
        &.last {
          font-size: 14px;
        }
        &.middle {

        }
        &.first-icon {
          border-radius: 50%;
          border: 1px solid #cccccc;
          &::after {
            content: '';
            position: absolute;
            left: 0;
            right: 0;
            top: 0;
            bottom: 0;
            width: 8px;
            height: 8px;
            margin: auto;
            border-radius: 50%;
            background-color: #c3c3c3;
          }
        }
      }
    }
    
    .time-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-right: 70px;
    }
    .status {
    }
    .context {
      flex: 0 0 340px;
    }
  }
  /* 最后一条数据 颜色改变*/
  .lastest {
    color: #ffbd82;
  }
}
</style>