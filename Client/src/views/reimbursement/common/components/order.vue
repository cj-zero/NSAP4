<template>
  <div class="order-wrapper">
    <el-form
      :model="formData"
      ref="form"
      class="my-form-wrapper"
      :disabled="disabled"
      :label-width="labelWidth"
      size="mini"
      :label-position="labelPosition"
      :show-message="false"
    >
      <!-- 普通控件 -->
      <el-row 
        type="flex" 
        v-for="(config, index) in normalConfig"
        :key="index">
        <el-col 
          :span="item.col"
          v-for="item in config"
          :key="item.prop"
        >
          <el-form-item :label="item.label" 
            :prop="item.prop"
            :rules="rules[item.prop] || { required: false }">
            <template v-if="item.label === '总金额'">
              ￥{{ totalMoney | toThousands }}
            </template>
            <template v-else-if="!item.type">
              <el-input 
                v-model="formData[item.prop]" 
                :style="{ width: item.width + 'px' }"
                :maxlength="item.maxlength"
                :disabled="item.disabled"
                @focus="customerFocus(item.prop) || noop"
                :readonly="item.readonly"
                >
                <i :class="item.icon" v-if="item.icon"></i>
              </el-input>
            </template>
            <template v-else-if="item.type === 'select'">
              <el-select 
                clearable
                :style="{ width: item.width + 'px' }"
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
            <template v-else-if="item.type === 'date'">
              <el-date-picker
                :disabled="item.disabled"
                :style="{ width: item.width + 'px' }"
                :value-format="item.valueFormat || 'yyyy-MM-dd'"
                :type="item.dateType || 'date'"
                :placeholder="item.placeholder"
                v-model="formData[item.prop]"
              ></el-date-picker>
            </template>
            <template v-else-if="item.type === 'button'">
              <el-button 
                type="primary" 
                style="width: 157px;" 
                @click="item.handleClick(formData.serviceOrderId)"
                :loading="reportBtnLoading">{{ item.btnText }}</el-button>
            </template>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
    <el-row type="flex" class="upload-wrapper">
      <span class="upload-title">上传附件</span>
      <upLoadFile 
        :disabled="!ifFormEdit"
        @get-ImgList="getFileList" 
        uploadType="file" 
        ref="uploadFile" 
        :maxSize="maxSize"
        :fileList="formData.attachmentsFileList || []"
        @deleteFileList="deleteFileList"></upLoadFile>
    </el-row>
    <!-- 出差 -->
    <div class="form-item-wrapper">
      <el-button v-if="ifShowTravel" @click="showForm(formData.reimburseTravellingAllowances, 'ifShowTravel')">添加出差补贴</el-button>
      <el-form 
        v-else
        ref="travelForm" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
        :disabled="!ifFormEdit"
      >
        <p class="title-wrapper">出差补贴</p>
        <el-table 
          border
          :data="formData.reimburseTravellingAllowances"
          @cell-click="onTravelCellClick"
          max-height="300px"
        >
          <!-- <el-table-column label="出差补贴" header-align="center"> -->
            <el-table-column
              v-for="item in travelConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :fixed="item.fixed"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseTravellingAllowances.' + scope.$index + '.'+ item.prop"
                    :rules="travelRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :disabled="item.disabled"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseTravellingAllowances.' + scope.$index + '.'+ item.prop"
                    :rules="travelRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :type="item.type" :min="0" :disabled="item.disabled" @input="onInput"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'reimburseTravellingAllowances.' + scope.$index + '.'+ item.prop"
                    :rules="travelRules[item.prop] || { required: false }"
                  >
                    <el-select
                      v-model="scope.row[item.prop]"
                    >
                      <el-option
                        v-for="optionItem in item.options"
                        :key="optionItem.label"
                        :value="optionItem.value"
                        :label="optionItem.label"
                      >
                      </el-option>
                    </el-select>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      :key="iconItem.icon"
                      :class="iconItem.icon" 
                      class="icon-item"
                      @click="iconItem.handleClick(scope, formData.reimburseTravellingAllowances, 'travel', iconItem.operationType)">
                    </i>
                  </template>
                </template>
              </template>
            </el-table-column>
          <!-- </el-table-column> -->
        </el-table>
        <p class="total-money">总金额: ￥{{ travelTotalMoney }}</p>
      </el-form>
    </div>

  <!-- 交通 -->

    <div class="form-item-wrapper">
      <el-button v-if="ifShowTraffic" @click="showForm(formData.reimburseFares, 'ifShowTraffic')">添加交通费用</el-button>
      <el-form 
        v-else
        ref="trafficForm" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
        :disabled="!ifFormEdit"
      >
        <p class="title-wrapper">交通费用</p>
        <el-table 
          border
          :data="formData.reimburseFares"
          max-height="300px"
          @cell-click="onTrafficCellClick"
        >
          <!-- <el-table-column label="交通费用" header-align="center"> -->
            <el-table-column
              v-for="item in trafficConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :width="item.width"
              :fixed="item.fixed"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                    :rules="trafficRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :disabled="item.disabled"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                    :rules="trafficRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0" @input="onInput" @focus="onFocus(item.prop)"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                    :rules="trafficRules[item.prop] || { required: false }"
                  >
                    <el-select
                      v-model="scope.row[item.prop]"
                    >
                      <el-option
                        v-for="optionItem in item.options"
                        :key="optionItem.label"
                        :value="optionItem.value"
                        :label="optionItem.label"
                      >
                      </el-option>
                    </el-select>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'upload'">   
                  <upLoadFile  
                    :disabled="!ifFormEdit"
                    @get-ImgList="getTrafficList" 
                    :limit="item.prop === 'invoiceAttachment' ? 1 : 100" 
                    uploadType="file" 
                    ref="trafficUploadFile" 
                    :prop="item.prop" 
                    :index="scope.$index"
                    :isReimburse="true"
                    @identifyInvoice="trafficIdentifyInvoice"
                    @deleteFileList="deleteFileList"
                    :fileList="
                      formData.reimburseFares[scope.$index] 
                        ?
                          (item.prop === 'invoiceAttachment' 
                            ? formData.reimburseFares[scope.$index].invoiceFileList 
                            : formData.reimburseFares[scope.$index].otherFileList
                          )
                        : []
                  ">
                  </upLoadFile>
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      :key="iconItem.icon"
                      :class="iconItem.icon" 
                      class="icon-item"
                      @click="iconItem.handleClick(scope, formData.reimburseFares, 'traffic', iconItem.operationType)">
                    </i>
                  </template>
                </template>
              </template>
            </el-table-column>
          <!-- </el-table-column> -->
        </el-table>
        <p class="total-money">总金额: ￥{{ trafficTotalMoney }}</p>
      </el-form>
    </div>
    
  <!-- 住宿 -->
    <div class="form-item-wrapper">
      <el-button v-if="ifShowAcc" @click="showForm(formData.reimburseAccommodationSubsidies, 'ifShowAcc')">添加住宿补贴</el-button>
      <el-form 
      v-else
      ref="accForm" 
      :model="formData" 
      size="mini" 
      :show-message="false"
      class="form-wrapper"
      :disabled="!ifFormEdit"
      >
        <p class="title-wrapper">住房补贴</p>
        <el-table 
          border
          :data="formData.reimburseAccommodationSubsidies"
          max-height="300px"
          @cell-click="onAccCellClick"
        >
          <!-- <el-table-column label="住房补贴" header-align="center"> -->
            <el-table-column
              v-for="item in accommodationConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :width="item.width"
              :fixed="item.fixed"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseAccommodationSubsidies.' + scope.$index + '.'+ item.prop"
                    :rules="accRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :disabled="item.disabled" @change="onChange"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseAccommodationSubsidies.' + scope.$index + '.'+ item.prop"
                    :rules="accRules[item.prop] || { required: false }"
                  >
                    <el-input 
                      v-model="scope.row[item.prop]" 
                      :type="item.type" 
                      :disabled="item.disabled" 
                      :min="0" 
                      @change="onChange"
                      @blur="onBlur"
                      @input="onInput"
                      @focus="onFocus(item.prop)"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'reimburseAccommodationSubsidies.' + scope.$index + '.'+ item.prop"
                    :rules="accRules[item.prop] || { required: false }"
                  >
                    <el-select
                      v-model="scope.row[item.prop]"
                    >
                      <el-option
                        v-for="optionItem in item.options"
                        :key="optionItem.label"
                        :value="optionItem.value"
                        :label="optionItem.label"
                      >
                      </el-option>
                    </el-select>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'upload'">
                  <upLoadFile  
                    :disabled="!ifFormEdit"
                    @get-ImgList="getAccList" 
                    :limit="item.prop === 'invoiceAttachment' ? 1 : 100" 
                    uploadType="file" 
                    ref="accUploadFile" 
                    :prop="item.prop" 
                    :index="scope.$index"
                    :isReimburse="true"
                    @identifyInvoice="accIdentifyInvoice"
                    @deleteFileList="deleteFileList"
                    :fileList="
                      formData.reimburseAccommodationSubsidies[scope.$index] 
                        ?
                          (item.prop === 'invoiceAttachment' 
                            ? formData.reimburseAccommodationSubsidies[scope.$index].invoiceFileList 
                            : formData.reimburseAccommodationSubsidies[scope.$index].otherFileList
                          )
                        : []
                    ">
                  </upLoadFile>
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      :key="iconItem.icon"
                      :class="iconItem.icon" 
                      class="icon-item"
                      @click="iconItem.handleClick(scope, formData.reimburseAccommodationSubsidies, 'accommodation', iconItem.operationType)">
                    </i>
                  </template>
                </template>
              </template>
            </el-table-column>
          <!-- </el-table-column> -->
        </el-table>
        <p class="total-money">总金额: ￥{{ accTotalMoney }}</p>
      </el-form>
    </div>

  <!-- 其它 -->

    <div class="form-item-wrapper">
      <el-button v-if="ifShowOther" @click="showForm(formData.reimburseOtherCharges, 'ifShowOther')">添加其他费用</el-button>
      <el-form 
        v-else
        ref="otherForm" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
        :disabled="!ifFormEdit"
      >
        <p class="title-wrapper">其他费用</p>
        <el-table 
         border
          :data="formData.reimburseOtherCharges"
          max-height="300px"
          @cell-click="onOtherCellClick"
        >
          <!-- <el-table-column label="其他费用" header-align="center"> -->
            <el-table-column
              v-for="item in otherConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :width="item.width"
              :fixed="item.fixed"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseOtherCharges.' + scope.$index + '.'+ item.prop"
                    :rules="otherRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :disabled="item.disabled"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseOtherCharges.' + scope.$index + '.'+ item.prop"
                    :rules="otherRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0" @focus="onFocus(item.prop)" @input="onInput"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'reimburseOtherCharges.' + scope.$index + '.'+ item.prop"
                    :rules="otherRules[item.prop] || { required: false }"
                  >
                    <el-select
                      v-model="scope.row[item.prop]"
                    >
                      <el-option
                        v-for="optionItem in item.options"
                        :key="optionItem.label"
                        :value="optionItem.value"
                        :label="optionItem.label"
                      >
                      </el-option>
                    </el-select>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'upload'">
                  <upLoadFile  
                    :disabled="!ifFormEdit"
                    @get-ImgList="getOtherList" 
                    :limit="item.prop === 'invoiceAttachment' ? 1 : 100" 
                    uploadType="file" 
                    ref="otherUploadFile" 
                    :prop="item.prop"
                    :index="scope.$index"
                    :isReimburse="true"
                    @identifyInvoice="otherIdentifyInvoice"
                    @deleteFileList="deleteFileList"
                    :fileList="
                      formData.reimburseOtherCharges[scope.$index] 
                        ?
                          (item.prop === 'invoiceAttachment' 
                            ? formData.reimburseOtherCharges[scope.$index].invoiceFileList 
                            : formData.reimburseOtherCharges[scope.$index].otherFileList
                          )
                        : []
                  "></upLoadFile>                   
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      :key="iconItem.icon"
                      :class="iconItem.icon" 
                      class="icon-item"
                      @click="iconItem.handleClick(scope, formData.reimburseOtherCharges, 'other', iconItem.operationType)">
                    </i>
                  </template>
                </template>
              </template>
            </el-table-column>
          <!-- </el-table-column> -->
        </el-table>
        <p class="total-money">总金额: ￥{{ otherTotalMoney }}</p>
      </el-form>
    </div> 
    <!-- 操作记录 -->
    <template v-if="this.title !== 'create' && this.title !== 'edit'">
      <el-table 
        :data="formData.reimurseOperationHistories"
        border
        max-height="300px"
      >
        <el-table-column label="操作记录" prop="action"></el-table-column>
        <el-table-column label="操作人" prop="createUser"></el-table-column>
        <el-table-column label="操作时间" prop="createTime"></el-table-column>
        <el-table-column label="审批时长" prop="intervalTime"></el-table-column>
        <el-table-column label="审批结果" prop="approvalResult"></el-table-column>
        <el-table-column label="备注" prop="remark"></el-table-column>
      </el-table>
    </template>
    <!-- 客户选择列表 -->
    <my-dialog 
      ref="customerDialog" 
      width="800px" 
      :mAddToBody="true" 
      :appendToBody="true"
      :btnList="customerBtnList"
      :onClosed="closeDialog">
      <common-table 
        ref="customerTable"
        maxHeight="500px"
        :data="customerInfoList"
        :columns="customerColumns"
      ></common-table>
      <pagination
        v-show="customerTotal > 0"
        :total="customerTotal"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="customerCurrentChange"
      />
    </my-dialog>
    <!-- 选择导入费用 -->
    <my-dialog 
      ref="costDialog" 
      width="800px" 
      :mAddToBody="true" 
      :appendToBody="true"
      :btnList="costBtnList"
      :onClosed="closeCostDialog">
      <common-table 
        ref="costTable"
        maxHeight="500px"
        :data="costData"
        :columns="costColumns"
        :selectedList="selectedList"
      ></common-table>
      <pagination
        v-show="costTotal > 0"
        :total="costTotal"
        :page.sync="listQueryCost.page"
        :limit.sync="listQueryCost.limit"
        @pagination="costCurrentChange"
      />
    </my-dialog>
    <!-- 完工报告 -->
    <my-dialog
      width="983px"
      title="服务行为报告单"
      ref="reportDialog"
      :mAddToBody="true" 
      :appendToBody="true"
      @closed="resetReport">
      <Report :data="reportData" ref="report"/>
    </my-dialog>
    <!-- 确认审批弹窗 -->
    <my-dialog
      ref="approve"
      :title="remarkTitle"
      :mAddToBody="true" 
      :appendToBody="true"
      :btnList="remarkBtnList"
      :closed="onApproveClose"
      v-loading="remarkLoading"
      width="350px">
      <remark ref="remark" @input="onRemarkInput"></remark>
    </my-dialog>
  </div>
</template>

<script>
import { addOrder, getOrder, updateOrder, approve, isSole } from '@/api/reimburse'
import { getList } from '@/api/reimburse/mycost'
import upLoadFile from "@/components/upLoadFile";
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
import Report from './report'
import Remark from './remark'
import { toThousands } from '@/utils/format'
import { findIndex } from '@/utils/process'
import { deepClone } from '@/utils'
import { travelRules, trafficRules, accRules, otherRules } from '../js/customerRules'
// import { EXPENSE_CATEGORY, RESPONSIBILITY_TYPE, RELATION_TYPE } from '../js/type'
import { customerColumns, costColumns } from '../js/config'
import { noop } from '@/utils/declaration'
import { categoryMixin, reportMixin, attachmentMixin } from '../js/mixins'
import { REIMBURSE_TYPE_MAP, IF_SHOW_MAP, REMARK_TEXT_MAP } from '../js/map'
export default {
  inject: ['parentVm'],
  mixins: [categoryMixin, reportMixin, attachmentMixin],
  components: {
    upLoadFile,
    Pagination,
    MyDialog,
    CommonTable,
    Report,
    Remark
  },
  props: {
    title: {
      type: String,
      default: ''
    },
    customerInfo: {
      type: Object,
      default () {
        return {}
      }
    },
    detailData: {
      type: Object,
      default () {
        return {}
      }
    },
    categoryList: {
      type: Array,
      default () {
        return []
      }
    }
  },
  data () {
    return {
      ifShowTraffic: true, // 是否展示交通补贴表格， 以下类似
      ifShowOther: true,
      ifShowAcc: true,
      ifShowTravel: true,
      currentIndex: 0, // 用来标记当前点击单元格所在表格中的索引值
      currentProp: '', // 当前选中的单元格的property, 对应table数据的key值
      currentLabel: '', // 当前选中的单元格的property, 对应table数据的label值
      currentRow: '', // 当前选中的
      tableType: '', // 用来判断当前被点击的表格类型
      maxSize: 100, // 文件大小
      customerBtnList: [
        { btnText: '确认', handleClick: this.confirm },
        { btnText: '取消', handleClick: this.closeDialog }
      ],
      costBtnList: [
        { btnText: '确认', handleClick: this.importConfirm },
        { btnText: '取消', handleClick: this.closeCostDialog }
      ],
      formData: { // 表单参数
        id: '',
        userName: '',
        createUserId: '',
        orgName: '',
        position: '',
        serviceOrderId: '',
        serviceOrderSapId: '',
        terminalCustomerId: '',
        terminalCustomer: '',
        shortCustomerName: '',
        becity: '',
        destination: '',
        businessTripDate: '',
        endDate: '',
        reimburseType: '',
        reimburseTypeText: '',
        projectName: '',
        remburseStatus: '',
        fromTheme: '',
        fillDate: '',
        report: '',
        bearToPay: '',
        responsibility: '',
        serviceRelations: '',
        payTime: '',
        remark: '',
        totalMoney: 0,
        attachmentsFileList: [],
        reimburseAttachments: [],
        reimburseTravellingAllowances: [],
        reimburseFares: [],
        reimburseAccommodationSubsidies: [],
        reimburseOtherCharges: [],
        reimurseOperationHistories: [], // 操作记录 我的提交不可见
        isDraft: false, // 是否是草稿
        delteReimburse: [], // 需要删除的行数据
        fileId: [], // 需要删除的附件ID
      },
      labelWidth: '80px',
      disabled: false,
      labelPosition: 'right',
      // formConfig,
      limit: 8,
      travelRules,
      trafficRules,
      accRules,
      otherRules,
      deleteList: [], // 删除表单列项
      customerColumns, // 用户列表表格配置
      customerInfoList: [], // 用户信息列表
      customerTotal: 0, // 用户列表总数
      listQuery: { // 用户列表的查询参数
        page: 1,
        limit: 30
      },
      costColumns,
      costTotal: 0, // 费用列表总数
      costData: [], // 费用列表信息
      listQueryCost: {
        page: 1,
        limit: 30
      },
      remarkBtnList: [
        { btnText: '确认', handleClick: this.approve },
        { btnText: '取消', handleClick: this.closeRemarkDialog }
      ],
      selectedList: [], // 费用列表导出的数据，用来后续判断导出列表中是否可选
      remarkType: '', // 
      remarkText: '', // 弹窗备注
      remarkLoading: false
    }
  },
  watch: {
    customerInfo: {
      immediate: true,
      deep: true,
      handler (val) {
        console.log('immediate watch')
        this.formData.createUserId = val.createUserId
        this.formData.userName = val.userName
        this.formData.orgName = val.orgName
        // this.formData.becity = val.becity
        // this.formData.businessTripDate = val.businessTripDate
        // this.formData.endDate = val.endDate
        // this.formData.destination = val.destination
        if (this.title === 'create') { // 只有才新建的时候才需要修改服务ID
          this._getCustomerInfo()    
        }
        if (this.title === 'create' || this.title === 'edit') { // 只有在create或者edit的时候，才可以导入费用模板
          this._getCostList() // 获取费用模板
        }
      }
    },
    detailData: {
      immediate: true,
      // deep: true,
      handler (val) {
        let { 
          reimburseTravellingAllowances: travel,
          reimburseFares: traffic,
          reimburseAccommodationSubsidies: acc,
          reimburseOtherCharges: other 
        } = val
        if (travel && travel.length) {
          this.ifShowTravel = false
        }
        if (traffic && traffic.length) {
          this.ifShowTraffic = false
        }
        if (acc && acc.length) {
          this.ifShowAcc = false
        }
        if (other && other.length) {
          this.ifShowOther = false
        }
        this.formData = Object.assign({}, this.formData, val)
      }
    },
    totalMoney (val) {
      this.formData.totalMoney = val
    },
    formData: {
      deep: true,
      handler () {
        console.log(this.formData, 'formData', this.formData.serviceRelations)
      }
    },
    'formData.serviceRelations' (val) {
      console.log(val, 'serviceRelations')
    }
  },
  computed: {
    ifFormEdit () { 
      return this.title === 'create' || this.title === 'edit'
    },
    travelTotalMoney () {
      let { reimburseTravellingAllowances } = this.formData
      if (reimburseTravellingAllowances.length) {
        return this.getTotal(reimburseTravellingAllowances)
      }
      return 0
    },
    trafficTotalMoney () {
      let { reimburseFares } = this.formData
      if (reimburseFares.length) {
        return this.getTotal(reimburseFares)
      }
      return 0
    },
    accTotalMoney () {
      let { reimburseAccommodationSubsidies } = this.formData
      if (reimburseAccommodationSubsidies.length) {
        return this.getTotal(reimburseAccommodationSubsidies)
      }
      return 0
    },
    otherTotalMoney () {
      let { reimburseOtherCharges } = this.formData
      if (reimburseOtherCharges.length) {
        console.log(this)
        return this.getTotal(reimburseOtherCharges)
      }
      return 0
    },
    totalMoney () {
      // let result = 0
      // let { 
      //   reimburseTravellingAllowances, 
      //   reimburseFares, 
      //   reimburseAccommodationSubsidies,
      //   reimburseOtherCharges 
      // } = this.formData
      // if (reimburseTravellingAllowances.length) {
      //   result += this.getTotal(reimburseTravellingAllowances)
      // }
      // if (reimburseFares.length) {
      //   result += this.getTotal(reimburseFares)
      // }
      // if (reimburseAccommodationSubsidies.length) {
      //   result += this.getTotal(reimburseAccommodationSubsidies)
      // }
      // if (reimburseOtherCharges.length) {
      //   result += this.getTotal(reimburseOtherCharges)
      // }
      return this.travelTotalMoney + this.trafficTotalMoney + this.accTotalMoney + this.otherTotalMoney
    },
    normalConfig () {
      let noneSlotConfig = this.formConfig.filter(item => item.type !== 'slot')
      let result = [], j = 0
      for (let i = 0; i < noneSlotConfig.length; i++) {
        
        if (!result[j]) {
          result[j] = []
        }
        result[j].push(noneSlotConfig[i])
        if (noneSlotConfig[i].isEnd) {
          j++
        }
      }
      return result
    },
    rules () { // 报销单上层表单规则
      // console.log('rules roleName', this.isCustomerSupervisor)
      return {
        serviceOrderSapId: [ { required: true } ],
        reimburseType: [ { required: true, trigger: ['change', 'blur'] } ],
        projectName: [ { required: true, trigger: ['change', 'blur'] } ],
        bearToPay: [ { required: this.isCustomerSupervisor, trigger: ['change', 'blur']} ],
        responsibility: [ { required: true, trigger: ['change', 'blur'] } ],
        serviceRelations: [ { required: true, trigger: ['change', 'blur'] } ]
      }
    },
    remarkTitle () {
      return `确认${REMARK_TEXT_MAP[this.remarkType]}此次报销`
    }
  },
  methods: {
    noop () {
      noop() 
    },
    getTotal (data) { // 获取总金额
      let result = 0
      result += data.reduce((prev, next) => {
        return prev + parseFloat(String(next.totalMoney || next.money))
      }, 0)
      // console.log(this.isValidNumber(result), result, 'result')
      return this.isValidNumber(result) ? result : 0
    },
    setTravelMoney () {
      // 以R或者M开头都是65
      return /^[R|M]/i.test(this.formData.orgName) ? '65' : '50'
    },
    showForm (data, type) { // 展示表格
      if (!this.ifFormEdit) return
      switch (type) {
        case 'ifShowTravel':
          data.push({
            id: '',
            days: '',
            money: this.setTravelMoney(),
            remark: '',
          })
          break
        case 'ifShowTraffic':
          data.push({
            id: '',
            trafficType: '',
            transport: '',
            from: '',
            to: '',
            money: '',
            maxMoney: '',
            remark: '',
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: [],
            invoiceFileList: [],
            otherFileList: []
          })
          break
        case 'ifShowAcc':
          data.push({
            id: '',
            days: '',
            money: '',
            totalMoney: '',
            maxMoney: '',
            remark: '',
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: [],
            invoiceFileList: [],
            otherFileList: []
          })
          break
        case 'ifShowOther':
          data.push({
            id: '',
            expenseCategory: '',
            money: '',
            maxMoney: '',
            remark: '',
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: [],
            invoiceFileList: [],
            otherFileList: []
          })
      }
      this[type] = false
    },
    processIcon (icon, index, data) { // 处理上下移动图标的展示
      return !(
        (icon === 'el-icon-top' && index === 0) ||
        (icon === 'el-icon-bottom' && index === data.length - 1) || 
        (icon === 'el-icon-delete' && index === 0) 
      )
    },
    async validate (ref ,data) {
      let isValid = true
      if (!data) {
        isValid = await this.$refs[ref].validate()
        return isValid
      } else {
        console.log(this.formData[data], 'formData')
        let ifInvoiceAttachment
        // if (this.title === 'create') { // 创建的时候
        //   ifInvoiceAttachment = this.formData[data].every(item => item.invoiceAttachment && item.invoiceAttachment.length)
        // } else {
          // 编辑的时候
        if (ref !== 'travelForm') {
          for (let i = 0; i < this.formData[data].length; i++) {
            ifInvoiceAttachment = true
            let { invoiceAttachment, invoiceFileList } = this.formData[data][i]
            console.log(invoiceAttachment, invoiceFileList, 'edit validate', data)
            if (invoiceFileList.length) { // 有可能是导入进来的数据(这个是没有新增的数据，跟普通新增的数据同样)，也有可能是已经新增过的数据
              // 新增过的数据reimburseId存在
              let ifDeleted = invoiceFileList[0].reimburseId
                ? this.formData.fileId.includes(invoiceFileList[0].id) // 判断invoiceFileList是否已经删除
                : !(invoiceFileList[0].isAdd) // 判断当前文件的状态是不是删除(模板数据而言)
  
              // 如果用于回显的附件给删除了，则需要判断的invoiceAttachment数组是否有值
              ifInvoiceAttachment = ifDeleted
                ? Boolean(invoiceAttachment && invoiceAttachment.length)
                : true
              // 只有有一个是false 就直接break
              if (!ifInvoiceAttachment) break
            } else {
              ifInvoiceAttachment = Boolean(invoiceAttachment && invoiceAttachment.length)
            }
          }
        }
        // }
        let isValid = await this.$refs[ref].validate()
        console.log('valid', isValid, this.formData[data], ifInvoiceAttachment)
        return ref === 'travelForm' ? isValid : ifInvoiceAttachment && isValid
      }
    },
    getFileList (val) {
      let resultArr = this.createFileList(val, {
        reimburseType: 0,
        attachmentType: 1
      })
      this.formData.reimburseAttachments = resultArr
    },
    getTrafficList (val, prop, index) {
      let data = this.formData.reimburseFares
      let resultArr = []
      resultArr = this.createFileList(val, {
        reimburseType: 2,
        attachmentType: prop === 'invoiceAttachment' ? 2 : 1
      })
      this.$set(data[index], prop, resultArr)
    },
    getAccList (val, prop, index) {
      let data = this.formData.reimburseAccommodationSubsidies
      let resultArr = []
      resultArr = this.createFileList(val, {
        reimburseType: 3,
        attachmentType:  prop === 'invoiceAttachment' ? 2 : 1
      })
      this.$set(data[index],  prop, resultArr)
    },
    getOtherList (val, prop, index) {
      let data = this.formData.reimburseOtherCharges
      let resultArr = []
      resultArr = this.createFileList(val, {
        reimburseType: 4,
        attachmentType: prop === 'invoiceAttachment' ? 2 : 1
      })
      this.$set(data[index], prop, resultArr)
    },
    setCurrentIndex (data, row) {
      this.currentRow = row
      this.currentIndex = findIndex(data, item => item === row)
    },
    trafficIdentifyInvoice (invoiceNo, money, prop) { // 获取发票号码
      if (prop === 'invoiceAttachment') {
        let data = this.formData.reimburseFares
        let currentRow = data[this.currentIndex]
        currentRow.money = money
        currentRow.maxMoney = money
        currentRow.invoiceNumber = invoiceNo
        console.log(data[this.currentIndex], '识别 traffic')
      }
    },
    accIdentifyInvoice (invoiceNo, money, prop) {
      if (prop === 'invoiceAttachment') {
        let data = this.formData.reimburseAccommodationSubsidies
        let currentRow = data[this.currentIndex]
        currentRow.totalMoney = money
        currentRow.maxMoney = money
        currentRow.invoiceNumber = invoiceNo
        currentRow.money = (currentRow.totalMoney / (currentRow.days || 1)).toFixed(2)
        console.log(data[this.currentIndex], '识别 acc', currentRow.maxMoney, currentRow.money, currentRow.totalMoney)
      }
    },
    otherIdentifyInvoice (invoiceNo, money, prop) {
      if (prop === 'invoiceAttachment') {
        let data = this.formData.reimburseOtherCharges
        let currentRow = data[this.currentIndex]
        currentRow.money = money
        currentRow.maxMoney = money
        currentRow.invoiceNumber = invoiceNo
        console.log(data[this.currentIndex], '识别 other')
      }
    },
    onTravelCellClick (row, column) {
      this.setCurrentProp(column, row)
    },
    onTrafficCellClick (row, column) {
      console.log('cell click traffic')
      this.tableType = 'traffic' // 判断当前点击的是哪个表格
      this.setCurrentProp(column, row)
      this.setCurrentIndex(this.formData.reimburseFares, row)
    },
    onAccCellClick (row, column) {
      this.tableType = 'acc' // 判断当前点击的是哪个表格
      console.log('cell click')
      this.setCurrentProp(column, row)
      this.setCurrentIndex(this.formData.reimburseAccommodationSubsidies, row)
    },
    onOtherCellClick (row, column) {
      this.tableType = 'other' // 判断当前点击的是哪个表格
      this.setCurrentProp(column, row)
      this.setCurrentIndex(this.formData.reimburseOtherCharges, row)
    },
    setCurrentProp ({ label, property }) {
      this.currentLabel = label
      this.currentProp = property
    },
    onChange (value) { // 天数 总金额 计算
      console.log(value, 'on change')
      this.changeMoneyByDaysOrTotalMoney(value)
    },
    onBlur (e) {
      console.log('blur')
      let value = e.target.value
      this.changeMoneyByDaysOrTotalMoney(value)
    },
    changeMoneyByDaysOrTotalMoney (value) {
      if (!this.isValidNumber(value)) {
        return
      }
      if (this.currentProp === 'totalMoney' || this.currentProp === 'days') {
        let data = this.formData.reimburseAccommodationSubsidies[this.currentIndex]
        let { days, totalMoney } = data
        if (!days || !this.isValidNumber(days) || !totalMoney || !this.isValidNumber(totalMoney)) { // 如果天数没有填入,或者不符合规范则直接return
          return
        }
        this.$set(data, 'money', (totalMoney / days).toFixed(2))
        console.log(this.formData, 'formData')
      }
    },
    onInput (value) {
      // let value = e.target.value
      let data = this.selectTable() // 通过判断tableType来选择当前的表格数据
      let currentRow = data[this.currentIndex]
      let { invoiceFileList, invoiceAttachment, maxMoney, invoiceNumber } = currentRow
      if (
          (invoiceFileList.length && !this.formData.fileId.includes(invoiceFileList[0].id)) || // 存在回显的文件代表已经新增的，并且还没被删除过
          (invoiceAttachment.length && invoiceNumber)
      ) {
        if (this.currentProp === 'totalMoney' || this.currentProp === 'money') { // 只算修改totalMoney或者money字段
          console.log(maxMoney, value, 'enter ine')
          this.tableType === 'acc'
            ? currentRow.totalMoney = Math.min(parseFloat(value), maxMoney)
            : currentRow.money = Math.min(parseFloat(value), maxMoney)
        }
      }
    },
    onFocus (val) {
      this.currentProp = val
      console.log('focus', val)
    },
    selectTable () {
      let { reimburseFares, reimburseOtherCharges, reimburseAccommodationSubsidies } = this.formData
      return this.tableType === 'traffic'
        ? reimburseFares
        : this.tableType === 'acc'
          ? reimburseAccommodationSubsidies
          : reimburseOtherCharges
    },
    addAndCopy (scope, data, type, operationType) {
      if (!this.ifFormEdit) return
      console.log(scope.row, data, type, operationType, 'operationType') // 判断是新增还是复制
      let { row } = scope
      switch (type) {
        case 'traffic':
          data.push({
            id: '',
            trafficType: operationType === 'add' ? '' : row.trafficType,
            transport: operationType === 'add' ? '' : row.transport,
            from: operationType === 'add' ? '' : row.from,
            to: operationType === 'add' ? '' : row.to,
            money: operationType === 'add' ? '' : row.money,
            maxMoney: operationType === 'add' ? '' : row.maxMoney,
            remark: operationType === 'add' ? '' : row.remark,
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: [],
            invoiceFileList: [], // 用于回显
            otherFileList: [] // 用于回显
          })
          break
        case 'accommodation':
          data.push({
            id: '',
            days: operationType === 'add' ? '' : row.days,
            money: operationType === 'add' ? '' : row.money,
            totalMoney: operationType === 'add' ? '' : row.totalMoney,
            maxMoney: operationType === 'add' ? '' : row.maxMoney,
            remark: operationType === 'add' ? '' : row.remark,
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: [],
            invoiceFileList: [], // 用于回显
            otherFileList: [] // 用于回显
          })
          break
        case 'other':
          data.push({
            id: '',
            expenseCategory: operationType === 'add' ? '' : row.expenseCategory,
            money: operationType === 'add' ? '' : row.money,
            maxMoney: operationType === 'add' ? '' : row.maxMoney,
            remark: operationType === 'add' ? '' : row.remark,
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: [],
            invoiceFileList: [], // 用于回显
            otherFileList: [] // 用于回显
          })
      }
    },
    delete (scope, data, type) {
      if (!this.ifFormEdit) return
      let { id, invoiceFileList } = scope.row
      if (id) { // 说明已经新建过的,新建过的表格数据 invoceFileList 是一定存在的
        if (invoiceFileList && invoiceFileList.length) {
          if (invoiceFileList[0].reimburseId) { // 导入的数据reimburseId是为空的，所以不需要添加到delteReimburse中
            this.formData.delteReimburse.push({
              deleteId: id,
              reimburseType: REIMBURSE_TYPE_MAP[type]
            })
            console.log(this.formData.delteReimburse, 'deleterei')
          } else {
            let index = findIndex(this.selectedList, item => item.id === id) // 找到当前删除行 对应导入之后的数据列表的索引值
            if (index !== -1) {
              console.log('删除 模板')
              this.selectedList.splice(index, 1) // 删除后，让导入的表格回复对应的可选状态
            }
          }
        } else {
          this.formData.delteReimburse.push({
            deleteId: id,
            reimburseType: REIMBURSE_TYPE_MAP[type]
          })
          console.log(this.formData.delteReimburse, 'deleterei travel')
        }
      } 
      data.splice(scope.$index, 1)
      if (!data.length) {
        console.log(IF_SHOW_MAP[type], 'IF_SHOW_MAP[type]')
        this[IF_SHOW_MAP[type]] = true
      }
    },
    deleteFileList (file) {
      let { reimburseId, id } = file
      console.log(file, reimburseId, id, 'file')
      // 在编辑的时候，(针对)删除已经新增过的附件, 如果是删除导入的附件，ID会为'' ,直接略过
      console.log(id, 'deleteId')
      if (reimburseId) { // 删除新增过的
        this.formData.fileId.push(id)
        console.log(this.delteReimburse, 'deleteFileList')
      } else {
        // 删除导入的模板
        // this.importDelList.push(fileId)
        file.isAdd = false
        console.log(this.formData, 'import list')
      }
    },
    isValidNumber (val) { // 判断是否是有效的数字
      val = Number(val)
      return !isNaN(val) && val >= 0
    },
    hasTotalMoneyKey (data) { // 用来判断是不是住宿表格
      for (let i = 0; i < data.length; i++) {
        for (let key in data[i]) {
          if (key === 'totalMoney') {
            return true
          }
        }
      }
      return false
    },
    getSummaries ({ columns, data }) { // 金额合计
      const sums = []
      let hasTotalMoney = this.hasTotalMoneyKey(data)
      columns.forEach((column, index) => {
        if (index === 0) {
          sums[index] = '总金额'
          return
        }
        if ((column.property === 'money' && !hasTotalMoney) || column.property === 'totalMoney') {
          const values = data.map(item => Number(item[column.property]))
          if (values.every(value => {
            return !isNaN(value) && value >= 0
          })) {
            sums[index] = values.reduce((prev, curr) => {
              const value = Number(curr);
              if (!isNaN(value)) {
                return prev + curr;
              } else {
                return prev;
              }
            }, 0);
            sums[index] = '￥' + toThousands(sums[index]);
          }
        }
      })
      return sums
    },
    clearFile () { // 删除上传的文件
      this.$refs.uploadFile.clearFiles()
    },
    customerFocus (prop) {
      if (prop === 'serviceOrderSapId') {
        this.$refs.customerDialog.open()
      }
    },
    customerCurrentChange (val) {
      Object.assign(this.listQuery, val)
      this._getCustomerInfo()
    },
    _getCustomerInfo () {
      getOrder(this.listQuery).then(res => {
        let { data, count } = res
        this.customerInfoList = data
        this.customerInfoList.forEach(item => {
          item.radioKey = 'id'
        })
        this.customerTotal = count
      }).catch(() => {
        this.$message.erro('获取用户信息失败')
      })
    },
    closeDialog () {
      this.$refs.customerTable.resetRadio()
      this.$refs.customerDialog.close()
    },
    confirm () {
      let currentRow = this.$refs.customerTable.getCurrentRow()
      if (Object.keys(currentRow).length) {
        let { 
          terminalCustomerId, 
          terminalCustomer, 
          u_SAP_ID, 
          fromTheme, 
          id, 
          userId,
          becity,
          businessTripDate,
          endDate,
          destination } = currentRow
        console.log(currentRow, 'currentrOW')
        let formData = this.formData // 对报销人的信息进行赋值
        formData.terminalCustomerId = terminalCustomerId
        formData.terminalCustomer = terminalCustomer
        formData.serviceOrderId = id
        formData.serviceOrderSapId = u_SAP_ID
        formData.fromTheme = fromTheme
        formData.CreateUserId = userId
        formData.becity = becity
        formData.businessTripDate = businessTripDate
        formData.endDate = endDate
        formData.destination = destination
      }
      this.currentRow = {}
      this.closeDialog()
    },
    _getCostList () {
      console.log('getCOstlist')
      getList({
        ...this.listQueryCost
      }).then(res => {
        let { data, count } = res
        this.costTotal = count
        this.costData = data.map(item => {
          item.moneyText = toThousands(item.money)
          if (item.reimburseType === Number(3)) { // 住宿费
            item.moneyText = toThousands(item.totalMoney)
          }
          return item
        })
        console.log(this.total, this.toSavetableData, 'data')
      }).catch(() => {
        this.$message.error('获取费用列表失败')
      })
    },
    costCurrentChange (val) {
      Object.assign(this.listQueryCost, val)
      this._getCostList()
    },
    openCostDialog () { // 打开导入费用弹窗
      this.$refs.costDialog.open()
    },
    closeCostDialog () {
      this.$refs.costDialog.close()
      this.$refs.costTable.clearSelection()
    },
    importConfirm () { // 确认导入
      const selectList = this.$refs.costTable.getSelectionList()
      if (!selectList.length) {
        return this.$message({
          type: 'warning',
          message: '请选择费用模板'
        })
      }
      let invoiceNumberList = selectList.map(item => {
        console.log(typeof item.invoiceNumber)
        return item.invoiceNumber
      })
      isSole(invoiceNumberList).then(() => {
        this.selectedList.push(...selectList) // 将选择的数组push到selected中
        const cloneSelectList = deepClone(selectList) // 避免引用造成影响
        this._normalizeSelectList(cloneSelectList) // 因为这些导出的数据相当于新数据，所以需要将附件ID删除
        this._addToTable(cloneSelectList) // 根据报销类型的不同插入到不同的表中
        this.closeCostDialog()
      }).catch(() => {
        this.$message.error('发票号码验证失败')
      })
      console.log(isSole, 'isSole')
      // this.selectedList.push(...selectList) // 将选择的数组push到selected中
      // const cloneSelectList = deepClone(selectList) // 避免引用造成影响
      // this._normalizeSelectList(cloneSelectList) // 因为这些导出的数据相当于新数据，所以需要将附件ID删除
      // this._addToTable(cloneSelectList) // 根据报销类型的不同插入到不同的表中
      // this.closeCostDialog()
      // console.log(selectList, 'selectList', this.selectedList)
    },
    
    _normalizeSelectList (selectList) {
      console.log(deepClone(selectList), 'deepClone')
      this._buildAttachment(selectList, true)
    },
    _addToTable (selectList) {
      let trafficList = selectList.filter(item => item.reimburseType === Number(2))
      let accList = selectList.filter(item => item.reimburseType === Number(3))
      let otherList = selectList.filter(item => item.reimburseType === Number(4))
      if (trafficList.length) {
        this.ifShowTraffic = false
        this.formData.reimburseFares.push(...trafficList)
        console.log('traffic success', this.formData.reimburseFares)
      }
      if (accList.length) {
        this.ifShowAcc = false
        this.formData.reimburseAccommodationSubsidies.push(...accList)
        console.log('acc success', this.formData.reimburseAccommodationSubsidies)
      }
      if (otherList.length) {
        this.ifShowOther = false
        this.formData.reimburseOtherCharges.push(...otherList)
        console.log('otherList success', this.formData.reimburseOtherCharges)
      }
      console.log('addTable', selectList, trafficList, accList, otherList)
    },
    openRemarkDialog (type) { // 打开备注弹窗，二次确认
      this.remarkType = type
      this.$refs.approve.open()
    },
    closeRemarkDialog () {
      this.remarkType = ''
      this.$refs.remark.reset()
      this.$refs.approve.close()
    },
    onRemarkInput (val) {
      this.remarkText = val
    },
    onApproveClose () {
      this.remarkType = ''
      this.$refs.remark.reset()
    },
    resetInfo () {
      let { createUserId, userName, orgName } = this.formData
      this.$refs.form.clearValidate()
      this.$refs.form.resetFields()
      this.clearFile()
      this.ifShowTraffic = this.ifShowOther = this.ifShowAcc = this.ifShowTravel = true
      this.selectedList = []
      this.formData = { // 表单参数
        id: '',
        userName,
        createUserId,
        orgName,
        position: '',
        serviceOrderId: '',
        serviceOrderSapId: '',
        terminalCustomerId: '',
        terminalCustomer: '',
        shortCustomerName: '',
        becity: '',
        destination: '',
        businessTripDate: '',
        endDate: '',
        reimburseType: '',
        reimburseTypeText: '',
        projectName: '',
        remburseStatus: '',
        fromTheme: '',
        fillDate: '',
        report: '',
        bearToPay: '',
        responsibility: '',
        serviceRelations: '',
        payTime: '',
        remark: '',
        totalMoney: 0,
        attachmentsFileList: [],
        reimburseAttachments: [],
        reimburseTravellingAllowances: [],
        reimburseFares: [],
        reimburseAccommodationSubsidies: [],
        reimburseOtherCharges: [],
        isDraft: false, // 是否是草稿
        delteReimburse: [], // 需要删除的行数据
        fileId: [], // 需要删除的附件ID
      }    
    },
    addSerialNumber (data) { // 为表格的数据添加序号
      data.forEach((item, index) => {
        item.serialNumber = index + 1
      })
    },
    async checkData () { // 校验表单数据是否通过
      let isFormValid = true, isTravelValid = true, isTrafficValid = true, isAccValid = true, isOtherValid = true
      try {
        isFormValid = await this.validate('form')
        if (!this.ifShowTravel) {
          isTravelValid = await this.validate('travelForm', 'reimburseTravellingAllowances')
        }
        if (!this.ifShowTraffic) {
          isTrafficValid = await this.validate('trafficForm', 'reimburseFares')
        }
        if (!this.ifShowAcc) {
          isAccValid = await this.validate('accForm', 'reimburseAccommodationSubsidies')
        }
        if (!this.ifShowOther) {
          isOtherValid = await this.validate('otherForm', 'reimburseOtherCharges')
        }
        console.log('checkData', isFormValid, isTravelValid, isTrafficValid, isAccValid, isOtherValid)
        return isFormValid && isTrafficValid && isAccValid && isOtherValid && isTravelValid
      } catch (err) {
        console.log(err)
        // this.$message.error('请将必填项填写')
      }      
    },
    async submit (isDraft) { // 提交
      let { 
        reimburseAccommodationSubsidies, 
        reimburseOtherCharges, 
        reimburseFares,
        reimburseAttachments,
        attachmentsFileList
      } = this.formData
      this.formData.reimburseAttachments = [...reimburseAttachments, ...attachmentsFileList]
      this.mergeFileList(reimburseAccommodationSubsidies)
      this.mergeFileList(reimburseOtherCharges)
      this.mergeFileList(reimburseFares)
      this.addSerialNumber(reimburseAccommodationSubsidies)
      this.addSerialNumber(reimburseOtherCharges)
      this.addSerialNumber(reimburseFares)
      let isValid = await this.checkData()
      console.log('submit', isValid, isDraft)
      if (!isValid) {
        return Promise.reject({ message: '请将必填项填写完整' })
      }
      this.formData.isDraft = isDraft ? true : false
      console.log(this.formData, 'formData', addOrder)
      return addOrder(this.formData)
    },
    async updateOrder (isDraft) { // 编辑
      console.log(isDraft, 'isDraft')
      let { 
        reimburseAccommodationSubsidies, 
        reimburseOtherCharges, 
        reimburseFares,
        reimburseAttachments,
        attachmentsFileList
      } = this.formData
      this.formData.reimburseAttachments = [...reimburseAttachments, ...attachmentsFileList]
      this.mergeFileList(reimburseAccommodationSubsidies)
      this.mergeFileList(reimburseOtherCharges)
      this.mergeFileList(reimburseFares)
      this.addSerialNumber(reimburseAccommodationSubsidies)
      this.addSerialNumber(reimburseOtherCharges)
      this.addSerialNumber(reimburseFares)
      let isValid = await this.checkData()
      console.log('submit', isValid, isDraft)
      if (!isValid) {
        return Promise.reject({ message: '请将必填项填写完整' })
      }
      this.formData.isDraft = isDraft ? true : false
      console.log(this.formData, 'update formData')
      return updateOrder(this.formData)
    },
    approve () {
      this._approve()
    },
    async _approve () {
      console.log('exec approve')
      this.$refs.form.validate(isValid => {
        console.log(isValid, 'ISvALID')
        if (!isValid) {
          return this.$message.error('请将必选项填写')
        } 
        console.log(this.remarkType, 'remarkType')
        let data = this.formData
        let params = {
          id: data.id, 
          shortCustomerName: data.shortCustomerName, 
          reimburseType: data.reimburseType, 
          projectName: data.projectName, 
          bearToPay: data.bearToPay, 
          responsibility: data.responsibility,
          remark: this.remarkText,
          flowInstanceId: data.flowInstanceId, // 流程ID
          isReject: this.remarkType === 'reject'
        }
        console.log(approve, params, this.parentVm, 'parentVm')
        this.remarkLoading = true
        approve(params).then(() => {
          this.$message({
            type: 'success',
            message: this.remarkType === 'reject' ? '驳回成功' : '操作成功'
          })
          this.remarkLoading = false
          this.closeRemarkDialog()
          this.parentVm._getList()
          this.parentVm.closeDialog()
        }).catch(() => {
          this.remarkLoading = false
          this.$message.error('操作失败')
        })
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
.order-wrapper {
  max-height: 600px;
  overflow-y: auto;
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
  &::-webkit-scrollbar {
    display: none;
  }
  ::v-deep .el-form-item--mini.el-form-item, .el-form-item--small.el-form-item {
    margin-bottom: 5px;
  }
  .upload-wrapper {
    margin: 20px 0;
    .upload-title {
      width: 80px;
      text-align: left;
    }
  }
  .form-item-wrapper {
    margin-bottom: 20px;
    .title-wrapper {
      width: 100%;
      text-align: center;
      height: 30px;
      line-height: 30px;
      color: #606266;
      font-size: 16px;
      background-color: #f5f7fa;
    }
    .total-money {
      height: 30px;
      line-height: 30px;
      color: #606266;
      text-indent: 10px;
      font-size: 12px;
      background-color: #f5f7fa;
    }
  }
  .form-wrapper {
    ::v-deep .el-form-item--mini {
      margin-bottom: 0;
    }
    ::v-deep .el-table__fixed-right, .el-table__fixed {
      background-color: #fff;
    }
  }
  .icon-item {
    font-size: 17px;
    margin: 5px;
    cursor: pointer;
  }
}
</style>