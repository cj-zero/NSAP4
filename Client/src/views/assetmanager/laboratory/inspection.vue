<template>
  <div>
    送检记录
    <el-table :data="tableData" :cell-style="cellStyle" :header-cell-style="headerCellStyle">
      <el-table-column label="序号">
        <template slot-scope="scope">
          {{ scope.$index }}
        </template>
      </el-table-column>
      <el-table-column label="校准日期" prop="assetJZDate">
      </el-table-column>
      <el-table-column label="失效日期" prop="assetSXDate">
      </el-table-column>
      <el-table-column label="证书" prop="">
        <template slot-scope="scope">
          <span class="item" @click="_view(scope.row.assetJZCertificate)">查看文件</span>
          <span class="item" @click="_download(scope.row.assetJZCertificate)">下载</span>
          <span class="item" @click="_print(scope.row.assetJZCertificate)">打印</span>
        </template>
      </el-table-column>
      <el-table-column label="校准数据">
        <el-table-column label="失效日期" prop="assetDate1">
          <template slot-scope="scope">
            <span class="item" @click="_view(scope.row.assetJZData1)">查看文件</span>
            <span class="item" @click="_download(scope.row.assetJZData1)">下载</span>
            <span class="item" @click="_print(scope.row.assetJZData1)">打印</span>
          </template>
        </el-table-column>
        <el-table-column label="失效日期" prop="assetDate2">
          <template slot-scope="scope">
            <span class="item" @click="_view(scope.row.assetJZData2)">查看文件</span>
            <span class="item" @click="_download(scope.row.assetJZData2)">下载</span>
            <span class="item" @click="_print(scope.row.assetJZData2)">打印</span>
          </template>
        </el-table-column>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
import { download, print } from '@/utils/file'
// const JZDateReg = new RegExp('assetJZData')
export default {
  components: {},
  // props: {
  //   tableData: {
  //     type: Array,
  //     default () {
  //       return []
  //     }
  //   }
  // },
  data () {
    return {
      tableData: [],
      tableOptions:[{
        label: '序号',
        name: 'order'
      }, {
        label: '校准日期',
        name: 'assetJZDate'
      }, {
        label: '失效日期',
        name: 'assetSXDate'
      }, {
        label: '证书',
        name: 'assetJZCertificate'
      }, {
        label: '校准数据',
        name: 'AssetJZData'
      }]
    }
  },
  methods: {
    cellStyle () {
      return {
        'text-align': 'center',
        'vertical-align': 'middle'
      }
    },
    headerCellStyle ({ rowIndex }) {
      let style = {
        'text-align': 'center',
        'vertical-align': 'middle'
      }
      if (rowIndex === 1) {
        style.display = 'none'
      }
      return style
    },
    _view () {
      // this.dialogImage = src
    },
    _download (src) {
      download(src)
    },
    _print (url) {
      print(url)
    }
  },
  created () {
    for (let i = 0; i < 4; i++) {
      this.tableData.push({
        assetJZDate: '2020-20-20',
        assetSXDate: '2020-20-20',
        assetJZCertificate: 'blob:http://localhost:1803/a117d03a-5475-449c-b10e-7f80fdd101ac',
        assetJZData1: 'blob:http://localhost:1803/a117d03a-5475-449c-b10e-7f80fdd101ac',
        assetJZData2: 'blob:http://localhost:1803/a117d03a-5475-449c-b10e-7f80fdd101ac',
      })
    }
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.item {
  color: rgba(102, 177, 255, 1);
  margin: 0 5px;
  text-decoration: underline;
}
</style>