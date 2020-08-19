<template>
<div class="pdf">
  <div class="pdf-tab">
    <div 
      :class="['btn-def',{'btn-active':activeIndex==index}]"
      v-for="(item,index) in pdfList"
      :key="item"
      @click.stop="pdfClick(item.pdfUrl,index)">{{item.title}}</div>
  </div>
  <pdf 
    v-for="i in numPages"
    :key="i"
    :src="pdfUrl"
    :page="i">
  </pdf>
</div>
</template>
<script>
import pdf from 'vue-pdf'
export default {
  name: 'Pdf',
  components:{
      pdf
  },
  data(){
      return {
      pdfList:[
        {
          pdfUrl:"https://dakaname.oss-cn-hangzhou.aliyuncs.com/file/2018-12-29/1546049718768.pdf",
          title:"你好，2019年"
        },
        {
          pdfUrl:"http://file.gp58.com/file/2018-11-14/111405.pdf",
          title:"中信证券观点"
        },
        {
          pdfUrl:"https://dakaname.oss-cn-hangzhou.aliyuncs.com/file/2018-12-28/1546003237411.pdf",
          title:"12月投资月刊"
        },
        {
          pdfUrl:"https://dakaname.oss-cn-hangzhou.aliyuncs.com/file/2018-12-28/1546003282521.pdf",
          title:"丰岭资本观点"
        },
      ],
      pdfUrl: '',
      numPages:1,
      activeIndex:0,
      }
  },
  mounted: function() {
    this.pdfTask(this.pdfList[0].pdfUrl)
  },
  methods: {
      pdfTask(pdfUrl){
        var self = this
        var loadingTask = pdf.createLoadingTask(pdfUrl)  
        loadingTask.then(pdf => {
          self.pdfUrl = loadingTask
          self.numPages = pdf.numPages
        }).catch((err) => {
          console.error('pdf加载失败', err)
      })
      },
    pdfClick(pdfUrl,index){
      if(index == this.activeIndex)return
      this.activeIndex = index
      this.pdfUrl = null
      this.pdfTask(pdfUrl)
    },
  }
}
</script>