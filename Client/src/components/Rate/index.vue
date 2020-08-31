<template>
  <div>
    <el-rate
      v-model="realScore"
      @change="onChange"
      :texts="texts"
      show-text
      :disabled="disabled">
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
    },
    disabled: {
      type: Boolean,
      default: false
    },
    data: {
      type: [Object, Array],
      default () {
        return {}
      }
    }
  },
  watch: {
    score: {
      immediate: true,
      handler (val) {
        console.log(val, 'val')
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
      this.$emit('change', {
        val,
        type: this.type,
        index: this.index
      })
    },
    clearScore () {
      this.realScore = 0
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