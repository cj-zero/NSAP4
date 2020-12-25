<template>
  <!-- pdf展示 -->
  <el-row 
    class="pdf-outer-wrapper" 
    type="flex" 
    justify="center" 
    align="middle"
  >
    <div class="pdf-mask"></div>
    <i class="el-icon-circle-close close-pdf-btn" @click="onClose"></i>
    <div class="pdf-content-wrapper">
      <vue-pdf :pdfURL="pdfURL"></vue-pdf>
    </div>
  </el-row>
</template>

<script>
import VuePdf from '@/components/VuePdf'
export default {
  components: {
    VuePdf
  },
  props: {
    pdfURL: {
      type: String,
      required: true
    },
    onClose: {
      type: Function,
      defualt: () => {}
    }
  },
  data () {
    return {

    }
  },
  methods: {

  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.pdf-outer-wrapper {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 20001;
  .pdf-mask {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, .5);
  }
  .close-pdf-btn {
    position: absolute;
    top: 40px;
    right: 40px;
    width: 40px;
    height: 40px;
    color: #fff;
    font-size: 40px;
    cursor: pointer;
  }
  .pdf-content-wrapper {
    width: 520px;
    height: 600px;
  }
}
</style>