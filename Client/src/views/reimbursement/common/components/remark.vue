<template>
  <div class="remark-wrapper">
    <span class="title">备注：</span>
    <el-input type="textarea" v-model="text" :placeholder="placeholder" @input="onInput"></el-input>
  </div>
</template>

<script>

export default {
  props: {
    maxLength: {
      type: [String, Number],
      default: 0
    },
    placeholder: {
      type: String,
      default: '请输入活动名称'
    }
  },
  data () {
    return {
      text: ''
    }
  },
  methods: {
    onInput (value) {
      console.log(value, 'e')
      this.$emit('input', value)
    },
    reset () {
      this.text = ''
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.remark-wrapper {
  display: flex;
  .title {
    width: 60px;
    font-size: 12px;
  }
  .textarea-wrapper {
    width: 100px;
  }
}
</style>