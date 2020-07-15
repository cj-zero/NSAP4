<template>
<div style="height:500px;overflow-y:scroll;text-align:centers;">
  <el-tree :data="dataTree" default-expand-all :props="defaultProps" @node-click="handleNodeClick"></el-tree>

</div>
</template>

<script>

export default {
    props:['dataTree'],
  data() {
    return {
      proplemTree: false,
            defaultProps:{
        label:'name',
        children:'childTypes'
      },//树形控件的显示状态
    };
  },
  mounted() {
    this.filterSerialNumberList = this.SerialNumberList;

  },
  methods: {
    handleNodeClick(value) {
      this.$emit("node-click", value);
    }
  }
};
</script>

<style>
</style>
