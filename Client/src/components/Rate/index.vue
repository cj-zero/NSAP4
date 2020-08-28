<template>
  <div>
    <el-rate
      v-model="realScore"
      @change="onChange"
      :texts="texts"
      show-text>
    </el-rate>
  </div>
</template>

<script>

export default {
  props: {
    score: {
      type: Number,
      default: 0
    },
    type: {
      type: String,
      default: ''
    },
    index: {
      type: Number,
      default: -1
    }
  },
  watch: {
    score: {
      immediate: true,
      handler (val) {
        this.realScore = val
      }
    }
  },
  data () {
    return {
      realScore: 0,
      texts: ['非常差', '差', '一般', '满意', '非常满意']
    }
  },
  methods: {  
    onChange (val) {
      console.log(val, 'limit')
      // this.$emit('change', val)
      this.$emit('change', {
        val,
        type: this.type,
        index: this.index
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