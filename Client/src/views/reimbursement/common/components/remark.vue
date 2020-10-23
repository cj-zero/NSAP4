<template>
  <div class="remark-wrapper">
    <span class="title">驳回原因：</span>
    <div class="right-wrapper">
      <el-input type="textarea" v-model="text" :placeholder="placeholder" @input="onInput"></el-input>
      <el-tag
        class="tag-item"
        size="mini"
        v-for="item in tagList" 
        :key="item.value"
        :type="item.value"
        @click="selectTag(item)">
        {{ item.label }}
      </el-tag>
    </div>
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
    },
    tagList: { // 标签按钮
      type: Array,
      default () {
        return []
      }
    }
  },
  data () {
    return {
      text: ''
    }
  },
  methods: {
    onInput (value) {
      this.$emit('input', value)
    },
    reset () {
      this.text = ''
    },
    selectTag (item) {
      this.text = item.label
      this.$emit('input', this.text)
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
    width: 80px;
    font-size: 12px;
  }
  .right-wrapper {
    .tag-item {
      margin: 5px 5px 0 0;
      cursor: pointer;
    }
  }
}
</style>