<template>
  <div 
    class="quotation-wrapper" 
    v-loading.fullscreen="loading" 
    :element-loading-text="loadingText"
    element-loading-spinner="el-icon-loading"
    element-loading-background="rgba(0, 0, 0, 0.8)"
  >
    <el-row type="flex" class="title-wrapper">
      <p><span>退库订单</span><span>{{ formData.returnNoteCode || '' }}</span></p>
      <p><span>申请人</span><span>{{ formData.creater }}</span></p>
      <p><span>创建时间</span><span>{{ formData.createTime | formatDateFilter }}</span></p>
      <p><span>销售员</span><span>{{ formData.salMan }}</span></p>
    </el-row>
    <!-- 主题内容 -->
    <el-scrollbar class="scroll-bar">
      <common-form
        :model="formData"
        :formItems="formItems"
        ref="form"
        class="my-form-wrapper"
        :class="{ 'my-form-view': !isCreated }"
        label-width="60px"
        :disabled="!isCreated"
        label-position="right"
        :show-message="false"
        :isCustomerEnd="true"
      >
      </common-form>
      <div class="divider"></div>
      <div class="courier-wrapper">
        <el-row class="title" type="flex" justify="space-between" align="middle">
          <el-row type="felx" align="middle">
            <span class="line"></span>
            <span class="text">物流信息</span>
          </el-row>
          <el-button 
            v-if="isShowAddBtn"
            class="add-btn" 
            type="primary" 
            size="mini" 
            @click="addExpressInfo"
          >新增</el-button>
        </el-row>
        <!-- 物流表格 -->
        <common-table
          class="courier-table table-wrapper"
          :class="{ 'courier-table': orderType === 'returnOrder' }"
          ref="courierTable"
          :data="expressList" 
          :columns="expressColumns"
          max-height="150px"
          @row-click="onExpressRowClick"
        >
          <template v-slot:operation="{ row }">
            <span class="picture" :class="{ 'finished': row.status === 1 }" @click="addExpressInfo">{{ formatOperationText(row) }}</span>
          </template>
          <template v-slot:expressInformation="{ row }">
            <el-row v-infotooltip.top-start.ellipsis>
              <img :src="rightImg" @click="_getExpressInfo(row)" class="search-icon">
              <span>{{ row.expressInformation }}</span>
            </el-row>
          </template>
          <template v-slot:freight="{ row }">
            <span style="text-align: right"></span>{{ row.freight | toThousands }}
          </template>
          <template v-slot:upload="{ row }">
            <span class="picture"  @click="showExpressPicture(row)">查看</span>
          </template>
        </common-table>
        <el-row type="flex" class="freight" :class="{ 'quality': orderType === 'quality', 'storage': orderType === 'storage' }" justify="end">
          <span class="title">总计</span>
          <p v-infotooltip.ellipsis.top-start class="money">￥{{ freightTotal | toThousands }}</p>
        </el-row>
      </div>
      <div class="divider"></div>
      <!-- 物料表格 -->
      <div class="material-wrapper">
        <el-row type="flex" class="title" align="middle">
          <i class="line"></i>
          <span class="text">物料信息</span>
        </el-row>
        <common-table 
          class="table-wrapper"
          ref="materialTable"
          :data="materialList"
          max-height="250px"
          :columns="materialColumns"
        >
          <!-- 查看图片 -->
          <template v-slot:upload="{ row }">
            <span class="picture" @click="showMaterialPicture(row)">查看</span>
          </template>
          <!-- 入库模块 -->
          <template v-slot:toPutGood="{ row }">
            <span :class="{ 'picture': !row.isGoodFinish }" @click="_storageMaterial(row, true)">{{ row.isGoodFinish ? 37 : (row.goodQty ? '入库' : '') }}</span>
          </template>
          <template v-slot:toPutBad="{ row }">
            <span :class="{ 'picture': !row.isSecondFinish }" @click="_storageMaterial(row, false)">{{ row.isSecondFinish ? 39 : (row.secondQty ? '入库' : '') }}</span>
          </template>
        </common-table>
        <el-row type="flex" justify="end" v-if="orderType === 'storage' && (!ifPutAllGood || !ifPutAllSecond)" class="storage-btn-wrapper">
          <span class="picture good" @click="_storageMaterial(expressList, true)" v-if="!ifPutAllGood">一键入库</span>
          <span class="picture second" @click="_storageMaterial(expressList, false)" v-if="!ifPutAllSecond">一键入库</span>
        </el-row>
      </div>
    </el-scrollbar>
    <!-- 退料列表搜索 -->
    <my-dialog 
      ref="orderListDialog"
      title="退料服务单"
      width="700px"
      :append-to-body="true"
      :btnList="orderBtnList"
      @closed="closeOrderDialog"
      :destroy-on-close="true"
    >
      <common-table
        ref="orderTable"
        height="400px"
        :data="orderList"
        :columns="orderColumns"
        radioKey="u_SAP_ID"
      >
      </common-table>
      <pagination
        v-show="orderTotal > 0"
        :total="orderTotal"
        :page.sync="listQueryOrder.page"
        :limit.sync="listQueryOrder.limit"
        @pagination="handleOrderCurrentChange"
      />
    </my-dialog>
    <!-- 新增快递弹窗 -->
    <my-dialog
      v-loading="expressDialogLoading"
      ref="expressInfoDialog"
      :title="dialogTitle"
      width="800px"
      top="10%"
      :append-to-body="true"
      :btnList="btnList"
      @closed="closeExpressDialog"
      @opened="onExpressInfoOpen"
      :destroy-on-close="true"
    >
      <AddExpressInfo 
        ref="addExpressInfo" 
        :formData="formData" 
        @addExpressInfo="onAddExpressInfo" 
        :orderType="orderType" 
        :expressList="expressList" />
    </my-dialog>
    <!-- 只能查看的表单 -->
    <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
      :append-to-body="true"
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
    <el-image-viewer
      v-if="previewVisible"
      :zIndex="99999"
      :url-list="previewImageUrlList"
      :on-close="closeViewer"
    >
    </el-image-viewer>
  </div>
</template>

<script>
import { getExpressInfo } from '@/api/material/quotation'
import { getServiceOrderInfo, storageMaterial } from '@/api/material/returnMaterial'
import { chatMixin, rolesMixin } from '../../common/js/mixins'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import elDragDialog from "@/directive/el-dragDialog";
import AddExpressInfo from './AddExpressInfo'
import { processDownloadUrl } from '@/utils/file'
import { findIndex, accAdd } from '@/utils/process'
import { formatDate } from '@/utils/date'
// import { toThousands } from '@/utils/format'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import rightImg from '@/assets/table/right.png'
import { 
  EXPRESS_COLUMNS_MAP,
  TABLE_COLUMNS_MAP,
  // STORAGE_COLUMNS_MAP,
  SHOW_STORAGE_COLUMNS_TO_PUT,
  QUALITY_TEXT_MAP,
  STORAGE_TEXT_MAP
} from '../js/config'
const LOADING_TEXT_MAP = {
  returnOrder: '退料中',
  quality: '检测中',
  storage: '入库中'
}
export default {
  mixins: [chatMixin, rolesMixin],
  // provide () {
  //   return {
  //     parentVm: this
  //   }
  // },
  directives: {
    elDragDialog
  },
  components: {
    zxform,
    zxchat,
    AddExpressInfo,
    ElImageViewer
  },
  props: {
    isCreated: {
      type: Boolean
    },
    orderType: {
      type: String,
      defualt: 'returnOrder'
    },
    detailInfo: {
      type: Object,
      default: () => {}
    },
    categoryList: Array
  },
  filters: {
    formatDateFilter (val) {
      return val ? formatDate(val, 'YYYY.MM.DD HH:mm:ss') : ''
    }
  },
  watch: {
    expressList: {
      deep: true,
      handler (val) {
        console.log(val, 'expressList')
      }
    },
    detailInfo: {
      immediate: true,
      handler (val) {
        if (val) {
          Object.assign(this.formData, val.mainInfo)
          this.currentIndex = 0
          this.expressList = val.expressList
          console.log(val, this.expressList, 'detail info')
        }
      }
    }
  },
  computed: {
    loadingText () {
      return LOADING_TEXT_MAP[this.orderType]
    },
    isShowAddBtn () {
      return this.orderType === 'returnOrder' && !this.formData.isLast
    },
    ifPutAllGood () { // 良品是否全部入库
      return this.expressList.length ? this.expressList[0].materialList.every(item => {
        const { isGoodFinish, goodQty } = item
        return +isGoodFinish || !+goodQty
      }) : false
    },
    ifPutAllSecond () { // 次品是否全部入库
      return this.expressList.length ? this.expressList[0].materialList.every(item => {
        const { isSecondFinish, secondQty } = item
        return +isSecondFinish || !+secondQty
      }) : false
    },
    dialogTitle () {
      return this.orderType === 'returnOrder' ? '新增快递信息' : '待检物料'
    },
    title () {
      return this.formData.acquisitionWay === '1' ? '自提' : '快递'
    },
    descriptionTitle () {
      return this.formData.acquisitionWay === '1' ? '物料状态' : '物流信息'
    },
    expressColumns () {
      return  EXPRESS_COLUMNS_MAP[this.orderType]
    },
    materialList () {
      console.log(this.currentIndex, this.expressList, 'computed')
      if (this.expressList[this.currentIndex]) {
        return this.expressList[this.currentIndex].materialList
      }
      return []
    },
    freightTotal () { // 总运费
      if (this.expressList && this.expressList.length) {
        return this.expressList.reduce((prev, expressInfo) => {
          return accAdd(prev, expressInfo.freight)
        }, 0)
      }
      return 0
    },
    materialColumns () {
      return TABLE_COLUMNS_MAP[this.orderType] ? TABLE_COLUMNS_MAP[this.orderType] : SHOW_STORAGE_COLUMNS_TO_PUT
    }
  },
  data () {
    return {
      currentRow: {},
      currentIndex: 0,
      loading: false,
      isOutbound: true,
      fileList: [],
      rightImg,
      pictureList: [], // 快递图片列表
      addVisible: false,
      formData: {
        returnNoteCode: '', // 退库单号
        salesOrderId: '', // 销售单号
        serviceSapId: '', // NSAP ID
        serviceOrderId: '', 
        creater: '',
        u_SAP_ID: '',
        customerName: '', // 客户名称
        customerCode: '', // 客户代码
        salMan: '', // 销售员
        contacterTel: '', // 电话
        contacter: '', // 联系人
        isLast: 0 // 判断是不是最后一次退料
      }, 
      formItems: [
        { tag: 'text', span: 3, attrs: { prop: 'serviceSapId', readonly: true }, itemAttrs: { prop: 'u_SAP_ID', label: '服务ID' }, on: { focus: this.onServiceIdFocus }},
        { tag: 'text', span: 6, attrs: { prop: 'customerCode', disabled: true }, itemAttrs: { prop: 'customerCode', label: '客户代码' } },
        { tag: 'text', span: 6, attrs: { prop: 'customerName', disabled: true }, itemAttrs: { prop: 'customerName', label: '客户名称' } },
        { tag: 'text', span: 3, attrs: { prop: 'contacter', disabled: true }, itemAttrs: { prop: 'contacter', label: '联系人',  } },
        { tag: 'text', span: 6, attrs: { prop: 'contacterTel', disabled: true }, itemAttrs: { prop: 'contacterTel', label: '电话' } },
      ],
      rules: {
        // 'u_SAP_ID': [{ required: true }],
        serviceOrderId: [{ required: true }],
        serviceSapId: [{ required: true }],
        customerCode: [{ required: true }],
        customerName: [{ required: true }],
        contacter: [{ required: true }],
        contactTel: [{ required: true }]
      },
      //物料表格
      materialRules: {
        delivery: [{ required: true, trigger: ['change', 'blur'] }]
      },
      // 新增快递弹窗
      expressDialogLoading: false,
      // 物流表格
      expressLoading: false,
      expressList: [],
      expressRules: {
        expressNumber: [{ required: true, trigger: ['change', 'blur'] }],
        // expressInformation: [{ required: true }]
      },
      // 物料表格
      materialLoading: false,
      btnList: [
        { btnText: '提交', handleClick: this.submitExpressInfo },
        { btnText: '关闭', class: 'close', handleClick: this.closeExpressDialog }
      ],
      // 退料服务单列表
      orderList: [],
      orderColumns: [
        { label: '#', type: 'radio', width: 50 },
        { label: '服务Id', prop: 'u_SAP_ID' },
        { label: '客户代码', prop: 'customerId' },
        { label: '客户名称', prop: 'customerName' },
        { label: '联系人', prop: 'contacter' },
        { label: '联系电话', prop: 'contactTel' }
      ],
      orderTotal: 0,
      listQueryOrder: {
        page: 1,
        limit: 20
      },
      orderBtnList: [
        { btnText: '确定', handleClick: this.confirmOrder }
      ],
      previewVisible: false,
      previewImageUrlList: [], // 用来展示图片
      hasBeenStorage: false // 判断是否调用过入库接口
    }
  },
  methods: {
    async onServiceIdFocus (prop) {
      console.log(prop)
      if (this.expressList.length) {
        return
      }
      try {  
        const { data, count } = await getServiceOrderInfo(this.listQueryOrder)
        this.orderList = data
        this.orderTotal = count
        this.$refs.orderListDialog.open()
      } catch (err) {
        this.orderList = []
        this.orderTotal = 0
        this.$message.error(err)
      }
    },
    formatOperationText (row) {
      const { status = 0 } = row
      const text = this.orderType === 'quality' ? QUALITY_TEXT_MAP[status] : STORAGE_TEXT_MAP[status]
      return text
    },
    _openServiceOrder () {
      console.log(this.formData.serviceOrderId, this.formData)
      this.openServiceOrder(this.formData.serviceOrderId, () => this.loading = true, () => this.loading = false)
    },
    closeViewer () {
      this.previewVisible = false
    },
    addExpressInfo () { // 增加快递
      if (this.orderType === 'returnOrder') { // 退料
        if (!this.formData.serviceOrderId) {
          return this.$message.warning('请先选择服务单')
        }
      }
      this.$refs.expressInfoDialog.open()
    },
    showMaterialPicture (row) {
      console.log(row, 'row')
      let { pictureId } = row
      if (!pictureId) {
        return this.$message.warning('暂无快递图片')
      }
      let pictureList = [processDownloadUrl(pictureId)]
      this.previewImageUrlList = pictureList
      this.previewVisible = true
    },
    handleOrderCurrentChange (val) {
      Object.assign(this.listQueryOrder, val)
    },
    confirmOrder () { // 选定需要退料的服务单
      console.log('confirm order')
      const currentRow = this.$refs.orderTable.getCurrentRow()
      if (!currentRow) {
        return this.$message.warning('请先选择数据')
      }
      const { id, contactTel, contacter, customerId, customerName, u_SAP_ID } = currentRow
      this.formData.serviceSapId = u_SAP_ID
      this.formData.serviceOrderId = id 
      this.formData.contacterTel = contactTel
      this.formData.contacter = contacter
      this.formData.customerCode = customerId
      this.formData.customerName = customerName
      this.$refs.orderListDialog.close()
      this.listQueryOrder = { page: 1, limit: 20 }
    },
    closeOrderDialog () {
      this.listQueryOrder = {
        page: 1,
        limit: 20
      }
    },
    openedFn () { // 打开弹窗后 默认展示第一条物流信息
      if (this.expressList && this.expressList.length) {
        this.$nextTick(() => {
          console.log('setCurrent parent mounted')
          this.$refs.courierTable.setCurrentRow(this.expressList[0])
        })
      }
    },
    onExpressRowClick (row) {
      this.currentIndex = this.expressList.findIndex(item => item === row)
      // this.currentIndex = findIndex(this.expressList, item => item.id === row.id)
      console.log(this.currentIndex, 'rowclick', findIndex)
    },
    showExpressPicture (row) { // 图片
      let { expressagePicture } = row
      let pictureList = expressagePicture.map(item => processDownloadUrl(item.pictureId))
      if (!pictureList.length) {
        return this.$message.warning('暂无快递图片')
      }
      this.previewImageUrlList = pictureList
      this.previewVisible = true
    },
    onAddExpressInfo (expressages) { // 监听到提交成功之后
      this.orderType === 'returnOrder' ? this.showReturnList(expressages) : this.showQualityList(expressages)
      this.closeExpressDialog()
    },
    showReturnList (expressages) {
      const { trackNumber: expressNumber, freight, returnMaterialDetail, expressPictureIds: list, isLastReturn } = expressages
      this.formData.isLast = Number(isLastReturn)
      let expressagePicture = list.map(pictureId => ({ pictureId }))
      const materialList = returnMaterialDetail.map(item => {
        const { materialCode, materialDescription, pictureId, returnQty: count, totalQty: totalCount, surplusQty } = item
        return {
          materialCode,
          materialDescription,
          pictureId,
          count,
          totalCount,
          surplusQty
        }
      })
      this.expressList.unshift({
        expressNumber,
        freight,
        expressagePicture,
        expressInformation: '',
        materialList
      })
      // this.expressList = expressages
      // this.formData.quotationStatus = start
      // 每次都添加完默认展示最新的物料数据
      this.currentIndex = 0
      this.$refs.courierTable.setCurrentRow(this.expressList[this.currentIndex])
      console.log(this.currentIndex, this.expressList, 'expressList', 'currentIndex')
    },
    showQualityList (expressages) {
      const { checkOutMaterials } = expressages
      const materialList = this.expressList[0].materialList
      checkOutMaterials.forEach((item, index) => {
        const { goodQty, secondQty } = item
        materialList[index].goodQty = goodQty
        materialList[index].secondQty = secondQty
      })
    },
    async _storageMaterial (row, isGood) {
      const key = isGood ? 'isGoodFinish' : 'isSecondFinish'
      const text = isGood ? '良品' : '次品'
      const whsCode = isGood ? 37 : 39
      const isPutWidthOne = Array.isArray(row) // 是否一键入库
      const expressageId = this.expressList[0].id
      const returnNoteId = this.formData.returnNoteCode
      const params = {
        expressageId,
        returnNoteId
      }
      if (isPutWidthOne) { // 一键入库
        const materialList = this.expressList[0].materialList
        const storageList = materialList.filter(item => {
          const { goodQty, secondQty } = item
          const count = isGood ? goodQty : secondQty
          return count >= 1 && !(+item[key])
        }) // 找到列表中没有入库的单
        console.log(storageList, 'storageList')
        
        if (!storageList.length) {
          return this.$message.warning(`没有可以入库的${text}`)
        }
        const putInMaterials = storageList.map(item => {
          const { id, materialId, goodQty, secondQty } = item
          const count = isGood ? goodQty : secondQty
          return {
            id,
            materialId,
            whsCode,
            qty: count
          }
        })
        params.putInMaterials = putInMaterials
        console.log(storageList, params)
      } else {
        const { isGoodFinish, isSecondFinish, id, materialId, goodQty, secondQty } = row
        const hasStoraged = isGood ? isGoodFinish : isSecondFinish
        const count = isGood ? goodQty : secondQty
        if (hasStoraged) {
          return this.$message.warning(`当前${text}已经入库`)
        }
        if (!count) {
          return this.$message.warning(`没有可以入库的${text}`)
        }
        const putInMaterials = [{
          id,
          materialId,
          whsCode,
          qty: count
        }]
        params.putInMaterials = putInMaterials
        console.log(params, 'params')
      }
      this.loading = true
      try {
        await storageMaterial(params)
        if (isPutWidthOne) {
          this.expressList[0].materialList.forEach(item => {
            const { goodQty, secondQty } = item
            const count = isGood ? goodQty : secondQty
            if (count) {
              item[key] = 1
            }
          })
        } else {
          row[key] = 1
        }
        if (!this.hasBeenStorage) {
          this.hasBeenStorage = true
        }
      } catch (err) {
        this.$message.error(err.message)
      } finally {
        this.loading = false
      }
      console.log(row, isGood, storageMaterial, 'storageMaterial')
    },
    onExpressInfoOpen () {
      this.$refs.addExpressInfo.onOpened()
    },
    submitExpressInfo () {
      this.$refs.addExpressInfo.submit()
    },
    closeExpressDialog () {
      this.$refs.expressInfoDialog.close()
      this.$refs.addExpressInfo.close()
    },
    _getExpressInfo (data) { // 查询物流信息
      console.log(data)
      let { expressNumber } = data
      if (expressNumber === '') {
        return this.$message.error('快递单号为空，无法查询')
      }
      // YT4851790722587
      getExpressInfo({ trackNumber: expressNumber.trim() }).then(res => {
        console.log(res, 'res')
        let expressList = JSON.parse(res.data).data
        data.expressInformation = expressList[expressList.length - 1].context
      }).catch(err => {
        // data.expressInformation = ''
        this.$message.error(err ? err.message : '查询物流信息失败')
      })
    },
    resetFile () { // 清空文件列表
      if (this.$refs.uploadFile) {
        this.$refs.uploadFile.clearFiles()
      }
    },
    resetInfo () {
      this.resetFile()
      this.reset()
    },
    _normalizeExpressList (list) { // 格式化物流列表
      list.forEach(item => {
        item.expressNumber = item.expressNumber.trim()
      })
    }
  },
  created () {

  },
  mounted () {
    console.log('outbound mounted')
  },
}
</script>
<style lang='scss' scoped>
.quotation-wrapper {
  position: relative;
  font-size: 12px;
  .divider {
    height: 1px;
    margin: 15px auto;
    background: #E6E6E6;
  }
  ::v-deep .el-form-item--mini.el-form-item, .el-form-item--small.el-form-item {
    margin-bottom: 5px;
  }
  ::v-deep .el-form-item__label {
    padding-right: 4px;
  }
  ::v-deep .el-input.is-disabled .el-input__inner {
    background-color: #fff;
    cursor: default;
    color: #606266;
    border-color: #DCDFE6;
  }
  ::v-deep .el-textarea.is-disabled .el-textarea__inner {
    background-color: #fff;
    cursor: default;
    color: #606266;
    border-color: #DCDFE6;
  }
  /* 附件样式 */
  ::v-deep .el-upload-list__item {
    margin-top: 0;
  }
  /* 表头文案 */
  > .title-wrapper { 
    position: absolute;
    top: -51px;
    left: 113px;
    height: 40px;
    line-height: 40px;
    p { 
      margin-right: 10px;
      font-size: 12px;
      span:nth-child(1) {
        color: #BFBFBF;
        margin-right: 10px;
      }
      span:nth-child(2) {
        color: #222222;
      }
    }
  }
  /* 表单表格内容 */
  .scroll-bar {
    &.el-scrollbar {
       ::v-deep {
        .el-scrollbar__wrap {
          max-height: 600px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
    .my-form-wrapper {
      ::v-deep {
        .el-date-editor.el-input {
          input {
            padding: 0 5px !important;
          }
          span {
            display: none;
          }
        }
        .el-input-number input {
          text-align: left;
        }
      }
    }
    .courier-wrapper, .material-wrapper, .material-content-wrapper {
      .table-wrapper {
        margin-top: 10px;
      }
      .title {
        .line {
          display: inline-block;
          width: 4px;
          height: 18px;
          margin-right: 8px;
          background: #F8B500;

        }
        .text {
          font-size: 18px;
          font-weight: 800;
          color: #222222;
        }
      }
    }
    /* 物流表格 */
    .courier-wrapper {
      // display: flex;
      margin-top: 10px;
      > .title {
        // width: 522px;
      }
      .courier-table {
        // width: 522px;
        .search-icon {
          cursor: pointer;
        }
        .picture {
          color: rgba(247, 195, 56, 1);
          cursor: pointer;
        }
        .finished {
          color: #222;
        }
      }
      .add-btn {
        width: 64px;
        color: #fff;
        background: #F8B500;
        border-color: transparent;
        border-radius: 4px;
      }
      /* 总运费 */
      .freight {
        margin-right: 70px;
        &.quality {
          margin-right: 120px;
        } 
        &.storage {
          margin-right: 70px;
        }
        font-weight: bold;
        .title {
          margin-right: 24px;
        }
        .money {
          max-width: 350px;
        }
      }
    }
    /* 物料表格 */
    .material-wrapper {
      margin-top: 20px;
      .storage-btn-wrapper {
        position: relative;
        height: 20px;
        margin: 5px 0 10px 0;
      }
      .picture {
        color: rgba(247, 195, 56, 1);
        cursor: pointer;
        &.good {
          position: absolute;
          right: 210px;
        }
        &.second {
          position: absolute;
          right: 30px;
        }
      }
    }
  }
}
</style>