<template>
  <div class="template-wrapper">
    <!-- 选择列表 -->
    <el-row type="flex" justify="space-between" align="middle">
      <div class="select-list-wrapper">
        <el-button class="customer-btn-class" type="primary" size="small" @click="toggleSelect">{{ btnText }}</el-button>
        <div class="select-list" v-show="selectList && selectList.length && ifShowSelect">
          <div class="select-item" v-for="item in selectList" :key="item.title">
            <div class="title">{{ item.title }}</div>
            <div class="tag-content">
              <el-tag v-for="tag in item.options" :key="tag.value" @click="selectTag(tag)">{{ tag.label }}</el-tag>
            </div>
          </div>
        </div>
      </div>
      <div class="date-wrapper">
        <el-date-picker
          disabled
          size="mini"
          v-model="createTime"
          type="date"
          format="yyyy-MM-dd HH:mm"
          placeholder="选择日期">
        </el-date-picker>
      </div>
    </el-row>
    <el-row class="template-content-wrapper">
      <el-form 
        ref="form" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
        :class="{ other: currentType === 3, acc: currentType === 2, uneditable: this.operation === 'view' }"
        :disabled="isDisabled"
      >
        <el-table 
          border
          :data="formData.list"
          max-height="300px"
          @cell-click="onCellClick"
          @row-click="onRowClick"
          v-if="currentType"
        > 
          <template v-for="item in config">
            <el-table-column
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :fixed="item.fixed"
              :width="item.width"
              :resizable="false"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                  <el-form-item
                    :prop="'list.' + scope.$index + '.'+ item.prop"
                    :rules="rules[item.prop] || { required: false }"
                  >
                    <div class="area-wrapper">
                      <el-input 
                        v-model="scope.row[item.prop]" 
                        :disabled="item.disabled || (item.prop === 'invoiceNumber' && scope.row.isValidInvoice)" 
                        :readonly="item.readonly || false"
                        :placeholder="item.placeholder"
                        @focus="onAreaFocus({ prop: item.prop, index: scope.$index })">
                        <el-tooltip
                          v-if="item.prop === 'invoiceNumber'"
                          :disabled="scope.row.isValidInvoice"
                          slot="suffix"
                          effect="dark"
                          placement="top-start"
                          :content="`${scope.row.isValidInvoice ? '' : '无发票附件'}`">
                          <i 
                           
                            class="el-input__icon"
                            :class="{
                              'el-icon-upload-success el-icon-circle-check success': scope.row.isValidInvoice,
                              'el-icon-warning-outline warning': !scope.row.isValidInvoice
                            }">
                          </i>
                        </el-tooltip>
                      </el-input>
                      <template v-if="operation !== 'view' && (item.prop === 'from' || item.prop === 'to')">
                        <div class="selector-wrapper" 
                          v-show="(scope.row.ifFromShow && item.prop === 'from') || (scope.row.ifToShow && item.prop === 'to')">
                          <AreaSelector @close="onCloseArea" @change="onAreaChange" :options="{ prop: item.prop, index: scope.$index }"/>
                        </div>
                      </template>
                    </div>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'list.' + scope.$index + '.'+ item.prop"
                    :rules="rules[item.prop] || { required: false }"
                  >
                    <el-input 
                      v-model="scope.row[item.prop]" 
                      :type="item.type" 
                      :min="0" 
                      :disabled="item.disabled" 
                      @change="onChange"
                      @input.native="onInput"
                      @focus="onFocus(item.prop)"
                      :placeholder="item.placeholder"
                      :class="{ 'money-class': item.prop === 'money' || item.prop === 'totalMoney' }"
                    ></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'list.' + scope.$index + '.'+ item.prop"
                    :rules="rules[item.prop] || { required: false }"
                  >
                    <el-select
                      v-model="scope.row[item.prop]"
                      @change="onChange"
                      @click.native="onSelectClick(item)"
                      :isDisabled="isDisabled"
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
                    @get-ImgList="getImgList" 
                    :limit="item.prop === 'invoiceAttachment' ? 1 : 100" 
                    uploadType="file" 
                    ref="uploadFile" 
                    :options="{ prop: item.prop, index: scope.$index }"
                    @deleteFileList="deleteFileList"
                    :isDisabled="isDisabled"
                    :ifShowTip="!isDisabled"
                    :onAccept="onAccept"
                    :fileList="
                      formData.list[0]
                        ?
                          (item.prop === 'invoiceAttachment' 
                            ? formData.list[0].invoiceFileList 
                            : formData.list[0].otherFileList
                          )
                        : []
                  "
                    >
                  </upLoadFile>
                </template>
              </template>
            </el-table-column>
          </template>
        </el-table>
      </el-form>
      <!-- <el-form 
        ref="form" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
        :disabled="isDisabled"
      >
        <el-table 
          border
          :data="formData.list"
          max-height="300px"
          @cell-click="onCellClick"
          @row-click="onRowClick"
          v-if="currentType"
        > 
          <template v-for="item in upLoadConfig">
            <el-table-column
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :fixed="item.fixed"
              :width="item.width"
            >
              <template slot-scope="scope">
                <upLoadFile  
                  @get-ImgList="getImgList" 
                  :limit="item.prop === 'invoiceAttachment' ? 1 : 100" 
                  uploadType="file" 
                  ref="uploadFile" 
                  :prop="item.prop" 
                  :index="scope.$index"
                  :isReimburse="true"
                  @identifyInvoice="identifyInvoice"
                  @deleteFileList="deleteFileList"
                  :isDisabled="isDisabled"
                  :fileList="
                    formData.list[0]
                      ?
                        (item.prop === 'invoiceAttachment' 
                          ? formData.list[0].invoiceFileList 
                          : formData.list[0].otherFileList
                        )
                      : []
                "
                  >
                </upLoadFile>
              </template>
            </el-table-column>
          </template>
        </el-table>
      </el-form> -->
    </el-row>
  </div>
</template>

<script>
import upLoadFile from "@/components/upLoadFile";
import { trafficRules, accRules, otherRules } from '../js/customerRules'
import { categoryMixin, attachmentMixin } from '../js/mixins'
import { addCost, updateCost } from '@/api/reimburse/mycost'
import { timeToFormat } from '@/utils'
import AreaSelector from '@/components/AreaSelector'
const RULES_MAP = {
  1: trafficRules,
  2: accRules,
  3: otherRules
}
const CONFIG_MAP = {
  1: 'trafficConfig',
  2: 'accommodationConfig',
  3: 'otherConfig'
}
const TRANSPORT_TYPE = 1 // 交通费用类型设为1
const ACC_TYPE = 2 // 住宿费用类型设为2
// const OTHER_TYPE = 3 // 交通费用类型设为3
export default {
  mixins: [categoryMixin, attachmentMixin],
  components: {
    upLoadFile,
    AreaSelector
  },
  props: {
    detailData: {
      type: Object,
      defualt () {
        return {}
      }
    },
    selectList: {
      type: Array,
      default () {
        return []
      }
    },
    categoryList: {
      type: Array,
      default () {
        return []
      }
    },
    operation: {
      type: String,
      default: ''
    }
  },
  watch: {
    detailData: {
      immediate: true,
      handler (val) {
        if (val.list && val.list.length) {
          let { reimburseType, id, createTime } = val.list[0]
          if (reimburseType) {
            this.id = id
            this.createTime = createTime
            this.changeTable(reimburseType - 1)
            this.formData = Object.assign({}, this.formData, val)
          }
        }
      }
    },
    // 'formData.list.0.totalMoney' (val) {
    //   if (val) {
    //     console.log('totalMoney', val)
    //     let { days } = this.formData.list[0]
    //     this.formData.list[0].money = days > 0 
    //       ? (val / days).toFixed(2)
    //       : 0
    //   }
    // }
  },
  data () {
    return {
      rules: '', // 规则
      currentType: '', // 当前的选择模板的type(类型)
      config: '',
      date: '',
      ifShowSelect: false,
      currentProp: '', // 当前表格对应的字段
      id: '', // 费用ID
      createTime: '',
      formData: {
        list: [{
          // id: '',
          createUserId: '',
          feeType: '',
          reimburseType: '',
          serialNumber: 1,
          trafficType: '',
          transport: '',
          from: '',
          to: '',
          money: '',
          maxMoney: '',
          invoiceNumber: '',
          remark: '',
          // createTime: '',
          days: '',
          totalMoney: '',
          expenseCategory: '',
          invoiceAttachment: [],
          otherAttachment: [],
          invoiceFileList: [], // 用于回显
          otherFileList: [], // 用于回显
          reimburseAttachments: [],
          ifFromShow: false, // 是否显示出发地地址选择器
          ifToShow: false // 是否显示目的地的地址选择器
        }]
      },
      maxMoney: 0, // 金钱的最大值，当识别了附件之后需要进行设置
      fileid: [], // 删除的附件ID
      // isFirstVisit: true // 是不是首次打开页面
      upLoadConfig: [
        { label: '发票附件', type: 'upload', prop: 'invoiceAttachment' },
        { label: '其他附件', type: 'upload', prop: 'otherAttachment' },
      ],
      prevAreaData: null
    }
  },
  computed: {
    isDisabled () {
      return this.operation === 'view'
    },
    btnText () {
      return this.formData.list[0].feeType ? this.formData.list[0].feeType : '费用类型'
    }
  },
  methods: {
    hasAttachment (data) { // 判断是否存在附件发票   
      let { invoiceFileList, invoiceAttachment } = data
      return (invoiceFileList.length && !this.fileid.includes(invoiceFileList[0].id)) || invoiceAttachment.length // 存在回显的文件代表已经新增的，并且还没被删除过
    },
    ifInvoicementListInEdit () { // 判断编辑状态下，是否有传附件信息
      let { otherFileList, otherAttachment } = this.formData.list[0]
      let hasAttachment = this.hasAttachment(this.formData.list[0])
      let isValid = true
      console.log(hasAttachment, 'hasAttachment')
      if (!hasAttachment) { // 如果是假发票号则需要判断其它附件的数量，真的说明有发票附件了，直接可以通过
        if (otherFileList.length) {
          let ifDeleted = false
          for (let i = 0; i < this.fileid.length; i++) {
            let fileId = this.fileid[i]
            if (!otherFileList.some(item => item.id === fileId)) {
              ifDeleted = false
              break
            } else {
              ifDeleted = true
            }
          }
          isValid = ifDeleted
            ? Boolean(otherAttachment && otherAttachment.length)
            : true
        } else {
          isValid = Boolean(otherAttachment && otherAttachment.length)
        }
      }
      return isValid
    },
    toggleSelect () {
      if (this.operation === 'view') {
        return
      }
      this.ifShowSelect = !this.ifShowSelect
    },
    selectTag (tag) {
      if (this.currentType === tag.type) {
        this._setRowData(tag)
        return this.toggleSelect()
      }
      if (this.currentType != tag.type) { // 当前选择的类型和上次选择的类型不同
        if (this.operation === 'edit') {
          this._deleteFileId() // 删除附件Id
        }
        if (this.currentType) { // 已经有选择之后，下次再选择需要清空
          this.resetInfo(false, tag.type)
        }
      }
      this._setRowData (tag)
      this.changeTable(tag.type)
      this.toggleSelect()
    },
    _setRowData (tag) { // 根据选择的标签设置响应的字段
      if (tag.type == 1) { // 交通类型
        this.formData.list[0].transport = tag.value
      }
      if (tag.type == 3) { // 其他类别
        this.formData.list[0].expenseCategory = tag.value
      }
      this.formData.list[0].feeType = tag.label
    },
    changeTable (type) {
      this.currentType = type
      this.rules = RULES_MAP[type]
      this.config = this[CONFIG_MAP[type]]
      this.formData.list[0].reimburseType = type + 1
    },
    onRowClick () {
      // console.log('row-clcik')
    },
    onCellClick (row, column) {
      this.setCurrentProp(column, row)
      // console.log(this.currentProp, 'currentPro')
    },
    isValidNumber (val) { // 判断是否是有效的数字
      val = Number(val)
      return !isNaN(val) && val >= 0
    },
    onChange (value) {
      console.log(this.currentProp, 'prop change')
      if (this.currentProp === 'transport') { // 如果改变的是交通方式，则对feeType进行相应的赋值
        this.formData.list[0].feeType = this.transportationList[value - 1].label
      } else if (this.currentProp === 'expenseCategory') {
        this.formData.list[0].feeType = this.otherExpensesList[value - 1].label
      } else { // 住房的 总金额和住宿关联
         if (!this.isValidNumber(value)) {
          return
        }
        if (this.currentProp === 'money') {
          this.formData.list[0].money = parseFloat(String(value))
        }
        if (this.currentProp === 'totalMoney' || this.currentProp === 'days') {
          let data = this.formData.list[0]
          let { days, totalMoney } = data   
          this.formData.list[0].totalMoney = totalMoney = parseFloat(String(totalMoney))
          this.formData.list[0].days = days = parseFloat(String(days))
          if (!days || !this.isValidNumber(days) || !totalMoney || !this.isValidNumber(totalMoney)) { // 如果天数没有填入,或者不符合规范则直接return
            return
          }
          this.$set(data, 'money', (totalMoney / days).toFixed(2))
        }
      }
    },
    onInput (e) {
      let val = e.target.value
      let { invoiceFileList, invoiceAttachment, invoiceNumber, maxMoney } = this.formData.list[0]
      // console.log(invoiceFileList, invoiceAttachment, invoiceNumber, this.currentProp, 'input')
      if (
        (
          (invoiceFileList.length && !this.fileid.includes(invoiceFileList[0].id)) || // 编辑的时候，有回显的附件，并且没有删除
          (invoiceAttachment.length && invoiceNumber)
        ) && maxMoney
      ) {
        if (this.currentProp === 'money' || this.currentProp === 'totalMoney') {
          this.currentType === ACC_TYPE
            ? this.formData.list[0].totalMoney = Math.min(parseFloat(val), maxMoney)
            : this.formData.list[0].money = Math.min(parseFloat(val), maxMoney)
        }
      }
    },
    onFocus (val) {
      this.currentProp = val
    },
    onAreaFocus ({ prop, index }) { // 打开地址选择
      if (this.prevAreaData) {
        this.prevAreaData.ifFromShow = false
        this.prevAreaData.ifToShow = false
      }
      if (prop === 'from' || prop === 'to') {
        let currentRow = this.formData.list[index]
        prop === 'from'
          ? this.$set(currentRow, 'ifFromShow', true)
          : this.$set(currentRow, 'ifToShow', true)
        this.prevAreaData = currentRow
      }
    },
    onCloseArea (options) { // 关闭地址选择器
      let { prop, index } = options
      let currentRow = this.formData.list[index]
      prop === 'from'
          ? this.$set(currentRow, 'ifFromShow', false)
          : this.$set(currentRow, 'ifToShow', false)
      this.prevAreaData = null
    },
    onAreaChange (val) {
      let { province, city, district, prop, index } = val
      let currentRow = this.formData.list[index]
      const countryList = ['北京市', '天津市', '上海市', '重庆市']
      let result = ''
      result = countryList.includes(province)
        ? province + district
        : city + district
      currentRow[prop] = result
      this.prevAreaData = null
    },
    onSelectClick (val) {
      this.currentProp = val.prop
    },
    setCurrentProp ({ property }) {
      this.currentProp = property
    },
    getImgList (val, { prop, index, fileId, uploadVm, operation }) {
      let data = this.formData.list
      let currentRow = data[index]
      let attachmentConfig = {
        data,
        index,
        prop,
        val,
        reimburseType: 5
      }
      // 删除操作也不进行识别
      if (fileId && prop === 'invoiceAttachment' && !operation) { // 图片上传成功会返回当前的pictureId, 并且只识别发票附件 
        this.identifyLoading = this.$loading({
          lock: true,
          text: '发票识别中'
        })
        this._identifyInvoice({ // 先进行识别再进行赋值
          fileId, 
          currentRow, 
          uploadVm,
          tableType: this.currentType === TRANSPORT_TYPE
            ? 'traffic' 
            : this.currentType === ACC_TYPE 
              ? 'acc'
              : 'other'
        }, this.currentType === ACC_TYPE).then(isValid => {
          isValid 
            ? this._setAttachmentList(attachmentConfig) 
            : this._setAttachmentList({ ...attachmentConfig, ...{ val: [] }})
          this.identifyLoading.close()
        })
      } else {
        this._setAttachmentList(attachmentConfig)
      }
    },
    _deleteFileId () {
      let { invoiceFileList, otherFileList } = this.formData.list[0]
      let invoiceIdList = invoiceFileList.map(item => item.id)
      let otherIdList = otherFileList.map(item => item.id)
      this.fileid.push(...invoiceIdList, ...otherIdList)
      console.log(this.formData.list[0], 'delete list')
    },
    getFileList (list) {
      return list.map(item => item.id)
    },
    clearFile () {
      if (this.$refs.uploadFile) {
        for (let i = 0; i < this.$refs.uploadFile.length; i++) {
          this.$refs.uploadFile[i].clearFiles()
        }
      }
    },
    resetInfo (isClose = true, type = '') {
      let prevData = this.formData.list[0]
      let { money, totalMoney, remark, maxMoney, from, to } = prevData
      this.$refs.form.clearValidate()
      this.$refs.form.resetFields()
      this.clearFile()
      this.formData = {
        list: [{
          createUserId: '',
          feeType: '',
          reimburseType: '',
          serialNumber: 1,
          trafficType: '',
          transport: '',
          from: this.currentType === TRANSPORT_TYPE ? from : '',
          to: this.currentType === TRANSPORT_TYPE ? to : '',
          money: !type 
            ? '' 
            : (this.currentType === ACC_TYPE ? totalMoney : money),
          maxMoney: type ? maxMoney : '',
          invoiceNumber: '',
          remark: type ? remark : '',
          days: '',
          totalMoney: type === ACC_TYPE ? money : '',
          expenseCategory: '',
          invoiceAttachment: [],
          otherAttachment: [],
          invoiceFileList: [], // 用于回显
          otherFileList: [], // 用于回显
          reimburseAttachments: [],
          ifFromShow: false, // 是否显示出发地地址选择器
          ifToShow: false
        }]
      }
      this.rules = ''
      this.config = ''
      this.currentProp = ''
      this.currentType = ''
      this.maxNumber = 0
      this.prevAreaData = null
      if (isClose) { // 关闭弹窗的时候
        this.ifShowSelect = false
        this.fileid = []
        this.id = ''
        this.createTime = ''
      }
    },
    async _operate () {
      if (!this.rules) {
        return Promise.reject({ message: '请先选择报销类别' })
      }
      this.mergeFileList(this.formData.list)
      let isValid = await this.$refs.form.validate()
      // console.log(isValid, this.ifInvoicementListInEdit(), 'VALIDA')
      if (isValid && this.ifInvoicementListInEdit()) {
        this.operation === 'create'
          ? this.formData.list[0].createTime = timeToFormat('yyyy-MM-dd HH:mm:ss')
          : this.formData.list[0].createTime = this.createTime
        this.formData.list[0].id = this.id
        this.formData.list[0].fileid = this.fileid
        return this.operation === 'create'
          ? addCost(this.formData.list[0])
          : updateCost(this.formData.list[0])
      } else {
        return Promise.reject({ message: '格式错误或必填项未填写' })
      }
    },
    deleteFileList ({ id }) {
      this.fileid.push(id)
      let currentRow = this.formData.list[0]
      this._setCurrentRow(currentRow, {
        invoiceNo: '',
        money: '',
        isAcc: this.currentType === ACC_TYPE,
        isValidInvoice: false
      })
    }
  }
}
</script>
<style lang='scss' scoped>
.template-wrapper {
  min-height: 122px;
  ::v-deep .el-form-item--mini.el-form-item {
    margin-bottom: 0;
  }
  ::v-deep .el-input__icon {
    &.success {
      color: rgba(0, 128, 0, 1);
    }
    &.warning {
      color: rgba(255, 165, 0, 1);
    }
  }
  .select-list-wrapper {
    position: relative;
    
    .select-list {
      position: absolute;
      z-index: 11;
      top: 35px;
      left: 0;
      width: 325px;
      min-height: 400px;
      // overflow: hidden;
      overflow-y: auto;
      background: #fff;
      box-shadow: 0 0 4px 4px rgba(0, 0, 0, .15);
      &::-webkit-scrollbar {
        display: none;
      }
      .select-item {
        .title {
          box-sizing: border-box;
          padding-left: 10px;
          line-height: 30px;
          height: 30px;
          background: #eee;
        }
        .tag-content {
          padding: 10px;
          .el-tag {
            cursor: pointer;
            margin-right: 10px;
            margin-top: 5px;
          }
        }
      }
    }
  }
  .template-content-wrapper {
    margin-top: 15px;
    ::v-deep .el-table  {
      overflow: visible;
      .el-table__body-wrapper {
        overflow: visible;
      }
      .cell {
        overflow: visible;
      }
    }
    .form-wrapper {
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
      &.acc {
        width: 916px;
      }
      &.other {
        width: 846px;
      }
      .money-class {
        ::v-deep input {
          text-align: right;
        }
      }
      .area-wrapper {
        position: relative;
        .selector-wrapper {
          position: absolute;
          top: 40px;
          left: 0;
          z-index: 100;
          // transform: translate3d(0, -100%, 0);
        }
      }
    }
  }
  ::v-deep .el-form-item__content {
    input::-webkit-outer-spin-button,
    input::-webkit-inner-spin-button {
      -webkit-appearance: none;
      appearance: none; 
      margin: 0; 
    }
    /* 火狐 */
    input{
      -moz-appearance: textfield;
    }
  }
}
</style>