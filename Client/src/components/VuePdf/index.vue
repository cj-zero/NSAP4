<template>
  <div class="pdf-wrapper" v-loading="loading">
    <span class="num-pages">当前{{ realPageNum }}页 / 共{{ numPages }}页</span>
    <div class="scroll-wrapper" @scroll="onScroll" ref="container">
      <template v-if="pdfURL">
        <pdf v-for="i in realPageNum" :key="i" :src="url" :page="i"></pdf>
      </template>
        <!-- <pdf
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
        </pdf> -->
    </div>
    <!-- <el-row class="tools-wrapper" type="flex" justify="center">
      <el-button type="primary" @click.stop="prePage" class="item" size="mini"> 上一页</el-button>
			<el-button type="primary" @click.stop="nextPage" class="item" size="mini"> 下一页</el-button>
			<div class="page">{{ pageNum }} / {{ pageTotalNum }} </div>
			<el-button type="primary" @click.stop="clock" class="item" size="mini"> 顺时针</el-button>
			<el-button type="primary" @click.stop="counterClock" class="item" size="mini"> 逆时针</el-button>
    </el-row> -->
  </div>
</template>

<script>
import pdf from 'vue-pdf'

export default {
  components: {
    pdf
  },
  props: {
    pdfURL: {
      type: String,
      required: true
    }
  },
  watch: {
    pdfURL: {
      immediate: true,
      handler (newVal) {
        if (newVal) {
          console.log('pdfURL', newVal)
          this.reset()
          this.getNumPages(newVal)
        }
      }
    }
  },
  data () {
    return {
      url: '',
      numPages: 1,
      pageNum: 1, // 当前页数
      pageTotalNum: 1, // 总页数
      pageRotate: 0, // 旋转角度
      loadedRatio: 0, // 加载进度
      curPageNum: 0,
      loading: false, // 是否出现loading
      realPageNum: 0 // 当前的页数
    }
  },
  methods: {
    getNumPages (url) {
      try {
        console.log(url)
        let loadingTask = pdf.createLoadingTask(url)
        this.loading = true
        loadingTask.promise.then(pdf => {
          this.url = loadingTask
          this.numPages = pdf.numPages
          if (this.numPages) {
            this.realPageNum = 1
          }
          this.loading = false
        }).catch(() => {
          this.loading = false
          this.$message.error('pdf加载出错')
        })
      } catch (err) {
        // TODO
      }
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
      this.loading = this.loadedRatio < 1
    },
    onScroll (e) {
      let target = e.target
      let { scrollTop, scrollHeight, clientHeight } = target
      if (scrollTop + clientHeight + 100 >= scrollHeight) {
        this.realPageNum = Math.min(this.numPages, ++this.realPageNum)
      }
    },
    reset () {
      this.numPages = 1
      this.pageNum = 1
      this.pageTotalNum = 1
      this.pageRotate = 0
      this.loadedRatio = 0
      this.curPageNum = 0
      this.loading = false
      this.url = ''
      if (this.$refs.container) {
       this.$refs.container.scrollTop = 0
      }
    }
  },
  created () {

  },
  mounted () {
    
  },
}
</script>
<style lang='scss' scoped>
.pdf-wrapper {
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: center;
  width: 100%;
  height: 100%;
  padding: 0 10px;
  background-color: #fff;
  .num-pages  {
    position: absolute;
    left: 50%;
    top: -10px;
    color: #fff;
    transform: translate3d(-50%, -100%, 0);
  }
  .scroll-wrapper {
    height: 100%;
    overflow-y: scroll;
    // ::v-deep .el-scrollbar__wrap {
    //   overflow-x: hidden; // 隐藏横向滚动栏
    //   margin-bottom: 0 !important;
    // }
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
    & > span {
      width: 100%;
      height: 0 !important;
      padding-bottom: 130%;
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