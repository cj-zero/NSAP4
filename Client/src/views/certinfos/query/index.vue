<template>
  <div class="app-container"
    v-loading.fullscreen="isSend"
    element-loading-text="驳回中"
    element-loading-spinner="el-icon-loading"
    element-loading-background="rgba(0, 0, 0, 0.3)"
  >
    <div class="bg-white">
      <sticky :className="'sub-navbar'">
        <div class="filter-container">
          <search :config="searchConfig" :listQuery="pageConfig" @search="onSearch"></search>
        </div>
      </sticky>
      <common-table
        :tableData="tableData"
        :headOptions="headOptions"
        :type="type"
        :isLoading="isLoading"
        @openDetail="onOpenDetail"
        @submit="onSubmit"
      >
      </common-table>
      <pagination
        v-show="totalCount > 0"
        :total="totalCount"
        :page.sync="pageConfig.page"
        :limit.sync="pageConfig.limit"
        @pagination="handleChange"
      />
      <el-dialog 
        v-el-drag-dialog
        class="certifiate-dialog dialog-mini"
        :visible.sync="visible"
        width="800px"
        :show-close="false"
        :modal="false"
        :modal-append-to-body="false"
        :top="'76px'"
        @closed="onClosed">
        <el-tabs v-model="activeName" type="card" @tab-click="handleClick">
          <el-tab-pane label="校准证书" name="first">
            <certifiate
              :type="type"
              placeholder="退回意见"
              :certNo="currentCertNo"
              :currentData="currentData"
              @handleSubmit="onHandleSubmit"
              @close="closeDialog"
            ></certifiate>
          </el-tab-pane>
          <el-tab-pane label="操作记录" name="second">
            <operation-record :id="currentId"></operation-record>
          </el-tab-pane>
          <el-tab-pane label="原始记录" name="third">
            <origin-record :currentData="currentData"></origin-record>
          </el-tab-pane>
        </el-tabs>
        <span slot="footer" class="dialog-footer">
          <el-button @click="closeDialog" size="mini">取 消</el-button>
        </span>
      </el-dialog>
    </div>
  </div>
</template>

<script>
import CommonTable from '../commonComponent/table'
// import Search from '../commonComponent/search'
import Search from '@/components/Search'
import Pagination from '@/components/Pagination'
import OperationRecord from './component/operationRecord'
import OriginRecord from './component/originRecord'
import Certifiate from '../commonComponent/certifiate'
import { queryLoad } from '@/api/cerfiticate'
import { commonMixin, certVerMixin } from '../mixin/mixin'
export default {
  name: 'query',
  mixins: [commonMixin, certVerMixin],
  components: {
    CommonTable,
    Pagination,
    Search,
    OperationRecord,
    OriginRecord,
    Certifiate
  },
  data () {
    return {
      headOptions: [
        // { label: '', name: 'radio', width: '50' },
        { label: '序号',  name: 'order', width: '50' },
        { label: '证书编号', name: 'certNo', width: '120' },
        { label: '型号规格', name: 'model', width: '180' },
        { label: '出厂编号', name: 'sn', width: '100' },
        { label: '资产编号', name: 'assetNo', width: '100' },
        { label: '校准日期', name: 'calibrationDate', width: '165' },
        { label: '校准人', name: 'operator', width: '100' },
        { label: '操作', name: 'operation', width: '' }
      ],
      activeName: 'first', // 卡片标识
      type: 'query', // 模块类型
      currentId: '', // 证书对应的ID
    }
  },
  methods: {
    _getQueryList () { // 获取表格列表
      this.isLoading = true
      queryLoad(this.pageConfig).then(res => {
        this.totalCount = res.count
        this.tableData = res.data.map(item => {
          item.calibrationDate = this.formatDate(item.calibrationDate)
          return item
        })
        this.isLoading = false
      }).catch(err => {
        this.isLoading = false
        this.$message.error(err.message)
      })
    },
    onSubmit ({ row, type, text }) {
      this._certVerificate(row, type, text)
    }
  },
  created () {
    this._getQueryList()
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.certifiate-dialog {
  ::v-deep .custom-theme .el-dialog__body {
    padding: 0 20px;
  }
  ::v-deep .el-dialog__body {
    padding: 0 20px;
  }
}
</style>