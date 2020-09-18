<template>
  <div class="app-container">
    <div class="bg-white">
      <search @search="onSearch"></search>
      <common-table
        :tableData="tableData"
        :headOptions="headOptions"
        :type="type"
        @openDetail="onOpenDetail"
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
import Search from '../commonComponent/search'
import Pagination from '@/components/Pagination'
import OperationRecord from './component/operationRecord'
import Certifiate from '../commonComponent/certifiate'
import { queryLoad } from '@/api/cerfiticate'
import { commonMixin } from '../mixin/mixin'
export default {
  name: 'query',
  mixins: [commonMixin],
  components: {
    CommonTable,
    Pagination,
    Search,
    OperationRecord,
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
      queryLoad(this.pageConfig).then(res => {
        console.log(res, 'res')
        this.tableData = res.data.map(item => {
          item.calibrationDate = this.formatDate(item.calibrationDate)
          return item
        })
      })
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