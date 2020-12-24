<template>
  <div class="quotation-wrapper" :class="{ uneditable: ifNotEdit }" v-loading="contentLoading">
    <el-row type="flex" class="title-wrapper">
      <p class="bold id" v-if="formData.salesOrderId && isSales">销售单号：<span>{{ formData.salesOrderId }}</span></p>
      <p class="bold id">{{ isSales ? '报价单号' : '领料单号' }}: <span>{{ formData.id || '' }}</span></p>
      <p class="bold">申请人: <span>{{ formData.createUser || createUser }}</span></p>
      <p>创建时间: <span>{{ formData.createTime || createTime }}</span></p>
      <p>销售员: <span>{{ formData.salesMan }}</span></p>
      <p class="bold id" v-if="formData.salesOrderId && !isSales">销售单号：<span>{{ formData.salesOrderId }}</span></p>
    </el-row>
    <!-- 主题内容 -->
    <el-scrollbar class="scroll-bar">
      <!-- 表单 -->
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
                  <span :class="{ 'total-money': item.label === '总金额'}">{{ item.label }}</span>
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
                    v-for="item in item.options"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </template>
              <template v-else-if="item.type === 'money'">
                <el-tooltip effect="dark" placement="top-start">
                  <div slot="content">{{ totalMoney | toThousands }}</div>
                  <p class="total-money text-overflow">{{ totalMoney | toThousands }}</p>
                </el-tooltip>
              </template>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <!-- 报价单模块 -->
      <template v-if="isReceive">
        <!-- 非预览 -->
        <template v-if="!isPreview">
          <!-- 序列号查询表格 -->
          <div class="serial-wrapper">
            <el-row type="flex" justify="space-between">
              <div class="form-wrapper">
                <el-form :model="listQuerySearial" size="mini" :disabled="ifNotEdit">
                  <el-row type="flex">
                    <el-col>
                      <el-form-item>
                        <el-input 
                          style="width: 150px;"
                          @keyup.enter.native="_getSerialNumberList" 
                          v-model="listQuerySearial.serialNumber" 
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
                          v-model="listQuerySearial.materialCode" 
                          placeholder="物料编码"
                          size="mini">
                        </el-input>
                      </el-form-item>
                    </el-col>
                  </el-row>
                </el-form>
              </div>
            </el-row>
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
                    <span style="margin-right: 3px;">{{ row.materialCode }}</span>
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
                <template v-slot:money="scope">
                  {{ scope.row.manufacturerSerialNumber | calcSerialTotalMoney(formData.quotationProducts) }}
                </template>
              </common-table>
            </div>
          </div>
          <!-- 物料填写表格 -->
          <div class="material-wrapper">
            <el-form 
              ref="materialForm" 
              :model="materialData" 
              size="mini" 
              :show-message="false"
              class="form-wrapper"
              :disabled="ifNotEdit"
            >
              <common-table
                class="material-table-wrapper"
                :data="materialData.list"
                :columns="materialConfig"
                max-height="200px"
              >
                <!-- 小计头 -->
                <template v-slot:totalPrice_header>
                  <el-tooltip effect="dark" placement="top-end">
                    <div slot="content">{{ materialData.list | calcTotalItem(materialData.isProtected) | toThousands }}</div>
                    <p class="text-overflow"> 小计 <span>{{ materialData.list | calcTotalItem(materialData.isProtected) | toThousands }}</span></p>
                  </el-tooltip>                     
                </template>
                <!-- 数量 -->
                <template v-slot:count="{ row }">
                  <el-form-item
                    :prop="'list.' + row.index + '.' + row.prop"
                    :rules="materialRules[row.prop]"
                  >
                    <el-input-number
                      size="mini"
                      style="width: 200px;"
                      :controls="false"
                      v-model="materialData.list[row.index][row.prop]" 
                      :precision="0"
                      :placeholder="`最大数量${row['maxQuantity']}`"
                      :max="+row['maxQuantity']"
                      :min="0"
                      @focus="onCountFocus(row.index)"
                      @change="onCountChange"
                    ></el-input-number>
                  </el-form-item>
                </template>
                <!-- 备注 -->
                <template v-slot:remark="{ row }">
                  <el-input size="mini" v-model="materialData.list[row.index][row.prop]"></el-input>
                </template>
                <!-- 折扣 -->
                <template v-slot:discount="{ row }">
                  <el-select 
                    size="mini"
                    v-model="materialData.list[row.index].discount" 
                    placeholder="请选择"
                    @change="onDiscountChange"
                    @focus="onDiscountFocus(row.index)">
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
                <template v-slot:operation="{ row }">
                  <div style="display: inline-block;" @click="deleteMaterialItem(row.index)">
                    <el-icon class="el-icon-delete icon-item"></el-icon>
                  </div>
                </template>
              </common-table>
            </el-form>
          </div>
        </template>
        <!-- 预览 -->
        <template v-else>
          <div class="preview-wrapper">
            <ul class="preview-list-wrapper">
              <li 
                class="preview-item"
                v-for="item in formData.quotationProducts"
                :key="item.manufacturerSerialNumber"
              >
                <el-row type="flex" class="serial-info-wrapper">
                  <el-form label-width="85px" inline size="mini" style="padding-top: 0;" disabled>
                    <el-form-item label="制造商序列号">
                      <el-input class="input-item" size="mini" v-model="item.productCode"></el-input>
                    </el-form-item>
                    <el-form-item label="物料编码" label-width="60px">
                      <el-input class="input-item" size="mini" v-model="item.materialCode"></el-input>
                    </el-form-item>
                    <el-form-item label="物料描述" label-width="60px">
                      <el-input class="input-item" size="mini" v-model="item.materialDescription"></el-input>
                    </el-form-item>
                    <el-form-item label="保修日期" label-width="60px">
                      <el-input class="input-item" size="mini" v-model="item.warrantyExpirationTime"></el-input>
                    </el-form-item>
                  </el-form>
                  <!-- <div class="total-money">总金额: {{ item.quotationMaterials | calcTotalItem | toThousands }}</div> -->
                </el-row>
                <div>
                  <common-table 
                    max-height="200px" 
                    :data="item.quotationMaterials" 
                    :columns="materialTableColumns">
                    <template v-slot:totalPrice_header="scope">
                      <el-tooltip effect="dark" placement="top-end">
                        <div slot="content">{{ item.quotationMaterials | calcTotalItem(item.isProtected) | toThousands }}</div>
                        <p class="text-overflow">{{ scope.row.label }} {{ item.quotationMaterials | calcTotalItem(item.isProtected) | toThousands }}</p>
                      </el-tooltip>                      
                    </template>
                    <template v-slot:discount="{ row }">
                      {{ row.discount ? row.discount * 100  + '%' : '' }}
                    </template>
                    <template v-slot:totalPrice="{ row }">
                      <span style="text-align: right;">{{ row.totalPrice | toThousands }}</span>
                    </template>
                  </common-table>
                </div>
              </li>
            </ul>
          </div>
        </template>
        <el-row justify="end" class="quotation-total-money">
          <p>总计：{{ totalMoney | toThousands }}</p>
        </el-row>
      </template>
      <template v-else>
        <!-- 审批/销售订单出现 -->
        <!-- 物料汇总表格 -->
        <div class="material-summary-wrapper">
          <common-table 
            class="material-summary-table"
            row-key="index"
            max-height="400px"
            :data="materialSummaryList" 
            :columns="materialAllColumns" 
            :loading="materialAllLoading">
            <template v-slot:materialCode="{ row }">
              <el-row type="flex" align="middle">
                <span style="margin-right: 3px;">{{ row.materialCode }}</span>
                <svg-icon iconClass="warranty" v-if="row.isProtected"></svg-icon>
              </el-row>
            </template>
          </common-table>
          <el-row type="flex" class="money-wrapper">
            <p>总金额：{{ salesPriceTotal | toThousands }}</p>
            <p>成本：{{ costPirceTotal | toThousands}}</p>
            <p>毛利： {{ grossProfit | toThousands }}%</p>
            <p>毛利总价：{{ grossProfitTotal | toThousands }}</p>
          </el-row>
        </div>
      </template>
      <!-- 操作记录 不可编辑时才出现 -->
      <template v-if="ifNotEdit && formData.quotationOperationHistorys && formData.quotationOperationHistorys.length">
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
  // getQuotationMaterialCode,
  approveQuotationOrder
} from '@/api/material/quotation'
import Remark from '@/views/reimbursement/common/components/remark'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
// import AreaSelector from '@/components/AreaSelector'
import { configMixin, quotationOrderMixin, categoryMixin, chatMixin } from '../js/mixins'
import { timeToFormat } from "@/utils";
import { findIndex, accAdd } from '@/utils/process'
import { isNumber } from '@/utils/validate'
import rightImg from '@/assets/table/right.png'
const NOT_EDIT_STATUS_LIST = ['view', 'approve', 'pay'] // 不可编辑的状态 1.查看 2.审批 3.支付
export default {
  inject: ['parentVm'],
  mixins: [configMixin, quotationOrderMixin, categoryMixin, chatMixin],
  components: {
    Remark,
    zxform,
    zxchat
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
      handler (val) {
        console.log(val, 'newVal')
        if (val) {
          if (this.isReceive) {
            if (this.status === 'create') {
              this._resetMaterialInfo() // 新建的时候,清除所有的跟物料相关的数据
            }
            if (this.status !== 'view') {
              this.listQuerySearial.page = 1
              this.listQuerySearial.limit = 50
              this._getSerialNumberList()
            } else {
              // 查看时直接预览
              this.isPreview = true
            }
          } else {
            // 审批/销售订单
            // 获取服务单的所有报价零件
            this._normalizeMaterialSummaryList()
            // this._getQuotationMaterialCode()
          }
        }
      }
    },
    detailInfo: {
      immediate: true,
      handler (val) {
        if (this.status !== 'create') {
          Object.assign(this.formData, val)
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
    },
    selectedMap: {
      deep: true,
      handler (val) {
        console.log(val, 'selectedMap')
      }
    }
  },
  data () {
    return {
      contentLoading: false, // 弹窗内容loading
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
      createTime: timeToFormat('yyyy-MM-dd HH:mm'),
      rules: {
        serviceOrderSapId: [ { required: true } ],
        terminalCustomerId: [ { required: true } ],
        terminalCustomer: [{ required: true }],
        shippingAddress: [{ required: true, trigger: ['change', 'blur'] }],
        invoiceCompany: [{ required: true, trigger: ['change', 'blur'] }],
        collectionAddress: [{ required: true, trigger: ['change', 'blur'] }],
        deliveryMethod: [{ required: true, trigger: ['change', 'blur'] }]
      }, // 上表单校验规则
      // 操作记录
      historyColumns: [
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
        { label: '最大数量', prop: 'maxQuantity', align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right' },
        { label: '销售价', prop: 'salesPrice', align: 'right' },
        { label: '折扣(%)', prop: 'discount', slotName: 'discount', align: 'right' },
        { label: '备注', slotName: 'remark', prop: 'remark' },
        { label: '小计', prop: 'totalPrice', disabled: true, align: 'right', isCustomizeHeader: true, slotName: 'totalPrice' },
        { label: '操作', slotName: 'operation' }
      ],    
      // 物料填写表格列表 
      materialRules: {
        count: [{ required: true }]
      },
      // 预览
      materialTableColumns: [
        { label: '序号', type: 'index' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '数量', prop: 'count', align: 'right' },
        { label: '最大数量', prop: 'maxQuantity', align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right' },
        { label: '销售价', prop: 'salesPrice', align: 'right' },
        { label: '折扣(%)', prop: 'discount', align: 'right', slotName: 'discount' },
        { label: '备注', prop: 'remark', type: 'input' },
        { label: '小计', prop: 'totalPrice', align: 'right', isCustomizeHeader: true, slotName: 'totalPrice' },
      ],
      // 物料汇总表格
      materialSummaryList: [],
      materialAllColumns: [
        { label: '序号', type: 'index', width: '50px' },
        { label: '物料编码', prop: 'materialCode', slotName: 'materialCode', width: '150px' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '数量', prop: 'count', align: 'right' },
        { label: '成本价', prop: 'costPrice', align: 'right' },
        { label: '销售价', prop: 'salesPrice', align: 'right' },
        { label: '总计', prop: 'totalPrice', align: 'right' },
        { label: '可领数量', prop: 'a', align: 'right' },
        { label: '当前库存', prop: 'b', align: 'right' },
        { label: '仓库', prop: 'c', align: 'right' }
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
        { label: '物料编码', prop: 'materialCode', slotName: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '保修到期', prop: 'warrantyExpirationTime' },
        { label: '金额', slotName: 'money', align: 'right' },
        { label: '领取物料', slotName: 'btn' }
        // { label: '领取物料',  width: 100, type: 'operation',
        //   actions: [{ btnText: '领取', handleClick: this.openMaterialDialog }] 
        // }
      ],
      listQuerySearial: {
        page: 1,
        limit: 50,
        serialNumber: '',
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
    salesPriceTotal () { // 汇总物料的 销售价总计
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.salesPrice)
      }, 0)
    },
    costPirceTotal () { // 汇总物料的 成本价总计
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.costPrice)
      }, 0)
    },
    grossProfitTotal () { // 毛利总价
      return this.salesPriceTotal - this.costPirceTotal
    },
    grossProfit () { // 毛利
      if (this.costPirceTotal === 0) {
        return 0 
      }
      return (this.grossProfitTotal / this.costPirceTotal * 100)
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
    onSerialRowClick (val) {
      this.currentSerialNumber = val.manufacturerSerialNumber
      this.currentSerialInfo = val
    },
    onServiceIdFocus (prop) {
      if (prop === 'serviceOrderSapId' && this.status === 'create') {
        this.$refs.customerDialog.open()
      }
    },
    _openServiceOrder () { // 打开服务单
      // if (!this.formData.serviceOrderId) {
      //   return this.$message.error('请先选择客户单')
      // }
      console.log(this.formData.serviceOrderId)
      this.openServiceOrder(this.formData.serviceOrderId, () => this.contentLoading = true, () => this.contentLoading = false)
      // this.$refs.customerDialog.open()
    },
    selectCustomer () { // 选择客户数据
      let val = this.$refs.customerTable.getCurrentRow()
      if (!val) {
        return this.$message.warning('请先选择数据')
      }
      let { terminalCustomer, terminalCustomerId, salesMan, id, u_SAP_ID, billingAddress, deliveryAddress } = val
      this.formData.terminalCustomer = terminalCustomer
      this.formData.terminalCustomerId = terminalCustomerId
      this.formData.salesMan = salesMan
      this.formData.serviceOrderId = id
      this.formData.serviceOrderSapId = u_SAP_ID
      this.formData.shippingAddress = billingAddress
      this.formData.collectionAddress = deliveryAddress
      this.closeCustomerDialog()
    },
    closeCustomerDialog () {
      this.$refs.customerTable.resetRadio() // 清空单选
      this.$refs.customerDialog.close()
    },
    customerCurrentChange ({ page, limit }) {
      this.listQueryCustomer.page = page
      this.listQueryCustomer.limit = limit
      this._getServiceOrderList()
    },
    _getServiceOrderList () {
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
      this.serialLoading = true
      getSerialNumberList({
        serviceOrderId: this.formData.serviceOrderId,
        ...this.listQuerySearial
      }).then(res => {
        let { data, count } = res
        this.serialNumberList = data
        this.serialCount = count
        this.serialLoading = false
        console.log(res, 'res')
      }).catch(err => {
        this.serialLoading = false
        this.$message.error(err.message)
      })
    },
    _getMaterialList (manufacturerSerialNumber, materialCode) { // 获取物料列表
      this.materialLoading = true
      getMaterialList({
        ManufacturerSerialNumbers: manufacturerSerialNumber,
        materialCode,
        ...this.listQueryMaterial
      }).then(res => {
        console.log(res, 'res')
        let { data, count } = res
        this.materialList = data
        this.materialCount = count
        this.materialLoading = false
      }).catch(err => {
        this.$message.error(err.message)
        this.materialLoading = false
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
      console.log(this.formData.quotationMergeMaterials)
      this.materialSummaryList = this.formData.quotationMergeMaterials.map((item, index) => {
        item.index = index
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
        let { itemCode, itemName, onHand, quantity,  buyUnitMsr } = selectItem
        item.unit = buyUnitMsr
        item.materialDescription = itemName
        item.materialCode = itemCode
        item.remark = ''
        item.unitPrice = 7
        item.salesPrice = 3 * item.unitPrice
        item.discount = '1.00'
        item.count = 1
        item.onHand = onHand
        item.maxQuantity = quantity
        console.log(item.maxQuantity, 'quantity')
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
      // 清空数据
      this.formData = {
        // id: '',  报价单号
        salesOrderId: '', // 销售单号
        serviceOrderSapId: '', // NSAP ID
        serviceOrderId: '', 
        createUser: '',
        terminalCustomer: '', // 客户名称
        terminalCustomerId: '', // 客户代码
        shippingAddress: '', // 开票地址
        collectionAddress: '', // 收款地址
        deliveryMethod: '', //发货方式
        invoiceCompany: '', // 开票单位
        salesMan: '', // 销售员
        totalMoney: 0, // 总金额
        quotationProducts: [] // 报价单产品表
      } // 表单数据
      this.$refs.form.clearValidate()
      this.$refs.form.resetFields()
      this.serialNumberList = []
      this.selectedMaterialList = [] // 已经选择了物料列表，再次弹窗时，不能再选
      this.selectedMap = {} // 已经选择物料列表
      this.listQueryMaterial = {
        page: 1,
        limit: 20
      }
      this.listQueryCustomer = { // 客户列表分页参数
        page: 1,
        limit: 20
      }
      // 序列号列表
      this.currentSerialInfo = null
      this.currentSerialNumber = '', // 当前选中的设备序列号
      this.listQuerySearial = {
        page: 1,
        limit: 50,
        serialNumber: '',
        materialCode: ''
      }
      // 预览数据
      this.isPreview = false
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
    async _operateOrder (isEdit, isDraft) { // 提交 存稿 针对于报价单
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
      // this.formData.quotationProducts.forEach((item, index) => {
      //   item.isProtected = !!(index % 2)
      // })
      console.log(this.formData.quotationProducts)
      return isEdit 
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
          if (type !== 'reject') {
            this.$confirm('确定审批通过?', '提示', {
              confirmButtonText: '确定',
              cancelButtonText: '取消',
              type: 'warning'
            }).then(() => {
              this._approve(type)
            })
          } else {
            this.$refs.remarkDialog.open()
          }
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
        isReject: type === 'reject',
        remark: this.remark,
        invoiceCompany: this.formData.invoiceCompany
      }
      type === 'reject'
        ? this.remarkLoading = true
        : this.contentLoading = true
      approveQuotationOrder(params).then(() => {
        this.$message({
          type: 'success',
          message:type === 'reject' 
            ? '驳回成功' 
            : (type === 'agree' ? '审核成功' : '收款成功')
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
  ::v-deep .el-form-item--mini.el-form-item, .el-form-item--small.el-form-item {
    margin-bottom: 5px;
  }
  &.uneditable {
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
    }
    /* 序列号查询表格 */
    .serial-wrapper {
      border-top: 2px solid #eee;
      .form-wrapper {
        
      }
      .serial-table-wrapper {
        height: 200px;
      }
    }
    /* 物料表格 */
    .material-wrapper {
      .form-wrapper {
        ::v-deep .el-input-number {
          width: 100% !important;
        }
        .material-table-wrapper {
          overflow: hidden;
          white-space: nowrap;
        }
        .icon-item {
          cursor: pointer;
        }
      }
    }
    /* 预览表格 */
    .preview-wrapper {
      margin-top: 10px;
      .preview-list-wrapper {
        .preview-item {
          margin-bottom: 10px;
          .serial-info-wrapper {
            .input-item {
              width: 150px;
            }
            .total-money {
              flex: 1;
              height: 28px;
              line-height: 28px;
              font-size: 16px;
              font-weight: bold;
              text-align: right;
            }
          }
          .material-table-wrapper {
            max-height: 100px;
          }
        }
      }
    }
    /* 报价单总价格 */
    .quotation-total-money {
      margin-top: 4px;
      padding: 1px 0;
      font-size: 17px;
      font-weight: bold;
      border-top: 1px solid;
      border-bottom: 1px solid;
      p {
        text-align: right;
      }
    }
    /* 物料汇总表格 */
    .material-summary-wrapper {
      margin-top: 10px;
      .material-summary-table {
        height: auto !important;
      }
      /* 物料汇总金额样式 */
      .money-wrapper { 
        justify-content: flex-end;
        font-size: 17px;
        font-weight: bold;
        p  {
          margin-left: 10px;
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
