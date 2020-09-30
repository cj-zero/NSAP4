<template>
  <div class="template-wrapper">
    <!-- 选择列表 -->
    <el-row type="flex" justify="space-between" align="middle">
      <div class="select-list-wrapper">
        <el-button type="primary" size="small" @click="toggleSelect">费用类型</el-button>
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
                    <el-input v-model="scope.row[item.prop]" :disabled="item.disabled"></el-input>
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
                    :prop="item.prop" 
                    :index="scope.$index"
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
              </template>
            </el-table-column>
          </template>
        </el-table>
      </el-form>
    </el-row>
  </div>
</template>

<script>
import upLoadFile from "@/components/upLoadFile";
import { trafficRules, accRules, otherRules } from '../js/customerRules'
import { categoryMixin, attachmentMixin } from '../js/mixins'
import { addCost, updateCost } from '@/api/reimburse/mycost'
import { timeToFormat } from '@/utils'
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
// const TRANSPORT_TYPE = 1 // 交通费用类型设为1
const ACC_TYPE = 2 // 住宿费用类型设为2
// const OTHER_TYPE = 3 // 交通费用类型设为3
export default {
  mixins: [categoryMixin, attachmentMixin],
  components: {
    upLoadFile
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
    }
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
          invoiceNumber: '1',
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
        }]
      },
      fileid: [], // 删除的附件ID
      // isFirstVisit: true // 是不是首次打开页面
    }
  },
  computed: {
    ifInvoicementList () { // 是否有上传发票附件
      let { invoiceAttachment } = this.formData.list[0]
      return this.operation === 'create'
        ? invoiceAttachment.length // 如果是新建则只需要判断invoiceAttachment是否值即可
        : this.ifInvoicementListInEdit()
    },
    isDisabled () {
      return this.operation === 'view'
    }
  },
  methods: {
    ifInvoicementListInEdit () { // 判断编辑状态下，是否有传附件信息
      let { invoiceFileList, invoiceAttachment } = this.formData.list[0]
      if (invoiceFileList.length) {
        console.log('enter')
        if (this.fileid.includes(invoiceFileList[0].id)) {
          console.log('has delete')
          // 如何删除了之前上传过的附件,则需要判断invocementList 即手动上传附件的数组是否为空
          return invoiceAttachment.length !== 0
        }
        return true
      }
      return invoiceAttachment.length !== 0
    },
    toggleSelect () {
      if (this.operation === 'view') {
        return
      }
      this.ifShowSelect = !this.ifShowSelect
    },
    selectTag (tag) {
      // this.resetInfo()
      console.log(this.currentType, tag.type)
      if (this.currentType === tag.type) {
        return this.toggleSelect()
      }
      if (this.currentType != tag.type) {
        if (this.operation === 'edit') {
          this._deleteFileId() // 删除附件Id
        }
        this.resetInfo(false, tag.type)
      }
      this.changeTable(tag.type)
      console.log(CONFIG_MAP)
      // 去除操作标题
      if (tag.type == 1) { // 交通类型
        console.log('traffic')
        this.formData.list[0].transport = tag.value
      }
      if (tag.type == 3) { // 其他类别
        this.formData.list[0].expenseCategory = tag.value
      }
      this.formData.list[0].reimburseType = tag.type + 1
      this.formData.list[0].feeType = tag.label
      this.toggleSelect()
      // this.isFirstVisit = false
      console.log(this.formData.list[0], 'formData')
    },
    changeTable (type) {
      this.currentType = type
      this.rules = RULES_MAP[type]
      this.config = this[CONFIG_MAP[type]].slice(0, -1)
      console.log(this.config, 'config')
    },
    onRowClick () {
      console.log('row-clcik')
    },
    onCellClick (row, column) {
      console.log('cellClick')
      this.setCurrentProp(column, row)
      console.log(this.currentProp, 'currentPro')
    },
    isValidNumber (val) { // 判断是否是有效的数字
      val = Number(val)
      return !isNaN(val) && val >= 0
    },
    onChange (value) {
      console.log(this.currentProp, 'prop change')
      if (this.currentProp === 'transport') { // 如果改变的是交通方式，则对feeType进行相应的赋值
        this.formData.list[0].feeType = this.transportationList[value - 1].label
        console.log(this.formData.list[0].feeType, 'this.formData.list[0].feeType')
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
    onSelectClick (val) {
      this.currentProp = val.prop
    },
    setCurrentProp ({ property }) {
      this.currentProp = property
    },
    getImgList (val, prop) {
      let data = this.formData.list[0]
      let resultArr = []
      resultArr = this.createFileList(val, {
        reimburseType: 5,
        attachmentType: prop === 'invoiceAttachment' ? 2 : 1
      })
      this.$set(data, prop, resultArr)
      console.log(this.formData.list, 'formData.list')
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
      let { money, totalMoney, remark } = prevData
      console.log(typeof type, 'type', remark, money, totalMoney)
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
          from: '',
          to: '',
          money: !type 
            ? '' 
            : (this.currentType === ACC_TYPE ? totalMoney : money),
          invoiceNumber: '1',
          remark: type ? remark : '',
          days: '',
          totalMoney: type === ACC_TYPE ? money : '',
          expenseCategory: '',
          invoiceAttachment: [],
          otherAttachment: [],
          invoiceFileList: [], // 用于回显
          otherFileList: [], // 用于回显
          reimburseAttachments: []
        }]
      }
      console.log(this.formData, 'change after')
      this.rules = ''
      this.config = ''
      this.currentProp = ''
      this.currentType = ''
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
      if (isValid && this.ifInvoicementList) {
        this.operation === 'create'
          ? this.formData.list[0].createTime = timeToFormat('yyyy-MM-dd HH:mm:ss')
          : this.formData.list[0].createTime = this.createTime
        this.formData.list[0].id = this.id
        this.formData.list[0].fileid = this.fileid
        return this.operation === 'create'
          ? addCost(this.formData.list[0])
          : updateCost(this.formData.list[0])
      } else {
        return Promise.reject({ message: '请将必填项填写' })
      }
    },
    deleteFileList ({ id }) {
      this.fileid.push(id)
      // console.log(this.fileid, 'deleteFileList')
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.template-wrapper {
  height: 400px;
  ::v-deep .el-form-item--mini.el-form-item {
    margin-bottom: 0;
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
  .select-list-wrapper {
    position: relative;
    
    .select-list {
      position: absolute;
      z-index: 11;
      top: 35px;
      left: 0;
      width: 325px;
      height: 400px;
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
  }
}
</style>