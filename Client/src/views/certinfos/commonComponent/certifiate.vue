<template>
  <div class="certifiate-wrapper">
    <el-row class="btn-wrapper">
      <el-button 
        v-if="!(currentData.activityName !== '待送审' && type === 'submit')"
        type="primary" 
        size="small" 
        class="left-btn" 
        @click="operate(0, leftBtnText, 'left')" 
        v-loading.fullscreen.lock="isLeftSend"
        :element-loading-text="`正在${leftBtnText}中`"
        element-loading-spinner="el-icon-loading"
        element-loading-background="rgba(0, 0, 0, 0.5)"
      >{{ leftBtnText }}</el-button>
      <el-button 
        type="primary" 
        size="small" 
        @click="operate(1, rightBtnText, 'right')" 
        v-if="type !== 'query' && type !== 'submit'" 
        v-loading.fullscreen.lock="isRightSend"
        :element-loading-text="`正在${rightBtnText}中`"
        element-loading-spinner="el-icon-loading"
        element-loading-background="rgba(0, 0, 0, 0.5)"
      >{{ rightBtnText }}</el-button>
    </el-row>
    <el-row v-if="type == 'review'">
      <el-input type="textarea" v-model="advice" :placeholder="placeholder"></el-input>
    </el-row>
    <el-scrollbar class="scroll-bar" ref="scrollbar" v-loading="loading">
        <div class="pdf-wrapper" ref="pdfWrap">
          <el-row type="flex" justify="center" align="middle" class="loading-mask" v-show="isError">
            <el-icon class="el-icon-refresh-right icon" @click.native="refresh"></el-icon>
          </el-row>
          <!-- <template v-if="certNo && !isError"> -->
            <pdf v-for="i in pageTotalNum" :key="i" :src="url" :page="i" @page-loaded="pageLoaded" @error="pdfError" @progress="onProgress"></pdf>
            <!-- <pdf
              v-if="!isError"
              class="scroll-wrapper"
              ref="pdf"
              :src="url"
              :page="pageNum"
              :rotate="pageRotate"
              @progress="onProgress"
              @page-loaded="pageLoaded($event)" 
              @error="pdfError($event)">
            </pdf> -->
            
          <!-- </template> -->
        </div>
        <!-- <el-row class="tools-wrapper" type="flex" justify="center">
          <el-button type="primary" @click.stop="prePage" class="item" size="mini"> 上一页</el-button>
          <el-button type="primary" @click.stop="nextPage" class="item" size="mini"> 下一页</el-button>
          <el-input type="primary" size="mini" class="item" @keyup.native="jumpToPage" v-model="pageNum"></el-input>
          <div class="page">{{ pageNum }} / {{ pageTotalNum }} </div>
          <el-button type="primary" @click.stop="clock" class="item" size="mini"> 顺时针</el-button>
          <el-button type="primary" @click.stop="counterClock" class="item" size="mini"> 逆时针</el-button>
        </el-row> -->
        
    </el-scrollbar>
    <pagination
      style="text-align: center;"
      v-show="pageTotalNum > 0"
      :total="pageTotalNum"
      :page.sync="pageNum"
      :limit.sync="limit"
      layout="total, prev, pager, next, jumper"
      @pagination="handleCurrentChange"
    />
    <!-- <div style="margin-top: 10px;" class="scroll-wrapper" @scroll="onScroll" ref="container"> -->
  </div>
</template>

<script>
import { downloadFile } from '@/utils/file'
import pdf from 'vue-pdf'
import { certVerMixin } from '../mixin/mixin'
import { getPdfURL } from '@/utils/utils'
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
      url: '',
      baseURL: `${process.env.VUE_APP_BASE_API}/Cert/DownloadCertPdf`,
      // url: 'http://192.168.1.207:52789/api/Cert/DownloadCertPdf/NWO080091?X-Token=1723b9dc',
      numPages: 1,
      pageNum: 1, // 当前页数
      limit: 1, // 一页多少个
      pageTotalNum: 0, // 总页数
      pageRotate: 0, // 旋转角度
      loadedRatio: 0, // 加载进度
      curPageNum: 0,
      loading: false, // 是否出现loading
      realPageNum: 0, // 当前的页数
      isError: false, // 标识错误
      cancelPDF: null, // 用来之前的取消请求
      topList: [0] // 每一页对应的topList
    }
  },
  computed: {
    leftBtnText () {
      return leftTextMap[this.type]
    },
    rightBtnText () {
      return rightTextMap[this.type]
    },
    // pdfURL () {
    //   return `${this.baseURL}/${this.currentData.certNo}?X-Token=${this.$store.state.user.token}`
    // }
  },
  methods: {
    operate (type, message, direction) {
      if (message === '下载') {
        this._download()
        return
      }
      this._certVerificate(this.currentData, type, message, direction, this.advice)
    },
    getNumPages (url) {
      try {
        this.loading = true
        let loadingTask = pdf.createLoadingTask(url)
        loadingTask.uid = this.timestamp // 给loadingTask绑定标识
        loadingTask.promise.then(pdf => {
          // console.log(loadingTask.certNo, timestamp)
          if (loadingTask.uid === this.timestamp) { 
            // 点开哪一个加载哪一个，由于是pdf解析是异步，所以通过闭包的方式来判断这次点击的pdf是否就是当前加载的pdf文档
            this.url = loadingTask
            // this.numPages = pdf.numPages
            this.pageTotalNum = pdf.numPages
            this.loading = false
            this.isLoaded = true
            // this.pageNum = num
          }
        }).catch(() => {
          if (loadingTask.uid === this.timestamp) {
            this.loading = false
            this.isError = true
            this.$message.error('pdf加载出错')
          }
        })
      } catch (err) {
        // TODO
        console.error(err)
      }
    },
    calculateTopList (list) {
      let height = 0
      list.forEach((element, index) => {
        if (index < list.length - 1) {
          let offsetHeight = element.offsetHeight
          height += offsetHeight
          this.topList.push(height)
        }
      })
      console.log(this.topList)
    },
    handleCurrentChange ({ page }) {
      this.scrollWrap.scrollTop = this.topList[page - 1]
      // this.pageNum = page
    },
    refresh () {
      this.getNumPages(this.pdfURL)
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
      // this.curPageNum = e
      this.isError = false
      this.loading = false
      console.log(e, 'pagedLoad')
      this.loadingCount++
      if (this.loadingCount === this.pageTotalNum) {
        // 获取每一页的高度
        if (this.$refs.pdfWrap) {
          this.$nextTick(() => {
            const pageList = Array.from(this.$refs.pdfWrap.childNodes).
              filter(element => {
                let { tagName } = element
                return tagName && tagName.toLowerCase() === 'span'
              }) 
            this.calculateTopList(pageList)
          })
        }
      }
      if (this.scrollWrap) {
        this.scrollWrap.scrollTop = 0
      }
    },
    // 其他的一些回调函数。
    pdfError() {
      this.isError = true
      this.loading = false
      this.$message.error('页面加载失败')
    },
    onProgress (e) {
      this.loadedRatio = e
      this.loading = this.loadedRatio < 1
    },
    onScroll (e) {
      let target = e.target
      let { scrollTop, scrollHeight, clientHeight } = target
      if (scrollTop + clientHeight + 10 >= scrollHeight) {
        this.realPageNum = Math.min(this.numPages, ++this.realPageNum)
      }
    },
    _download () {
      if (!this.pdfURL) {
        return this.$message.error('pdf文件未加载成功')
      }
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
      this.url = ''
      this.loadingCount = 0
      this.isError = false
      this.top = [0]
      if (this.$refs.container) {
       this.$refs.container.scrollTop = 0
      }
    }
  },
  watch: {
    currentData: {
      async handler () {
        // 证书编号发生变化就重新加载PDF
        if (this.currentData.certNo) {
          this.reset()
          this.pdfURL = await getPdfURL('/Cert/DownloadCertPdf', { serialNumber: this.currentData.certNo })
          this.timestamp = +new Date()
          this.getNumPages(this.pdfURL)
        }
      },
      immediate: true
    }
  },
  created () {
    this.pdfURL = ''
  },
  mounted () {
    this.scrollWrap = this.$refs.scrollbar.$el.querySelector('.el-scrollbar__wrap')
    this.loadingCount = 0 // 用来判断pdf是否全部加载完成
    console.log('mounted')
  },
}
</script>
<style lang='scss' scoped>
.certifiate-wrapper {
  .scroll-bar {
    position: relative;
    margin-top: 10px;
    &.el-scrollbar {
       ::v-deep {
        .el-scrollbar__wrap {
          max-height: 500px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
    .pdf-wrapper {
      position: relative;
      min-height: 500px;
      overflow: hidden;
      .loading-mask {
        position: absolute;
        left: 0;
        right: 0;
        top: 0;
        height: 500px;
        z-index: 1;
        .icon {
          color: #2c7be4;
          font-size: 30px;
          cursor: pointer;
        }
      }
    }
  }
  // .pdf-wrapper {
  //     height: 400px;
  //     overflow-y: auto;
  //   }
  //   .refresh-wrapper {
  //     position: absolute;
  //     left: 0;
  //     bottom: 55px;
  //     right: 0;
  //     top: 0;
  //     .icon {
  //       width: 20px;
  //       height: 20px;
  //       cursor: pointer;
  //     }
  //   }
  // .scroll-wrapper {
  //   height: 585.7px;
  //   overflow-y: scroll;
  //   &::-webkit-scrollbar {
  //     width: 7px;
  //     height: 400px;
  //   }
  //   &::-webkit-scrollbar-thumb {
  //     background-color: #eee;
  //     border-radius: 10px;
  //   }
  //   &::-webkit-scrollbar-track {
  //     border-radius: 10px;
  //   }
  //   & > span {
  //     width: 100%;
  //     height: 0 !important;
  //     padding-bottom: 130%;
  //   }
  // }
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