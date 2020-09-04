<template>
  <el-tabs v-model="activeName" type="card" @tab-click="handleClick">
    <el-tab-pane 
      v-for="item in texts" 
      :key="item.name" 
      :label="item.label" 
      :name="item.name"></el-tab-pane>
  </el-tabs>
</template>

<script>

export default {
  props: {
    texts: {
      type: Array,
      default () {
        return []
      }
    },
    initialName: {
      type: String,
      default: ''
    }
  },
  watch: {
    texts (val) {
      console.log('texts', val)
    },
    initialName: {
      immediate: true,
      handler (val) {
        this.activeName = val
        console.log(this.activeName, 'activeName')
      }
    }  
  },
  data () {
    return {
      activeName: 'message'
    }
  },
  methods: {
    handleClick (val) {
      this.$emit('tabChange', val.name)
    },
    clear () {
      this.texts = []
      this.activeName = this.initialName
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
</style>