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
            <el-radio-group disabled class="radio-item right" v-model="form.orderTakeType">
              <el-radio :label="1">电话服务</el-radio>
              <el-radio :label="2">上门服务</el-radio>
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
            prop="fromTheme"
            :rules="{
          required: true, message: '呼叫主题不能为空', trigger: 'blur'
        }"
          >
            <el-input v-model="form.fromTheme" type="textarea" maxlength="255"></el-input>
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
    </div>
    <el-dialog
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
  </div>
</template>

<script>
import * as problemtypes from "@/api/problemtypes";
import * as solutions from "@/api/solutions";
import problemtype from "../problemtype";
import solution from "../solution";
import fromfSNC from '../fromfSNC'
import Pagination from "@/components/Pagination";
import { solveTechApplyDevices } from '@/api/serve/technicianApply'
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
    Pagination
  },
  watch: {
    formData: {
      immediate: true,
      handler (val) {
        console.log('formData change')
        this.form = Object.assign({}, this.form, val);
        console.log(val, "new FormData");
      }
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
        fromId: 1, //呼叫来源 1-电话 2-APP
        problemTypeId: "", //问题类型Id
        fromType: 1, //呼叫类型1-提交呼叫 2-在线解答（已解决）
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
      console.log(val, 'single')
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
      console.log(res, 'res', this.sortForm - 1)
      this.form.problemTypeName = res.name;
      this.form.problemTypeId = res.id;
      this.problemLabel = res.name;
      this.proplemTree = false;
      // console.log(this.formList, 'nodeClick', this.problemLabel)
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
      console.log(this.form, 'form')
      return this.form.fromTheme !== "" &&
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
          serviceOrderId: this.instance.serveId
        }
      }
      if (type === 'fail') {
        this._solve(config, type)
      } else {
        let isValid = this.checkForm()
        console.log(isValid, 'valid')
        isValid ? this._solve(config, type) : this.$message.error('请将必填项填写')
      }
    },
    _solve (config, type) {
      solveTechApplyDevices(config).then(res => {
        console.log(res, 'res')
        this.$message({
          type: 'success',
          message: '操作成功'
        })
        this.$emit('close', type)
      }).catch(err => {
        this.$message.error('操作失败')
        console.log(err, 'err')
      })
    },
    close () {
      this.$emit('close')
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
        console.log(this.datasolution, "solution");
        this.solutionCount = response.count;
        this.listLoading = false;
      });
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
  .operation-wrapper {
    position: absolute;
    right: -10px;
    top: -10px;
    padding: 10px;
    transform: translate3d(100%, 0, 0);
    background-color: rgba(242, 242, 242, 1);
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
  }
}
</style>