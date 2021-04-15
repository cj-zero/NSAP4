<template>
  <el-table
    :data="tableData"
    border
    @row-click="onRowClick"
    class="form-wrapper"
    v-loading="isLoading"
    :row-style="rowStyle"
    height="100%"
    @selection-change="handleSelectionChange"
    row-key="id"
    ref="commonTable"
  >
    <el-table-column
      v-if="type !== 'query'"
      type="selection"
      :reserve-selection="true"
      :selectable="selectable"
    >
    </el-table-column>
    <el-table-column   
      v-for="item in headOptions"
      :key="item.label"
      :width="item.width"
      :align="item.align || 'left'"
      :prop="item.name"
      :label="item.label"
      :fixed="item.fixed || false"
    >
      <template slot-scope="scope">
        <!-- 单选按钮 -->
        <template v-if="item.name === 'radio'">
          <!-- <el-radio v-model="radio" :label="9"></el-radio> -->
          <el-radio v-model="radio" :label="scope.row.certNo"></el-radio>
        </template>
        <!-- 序号 -->
        <template v-else-if="item.name === 'order'">
          {{ scope.$index + 1 }}
        </template>
        <!-- 操作按钮 -->
        <template v-else-if="item.name === 'operation'">
          <template v-if="type === 'submit'">
            <el-button 
              v-if="isShowBtn(scope.row.activityName)"
              :type="scope.row.activityName === '待审核' ? 'danger' : 'primary'" 
              size="mini"
              @click="submit(scope.row, scope.row.activityName === '待审核' ? 4 : 0)"
              v-loading.fullscreen.lock="isSend"
              :element-loading-text="showText(scope.row.activityName, true)"
              element-loading-spinner="el-icon-loading"
              element-loading-background="rgba(0, 0, 0, 0.4)"
            >{{ showText(scope.row.activityName) }}</el-button>
          </template>
          <template v-else-if="type === 'query'">
            <el-button type="primary" size="mini" @click="download(scope.row)">下载</el-button>
          </template>
        </template>
        <!-- 证书编号 -->
        <template v-else-if="item.name === 'certNo'">
          <el-link type="primary" @click.stop="openDetail(scope.row, type)">{{ scope.row[item.name] }}</el-link>
        </template>
        <template v-else-if="item.name === 'activityName'">
          <span :style="getColor(scope.row.activityName)">{{ scope.row[item.name] }}</span>
        </template>
        <template v-else>
          {{ scope.row[item.name] }}
        </template>
      </template>
    </el-table-column>
  </el-table>
</template>

<script>
const textMap = ['送审', '撤回'] // 分别对应所在下标
import { certVerMixin } from '../mixin/mixin'
import { downloadFile } from '@/utils/file'
import { getPdfURL } from '@/utils/utils'
const colorList = {
  '待送审': '#cd2b25',
  '待审核': '#15a8ac',
  '待批准': '#0dad48'
}
export default {
  components: {},
  mixins: [certVerMixin],
  props: {
    tableData: { // table数据
      type: Array,
      default() {
        return []
      }
    },
    headOptions: { // 头部选项
      type: Array,
      default () {
        return []
      }
    },
    type: { // 操作类型
      type: String,
      default: ''
    },
    isLoading: {
      type: Boolean,
      default: false
    }
  },
  data () {
    return {
      radio: ''
  }
  },
  methods: {
    isShowBtn (activityName) {
      return this.type === 'submit' && ['待批准'].indexOf(activityName) === -1
    },
    showText (activityName, isLoadingText) {
      const text = activityName === '待审核' ? '撤回' : '送审'
      return isLoadingText ? `正在${text}中` : text
    },
    clearSelection () {
      this.$refs.commonTable.clearSelection()
      this.selectList = []
    },
    selectable (row) {
      console.log(row.activityName !== '待审核', 'selectable')
      return this.type === 'review' || (this.type === 'submit' && row.activityName !== '待审核')
    },
    rowStyle ({ rowIndex }) {
      return rowIndex % 2 === 0 ? {
        'background-color' : '#eee'
      } : {}
    },
    getColor (text) {
      // console.log(text, 'mapText', colorList[text])
      return {
        color: colorList[text] || '#cd2b25'
      }
    },
    handleSelectionChange (val) {
      this.$emit('selectionChange', val)
    },
    onRowClick (row)  {
      this.radio = row.zsOrder
    },
    openDetail (row) {
      // 传出数据跟操作类型
      const { activityName } = row
      const hasSend = activityName !== '待审核' // 撤回
      this.$emit('openDetail', row, hasSend)
    },
    submit (row, type) { // type: 0： 送审 1：撤回
      this._certVerificate(row, type, textMap[type] || '撤回')
    },
    async download (row) {
      let url = await getPdfURL('/Cert/DownloadCertPdf', { serialNumber: row.certNo })
      console.log(url, 'pdfURL')
      downloadFile(url)
    }
  },
  created () {

  },
  mounted () {

  },
  updated () {
  }
}
</script>
<style lang='scss' scoped>
.form-wrapper {
  ::v-deep.el-radio__label {
    display: none;
  }
}
</style>