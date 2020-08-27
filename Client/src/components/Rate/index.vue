<template>
  <el-rate
    v-model="value"
    show-text
    @change="onChange"
    :disabled="readOnly">
  </el-rate>
</template>

<script>

export default {
  props: {
    max: {
      type: Number,
      defautl: 5
    },
    readOnly: {
      type: Boolean,
      default: false
    },
    type: {
      type: String,
      default: ''
    }
  },

  data () {
    return {
      value: 0
    }
  },
  methods: {
    onChange (val, type) {
      this.$emit('change', {
        val,
        type
      })
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