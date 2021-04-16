<template>
  <div 
    class="app-container"
    v-loading.fullscreen="isSend"
    :element-loading-text="`${loadingText}中`"
    element-loading-spinner="el-icon-loading"
    element-loading-background="rgba(0, 0, 0, 0.3)"
  >
    <div class="bg-white">
      <sticky :className="'sub-navbar'">
        <div class="filter-container">
          <search :config="searchConfig" :listQuery="pageConfig" @search="onSearch"></search>
        </div>
      </sticky>
      <!-- <search @search="onSearch" :type="type" @approve="onApprove"></search> -->
      <common-table
        ref="table"
        :isLoading="isLoading"
        :tableData="tableData"
        :headOptions="headOptions"
        :type="type"
        @openDetail="onOpenDetail"
        @handleSubmit="onHandleSubmit"
        @selectionChange="onSelection"
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
      >
        <certifiate
          :type="type"
          :hasSend="hasSend"
          :certNo="currentCertNo"
          :currentData="currentData"
          @handleSubmit="onHandleSubmit"
          @close="closeDialog"
        ></certifiate>
        <span slot="footer" class="dialog-footer">
          <el-button @click="visible = false" size="mini">取 消</el-button>
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
import Certifiate from '../commonComponent/certifiate'
import { commonMixin, certVerMixin } from '../mixin/mixin'
import { loadApprover } from '@/api/cerfiticate'
export default {
  name: 'submit',
  mixins: [commonMixin, certVerMixin],
  components: {
    CommonTable,
    Pagination,
    Search,
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
        { label: '状态', name: 'activityName', width: '100' },
        { label: '操作', name: 'operation', width: '100' },
        { label: '备注', name: 'rejectContent' }
      ],
      type: 'submit',
      hasSend: true,
      activityName: ''
    }
  },
  watch: {
    isSend (val) {
      console.log(val, 'isSend')
    }
  },
  computed: {
    loadingText () {
      return this.activityName === '待审核' ? '撤回' : '送审'
    }
  },
  methods: {
    _loadApprover () { // 加载证书审核、证书校验页面
      this.isLoading = true
      loadApprover({
        ...this.pageConfig,
        flowStatus: 1
      }).then(res => {
        console.log(res, 'res')
        this.tableData = res.data.map(item => {
          item.calibrationDate = this.formatDate(item.calibrationDate)
          return item
        })
        this.totalCount = res.count
        this.isLoading = false
        this.$refs.table.clearSelection()
      }).catch(() => {
        this.isLoading = false
      })
    },
    onSubmit ({ row, type, text }) {
      this.activityName = row.activityName
      this._certVerificate(row, type, text)
    }
  },
  created () {
    this._loadApprover()
  }
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