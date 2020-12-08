<template>
  <div class="timeline">
    <el-timeline>
      <el-timeline-item v-for="(activity, index) in activities" :key="index" :timestamp="activity.operationCreateTime" color="#0bbd87">
        <p effect="dark" size="small">
          <span :class="{ highlight: !!activity.inspectId }">{{ activity.operationUser }}</span>
          <el-button v-if="activity.inspectId" size="mini" round type="success" @click.native="timelineClick(activity.inspectId)">去查看</el-button>
        </p>
        <div class="content" v-html="activity.operationContent"></div>
      </el-timeline-item>
    </el-timeline>
  </div>
</template>

<script>
export default {
  props: {
    list: {
      type: Array,
      default: () => []
    }
  },
  computed: {
    activities() {
      const list = this.list
      return list.reverse()
    }
  },
  methods: {
    timelineClick(inspectId) {
      if (inspectId) {
        this.$emit('timeline-click', inspectId)
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.timeline {
  padding: 10px;
  .content {
    margin-top: 10px;
    font-size: 12px;
  }
}
.highlight {
  color: #f4615c;
  margin-right: 5px;
}
</style>
