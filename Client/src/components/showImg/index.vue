<template>
  <div>
    <div class="demo-image__lazy">
      <el-image
        style="width:60px;height:50px;display:inline-block;margin:0 10px;"
        @click="handlePreviewFile(`${baseURL}/files/Download/${url.pictureId}?X-Token=${tokenValue}`)"
        v-for="url in PicturesList"
        :key="url.pictureId"
        :src="`${baseURL}/files/Download/${url.pictureId}?X-Token=${tokenValue}`"
        lazy
      ></el-image>
    </div>
               <el-dialog
        :visible.sync="previewVisible"
        width="600px"
      >
        <img :src="previewUrl" alt style="display: block;width: 80%;margin: 0 auto;" />
         <span slot="footer" class="dialog-footer">
          <el-button size="mini" @click="previewVisible = false">关闭</el-button>
      </span>
      </el-dialog>
  </div>
</template>

<script>
export default {
  props: ["PicturesList"],
  //一个用来显示图片的组件
  // ##PicturesList即为传入图片的列表
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
                  previewUrl: "", //预览图片的定义
            previewVisible:false,
    };
  },

  methods: {
    // handlePreviewFile(item) {
    //   //预览图片
    //   this.$emit("handle-PreviewFile", item);
    // }
            handlePreviewFile(item) {
      //预览图片
      this.previewVisible = true;
      this.previewUrl = item;
    },
  }
};
</script>

<style>
</style>