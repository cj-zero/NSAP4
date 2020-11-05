<template>
  <el-table ref="table" :data="tableData" border :cell-style="cellStyle" highlight-current-row :header-cell-style="headerCellStyle">
    <el-table-column label="校准日期" prop="inspectStartDate" :formatter="formatter"> </el-table-column>
    <el-table-column label="失效日期" prop="inspectEndDate" :formatter="formatter"> </el-table-column>
    <el-table-column label="校准证书">
      <template v-if="scope.row.inspectCertificate" slot-scope="scope">
        <a :href="getFileUrl(scope.row.inspectCertificate)" class="item" target="_blank">查看文件</a>
        <span class="item" @click="_download(scope.row.inspectCertificate)">下载</span>
      </template>
    </el-table-column>
    <el-table-column label="校准数据">
      <template v-if="scope.row.inspectDataTwo" slot-scope="scope">
        <a v-if="scope.row.inspectDataTwo" :href="getFileUrl(scope.row.inspectDataTwo)" class="item" target="_blank">查看文件</a>
        <span class="item" @click="_download(scope.row.inspectDataTwo)">下载</span>
      </template>
    </el-table-column>
  </el-table>
</template>

<script>
import { downloadFile } from '@/utils/file'
export default {
  props: {
    list: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token
    }
  },
  computed: {
    tableData() {
      return this.list
    }
  },
  methods: {
    formatter(_, __, time) {
      return time.split(' ')[0]
    },
    cellStyle() {
      return {
        'text-align': 'center',
        'vertical-align': 'middle'
      }
    },
    highlightCell(inspectId) {
      const row = this.tableData.find(item => item.id === inspectId)
      this.$refs.table.setCurrentRow(row)
    },
    headerCellStyle({ rowIndex }) {
      let style = {
        'text-align': 'center',
        'vertical-align': 'middle'
      }
      if (rowIndex === 1) {
        style.display = 'none'
      }
      return style
    },
    _download(id) {
      const src = this.getFileUrl(id)
      if (src) {
        downloadFile(src)
      }
    },
    getFileUrl(id) {
      if (!id) return ''
      return `${this.baseURL}/files/Download/${id}?X-Token=${this.tokenValue}`
    }
  }
}
</script>
<style lang="scss" scoped>
.item {
  color: rgba(102, 177, 255, 1);
  margin: 0 5px;
  border-bottom: 1px solid currentColor;
  cursor: pointer;
}
</style>
