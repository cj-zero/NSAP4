<template>
  <div class="my-submission-wrapper">
    <tab-list :initialName="initialName" :texts="texts" @tabChange="onTabChange"></tab-list>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
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
              v-for="item in submissionColumns"
              :key="item.prop"
              :width="item.width"
              :label="item.label"
              :align="item.align || 'left'"
              :sortable="item.isSort || false"
              :type="item.originType || ''"
              :show-overflow-tooltip="item.label !== '呼叫主题'"
            >
              <template slot-scope="scope" >
                <div class="link-container" v-if="item.type === 'link'">
                  <img :src="rightImg" @click="item.handleJump({ ...scope.row, ...{ type: 'view' }})" class="pointer">
                  <span>{{ scope.row[item.prop] }}</span>
                </div>
                <template v-else-if="item.label === '服务报告'">
                  <div class="link-container">
                    <img :src="rightImg" @click="item.handleClick(scope.row, 'table')" class="pointer">
                    <span>查看</span>
                  </div>
                </template>
                <template v-else-if="item.label === '呼叫主题'">
                  <span v-infotooltip.top-start.ellipsis="scope.row.themeList">{{ scope.row[item.prop] }}</span>
                  <!-- <el-tooltip placement="top-start">
                    <div slot="content">
                      <p v-for="(content, index) in scope.row.themeList" :key="index">{{ content }}</p>
                    </div>
                    <span style="white-space: nowrap;">{{ scope.row[item.prop] }}</span>
                  </el-tooltip> -->
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
    <my-dialog
      ref="myDialog"
      :width="this.title === 'view' ? '1206px' : '1336px'"
      :btnList="btnList"
      @closed="closeDialog"
      :title="textMap[title]"
      :loading="dialogLoading"
    >
      <order 
        ref="order" 
        :title="title"
        :detailData="detailData"
        :categoryList="categoryList"
        :customerInfo="customerInfo">
      </order>
    </my-dialog>
    <my-dialog
      ref="reportDialog"
      width="983px"
      title="服务行为报告单"
      @closed="resetReport">
      <Report :data="reportData" ref="report"/>
    </my-dialog>
    <!-- 只能查看的表单 -->
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
// 报销状态 1: '撤回' 2: '驳回' 3: '未提交' 4: '客服主管审批' 5: '财务初审' 6: '财务复审' 7: '总经理审批' 8: '待支付' 9: '已支付' -1: '已结束'
import TabList from '@/components/TabList'
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import Order from './common/components/order'
import Report from './common/components/report'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { tableMixin, categoryMixin, reportMixin, chatMixin } from './common/js/mixins'
import { getOrder, withdraw, deleteOrder } from '@/api/reimburse'
import { serializeParams } from '@/utils/process'
export default {
  name: 'mySubmission',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin],
  components: {
    TabList,
    Search,
    Sticky,
    // CommonTable,
    Pagination,
    MyDialog,
    Order,
    Report,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { type: 'search' },
        { type: 'button', btnText: '新建', isSpecial: true, handleClick: this.addAccount },
        { type: 'button', btnText: '编辑', handleClick: this.getDetail, options: { type: 'edit', name: 'mySubmit' } },
        { type: 'button', btnText: '撤回', handleClick: this.recall },
        { type: 'button', btnText: '删除', handleClick: this.deleteOrder },
        { type: 'button', btnText: '打印', handleClick: this.print },
        { type: 'button', btnText: '导出表格', handleClick: this.exportExcel, isShow: !!this.isCustomerSupervisor }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '导入费用', handleClick: this.importFee, isShow: this.title !== 'view' },
        { btnText: '提交', handleClick: this.submit, loading: this.submitLoading, isShow: this.title !== 'view' },
        { btnText: '存为草稿', handleClick: this.saveAsDraft, loading: this.draftLoading, isShow: this.title !== 'view' },
        { btnText: '重置', handleClick: this.reset, isShow: this.title === 'create', className: 'danger' },
        { btnText: '关闭', handleClick: this.closeDialog, className: 'close' }
      ]
    }
  },
  data () {
    return {
      initialName: '', // 初始标签的值
      texts: [ // 标签数组
        { label: '全部', name: '' },
        { label: '草稿箱', name: '3' },
        { label: '报销中', name: '4' },
        { label: '已支付', name: '9' },
        { label: '已撤回/已驳回', name: '1' }
      ],
      statusOptions: [ // 审核状态
        { label: '全部', value: '' },
        { label: '审批中', value: '1' },
        { label: '已支付', value: '2' },
        { label: '已撤回', value: '3' },
        { label: '草稿箱', value: '4' }
      ],
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
      submitLoading: false, 
      draftLoading: false,
      editLoading: false,
      isSubmit: true
    } 
  },
  methods: {
    onTabChange (name) {
      this.listQuery.remburseStatus = name
      if (name) {
        // 如果不是tab不是全部，就删除listQuery.status字段
        delete this.listQuery.status
      }
      this.listQuery.page = 1
      this._getList()
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
    },
    addAccount () { // 添加
      getOrder().then(res => {
        let data = res.data
        if (data && data.length) {
          let { 
            userId, 
            userName, 
            orgName, 
            becity, 
            businessTripDate, 
            destination, 
            endDate,
            serviceRelations 
          } = data[0]
          this.customerInfo = {
            createUserId: userId,
            userName,
            orgName,
            becity,
            businessTripDate,
            endDate,
            destination,
            serviceRelations 
          }
          this.title = 'create'
          this.$refs.myDialog.open()
        } else {
          this.$message.error('暂无可报销服务单号，请凭借已完成服务单号进行报销')
        }
      }).catch(() => {
        this.$message.error('获取服务单列表失败')
      })
    },
    recall () { // 撤回操作
      if (!this.currentRow) { // 编辑审核等操作
        return this.$message.warning('请先选择报销单')
      }
      if (this.currentRow.createUserId !== this.originUserId) {
        return this.$message.warning('当前用户与报销人不符，不可编辑')
      }
      if (this.currentRow.remburseStatus !== 4) { // 只有到客服主管审批的时候才可以撤回，且未读
        return this.$message.warning('当前状态不可撤回')
      }
      this.$confirm('确定撤回?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        withdraw({
          reimburseInfoId: this.currentRow.id
        }).then(() => {
          this.$message.success('撤回成功')
          this._getList()
        }).catch(err => {
          this.$message.error(err.message || '撤回失败')
        })
      })
    },
    deleteOrder () { // 删除报销单
      if (!this.currentRow) { // 编辑审核等操作
        return this.$message.warning('请先选择报销单')
      }
      if (this.currentRow.createUserId !== this.originUserId) {
        return this.$message.warning('当前用户与报销人不符，不可编辑')
      }
      if (this.currentRow.remburseStatus !== 3) { // 只有草稿状态，即未提交才可以删除
        return this.$message.warning('当前状态不可删除')
      }
      this.$confirm('确定删除?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        deleteOrder({
          reimburseInfoId: this.currentRow.id
        }).then(() => {
          this.$message.success('删除成功')
          this._getList()
        }).catch(err => {
          this.$message.error(err.message || '删除失败')
        })
      })
    },
    _addOrder (isDraft = false) {
      isDraft
        ? this.draftLoading = true
        : this.submitLoading = true
      this.dialogLoading = true
      this.$refs.order.submit(isDraft).then(() => {
        this.$message.success(isDraft ? '存为草稿成功' : '提交成功')
        this._getList()
        this.$refs.order.resetInfo()
        this.closeDialog()
        isDraft
          ? this.draftLoading = false
          : this.submitLoading = false
        this.dialogLoading = false
      }).catch(err => {
        isDraft
          ? this.draftLoading = false
          : this.submitLoading = false
        this.dialogLoading = false
        this.$message.error(err.message)
      })
    },
    importFee () {
      this.$refs.order.openCostDialog()
    },
    submit () { // 提交
      this.title === 'create'
        ? this._addOrder()
        : this.edit()
    }, 
    saveAsDraft () { // 存为草稿
      this.title === 'create' // 判断是新建的还是已经创建的
        ? this._addOrder(true)
        : this.edit(true)
    }, 
    edit (isDraft) {
      // 编辑
      isDraft
        ? this.draftLoading = true
        : this.editLoading = true
      this.dialogLoading = true
      this.$refs.order.updateOrder(isDraft).then(() => {
        this.$message.success(isDraft ? '存为草稿成功' : '提交成功')
        this._getList()
        this.$refs.order.resetInfo()
        this.closeDialog()
        isDraft
          ? this.draftLoading = false
          : this.editLoading = false
        this.dialogLoading = false
      }).catch((err) => {
        isDraft
          ? this.draftLoading = false
          : this.editLoading = false
        this.dialogLoading = false
        this.$message.error(err.message)
      })
    },
    exportExcel () {
      let params = serializeParams(this.listQuery)
      console.log(this.$store.state.user.token, 'token')
      let staticUrl = `${process.env.VUE_APP_BASE_API}/serve/Reimburse/ExportLoad?${params}&X-token=${this.$store.state.user.token}`
      window.open(staticUrl, '_blank')
    },
    reset () {
      this.$confirm('确定重置?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        this.$refs.order.resetInfo()
      })
      // this.$refs.order.resetInfo()
    }, // 重置
    closeDialog () {
      this.$refs.order.resetInfo()
      this.$refs.myDialog.close()
    }
  },
  created () {
    this.listQuery.pageType = 1
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