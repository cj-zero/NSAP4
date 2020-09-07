<template>
  <div style="width:100%;">
    <template v-if="serviceLogsList.length">
      <el-collapse accordion>
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
      </el-collapse>
    </template>
    <template v-else>
      暂无数据~~
    </template>
  </div>
</template>

<script>
import { getServiceOrderLogs } from '@/api/serve/serviceLog'
export default {
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
  created () {
    getServiceOrderLogs().then(res => {
      console.log(res, 'res')
      this.serviceLogsList = res.data
    }).catch(err => {
      console.log(err)
    })
  },
  mounted() {

  },
  // computed:mapState([
  //  "count"
  // ]),
 
  methods: {
  }
};
</script>

<style lang="scss" scoped>
</style>
