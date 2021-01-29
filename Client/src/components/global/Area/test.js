<template>
  <div>
    <el-cascader
    v-model="value"
    :options="options"
    :props="props"
    @change="handleChange"></el-cascader>
    <div class="area-selector-wrapper">
      
    </div>
  </div>
</template>

<script>
import areaData from './area.json'
import ClickOutSide from 'element-ui/'
export default {
  name: 'CommonArea',
  components: {},
  data () {
    return {
      value: '北京市市辖区朝阳区',
      options: areaData,
      props: {
        value: 'name',
        label: 'name'
      }
    }
  },
  watch: {
    value (newVal) {
      if (newVal) {
        console.log(newVal, 'changed')
      }
    }
  },
  methods: {
    handleChange (val) {
      console.log(val, 'val')
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