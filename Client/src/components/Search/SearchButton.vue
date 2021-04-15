<template>
  <el-button v-bind="$attrs" v-on="$listeners">{{ btnText }}</el-button>
</template>

<script>
export default {
  name: 'search-button',
  props: {
    btnText: {
      type: String,
      default: '查询'
    }
  }
}
</script>

<style>

</style>