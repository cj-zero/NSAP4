<template>
  <div class="quotation-wrapper">
    <el-row type="flex" class="title-wrapper">
      <p class="bold id">报价单号: <span>{{ formData.id || '' }}</span></p>
      <p class="bold">申请人: <span>{{ formData.createUser }}</span></p>
      <p>创建时间: <span>{{ formData.createTime }}</span></p>
      <p>销售员: <span>{{ formData.salesMan }}</span></p>
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
                    <img :src="rightImg" @click="openCustomerList" class="pointer">
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
        <!-- <el-form :model="courierAllList"> -->
          <div class="courier-table-wrapper">
            <common-table
            class="courier-table"
            ref="courierTable"
            :data="courierList" 
            :columns="courierColumns2"
            :height="0"
            max-height="150px"
          >
       
            <template v-slot:courierNumber="{ row }">
              <el-input size="mini" v-model="courierList[row.index].number"></el-input>
            </template>
         
            <template v-slot:logisticsInfo="{ row }">
              <el-row>
                <img :src="rightImg" @click="getExpressInfo(row)" class="search-icon">
                {{ courierList[row.index].info }}
              </el-row>
            </template>
       
            <template v-slot:remark="{ row }">
              <el-input size="mini" v-model="courierList[row.index].remark"></el-input>
            </template>
        
            <template v-slot:pictures="{ row }">
              <UpLoadFile 
                ref="uploadFile" 
                uploadType="file" 
                :limit="3" 
                :onAccept="onAccept"
                :fileList="courierList[row.index].fileList || []"
                @get-ImgList="getFileList" 
                :options="{ index: row.index }" 
              />
            </template> 
          </common-table>
          </div>
        <!-- </el-form> -->
        
        <div>
          <el-button class="add-courier-btn" size="min" @click="addCourier">新增快递</el-button>
        </div>
        
      </div>
      <!-- 物料表格 -->
      <div class="material-wrapper">
        <common-table 
          ref="materialTable"
          :data="materialList" 
          :columns="materialColumns">
          <template v-slot:delivery="{ row }">
            <el-input-number 
              size="mini"
              :controls="false"
              v-model="materialList[row.index].delivery"
              :mini="0"
              :max="materialList[row.index].count - materialList[row.index].outbound"
              :disabled="isOutboundAll(materialList[row.index])"
            >
            </el-input-number>  
          </template>  
        </common-table>
      </div>
      <my-dialog></my-dialog>
      <pagination :total="0"></pagination>
    </el-scrollbar>
  </div>
</template>

<script>
import { configMixin } from '../../common/js/mixins'
import CommonTable from '@/components/CommonTable' // 对于不可编辑的表格
import MyDialog from '@/components/Dialog'
import Pagination from '@/components/Pagination'
import UpLoadFile from '@/components/upLoadFile'
import rightImg from '@/assets/table/right.png'
import { isImage } from '@/utils/file'
export default {
  mixins: [configMixin],
  components: {
    CommonTable,
    MyDialog,
    Pagination,
    UpLoadFile
    // AreaSelector
  },
  computed: {
    courierAllList () {
      return {
        courierList: this.courierList
      }
    }
  },
  data () {
    return {
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
      // 物流表格
      courierLoading: false,
      courierList: [{
        number: '1',
        info: '',
        pictures: [],
        fileList: []
      }],
      courierColumns: [
        { label: '快递单号', type: 'input', prop: 'number', width: '100px' },
        { label: '物流信息', prop: 'info' },
        { label: '备注', type: 'input', prop: 'remark', width: '150px' },
        { label: '图片', prop: 'pictures', width: '200px' }
      ],
      courierColumns2: [
        { label: '快递单号', type: 'slot', slotName: 'courierNumber', prop: 'number', width: '100px' },
        { label: '物流信息', type: 'slot', slotName: 'logisticsInfo', prop: 'info' },
        { label: '备注', type: 'slot', slotName: 'remark', prop: 'remark', width: '150px' },
        { label: '图片', type: 'slot', slotName: 'pictures', prop: 'pictures', width: '200px' }
      ],
      // 物料表格
      materialList: [{
        materialCode: '1111',
        materialDescription: '232323',
        count: 5,
        unit: 'PCS',
        outbound: 5,
        delivery: ''
      }],
      materialColumns: [
        { label: '序号', type: 'order' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '数量', prop: 'count' },
        { label: '单位', prop: 'unit' },
        { label: '已出库', prop: 'outbound' },
        { label: '出库数量', prop: 'delivery', type: 'slot', slotName: 'delivery' }
      ]
    }
  },
  watch: {
    courierList: {
      deep: true,
      handler (val) {
        console.log(val, 'courierList')
      }
    }
  },
  methods: {
    onServiceIdFocus (prop) {
      console.log(prop)
    },
    openCustomerList () {
      this.$refs.customerDialog.open()
    },
    addCourier () { // 增加快递
      this.courierList.push({
        number: '1',
        info: '',
        pictures: []
      })
    },
    isOutboundAll (data) { // 判断是否物料的是否已经出料完成
      return !(data.count - data.outbound)
    },
    getExpressInfo (data) { // 查询物流信息
      console.log(data)
    },
    getFileList (value, { index }) {
      this.courierList[index].pictures = value
      console.log(this.courierList, 'fileList')
    },
    onAccept (file) { // 限制发票文件上传的格式
      let { type } = file
      // console.log(size, 'file size')
      let isValid = isImage(type)
      if (!isValid) {
        this.$notifyMessage(() => this.$message.error('文件格式只能为图片'))
        return false
      }
      return true
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