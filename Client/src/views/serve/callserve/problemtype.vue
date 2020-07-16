<template>
<div style="overflow-y:scroll;text-align:centers;height:500px;">
  <el-tree class="treeCenter" :data="dataTree" default-expand-all :props="defaultProps" @node-click="handleNodeClick"></el-tree>

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

<style lang="scss" scoped>
 .treeCenter {
   ::v-deep .el-tree-node__content{
    display: flex;
    align-items: center;
    // justify-content: center;
    height: 26px;
    cursor: pointer;
}
 }
</style>>


