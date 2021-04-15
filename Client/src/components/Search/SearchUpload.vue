<template>
  <el-upload
    v-bind="$attrs"
    v-on="$listeners"
  >
    <el-button size="small" type="primary">{{ uploadText }}</el-button>
  </el-upload>
</template>

<script>
export default {
  name: 'search-upload',
  props: {
    uploadText: {
      type: String,
      default: ''
    }
  }
}
</script>

<style>

</style>