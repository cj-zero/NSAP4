<template>
  <div style="width:100%">
    <el-scrollbar class="scroll-bar">
      <template v-if="serviceLogsList.length">
        <el-timeline>
          <el-timeline-item
          v-for="(item, index) in serviceLogsList"
          :key="index"
          :timestamp="item.createTime">
          <el-row type="flex" justify="space-between">
            <span class="operator">{{ item.createuserName }}</span>
          </el-row>
          <el-row>
            {{ item.action }}
          </el-row>
          </el-timeline-item>
        </el-timeline>
      </template>
      <template v-else>
        暂无数据~~
      </template>
    </el-scrollbar>
  </div>
</template>

<script>
import { getServiceOrderLogs } from '@/api/serve/serviceLog'
export default {
  props: {
    serveId: {
      type: [Number, String],
      default: ''
    }
  },
  data() {
    return {
      wrapClass: 'wrapClass',
      serviceLogsList: [],
      activities: [{
          content: '活动按期开始',
          timestamp: '2018-04-15'
        }, {
          content: '通过审核',
          timestamp: '2018-04-13'
        }, {
          content: '创建成功',
          timestamp: '2018-04-11'
        }]
    }
  },
  watch: {
    serveId () {
      this._getServiceOrderLogs()
    }
  },
  methods: {
    _getServiceOrderLogs () {
      getServiceOrderLogs({
        serviceOrderId: this.serveId
      }).then(res => {
        this.serviceLogsList = res.data
      }).catch(() => {
        this.serviceLogsList = []
        this.$message.error('加载日志失败')
      })
    }
  },
  created () {
    this._getServiceOrderLogs()
  }
};
</script>

<style lang="scss" scoped>
.scroll-bar {
  &.el-scrollbar {
    ::v-deep .el-scrollbar__wrap {
      max-height: 600px; // 最大高度
      overflow-x: hidden; // 隐藏横向滚动栏
      margin-bottom: 0 !important;
    }
  }
}
</style>
