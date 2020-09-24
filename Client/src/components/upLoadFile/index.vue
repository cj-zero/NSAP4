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
    <template v-if="uploadType === 'image'">
      <el-upload 
        ref="img"
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
        :disabled="disabled"
        >
        <i class="el-icon-plus"></i>
        <span class="el-upload-list__item-delete" @click="handleDownload(file)">
          <i class="el-icon-download"></i>
        </span>
      </el-upload>
      <!-- <Model
          :visible="dialogVisible"
          @on-close="dialogVisible = false"
          width="600px"
          :fullscreen="true"
        >
        <img :src="dialogImageUrl" alt style="display: block;width: 80%;margin: 0 auto;" />
        <template slot="action">
          <el-button size="mini" @click="dialogVisible = false">关闭</el-button>
        </template>
      </Model> -->
      <el-image-viewer
        v-if="dialogVisible"
        :url-list="[dialogImageUrl]"
        :on-close="closeViewer"
      >
      </el-image-viewer>
    </template>
    <template v-else>
      <el-upload
        ref="file"
        class="upload-demo"
        :action="action"
        name="files"
        :headers="headers"
        :on-success="successBack"
        :on-remove="handleRemove"
        multiple
        :limit="limit"
        :on-exceed="onExeed"
        :disabled="disabled"
        :file-list="fileList"
        >
        <el-button size="mini" type="primary">点击上传</el-button>
      </el-upload>
    </template>
  </div>
</template>

<script>
// import Model from "@/components/Formcreated/components/Model";
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
export default {
  components: {
    // Model
    ElImageViewer
  },
  props: {
    uploadType: {
      type: String,
      default: 'image'
    },
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
    },
    disabled: {
      type: Boolean,
      default: false
    },
    prop: { // 在组件用于数组遍历的时候
      type: String,
      default: ''
    },
    index: { // 在组件用于数组遍历的时候
      type: Number,
      default: 0
    },
    fileList: {
      type: Array,
      default () {
        return []
      }
    }
  },
  data() {
    return {
      dialogImageUrl: "",
      dialogVisible: false,
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`,
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
    closeViewer () {
      this.dialogVisible = false
    },
    handleRemove(file) {
      let { uid, id } = file
      if (this.fileList && this.fileList.length) { // 如果是fileList列表则直接通过id来判断，将id值传出去，用于删除附件
        let findIndex = this.fileList.findIndex(item => item.id === id)
        if (findIndex !== -1) { 
          // 如果找到了，则说明删除的是fileList里面的值，这个时候不可以直接删除fileList中对应的元素，
          // 因为会触发upload组件的fileList，引发图片错乱问题
          // 通过向外传递fileList中删除的图片id，父组件调用接口的时候进行相应的传参
          console.log(id, 'delete id')
          return this.$emit('deleteFileList', id)
        }
      }
      let findIndex = this.newPictureList.findIndex(item => {
        return item.uid === uid
      })
      this.newPictureList.splice(findIndex, 1)
      this.pictures.splice(findIndex, 1)
      console.log(file, 'deleteFile')
      // this.$emit('get-ImgList', this.pictures, this.prop, this.index)
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
      let testmsg = /^image\/(jpeg|png|jpg)$/.test(file.type)
      if (!testmsg) {
        this.$message.error('上传图片格式不对!')
      }
      // if (this.pictures.length >)
      return testmsg
    },
    onExeed () { 
      this.$message.error(`最多上传${this.limit}个文件`)
    },
    successBack(res, file){
      this.newPictureList.push({
        pictureId:res.result[0].id,
        uid: file.uid
      })
      let picConig = {
        pictureId: res.result[0].id
      }
      if (this.uploadType === 'file') {
        picConig.pictureType = 3
      }
      this.pictures.push(picConig) 
      this.$message({
        type:'success',
        message:'上传成功'
      })
      console.log('beofore', this.index)
      this.$emit('get-ImgList', this.pictures, this.prop, this.index)
    },
    clearFiles () {
      this.uploadType === 'image'
        ? this.$refs.img.clearFiles()
        : this.$refs.file.clearFiles()
      this.pictures = []
      this.newPictureList = []
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

