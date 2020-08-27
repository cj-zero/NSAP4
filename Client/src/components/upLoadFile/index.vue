<template>
  <div>
    <!-- <el-upload
      :action="action"
      multiple
      name="files"
      :file-list="fileList"
      class="img"
      :before-upload="beforeUpload"
      :headers="headers"
      list-type="picture-card"
      accpet="image/png, image/jpeg,image/jpg,image/webp,image/gif"
      :auto-upload="true"
      :width="setImage"
      :on-success="successBack"
      :on-remove="handleRemove"
    >
      <i slot="default" class="el-icon-plus"></i>
      <div slot="file" slot-scope="{file}">
        <img class="el-upload-list__item-thumbnail" :src="file.url" alt>
        <span class="el-upload-list__item-actions" style="overflow-x:scroll;">
          <span class="el-upload-list__item-preview" @click="handlePictureCardPreview(file)">
            <i class="el-icon-zoom-in"></i>
          </span>
           <span v-if="!disabled" class="el-upload-list__item-delete" @click="handleDownload(file)">
            <i class="el-icon-download"></i>
          </span>
          <span v-if="!disabled" class="el-upload-list__item-delete" @click="handleRemove(file)">
            <i class="el-icon-delete"></i>
          </span> -->
        <!-- </span>
      </div>
    </el-upload> -->
    <el-upload 
      :action="action"
      list-type="picture-card"
      multiple
      name="files"
      :before-upload="beforeUpload"
      :width="setImage"
      :on-success="successBack"
      :on-preview="handlePictureCardPreview"
      :on-remove="handleRemove"
      :headers="headers"
      class="img"
      :limit="limit"
      :auto-upload="true"
      :on-exceed="onExeed"
      >
      <i class="el-icon-plus"></i>
      <span class="el-upload-list__item-delete" @click="handleDownload(file)">
        <i class="el-icon-download"></i>
      </span>
    </el-upload>
    <el-dialog :visible.sync="dialogVisible">
      <!-- <el-row style="margin-bottom: 20px">
        <el-button type="primary" @click="handleDownload" style="margin-left: 20px" size="mini">下载</el-button>
      </el-row> -->
      <el-image
        :src="dialogImageUrl"
        :fit="fit"></el-image>
        <!-- <img width="100%" :src="dialogImageUrl" alt /> -->
    </el-dialog>
  </div>
</template>

<script>
export default {
  props: {
    setImage: {
      type: [String, Object],
      default: ''
    },
    limit: {
      type: Number,
      default: 0
    },
    fit: {
      type: String,
      default: 'fill'
    }
  },
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
      newPictureList: []
      }
  },
  watch:{
      fileList:{
          deep:true,
          handler(val){
              console.log(val, 'fileList')
          }
      }
  },
  methods: {
    handleRemove(file) {
      let { uid } = file
      let findIndex = this.newPictureList.findIndex(item => {
        return item.uid === uid
      })
      this.pictures.splice(findIndex)
    },
    handlePictureCardPreview(file) {
      this.dialogImageUrl = file.url;
      this.dialogVisible = true;
    },
    handleDownload() {
      let a = document.createElement('a')
      a.download = this.dialogImageUrl
      a.href = this.dialogImageUrl
      a.target = 'self'
      a.click()
    },
    beforeUpload (file) {
      console.log(file, 'file')
      let testmsg = /^image\/(jpeg|png|jpg)$/.test(file.type)
      if (!testmsg) {
        this.$message.error('上传图片格式不对!')
      }
      // if (this.pictures.length >)
      return testmsg
    },
    onExeed () {
      console.log('onecdsad')
      this.$message.error(`最多上传${this.limit}个文件`)
    },
    successBack(res, file, fileList){
      this.newPictureList.push({
        pictureId:res.result[0].id,
        uid: file.uid
      })
      console.log(res, res.result[0].id, 'id', file, fileList)
      this.pictures.push({
        pictureId:res.result[0].id
      }) 
      this.$message({
        type:'success',
        message:'上传成功'
      })
      this.$emit('get-ImgList',this.pictures)
    }
  }
};
</script>

<style lang="scss" scoped>
.img{
  ::v-deep .el-upload--picture-card {
    width: 70px !important;
    height: 70px !important;
    line-height: 70px !important;
  }
  ::v-deep .el-upload-list__item {
    width: 70px;
    height: 70px;
    line-height: 70px;
    // ::-webkit-scrollbar {
    //   width: 1px !important;
    // }
  }
  ::v-deep .el-icon-check {
    position: absolute;
    right: 14px;
  }
}


</style>

