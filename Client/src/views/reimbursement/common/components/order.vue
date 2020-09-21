<template>
  <div class="order-wrapper">
    <el-form
      :model="formData"
      ref="form"
      class="my-form-wrapper"
      :disabled="disabled"
      :label-width="labelWidth"
      size="mini"
      :label-position="labelposition"
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
          <el-form-item :label="item.label">
            <template v-if="!item.type">
              <el-input 
                v-model="formData[item.prop]" 
                :style="{ width: item.width + 'px' }"
                :maxlength="item.maxlength"
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
                :style="{ width: item.width + 'px' }"
                :value-format="item.valueFormat || 'yyyy-MM-dd'"
                :type="item.dateType || 'date'"
                :placeholder="item.placeholder"
                v-model="formData[item.prop]"
              ></el-date-picker>
            </template>
            <template v-else-if="item.type === 'button'">
              <el-button @click="handleClick">{{ item.btnText }}</el-button>
            </template>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
    <el-row type="flex" class="upload-wrapper">
      <span class="upload-title">上传附件</span>
      <upLoadFile @get-ImgList="getFileList" :limit="limit" uploadType="file" ref="uploadFile"></upLoadFile>
    </el-row>
    <!-- 出差 -->
    <div class="form-item-wrapper">
      <el-button v-if="ifShowTravel" @click="showForm('ifShowTravel')">添加出差补贴</el-button>
      <el-form 
        v-else
        ref="travelForm" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
      >
        <p class="title-wrapper">出差补贴</p>
        <el-table 
          border
          :data="formData.reimburseTravellingAllowances"
          :summary-method="getSummaries"
          show-summary
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
                    <el-input v-model="scope.row[item.prop]"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseTravellingAllowances.' + scope.$index + '.'+ item.prop"
                    :rules="travelRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :type="item.type" :min="0"></el-input>
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
                      v-if="processIcon(iconItem.icon, scope.$index, formData.reimburseTravellingAllowances)"
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
      </el-form>
    </div>

  <!-- 交通 -->

    <div class="form-item-wrapper">
      <el-button v-if="ifShowTraffic" @click="showForm('ifShowTraffic')">添加交通费用</el-button>
      <el-form 
        v-else
        ref="trafficForm" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
      >
        <p class="title-wrapper">交通费用</p>
        <el-table 
          border
          :data="formData.reimburseFares"
          :summary-method="getSummaries"
          show-summary
          max-height="300px"
          @cell-click="onTrafficCellClick"
          @cell-mouse-enter="onTrafficCellMouseEnter"
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
                    <el-input v-model="scope.row[item.prop]"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                    :rules="trafficRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0"></el-input>
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
                  @get-ImgList="getTrafficList" 
                  :limit="limit"
                   uploadType="file" 
                   ref="trafficUploadFile" 
                   :prop="item.prop" 
                   :fileList="scope.row[item.prop]">
                  </upLoadFile>
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      v-if="processIcon(iconItem.icon, scope.$index, formData.reimburseFares)"
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
      </el-form>
    </div>
    

  <!-- 住宿 -->

    <div class="form-item-wrapper">
      <el-button v-if="ifShowAcc" @click="showForm('ifShowAcc')">添加住宿补贴</el-button>
      <el-form 
      v-else
      ref="accForm" 
      :model="formData" 
      size="mini" 
      :show-message="false"
      class="form-wrapper"
      >
        <p class="title-wrapper">住房补贴</p>
        <el-table 
          border
          :data="formData.reimburseAccommodationSubsidies"
          :summary-method="getSummaries"
          show-summary
          max-height="300px"
          @cell-mouse-enter="onAccCellMouseEnter"
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
                    <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0" @change="onChange"></el-input>
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
                    @get-ImgList="getAccList" 
                    :limit="limit" 
                    uploadType="file" 
                    ref="accUploadFile" 
                    :prop="item.prop" 
                    :fileList="scope.row[item.prop]">
                  </upLoadFile>
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      v-if="processIcon(iconItem.icon, scope.$index, formData.reimburseAccommodationSubsidies)"
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
      </el-form>
    </div>

  <!-- 其它 -->

    <div class="form-item-wrapper">
      <el-button v-if="ifShowOther" @click="showForm('ifShowOther')">添加其他费用</el-button>
      <el-form 
        v-else
        ref="otherForm" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
      >
        <p class="title-wrapper">其他费用</p>
        <el-table 
         border
          :data="formData.reimburseOtherCharges"
          :summary-method="getSummaries"
          show-summary
          max-height="300px"
          @cell-click="onOtherCellClick"
          @cell-mouse-enter="onOtherCellMouseEnter"
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
                    <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0"></el-input>
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
                  <upLoadFile  @get-ImgList="getOtherList" :limit="limit" uploadType="file" ref="otherUploadFile" :prop="item.prop" :fileList="scope.row[item.prop]"></upLoadFile>                   
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      v-if="processIcon(iconItem.icon, scope.$index, formData.reimburseOtherCharges)"
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
      </el-form>
    </div> 
    <el-button @click="validate">提交</el-button>
    <el-button @click="validateTraffic">提交交通</el-button>
    <el-button @click="validateAcc">提交住房</el-button>
    <el-button @click="validateOther">提交其他</el-button>
  </div>
</template>

<script>
// import CommonForm from './CommonForm'
import upLoadFile from "@/components/upLoadFile";
import { toThousands } from '@/utils/format'
import { findIndex } from '@/utils/process'
import { travelRules, trafficRules, accRules, otherRules } from '../js/customerRules'
import { formConfig, travelConfig, trafficConfig, accommodationConfig, otherConfig } from '../js/config'
const EXCLUDELIST = ['reimburseAttachments', 'reimburseTravellingAllowance', 'reimburseFares', 'reimburseAccommodationSubsidies', 'reimburseOtherCharges']
export default {
  components: {
    upLoadFile
  },
  props: {
    operation: {
      type: String,
      default: 'create'
    }
  },
  data () {
    let iconList = [ // 操作配置
      { icon: 'el-icon-document-add', handleClick: this.addAndCopy, operationType: 'add' }, 
      { icon: 'el-icon-document-copy', handleClick: this.addAndCopy, operationType: 'copy' }, 
      { icon: 'el-icon-top', handleClick: this.up },
      { icon: 'el-icon-bottom', handleClick: this.down },
      { icon: 'el-icon-delete', handleClick: this.delete }
    ]
    return {
      ifShowTraffic: true, // 是否展示交通补贴表格， 以下类似
      ifShowOther: true,
      ifShowAcc: true,
      ifShowTravel: true,
      currentIndex: 0, // 用来标记当前点击单元格所在表格中的索引值
      currentProp: '', // 当前选中的单元格的property, 对应table数据的key值
      currentLabel: '', // 当前选中的单元格的property, 对应table数据的label值
      currentRow: '', // 当前选中的
      maxSize: 1000,
      formData: { // 表单参数
        id: '',
        people: '',
        org: '',
        position: '',
        serviceOrderId: '',
        customerId: '',
        customerName: '',
        shortCustomerName: '',
        origin: '',
        destination: '',
        originDate: '',
        endDate: '',
        remburseType: '',
        projectName: '',
        remburseStatus: '',
        theme: '',
        fillDate: '',
        report: '',
        bearToPay: '',
        responsibility: '',
        serviceRelations: '',
        payTime: '',
        remark: '',
        totalMoney: '',
        reimburseAttachments: [],
        reimburseTravellingAllowances: [{
          id: '',
          days: '',
          money: '',
          remark: ''
        }],
        reimburseFares: [{
          id: '',
          trafficType: '',
          transport: '',
          from: '',
          to: '',
          money: '',
          remark: '',
          invoiceNumber: '1',
          invoiceAttachment: [],
          otherAttachment: [],
          reimburseAttachments: []
        }],
        reimburseAccommodationSubsidies: [{
          id: '',
          days: '',
          money: '',
          totalMoney: '',
          remark: '',
          invoiceNumber: '1',
          invoiceAttachment: [],
          otherAttachment: [],
          reimburseAttachments: []
        }],
        reimburseOtherCharges: [{
          id: '',
          expenseCategory: '',
          money: '',
          remark: '',
          invoiceNumber: '1',
          invoiceAttachment: [],
          otherAttachment: [],
          reimburseAttachments: []
        }],
        isDraft: false // 是否是草稿
      },
      labelWidth: '80px',
      disabled: false,
      labelposition: 'left',
      formConfig,
      limit: 8,
      travelConfig: [...travelConfig, { label: '操作', type: 'operation', iconList, fixed: 'right' }],
      trafficConfig: [...trafficConfig, { label: '操作', type: 'operation', iconList, fixed: 'right', width: 160 }],
      accommodationConfig: [...accommodationConfig, { label: '操作', type: 'operation', iconList, fixed: 'right', width: 160 }],
      otherConfig: [...otherConfig, { label: '操作', type: 'operation', iconList, fixed: 'right', width: 160 }],
      travelRules,
      trafficRules,
      accRules,
      otherRules,
      deleteList: []
    }
  },
  watch: {
    data (val) {
      Object.assign(this.formData, process(val))
    },
    totalMoney (val) {
      this.formData.totalMoney = val
    },
    formData: {
      deep: true,
      handler () {
        //
      }
    }
  },
  computed: {
    totalMoney () {
      return 1
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
    }
  },
  methods: {
    showForm (type) { // 展示表格
      this[type] = false
    },
    onDataChange (val) {
      Object.assign(this.formData, this.process(val))
      console.log(this.formData, 'formData')
    },
    process (val) {
      let result = {}
      for (let key in val) {
        if (!EXCLUDELIST.includes(key)) {
          result[key] = val[key]
        }
      }
      return result
    },
    processIcon (icon, index, data) { // 处理上下移动图标的展示
      return !(
        (icon === 'el-icon-top' && index === 0) ||
        (icon === 'el-icon-bottom' && index === data.length - 1) || 
        (icon === 'el-icon-delete' && index === 0)
      )
    },
    async validate () {
      let isValid = await this.$refs.travelForm.validate()
      console.log(isValid, 'isValid', this.formData)
    },
    async validateTraffic () {
      // let ifInvoiceAttachment = this.formData.reimburseFares.every(item => item.invoiceAttachment && item.invoiceAttachment.length)
      let isValid = await this.$refs.trafficForm.validate()
      console.log('trafficValid', isValid, this.formData)
    },
    async validateAcc () {
      // let ifInvoiceAttachment = this.formData.reimburseAccommodationSubsidies.every(item => item.invoiceAttachment && item.invoiceAttachment.length)
      let isValid = await this.$refs.accForm.validate()
      console.log('AccValid', isValid, this.formData)
    },
    async validateOther () {
      // let ifInvoiceAttachment = this.formData.reimburseOtherCharges.every(item => item.invoiceAttachment && item.invoiceAttachment.length)
      let isValid = await this.$refs.otherForm.validate()
      console.log('otherValid', isValid, this.formData)
    },
    buildAttachment (fileId, reimburseType, attachmentType = 1, reimburseId = 0, id = 0) { // 构建附件的数据格式
      return {
        fileId,
        reimburseType,
        attachmentType,
        reimburseId,
        id
      }
    },
    createFileIdArr (data) { // 附件ID列表
      return data.map(item => item.pictureId)
    },
    createFileList (data, { reimburseType, attachmentType, reimburseId, id }) { // 附件列表
      let fileIdList = this.createFileIdArr(data)
      let resultArr = fileIdList.map(fileId => {
        return this.buildAttachment(fileId, reimburseType, attachmentType, reimburseId, id)
      })
      return resultArr
    },
    getFileList (val) {
      let resultArr = this.createFileList(val, {
        reimburseType: 0,
        attachmentType: 1
      })
      console.log(resultArr, 'resultArr')
      this.formData.reimburseAttachments = resultArr
      console.log(this.formData.reimburseAttachments, 'fileList')
    },
    getTrafficList (val, prop) {
      let data = this.formData.reimburseFares
      let resultArr = []
      if (this.operation === 'create') {
        resultArr = this.createFileList(val, {
          reimburseType: 2,
          attachmentType: prop === 'invoiceAttachment' ? 2 : 1
        })
      }
      this.$set(data[this.currentIndex], prop, resultArr)
    },
    getAccList (val, prop) {
      let data = this.formData.reimburseAccommodationSubsidies
      let resultArr = []
      if (this.operation === 'create') {
        resultArr = this.createFileList(val, {
          reimburseType: 3,
          attachmentType:  prop === 'invoiceAttachment' ? 2 : 1
        })
      }
      this.$set(data[this.currentIndex],  prop, resultArr)
    },
    getOtherList (val, prop) {
      let data = this.formData.reimburseOtherCharges
      let resultArr = []
      if (this.operation === 'create') {
        resultArr = this.createFileList(val, {
          reimburseType: 4,
          attachmentType: prop === 'invoiceAttachment' ? 2 : 1
        })
      }
      this.$set(data[this.currentIndex], prop, resultArr)
      console.log(this.formData, 'formData')
    },
    setCurrentIndex (data, row) {
      this.currentRow = row
      this.currentIndex = findIndex(data, item => item === row)
      console.log(this.currentIndex, 'currentIndex')
    },
    onTravelCellClick (row, column) {
      this.setCurrentProp(column, row)
      // console.log(row, column, cell, event, 'cellChange')
    },
    onTrafficCellClick (row, column) {
      this.setCurrentProp(column, row)
      // console.log(row, column, cell, event, 'cellChange')
    },
    onAccCellClick (row, column) {
      this.setCurrentProp(column, row)
    },
    onOtherCellClick (row, column) {
      this.setCurrentProp(column, row)
    },
    onTrafficCellMouseEnter (row) {
      this.setCurrentIndex(this.formData.reimburseFares, row)
    },
    onAccCellMouseEnter (row) {
      this.setCurrentIndex(this.formData.reimburseAccommodationSubsidies, row)
    },
    onOtherCellMouseEnter (row) {
      this.setCurrentIndex(this.formData.reimburseOtherCharges, row)
    },
    setCurrentProp ({ label, property }) {
      this.currentLabel = label
      this.currentProp = property
    },
    onChange (value) { // 天数 总金额 计算
      console.log(value, 'onchange')
      if (!this.isValidaNumber(value)) {
        return
      }
      if (this.currentProp === 'totalMoney' || this.currentProp === 'days') {
        let data = this.formData.reimburseAccommodationSubsidies[this.currentIndex]
        let { days, totalMoney } = data
        if (!days || !this.isValidaNumber(days) || !totalMoney || !this.isValidaNumber(totalMoney)) { // 如果天数没有填入,或者不符合规范则直接return
          return
        }
        this.$set(data, 'money', (totalMoney / days).toFixed(2))
        console.log(this.formData, 'formData')
      }
    },
    addAndCopy (scope, data, type, operationType) {
      console.log(operationType, 'operationType') // 判断是新增还是复制
      let { row } = scope
      switch (type) {
        case 'travel':
          data.push({
            id: '',
            days: operationType === 'add' ? '' : row.days,
            money: operationType === 'add' ? '' : row.money,
            remark: operationType === 'add' ? '' : row.remark,
          })
          break
        case 'traffic':
          data.push({
            id: '',
            trafficType: operationType === 'add' ? '' : row.trafficType,
            transport: operationType === 'add' ? '' : row.transport,
            from: operationType === 'add' ? '' : row.from,
            to: operationType === 'add' ? '' : row.to,
            money: operationType === 'add' ? '' : row.money,
            remark: operationType === 'add' ? '' : row.remark,
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: []
          })
          break
        case 'accommodation':
          data.push({
            id: '',
            days: operationType === 'add' ? '' : row.days,
            money: operationType === 'add' ? '' : row.money,
            totalMoney: operationType === 'add' ? '' : row.totalMoney,
            remark: operationType === 'add' ? '' : row.remark,
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: []
          })
          break
        case 'other':
          data.push({
            id: '',
            expenseCategory: operationType === 'add' ? '' : row.expenseCategory,
            money: operationType === 'add' ? '' : row.money,
            remark: operationType === 'add' ? '' : row.remark,
            invoiceNumber: '',
            invoiceAttachment: [],
            otherAttachment: []
          })
      }
    },
    delete (scope, data) {
      data.splice(scope.$index, 1)
    },
    up (scope, data) {
      let { $index } = scope
      let prevIndex = $index - 1
      let currentItem = data[$index]
      this.$set(data, $index, data[prevIndex])
      this.$set(data, prevIndex, currentItem)
    },
    down (scope, data) {
      let { $index } = scope
      let lastIndex = $index + 1
      let currentItem = data[$index]
      this.$set(data, $index, data[lastIndex])
      this.$set(data, lastIndex, currentItem)
      // let 
    },
    isValidaNumber (val) { // 判断是否是有效的数字
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
      console.log(columns, data, 'getSum')
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
    resetInfo () {
      this.formData = { // 表单参数
        id: '',
        people: '',
        org: '',
        position: '',
        serviceOrderId: '',
        customerId: '',
        customerName: '',
        shortCustomerName: '',
        origin: '',
        destination: '',
        originDate: '',
        endDate: '',
        remburseType: '',
        projectName: '',
        remburseStatus: '',
        theme: '',
        fillDate: '',
        report: '',
        bearToPay: '',
        responsibility: '',
        serviceRelations: '',
        payTime: '',
        remark: '',
        totalMoney: '',
        reimburseAttachments: [],
        reimburseTravellingAllowances: [{
          id: '',
          days: '',
          money: '',
          remark: ''
        }],
        reimburseFares: [{
          id: '',
          trafficType: '',
          transport: '',
          from: '',
          to: '',
          money: '',
          remark: '',
          invoiceNumber: '1',
          invoiceAttachment: [],
          otherAttachment: [],
          reimburseAttachments: []
        }],
        reimburseAccommodationSubsidies: [{
          id: '',
          days: '',
          money: '',
          totalMoney: '',
          remark: '',
          invoiceNumber: '1',
          invoiceAttachment: [],
          otherAttachment: [],
          reimburseAttachments: []
        }],
        reimburseOtherCharges: [{
          id: '',
          expenseCategory: '',
          money: '',
          remark: '',
          invoiceNumber: '1',
          invoiceAttachment: [],
          otherAttachment: [],
          reimburseAttachments: []
        }]
      }
      this.clearFile()
      this.ifShowTraffic = this.ifShowOther = this.ifShowAcc = this.ifShowTravel = true
    }
  },
  mergeFileList (data) {   
    data.forEach(item => {
      let { invoiceAttachment, otherAttachment } = item
      item.reimburseAttachments = [...invoiceAttachment, ...otherAttachment]
    })
  },
  submit () { // 提交
    let { 
      reimburseTravellingAllowances, 
      reimburseAccommodationSubsidies, 
      reimburseOtherCharges, 
      reimburseFares 
    } = this.formData
    this.mergeFileList(reimburseTravellingAllowances)
    this.mergeFileList(reimburseAccommodationSubsidies)
    this.mergeFileList(reimburseOtherCharges)
    this.mergeFileList(reimburseFares)
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
  }
  .form-wrapper {
    ::v-deep .el-form-item--mini {
      margin-bottom: 0;
    }
  }
  .icon-item {
    font-size: 17px;
    margin: 5px;
    cursor: pointer;
  }
}
</style>