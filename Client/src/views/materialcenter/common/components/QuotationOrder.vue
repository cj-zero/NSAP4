<template>
  <div class="quotation-wrapper" v-loading="contentLoading">
    <el-row type="flex" class="title-wrapper">
      <p v-if="formData.salesOrderId && isSales"><span>销售订单</span><span>{{ formData.salesOrderId }}</span></p>
      <p v-if="!isSales"><span>{{ isSales ? '报价单号' : '领料单号' }}</span><span>{{ formData.id || '' }}</span></p>
      <p><span>申请人</span><span>{{ formData.createUser || createUser }}</span></p>
      <p><span>创建时间</span><span>{{ formData.createTime || createTime }}</span></p>
      <p><span>销售员</span><span>{{ formData.salesMan }}</span></p>
      <p v-if="formData.salesOrderId && !isSales">销售订单：<span>{{ formData.salesOrderId }}</span></p>
    </el-row>
    <!-- 主题内容 -->
    <el-scrollbar class="scroll-bar">
      <!-- 表单 -->
      <common-form
        :model="formData"
        :formItems="formItems"
        ref="form"
        class="my-form-wrapper"
        :class="{ 'my-form-view': isPreview }"
        label-width="60px"
        :disabled="isPreview"
        label-position="right"
        :show-message="false"
        :isCustomerEnd="true"
        :rules="formRules"
        :hide-required-asterisk="true"
      >
      
      </common-form>
      <!-- 技术员回传文件 -->
      <el-row class="upload-file-wrapper" type="flex" v-if="status === 'upload'">
        <span class="title-text">附件</span>
        <upLoadFile 
          :disabled="false"
          @get-ImgList="getFileList" 
          ref="uploadFile"
          :ifShowTip="true"
          :onAccept="onAccept"
          :fileList="formData.attachmentsFileList || []"
          >
        </upLoadFile>
      </el-row>
      <!-- 分割线 -->
      <div class="divider"></div>
      <!-- 报价单模块 -->
      <template v-if="isReceive">
        <!-- 非预览 -->
        <template v-if="!isPreview">
          <!-- 序列号查询表格 -->
          <div class="serial-wrapper">
            <el-row type="flex" justify="space-between">
              <div class="form-wrapper">
                <el-form :model="listQuerySearial" size="mini">
                  <el-row type="flex">
                    <el-col>
                      <el-form-item>
                        <el-input 
                          style="width: 150px;"
                          @keyup.enter.native="_getSerialNumberList" 
                          v-model.trim="listQuerySearial.manufacturerSerialNumbers" 
                          placeholder="制造商序列号"
                          size="mini">
                        </el-input>
                      </el-form-item>
                    </el-col>
                    <el-col>
                      <el-form-item>
                        <el-input 
                          style="width: 150px; margin-left: 10px;"
                          @keyup.enter.native="_getSerialNumberList" 
                          v-model.trim="listQuerySearial.materialCode" 
                          placeholder="物料编码"
                          size="mini">
                        </el-input>
                      </el-form-item>
                    </el-col>
                  </el-row>
                </el-form>
              </div>
            </el-row>
            <!-- 设备序列号表格 -->
            <div class="serial-table-wrapper">
              <common-table 
                @row-click="onSerialRowClick"
                ref="serialTable" 
                max-height="200px"
                :data="serialNumberList" 
                :columns="serialColumns" 
                :loading="serialLoading">
                <template v-slot:materialCode="{ row }">
                  <el-row type="flex" align="middle">
                    <span span v-infotooltip.top-start.ellipsis style="width: calc(100% - 12px); padding-right: 3px;">{{ row.materialCode }}</span>
                    <svg-icon iconClass="warranty" v-if="row.isProtected"></svg-icon>
                  </el-row>
                </template>
                <template v-slot:btn="scope">
                  <el-button 
                    type="success"
                    @click="openMaterialDialog(scope.row)"
                    size="mini">
                      <el-icon class="el-icon-edit">添加</el-icon>
                    </el-button>
                </template>
                <!-- 小计 -->
                <template v-slot:money="scope">
                  {{ scope.row.manufacturerSerialNumber | calcSerialTotalMoney(formData.quotationProducts) | toThousands }}
                </template>
              </common-table>
              <el-row class="money-line" justify="end">
                <span class="title">合计</span>
                ￥{{ totalMoney | toThousands }}
              </el-row>
            </div>
          </div>
          <div class="divider"></div>
          <!-- 物料填写表格 -->
          <div class="material-wrapper">
            <el-row type="flex" style="width: 200px;margin-bottom: 10px;">
              <span style="margin-right: 12px;color: #BFBFBF;">序列号</span>
              <span>{{ currentSerialNumber }}</span>
            </el-row>
            <el-form 
              ref="materialForm" 
              :model="materialData" 
              size="mini" 
              :show-message="false"
              class="form-wrapper"
            >
              <common-table
                class="material-table-wrapper"
                :data="materialData.list"
                :columns="materialConfig"
                max-height="200px"
              >
                <!-- 小计头 -->
                <!-- <template v-slot:totalPrice_header> -->
                  
                  <!-- <el-tooltip effect="dark" placement="top-end">
                    <div slot="content">{{ materialData.list | calcTotalItem(materialData.isProtected) | toThousands }}</div>
                    <p class="text-overflow">小计 <span>{{ materialData.list | calcTotalItem(materialData.isProtected) | toThousands }}</span></p>
                  </el-tooltip>                      -->
                <!-- </template> -->
                <!-- 数量 -->
                <template v-slot:count="{ row, index }">
                  <el-form-item
                    :prop="'list.' + index + '.' + 'count'"
                    :rules="materialRules['count']"
                  >
                    <el-input-number
                      size="mini"
                      style="width: 200px;"
                      :controls="false"
                      v-model="row.count"
                      :precision="0"
                      :placeholder="`最大数量${row['maxQuantity']}`"
                      :max="+row['maxQuantityText']"
                      :min="0"
                      @focus="onCountFocus(index)"
                      @change="onCountChange"
                    ></el-input-number>
                  </el-form-item>
                </template>
                <template v-slot:maxQuantity="{ row }">
                  <el-row type="flex" justify="end" align="middle">
                    <span span v-infotooltip.top-start.ellipsis style="width: calc(100% - 12px); padding-right: 3px;">{{ row.maxQuantity }}</span>
                    <el-tooltip effect="dark" placement="top-end">
                      <div slot="content">领取的数量只能是整数，大于等于当前最大数量</div>
                      <i class="notice-icon el-icon-warning-outline" v-if="!isIntegerNumber(+row.maxQuantity)"></i>
                    </el-tooltip>
                  </el-row>
                </template>
                <!-- 备注 -->
                <template v-slot:remark="{ row }">
                  <el-input size="mini" v-model="row.remark"></el-input>
                </template>
                <!-- 折扣 -->
                <template v-slot:discount="{ row, index }">
                  <el-select 
                    size="mini"
                    v-model="row.discount" 
                    placeholder="请选择"
                    @change="onDiscountChange"
                    @focus="onDiscountFocus(index)">
                    <el-option
                      v-for="item in discountList"
                      :key="item.value"
                      :label="item.label"
                      :value="item.value">
                    </el-option>
                  </el-select>
                </template>
                <!-- 总价格 -->
                <template v-slot:totalPrice="{ row }">
                  <span style="text-align: right;">{{ row.totalPrice | toThousands}}</span>
                </template>
                <!-- 操作 -->
                <template v-slot:operation="{ index }">
                  <div style="display: inline-block;" @click="deleteMaterialItem(index)">
                    <el-icon class="el-icon-delete icon-item"></el-icon>
                  </div>
                </template>
              </common-table>
            </el-form>
            <el-row class="subtotal-wrapper">
              <span class="title">合计</span>
              ￥{{ materialData.list | calcTotalItem(materialData.isProtected) | toThousands }}
            </el-row>
          </div>
        </template>
      </template>
      <!-- 不是新建状态并且处于预览状态 -->
      <template v-if="status !== 'create' && isPreview">
        <!-- 物料汇总表格 -->
        <div class="material-summary-wrapper">
          <common-table 
            class="material-summary-table"
            row-key="index"
            max-height="300px"
            :data="materialSummaryList" 
            :columns="materialAllColumns"
            :loading="materialAllLoading">
            <template v-slot:materialCode="{ row }">
              <el-row type="flex" align="middle" >
                <span v-infotooltip.top-start.ellipsis style="width: calc(100% - 12px); padding-right: 3px;">{{ row.materialCode }}</span>
                <svg-icon iconClass="warranty" v-if="row.isProtected"></svg-icon>
              </el-row>
            </template>
            <template v-slot:discount="{ row }">
              {{ (row.discount * 100) | toThousands }}%
            </template>
          </common-table>
          <el-row type="flex" class="money-wrapper" justify="end">
            <p v-infotooltip.top-start.ellipsis>￥{{ summaryTotalPrice | toThousands }}</p>
            <p v-infotooltip.top-start.ellipsis>￥{{ costPirceTotal | toThousands}}</p>
            <p v-infotooltip.top-start.ellipsis>￥{{ grossProfitTotal | toThousands }}</p>
            <p v-infotooltip.top-start.ellipsis>{{ grossProfit | toThousands }}%</p>
          </el-row>
        </div>
      </template>
      <div class="divider" v-if="status !== 'create'"></div>
      <!-- 操作记录 不可编辑时才出现 -->
      <template v-if="(this.status !== 'create') && isPreview && formData.quotationOperationHistorys && formData.quotationOperationHistorys.length">
        <div class="history-wrapper">
          <common-table 
            :data="formData.quotationOperationHistorys" 
            :columns="historyColumns" 
            max-height="200px"
          >
            <template v-slot:intervalTime="scope">
              {{ scope.row.intervalTime | m2DHM }}
            </template>
          </common-table>
        </div>
      </template>
    </el-scrollbar>
    <!-- 客户弹窗 -->
    <my-dialog
      ref="customerDialog"
      title="服务列表"
      width="770px"
      :btnList="customerBtnList"
      :append-to-body="true"
    >
      <common-table 
        ref="customerTable" 
        :data="customerData" 
        :columns="customerColumns" 
        max-height="400px"
        radioKey='id'>
      </common-table>
      <pagination
        v-show="customerTotal > 0"
        :total="customerTotal"
        :page.sync="listQueryCustomer.page"
        :limit.sync="listQueryCustomer.limit"
        @pagination="customerCurrentChange"
      />
    </my-dialog>
    <!-- 物料编码弹窗 -->
    <my-dialog 
      ref="materialDialog"
      title="物料编码"
      width="580px"
      :btnList="materialBtnList"
      :append-to-body="true"
      @closed="closeMaterialDialog"
    >
      <common-table  
        ref="materialTable" 
        row-key="id"
        height="400px"
        :data="materialList" 
        :columns="materialColumns" 
        :loading="materialLoading"
        :selectedList="selectedMaterialList"
        selectedKey="itemCode"></common-table>
      <pagination
        v-show="materialCount > 0"
        :total="materialCount"
        :page.sync="listQueryMaterial.page"
        :limit.sync="listQueryMaterial.limit"
        @pagination="handleMaterialChange"
      />
    </my-dialog>
    <!-- 驳回理由弹窗 -->
    <my-dialog
      ref="remarkDialog"
      title="驳回"
      :append-to-body="true"
      :btnList="remarkBtnList"
      @closed="onRemarkClose"
      v-loading="remarkLoading"
      width="350px">
      <remark ref="remark" @input="onRemarkInput" :tagList="[]"></remark>
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
    <!-- 预览合同图片 -->
    <!-- 预览图片 -->
    <el-image-viewer
      v-if="previewVisible"
      :url-list="previewImageUrlList"
      :on-close="closeViewer"
    >
    </el-image-viewer>
    <!-- <el-button @click="_checkFormData">点击校验</el-button> -->
  </div>
</template>

<script>
import { 
  getServiceOrderList,
  getSerialNumberList, 
  getMaterialList, 
  AddQuotationOrder, 
  updateQuotationOrder,
  // getQuotationMaterialsCode,
  approveQuotationOrder
} from '@/api/material/quotation'
import Remark from '@/views/reimbursement/common/components/remark'
import UpLoadFile from '@/components/upLoadFile'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
// import AreaSelector from '@/components/AreaSelector'
import { configMixin, quotationOrderMixin, categoryMixin, chatMixin, uploadFileMixin } from '../js/mixins'
import { timeToFormat } from "@/utils";
import { findIndex, accAdd } from '@/utils/process'
// import { toThousands } from '@/utils/format'
import { isNumber, isIntegerNumber } from '@/utils/validate'
import rightImg from '@/assets/table/right.png'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
const NOT_EDIT_STATUS_LIST = ['edit', 'upload', 'pay'] // 不可编辑的状态 1.查看 2.审批 3.支付
export default {
  inject: ['parentVm'],
  mixins: [configMixin, quotationOrderMixin, categoryMixin, chatMixin, uploadFileMixin],
  components: {
    Remark,
    zxform,
    zxchat,
    UpLoadFile,
    ElImageViewer
    // AreaSelector
  },
  filters: {
    calcTotalItem (val, isProtected) { // 计算每一个物料表格的总金额
      return isProtected ? 0 : 
        val.filter(item => isNumber(item.totalPrice))
          .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
    },
    calcSerialTotalMoney (serialNumber, list) {
      let index = findIndex(list, item => {
        return item.productCode === serialNumber
      })
      if (index !== -1 && !list[index].isProtected) { // 保外的才算钱
        return list[index].
          quotationMaterials.filter(item => isNumber(item.totalPrice))
          .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
      }
      return 0
    }
  },
  props: {
    customerList: {
      type: Array,
      default () { () => [] }
    },
    detailInfo: { // 详情
      type: Object,
      default () { () => {} }
    },
    status: { // 判断当前报价单所处于的状态 view create edit approve pay
      type: String
    },
    isReceive: { // 用来区别报价单模块(true) 和 (销售订单模块/待处理物料模块 | false)
      type: Boolean
    },
    isSales: Boolean, // 用来区分销售订单和领料单
    categoryList: {
      type: Array,
      default: () => []
    }
  },
  watch: {
    customerList: {
      immediate: true,
      handler (val, oldVal) {
        console.log(val, oldVal, 'val')
        if (this.status === 'create') { // 新建时才需要去获取客户信息列表
          this._getServiceOrderList()
        }
      }
    },
    'formData.serviceOrderSapId': {
      immediate: true,
      handler (val, oldVal) {
        console.log(val, 'newVal')
        if (val !== oldVal && val) {
          // if (this.isReceive) {
          if (this.isReceive) {
            if (this.status === 'create') {
              this.isPreview = false
              this._resetMaterialInfo() // 新建的时候,清除所有的跟物料相关的数据
            }
            if (this.status === 'edit' || this.status === 'create') {
              this.listQuerySearial.page = 1
              this.listQuerySearial.limit = 50
              this._getSerialNumberList()
            }
          }
          if (this.status !== 'create') {
            this._normalizeMaterialSummaryList()
          }
        }
      }
    },
    detailInfo: {
      immediate: true,
      handler (val) {
        if (this.status !== 'create') {
          this.isPreview = true
          Object.assign(this.formData, val)
          this.formData.quotationProducts = this.formData.quotationProducts.map(product => {
            product.quotationMaterials.forEach(material => {
              material.discount = String(Number(material.discount).toFixed(2)) // 保证discount是string类型，且跟字典对应上
            })
            return product
          })
          this.currentSerialNumber = this.formData.quotationProducts[0].productCode
          // 构建selected Map
          if (this.status === 'edit') {
            for (let i = 0; i < this.formData.quotationProducts.length; i++) {
              let item = this.formData.quotationProducts[i]
              this.selectedMap[item.productCode] = []
              for (let j = 0; j < item.quotationMaterials.length; j++) {
                let materilItem = item.quotationMaterials[j]
                this.selectedMap[item.productCode].push({ itemCode: materilItem.materialCode })
              }
            }
          }
        }
      }
    }
  },
  data () {
    return {
      // 合同图片
      previewImageUrlList: [], // 合同图片列表
      previewVisible: false,
      contentLoading: false, // 弹窗内容loading
      rightImg,
      // 表单数据
      formData: {
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
        newestContactTel: '', // 电话
        newestContacter: '', // 联系人
        acceptancePeriod: '', // 验收期限
        deliveryDate: '', // 交货日期
        moneyMeans: '', // 货币方式
        acquisitionWay: '', // 领料方式
        quotationProducts: [] // 报价单产品表
      }, 
      // 表单规则
      formRules: {
        serviceOrderSapId: [{ required: true }],
        terminalCustomerId: [{ required: true }],
        terminalCustomer: [{ required: true }],
        newestContacter: [{ required: true }],
        newestContactTel: [{ required: true }],
        deliveryDate: [{ required: true, trigger: ['change', 'blur'] }],
        acceptancePeriod: [{ required: true, trigger: ['change', 'blur'] }],
        shippingAddress: [{ required: true, trigger: ['change', 'blur'] }],
        acquisitionWay: [{ required: true, trigger: ['change', 'blur'] }],
        moneyMeans: [{ required: true, trigger: ['change', 'blur'] }],
        invoiceCompany: [{ required: true, trigger: ['change', 'blur'] }],
        deliveryMethod: [{ required: true, trigger: ['change', 'blur'] }],
        collectionAddress: [{ required: true, trigger: ['change', 'blur'] }]
      },
      createTime: timeToFormat('yyyy-MM-dd HH:mm'),
      // 操作记录
      historyColumns: [
        { label: '#', type: 'index', width: 50 },
        { label: '操作记录', prop: 'action' },
        { label: '操作人', prop: 'createUser' },
        { label: '操作时间', prop: 'createTime' },
        { label: '审批时长', prop: 'intervalTime', slotName: 'intervalTime' },
        { label: '审批结果', prop: 'approvalResult' },
        { label: '备注', prop: 'remark' }
      ],
      // 物料弹窗列表
      materialList: [],
      materialCount: 0,
      selectedMaterialList: [], // 已经选择了物料列表，再次弹窗时，不能再选
      selectedMap: {}, // 已经选择物料列表
      materialLoading: false,
      listQueryMaterial: {
        page: 1,
        limit: 20
      },
      materialBtnList: [
        { btnText: '确定', handleClick: this.selectMaterial },
        { btnText: '取消', handleClick: this.closeMaterialDialog }
      ],
      materialColumns: [ 
        { type: 'selection' },
        { label: '物料编码', prop: 'itemCode', width: 100 },
        { label: '物料描述', prop: 'itemName' },
        { label: '零件规格', prop: 'buyUnitMsr', width: 100, align: 'right' },
        { label: '库存量', prop: 'onHand', width: 100, align: 'right' },
        { label: '仓库号', prop: 'whsCode', width: 100, align: 'right' }
      ],
      // 根据设备序列号生成的物料表格
      materialConfig:[
        { label: '序号', type: 'index' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '数量', prop: 'count', slotName: 'count', align: 'right' },
        { label: '最大数量', prop: 'maxQuantity', align: 'right', slotName: 'maxQuantity', 'show-overflow-tooltip': false },
        { label: '库存量', prop: 'onHand' },
        { label: '仓库', prop: 'whsCode' },
        { label: '成本价(￥)', prop: 'unitPrice', align: 'right' },
        { label: '销售价(￥)', prop: 'salesPrice', align: 'right' },
        { label: '折扣(%)', prop: 'discount', slotName: 'discount', align: 'right' },
        { label: '小计(￥)', prop: 'totalPrice', disabled: true, align: 'right' },
        { label: '备注', slotName: 'remark', prop: 'remark' },
        { label: '操作', slotName: 'operation' }
      ],    
      // 物料填写表格列表 
      materialRules: {
        count: [{ required: true }]
      },
      // 物料汇总表格
      materialSummaryList: [],
      materialAllColumns: [
        { label: '序号', type: 'index', width: 50 },
        { label: '物料编码', prop: 'materialCode', slotName: 'materialCode', width: 130, 'show-overflow-tooltip': false },
        { label: '物料描述', prop: 'materialDescription', width: 200 },
        { label: '数量', prop: 'count', align: 'right', width: 70 },
        // { label: '单价', prop: '', align: 'right' },
        { label: '单价(￥)', prop: 'salesPrice', align: 'right', width: 100 },
        { label: '折扣(%)', prop: 'discount', align: 'right', slotName: 'discount', width: 100 },
        { label: '总计(￥)', prop: 'totalPrice', align: 'right', width: 100 },
        { label: '成本价(￥)', prop: 'costPrice', align: 'right', width: 100 },
        { label: '总成本(￥)', prop: 'totalCost', align: 'right', width: 100 },
        { label: '总毛利(￥)', prop: 'margin', align: 'right', width: 100 },
        { label: '毛利%', prop: 'profit', align: 'right', width: 80 }
      ],
      materialAllLoading: false,
      // 客户列表
      customerData: [],
      customerTotal: 0,
      listQueryCustomer: { // 客户列表分页参数
        page: 1,
        limit: 20
      },
      customerColumns: [
        { type: 'radio', width: '50px' },
        { label: '服务单号', prop: 'u_SAP_ID' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '销售员', prop: 'salesMan' }
      ],
      customerBtnList: [
        { btnText: '确定', handleClick: this.selectCustomer },
        { btnText: '取消', handleClick: this.closeCustomerDialog }
      ],
      // 序列号列表
      currentSerialNumber: '', // 当前选中的设备序列号
      serialNumberList: [],
      serialCount: 0,
      serialLoading: false,
      serialColumns: [
        { label: '制造商序列号', prop: 'manufacturerSerialNumber' },
        { label: '物料编码', prop: 'materialCode', slotName: 'materialCode', 'show-overflow-tooltip': false },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '保修到期', prop: 'docDate' },
        { label: '生产订单', prop: 'productionOrder' },
        { label: '销售订单', prop: 'salesOrder' },
        { label: '小计￥', slotName: 'money', align: 'right' },
        { label: '领取物料', slotName: 'btn' }
        // { label: '领取物料',  width: 100, type: 'operation',
        //   actions: [{ btnText: '领取', handleClick: this.openMaterialDialog }] 
        // }
      ],
      listQuerySearial: {
        page: 1,
        limit: 50,
        manufacturerSerialNumbers: '',
        materialCode: ''
      },
      // 预览数据
      isPreview: false, // 是否预览
      // 地址选择器
      isShowCollect: false,
      isShowShipping: false,
      // 驳回理由弹窗
      remark: '',
      remarkLoading: false,
      cancelRequestMaterial: null, // 用来取消物料列表的请求，防止数紊乱
      cancelRequestSerialList: null // 用来取消通过客户代码获取的设备序列号列表请求
    }
  },
  computed: {
    ifNotEdit () {
      console.log(this.status, 'status')
      return NOT_EDIT_STATUS_LIST.includes(this.status)
    },
    totalMoney () { // 报价单总金额
      if (this.formData.quotationProducts.length) {
        let val = this.formData.quotationProducts
        return this._calcTotalMoney(val)
      } 
      return 0
    },
    summaryTotalPrice () { // 汇总物料的销售总计
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.totalPrice)
      }, 0)
    },
    // salesPriceTotal () { // 汇总物料的销售价总计
    //   return this.materialSummaryList.reduce((prev, next) => {
    //     return accAdd(prev, next.salesPrice)
    //   }, 0)
    // },
    costPirceTotal () { // 汇总物料的成本价总计
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.totalCost)
      }, 0)
    },
    grossProfitTotal () { // 汇总物料的总毛利
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.margin)
      }, 0)
    },
    grossProfit () { // 毛利率
      if (this.grossProfitTotal === 0 || this.summaryTotalPrice === 0) { // 总毛利为零时， 毛利率为零
        return 0 
      }
      return (this.grossProfitTotal / this.summaryTotalPrice) * 100
    },
    materialData () {
      // 找到当前选择的设备序列号对应的物料列表数据
      let index = findIndex(this.formData.quotationProducts, item => {
        return item.productCode === this.currentSerialNumber
      })
      let { quotationMaterials = [], isProtected } = index !== -1 ? this.formData.quotationProducts[index] : {}
      return  { list: quotationMaterials , isProtected }
    },
    remarkBtnList () {
      return [
        { btnText: '确认', handleClick: this.approveByReject },
        { btnText: '取消', handleClick: this.closeRemark }
      ]
    }
  },
  methods: {
    isIntegerNumber,
    closeViewer () { // 关闭合同图片
      this.previewVisible = false
    },
    showContract () { // 展示合同图片
      if (!this.formData.quotationPictures.length) {
        return this.$message.warning('暂无合同片')
      }
      this.previewVisible = true
      this.previewImageUrlList = this.formData.quotationPictures 
    },
    getFileList (val) {
      console.log(val)
      let ids = val.map(item => item.pictureId)
      this.pictureIds = ids
    },
    // 计算总金额
    _calcTotalMoney (val) {
      let result = 0
      for (let i = 0; i < val.length; i++) {
        if (!val[i].isProtected) {
          result += val[i].quotationMaterials.filter(item => isNumber(item.totalPrice))
          .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
        }
      }
      return result
    },
    onSerialRowClick (val) { // 点击序列号表格 展示对应的物料表格
      this.currentSerialNumber = val.manufacturerSerialNumber
      this.currentSerialInfo = val
    },
    onServiceIdFocus () {
      if (this.status === 'create') {
        this.$refs.customerDialog.open()
      }
    },
    _openServiceOrder () { // 打开服务单
      this.openServiceOrder(this.formData.serviceOrderId, () => this.contentLoading = true, () => this.contentLoading = false)
    },
    selectCustomer () { // 选择客户数据
      let val = this.$refs.customerTable.getCurrentRow()
      if (!val) {
        return this.$message.warning('请先选择数据')
      }
      let { 
        terminalCustomer, terminalCustomerId, salesMan, id, u_SAP_ID, billingAddress, 
        deliveryAddress, frozenFor, balance, newestContacter, newestContactTel } = val
      this.formData.terminalCustomer = terminalCustomer
      this.formData.terminalCustomerId = terminalCustomerId
      this.formData.salesMan = salesMan
      this.formData.serviceOrderId = id
      this.formData.serviceOrderSapId = u_SAP_ID
      this.formData.shippingAddress = billingAddress
      this.formData.collectionAddress = deliveryAddress
      this.formData.frozenFor = frozenFor
      this.formData.balance = balance
      this.formData.newestContacter = newestContacter
      this.formData.newestContactTel =newestContactTel
      this.closeCustomerDialog()
    },
    closeCustomerDialog () {
      this.$refs.customerTable.resetCurrentRow()
      this.$refs.customerTable.resetRadio() // 清空单选
      this.$refs.customerDialog.close()
    },
    customerCurrentChange ({ page, limit }) {
      this.listQueryCustomer.page = page
      this.listQueryCustomer.limit = limit
      this._getServiceOrderList()
    },
    _getServiceOrderList () { // 获取用户信息列表
      getServiceOrderList(this.listQueryCustomer).then(res => {
        let { count, data } = res
        this.customerData = data
        this.customerTotal = count
      }).catch(err => {
        this.$message.error(err.message)
      })
    },
    _getSerialNumberList () { // 获取设备序列号列表
      if (!this.formData.serviceOrderSapId) {
        return this.$message.warning('请先选择服务单!')
      }
      if (this.cancelRequestSerialList) {
        this.cancelRequestSerialList('customer abort')
      }
      this.serialLoading = true
      getSerialNumberList({
        serviceOrderId: this.formData.serviceOrderId,
        ...this.listQuerySearial
      }, this).then(res => {
        let { data, count } = res
        this.serialNumberList = data
        this.serialCount = count
        this.serialLoading = false
        console.log(res, 'res')
      }).catch(err => {
        if (err.message !== 'customer abort') {
          this.serialLoading = false
          this.$message.error(err.message)
        }
      })
    },
    _getMaterialList (manufacturerSerialNumber, materialCode) { // 获取物料列表
      if (this.cancelRequestMaterial) {
        this.cancelRequestMaterial('customer abort')
      }
      this.materialLoading = true
      getMaterialList({
        ManufacturerSerialNumbers: manufacturerSerialNumber,
        materialCode,
        ...this.listQueryMaterial
      }, this).then(res => {
        console.log(res, 'res')
        let { data, count } = res
        this.materialList = data
        this.materialCount = count
        this.materialLoading = false
      }).catch(err => {
        if (err.message !== 'customer abort') {
          this.materialLoading = false
          this.$message.error(err.message)
        }
      })
    },
    handleMaterialChange ({ page, limit }) {
      this.listQueryMaterial.page = page
      this.listQueryMaterial.limit = limit
      this._getMaterialList()
    },
    openMaterialDialog (val) { // 添加物料
      let { manufacturerSerialNumber, materialCode } = val
      if (this.currentSerialNumber !== manufacturerSerialNumber) {
        this.listQueryMaterial.page = 1
        this.listQueryMaterial.limit = 20
        this.currentSerialNumber = manufacturerSerialNumber
        this.currentSerialInfo = val
      }
      this._getMaterialList(manufacturerSerialNumber, materialCode)
      this.selectedMaterialList = this.selectedMap[this.currentSerialNumber] || []
      this.$refs.materialDialog.open()
    }, 
    deleteMaterialItem (index) { // 删除物料
      console.log('delete', index)
      // 执行删除操作时
      // 删除表格数据
      this.materialData.list.splice(index, 1)
      this.selectedMap[this.currentSerialNumber].splice(index, 1)
      if (!this.materialData.list.length) {
        // deleteList
        this.formData.quotationProducts.splice(this.materialIndex, 1)
        delete this.selectedMap[this.currentSerialNumber]
      }
    },
    _normalizeMaterialSummaryList () {

      this.materialSummaryList = this.formData.quotationMergeMaterials.map((item, index) => {
        let { count, costPrice } = item
        item.index = index
        // item.grossProfit = salesPrice - costPrice // 总毛利
        item.totalCost = costPrice * count // 总成本
        return item
      })
    },
    onDiscountChange (val) {
      let { list, isProtected } = this.materialData
      let data = list[this.materialItemIndex]
      console.log(val, 'val')
      let { salesPrice, count } = data
      data.totalPrice = Number((!isProtected ? (val * salesPrice * count || 0) : 0).toFixed(2))
    },
    onDiscountFocus (index) {
      this.materialItemIndex = index
      console.log(this.materilItemIndex, 'discount focus')
    },
    onCountFocus (index) {
      this.materialItemIndex = index // 当前点击的物料表格的第几项
    },
    onCountChange (val) {
      let { list, isProtected } = this.materialData
      let data = list[this.materialItemIndex]
      let { salesPrice, discount } = data
      data.totalPrice = Number((!isProtected ? (val * salesPrice * discount || 0) : 0).toFixed(2))
    },
    _resetMaterialInfo () { // 重置物料相关的变量和数据
      this.formData.quotationProducts = []
      this.selectedMap = {}
      this.selectedMaterialList = []
      this.currentSerialNumber = ''
      this.listQueryMaterial.page = 1
      this.listQueryMaterial.limit = 20
    },
    selectMaterial () { // 选择弹窗物料
      let selectedList = this.$refs.materialTable.getSelectionList()
      if (!selectedList.length) {
        return this.$message.warning('请先选择零件')
      }
      this._mergeSelectedList(selectedList)
      this.closeMaterialDialog()
      console.log(this.formData.quotationProducts, 'productList')
    }, 
    _normalizeSelectedList (selectedList) { // 格式化物料表格数据
      return selectedList.map(selectItem => {
        let item = {}
        let { isProtected } = this.currentSerialInfo
        let { itemCode, itemName, onHand, quantity,  buyUnitMsr, whsCode } = selectItem
        item.unit = buyUnitMsr
        item.materialDescription = itemName
        item.materialCode = itemCode
        item.remark = ''
        item.unitPrice = 7
        item.salesPrice = 3 * item.unitPrice
        item.discount = '1.00'
        item.count = 1
        item.onHand = onHand
        item.whsCode = whsCode
        item.maxQuantity = quantity
        item.maxQuantityText = Math.ceil(quantity)
        console.log(item.maxQuantity, 'quantity', item.maxQuantityText)
        item.totalPrice = Number((!isProtected ? item.salesPrice * item.count : 0).toFixed(2))
        return item
      })
    },
    _mergeSelectedList (selectedList) { // 整合所有的物料表格数据
      let index = findIndex(this.formData.quotationProducts, item => {
        return item.productCode === this.currentSerialNumber
      })
      let materialList = this._normalizeSelectedList(selectedList)
      // 如果数组中已经存在了这个对象值 则需要将之前选择的跟现在选择的进行合并
      if (index > -1) {
        this.formData.quotationProducts[index].quotationMaterials.push(...materialList)
        this.selectedMap[this.currentSerialNumber].push(...selectedList)
      } else {
        this.formData.quotationProducts.push({
          ...this.currentSerialInfo,
          productCode: this.currentSerialNumber,
          quotationMaterials: materialList
        })
        this.selectedMap[this.currentSerialNumber] = []
        this.selectedMap[this.currentSerialNumber].push(...selectedList)
      }
      this.materialIndex = index > -1 ? index : this.formData.quotationProducts.length - 1 // 设置当前物料表格对应quotationProducts第几项数据
    },
    closeMaterialDialog () { // 关闭弹窗
      this.$refs.materialTable.clearSelection()
      this.$refs.materialDialog.close()
    },
    resetInfo () { // 每次关闭弹窗
      // 预览数据
      this.reset()
      console.log(this.formData, this.formData.serviceOrderSapId)
      this.$nextTick(() => {
        this.$refs.form.clearValidate()
      })
    },
    togglePreview () { // 预览
      if (!this.formData.quotationProducts.length) {
        return this.$message.warning('请先选择零件')
      }
      this.isPreview = !this.isPreview
    },
    onAreaFocus (prop) {
      this.onCloseArea()
      if (prop === 'shippingAddress') {
        this.isShowShipping = true 
      } else {
        this.isShowCollect = true
      }
    },  
    onCloseArea () { // 关闭地址选择器
      this.isShowCollect = false
      this.isShowShipping = false
    },
    onAreaChange (val) { // 选择地址完毕
      let { province, city, district, prop } = val
      const countryList = ['北京市', '天津市', '上海市', '重庆市']
      let result = ''
      result = countryList.includes(province)
        ? province + district
        : city + district
      console.log(prop, 'prop')
      this.formData[prop] = result
      this.onCloseArea()
    },
    onRemarkInput (value) {
      this.remark = value
    },
    closeRemark () {
      this.remark = ''
      this.$refs.remark.reset()
      this.$refs.remarkDialog.close()
    },
    onRemarkClose () {
      this.remark = ''
      this.$refs.remark.reset()
    },
    async _checkFormData () {
      let isFormValid = false
      try {
        isFormValid = await this.$refs.form.validate()
        console.log(isFormValid, 'isFormValid')
      } catch (err) {
        console.log(isFormValid, 'isFormValid')
      }
      return isFormValid
    },
    async _operateOrder (isDraft) { // 提交 存稿 针对于报价单
      // 判断表头表单
      console.log(this.formData.quotationProducts)
      let isFormValid = await this._checkFormData()
      console.log(isFormValid, 'isFormValid')
      if (!isFormValid) {
        return Promise.reject({ message: '请将表单必填项填写完成' })
      }
      // 判断物料列表
      if (!this.formData.quotationProducts.length) {
        return Promise.reject({ message: '零件物料不能为空' })
      }
      let materialList = []
      this.formData.quotationProducts.forEach(item => {
        materialList.push(...item.quotationMaterials)
      })
      let isMaterialValid = materialList.every(item => item.count)
      if (!isMaterialValid) {
        return Promise.reject({ message: '零件数量不能为空' })
      }
      console.log(this.formData.quotationProducts)
      return this.status !== 'create'
        ? updateQuotationOrder({
            ...this.formData,
            isDraft,
            totalMoney: this.totalMoney
          })
        : AddQuotationOrder({
          ...this.formData,
            isDraft,
            createTime: this.createTime,
            totalMoney: this.totalMoney,
            createUser: this.formData.createUser || this.createUser
          })
    },
    async beforeApprove (type) { // 待处理的报价单 审批
      this.$refs.form.validate(isValid => {
        if (isValid) {
          console.log(this.pictureIds)
          if (type !== 'pay') { // 技术员回传，一定要上传图片
            if (!this.pictureIds || (this.pictureIds && !this.pictureIds.length)) {
              return this.$message.warning('必须上传附件')
            }
          }
          let text = type === 'pay' ? '确定支付' : '确定上传'
          this.$confirm(text + '?', '提示', {
              confirmButtonText: '确定',
              cancelButtonText: '取消',
              type: 'warning'
            }).then(() => {
              this._approve(type)
            })
        } else {
          this.$message.error('请将必填项填写')
        }
      })
    },
    approveByReject () {
      this._approve('reject')
    },
    async _approve (type) { // 审核
      let isFormValid = await this._checkFormData()
      if (!isFormValid) {
        return Promise.reject({ message: '请将表单必填项填写完成' })
      }
      let params = {
        id: this.formData.id,
        isReject: false,
        pictureIds: (this.pictureIds && this.pictureIds.length) ? this.pictureIds : undefined
        // remark: this.remark,
        // invoiceCompany: this.formData.invoiceCompany
      }
      type === 'reject'
        ? this.remarkLoading = true
        : this.contentLoading = true
      approveQuotationOrder(params).then(() => {
        this.$message({
          type: 'success',
          message: type === 'reject' 
            ? '驳回成功' 
            : (type === 'pay' ? '收款成功' : '提交成功')
        })
        this.parentVm._getList()
        this.parentVm.close()
        if (type === 'reject') {
          this.remarkLoading = false
          this.closeRemark()
        } else {
          this.contentLoading = false
          this.parentVm.close()
        }
      }).catch(() => {
        type === 'reject'
          ? this.remarkLoading = false
          : this.contentLoading = false
        this.$message.error('操作失败')
      })
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
  font-size: 12px;
  .divider {
    height: 1px;
    margin: 15px auto;
    background: #E6E6E6;
  }
  /* 表头文案 */
  > .title-wrapper { 
    position: absolute;
    top: -51px;
    left: 130px;
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
  /* 外层滚动 */
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
    /* 表单内容 */
    .my-form-wrapper {
      flex: 1;
      .area-wrapper {
        position: relative;
        .selector-wrapper {
          position: absolute;
          left: 0;
          top: 27px;
          z-index: 10;
        }
      }
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
    /* 回传附件样式 */
    .upload-file-wrapper { 
      .title-text {
        width: 60px;
        padding-right: 4px;
        text-align: right;
        color: #C0C4CC;
      }
    }
    /* 序列号查询表格 */
    .serial-wrapper {
      .form-wrapper {
        
      }
      .serial-table-wrapper {
        margin-top: 10px;
        .money-line {
          margin: 10px 0;
          text-align: right;
          span {
            margin-right: 20px;
            &.title {
              color: #d0d0d0;
            }
          }
        }
      }
    }
    /* 物料填写表格 */
    .material-wrapper {
      .form-wrapper {
        ::v-deep .el-input-number {
          width: 100% !important;
        }
        .material-table-wrapper {
          overflow: hidden;
          white-space: nowrap;
          .notice-icon {
            color: rgb(248, 181, 0);
          }
        }
        .icon-item {
          cursor: pointer;
        }
      }
      .subtotal-wrapper {
        margin-top: 10px;
        text-align: right;
        span {
          margin-right: 20px;
          &.title {
            color: #d0d0d0;
          }
        }
      }
    }
    /* 物料汇总表格 */
    .material-summary-wrapper {
      width: 1132px;
      margin-top: 10px;
      .material-summary-table {
        height: auto !important;
        ::v-deep .el-table__footer-wrapper {
          font-weight: bold;
        }
      }
      /* 物料汇总金额样式 */
      .money-wrapper { 
        font-size: 12px;
        font-weight: bold;
        p {
          width: 100px;
          text-align: right;
          &:nth-child(1) {
            margin-right: 100px;
          }
          &:nth-child(4) {
            width: 80px;
          }
        }
      }
    }
    /* 操作记录表格 */
    .history-wrapper {
      margin-top: 10px;
    }
  }
}
</style>
