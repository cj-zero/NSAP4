<template>
  <el-input
    v-bind="$attrs"
    v-on="$listeners"
    :value="value"
    @keyup.enter.native="onKeyup"
  >
  </el-input>
</template>

<script>
import emitterMixin from '@/utils/emitter'
export default {
  name: 'search-input',
  mixins: [emitterMixin],
  props: {
    value: {
      default: ''
    }
  },
  methods: {
    onInput (val) {
      console.log('123123', val)
      this.$emit('input', val)
    },
    onKeyup () {
      console.log('search')
      this.dispatch('my-search', 'search')
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