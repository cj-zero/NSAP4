<template>
  <div class="quotation-wrapper" v-loading="returnLoading">
    <el-row type="flex" class="title-wrapper">
      <p><span>退料订单</span><span>{{ formData.returnNoteCode || '' }}</span></p>
      <p><span>服务ID</span>{{ formData.serviceOrderSapId }}</p>
      <p><span>申请人</span><span>{{ formData.createUser }}</span></p>
      <p><span>销售员</span><span>{{ formData.salMan }}</span></p>
      <p><span>创建时间</span><span>{{ formData.createTime }}</span></p>
    </el-row>
    <!-- 主题内容 -->
    <el-scrollbar class="scroll-bar">
      <common-form
        ref="form"
        class="my-form-view"
        :model="formData" 
        label-width="60px" 
        :formItems="formItems" 
        :columnNumber="5"
        :disabled="true"
      ></common-form>
      <div class="divider"></div>
      <div class="courier-wrapper">
        <el-row type="flex" align="middle">
          <i class="line"></i>
          <span class="text">物流信息</span>
        </el-row>
        <!-- 物流表格 -->
        <div class="courier-table-wrapper">
          <common-table
            ref="expressTable"
            :data="expressList" 
            :columns="expressColumns"
            max-height="150px"
            @row-click="onRowClick"
          >
            <!-- 物流信息 -->
            <template v-slot:expressInformation="{ row, index }">
              <el-row type="flex" align="middle">
                <img style="width: 12px;height: 12px;" :src="rightImg" @click="getExpressInformation(row, index)" class="pointer">
                <p v-infotooltip.top.start.ellipsis>{{ row.expressInformation }}</p>
              </el-row>
            </template>
          </common-table>
        </div>
      </div>
      <div class="divider"></div>
      <!-- 物料表格 -->
      <div class="material-wrapper">
        <el-row type="flex" align="middle">
          <i class="line"></i>
          <span class="text">物料信息</span>
        </el-row>  
        <!-- 物料信息 -->
        <common-table
          class="express-table"
          style="margin-top: 10px;" 
          ref="materialTable"
          :data="materialList"
          max-height="250px" 
          :columns="materialColumns"
          :cell-style="cellStyle"
          :row-style="rowStyle">
          <!-- 核对设备 -->
          <template v-slot:check="{ row }">
            <el-button 
              class="success-btn btn" 
              size="mini" 
              @click.stop="check(row, 1)">通过</el-button>
            <el-button 
              class="btn"
              :type="row.check === 2 ? 'info' : 'danger'" 
              size="mini" 
              @click.stop="check(row, 2)">未通过</el-button>
          </template>
          <!-- 差错数量 -->
          <template v-slot:wrongCount="{ row }">
            <el-input-number 
              size="mini"
              :controls="false"
              v-model="row.wrongCount"
              :mini="0"
              :max="row.count"
            >
            </el-input-number>
          </template>  
          <!-- 收货备注 -->
          <!-- <template v-slot:receiveRemark="{ row }">
            <el-form-item 
              :prop="'materialList.' + row.index + '.' + row.prop"
              :rules="{ required: checkList[row.index].isPass === 2 }">
              <el-input 
                :disabled="status === 'view'"
                size="mini"
                @input="onReceiveInput(row.index)"
                v-model="materialList[row.index].receivingRemark"
              >
              </el-input>
            </el-form-item>
          </template> -->
          <!-- 图片 -->
          <template v-slot:pictures="{ row }">
            <el-button 
              v-if="row.pictureId" 
              size="mini" 
              class="customer-btn-class"
              @click="previewPicture(row.pictureId)"
            >查看</el-button>
          </template>
        </common-table>
      </div>
    </el-scrollbar>
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
    <!-- 图片预览 -->
    <el-image-viewer
      v-if="dialogVisible"
      :zIndex="99999"
      :url-list="[dialogImageUrl]"
      :on-close="closeViewer"
    >
    </el-image-viewer>
  </div>
</template>


<script>
import { saveReceiveInfo, accraditate, getExpressInfo } from '@/api/material/returnMaterial'
import { chatMixin } from '../../common/js/mixins'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import rightImg from '@/assets/table/right.png'
import { processDownloadUrl } from '@/utils/file'
import { findIndex } from '@/utils/process'
export default {
  mixins: [chatMixin],
  components: {
    ElImageViewer,
    zxchat,
    zxform
    // AreaSelector
  },
  props: {
    detailInfo: {
      type: Object,
      default: () => {}
    },
    status: {
      type: String
    },
    isReturn: Boolean // 是否已经退料
  },
  watch: {
    detailInfo: {
      immediate: true,
      handler () {
        let { expressList, mainInfo } = this.detailInfo
        let { serviceSapId, customerCode, customerName, creater } = mainInfo
        Object.assign(this.formData, mainInfo)
        this.formData.serviceOrderSapId = serviceSapId
        this.formData.terminalCustomer = customerName
        this.formData.terminalCustomerId = customerCode
        this.formData.createUser = creater
        this.expressList = expressList
        this.currentIndex = 0
      }
    }
  },
  computed: {
    materialFormData () {
      return {
        materialList: this.materialList
      }
    },
    materialList () {
      return this.expressList && this.expressList.length 
        ? this.expressList[this.currentIndex].materialList
        : []
    },
    allMaterialList () {
      let result = []
      if (this.expressList && this.expressList.length) {
        this.expressList.forEach(item => {
          result.push(...item.materialList)
        })
      }
      return [...result]
    }
  },
  data () {
    return {
      dialogVisible: false,
      dialogImageUrl: '',
      rightImg,
      formData: {
        // id: '',  报价单号
        returnMaterial: '', // 退料单号
        serviceOrderSapId: '', // NSAP ID
        serviceOrderId: '', 
        createUser: '', // 创建人
        terminalCustomer: '', // 客户名称
        terminalCustomerId: '', // 客户代码
        salMan: '', // 销售员
        remark: '' // 收货备注
      }, // 表单数据
      rules: {},
      // 物流表格
      expressList: [],
      expressColumns: [
        { label: '快递单号', prop: 'expressNumber', width: '100px' },
        { label: '物流信息', prop: 'expressInformation', slotName: 'expressInformation', 'show-overflow-tooltip': false },
        { label: '退货备注', prop: 'tuihuo', slotName: 'tuihuo' },
        { label: '签收备注', prop: 'qianshou', slotName: '签收'}
        // { label: '图片', type: 'slot', slotName: 'pictures', prop: 'pictures', width: '100px' }
      ],
      currentIndex: 0, // 当前点击的物流信息表格索引
      // 物料表格
      // materialList: [],
      materialColumns: [
        { label: '#', type: 'index' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '本次退还数量', prop: 'count', align: 'right' },
        // { label: '需退总计', prop: 'totalCount', align: 'right' },
        { label: '图片', slotName: 'pictures' },
        { label: '寄回备注', prop: 'shippingRemark' },
        { label: '出错数量', prop: 'wrongCount', slotName: 'wrongCount', align: 'right' },
        { label: '核对验收', slotName: 'check', width: '150px' }
      ],
      returnLoading: false,
      formItems: [
        { tag: 'text', span: 4, attrs: { prop: 'serviceSapId', disabled: true }, itemAttrs: { label: '服务ID' } },
        { tag: 'text', span: 4, attrs: { prop: 'terminalCustomerId', disabled: true }, itemAttrs: { label: '客户代码' } },
        { tag: 'text', span: 8, attrs: { prop: 'terminalCustomer', disabled: true }, itemAttrs: { label: '客户名称' } },
        { tag: 'text', span: 4, attrs: { prop: 'contacter', disabled: true }, itemAttrs: { label: '联系人' } },
        { tag: 'text', span: 4, attrs: { prop: 'contacterTel', disabled: true }, itemAttrs: { label: '电话' } },
      ]
    }
  },
  methods: {
    previewPicture (pictureId) {
      this.dialogImageUrl = processDownloadUrl(pictureId)
      this.dialogVisible = true
    },
    openedFn () {
      if(this.expressList && this.expressList.length) {
        this.$nextTick(() => {
          this.$refs.expressTable.setCurrentRow(this.expressList[0]) // 默认选中第一个
        })
      }
    },
    closeViewer () {
      this.dialogVisible = false
    },
    onRowClick (row) {
      this.currentIndex = findIndex(this.expressList, item => item.id === row.id)
    },
    rowStyle ({ row }) {
      let isPass = this.checkIsPass(row)
      console.log('isPass', isPass)
      return isPass 
        ? { backgroundColor: 'rgba(0, 0, 0, .1)' }
        : { }
    },
    cellStyle ({ row }) {
      let isPass = this.checkIsPass(row)
      console.log('isPass', isPass)
      return isPass 
        ? { backgroundColor: 'transparent' }
        : { }
    },
    checkIsPass (row) { // 判断当前行是否已经通过
      return Number(row.check) === 1
    },
    _openServiceOrder () {
      this.openServiceOrder(this.formData.serviceOrderId, () => this.returnLoading = true, () => this.returnLoading = false)
    },
    check (data, isValid) { // 选择通过或者未通过
      console.log(data, isValid)
      // 点击通过或者未通过的时候，将对应的值加入到this.checkList中
      let index = findIndex(this.checkList, item => item.id === data.id)
      if (index === -1) {
        this.checkList.push(data)
      }
      data.check = isValid
      console.log(data.isPass, 'isPass')
    },
    isMaterialValid (isSave) {
      return isSave 
        ? (this.checkList && this.checkList.length) 
        : this.allMaterialList.every(item => item.check)
    },
    generateReturnMaterials (data) { // 生成退料处理列表
      return data.map(item => {
        let { id, check: isPass, receivingRemark: receiveRemark, wrongCount = 0 } = item
        return {
          id,
          isPass, // check: 0 未处理 1 通过 2 未通过
          wrongCount,
          receiveRemark
        }
      })
    },
    async checkOrSave (isSave) { // isSave 保存还是验收
      let isMaterialValid = this.isMaterialValid(isSave)
      if (!isMaterialValid) {
        return Promise.reject({ message: `${isSave ? '至少对其中一个物料进行操作' : '请对所有物料进行校验操作' }` })
      }
      let returnMaterials = this.generateReturnMaterials(isSave ? this.checkList : this.allMaterialList)
      console.log(this.checkList, returnMaterials, isMaterialValid, 'checked')
      let params = {
        id: this.formData.returnNoteCode,
        returnMaterials,
        remark: this.formData.remark
      }
      console.log(params)
      return isSave ? saveReceiveInfo(params) : accraditate(params)
    },
    getExpressInformation (row) {
      console.log(row, 'row')
      if (!row.id) {
        return this.$message.error('无物流Id')
      }
      getExpressInfo({expressageId: row.id }).then(res => {
        let expressInformation = JSON.parse(res.data).data
        row.expressInformation = expressInformation[expressInformation.length - 1].context
        console.log(expressInformation, row.expressInformation)
      }).catch(err => {
        row.expressInformation = ''
        this.$message.error(err.message)
      })
    },
    resetInfo () {
      this.checkList = []
      this.reset()
    }
  },
  created () {
    this.checkList = [] // 验收收货记录列表
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.quotation-wrapper {
  position: relative;
  .divider {
    height: 1px;
    margin: 15px auto;
    background: #E6E6E6;
  }
  ::v-deep .el-form-item--mini.el-form-item, .el-form-item--small.el-form-item {
    margin-bottom: 5px;
  }
  ::v-deep .el-input-number {
    width: 100%;
    input {
      text-align: right;
    }
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
    left: 114px;
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
  .title-text {
    margin-bottom: 5px;
    font-size: 18px;
    color: #eee;
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
    .courier-wrapper, .material-wrapper {
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
    /* 物流表格 */
    .courier-wrapper {
      // display: flex;
      margin-top: 10px;
      .courier-table-wrapper {
        width: 700px;
        margin-top: 10px;
        // max-height: 100px;
        margin-right: 20px;
      }
      .add-courier-btn {
        color: #fff;
        background-color: rgba(248, 181, 0, 1);
      }
    }
    /* 物料表格 */
    .material-wrapper {
      margin-top: 10px;
      .btn {
        color: #fff;
        &.success-btn {
          background-color: rgba(248, 181, 0, 1);
        }
      }
    }
  }
}
</style>