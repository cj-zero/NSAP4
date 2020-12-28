<template>
  <div>
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
    </template>
    <template v-else>
      <el-upload
        ref="file"
        class="upload-demo"
        :class="{ 'hidden-tip': !canShowTip }"
        :action="action"
        name="files"
        :headers="headers"
        :on-success="successBack"
        :on-remove="handleRemove"
        :on-preview="handlePreview"
        :before-upload="beforeFileUpload"
        multiple
        :limit="limit"
        :on-exceed="onExeed"
        :disabled="disabled"
        :file-list="fileList"
        >
        <!-- <el-button size="mini" type="primary">点击上传</el-button>
         -->
         <template v-if="canShowTip">
           <i class="el-icon-upload"></i>
          <span class="upload-text">上传</span>
         </template>
      </el-upload>
    </template>
    <el-image-viewer
      v-if="dialogVisible"
      :url-list="[dialogImageUrl]"
      :on-close="closeViewer"
    >
    </el-image-viewer>
    <!-- pdf展示 -->
    <el-row 
      v-if="pdfVisible"
      class="pdf-outer-wrapper" 
      type="flex" 
      justify="center" 
      align="middle"
    >
      <div class="pdf-mask"></div>
      <i class="el-icon-circle-close close-pdf-btn" @click="closeViewer(true)"></i>
      <div class="pdf-content-wrapper">
        <vue-pdf :pdfURL="pdfURL"></vue-pdf>
      </div>
    </el-row>
  </div>
</template>


<script>
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import { downloadFile } from '@/utils/file'
import VuePdf from '@/components/VuePdf'
export default {
  components: {
    ElImageViewer,
    VuePdf
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
      type: Number
    },
    fit: {
      type: String,
      default: 'fill'
    },
    disabled: {
      type: Boolean,
      default: false
    },
    options: { // 需要向外传递的额外数据
      type: Object,
      default () {
        return {}
      }
    },
    ifShowTip: {
      type: Boolean,
      default: true
    },
    fileList: {
      type: Array,
      default () {
        return []
      }
    },
    maxSize: {
      type: [Number, String]
    },
    onAccept: { // 文件上传之前的校验函数
      type: Function
    },
    isInline: Boolean // 是否在当前页面加载pdf文件，false则直接新建浏览器窗口展示
  },
  updated () {
    console.log('updated')
  },
  data () {
    return {
      dialogImageUrl: "",
      dialogVisible: false,
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`, // 图片上传基地址
      headers:{
        "X-Token":this.$store.state.user.token
      },
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download", // 图片下载基地址
      tokenValue: this.$store.state.user.token,
      pictures:[],
      newPictureList: [],
      isShowTip: true, // 是否展示tip
      pdfURL: '', // pdf路径
      pdfVisible: false // pdf展示
    }
  },
  watch: {
    fileList: {
      immediate: true,
      handler (val) {
        console.log('val', this.fileList)
        if (this.ifShowTip && this.limit) { 
          // 如果是再编辑状态下,需要判断当前的文件数量是否小于等于限制数量，从而控制tip是否展示
          this.isShowTip = val.length < this.limit
        }
      }
    },
    isShowTip (val) {
      console.log(val, 'isShowTip')
    }
  },
  computed: {
    canShowTip () {
      return !this.ifShowTip // 如果是页面处于不可编辑状态ifShowTip: false
      ? this.ifShowTip
      : this.isShowTip
    }
  },
  methods: {
    closeViewer (isPdf) {
      isPdf ? this.pdfVisible = false : this.dialogVisible = false
    },
    handleRemove(file, fileList) {
      if (this.limit && fileList.length < this.limit) {
        this.isShowTip = true
      }
      let { uid, id } = file
      if (this.fileList && this.fileList.length) { // 如果是fileList列表则直接通过id来判断，将id值传出去，用于删除附件
        let findIndex = this.fileList.findIndex(item => item.id === id)
        if (findIndex !== -1) { 
          // 如果找到了，则说明删除的是fileList里面的值，这个时候不可以直接删除fileList中对应的元素，
          // 因为会触发upload组件的fileList，引发图片错乱问题
          // 通过向外传递fileList中删除的图片id，父组件调用接口的时候进行相应的传参
          console.log(id, 'delete id')
          return this.$emit('deleteFileList', file, this.options)
        }
      }
      let findIndex = this.newPictureList.findIndex(item => {
        return item.uid === uid
      })
      this.newPictureList.splice(findIndex, 1)
      this.pictures.splice(findIndex, 1)
      console.log(file, 'deleteFile')
      this.$emit('get-ImgList', this.pictures, {
        ...this.options,
        operation: 'delete' // 删除操作
      })
      
    },
    handlePictureCardPreview(file) {
      this.dialogImageUrl = file.url;
      this.dialogVisible = true;
    },
    handlePreview (file) { // 打开文件
      console.log(file, 'file')
      let { url, fileType } = file
      if (url) {
        if (fileType) { // 文件类型 后台返回的
          if (/^image\/\w+/i.test(fileType)) {
            this.dialogImageUrl = file.url
            this.dialogVisible = true
          } else {
            if (fileType === 'application/pdf' && this.isInline) {
              // 如果返回类型是pdf，并且是在当前页面弹窗展示
              this.pdfVisible = true
              this.pdfURL = url
            } else {
              window.open(file.url)
              console.log(downloadFile)
            }
          }
        }
      }
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
    beforeFileUpload (file) {
      if (this.maxSize) { // 控制文件的大小
        let isLt100M = (file.size / 1024 / 1024) <= this.maxSize
        if (!isLt100M) {
          this.$message.error(`文件超出${this.maxSize}M!`)
          return false
        }
      }
      if (this.onAccept) { // 自定义上传之前的回调函数P
        return this.onAccept(file, { prop: this.options.prop })
      }
      return true
    },
    onExeed () { 
      this.$message.error(`最多上传${this.limit}个文件`)
    },
    successBack(res, file, fileList){
      console.log(fileList, 'success FileList')
      if (this.limit && fileList.length >= this.limit) { // 如果当前的已经上传文件的数量大于等于最大上传数量，隐藏Tip
        this.isShowTip = false
      }
      let _this = this
      this.newPictureList.push({
        pictureId: res.result[0].id,
        uid: file.uid
      })
      file.url = `${this.baseURL}/${res.result[0].id}?X-Token=${this.tokenValue}`
      file.fileType = file.raw.type
      let picConig = {
        pictureId: res.result[0].id
      }
      if (this.uploadType === 'file') { // 这里其实仅针对服务模块的服务单的附件设置的，后续将这块抽出
        picConig.pictureType = 3
      }
      this.pictures.push(picConig) 
      this.$message({
        type:'success',
        message:'上传成功'
      })
      this.$emit('get-ImgList', this.pictures, {
        fileId: res.result[0].id, // 当前上传成功的ID
        fileList,
        uploadVm: _this,
        ...this.options
      })
    },
    clearFiles () {
      this.uploadType === 'image'
        ? this.$refs.img.clearFiles()
        : this.$refs.file.clearFiles()
      this.pictures = []
      this.newPictureList = []
      this.isShowTip = true
    }
  },
  created () {}
};
</script>

<style lang="scss" scoped>
.upload-demo {
  display: block;
  &.hidden-tip {
    ::v-deep .el-upload {
      display: none;
    }
  }
  .upload-text {
    font-size: 12px; 
    margin-left: 5px;
  }
}
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
/* pdf文件 */
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

