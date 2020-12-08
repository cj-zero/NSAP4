<template>
  <div class="content-wrapper">
    <el-form
      v-loading="waitingAdd"
      :model="form"
      :disabled="!isNew"
      label-width="100px"
      class="rowStyle"
      ref="itemForm"
      size="mini"
    >
      <el-row class="row-bg" align="middle" type="flex">
        <el-col :span="7">
          <el-form-item label="工单ID">
            <el-input disabled v-model="form.workOrderNumber"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="4">
          <el-form-item label="服务类型">
            <el-radio-group class="radio-item" v-model="form.feeType">
              <el-radio :label="1">免费</el-radio>
              <el-radio :label="2">收费</el-radio>
            </el-radio-group>
          </el-form-item>
        </el-col>
        <el-col :span="4">
          <el-form-item label="服务方式" class="service-way">
            <el-radio-group disabled class="radio-item right" v-model="form.serviceMode">
              <el-radio :label="1">上门服务</el-radio>
              <el-radio :label="2">电话服务</el-radio>
              <el-radio :label="3">返厂维修</el-radio>
            </el-radio-group>
          </el-form-item>
        </el-col>
        <!-- <el-col :span="2" v-if="ifEdit"></el-col> -->
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="18">
          <el-form-item
              label="呼叫主题"
              required
            >
              <!-- <el-input v-model="formList[0].fromTheme" type="textarea" maxlength="255" autosize style="display: none;"></el-input> -->
              <div class="form-theme-content" @click="openFormThemeDialog($event)">
                <el-scrollbar wrapClass="scroll-wrap-class">
                  <div class="form-theme-list">
                    <transition-group name="list" tag="ul">
                      <li class="form-theme-item" v-for="(themeItem, themeIndex) in form.themeList" :key="themeItem.id" >
                        <el-tooltip popper-class="form-theme-toolip" effect="dark" :content="themeItem.description" placement="top">
                          <p class="text">{{ themeItem.description }}</p>
                        </el-tooltip>
                        <i v-if="isNew" class="delete el-icon-error" @click.stop="deleteTheme(form, themeIndex)"></i>
                      </li>
                    </transition-group>
                  </div>
                </el-scrollbar>
              </div>
            </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="技术员">
            <el-input disabled v-model="form.currentUser"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-between">
        <el-col :span="7">
          <el-form-item
            label="序列号"
            prop="manufacturerSerialNumber"
            :rules="{
          required: true, message: '制造商序列号不能为空', trigger: 'blur'
        }"
          >
            <el-input
              @focus="handleIconClick"
              v-model="form.manufacturerSerialNumber"
              readonly
            >
              <!-- <el-button size="mini" slot="append" icon="el-icon-search" @click="handleIconClick"></el-button> -->
              <el-button
                size="mini"
                slot="append"
                icon="el-icon-search"
                @click="handleIconClick"
              ></el-button>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="内部序列号">
            <el-input v-model="form.internalSerialNumber" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="5">
          <el-form-item label="服务合同">
            <el-input v-model="form.contractId" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="保修结束日期">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择日期"
              v-model="form.warrantyEndDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="7">
          <el-form-item label="物料编码">
            <el-input v-model="form.materialCode" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="呼叫状态">
            <el-select v-model="form.status" disabled clearable placeholder="请选择">
              <el-option
                v-for="ite in options_status"
                :key="ite.value"
                :label="ite.label"
                :value="ite.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="5"></el-col>
        <el-col :span="6">
          <el-form-item label="清算日期">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择日期"
              v-model="form.liquidationDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex">
        <el-col :span="18">
          <el-form-item label="物料描述">
            <el-input disabled v-model="form.materialDescription"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="预约时间">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择日期"
              v-model="form.bookingDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="7">
          <el-form-item
            label="呼叫类型"
            prop="fromType"
            :rules="{
          required: true, message: '呼叫类型不能为空', trigger: 'blur'
        }"
          >
            <el-select v-model="form.fromType">
              <el-option
                v-for="item in options_type"
                :key="item.label"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item
            label="问题类型"
            prop="problemTypeId"
            :rules="{
          required: true, message: '问题类型不能为空', trigger: 'clear' }"
          >
            <el-input style="display:none;" v-model="form.problemTypeId"></el-input>
            <el-input
              v-model="form.problemTypeName"
              readonly
              @focus="()=>{proplemTree=true}"
            >
              <el-button
                size="mini"
                slot="append"
                icon="el-icon-search"
                @click="()=>{proplemTree=true}"
              ></el-button>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="5">
          <el-form-item label="优先级">
            <el-select v-model="form.priority" placeholder="请选择">
              <el-option
                v-for="ite in options_quick"
                :key="ite.value"
                :label="ite.label"
                :value="ite.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="上门时间">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择日期"
              v-model="form.visitTime"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="18">
          <el-form-item
            label="解决方案"
            prop="solutionId"
            :rules="{
          required: form.fromType === 2, message: '解决方案不能为空', trigger: 'clear' }"
          >
            <el-input type="textarea" style="display:none;" v-model="form.solutionId"></el-input>
            <el-input
              v-model="form.solutionsubject"
              @focus="()=>{solutionOpen=true}"
              :disabled="form.fromType!==2"
              readonly
            >
              <el-button
                :disabled="form.fromType!==2"
                size="mini"
                slot="append"
                icon="el-icon-search"
                @click="()=>{solutionOpen=true}"
              ></el-button>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="结束时间">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择日期"
              v-model="form.startTime"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-form-item label="备注" prop="remark">
        <el-input type="textarea" v-model="form.remark"></el-input>
      </el-form-item>
    </el-form>
    <div class="operation-wrapper">
      <div class="info-wrapper">
        <el-row v-if="!isNew" type="flex" class="old">
          <span class="del">删除</span>
          <div>
            <p>序列号: {{ info.orginalManufSN }}</p>
          </div>
        </el-row>
        <el-row type="flex">
          <span class="add">新增</span>
          <div>
            <p>序列号: {{ info.manufSN }}</p>
            <p>物料编码: {{ info.itemCode }}</p>
          </div>
        </el-row>
      </div>
      <el-row class="btn-wrapper" type="flex" align="middle">
        <span class="item">{{ info.currentUser }}</span>
        <el-button class="item" size="mini" type="primary" @click="process('success')">确认</el-button>
        <el-button class="item" size="mini" type="info" @click="process('fail')">不通过</el-button>
        <el-button class="item" size="mini" type="info" @click="close" plain>关闭</el-button>
      </el-row>
      <div class="other-wrapper"></div>
    </div>
    <el-dialog
      v-el-drag-dialog
      :modal-append-to-body="false"
      :append-to-body="true"
      :close-on-click-modal="false"
      class="addClass1"
      center
      :destroy-on-close="true"
      :visible.sync="proplemTree"
      width="250px"
    >
      <problemtype @node-click="NodeClick" :dataTree="dataTree"></problemtype>
    </el-dialog>
    <el-dialog
      v-el-drag-dialog
      center
      :modal-append-to-body="false"
      :append-to-body="true"
      class="addClass1"
      loading
      :visible.sync="solutionOpen"
      width="1000px"
      :close-on-click-modal="false"
    >
      <solution
        @solution-click="solutionClick"
        :list="datasolution"
        :total="solutionCount"
        :listLoading="listLoading"
        @page-Change="pageChange"
        @search="onSearch"
      ></solution>
    </el-dialog>
    <el-dialog
      v-el-drag-dialog
      :append-to-body="true"
      :destroy-on-close="true"
      :modal-append-to-body="false"
      class="addClass1"
      title="选择制造商序列号"
      width="1196px"
      top="8vh"
      :close-on-click-modal="false"
      :visible.sync="dialogfSN">
      <div style="width:600px;margin:10px 0;" class="search-wrapper">
        <el-input
          @keyup.enter.native="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model="inputSearch"
          placeholder="制造商序列号"
        >
          <i class="el-icon-search el-input__icon" slot="suffix"></i>
        </el-input>
        <el-input
          @keyup.enter.native="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model="inputItemCode"
          placeholder="输入物料编码"
        >
          <i class="el-icon-search el-input__icon" slot="suffix"></i>
        </el-input>
        <!-- <el-checkbox v-model="inputname" v-show="!isEditOperation && !hasCreateOtherOrder">其他设备</el-checkbox> -->
      </div>
      <fromfSNC
        :SerialNumberList="filterSerialNumberList"
        :serLoading="serLoad"
        @singleSelect="onSingleSelect"
        :ifEdit="isEditOperation"
      />
      <pagination
        v-show="serialCount>0"
        :total="serialCount"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleChange"
      />
      <span slot="footer" class="dialog-footer">
        <el-button @click="onClose">取 消</el-button>
        <el-button type="primary" @click="pushForm">确 定</el-button>
      </span>
    </el-dialog>
    <my-dialog 
      ref="formTheme"
      width="500px"
      :btnList="themeBtnList"
      :appendToBody="true"
      @onClose="closeFormTheme"
    >
      <el-input
        style="width: 200px; margin-bottom: 10px;"
        type="primary"
        size="mini"
        @keyup.enter.native="queryTheme" 
        v-model="listQueryTheme.key" 
        placeholder="呼叫主题内容">
      </el-input>
      <div style="height: 400px;">
        <common-table 
          :loading="themeLoading"
          ref="formThemeTable" 
          :data="themeList" 
          :columns="columns" 
          :selectedList="selectedList"
          selectedKey="id"
        ></common-table>
      </div>
      <pagination
        v-show="themeTotal > 0"
        :total="themeTotal"
        :page.sync="listQueryTheme.page"
        :limit.sync="listQueryTheme.limit"
        @pagination="handleChangeTheme"
      /> 
    </my-dialog>
  </div>
</template>

<script>
import * as problemtypes from "@/api/problemtypes";
import * as solutions from "@/api/solutions";
import problemtype from "../problemtype";
import solution from "../solution";
import fromfSNC from '../fromfSNC'
import Pagination from "@/components/Pagination";
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
import { solveTechApplyDevices } from '@/api/serve/technicianApply'
import { getListByType } from '@/api/serve/knowledge'
export default {
  inject: ['instance'],
  props: {
    isNew: {
      type: Boolean,
      default: false,
    },
    formData: {
      type: Object,
      default() {
        return {};
      },
    },
    info: {
      type: Object,
      default () {
        return {}
      }
    },
    filterSerialNumberList: {
      type: Array,
      default () {
        return []
      }
    },
    serialCount: {
      type: Number,
      default: 0
    },
    listQuery: {
      type: Object,
      default () {
        return {}
      }
    },
    serLoad: {
      type: Boolean,
      default: false
    }
  },
  components: {
    problemtype,
    solution,
    fromfSNC,
    Pagination,
    MyDialog,
    CommonTable
  },
  watch: {
    formData: {
      immediate: true,
      handler (val) {
        this.form = Object.assign({}, this.form, val);
      }
    }
  },
  computed: {
    themeBtnList () {
      return [
        { btnText: '确认', handleClick: this.confirmTheme },
        { btnText: '取消', handleClick: this.closeFormTheme }
      ]
    }
  },
  data() {
    return {
      waitingAdd: false,
      dataTree: [],
      datasolution: [],
      solutionCount: 0,
      solutionOpen: false,
      listLoading: false,
      proplemTree: false,
      isEditOperation: true,
      dialogfSN: false,
      formDataSelect: {}, //选择的编辑的数据
      inputSearch: '',
      inputItemCode: '',
      form: {
        priority: 1, //优先级 4-紧急 3-高 2-中 1-低
        feeType: 1, //服务类型 1-免费 2-收费
        submitDate: "", //工单提交时间
        recepUserId: "", //接单人用户Id
        remark: "", //备注
        status: 1, //呼叫状态 1-待确认 2-已确认 3-已取消 4-待处理 5-已排配 6-已外出 7-已挂起 8-已接收 9-已解决 10-已回访
        currentUserId: "", //App当前流程处理用户Id
        fromTheme: "", //呼叫主题
        themeList: [], // 呼叫主题数组
        fromId: 1, //呼叫来源 1-电话 2-APP
        problemTypeId: "", //问题类型Id
        fromType: "", //呼叫类型1-提交呼叫 2-在线解答（已解决）
        materialCode: "", //物料编码
        materialDescription: "", //物料描述
        manufacturerSerialNumber: "", //制造商序列号
        internalSerialNumber: "", //内部序列号
        warrantyEndDate: "", //保修结束日期
        bookingDate: "", //预约时间
        visitTime: "", //上门时间
        liquidationDate: "", //清算日期
        contractId: "", //服务合同
        solutionId: "", //解决方案
        troubleDescription: "",
        processDescription: "",
      },
      options_status: [
        { label: "已回访", value: 8 },
        { label: "已解决", value: 7 },
        { label: "已接收", value: 6 },
        { label: "已挂起", value: 5 },
        { label: "已外出", value: 4 },
        { label: "已预约", value: 3 },
        { label: "已排配", value: 2 },
        { label: "待处理", value: 1 },
      ],
      options_type: [
        { value: 1, label: "提交呼叫" },
        { value: 2, label: "在线解答" },
      ], //呼叫类型
      options_quick: [
        { label: "高", value: 3 },
        { label: "中", value: 2 },
        { label: "低", value: 1 },
      ],
      themeList: [], // 呼叫主题列表
      themeTotal: 0, // 呼叫主题总数
      themeLoading: false, // 表格loading
      listQueryTheme: {
        page: 1,
        limit: 20,
        type: 4, // 呼叫主题
        key: '' // 搜搜呼叫主题
      },
      columns: [
        { type: 'selection' },
        { label: '呼叫主题', prop: 'name' }
      ],
      selectedList: [] // 当前呼叫主题框存在的数组
    };
  },
  methods: {
    handleIconClick() {
      this.dialogfSN = true
    },
    searchList () {
      this.$emit('searchList', {
        ManufSN: this.inputSearch,
        ItemCode: this.inputItemCode
      })
    },
    pushForm () {
      if (Object.keys(this.formDataSelect).length) {
        this.form.manufacturerSerialNumber = this.formDataSelect.manufSN
        this.form.internalSerialNumber = this.formDataSelect.internalSN
        this.form.materialCode = this.formDataSelect.itemCode
        this.form.materialDescription = this.formDataSelect.itemName
      }
      this.dialogfSN = false
    },
    pageChange(res) {
      this.listLoading = true;
      solutions.getList(res).then((response) => {
        this.datasolution = response.data;
        this.solutionCount = response.count;
        this.listLoading = false;
      });
    },
    handleChange (val) {
      this.$emit('handlelChange', val)
    },
    onSingleSelect (val) {
      this.formDataSelect = val
    },
    onSearch(params) {
      this.listLoading = false;
      solutions
        .getList(params)
        .then((response) => {
          this.datasolution = response.data;
          this.solutionCount = response.count;
          this.listLoading = false;
        })
        .catch(() => {
          this.listLoading = false;
        });
    },
    NodeClick(res) {
      this.form.problemTypeName = res.name;
      this.form.problemTypeId = res.id;
      this.problemLabel = res.name;
      this.proplemTree = false;
    },
    onClose () {
      this.dialogfSN = false
      this.inputSearch = ''
    },
    solutionClick(res) {
      this.form.solutionsubject = res.subject;
      this.form.solutionId = res.id;
      this.solutionOpen = false;
    },
    checkForm () {
      return this.form.themeList &&
        this.form.themeList.length &&
        this.form.fromType !== "" &&
        this.form.problemTypeId !== "" &&
        this.form.manufacturerSerialNumber !== "" &&
        (this.form.fromType === 2 ? this.form.solutionId !== "" : true)
    },
    process (type) {
      let solveType = type === 'fail' ? 0 :
        this.isNew ? 2 : 1
      let config = {
        solveType,
        applyId: this.info.id,
        addServiceWorkOrder: type === 'fail' ? {} : {
          ...this.form,
          fromTheme: JSON.stringify(this.form.themeList),
          serviceOrderId: this.instance.serveId
        }
      }
      console.log(config, this.instance, 'instance')
      if (type === 'fail') {
        this._solve(config, type)
      } else {
        let isValid = this.checkForm()
        isValid ? this._solve(config, type) : this.$message.error('请将必填项填写')
      }
    },
    _solve (config, type) {
      solveTechApplyDevices(config).then(() => {
        this.$message({
          type: 'success',
          message: '操作成功'
        })
        this.$emit('close', type)
      }).catch(err => {
        this.$message.error(`${err.message}`)
        console.log(err, 'err')
      })
    },
    close () {
      this.$emit('close')
    },
    reset () {
      this.from = {
        priority: 1, //优先级 4-紧急 3-高 2-中 1-低
        feeType: 1, //服务类型 1-免费 2-收费
        submitDate: "", //工单提交时间
        recepUserId: "", //接单人用户Id
        remark: "", //备注
        status: 1, //呼叫状态 1-待确认 2-已确认 3-已取消 4-待处理 5-已排配 6-已外出 7-已挂起 8-已接收 9-已解决 10-已回访
        currentUserId: "", //App当前流程处理用户Id
        fromTheme: "", //呼叫主题
        fromId: 1, //呼叫来源 1-电话 2-APP
        problemTypeId: "", //问题类型Id
        fromType: "", //呼叫类型1-提交呼叫 2-在线解答（已解决）
        materialCode: "", //物料编码
        materialDescription: "", //物料描述
        manufacturerSerialNumber: "", //制造商序列号
        internalSerialNumber: "", //内部序列号
        warrantyEndDate: "", //保修结束日期
        bookingDate: "", //预约时间
        visitTime: "", //上门时间
        liquidationDate: "", //清算日期
        contractId: "", //服务合同
        solutionId: "", //解决方案
        troubleDescription: "",
        processDescription: "",
      },
      this.listQueryTheme = {
        page: 1,
        limit: 20,
        key: '',
        type: 7
      }
    },
    _getFormThemeList () {
      this.themeLoading = true
      getListByType(this.listQueryTheme).then(res => {
        let { data, count } = res
        this.themeList = data
        this.themeTotal = count
        this.themeLoading = false
      }).catch(err => {
        this.$message.error(err.message)
        this.themeLoading = false
      })
    },
    queryTheme () {
      this.listQueryTheme.page = 1
      // this.$refs.formThemeTable.clearSelection()
      this._getFormThemeList()
    },
    handleChangeTheme (val) {
      this.listQueryTheme.page = val.page
      this.listQueryTheme.limit = val.limit
      this._getFormThemeList()
    },
    confirmTheme () {
      let selectList = this.$refs.formThemeTable.getSelectionList()
      if (!selectList.length) {
        return this.$message.warning('请先选择数据')
      }
      selectList = selectList.map(item => {
        let { id, name } = item
        return { id, description: name }
      })
      let data = this.form 
      let newList = (data.themeList || []).concat(selectList)
      if (newList && newList.length > 10) {
        return this.$message.warning('最多选择十条数据!')
      }
      this.$set(data, 'themeList', newList)
      this.closeFormTheme()
    },
    closeFormTheme () {
      this.$refs.formThemeTable.clearSelection() // 清空多选
      this.$refs.formTheme.close()
    },
    openFormThemeDialog () {
      if (!this.isNew) return
      let data = this.form
      if (data.themeList && data.themeList.length > 10) {
        return this.$message.warning('最多选择十条数据!')
      }
      this.selectedList = data.themeList || []
      this.$refs.formTheme.open()
    },
    deleteTheme (data, themeIndex) { 
      if (!this.isNew) return
      data.themeList.splice(themeIndex, 1)
    }
  },
  created() {},
  mounted() {
    problemtypes.getList().then((res) => {
      this.dataTree = res.data;
    });
    solutions
      .getList({
        page: 1,
        limit: 20,
      })
      .then((response) => {
        this.datasolution = response.data;
        this.solutionCount = response.count;
        this.listLoading = false;
      });
      this._getFormThemeList()
  },
};
</script>
<style lang='scss' scoped>
.content-wrapper {
  position: relative;
  ::v-deep .el-button--mini {
    padding: 7px 8px;
  }
  ::v-deep .el-input-group__append {
    padding: 0 10px 0 20px;
  }
  ::v-deep .service-way {
    .el-form-item__label {
      margin-top: 10px;
    }
  }
  .radio-item {
    display: flex;
    flex-direction: column;
  }
  /* 呼叫主题样式 */
  .form-theme-content {
    position: relative;
    box-sizing: border-box;
    min-height: 30px;
    padding: 1px 15px;
    color: #606266;
    font-size: 12px;
    line-height: 1.5;
    border-radius: 4px;
    border: 1px solid #DCDFE6;
    background-color: #fff;
    outline: none;
    transition: border-color .2s cubic-bezier(.645, .045, .355, 1);
    cursor: pointer;
    &:focus {
      border-color: #409EFF;
    }
    .form-theme-mask {
      position: absolute;
      left: 0;
      right: 0;
      top: 0;
      bottom: 0;
      z-index: 10;
    }
    ::v-deep .el-scrollbar {
      .scroll-wrap-class {
        max-height: 100px; // 最大高度
        overflow-x: hidden; // 隐藏横向滚动栏
        margin-bottom: 0 !important;
      }
    }
    .form-theme-list {
      .form-theme-item {
        display: inline-block;
        margin-right: 2px;
        margin-bottom: 2px;
        padding: 2px;
        background-color: rgba(239, 239, 239, 1);
        .text-content {
          max-width: 480px;
        }
        &.list-enter-active, &.list-leave-acitve {
          transition: all .4s;
        }
        &.list-enter, &.list-leave-to {
          opacity: 0;
        }
        &.list-enter-to, &.list-leave {
          opacity: 1;
        }
        .text {
          display: inline-block;
          overflow: hidden;
          max-width: 478px;
          text-overflow: ellipsis;
          white-space: nowrap;
          vertical-align: middle;
        }
        .delete {
          margin-left: 5px;
          font-size: 12px;
          vertical-align: middle;
          cursor: pointer;
        }
      }
    }
  }
  .operation-wrapper {
    display: flex;
    flex-direction: column;
    position: absolute;
    right: -10px;
    top: -10px;
    bottom: -10px;
    padding: 10px;
    transform: translate3d(100%, 0, 0);
    background-color: rgba(242, 242, 242, 1);
    border: 1px solid rgba(192, 192, 192,1);
    border-radius: 4px;
    .info-wrapper {
      padding: 5px;
      background-color: #fff;
      .old {
        margin-bottom: 5px;
      }
      .del {
        margin-right: 5px;
        color: red;
      }
      .add {
        margin-right: 5px;
        color: rgba(102, 177, 255, 1);
      }
    }
    .btn-wrapper {
      margin-top: 10px;
      .item {
        margin: 0 5px
      }
    }
    .other-wrapper {
      flex: 1;
      background-color: #fff;
    }
  }
}
</style>