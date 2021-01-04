<template>
  <div class="quotation-wrapper" v-loading="loading">
    <el-row type="flex" class="title-wrapper">
      <p class="bold id">出库单号: <span>{{ formData.id || '' }}</span></p>
      <p class="bold">申请人: <span>{{ formData.createUser }}</span></p>
      <p>创建时间: <span>{{ formData.createTime }}</span></p>
      <p>销售员: <span>{{ formData.salesMan }}</span></p>
      <p class="bold id">销售单号：<span>{{ formData.salesOrderId }}</span></p>
    </el-row>
    <!-- 主题内容 -->
    <el-scrollbar class="scroll-bar">
      <el-form
        :model="formData"
        ref="form"
        class="my-form-wrapper" 
        label-width="80px"
        size="mini"
        :disabled="true"
        label-position="right"
        :show-message="false"
      >
        <!-- 普通控件 -->
        <el-row 
          type="flex" 
          v-for="(config, index) in formatFormConfig"
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
                    <img :src="rightImg" @click="_openServiceOrder" class="pointer">
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
        <el-form 
          ref="expressInfo"
          :model="expressListData" 
          :show-message="false"
          size="mini">
        <!-- <el-form :model="courierAllList"> -->
          <div class="courier-table-wrapper">
            <common-table
            class="courier-table"
            ref="courierTable"
            :data="expressListData.list" 
            :columns="expressColumns"
            max-height="150px"
          >
            <template v-slot:expressNumber="{ row, index }">
              <el-form-item
                :prop="'list.' + index + '.' + 'expressNumber'"
                :rules="expressRules['expressNumber']"
              >
                <el-input size="mini" v-model="row.expressNumber" :disabled="isAddExpressInfo(row)" v-infotooltip:200.top-start>></el-input>
              </el-form-item>
            </template>
            <!-- 物流信息 -->
            <template v-slot:expressInformation="{ row, index }">
              <el-form-item
                :prop="'list.' + index + '.' + 'expressInformation'"
                :rules="expressRules['expressInformation']"
              >
                <el-row>
                  <img :src="rightImg" @click="_getExpressInfo(row, index)" class="search-icon">
                  {{ row.expressInformation }}
                </el-row>
              </el-form-item>
            </template>
            <template v-slot:remark="{ row }">
              <el-input size="mini" v-model="row.remark" :disabled="isAddExpressInfo(row)" v-infotooltip:200.top-start></el-input>
            </template>
            <template v-slot:expressagePicture="{ row, index }">
              <UpLoadFile 
                ref="uploadFile" 
                uploadType="file" 
                :limit="3" 
                :ifShowTip="!isAddExpressInfo(row)"
                :onAccept="onAccept"
                :fileList="expressList[index].fileList || []"
                @get-ImgList="getFileList" 
                :options="{ index }" 
                :disabled="isAddExpressInfo(row)"
              />
            </template> 
          </common-table>
          </div>
        </el-form>
        <div>
          <el-button class="add-courier-btn" size="mini" @click="addCourier" v-if="status !== 'view'">新增快递</el-button>
        </div>
      </div>
      <!-- 物料表格 -->
      <div class="material-wrapper">
        <el-form 
          ref="materialForm"
          :model="materialListData" 
          :show-message="false"
          size="mini">
          <common-table 
            ref="materialTable"
            :data="materialListData.list" 
            :columns="materialColumns">
            <template v-slot:delivery="{ row, index }">
              <el-form-item
                :prop="'list.' + index + '.' + 'delivery'"
                :rules="[{ required: !isOutboundAll(row), trigger: ['change', 'blur'] }  ]"
              >
                <el-input-number 
                  size="mini"
                  :controls="false"
                  v-model="row.delivery"
                  :mini="0"
                  :max="row.count - row.sentQuantity"
                  :disabled="isOutboundAll(row)"
                >
                </el-input-number>
              </el-form-item>
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
  </div>
</template>

<script>
import { getExpressInfo, updateOutboundOrder } from '@/api/material/quotation'
import { configMixin, chatMixin, categoryMixin } from '../../common/js/mixins'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
// import Pagination from '@/components/Pagination'
import UpLoadFile from '@/components/upLoadFile'
import rightImg from '@/assets/table/right.png'
import { isImage, processDownloadUrl } from '@/utils/file'
export default {
  mixins: [configMixin, chatMixin, categoryMixin],
  components: {
    // Pagination,
    UpLoadFile,
    zxform,
    zxchat
    // AreaSelector
  },
  props: {
    detailInfo: {
      type: Object,
      default: () => {}
    },
    status: String,
    categoryList: Array
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
        Object.assign(this.formData, val)
        this.expressList = val.expressages.map(item => {
          item.fileList = item.expressagePicture.map(item => {
            item.url = processDownloadUrl(item.pictureId)
            item.name = item.fileName
            return item
          })
          return item
        })
        this.initalExpressList = this.expressList.map(item => item.id) // 添加过的物流信息不能修改
        this.materialList = val.quotationMergeMaterials.map(item => {
          item.sentQuantity = Number(item.sentQuantity)
          return item
        })
        console.log(val, this.expressList, 'detail info')
      }
    }
  },
  computed: {
    courierAllList () {
      return {
        expressList: this.expressList
      }
    },
    materialListData () { // 物料表单数据
      return {
        list: this.materialList
      }
    },
    expressListData () { // 物流表单数据
      return {
        list: this.expressList
      }
    },
    materialColumns () {
      let columns = [
        { label: '序号', type: 'index' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '总数量', prop: 'count', align: 'right' },
        { label: '单位', prop: 'unit', align: 'right' },
        { label: '已出库', prop: 'sentQuantity', align: 'right' }
      ]
      console.log(this.status)
      return this.status === 'view' ? columns : columns.concat([{ label: '出库数量', prop: 'delivery', slotName: 'delivery' }])
    }
  },
  data () {
    return {
      loading: false,
      isOutbound: true,
      fileList: [],
      rightImg,
      formData: {
        // id: '',  报价单号
        salesOrderId: '', // 销售单号
        serviceOrderSapId: '', // NSAP ID
        serviceOrderId: '', 
        createUser: '',
        terminalCustomer: '', // 客户名称
        terminalCustomerId: '', // 客户代码
        shippingAddress: '', // 开票地址
        collectionAddress: '', // 收款地址
        deliveryMethod: '', // 发货方式
        invoiceCompany: '', // 开票方式
        salesMan: '', // 销售员
        totalMoney: 0, // 总金额
        quotationProducts: [] // 报价单产品表
      }, // 表单数据
      rules: {
        serviceOrderSapId: [ { required: true } ],
        terminalCustomerId: [ { required: true } ],
        terminalCustomer: [{ required: true }],
        shippingAddress: [{ required: true, trigger: ['change', 'blur'] }],
        invoiceCompany: [{ required: true, trigger: ['change', 'blur'] }],
        collectionAddress: [{ required: true, trigger: ['change', 'blur'] }],
        deliveryMethod: [{ required: true, trigger: ['change', 'blur'] }]
      }, // 上表单校验规则
      //物料表格
      materialRules: {
        delivery: [{ required: true, trigger: ['change', 'blur'] }]
      },
      // 物流表格
      expressLoading: false,
      expressList: [],
      expressRules: {
        expressNumber: [{ required: true, trigger: ['change', 'blur'] }],
        // expressInformation: [{ required: true }]
      },
      expressColumns: [
        { label: '快递单号', type: 'slot', slotName: 'expressNumber', prop: 'expressNumber', width: '100px' },
        { label: '物流信息', type: 'slot', slotName: 'expressInformation', prop: 'expressInformation' },
        { label: '备注', type: 'slot', slotName: 'remark', prop: 'remark', width: '150px' },
        { label: '图片', type: 'slot', slotName: 'expressagePicture', prop: 'expressagePicture', width: '200px' }
      ],
      // 物料表格
      materialList: []
    }
  },
  methods: {
    onServiceIdFocus (prop) {
      console.log(prop)
    },
    _openServiceOrder () {
      console.log(this.formData.serviceOrderId, this.formData)
      this.openServiceOrder(this.formData.serviceOrderId, () => this.loading = true, () => this.loading = false)
    },
    addCourier () { // 增加快递
      this.expressList.push({
        expressNumber: '',
        expressInformation: '',
        expressagePicture: [],
        fileList: [],
        remark: ''
      })
    },
    isAddExpressInfo (row) {
      let { id } = row
      console.log(row, this.initalExpressList, 'isadd')
      return id ? this.initalExpressList.includes(id) : false
    },
    isOutboundAll (data) { // 判断是否物料的是否已经出料完成
      console.log(data.count, data.sentQuantity, data.count - data.sentQuantity)
      return !(data.count - data.sentQuantity)
    },
    _getExpressInfo (data, index) { // 查询物流信息
      console.log(data)
      if (data.expressNumber === '') {
        return this.$message.error('请先填写快递单号')
      }
      // YT4851790722587
      getExpressInfo({ trackNumber: data.expressNumber.trim() }).then(res => {
        console.log(res, 'res')
        let expressInfoList = JSON.parse(res.data).data
        this.expressList[index].expressInformation = expressInfoList[expressInfoList.length - 1].context
      }).catch(err => {
        this.expressList[index].expressInformation = ''
        this.$message.error(err.message)
      })
    },
    getFileList (value, { index }) {
      this.expressList[index].expressagePicture = value
      console.log(this.expressList[index].expressagePicture, 'fileList')
    },
    onAccept (file) { // 限制发票文件上传的格式
      let { type } = file
      // console.log(size, 'file size')
      let isValid = isImage(type)
      if (!isValid) {
        this.$delay(() => this.$message.error('文件格式只能为图片'))
        return false
      }
      return true
    },
    resetFile () { // 清空文件列表
    console.log(this.$refs.uploadFile, this.$refs.uploadFile.length)
      if (this.$refs.uploadFile) {
        this.$refs.uploadFile.clearFiles()
      }
    },
    resetInfo () {
      this.resetFile()
    },
    _normalizeExpressList (list) { // 格式化物流列表
      list.forEach(item => {
        item.expressNumber = item.expressNumber.trim()
      })
    },
    _normalizeQuotationMergeMaterials (list) {
      list.forEach(item => {
        console.log(item, item.sentQuantity, item.delivery)
        item.sentQuantity += item.delivery || 0
      })
    },
    async updateMaterial () {
      if (!this.expressList.length) {
        return Promise.reject({ message: '至少填写一个快递' })
      }
      let isExpressValid = await this.$refs.expressInfo.validate()
      let isMaterialValid = await this.$refs.materialForm.validate()
      if (isExpressValid && isMaterialValid) {
        this._normalizeExpressList(this.expressList)
        this._normalizeQuotationMergeMaterials(this.materialList)
        let params = {
          id: this.formData.id,
          quotationMergeMaterials: this.materialList,
          expressages: this.expressList
        }
        console.log(params, 'params')
        return updateOutboundOrder(params)
      }
      else {
        return Promise.reject({ message: '请将必填项填写' })
      }
    },
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
        width: 800px;
        // max-height: 100px;
        margin-right: 20px;
        .courier-table {
          height: auto !important;
          .search-icon {
            cursor: pointer;
          }
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