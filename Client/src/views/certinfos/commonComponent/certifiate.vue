<template>
  <div class="certifiate-wrapper">
    <el-row class="btn-wrapper">
      <el-button type="primary" size="small" class="left-btn" @click="operate(0, leftBtnText)">{{ leftBtnText }}</el-button>
      <el-button type="primary" size="small" @click="operate(1, rightBtnText)" v-if="type !== 'query' && type !== 'submit'">{{ rightBtnText }}</el-button>
    </el-row>
    <el-row v-if="type !== 'submit'">
      <el-input type="textarea" v-model="advice" :placeholder="placeholder"></el-input>
    </el-row>
    <el-row style="margin-top: 10px;">
      <template v-if="certNo">
        <pdf
          class="scroll-wrapper"
          v-loading="loading" 
          ref="pdf"
          :src="pdfURL"
          :page="pageNum"
          :rotate="pageRotate"
          @progress="onProgress"
          @page-loaded="pageLoaded($event)" 
          @num-pages="pageTotalNum = $event" 
          @error="pdfError($event)">
        </pdf>
      </template>
    </el-row>
    <el-row class="tools-wrapper" type="flex" justify="center">
      <el-button type="primary" @click.stop="prePage" class="item" size="mini"> 上一页</el-button>
			<el-button type="primary" @click.stop="nextPage" class="item" size="mini"> 下一页</el-button>
			<div class="page">{{ pageNum }} / {{ pageTotalNum }} </div>
			<el-button type="primary" @click.stop="clock" class="item" size="mini"> 顺时针</el-button>
			<el-button type="primary" @click.stop="counterClock" class="item" size="mini"> 逆时针</el-button>
    </el-row>
  </div>
</template>

<script>
import { downloadFile } from '@/utils/file'
import pdf from 'vue-pdf'
import { certVerMixin } from '../mixin/mixin'
const leftTextMap = {
  submit: '送审',
  review: '通过',
  query: '下载'
}
const rightTextMap = {
  review: '不通过'
}
export default {
  mixins: [certVerMixin],
  components: {
    pdf
  },
  props: {
    type: {
      type: String,
      default: ''
    },
    placeholder: {
      type: String,
      default: ''
    },
    certNo: {
      tyep: String, // 证书编号
      default: ''
    },
    currentData: { // 当前打开项的数据
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {
      advice: '',
      baseURL: `${process.env.VUE_APP_BASE_API}/Cert/DownloadCertPdf`,
      // url: 'http://192.168.1.207:52789/api/Cert/DownloadCertPdf/NWO080091?X-Token=1723b9dc',
      numPages: 1,
      pageNum: 1, // 当前页数
      pageTotalNum: 1, // 总页数
      pageRotate: 0, // 旋转角度
      loadedRatio: 0, // 加载进度
      curPageNum: 0,
      loading: false // 是否出现loading
    }
  },
  computed: {
    leftBtnText () {
      console.log(this.type, leftTextMap[this.type], 'leftBtnText')
      return leftTextMap[this.type]
    },
    rightBtnText () {
      return rightTextMap[this.type]
    },
    pdfURL () {
      return `${this.baseURL}/${this.certNo}?X-Token=${this.$store.state.user.token}`
    }
  },
  methods: {
    operate (type, message) {
      if (message === '下载') {
        this._download()
        return
      }
      console.log(this.currentData, 'currentData')
      this._certVerificate(this.currentData, type, message, this.advice)
    },
    getNumPages (url) {
      console.log(url, 'url')
      let loadingTask = pdf.createLoadingTask(url)
      // console.log(loadingTask, typeof loadingTask, 'loadingTask')
      loadingTask.promise.then(pdf => {
        this.pageTotalNum = pdf.numPages
      }).catch(() => {
        this.$message.error('pdf加载出错')
      })
    },
    // 上一页函数，
    prePage() {
      let page = this.pageNum
      page = page > 1 ? page - 1 : this.pageTotalNum
      this.pageNum = page
    },
    // 下一页函数
    nextPage() {
      let page = this.pageNum
      page = page < this.pageTotalNum ? page + 1 : 1
      this.pageNum = page
    },
    // 页面顺时针翻转90度。
    clock() {
      this.pageRotate += 90
    },
    // 页面逆时针翻转90度。
    counterClock() {
      this.pageRotate -= 90
    },
    // 页面加载回调函数，其中e为当前页数
    pageLoaded(e) {
      this.curPageNum = e
      this.$refs.pdf.$el.scrollTop = 0
    },
    // 其他的一些回调函数。
    pdfError() {
      this.$message.error('页面加载失败')
    },
    onProgress (e) {
      this.loadedRatio = e
      console.log(this.loadedRatio, 'laodingPeo')
      this.loading = this.loadedRatio < 1
      console.log(this.loading, 'loading')
    },
    
    _download () {
      // axios
      //   .get('http://192.168.1.207:52789/api/Cert/DownloadCertPdf/NWO080091?X-Token=1723b9dc')
      //   .then(res => {
      //     console.log(res, 'res')
      //     // let blob = new Blob(res.data)
      //     // this.funDownload(res.data, 'pdf')
      //     // console.log(blob, 'blob')
      //   })
      // download(this.url)
      // downloadPDF(this.url)
      downloadFile(this.pdfURL)
    },
    reset () {
      this.advice = ''
      this.numPages = 1
      this.pageNum = 1
      this.pageTotalNum = 1
      this.pageRotate = 0
      this.loadedRatio = 0
      this.curPageNum = 0
      this.loading = false
      this.$refs.pdf.$el.scrollTop = 0
    }
  },
  watch: {
    certNo (val, oldVal) {
      // 证书编号发生变化就重新加载PDF
      console.log(val, oldVal, 'val')
      this.reset()
      this.getNumPages(this.pdfURL)
    }
  },
  created () {

  },
  mounted () {
    
  },
}
</script>
<style lang='scss' scoped>
.certifiate-wrapper {
  .scroll-wrapper {
    height: 400px;
    overflow-y: scroll;
    &::-webkit-scrollbar {
      width: 7px;
      height: 400px;
    }
    &::-webkit-scrollbar-thumb {
      background-color: #eee;
      border-radius: 10px;
    }
    &::-webkit-scrollbar-track {
      border-radius: 10px;
    }
  }
  .btn-wrapper {
    margin-bottom: 10px;
  }
  .left-btn {
    margin: 0 20px;
  }
  .tools-wrapper {
    align-items: center;
    margin-top: 10px;
    .item, .page {
      margin: 0 5px;
    }
  }
}
</style>