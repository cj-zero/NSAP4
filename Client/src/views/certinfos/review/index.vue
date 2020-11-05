<template>
  <div class="app-container">
    <div class="bg-white">
      <search @search="onSearch" :type="type"></search>
      <common-table
        :tableData="tableData"
        :headOptions="headOptions"
        :type="type"
        @openDetail="onOpenDetail"
        :isLoading="isLoading"
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
        class="certifiate-dialog"
        :visible.sync="visible"
        width="800px"
        :show-close="false"
      >
        <certifiate
          :type="type"
          placeholder="审批意见"
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
import Search from '../commonComponent/search'
import Pagination from '@/components/Pagination'
import Certifiate from '../commonComponent/certifiate'
import { commonMixin } from '../mixin/mixin'
import { loadApprover } from '@/api/cerfiticate'
export default {
  name: 'review',
  mixins: [commonMixin],
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
        { label: '型号规格', name: 'model', width: '100' },
        { label: '出厂编号', name: 'sn', width: '100' },
        { label: '资产编号', name: 'assetNo', width: '100' },
        { label: '校准日期', name: 'calibrationDate', width: '165' },
        { label: '状态', name: 'activityName', width: '100' },
        { label: '校准人', name: 'operator', width: '' }
      ],
      type: 'review'
    }
  },
  methods: {
    _loadApprover () { // 加载证书审核、证书校验页面
      this.isLoading = true
      loadApprover({
        ...this.pageConfig,
        flowStatus: 2
      }).then(res => {
        console.log(res, 'res')
        this.tableData = res.data.map(item => {
          item.calibrationDate = this.formatDate(item.calibrationDate)
          return item
        })
        this.totalCount = res.count
        this.isLoading = false
      }).catch(() => {
        this.isLoading = false
      })
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