<template>
  <el-table
    :data="tableData"
    border
    @row-click="onRowClick"
    class="form-wrapper"
    v-loading="isLoading"
  >
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
            <el-button type="primary" size="mini" @click="submit(scope.row, 0)">送审</el-button>
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
      radio: '',
      baseURL: `${process.env.VUE_APP_BASE_API}/Cert/DownloadCertPdf`,
  }
  },
  methods: {
    getColor (text) {
      console.log(text, 'mapText', colorList[text])
      return {
        color: colorList[text]
      }
    },
    onRowClick (row)  {
      console.log(row, 'row')
      this.radio = row.zsOrder
    },
    openDetail (row) {
      // 传出数据跟操作类型
      console.log('openDetail')
      this.$emit('openDetail', row)
    },
    submit (row, type) { // type: 0： 送审 1：撤回
      this._certVerificate(row, type, textMap[type])
    },
    download (row) {
      let url = `${this.baseURL}/${row.certNo}?X-Token=${this.$store.state.user.token}`
      downloadFile(url)
    }
  },
  created () {

  },
  mounted () {

  },
  updated () {
    console.log(this.tableData, this.headOptions, 'updated')
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