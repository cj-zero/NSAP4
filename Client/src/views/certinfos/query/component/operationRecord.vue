<template>
  <div class="operation-wrapper">
    <el-timeline v-if="resultList.length">
      <el-timeline-item
        v-for="(activity, index) in resultList"
        :key="index"
        :timestamp="activity.createTime">
        {{ activity.action }}
      </el-timeline-item>
    </el-timeline>
    <div v-else class="empty-text">暂无操作记录</div>
  </div>
</template>

<script>
import { getCertOperationHistory } from '@/api/cerfiticate'
export default {
  props: {
    id: {
      type: String,
      default: ''
    }
  },
  data () {
    return {
      resultList: []
    }
  },
  methods: {
    _getCertOperationHistory () {
      getCertOperationHistory({
        id: this.id
      }).then(res => {
        console.log(res, 'res record')
        this.resultList = res.result
      })
    }
  },
  watch: {
    id (val) {
      console.log(val, 'id')
      this._getCertOperationHistory()
    }
  },
  mounted () {
    this._getCertOperationHistory()
  }
}
</script>
<style lang='scss' scoped>
.operation-wrapper {
  position: relative;
  width: 100%;
  height: 100%;
  .empty-text {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    margin: auto;
  }
}
</style>