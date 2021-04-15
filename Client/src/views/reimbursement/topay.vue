<template>
  <div class="my-submission-wrapper">
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
        :columns="toPayColumns"
        :loading="tableLoading"
      ></common-table>
      <pagination
        v-show="total>0"
        :total="total"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleCurrentChange"
      />
    </Layer>
      <!-- 审核弹窗 -->
      <my-dialog
        ref="myDialog"
        width="1351px"
        @closed="closeDialog"
        :title="textMap[title]"
        :btnList="btnList"
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
      <!-- 完工报告 -->
      <!-- <my-dialog
        ref="reportDialog"
        width="983px"
        title="服务行为报告单"
        :onClosed="resetReport">
        <Report :data="reportData" ref="report"/>
      </my-dialog> -->
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
import Search from '@/components/Search'
import Order from './common/components/order'
// import Report from './common/components/report'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import { tableMixin, categoryMixin, reportMixin, chatMixin } from './common/js/mixins'
import { pay } from '@/api/reimburse'
import { serializeParams } from '@/utils/process'
export default {
  name: 'toPay',
  mixins: [tableMixin, categoryMixin, reportMixin, chatMixin],
  components: {
    Search,
    Order,
    // Report,
    zxform,
    zxchat
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } },
        { component: { tag: 's-button', attrs: { btnText: '支付', type: 'primary' }, on: { click: this._pay } }, isShow: this.isCN },
        { component: { tag: 's-button', attrs: { btnText: '导出', class: ['customer-btn-class'] }, on: { click: this._export } }, isShow: this.isCN },
        // { type: 'search' },
        // { type: 'button', handleClick: this._pay, btnText: '支付', isSpecial: true, options:  { type: 'toPay' }, isShow: this.isCN },
        // { type: 'button', handleClick: this._export, btnText: '导出', isSpecial: true, isShow: this.isCN }
      ]
    }, // 搜索配置
    btnList () {
      return [
        { btnText: '确认支付', handleClick: this.toPay, isShow: this.title !== 'view' },
        { btnText: '关闭', handleClick: this.closeDialog, className: 'close' }
      ]
    },
    isCN () { // 出纳角色
      return this.rolesList && this.rolesList.length
        ? this.rolesList.some(item => item === '出纳')
        : false
    }
  },
  data () {
    return {
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
      token: this.$store.state.user.token,
      selectList: [], // 多选已经选中的列表
      isToPay: true
    }
  },
  methods: {
    _pay () {
      this.selectList = this.$refs.table.getSelectionList()
      if (!this.selectList.length) {
        return this.$message.warning('请先选择报销单')
      }
      let reimburseId = this.selectList.map(item => item.id)
      console.log(reimburseId, 'reimburseList')
      this.$confirm('是否已支付报销费用？?', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        }).then(() => {
          this.tableLoading = true
          pay({ reimburseId }).then(() => {
            this.$message.success('支付成功')
            this.tableLoading = false
            this._getList()
          }).catch(err => {
            this.tableLoading = false
            this.$message.error(err.message)
          })
        })
    },
    _export () {
      this.$confirm('确认导出？', '确认信息', {
        confirmButtonText: '确认',
        cancelButtonText: '取消',
        closeOnClickModal: false,
        type: 'warning'
      })
      .then(() => {
        let searchStr = serializeParams(this.listQuery)
        searchStr += `&X-Token=${this.token}`
        console.log(searchStr)
        window.open(`${process.env.VUE_APP_BASE_API}/serve/Reimburse/Export?${searchStr}`, '_blank')
      }) 
    },
    closeDialog () {
      this.$refs.order.resetInfo()
      this.$refs.myDialog.close()
    }
  },
  created () {
    this.listQuery.pageType = 5
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