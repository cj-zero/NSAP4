<template>
  <div>
    <el-select :placeholder="placeholder" v-model="value" @change='onChange'>
      <el-option v-for="item in options" :label="item" :value="item" :key="item"></el-option>
    </el-select>
  </div>
</template>

<script>

export default {
  props: {
    index: {
      type: Number,
      defualt: -1
    },
    options: {
      type: Array,
      default () {
        return []
      }
    },
    placeholder: {
      type: String,
      default: '请选择'
    }
  },
  data () {
    return {
      value: ''
    }
  },
  methods: {
    onChange (val) {
      this.$emit('change', {
        val,
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