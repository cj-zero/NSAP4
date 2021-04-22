<template>
  <div class="equipment-wrapper">
    <div class="search-wrapper">
      <Search 
        :listQuery="equipmentListQuery"
        @search="onSearch('eq')"
        :config="equipmentSearchConfig"
      ></Search>
    </div>
    <div class="table-wrapper">
      <common-table 
        ref="eqTable"
        v-loading="eqLoading"
        :data="equipmentList" 
        :columns="equipmentColumns" 
        height="400px" 
        row-key="certificateNumber">
        <template v-slot:serialNumber="{ row }">
          <el-link type="primary" @click="showCertNoList(row)">{{ row.testerSn }}</el-link>
        </template>
        <template v-slot:operation="{ row }">
          <el-button v-if="row.certificateNumber" type="primary" size="mini" @click.stop="download(row)">下载</el-button>
        </template>
      </common-table>
      <pagination
        v-show="eqTotal>0"
        :total="eqTotal"
        :page.sync="equipmentListQuery.page"
        :limit.sync="equipmentListQuery.limit"
        @pagination="handleEqChange"
      />
    </div>
    <my-dialog 
      ref="certNoDialog" 
      width="900px" 
      :append-to-body="true"
      @opened="onOpened" 
      @closed="onClosed"
      :destroy-on-close="true"
    >
      <template v-slot:title>
        <el-row class="title" type="flex" align="middle">
          <div>序列号<span>{{ currentRow.testerSn }}</span></div>
          <div>型号规格<span>{{ currentRow.testerModel }}</span></div>
          <div>客户代码<span>{{ currentInfo.cardCode }}</span></div>
          <div>客户名称<span>{{ currentInfo.cardName }}</span></div>
          <div>创建时间<span>{{ currentInfo.createDate }}</span></div>
        </el-row>
      </template>
      <div class="certinfo-wrapper">
        <div style="line-height: 33px;"></div>
        <Search 
        :listQuery="certListQuery"
        @search="onSearch('cert')"
        :config="certSearchConfig"
      ></Search>
      </div>
      <div style="margin-top: 10px;">
        <common-table 
          ref="certTable" 
          :data="certNoList" 
          :columns="certColumns" 
          height="500px" 
          v-loading="certLoading" 
          row-key="certNo">
          <template v-slot:certNo="{ row }">
            <el-link @click="showCertInfo(row)">{{ row.certNo }}</el-link>
          </template>
          <template v-slot:operation="{ row }">
            <el-button type="primary" size="mini" @click.stop="download(row, 'cert')">下载</el-button>
          </template>
        </common-table>
        <pagination
          v-show="certTotal>0"
          :total="certTotal"
          :page.sync="certListQuery.page"
          :limit.sync="certListQuery.limit"
          @pagination="handleCertChange"
        />
      </div>
    </my-dialog>
    <!-- 校准证书pdf弹窗 -->
    <my-dialog ref="certInfoDialog" title="校准证书详情" width="800px" :append-to-body="true">
      <Certifiate :currentData="currentData" :certNo="currentData.certNo" :ifShowBtn="false"></Certifiate>
    </my-dialog>
  </div>
</template>

<script>
import Search from '@/components/Search'
import Certifiate from '../../commonComponent/certifiate'
import { querySalesLoad, downloadTest } from '@/api/cerfiticate'
import { print, getPdfURL } from '@/utils/utils'
import { getFile } from '@/utils/file'
import { formatDate } from '@/utils/date'
import { mapMutations } from 'vuex'
import JSZip from 'jszip'
import FileSaver from 'file-saver'
// import { serializeParams } from '@/utils/process'
const W_370 = { width: '370px', display: 'inline-flex' }
const W_150 = { width: '150px' }
export default {
  components: {
    Search,
    Certifiate
  },
  props: {
    salesOrderId: {
      type: [Number, String],
      default: ''
    },
    currentInfo: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {
      eqLoading: false,
      certLoading: false,
      currentRow: {},
      currentData: {},
      equipmentListQuery: {
        page: 1,
        limit: 30,
        pageStatus: 2
      },
      equipmentList: [],
      eqTotal: 0,
      equipmentColumns: [
        { type: 'selection' },
        { type: 'index', label: '序号' },
        { label: '序列号', slotName: 'serialNumber' },
        { label: '型号规格', prop: 'testerModel' },
        { label: '资产编号', prop: 'assetNo' },
        { label: '证书编号', prop: 'certificateNumber' },
        { label: '校准日期', prop: 'time', width: 100 },
        { label: '失效日期', prop: 'expirationDate', width: 100 },
        { label: '校准人', prop: 'operator' },
        { label: '操作', slotName: 'operation' },
      ],
      equipmentSearchConfig: [
        { prop: 'manufacturerSerialNumbers', component: { attrs: { placeholder: '序列号', style: W_150 } } },
        { prop: 'testerModel', component: { attrs: { placeholder: '型号规格', style: W_150 } }  },
        { prop: 'certNo', component: { attrs: { placeholder: '证书编号', style: W_150 } } },
        { prop: 'date', 
          component: { 
            tag: 'date', on: { change: this.onDateChange.bind(this, 'eq') },
            attrs: { type: 'daterange', 'range-separator': '至', 'start-placeholder': '校准开始日期', 'end-placeholder': '校准结束日期', style: W_370, clearable: true }, 
          } 
        },
        { component: { tag: 's-button', attrs: { btnText: '查询', type: 'primary' }, on: { click: this.onSearch.bind(this, 'eq') } } },
        { component: { tag: 's-button', attrs: { btnText: '一键下载', type: 'primary' }, on: { click: this.downloadAll.bind(this, 'eq') }  } }
      ],
      certListQuery: {
        page: 1,
        limit: 30,
        pageStatus: 3
      },
      certTotal: 0,
      certNoList: [],
      certColumns: [
        { type: 'selection' },
        { label: '序号', type: 'index' },
        { label: '证书编号', slotName: 'certNo' },
        { label: '校准日期', prop: 'calibrationDate', width: 100 },
        { label: '失效日期', prop: 'expirationDate', width: 100 },
        { label: '校准人', prop: 'operator' },
        { label: '操作', slotName: 'operation' }
      ],
      certSearchConfig: [
        { prop: 'certNo', component: { attrs: { placeholder: '证书编号', style: W_150 } } },
        { prop: 'operator', component: { attrs: { placeholder: '校准人', style: W_150 } } },
        { prop: 'date', 
          component: { 
            tag: 'date', on: { change: this.onDateChange.bind(this, 'cert') },
            attrs: { type: 'daterange', 'range-separator': '至', 'start-placeholder': '校准开始日期', 'end-placeholder': '校准结束日期', style: W_370, clearable: true }, 
          } 
        },
        { component: { tag: 's-button', attrs: { btnText: '查询', type: 'primary' }, on: { click: this.onSearch.bind(this, 'cert') } } },
        { component: { tag: 's-button', attrs: { btnText: '一键下载', type: 'primary' }, on: { click: this.downloadAll.bind(this, 'cert') } } }
      ],
    }
  },
  methods: {
    ...mapMutations('certinfos', {
      addProcess: 'ADD_PROCESS',
      addProcessCount: 'ADD_PROCESS_COUNT',
      deleteProcess: 'DELETE_PROCESS'
    }),
    async downloadAll (type) {
      let data = type === 'eq' ? this.$refs.eqTable.getSelectionList() : this.$refs.certTable.getSelectionList()
      data = data.filter(item => (item.certificateNumber || item.certNo))
      if (!data || !data.length) {
        return this.$message.warning('当前未选中数据或所选数据没有证书编号')
      }
      const serialNumber = data.map(item => (item.certificateNumber || item.certNo))
      print('/Cert/BatchDownloadCertPdf', { serialNumber: serialNumber.join(',')  })
      // const params = {
      //   serialNumber,
      //   'X-token': this.$store.state.user.token
      // }
      // const url = '/Cert/DownloadTest'
      // window.open(`${process.env.VUE_APP_BASE_API}${url}?${serializeParams(params)}`)
      console.log(JSZip, getPdfURL, FileSaver, getFile, formatDate, downloadTest)
      // const promiseList = []
      // let zip = new JSZip();
      // const nowDate = new Date()
      // const processItem = {
      //   id: +nowDate,
      //   date: formatDate(nowDate, 'YYYY-MM-DD HH:mm:ss'),
      //   totalCount: data.length,
      //   count: 0
      // }
      // this.addProcess(processItem)
      // for (let i = 0; i < data.length; i++) {
      //   const item = data[i]
      //   const pdfUrl = await getPdfURL('/Cert/DownloadCertPdf', { serialNumber: item.certificateNumber || item.certNo })
      //   promiseList.push(getFile(pdfUrl).then(data => {
      //     console.log(data, 'data')
      //     const file_name = `${+new Date()}_${(item.certificateNumber || item.certNo)}.pdf`
      //     zip.file(file_name, data, { binary: true }) // 逐个添加文件
      //     this.addProcessCount(processItem)
      //   }))
      // }
      // Promise.all(promiseList).then(() => {
      //   zip.generateAsync({ type:"blob" }).then(content => { // 生成二进制流
      //     // this.deleteProcess(processItem)
      //     FileSaver.saveAs(content, "证书下载.zip") // 利用file-saver保存文件
      //   })
      // }).catch(err => {
      //   this.$message.error(err.message || '下载失败')
      // })
    },
    _getEquipmentList () {
      console.log('getList')
    },
    async _getList (type) {
      type === 'cert' ? this.certLoading = true : this.eqLoading = true
      const listQuery = type === 'cert' ? this.certListQuery : { ...this.equipmentListQuery, salesOrderId: this.salesOrderId }
      try {
        let { data, count } = await querySalesLoad(listQuery)
        data = data.map(item => {
          type === 'cert' ? (item.calibrationDate = formatDate(item.calibrationDate)) : (item.time = formatDate(item.time))
          item.expirationDate = formatDate(item.expirationDate)
          return item
        })
        console.log(data, 'count')
        if (type === 'cert') {
          this.certNoList = data
          this.certTotal = count
        } else {
          this.equipmentList = data
          this.eqTotal = count
        }
      } catch (err) {
        this.$message.error(err.message)
        type === 'cert' ? this.certNoList = [] : this.equipmentList = []
      } finally {
        type === 'cert' ? this.certLoading = false : this.eqLoading = false
      }
    },
    onSearch (type) {
      type === 'eq' ? this.equipmentListQuery.page = 1 : this.certListQuery.page = 1
      this._getList(type)
    },
    onDateChange (type, val) {
      if (type === 'eq') {
        this.equipmentListQuery.startCalibrationDate = val ? val[0] : ''
        this.equipmentListQuery.endCalibrationDate = val ? val[1] : ''
      } else {
        this.certListQuery.startCalibrationDate = val ? val[0] : ''
        this.certListQuery.endCalibrationDate = val ? val[1] : ''
      }
      console.log(type, val, 'ondatechange')
    },
    download (row, type) {
      const { certificateNumber, certNo } = row
      if ((!certificateNumber && !type) || (!certNo && type)) {
        return this.$message.warning('无证书编号')
      }
      const serialNumber = type ? certNo : certificateNumber
      print('/Cert/DownloadCertPdf', { serialNumber })
    },
    showCertNoList (row) {
      console.log(row, 'showCertNoList')
      this.certListQuery.manufacturerSerialNumbers = row.testerSn
      this.currentRow = row
      this.$refs.certNoDialog.open()
    },
    onOpened () {
      this._getList('cert')
    },
    onClosed () {
      this.certListQuery = { page: 1, limit: 30, pageStatus: 3 }
    },
    showCertInfo (row) {
      console.log(row, 'showCertInfo')
      this.currentData = row
      this.$refs.certInfoDialog.open()
    },
    handleEqChange (val) {
      Object.assign(this.equipmentListQuery, val)
      this._getList('eq')
    },
    handleCertChange (val) {
      Object.assign(this.certListQuery, val)
      this._getList('cert')
    }
  },
  created () {

  }
}
</script>

<style lang="scss" scoped>
.equipment-wrapper {
  .search-wrapper {
    line-height: 33px;
  }
  .table-wrapper {
    margin-top: 10px;
  }
}
/* 设备列表弹窗 */
.title {
  div {
    margin-left: 10px;
    color: #a6a6a6;
    span {
      margin-left: 10px;
      color: #000;
    }
  }
}
</style>