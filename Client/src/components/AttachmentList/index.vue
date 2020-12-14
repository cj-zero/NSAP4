<template>
  <div class="attachment-list-wrapper">
    <template v-if="fileList && fileList.length">
      <ul class="attachment-list">
        <template v-for="file in fileList">
          <li  
            :key="file.fileId" 
            class="attachment-item text-overflow" 
            @click="openFile(file)"
          >
            <el-icon class="el-icon-document"></el-icon>
            <span class="file-name">{{ file.fileName }}</span>
          </li>
        </template>
      </ul>
    </template>
    <el-image-viewer
      v-if="dialogVisible"
      :url-list="[dialogImageUrl]"
      :on-close="closeViewer"
    >
    </el-image-viewer>
  </div>
</template>

<script>
import { isImage } from '@/utils/file'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
export default {
  components: {
    ElImageViewer
  },
  props: {
    fileList: {
      type: Array,
      default: () => []
    }
  },
  data () {
    return {
      dialogImageUrl: '',
      dialogVisible: false
    }
  },
  methods: {
    openFile (file) {
      console.log(file, 'file')
      let { url, fileType } = file
      if (url) {
        if (fileType) { // 文件类型 后台返回的
          if (isImage(fileType)) {
            this.dialogImageUrl = file.url
            this.dialogVisible = true
          } else {
            window.open(url)
          }
        }
      }
    },
    closeViewer () {
      this.dialogVisible = false
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.attachment-list-wrapper {
  .attachment-list {
    .attachment-item {
      margin-bottom: 2px;
      cursor: pointer;
      &:nth-last-child(1) {
        margin-bottom: 0;
      }
      .file-name {
        white-space: nowrap;
        overflow: hidden;
      }
    }
  }
}
</style>