<template>
  <el-table :data="tableData" border class="origin-wrapper">
    <el-table-column label="序号"> 
      <template slot-scope="scope">
        {{ scope.$index + 1 }}
      </template>
    </el-table-column>
    <el-table-column label="校准证书"> 
      <template slot-scope="scope">
        <span class="certNo" @click="_downloadFile('word')">{{ scope.row.certNo }}</span>
      </template>
    </el-table-column>
    <el-table-column label="原始数据"> 
      <template slot-scope="scope">
        <span class="certNo" @click="_downloadFile('execl')">{{ scope.row.certNo }}</span>
      </template>
    </el-table-column>
    <el-table-column label="原始数据上传时间"> 
      <template slot-scope="scope">
        {{ scope.row.createTime }}
      </template>
    </el-table-column>
  </el-table>
</template>

<script>
import { downloadFile } from '@/utils/file'
import { getPdfURL } from '@/utils/utils'
export default {
  props: {
    currentData: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  computed: {
    tableData () {
      if (Object.keys(this.currentData).length) {
        let { createTime, certNo } = this.currentData
        return [{
          createTime,
          certNo
        }]
      } else {
        return []
      }
    }
  },
  components: {},
  data () {
    return {
      // headOptoins
      baseURL: `${process.env.VUE_APP_BASE_API}/Cert`
    }
  },
  methods: {
    async _downloadFile (type) {
      let url = type === 'word'
        ? `/Cert/DownloadCert`
        : `/Cert/DownloadBaseInfo`
      console.log(url, url + this.currentData.certNo, 'url')
      const pdfURL = await getPdfURL(url, { serialNumber: this.currentData.certNo })
      // downloadFile(url + this.currentData.certNo + `?X-Token=${this.$store.state.user.token}`)
      console.log(pdfURL, 'pdfURL')
      downloadFile(pdfURL)
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.origin-wrapper {
  .certNo {
    cursor: pointer;
    color: rgba(255, 0, 0, 1);
  }
}
</style>