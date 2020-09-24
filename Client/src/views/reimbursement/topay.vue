<template>
  <div class="my-submission-wrapper">
    <tab-list :initialName="initialName" :texts="texts" @tabChange="onTabChange"></tab-list>
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
              show-overflow-tooltip
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
              >
                <template slot-scope="scope" >
                  <div class="link-container" v-if="item.type === 'link'">
                    <img :src="rightImg" @click="item.handleJump(scope.row)" class="pointer">
                    <span>{{ scope.row[item.prop] }}</span>
                  </div>
                  <template v-else-if="item.type === 'operation'">
                    <el-button 
                      v-for="btnItem in item.actions"
                      :key="btnItem.btnText"
                      @click="btnItem.btnClick(scope.row)" 
                      type="text" 
                      :icon="item.icon || ''"
                      :size="item.size || 'mini'"
                    >{{ btnItem.btnText }}</el-button>
                  </template>
                  <template v-else-if="item.label === '总金额'">
                    {{ scope.row[item.prop] | toThousands }}
                  </template>
                  <template v-else-if="item.label === '服务报告'">
                    <el-button @click="item.handleClick" size="mini" type="primary">{{ item.btnText }}</el-button>
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
        :center="true"
        width="800px"
        :btnList="btnList"
        :onClosed="closeDialog"
        :title="textMap[title]"
      >
        <order 
          ref="order" 
          :title="title"
          :detailData="detailData"
          :categoryList="categoryList"
          :customerInfo="customerInfo">
        </order>
      </my-dialog>
  </div>
</template>

<script>
import TabList from '@/components/TabList'
import Search from '@/components/Search'
import Sticky from '@/components/Sticky'
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import Order from './common/components/order'
import { tableMixin, categoryMixin } from './common/js/mixins'
import { getOrder, getList, getDetails } from '@/api/reimburse'

export default {
  mixins: [tableMixin, categoryMixin],
  components: {
    TabList,
    Search,
    Sticky,
    // CommonTable,
    Pagination,
    MyDialog,
    Order
  },
  computed: {
    searchConfig () {
      return [
        ...this.commonSearch,
        { type: 'search' },
        { type: 'button', btnText: '新建', handleClick: this.addAccount },
        { type: 'button', btnText: '编辑', handleClick: this.edit },
        { type: 'button', btnText: '撤回', handleClick: this.recall }
      ]
    } // 搜索配置
  },
  data () {
    return {
      initialName: '', // 初始标签的值
      texts: [ // 标签数组
        { label: '全部', name: '' },
        { label: '草稿箱', name: 'draft' },
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
      btnList: [
        { btnText: '提交', handleClick: this.submit },
        { btnText: '存为草稿', handleClick: this.saveAsDraft },
        { btnText: '重置', handleClick: this.reset },
        { btnText: '关闭', handleClick: this.closeDialog }
      ],
      customerInfo: {}, // 当前报销人的id， 名字
      categoryList: [], // 字典数组
    }
  },
  methods: {
    _getList () {
      this.tableLoading = true
      console.log('before getList', this.formQuery, this.listQuery)
      getList({
        ...this.formQuery,
        ...this.listQuery
      }).then(res => {
        let { data, count } = res
        this.tableData = this._normalizeList(data).reverse()
        this.total = count
        console.log(res, 'res', this.tableData)
        this.tableLoading = false
      }).catch(() => {
        this.tableLoading = false
      })
    },
    _normalizeList (data) {
      return data.map(item => {
        let { reimburseResp } = item
        delete item.reimburseResp
        item = Object.assign({}, item, { ...reimburseResp })
        return item
      })
    },
    handleCurrentChange ({page, limit}) {
      this.listQuery.page = page
      this.listQuery.limit = limit
      this._getList()
    },
    onTabChange (name) {
      console.log('tabCHange', name)
      if (name === 'draft') {
        this.formQuery.isDraft = true
        this.formQuery.remburseStatus = ''
      } else {
        this.formQuery.remburseStatus = name
        name === '' 
          ? this.formQuery.isDraft = ''
          : this.formQuery.isDraft = false
      }
      console.log(this.formQuery.remburseStatus, 'status')
      this.listQuery.page = 1
      this._getList()
    },
    onChangeForm (val) {
      this.currentFormQuery = val
      Object.assign(this.listQuery, val)
    },
    onSearch () {
      this._getList()
    },
    openTree (row) { // 打开详情
      console.log(row, 'row')
    },
    addAccount () { // 添加
      getOrder().then(res => {
        console.log(res, 'res')
        let data = res.data
        if (data && data.length) {
          let { 
            userId, 
            userName, 
            orgName, 
            becity, 
            businessTripDate, 
            destination, 
            endDate 
          } = data[0]
          this.customerInfo = {
            createUserId: userId,
            userName,
            orgName,
            becity,
            businessTripDate,
            endDate,
            destination
          }
          this.title = 'create'
          this.$refs.myDialog.open()
        }
      }).catch(() => {
        this.$message.error('获取失败')
      })
    },
    edit () {
      if (!this.currentRow) {
        return this.$message({
          type: 'warning',
          message: '请先选择报销单'
        })
      }
      getDetails({
        reimburseInfoId: this.currentRow.id
      }).then(res => {
        console.log(res, 'detail')
        let { reimburseResp } = res.data
        delete res.data.reimburseResp
        this.detailData = Object.assign({}, res.data, { ...reimburseResp })
        try {
          this._normalizeDetail(this.detailData)
        } catch (err) {
          console.log(err, 'err')
        }
        
        this.title = 'edit'
        console.log(this.detailData, 'detialData')
        this.$refs.myDialog.open()
      }).catch(() => {
        this.$message.error('获取详情失败')
      })
    },
    _normalizeDetail (data) {
      let { 
        reimburseAttachments,
        // reimburseTravellingAllowances,
        reimburseFares,
        reimburseAccommodationSubsidies,
        reimburseOtherCharges 
      } = data
      data.attachmentsFileList = reimburseAttachments
        .map(item => {
          item.name = item.attachmentName
          item.url = `${this.baseURL}/${item.fileId}?X-Token=${this.tokenValue}`
          return item
        })
      data.reimburseAttachments = []
      // this._buildAttachment(reimburseTravellingAllowances)
      this._buildAttachment(reimburseFares)
      this._buildAttachment(reimburseAccommodationSubsidies)
      this._buildAttachment(reimburseOtherCharges)
    },
    _buildAttachment (data) { // 为了回显，并且编辑 目标是为了保证跟order.vue的数据保持相同的逻辑
      data.forEach(item => {
        let { reimburseAttachments } = item
        console.log(reimburseAttachments, 'foeEach', item)
        item.invoiceFileList = this.getTargetAttachment(reimburseAttachments, 2)
        item.otherFileList = this.getTargetAttachment(reimburseAttachments, 1)
        item.invoiceAttachment = [],
        item.otherAttachment = []
        item.reimburseAttachments = []
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
    recall () { // 撤回操作
      console.log('recall')
    },
    async submit () {
      try {
        console.log(this.$refs.order, 'order', this.$refs)
        await this.$refs.order.submit()
        this._getList()
        this.$refs.order.resetInfo()
        this.closeDialog()
      } catch (err) {
        this.$message.error('提交失败')
      }
    }, // 提交
    async saveAsDraft () {
      console.log('草稿')
      try {
        await this.$refs.order.submit(true)
        this._getList()
        this.$refs.order.resetInfo()
        this.closeDialog()
      } catch (err) {
        this.$message.error('存为草稿失败')
      }
    }, // 存为草稿
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