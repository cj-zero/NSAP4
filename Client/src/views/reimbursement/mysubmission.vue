<template>
  <div class="my-submission-wrapper">
    <tab-list :initialName="initialName" :texts="texts" @tabChange="onTabChange"></tab-list>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="listQuery"
          :config="searchConfig"
          @search="onSearch">
        </Search>
      </div>
    </sticky>
    <Layer>
      <common-table
        ref="table"
        height="100%"
        :data="tableData"
        :columns="submissionColumns"
        :loading="tableLoading"
        @row-click="onRowClick"
      >
        <template v-slot:report="{ row }">
          <div class="link-container">
            <img :src="rightImg" @click="openReport(row, 'table')" class="pointer">
            <span>查看</span>
          </div>
        </template>
        <template v-slot:fromTheme="{ row }">
          <span v-infotooltip.ellipsis="row.themeList">{{ row.fromTheme }}</span>
        </template>
      </common-table>
      <pagination
        v-show="total>0"
        :total="total"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleCurrentChange"
      />
    </Layer>
    <my-dialog
      ref="myDialog"
      :width="this.title === 'view' ? '1351px' : '1481px'"
      :btnList="btnList"
      @closed="closeDialog"
      :title="textMap[title]"
      :loading="dialogLoading"
    >
      <order 
        ref="order" 
        :map="map"
        :BMap="BMap"
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
    <!-- 百度地图实例化 -->
    <Bmap @mapInitail="onMapInitail" />
  </div>
</template>

<script>
// 报销状态 1: '撤回' 2: '驳回' 3: '未提交' 4: '客服主管审批' 5: '财务初审' 6: '财务复审' 7: '总经理审批' 8: '待支付' 9: '已支付' -1: '已结束'
import TabList from '@/components/TabList'
import Search from '@/components/Search'
import Order from './common/components/order'
import Report from './common/components/report'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'

import { tableMixin, categoryMixin, reportMixin, chatMixin } from './common/js/mixins'
import { getOrder, withdraw, deleteOrder } from '@/api/reimburse'
import { serializeParams } from '@/utils/process'
// import { loadBMap } from '@/utils/remoteLoad'
import Bmap from '@/components/bmap'
export default {
  name: 'mySubmission',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin],
  components: {
    TabList,
    Search,
    Order,
    Report,
    zxform,
    zxchat,
    Bmap
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } },
        { component: { tag: 's-button', attrs: { btnText: '新建', class: ['customer-btn-class'] }, on: { click: this.addAccount } } },
        { component: { tag: 's-button', attrs: { btnText: '编辑' }, on: { click: this.toEdit } } },
        { component: { tag: 's-button', attrs: { btnText: '撤回', type: 'danger' }, on: { click: this.recall } } },
        { component: { tag: 's-button', attrs: { btnText: '删除', type: 'danger' }, on: { click: this.deleteOrder } } },
        { component: { tag: 's-button', attrs: { btnText: '打印' }, on: { click: this.print } } },
        { component: { tag: 's-button', attrs: { btnText: '导出表格' }, on: { click: this.exportExcel } }, isShow: !!this.isCustomerSupervisor },
        // { type: 'search' },
        // { type: 'button', btnText: '新建', isSpecial: true, handleClick: this.addAccount },
        // { type: 'button', btnText: '编辑', handleClick: this.getDetail, options: { type: 'edit', name: 'mySubmit' } },
        // { type: 'button', btnText: '撤回', handleClick: this.recall },
        // { type: 'button', btnText: '删除', handleClick: this.deleteOrder },
        // { type: 'button', btnText: '打印', handleClick: this.print },
        // { type: 'button', btnText: '导出表格', handleClick: this.exportExcel, isShow: !!this.isCustomerSupervisor }
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
      submissionColumns: [ // 表格配置(我的提交)
        { label: '报销单号', prop: 'mainIdText', type: 'link', width: 70, handleClick: this.getDetail, options: { type: 'view' } },
        { label: '报销状态', prop: 'remburseStatusText', width: 100 },
        { label: '服务ID', prop: 'serviceOrderSapId', width: 80, type: 'link', handleClick: this._openServiceOrder },
        { label: '呼叫主题', prop: 'fromTheme', width: 250, 'show-overflow-tooltip': false, slotName: 'fromTheme' },
        { label: '客户代码', prop: 'terminalCustomerId', width: 75 },
        { label: '客户名称', prop: 'terminalCustomer', width: 170 },
        { label: '总金额', prop: 'totalMoney', width: 100, align: 'right' },
        { label: '总天数', prop: 'days', width: 60, align: 'right' },
        { label: '出发日期', prop: 'businessTripDate', width: 85 },
        { label: '结束日期', prop: 'endDate', width: 85 },
        { label: '报销部门', prop: 'orgName', width: 70 },
        { label: '报销人', prop: 'userName', width: 70 },
        // { label: '劳务关系', prop: 'serviceRelations', width: 100 },
        { label: '业务员', prop: 'salesMan', width: 80 },
        { label: '服务报告', width: 70, slotName: 'report'  },
        { label: '填报日期', prop: 'fillTime', width: 85 },
        { label: '备注', prop: 'remark' }
      ],
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
      submitLoading: false, 
      draftLoading: false,
      editLoading: false,
      isSubmit: true,
      map: null,
      BMap: null
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
    toEdit () {
      this.getDetail({ type: 'edit', name: 'mySubmit' })
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
        : this._edit()
    }, 
    saveAsDraft () { // 存为草稿
      this.title === 'create' // 判断是新建的还是已经创建的
        ? this._addOrder(true)
        : this._edit(true)
    }, 
    _edit (isDraft) {
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
    },
    onMapInitail ({ map, BMap }) {
      this.map = map
      this.BMap = BMap
      console.log(map, BMap)
    }
  },
  async created () {
    this.listQuery.pageType = 1
    this._getList()
    this._getCategoryName()
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