<template>
  <div class="timeline">
    <el-timeline>
      <el-timeline-item
        v-for="(activity, index) in activities"
        :key="index"
        :timestamp="activity.operationCreateTime"
        color="#0bbd87"
        :class="{ highlight: !!activity.inspectId }"
        @click.native="timelineClick(activity.inspectId)"
      >
        <el-tag effect="dark" size="small">{{ activity.operationUser }}</el-tag>
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
  background-color: rgba(#409eff, 0.1);
}
</style>
