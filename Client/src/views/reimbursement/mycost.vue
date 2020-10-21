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
          <el-table 
            ref="table"
            :data="tableData" 
            v-loading="tableLoading" 
            size="mini"
            border
            fit
            height="100%"
            style="width: 100%;"
            @row-click="onRowClick"
            highlight-current-row
            >
            <el-table-column
              v-for="item in columns"
              :key="item.prop"
              :width="item.width"
              :label="item.label"
              :align="item.align || 'left'"
              :sortable="item.isSort || false"
              :type="item.originType || ''"
              show-overflow-tooltip
            >
              <template slot-scope="scope" >
                <div class="link-container" v-if="item.type === 'link'">
                  <img :src="rightImg" @click="item.handleJump({ ...scope.row, ...{ type: 'view' }})" class="pointer">
                  <span>{{ scope.$index + 1 }}</span>
                </div>
                <template v-else-if="item.label === '发票附件'">
                  <div class="link-container">
                    <img :src="rightImg" @click="item.handleJump(scope.row.reimburseAttachments)" class="pointer">
                    <span>查看</span>
                  </div>
                </template>
                <template v-else>
                  {{ scope.row[item.prop] }}
                </template>
              </template>    
            </el-table-column>
          </el-table>
          <!-- <common-table :data="tableData" :columns="columns" :loading="tableLoading"></common-table> -->
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
    <!-- 模板弹窗 -->
    <my-dialog
      ref="myDialog"
      :center="true"
      width="1206px"
      :onClosed="closeDialog"
      :title="`${textMap[type]}费用模板`"
      :btnList="btnList"
      :loading="dialogLoading"
    >
      <cost-template 
        ref="cost"
        :operation="type"
        :selectList="selectList" 
        :categoryList="categoryList"
        :detailData="detailData"></cost-template>
    </my-dialog>
    <el-image-viewer
      v-if="dialogVisible"
      :url-list="[dialogImageUrl]"
      :on-close="closeViewer"
    >
    </el-image-viewer>
  </div>
</template>

<script>
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CostTemplate from './common/components/CostTemplate'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import rightImg from '@/assets/table/right.png'
import { getCategoryName } from '@/api/reimburse'
import { categoryMixin } from './common/js/mixins'
import { getList, getDetail, deleteCost } from '@/api/reimburse/mycost'
import { toThousands } from '@/utils/format'
import { downloadFile } from '@/utils/file'
const TRANSPORT_TYPE = 1 // 交通费用类型设为1
const ACC_TYPE = 2 // 住宿费用类型设为2
const OTHER_TYPE = 3 // 交通费用类型设为3
export default {
  mixins: [categoryMixin],
  components: {
    Search,
    Sticky,
    // CommonTable,
    Pagination,
    MyDialog,
    CostTemplate,
    ElImageViewer
  },
  computed: {
    btnList () {
      return [
        { btnText: '保存', handleClick: this.toSave, isShow: this.type !== 'view' },
        { btnText: '关闭', handleClick: this.closeDialog }
      ]
    }
  },
  data () {
    return {
      type: '', // 判断当前操作是新增还是编辑
      rightImg,
      formQuery: { // 查询字段参数
        startTime: '',
        endTime: '',
      },
      listQuery: { // 分页参数
        page: 1,
        limit: 30
      },
      textMap: {
        create: '新建',
        edit: '编辑',
        view: '查看'
      },
      tableData: [],
      total: 0,
      tableLoading: false,
      dialogLoading: false,
      categoryList: [], // 字典分类列表
      searchConfig: [ // 搜索配置
        { placeholder: '填报起始时间', prop: 'startTime', type: 'date', width: 150 },
        { placeholder: '填报结束时间', prop: 'endTime', type: 'date', width: 150 },
        { type: 'search' },
        { type: 'button', handleClick: this.create, btnText: '新建', isSpecial: true, options: { type: 'create' } },
        { type: 'button', handleClick: this.getDetail, btnText: '编辑', options: { type: 'edit' } },
        { type: 'button', handleClick: this.delete, btnText: '删除' },
      ],
      columns: [ // 表格配置
        { label: '序号', type: 'link', handleJump: this.getDetail },
        { label: '费用类型', prop: 'feeType' },
        { label: '总金额', prop: 'moneyText' },
        { label: '发票号码', prop: 'invoiceNumber' },
        { label: '发票附件', prop: 'invoiceAttachment', handleJump: this.jumpToDetail },
        { label: '日期', prop: 'createTime', width: 90 },
        { label: '备注', prop: 'remark' }
      ],
      selectList: [], // 选择列表
      currentRow: null, // 当前选择行的数据
      detailData: {
        list: []
      },
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download", // 图片基地址
      tokenValue: this.$store.state.user.token,
      dialogImageUrl: "",
      dialogVisible: false,
    } 
  },
  methods: {
    closeViewer () {
      this.dialogVisible = false
    },
    _getList () {
      this.tableLoading = true
      getList({
        ...this.listQuery
      }).then(res => {
        let { data, count } = res
        this.total = count
        this.tableData = this._normalizeList(data)
        this.tableLoading = false
      }).catch(() => {
        this.tableLoading = false
        this.$message.error('加载列表失败')
      })
    },
    _getCategoryName () {
      getCategoryName().then(res => {
        this.categoryList = res.data
        this._normalizeSelectList()
      }).catch((err) => {
        console.log(err, 'err')
        this.$message.error('获取字典分类失败')
      })
    },
    _normalizeSelectList () {
      this.selectList.push({
        title: '交通费用',
        options: this.transportationList.map(item => {
          item.type = TRANSPORT_TYPE
          return item
        })
      })
      this.selectList.push({
        title: '住宿费用',
        options: [{ type: ACC_TYPE, label: '住宿费用'}]
      })
      this.selectList.push({
        title: '其他费用',
        options: this.otherExpensesList.map(item => {
          item.type = OTHER_TYPE
          return item
        })
      })
    },
    _normalizeList (data) {
      return data.map(item => {
        item.createTime = item.createTime.split(' ')[0]
        item.moneyText = toThousands(item.money)
        if (item.reimburseType === Number(3)) { // 住宿费
          item.moneyText = toThousands(item.totalMoney)
        }
        return item
      })
    },
    jumpToDetail (attachmentList) {
      let invoiceList = attachmentList.filter(item => item.attachmentType === 2) // 发票附件
      if (invoiceList && invoiceList.length) {
        let { fileId, fileType } = invoiceList[0]
        let url = `${this.baseURL}/${fileId}?X-Token=${this.tokenValue}`
        if (fileType) { // 文件类型 后台返回的
          if (/^image\/\w+/i.test(fileType)) {
            this.dialogImageUrl = url
            this.dialogVisible = true
          } else {
            downloadFile(url)
          }
        } 
      } else {
        this.$message({
          type: 'warning',
          message: '无发票附件'
        })
      }
    },
    getDetail (val) { // 获取详情
      let { type } = val
      let myExpendsId = ''
      if (type === 'view') {
        myExpendsId = val.id
      } else {
        if (!this.currentRow) {
          return this.$message({
            type: 'warning',
            message: '请先选择费用单'
          })
        }
        myExpendsId = this.currentRow.id
      }
      getDetail({
        myExpendsId
      }).then(res => {
        let data = res.data
        this._buildAttachment([data])
        this.detailData = {
          list: [data]
        }
        this.type = type
        this.$refs.myDialog.open()
      }).catch((err) => {
        console.error(err)
        this.$message.error('获取详情失败')
      })
    },
     _buildAttachment (data) { // 为了回显，并且编辑 目标是为了保证跟order.vue的数据保持相同的逻辑
      data.forEach(item => {
        let { reimburseAttachments } = item
        item.fileid = []
        item.invoiceFileList = this.getTargetAttachment(reimburseAttachments, 2)
        item.otherFileList = this.getTargetAttachment(reimburseAttachments, 1)
        item.invoiceAttachment = [],
        item.otherAttachment = []
        item.reimburseAttachments = []
        item.maxMoney = item.totalMoney || item.money
        item.isValidInvoice = Boolean(item.invoiceFileList.length)
      })
    },
    getTargetAttachment (data, attachmentType) { // 用于el-upload 回显
      return data.filter(item => item.attachmentType === attachmentType)
        .map(item => {
          item.name = item.attachmentName
          item.url = `${this.baseURL}/${item.fileId}?X-Token=${this.tokenValue}`
          return item
        })
    },
    onRowClick (row) {
      this.currentRow = row
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
      this._getList()
    },
    handleCurrentChange (val) {
      Object.assign(this.listQuery, val)
      this._getList()
    },
    onSearch () {
      this._getList()
    },
    create () {
      this.type = 'create'
      this.$refs.myDialog.open()
    },
    delete () {
      if (!this.currentRow) {
        return this.$message({
          type: 'warning',
          message: '请先选择费用单'
        })
      }
      this.$confirm('此操作将永久删除该数据, 是否继续?', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        }).then(() => {
          deleteCost([this.currentRow.id]).then(() => {
            this.$message({
              type: 'success',
              message: '删除成功!'
            })
            this._getList()
          }).catch(() => {
            this.$message.error('删除失败')
          })
        }).catch(() => {
          this.$message({
            type: 'info',
            message: '已取消删除'
          })  
        })
    },
    toSave () {
      this.dialogLoading = true
      this.$refs.cost._operate().then(() => {
        this.$message({
          type: 'success',
          message: '保存成功'
        })
        this.closeDialog()
        this.dialogLoading = false
        this._getList()
      }).catch(err => {
        if (!err) {
          this.$message.error('格式错误或必填项未填写')
        } else {
          this.$message.error(err.message)
        }
        this.dialogLoading = false
      })
    },
    closeDialog () {
      this.$refs.cost.resetInfo()
      this.$refs.myDialog.close()
    }
  },
  created () {
    this._getList()
    this._getCategoryName()
  },
  mounted () {

  },
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