<template>
  <div>
    <el-upload
      :action="action"
      multiple
      name="files"
      :file-list="fileList"
      class="img"
      :headers="headers"
      list-type="picture-card"
      accpet="image/png, image/jpeg,image/jpg,image/webp,image/gif"
      :auto-upload="true"
      :width="setImage"
      :on-success="successBack"
    >
      <i slot="default" class="el-icon-plus"></i>
      <div slot="file" slot-scope="{file}">
        <img class="el-upload-list__item-thumbnail" :src="file.url" alt />
        <span class="el-upload-list__item-actions" style="overflow-x:scroll;">
          <span class="el-upload-list__item-preview" @click="handlePictureCardPreview(file)">
            <i class="el-icon-zoom-in"></i>
          </span>
          <span v-if="!disabled" class="el-upload-list__item-delete" @click="handleDownload(file)">
            <i class="el-icon-download"></i>
          </span>
          <span v-if="!disabled" class="el-upload-list__item-delete" @click="handleRemove(file)">
            <i class="el-icon-delete"></i>
          </span>
        </span>
      </div>
    </el-upload>
    <el-dialog :visible.sync="dialogVisible">
      <img width="100%" :src="dialogImageUrl" alt />
    </el-dialog>
  </div>
</template>

<script>
export default {
  props: ["setImage"],
  data() {
    return {
      dialogImageUrl: "",
      dialogVisible: false,
      disabled: false,
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`,
    fileList:[],
    headers:{
      "X-Token":this.$store.state.user.token
    },
    pictures:[],
    }
  },
  watch:{
      fileList:{
          deep:true,
          handler(val){
              console.log(val)
          }
      }
  },
  methods: {
    handleRemove(file,fileList) {
      console.log(file,fileList);
    },
    handlePictureCardPreview(file) {
      this.dialogImageUrl = file.url;
      this.dialogVisible = true;
    },
    handleDownload(file) {
      console.log(file);
    },
    successBack(res){
      this.pictures.push({pictureId:res.result[0].id}) 
      this.$message({
        type:'success',
        message:'上传成功'
      })
      this.$emit('get-ImgList',this.pictures)
    },
 
  }
};
</script>

<style lang="scss" scoped>
.img {
  ::v-deep .el-upload--picture-card {
    width: 50px;
    height: 50px;
    line-height: 50px;
  }
  ::v-deep .el-upload-list__item {
    width: 50px;
    height: 50px;
    line-height: 50px;
    ::-webkit-scrollbar {
      width: 1px !important;
    }
  }
}
</style>

