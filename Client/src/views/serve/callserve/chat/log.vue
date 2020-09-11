<template>
  <div style="width:100%;max-height:600px;overflow-y:auto;">
    <template v-if="serviceLogsList.length">
      <!-- <el-collapse accordion>
        <div class="item"
          v-for="item in serviceLogsList"
          :title="item.materialType"
          :key="item.id"
          :name="item.id"
        >
          <el-collapse-item>
            <el-timeline>
              <el-timeline-item
              v-for="(activity, index) in item.materialType"
              :key="index"
              :timestamp="activity.timestamp">
              <el-row type="flex" justify="space-between">
                <span class="operator">{{ item.createuserName }}</span>
                <span class="create-time">{{ item.createTime }}</span>
              </el-row>
              <el-row>
                {{ item.action }}
              </el-row>
              </el-timeline-item>
            </el-timeline>
          </el-collapse-item>
          <span class="user">{{ item.createuserName }}</span>
        </div>
      </el-collapse> -->
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
        console.log(res, 'res')
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
</style>
