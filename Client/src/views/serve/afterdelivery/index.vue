<template>
  <div class="my-submission-wrapper">
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="formQuery" 
          :config="searchConfig"
          @changeForm="onChangeForm" 
          @search="onSearch">
        </Search>
      </div>
    </sticky>
    <div class="app-container">
      <div class="bg-white">
        <div class="content-wrapper">
          <common-table 
            height="100%"
            ref="expressTable" 
            :data="tableData" 
            :columns="repairColumns" 
            :loading="tableLoading"
            @row-click="onRowClick">
            <template v-slot:attachment="{ row }">
              <!-- <UploadFile 
                uploadType="file" 
                :fileList="row.fileList" 
                :ifShowTip="false" 
                :disabled="true" 
              /> -->
              <AttachmentList :fileList="row.fileList">
                <template v-slot:a>
                  aaaaa
                </template>
                <template slot-scope="scope">
                  <span>{{ scope.row.index }}</span>
                </template>
              </AttachmentList>
            </template>
          </common-table>
          <pagination
            v-show="total>0"
            :total="total"
            :page.sync="listQuery.page"
            :limit.sync="listQuery.limit"
            @pagination="handleCurrentChange"
          />
        </div>
      </div>
    </div>    
    <my-dialog 
      ref="expressDialog"
      width="400px"
      :loading="dialogLoading"
      title="快递单详情"
      :btnList="btnList"
      @closed="close"
    >
      <express-order 
        ref="expressOrder" 
        :currentRow="currentRow"
        :status="status"
      ></express-order>
    </my-dialog>
    <!-- 只能查看的表单 -->
    <my-dialog
      width="515px"
      ref="expressListDialog"
      title="物流信息">
      <time-line :expressInfoList="expressInfoList"></time-line>
    </my-dialog>
    <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
    >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
            formName="查看"
            labelposition="right"
            labelwidth="72px"
            max-width="800px"
            :isCreate="false"
            :refValue="dataForm"
          ></zxform>
        </el-col>
        <el-col :span="6" class="lastWord">   
          <zxchat :serveId='serveId' formName="报销"></zxchat>
        </el-col>
      </el-row>
    </my-dialog>
  </div>
</template>

<script>
import { getReturnRepairList, withDrawExpress, getExpressInfo } from '@/api/serve/afterdelivery'
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
// import UploadFile from '@/components/upLoadFile'
import AttachmentList from '@/components/AttachmentList'
import TimeLine from './components/TimeLine'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import ExpressOrder from './components/ExpressOrder'
import { processDownloadUrl } from '@/utils/file'
import { formatDate, getWeek } from '@/utils/date'
import { chatMixin } from '@/mixins/serve'
// import tableData from './mock'
export default {
  name: 'afterDelivery',
  mixins: [chatMixin],
  components: {
    Search,
    Sticky,
    CommonTable,
    Pagination,
    MyDialog,
    zxform,
    zxchat,
    // UploadFile,
    ExpressOrder,
    AttachmentList,
    TimeLine
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'sapId', placeholder: '服务ID', width: 100 },
        { prop: 'customer', placeholder: '客户', width: 100 },
        { prop: 'expressNum', placeholder: '快递单号', width: 100 },
        { prop: 'creater', placeholder: '寄件人', width: 100 },
        { prop: 'expressState', placeholder: '快递状态', width: 100, type: 'select', options: this.statusOptions },
        { prop: 'startDate', placeholder: '创建开始日期', type: 'date', width: 150 },
        { prop: 'endDate', placeholder: '创建结束日期', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', btnText: '寄出', handleClick: this.send, isSpecial: true },
        { type: 'button', btnText: '撤回', handleClick: this.withDrawExpress, style: { backgroundColor: '#f56c6c', color: '#fff' } },
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '提交', handleClick: this.submit, isShow: this.status !== 'view', loading: this.submitLoading },
        { btnText: '关闭', handleClick: this.close, className: 'close' }      
      ]
    }
  },
  data () {
    return {
      submitLoading: false,
      cancelRequestFn: null,
      expressInfoList: [], // 物流信息列表
      statusOptions: [
        { label: '全部', value: 0 },
        { label: '未签收', value: 1 },
        { label: '已签收', value: 2 },
      ],
      formQuery: {
        sapId: '', // 服务ID
        customer: '', // 客户
        expressNum: '', // 快递单号
        creater: '', // 寄件人
        expressState: 0, // 快递状态
        startDate: '', // 创建开始
        endDate: '' // 创建结束
      },
      listQuery: {
        page: 1,
        limit: 50,
      },
      dialogLoading: false,
      tableLoading: false,
      tableData: [],
      total: 0,
      currentRow: null,
      repairColumns: [
        { type: 'index', label: '序号' },
        { label: '服务ID', prop: 'u_SAP_ID', handleClick: this._openServiceOrder, type: 'link', width: 60 },
        { label: '设备类型', prop: 'materialType', width: 60 },
        { label: '客户代码', prop: 'customerId', width: 80 },
        { label: '客户名称', prop: 'customerName', width: 100 },
        { label: '寄回/寄出', prop: 'typeName', width: 70 }, 
        { label: '快递单号', prop: 'expressNumber', width: 60 },
        { label: '快递信息', prop: 'expressInfoName', type: 'link', handleClick: this.getExpressInfo },
        { label: '寄件人', prop: 'creater', width: 80 },
        { label: '备注', prop: 'remark' },
        { label: '附件', slotName: 'attachment', width: 100 },
        { label: '创建时间', prop: 'createTime', width: 80 }
      ],
      status: 'create' // 报价单状态
    } 
  },
  methods: {
    onRowClick (row) {
      this.currentRow = row
    },
    _getList () {
      this.tableLoading = true
      getReturnRepairList(this.listQuery).then(res => {
        let { count, data } = res
        this.tableData = this._normalizeList(data)
        this.total = count
        this.tableLoading = false
        this.$refs.expressTable.resetCurrentRow()
        this.currentRow = null
      }).catch(err => {
        this.$message.error(err.message)
        this.tableLoading = false
      })
    },
    _normalizeList (list) {
      return list.map(item => {
        let { expressId, expressInfo } = item
        let result = { expressId, ...expressInfo[0] }
        let  { expressInformation, expressAccessorys, isCheck } = result
        let expressInfoList = expressInformation ? JSON.parse(expressInformation).data : '' // 物流信息
        let newContext = expressInfoList ? expressInfoList[expressInfoList.length - 1].context : ''
        result.expressInfoName = (Number(isCheck) ? '已签收 ' : '') + newContext
        result.fileList = expressAccessorys.map(item => {
          item.name = item.fileName
          item.url = processDownloadUrl(item.fileId)
          return item
        })
        return result
      })
    },
    withDrawExpress () {
      let currentRow = this.$refs.expressTable.getCurrentRow()
      if (!currentRow) {
        return this.$message.warning('请先选择数据')
      }
      if (currentRow.typeName === '寄回') {
        return this.$message.warning('当前状态不可撤回')
      }
      this.$confirm('确认撤回？', '提示信息', {
        confirmButtonText: '确认',
        cancelButtonText: '取消'
      }).then(() => {
        withDrawExpress({ expressId: currentRow.expressId }).then(() => {
          this.$message.success('撤销成功')
          this._getList()
        }).catch(err => {
          this.$message.error(err.message)
        })
      })
    },
    submit () { // 提交
      this.submitLoading = true
      this.$refs.expressOrder.submit().then(() => {
        this.$message.success('寄出成功')
        this.submitLoading = false
        this._getList()
        this.close()
      }).catch(err => {
        this.submitLoading = false
        this.$message.error(err.message)
      })
    },
    send () { // 寄出
      let currentRow = this.$refs.expressTable.getCurrentRow()
      if (!currentRow) {
        return this.$message.warning('请先选择数据')
      }
      if (currentRow.typeName === '寄出') {
        return this.$message.warning('当前状态不可寄出')
      }
      this.currentRow = currentRow
      this.$refs.expressDialog.open()
    },
    getExpressInfo (row) { // 获取物流信息
      if (Number(row.isCheck)) {
        return this.$message.warning('快递已签收')
      }
      if (this.cancelRequestFn) {
        this.cancelRequestFn()
      }
      getExpressInfo({ id: row.expressId }, this).then(res => {
        let { expressinfo } = res.data
        if (!expressinfo) {
          return this.$message.warning('暂无物流数据')
        }
        let expressInfoList = JSON.parse(expressinfo).data
        this.expressInfoList = this.normalizeExpressInfoList(expressInfoList)
        this.$refs.expressListDialog.open()
      }).catch(err => {
        this.$message.error(err.message)
      })
    },
    normalizeExpressInfoList (list) {
      return list.map(item => {
        item.week = getWeek(item.time)
        item.hourMins = formatDate(item.time, 'HH:mm')
        item.dateTime = formatDate(item.time, 'YYYY.MM.DD')
        return item
      })
    },
    _openServiceOrder (row) {
      console.log(row)
      this.openTree(row.serviceOrderId, () => this.tableLoading = true, () => this.tableLoading = false)
    },
    onChangeForm (val) {
      this.listQuery.page = 1
      Object.assign(this.listQuery, val)
      this.onSearch()
    },
    onSearch () {
      // this.listQuery.page = 1
      this._getList()
    },
    handleCurrentChange ({ page, limit }) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    },
    close () {
      this.$refs.expressOrder.resetInfo()
      this.$refs.expressDialog.close()
    }
  },
  created () {
    this._getList()
  }
}
</script>
<style lang='scss' scoped>
.my-submission-wrapper {
  ::v-deep .el-tabs__header {
    background-color: #fff;
    margin-bottom: 0;
  }
}
</style>