<template>
  <el-timeline>
    <el-timeline-item
      v-for="(activity, index) in activities"
      :key="index"
      :timestamp="activity.timestamp"
      color="#0bbd87"
      >{{ activity.content }}
    </el-timeline-item>
  </el-timeline>
</template>

<script>
export default {
  props: {
    list: {
      type: Array,
      default: () => [],
    },
  },
  computed: {
    activities() {
      return this.list
        .map((i) => {
          return {
            content: i.operationContent,
            timestamp: i.operationCreateTime,
          };
        })
        .reverse();
    },
  },
};
</script>
