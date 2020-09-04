<template>
  <div class="img-wrapper">
    <div class="demo-image__lazy">
      <div class="img-list" v-for="img in imgList" :key="img.id">
        <img
          style="width:60px;height:50px;display:inline-block;"
          :src="`${baseURL}/${img.pictureId ? img.pictureId : img.id}?X-Token=${tokenValue}`"
          lazy
        >
        <div class="operation-wrapper">
          <i
            class="el-icon-zoom-in"
            @click="handlePreviewFile(`${baseURL}/${img.pictureId ? img.pictureId : img.id}?X-Token=${tokenValue}`)"
          ></i>
          <i
            class="el-icon-download"
            @click="downloadFile(`${baseURL}/${img.pictureId ? img.pictureId : img.id}?X-Token=${tokenValue}`)"
          ></i>
        </div>
      </div>
    </div>
    <Model
      :visible="previewVisible"
      @on-close="previewVisible = false"
      ref="formPreview"
      width="600px"
      form
    >
      <img :src="previewUrl" alt style="display: block;width: 80%;margin: 0 auto;" />
      <template slot="action">
        <el-button size="mini" @click="previewVisible = false">关闭</el-button>
      </template>
    </Model>
  </div>
</template>
  
<script>

import Model from "@/components/Formcreated/components/Model";
import { download } from '@/utils/file'
export default {
  components: {
    Model
  },
  props: {
    imgList: {
      type: Array,
      default () {
        return []
      }
    }
  },
  data () {
    return {
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download",
      tokenValue: this.$store.state.user.token,
      previewVisible: false,
      previewUrl: ''
    }
  },
  methods: {
    handlePreviewFile (url) {
      this.previewVisible = true
      this.previewUrl = url
    },
    downloadFile(url) {
      download(url);
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.demo-image__lazy {
  .img-list {
    position: relative;
    display: inline-block;
    width: 60px;
    height: 50px;
    margin: 0 10px;
    .operation-wrapper {
      position: absolute;
      display: flex;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      opacity: 0;
      justify-content: space-around;
      align-items: center;
      transition: opacity 0.5s;
      background-color: rgba(0, 0, 0, 5);
      .el-icon-download,
      .el-icon-zoom-in {
        color: white;
      }
    }
    &:hover {
      .operation-wrapper {
        opacity: 1;
      }
    }
  }
}
</style>