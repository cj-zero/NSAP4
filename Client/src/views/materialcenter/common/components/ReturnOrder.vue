<template>
  <div class="quotation-wrapper">
    <el-row type="flex" class="title-wrapper">
      <p class="bold id">退料单号: <span>{{ formData.returnNoteCode || '' }}</span></p>
      <p class="bold">服务ID： {{ formData.serviceOrderSapId }}</p>
      <p class="bold">申请人: <span>{{ formData.createUser }}</span></p>
      <p>销售员: <span>{{ formData.salMan }}</span></p>
      <p>创建时间: <span>{{ formData.createTime }}</span></p>
    </el-row>
    <!-- 主题内容 -->
    <el-scrollbar class="scroll-bar">
      <el-form
        :model="formData"
        ref="form"
        class="my-form-wrapper" 
        label-width="80px"
        size="mini"
        label-position="right"
        :show-message="false"
      >
        <!-- 普通控件 -->
        <el-row 
          type="flex" 
          v-for="(config, index) in formatReturnConfig"
          :key="index">
          <el-col 
            :span="item.col"
            v-for="item in config"
            :key="item.prop"
          >
            <el-form-item
              :prop="item.prop"
              :rules="rules[item.prop] || { required: false }">
              <span slot="label">
                <template v-if="item.prop === 'serviceOrderSapId'">
                  <div class="link-container" style="display: inline-block">
                    <span>{{ item.label }}</span>
                    <img :src="rightImg" @click="openServiceOrder" class="pointer">
                  </div>
                </template>
                <template v-else>
                  <span>{{ item.label }}</span>
                </template>
              </span>
              <template v-if="!item.type">
                <el-input 
                  v-model="formData[item.prop]" 
                  :style="{ width: item.width + 'px' }"
                  :maxlength="item.maxlength"
                  :disabled="item.disabled"
                  :readonly="item.readonly"
                  @focus="onServiceIdFocus(item.prop)"
                  >
                  <i :class="item.icon" v-if="item.icon"></i>
                </el-input>
              </template>
              <template v-else-if="item.type === 'select'">
                <el-select 
                  clearable
                  :style="{ width: item.width }"
                  v-model="formData[item.prop]" 
                  :placeholder="item.placeholder"
                  :disabled="item.disabled">
                  <el-option
                    v-for="(item,index) in item.options"
                    :key="index"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </template>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <div class="courier-wrapper">
        <!-- 物流表格 -->
        <div class="courier-table-wrapper">
          <common-table
            class="courier-table"
            ref="courierTable"
            :data="courierList" 
            :columns="courierColumns"
            :height="0"
            max-height="150px"
          >
            <!-- <template v-slot:courierNumber="{ row }">
              <el-input size="mini" v-model="courierList[row.index].number"></el-input>
            </template> -->
            <!-- 物流信息 -->
            <!-- <template v-slot:logisticsInfo="{ row }">
              {{ courierList[row.index].info }}
            </template> -->
            <!-- 备注 -->
            <!-- <template v-slot:remark="{ row }">
              <el-input size="mini" v-model="courierList[row.index].remark"></el-input>
            </template> -->
            <!-- 图片信息 -->
            <template v-slot:expressInformation="{ row }">
              <el-row type="flex" align="middle">
                <img style="width: 12px;height: 12px;" :src="rightImg" @click="getExpressInformation(row)" class="pointer">
                <span>{{ courierList[row.index].expressInformation }}</span>
              </el-row>
            </template>
            <!-- <template v-slot:pictures>
              <UpLoadFile uploadType="file" :limit="3" :disabled="true" />
            </template>  -->
          </common-table>
        </div>
        
      </div>
      <!-- 物料表格 -->
      <div class="material-wrapper">
        <el-form 
          ref="materialForm"
          :model="materialFormData" 
          :show-message="false"
          size="mini">
          <common-table 
            ref="materialTable"
            :data="materialFormData.materialList"
            maxHeight="250px" 
            :show-message="false"
            :columns="materialColumns"
            :cellStyle="cellStyle"
            :rowStyle="rowStyle">
            <template v-slot:check="{ row }">
              <el-button 
                v-if="(status === 'view' && checkList[row.index].isPass !== 2) || status === 'toReturn'" 
                :disabled="status === 'view'"
                type="success" 
                size="mini" 
                @click="check(1, row.index)">通过</el-button>
              <el-button 
                :disabled="status === 'view'"
                v-if="(status === 'view' && checkList[row.index].isPass !== 1) || status === 'toReturn'"
                :type="checkList[row.index].isPass === 2 ? 'info' : 'danger'" 
                size="mini" 
                @click="check(2, row.index)">未通过</el-button>
            </template>
            <template v-slot:wrongCount="{ row }">
              <el-form-item 
                :prop="'materialList.' + row.index + '.' + row.prop"
                :rules="{ required: checkList[row.index].isPass === 2 }">
                <el-input-number 
                  :disabled="status === 'view'"
                  size="mini"
                  :controls="false"
                  v-model="materialList[row.index].wrongCount"
                  :mini="0"
                  :max="materialList[row.index].totalCount - materialList[row.index].count"
                >
                </el-input-number>
              </el-form-item>
            </template>  
            <template v-slot:receiveRemark="{ row }">
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
            </template>
            <template v-slot:pictures="{ row }">
              <el-button size="mini" class="customer-btn-class" @click="previewPicture(row.pictureId)">查看</el-button>
            </template>
          </common-table>
        </el-form>
        
      </div>
    </el-scrollbar>
     <!-- 只能查看的表单 -->
    <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
      :appendToBody="true"
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
      :url-list="[dialogImageUrl]"
      :on-close="closeViewer"
    >
    </el-image-viewer>
  </div>
</template>


<script>
import { saveReceiveInfo, accraditate, getExpressInfo } from '@/api/material/returnMaterial'
import { configMixin, chatMixin } from '../js/mixins'
import CommonTable from '@/components/CommonTable' // 对于不可编辑的表格
// import MyDialog from '@/components/Dialog'
// import UpLoadFile from '@/components/upLoadFile'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import rightImg from '@/assets/table/right.png'
import { processDownloadUrl } from '@/utils/file'
export default {
  mixins: [configMixin, chatMixin],
  components: {
    CommonTable,
    // MyDialog,
    // UpLoadFile,
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
        let { expressList, returnNoteList, mainInfo } = this.detailInfo
        let { serviceSapId, customerCode, customerName, creater } = mainInfo
        Object.assign(this.formData, mainInfo)
        this.formData.serviceOrderSapId = serviceSapId
        this.formData.terminalCustomer = customerName
        this.formData.terminalCustomerId = customerCode
        this.formData.createUser = creater
        this.courierList = this._normalizeExpressList(expressList)
        this.materialList = returnNoteList
        // 初始化 验收列表
        this.checkList = this.materialList.map(item => {
          let { id, check: isPass, wrongCount, receivingRemark: receiveRemark } = item
          return {
            id,
            isPass, // check: 0 未处理 1 通过 2 未通过
            wrongCount,
            receiveRemark
          }
        })
        console.log(this.courierList, this.materialList, this.checkList)
      }
    }
  },
  computed: {
    materialFormData () {
      return {
        materialList: this.materialList
      }
    },
    materialColumns () {
      let config = [
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '本次退还数量', prop: 'count' },
        { label: '需退总计', prop: 'totalCount' },
        { label: '图片', type: 'slot', slotName: 'pictures' },
        { label: '发货备注', prop: 'shippingRemark' },
        { label: '核对验收', type: 'slot', slotName: 'check', width: '150px' }
      ]
      return this.isReturn 
        ?  config 
        : config.concat([
          { label: '差错数量', prop: 'wrongCount', type: 'slot', slotName: 'wrongCount' },
          { label: '收货备注', prop: 'receivingRemark', type: 'slot', slotName: 'receiveRemark' }
        ])
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
      }, // 表单数据
      rules: {},
      // 物流表格
      courierList: [],
      courierColumns: [
        { label: '快递单号', prop: 'expressNumber', width: '100px' },
        { label: '物流信息', prop: 'expressInformation', type: 'slot', slotName: 'expressInformation' },
        // { label: '备注', type: 'slot', slotName: 'remark', prop: 'remark', width: '150px' },
        // { label: '图片', type: 'slot', slotName: 'pictures', prop: 'pictures', width: '100px' }
      ],
      // 物料表格
      materialList: [],
      checkList: [] // 验收收货记录列表
    }
  },
  methods: {
    previewPicture (pictureId) {
      this.dialogImageUrl = processDownloadUrl(pictureId)
      this.dialogVisible = true
    },
    closeViewer () {
      this.dialogVisible = false
    },
    rowStyle ({ rowIndex }) {
      let isPass = this.checkIsPass(rowIndex)
      console.log('isPass', isPass)
      return isPass 
        ? { backgroundColor: 'rgba(0, 0, 0, .1)' }
        : { }
    },
    cellStyle ({ rowIndex }) {
      let isPass = this.checkIsPass(rowIndex)
      console.log('isPass', isPass)
      return isPass 
        ? { backgroundColor: 'transparent' }
        : { }
    },
    checkIsPass (rowIndex) { // 判断当前行是否已经通过
      return this.checkList.some((item, index) => {
        return item.isPass === 1 && rowIndex === index
      })
    },
    _normalizeExpressList (expressList) { // 格式化物流信息列表
      return expressList.map(item => {
        let infoList = JSON.parse(item.expressInformation).data
        item.expressInformation = infoList[infoList.length - 1].context
        return item
      })
    },
    onReceiveInput (index) {
      this.checkList[index].receiveRemark = this.materialList[index].receivingRemark
    },
    openServiceOrder () {
      this._openServiceOrder()
    },
    check (isValid, index) { // 选择通过或者未通过
      console.log(isValid, index)
      if (this.status === 'view') {
        return
      }
      this.checkList[index].isPass = isValid
      console.log(this.checkList[index].isPass, 'isPass')
    },
    async validate () {
      console.log('validate')
      let isValid
      try {
        isValid = await this.$refs.materialForm.validate()
      } catch (err) {
        isValid = err
      }
      return isValid
    },
    async checkOrSave (isSave) { // isSave 保存还是验收
      if (this.checkList.some(item => item.isPass === 0)) {
        return Promise.reject({ message: '请对所有物料进行校验操作' })
      }
      let isFormValid = await this.validate()
      console.log(isFormValid, 'isFormValid')
      if (!isFormValid) {
        return Promise.reject({ message: '请将表单必填项填写完成' })
      }
      console.log(this.checkList)
      return isSave ? saveReceiveInfo(this.checkList) : accraditate({
        id: this.formData.returnNoteCode,
        returnMaterials: this.checkList
      })
    },
    getExpressInformation (row) {
      console.log(row, 'row')
      if (!row.id) {
        return this.$message.error('无物流Id')
      }
      getExpressInfo({expressageId: row.id }).then(res => {
        console.log(res, 'res')
      })
    },
    resetInfo () {
      this.checkList = []
      this.materialList = []
      this.courierList = []
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.quotation-wrapper {
  position: relative;
  ::v-deep .el-form-item--mini.el-form-item, .el-form-item--small.el-form-item {
    margin-bottom: 5px;
  }
  ::v-deep .el-input-number {
    width: 100%;
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
    top: -48px;
    left: 95px;
    height: 40px;
    line-height: 40px;
    > p {
      min-width: 55px;
      margin-right: 10px;
      &.id {
        color: red;
      }
      &.bold {
        font-weight: bold;
      }
      span {
        font-weight: normal;
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
    /* 物流表格 */
    .courier-wrapper {
      display: flex;
      margin-top: 10px;
      .courier-table-wrapper {
        width: 700px;
        // max-height: 100px;
        margin-right: 20px;
        .courier-table {
          height: auto !important;
        }
      }
      .add-courier-btn {
        color: #fff;
        background-color: rgba(248, 181, 0, 1);
      }
    }
    /* 物料表格 */
    .material-wrapper {
      margin-top: 10px;
    }
  }
}
</style>