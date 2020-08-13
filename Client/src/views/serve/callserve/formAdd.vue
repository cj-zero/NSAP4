<template>
  <div class="form-add-wrapper">
    <!-- form数组，不包括第一项 -->
    <div class="order-wrapper" style="border:1px solid silver;padding:5px;">
      <el-form
        v-loading="waitingAdd "
        :model="formList[0]"
        :disabled="!isCreate && isOrderDisabled(formList[0])"
        label-width="90px"
        class="rowStyle"
        ref="itemForm"
        size="small"
      >
        <el-row class="row-bg">
          <el-col :span="8">
            <el-form-item label="工单ID">
              <el-input disabled v-model="formList[0].id"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="ifEdit?8:7">
            <el-form-item label="服务类型">
              <el-radio-group
                size="small"
                :class="ifEdit?'editRodia':''"
                v-model="formList[0].feeType"
              >
                <el-radio :label="1">免费</el-radio>
                <el-radio :label="2">收费</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>

          <el-col :span="4" v-if="!ifEdit" style="height:35px;line-height:35px;font-size:13px;">
            <el-switch size="small" v-model="formList[0].editTrue" active-text="修改后续" :width="40"></el-switch>
          </el-col>
          <el-col :span="2" v-if="ifEdit"></el-col>
          <!-- <el-col :span="ifEdit?3:2" style="height:40px;line-height:30px;">
            <el-button
              type="danger"
              v-if="formList.length>1"
              size="mini"
              style="margin-right:20px;"
              icon="el-icon-delete"
              @click="deleteForm(formList[0])"
            >删除</el-button>
          </el-col>
          <el-col :span="ifEdit?3:2" style="height:40px;line-height:30px;">
            <el-button
              type="success"
              size="mini"
              icon="el-icon-share"
              @click="handleIconClick({})"
            >新增</el-button>
          </el-col> -->
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item
              label="序列号"
              prop="manufacturerSerialNumber"
              :rules="{
              required: true, message: '制造商序列号不能为空', trigger: 'blur'
            }"
            >
              <el-input
                @focus="handleIconClick(formList[0], 0)"
                v-model="formList[0].manufacturerSerialNumber"
                readonly
                :disabled="!form.customerId || isOther(formList[0].manufacturerSerialNumber)"
                size="small"
              >
                <!-- <el-button size="mini" slot="append" icon="el-icon-search" @click="handleIconClick(formList[0], 0)"></el-button> -->
                <el-button
                  size="mini"
                  :disabled="!form.customerId || isOther(formList[0].manufacturerSerialNumber)"
                  slot="append"
                  icon="el-icon-search"
                  @click="handleIconClick(formList[0], 0)"
                ></el-button>
              </el-input>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="内部序列号">
              <el-input size="small" v-model="formList[0].internalSerialNumber" disabled></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="服务合同">
              <el-input size="small" v-model="formList[0].contractId" disabled></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="物料编码">
              <el-input size="small" v-model="formList[0].materialCode" disabled></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="16">
            <el-form-item label="物料描述">
              <el-input size="small" disabled v-model="formList[0].materialDescription"></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="24">
            <el-form-item
              label="呼叫主题"
              prop="fromTheme"
              :rules="{
              required: true, message: '呼叫主题不能为空', trigger: 'blur'
            }"
            >
              <el-input size="small" v-model="formList[0].fromTheme"></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item
              label="呼叫类型"
              prop="fromType"
              :rules="{
              required: true, message: '呼叫类型不能为空', trigger: 'blur'
            }"
            >
              <el-select v-model="formList[0].fromType" size="small">
                <el-option
                  v-for="item in options_type"
                  :key="item.label"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="呼叫状态">
              <!-- <el-input v-model="form.status" disabled></el-input> -->
              <el-select
                size="small"
                v-model="formList[0].status"
                :disabled="isCreate"
                clearable
                placeholder="请选择"
              >
                <el-option
                  v-for="ite in options_status"
                  :key="ite.value"
                  :label="ite.label"
                  :value="ite.value"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="清算日期">
              <el-date-picker
                size="small"
                disabled
                type="date"
                placeholder="选择日期"
                v-model="formList[0].liquidationDate"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item
              label="问题类型"
              prop="problemTypeId"
              :rules="{
              required: true, message: '问题类型不能为空', trigger: 'clear' }"
            >
              <el-input size="small" style="display:none;" v-model="formList[0].problemTypeId"></el-input>
              <!-- {{ formList[0].problemTypeName }} -->
              <el-input
                v-model="formList[0].problemTypeName"
                readonly
                size="small"
                @focus="()=>{proplemTree=true,sortForm=1}"
              >
                <el-button
                  size="mini"
                  slot="append"
                  icon="el-icon-search"
                  @click="()=>{proplemTree=true,sortForm=1}"
                ></el-button>
              </el-input>
              <!-- <el-button
                size="mini"
                slot="append"
                icon="el-icon-search"
                @click="()=>{proplemTree=true,sortForm=1}"
              ></el-button>-->
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="优先级">
              <!-- <el-input v-model="formList[0].priority"></el-input> -->
              <el-select v-model="formList[0].priority" size="small" placeholder="请选择">
                <el-option
                  v-for="ite in options_quick"
                  :key="ite.value"
                  :label="ite.label"
                  :value="ite.value"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="预约时间">
              <el-date-picker
                disabled
                size="small"
                type="date"
                placeholder="选择开始日期"
                v-model="formList[0].bookingDate"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="技术员">
              <el-input size="small" disabled></el-input>
            </el-form-item>
          </el-col>

          <el-col :span="8">
            <el-form-item label="保修结束日期">
              <el-date-picker
                disabled
                size="small"
                type="date"
                placeholder="选择开始日期"
                v-model="formList[0].warrantyEndDate"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="结束时间">
              <el-date-picker
                size="small"
                disabled
                type="date"
                placeholder="选择日期"
                v-model="formList[0].startTime"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item
          label="解决方案"
          prop="solutionId"
          :rules="{
              required: formList[0].fromType === 2, message: '解决方案不能为空', trigger: 'clear'
            }"
        >
          <el-input
            type="textarea"
            style="display:none;"
            size="small"
            v-model="formList[0].solutionId"
          ></el-input>
          <el-input
            v-model="formList[0].solutionsubject"
            @focus="()=>{solutionOpen=true,sortForm=1}"
            :disabled="formList[0].fromType!==2"
            readonly
          >
            <el-button
              :disabled="formList[0].fromType!==2"
              size="mini"
              slot="append"
              icon="el-icon-search"
              @click="()=>{solutionOpen=true,sortForm=1}"
            ></el-button>
          </el-input>
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input type="textarea" size="small" v-model="formList[0].remark"></el-input>
        </el-form-item>
        <el-form-item label="故障描述" prop="remark" v-if="!isCreate">
          <el-input type="textarea" size="small" v-model="formList[0].troubleDescription"></el-input>
        </el-form-item>
        <el-form-item label="过程描述" prop="remark" v-if="!isCreate">
          <el-input type="textarea" size="small" v-model="formList[0].processDescription"></el-input>
        </el-form-item>
        <el-form-item>
          <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
            <el-col :span="6"></el-col>

            <!-- <el-col :span="5">
              <el-button
                size="small"
                type="primary"
                icon="el-icon-share"
                @click="postWorkOrder(item)"
              >确定修改</el-button>
            </el-col>-->
            <el-col :span="4">
              <div class="showSort" style="height:40px;line-height:40px;">{{1}}/{{formList.length}}</div>
            </el-col>
            <el-col :span="5" v-if="ifEdit">
              <el-button
                type="success"
                size="small"
                icon="el-icon-share"
                @click="addWorkOrder(formList[0])"
              >确定新增</el-button>
            </el-col>
          </el-row>
        </el-form-item>
      </el-form>
      <!-- 新增删除按钮 -->
      <div class="operation-btn-wrapper">
        <div class="item" style="height:40px;line-height:30px;">
          <el-button
            type="danger"
            v-if="formList.length>1"
            size="mini"
            style="margin-right:10px;"
            icon="el-icon-delete"
            @click="deleteForm(formList[0], 0, isOrderDisabled(formList[0]))"
          >删除</el-button>
        </div>
        <div class="item" style="height:40px;line-height:30px;">
          <el-button
            type="success"
            size="mini"
            icon="el-icon-share"
            @click="handleIconClick({})"
          >新增</el-button>
        </div>
      </div>
      <!-- <div class="confirm-add-btn" v-if="ifEdit">
        <el-button
          type="success"
          size="small"
          icon="el-icon-share"
          @click="addWorkOrder(formList[0])"
        >确定新增</el-button>
      </div> -->
    </div>
    <el-collapse v-model="activeNames" class="openClass" v-if="formList.length>1">
      <el-collapse-item title="展开更多订单" name="1">
        <div
          v-for="(item,index) in formList.slice(1)"
          :key="`key_${index}`"
          style="border:1px solid silver;padding:5px;"
          class="order-wrapper"
        >
          <el-form
            :model="item"
            :disabled="!isCreate && isOrderDisabled(item)"
            label-width="90px"
            class="rowStyle"
            ref="itemFormList"
          >
            <el-row class="row-bg">
              <el-col :span="8">
                <el-form-item label="工单ID">
                  <el-input size="small" disabled v-model="item.id"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="ifEdit?8:7">
                <el-form-item label="服务类型">
                  <el-radio-group size="small" v-model="item.feeType">
                    <el-radio :label="1">免费</el-radio>
                    <el-radio :label="2">收费</el-radio>
                  </el-radio-group>
                </el-form-item>
              </el-col>

              <el-col :span="4" v-if="!ifEdit" style="height:40px;line-height:30px;font-size:13px;">
                <el-switch size="small" v-model="item.editTrue" active-text="修改后续" :width="40"></el-switch>
              </el-col>
              <el-col :span="2" v-if="ifEdit"></el-col>

              <!-- <el-col :span="ifEdit?3:2" style="height:40px;line-height:30px;">
                <el-button
                  type="danger"
                  v-if="formList.length>1"
                  size="mini"
                  style="margin-right:20px;"
                  icon="el-icon-delete"
                  :disabled="false"
                  @click="deleteForm(item)"
                >删除</el-button>
              </el-col>
              <el-col :span="ifEdit?3:2" style="height:40px;line-height:30px;">
                <el-button
                  type="success"
                  size="mini"
                  icon="el-icon-share"
                  :disabled="false"
                  @click="handleIconClick({})"
                >新增</el-button>
              </el-col> -->
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item
                  label="序列号"
                  size="small"
                  :rules="{
              required: true, message: '制造商序列号不能为空', trigger: 'blur'
            }"
                >
                  <el-input
                    @focus="handleIconClick(item, index + 1)"
                    v-model="item.manufacturerSerialNumber"
                    readonly
                    size="small"
                    :disabled="!form.customerId || isOther(item.manufacturerSerialNumber)"
                  >
                    <el-button
                      size="mini"
                      slot="append"
                      icon="el-icon-search"
                      :disabled="!form.customerId || isOther(item.manufacturerSerialNumber)"
                      @click="handleIconClick(item, index + 1)"
                    ></el-button>
                  </el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="内部序列号">
                  <el-input size="small" v-model="item.internalSerialNumber" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="服务合同">
                  <el-input size="small" v-model="item.contractId" disabled></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="物料编码">
                  <el-input size="small" v-model="item.materialCode" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="16">
                <el-form-item label="物料描述">
                  <el-input size="small" disabled v-model="item.materialDescription"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row
              type="flex"
              class="row-bg"
              justify="space-around"
              style="height:40px;line-height:40px;"
            >
              <el-col :span="24">
                <el-form-item
                  label="呼叫主题"
                  prop="fromTheme"
                  :rules="{
                    required: true, message: '呼叫主题不能为空', trigger: 'blur'
                  }"
                >
                  <el-input size="small" v-model="item.fromTheme"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item
                  label="呼叫类型"
                  prop="fromType"
                  :rules="{
                    required: true, message: '呼叫类型不能为空', trigger: 'blur'
                  }"
                >
                  <el-select v-model="item.fromType" size="small">
                    <el-option
                      v-for="item in options_type"
                      :key="item.label"
                      :label="item.label"
                      :value="item.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="呼叫状态">
                  <!-- <el-input v-model="form.status" disabled></el-input> -->
                  <el-select
                    size="small"
                    v-model="item.status"
                    :disabled="isCreate"
                    clearable
                    placeholder="请选择"
                  >
                    <el-option
                      v-for="ite in options_status"
                      :key="ite.value"
                      :label="ite.label"
                      :value="ite.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="清算日期">
                  <el-date-picker
                    size="small"
                    disabled
                    type="date"
                    placeholder="选择日期"
                    v-model="item.liquidationDate"
                    style="width: 100%;"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item
                  label="问题类型"
                  prop="problemTypeId"
                  :rules="{
                  required: true, message: '问题类型不能为空', trigger: 'clear' }"
                >
                  <el-input size="small" style="display:none;" v-model="item.problemTypeId"></el-input>
                  <el-input
                    :value="item.problemTypeName"
                    readonly
                    size="small"
                    @focus="()=>{proplemTree=true,sortForm=index+2}"
                  >
                    <el-button
                      size="mini"
                      slot="append"
                      icon="el-icon-search"
                      @click="()=>{proplemTree=true,sortForm=index+2}"
                    ></el-button>
                  </el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="优先级">
                  <!-- <el-input v-model="item.priority"></el-input> -->
                  <el-select v-model="item.priority" size="small" placeholder="请选择">
                    <el-option
                      v-for="ite in options_quick"
                      :key="ite.value"
                      :label="ite.label"
                      :value="ite.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="预约时间">
                  <el-date-picker
                    disabled
                    size="small"
                    type="date"
                    placeholder="选择开始日期"
                    v-model="item.bookingDate"
                    style="width: 100%;"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row
              type="flex"
              class="row-bg"
              justify="space-around"
              style="height:40px;line-height:40px;"
            >
              <el-col :span="8">
                <el-form-item label="技术员">
                  <el-input size="small" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="保修结束日期">
                  <el-date-picker
                    disabled
                    size="small"
                    type="date"
                    placeholder="选择开始日期"
                    v-model="item.warrantyEndDate"
                    style="width: 100%;"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="结束时间">
                  <el-date-picker
                    size="small"
                    disabled
                    type="date"
                    placeholder="选择日期"
                    v-model="item.startTime"
                    style="width: 100%;"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
            </el-row>

            <el-form-item
              label="解决方案"
              prop="solutionId"
              :rules="{
                required: item.fromType === 2, message: '解决方案不能为空', trigger: 'clear'
              }"
            >
              <el-input
                type="textarea"
                style="display:none;"
                size="small"
                v-model="item.solutionId"
              ></el-input>
              <el-input v-model="item.solutionsubject" :disabled="item.fromType!==2" readonly>
                <el-button
                  :disabled="item.fromType !== 2"
                  size="mini"
                  slot="append"
                  icon="el-icon-search"
                  @click="()=>{solutionOpen=true,sortForm=index+2}"
                ></el-button>
              </el-input>
            </el-form-item>
            <el-form-item label="备注" prop="remark">
              <el-input type="textarea" size="small" v-model="item.remark"></el-input>
            </el-form-item>
            <el-form-item label="故障描述" v-if="!isCreate" prop="remark">
              <el-input type="textarea" size="small" v-model="item.troubleDescription"></el-input>
            </el-form-item>
            <el-form-item label="过程描述" v-if="!isCreate" prop="remark">
              <el-input type="textarea" size="small" v-model="item.processDescription"></el-input>
            </el-form-item>
            <el-form-item>
              <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
                <el-col :span="6"></el-col>
                <el-col :span="4">
                  <div
                    class="showSort"
                    style="height:40px;line-height:40px;"
                  >{{index+2}}/{{formList.length}}</div>
                </el-col>
                <el-col :span="5" v-if="ifEdit">
                  <el-button
                    type="success"
                    size="small"
                    icon="el-icon-share"
                    @click="addWorkOrder(item, index)"
                  >确定新增</el-button>
                </el-col>
              </el-row>
            </el-form-item>
          </el-form>
          <div class="operation-btn-wrapper">
            <div class="item" style="height:40px;line-height:30px;">
              <el-button
                type="danger"
                v-if="formList.length>1"
                size="mini"
                style="margin-right:10px;"
                icon="el-icon-delete"
                @click="deleteForm(item, index, isOrderDisabled(item))"
              >删除</el-button>
            </div>
            <div class="item" style="height:40px;line-height:30px;">
              <el-button
                type="success"
                size="mini"
                icon="el-icon-share"
                @click="handleIconClick({})"
              >新增</el-button>
            </div>
          </div>
          <!-- <div class="confirm-add-btn" v-if="ifEdit">
            <el-button
              type="success"
              size="small"
              icon="el-icon-share"
              @click="addWorkOrder(item, index)"
            >确定新增</el-button>
          </div> -->
        </div>
      </el-collapse-item>
    </el-collapse>
    <!-- </div> -->
    <el-dialog
      :modal-append-to-body="false"
      :append-to-body="true"
      class="addClass1"
      :title="`第${sortForm}工单`"
      center
      :destroy-on-close="true"
      :visible.sync="proplemTree"
      width="250px"
    >
      <problemtype @node-click="NodeClick" :dataTree="dataTree"></problemtype>
    </el-dialog>
    <el-dialog
      :title="`第${sortForm}个工单的解决方案`"
      center
      :modal-append-to-body="false"
      :append-to-body="true"
      class="addClass1"
      loading
      :visible.sync="solutionOpen"
      width="1000px"
    >
      <solution
        @solution-click="solutionClick"
        :list="datasolution"
        :total="solutionCount"
        :listLoading="listLoading"
        @page-Change="pageChange"
        @search="onSearch"
      ></solution>
      <!-- <span slot="footer" class="dialog-footer">
        <el-button size="small" @click="solutionOpen = false">取 消</el-button>
        <el-button size="small" type="primary" @click="solutionOpen=false">确 定</el-button>
      </span>-->
    </el-dialog>
    <el-dialog
      :append-to-body="true"
      :destroy-on-close="true"
      :modal-append-to-body="false"
      class="addClass1"
      title="选择制造商序列号"
      @open="openDialog"
      width="70%"
      :visible.sync="dialogfSN">
      <div style="width:600px;margin:10px 0;" class="search-wrapper">
        <el-input
          @input="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model="inputSearch"
          placeholder="制造商序列号"
        >
          <i class="el-icon-search el-input__icon" slot="suffix"></i>
        </el-input>
        <el-input
          @input="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model="inputItemCode"
          placeholder="输入物料编码"
        >
          <i class="el-icon-search el-input__icon" slot="suffix"></i>
        </el-input>

        <el-checkbox v-model="inputname" v-show="!isEditOperation && !hasCreateOtherOrder">其他设备</el-checkbox>
      </div>
      <fromfSN
        v-if="!isEditOperation"
        :SerialNumberList="filterSerialNumberList"
        :serLoading="serLoad"
        @change-Form="changeForm"
        @singleSelect="onSingleSelect"
        :ifEdit="isEditOperation"
        :visible="dialogChange"
        :formList="formList"
        :currentTarget="currentTarget"
      ></fromfSN>
      <fromfSNC
        v-else
        :SerialNumberList="filterSerialNumberList"
        :serLoading="serLoad"
        @change-Form="changeForm"
        @singleSelect="onSingleSelect"
        :ifEdit="isEditOperation"
        :formList="formList"
        :visible="dialogChange"
        :currentTarget="currentTarget"
        @toggleDisabledClick="toggleDisabledClick"
      />
      <pagination
        v-show="SerialCount>0"
        :total="SerialCount"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleChange"
      />
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogfSN = false">取 消</el-button>
        <el-button type="primary" @click="pushForm" :disabled="isDisalbed">确 定</el-button>
      </span>
    </el-dialog>

  </div>
</template>

<script>
import { getSerialNumber } from "@/api/callserve";
import Pagination from "@/components/Pagination";
import * as callservesure from "@/api/serve/callservesure";
import fromfSN from "./fromfSN";
import fromfSNC from "./fromfSNC";
import * as problemtypes from "@/api/problemtypes";
import * as solutions from "@/api/solutions";
import problemtype from "./problemtype";
import solution from "./solution";
import { mapMutations } from 'vuex'
export default {
  components: { fromfSN, problemtype, solution, Pagination, fromfSNC },
  provide() {
    let that = this;
    return {
      vm: that,
    };
  },
  props: ["isCreate", "ifEdit", "serviceOrderId", "propForm"],
  // ##propForm编辑或者查看详情传过来的数据
  data() {
    return {
      dialogChange: false,
      defaultProps: {
        label: "name",
        children: "childTypes",
      }, //树形控件的显示状态
      sortForm: "", //点击解决方案的排序
      solutionOpen: false, //显示解决方案
      problemLabel: "",
      dia_copyForm: false, //批量修改表单的弹窗
      copyForm: {
        //批量修改所绑定的值
        fromTheme: "", //呼叫主题
        fromType: 1, //呼叫类型1-提交呼叫 2-在线解答（已解决）
        status: 1, //呼叫状态 1-待确认 2-已确认 3-已取消 4-待处理 5-已排配 6-已外出 7-已挂起 8-已接收 9-已解决 10-已回访
        problemTypeId: "", //问题类型Id
        problemTypeName: "",
        solutionsubject: "",
        priority: 1, //优先级 4-紧急 3-高 2-中 1-低
        solutionId: "", //解决方案
        remark: "", //备注
      },
      dataTree: [], //问题类型的组合
      datasolution: [], //解决方案集合
      solutionCount: "",
      listLoading: true,
      proplemTree: false,
      serLoad: true,
      addressList: [], //客户地址集合
      cntctPrsnList: [], //客户联系人集合
      thisPage: 0, //当前选择的工单页
      SerialNumberList: [],
      filterSerialNumberList: [],
      formListStart: [], //选择的表格数据
      formList: [
        {
          // serviceOrderId:'',
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
          editTrue: true, //修改后续的状态
        },
      ], //表单依赖的表格数据
      // checked: '', // 序列号无数据时出现其它选项，有数据时候禁用
      dialogfSN: false,
      inputSearch: "",
      inputItemCode: "", //物料编码
      activeNames: [], //活跃名称
      inputname: false, //是否为其他
      options_sourse: [
        { value: "电话", label: "电话" },
        { value: "钉钉", label: "钉钉" },
        { value: "QQ", label: "QQ" },
        { value: "微信", label: "微信" },
        { value: "邮件", label: "邮件" },
        { value: "APP", label: "APP" },
        { value: "Web", label: "Web" },
      ],
      options_status: [
        { label: "已回访", value: 10 },
        { label: "已解决", value: 9 },
        { label: "已接收", value: 8 },
        { label: "已挂起", value: 7 },
        { label: "已外出", value: 6 },
        { label: "已排配", value: 5 },
        { label: "待处理", value: 4 },
        { label: "已取消", value: 3 },
        { label: "已确认", value: 2 },
        { label: "待确认", value: 1 },
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
      listQuery: {
        page: 1,
        limit: 30,
        CardCode: "",
      },
      waitingAdd: false, //等待添加的进程结束
      SerialCount: "",
      ifFormPush: false, //表单是否被动态添加过
      isEditOperation: "", // 编辑还是新增操作
      currentTarget: "", // 当前所选择的表格数据
      ruleCopy: {
        fromTheme: [
          { required: true, message: "请选择活动区域", trigger: "change" },
        ],
        fromType: [
          { required: true, message: "请选择活动区域", trigger: "change" },
        ],
        problemTypeId: [
          { required: true, message: "请选择活动区域", trigger: "change" },
        ],
      },
      isDisalbed: false,
    };
  },
  created() {
    console.log(this.isCreate, 'isCreated')
    this.setFormList(this.formList)
  },

  mounted() {
    this.listLoading = true;
    this.getSerialNumberList();
    //获取问题类型的数据
    problemtypes
      .getList()
      .then((res) => {
        this.dataTree = res.data;
      })
      .catch((error) => {
        console.log(error);
      });
    solutions.getList().then((response) => {
      this.datasolution = response.data;
      this.solutionCount = response.count;
      this.listLoading = false;
    });
    if (this.propForm && this.propForm.length) {
      this.formList = this.propForm;
      this.formInitailList = this.propForm.slice() // 保存已经新建的表单项，用于后续判断后续是否能够编辑
      console.log(this.formList, this.formInitailList, 'formInitailList')
      this.setFormList(this.formList)
    }
  },
  computed: {
    newValue() {
      return JSON.stringify(this.formList);
    },
    hasCreateOtherOrder () {
      // 是否已经创建了其他工单，用来判断其他按钮是否可选
      return this.formList.some(item => {
        return item.manufacturerSerialNumber === '其他'
      })
    },
  },
  watch: {
    newValue: {
      handler: function (Val, Val1) {
        let newVal = JSON.parse(Val);
        let oldVal = JSON.parse(Val1);
        if (!this.ifEdit) {
          newVal.map((item, index) => {
            //循环新数组的每一项对象
            if (
              JSON.stringify(newVal[index]) !== JSON.stringify(oldVal[index])
            ) {
              let newValChild = newVal[index]; //新值的每一项
              let oldValChild = oldVal[index];
              if (newVal.length == oldVal.length) {
                for (let item1 in oldValChild) {
                  if (newValChild[item1] !== oldValChild[item1]) {
                    //如果新值和旧值不一样
                    let sliceList = this.formList.slice(index);
                    if (this.formList[index].editTrue) {
                      //如果可以修改
                      //  console.log(thisForm[item1],newValChild[item1],item1)
                      if (
                        item1 == "editTrue" ||
                        item1 == "manufacturerSerialNumber"
                      ) {
                        return;
                      } else if (item1 == "problemTypeId") {
                        sliceList.map((itemF, ind) => {
                          if (ind !== 0) {
                            itemF.problemTypeId = newValChild.problemTypeId;
                            itemF.problemTypeName = newValChild.problemTypeName;
                          }
                        });
                      } else if (item1 == "solutionId") {
                        sliceList.map((itemF, ind) => {
                          if (ind !== 0) {
                            itemF.solutionId = newValChild.solutionId;
                            itemF.solutionsubject = newValChild.solutionsubject;
                          }
                        });
                      } else {
                        sliceList.map((itemF, ind) => {
                          if (ind !== 0) {
                            itemF[item1] = newValChild[item1];
                          }
                        });
                      }
                    }
                  }
                }
              }
            }
          });
        }
        this.$emit("change-form", newVal);
      },
      deep: true,
      // immediate: true
    },
    // propForm: {
    //   deep: true,
    //   immediate: true,
    //   handler(val) {
    //     if (val && val.length) {
    //       this.formList = val;
    //       console.log(this.formList);
    //     }
    //   },
    // },
    "form.customerId": {
      deep: true,
      handler(val) {
        this.listQuery.CardCode = val;
        getSerialNumber(this.listQuery)
          .then((res) => {
            this.SerialNumberList = res.data;
            this.filterSerialNumberList = this.SerialNumberList;
            this.SerialCount = res.count;
          })
          .catch((error) => {
            console.log(error);
          });
      },
    },
    formList: {
      deep: true,
      handler (val) {
        console.log(val, 'val')
        this.setFormList(val)
      }
    },
    dialogfSN() {
      this.dialogChange = !this.dialogChange;
    },
  },
  // updated(){

  //     this.listQuery.customerId=this.form.customerId
  //   // }
  //   // console.log(this.form.customerId)
  // },
  inject: ["form"],
  methods: {
    ...mapMutations('form', {
      setFormList: 'SET_FORM_LIST'
    }),
    isOrderDisabled (val) {
      return (this.formInitailList || []).some(item => item.manufacturerSerialNumber === val.manufacturerSerialNumber)
    },
    isOther (val) {
      // 工单是否为其他
      return val === '其他'
    },
    toggleDisabledClick(val) {
      this.isDisalbed = val;
    },
    getSerialNumberList() {
      this.listLoading = true;
      this.serLoad = true;
      getSerialNumber(this.listQuery)
        .then((res) => {
          this.SerialNumberList = res.data;
          this.filterSerialNumberList = this.SerialNumberList;
          this.SerialCount = res.count;
          this.serLoad = false;
          this.listLoading = false;
        })
        .catch((error) => {
          console.log(error);
        });
    },

    handleChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      this.getSerialNumberList();
    },
    pageChange(res) {
      this.listLoading = true;
      solutions.getList(res).then((response) => {
        this.datasolution = response.data;
        this.solutionCount = response.count;
        this.listLoading = false;
      });
    },
    solutionget(res) {
      this.datasolution = res;
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
      this.copyForm.problemTypeName = res.name;
      this.copyForm.problemTypeId = res.id;
      // this.$set(this.formList[this.sortForm - 1], 'problemTypeName', res.name)
      this.formList[this.sortForm - 1].problemTypeName = res.name;
      // console.log(this.formList[this.sortForm - 1].problemTypeName, 'name')
      this.formList[this.sortForm - 1].problemTypeId = res.id;
      this.problemLabel = res.name;
      this.proplemTree = false;
      // console.log(this.formList, 'nodeClick', this.problemLabel)
    },
    solutionClick(res) {
      this.copyForm.solutionsubject = res.subject;
      this.copyForm.solutionId = res.id;
      this.formList[this.sortForm - 1].solutionsubject = res.subject;
      this.formList[this.sortForm - 1].solutionId = res.id;
      // this.problemLabel = res.name;
      console.log(this.formList[this.sortForm - 1], res, this.sortForm - 1);
      this.solutionOpen = false;
    },
    switchType(val) {
      let vall = "";
      this.dataTree.filter((item) => {
        if (item.id == val) {
          vall = item.name;
          return vall;
        }
        item.childTypes.filter((item) => {
          if (item.id == val) {
            vall = item.name;
            return vall;
          }
          return vall;
        });
      });
      return vall;
    },
    switchSo(val) {
      let res = this.datasolution.filter((item) => item.id == val);
      return res.length ? res[0].subject : "请选择";
    },
    openDialog() {
      this.filterSerialNumberList = this.SerialNumberList;
    },
    changeForm(res) {
      this.formListStart = res;
      this.$emit('change-Form', res)
      // console.log(res, this.formListStart, 'changeForm')
    },

    pushForm() {
        if (!this.formList[0].manufacturerSerialNumber) {
          //判断从哪里新增的依据是第一个工单是否有id
          if (this.inputname) {
            //是否有新增其他选项
            this.inputname = false
            this.formListStart.push({
              manufSN: "其他",
              editTrue: false,
              internalSerialNumber: "",
              materialCode: "",
              materialDescription: "",
              feeType: 1,
              fromTheme: "",
              fromType:1,
              problemTypeName:  "",
              problemTypeId: "",
              priority: 1,
              remark: "",
              solutionId: "",
              status: 1,
              solutionsubject:  "",
            });
          }
          (this.formList[0].manufacturerSerialNumber = this.formListStart[0].manufSN),
            (this.formList[0].internalSerialNumber = this.formListStart[0].internalSN),
            (this.formList[0].materialCode = this.formListStart[0].itemCode),
            (this.formList[0].materialDescription = this.formListStart[0].itemName),
            (this.formList[0].feeType = 1),
            (this.formList[0].editTrue = true),
            (this.formList[0].fromTheme =""),
            (this.formList[0].fromType =  1),
            (this.formList[0].problemTypeName = ""),
            (this.formList[0].problemTypeId =  ""),
            (this.formList[0].priority =  1),
            (this.formList[0].remark = ""),
            (this.formList[0].solutionId =  ""),
            (this.formList[0].status = 1),
            (this.formList[0].solutionsubject =  "");

          const newList = this.formListStart.splice(1,this.formListStart.length);
          for (let i = 0; i < newList.length; i++) {
            this.formList.push({
              manufacturerSerialNumber: newList[i].manufSN,
              editTrue: false,
              internalSerialNumber: newList[i].internalSN,
              materialCode: newList[i].itemCode,
              materialDescription: newList[i].itemName,
              feeType: 1,
              fromTheme: "",
              fromType:  1,
              problemTypeName: "",
              problemTypeId:  "",
              priority:  1,
              remark:  "",
              solutionId: "",
              status: 1,
              solutionsubject:  "",
            });
          }
          
          this.ifFormPush = true;
        } else {
          this.ifFormPush = true;
          if(this.isEditOperation){
            if (this.inputname) {
              this.inputname = false
              this.formListStart={
                manufSN: "其他",
                editTrue: false,
                internalSerialNumber: "",
                materialCode: "",
                materialDescription: "",
                feeType: 1,
                fromTheme:  "",
                fromType:  1,
                problemTypeName:  "",
                problemTypeId:  "",
                priority:  1,
                remark:  "",
                solutionId:  "",
                status:  1,
                solutionsubject:  "",
              }
            }
            (this.formList[this.thisPage].manufacturerSerialNumber = this.formListStart.manufSN),
            (this.formList[this.thisPage].internalSerialNumber = this.formListStart.internalSN),
            (this.formList[this.thisPage].materialCode = this.formListStart.itemCode),
            (this.formList[this.thisPage].materialDescription = this.formListStart.itemName),
            (this.formList[this.thisPage].feeType = 1),
            (this.formList[this.thisPage].editTrue = false),
            (this.formList[this.thisPage].fromTheme =""),
            (this.formList[this.thisPage].fromType =  1),
            (this.formList[this.thisPage].problemTypeName = ""),
            (this.formList[this.thisPage].problemTypeId =  ""),
            (this.formList[this.thisPage].priority =  1),
            (this.formList[this.thisPage].remark = ""),
            (this.formList[this.thisPage].solutionId =  ""),
            (this.formList[this.thisPage].status = 1),
            (this.formList[this.thisPage].solutionsubject =  "");
          }else{
            if (this.inputname) {
              this.inputname = false
              this.formListStart.push({
                manufSN: "其他",
                editTrue: false,
                internalSerialNumber: "",
                materialCode: "",
                materialDescription: "",
                feeType: 1,
                fromTheme:  "",
                fromType:  1,
                problemTypeName:  "",
                problemTypeId:  "",
                priority:  1,
                remark:  "",
                solutionId:  "",
                status:  1,
                solutionsubject:  "",
              });
            }
            for (let i = 0; i < this.formListStart.length; i++) {
              this.formList.push({
                manufacturerSerialNumber: this.formListStart[i].manufSN,
                editTrue: false,
                internalSerialNumber: this.formListStart[i].internalSN,
                materialCode: this.formListStart[i].itemCode,
                materialDescription: this.formListStart[i].itemName,
                feeType: 1,
                fromTheme:  "",
                fromType:  1,
                problemTypeName:  "",
                problemTypeId:  "",
                priority:  1,
                remark:  "",
                solutionId:  "",
                status:  1,
                solutionsubject:  "",
              });
            }
          }
        // this.$refs[formName].resetFields();
      } 
      this.formListStart = []
      // this.formListStart
      this.dialogfSN = false;
    },
        onSingleSelect(res) {
      // this.currentTarget.manufSN = val.manufSN
            this.formListStart = res;
      // this.formList[this.thisPage].manufacturerSerialNumber = res.manufSN;
      // this.formList[this.thisPage].internalSerialNumber = res.internalSN;
      // this.formList[this.thisPage].contractId = res.contractID;
      // this.formList[this.thisPage].materialCode = res.itemCode;
      // this.formList[this.thisPage].materialDescription = res.itemName;
    },
    handleIconClick(value, index) {
      if (!this.form.customerId) {
        return this.$message.error("客户代码不能为空！");
      }
      this.thisPage = index;
      this.$nextTick(() => {
        this.currentTarget = value;
        this.isEditOperation = value
          ? Boolean(value.manufacturerSerialNumber)
          : false;
        this.dialogfSN = true;
      });
    },
    addWorkOrder(result, index) {
      const { itemForm, itemFormList } = this.$refs;
      const targetForm = index !== undefined ? itemFormList[index] : itemForm;
      targetForm.validate((valid) => {
        if (valid) {
          callservesure
            .addWorkOrder(result)
            .then(() => {
              this.$message({
                message: "新增工单成功",
                type: "success",
              });
            })
            .catch((res) => {
              this.$message({
                message: `${res}`,
                type: "error",
              });
            });
        }
      });
    },
    searchList() {
      this.listQuery.ManufSN = this.inputSearch;
      this.listQuery.ItemCode = this.inputItemCode;
      // this.listQuery.CardName = this.inputname;
      this.getSerialNumberList();
      // if (!res) {
      //   this.filterSerialNumberList = this.SerialNumberList;
      // } else {
      //   let list = this.SerialNumberList.filter(item => {
      //     return item.manufSN.indexOf(res) > 0;
      //   });
      //   this.filterSerialNumberList = list;
      // }
    },
    searchSelect(res) {
      if (res.count) {
        this.filterSerialNumberList = this.filterSerialNumberList.filter(
          (item) => item.manufSN === res.manufSN
        );
        // this.filterSerialNumberList = newList;
      }
      this.formList[this.thisPage].manufacturerSerialNumber = res.manufSN;
      this.formList[this.thisPage].internalSerialNumber = res.internalSN;
      this.formList[this.thisPage].contractId = res.contractID;
      this.formList[this.thisPage].materialCode = res.itemCode;
      this.formList[this.thisPage].materialDescription = res.itemName;
      // this.inputSearch = res.manufSN;
    },
    // async querySearch(queryString, cb) {
    //   this.listQuery.ManufSN = queryString;
    //   await this.getSerialNumberList();
    //   var filterSerialNumberList = this.SerialNumberList;
    //   console.log(filterSerialNumberList, this.listQuery.ManufSN);
    //   var results = queryString
    //     ? filterSerialNumberList.filter(this.createFilter(queryString))
    //     : filterSerialNumberList;
    //   console.log(results);
    //   // 调用 callback 返回建议列表的数据
    //   cb(results);
    // },
    createFilter(queryString) {
      return (filterSerialNumberList) => {
        //循环数组的每一项
        return (
          filterSerialNumberList.manufSN
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    handleSelect(item) {
      console.log(item);
    },
    // deleteForm(res) {
    //   this.$confirm(
    //     `此操作将删除序列商序列号为${res.manufacturerSerialNumber}的表单, 是否继续?`,
    //     "提示",
    //     {
    //       confirmButtonText: "确定",
    //       cancelButtonText: "取消",
    //       type: "warning",
    //     }
    //   )
    //     .then(() => {
    //       this.formList = this.formList.filter((item) => {
    //         return (
    //           item.manufacturerSerialNumber != res.manufacturerSerialNumber
    //         );
    //       });
    //       this.$message({
    //         type: "success",
    //         message: "删除成功!",
    //       });
    //     })
    //     .catch(() => {
    //       this.$message({
    //         type: "info",
    //         message: "已取消删除",
    //       });
    //     });
    // },
    deleteForm(res, index, isOrderDisabled) {
      if (this.formList.length <= 1) {
        this.$message.error('至少有一个工单')
      }
      this.$confirm(
        `此操作将删除序列商序列号为${res.manufacturerSerialNumber}的表单, 是否继续?`,
        "提示",
        {
          confirmButtonText: "确定",
          cancelButtonText: "取消",
          type: "warning",
        }
      )
        .then(() => {
          // 新建的时候
          if (this.isCreate) {
            this.formList.splice(index, 1)
            return
          }
          // 编辑的时候
          callservesure
            .delWorkOrder({ id: res.id })
            .then(() => {
              this.$message({
                message: "删除工单成功",
                type: "success",
              });
              if (isOrderDisabled) {
                // 如果删除的项目是一开始新增的时候有的，这个时候就要从this.formInitailList数组队列中删除，
                // 因为有可能再次选择已删除的项目,这个时候就不能禁止编辑
                this.formInitailList.splice(index, 1)
              }
              this.formList = this.formList.filter((item) => {
                return (
                  item.manufacturerSerialNumber != res.manufacturerSerialNumber
                );
              });
            })
            .catch(() => {
              this.$message({
                type: "error",
                message: "删除失败",
              });
            });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消删除",
          });
        });
    },
  },
};
</script>

<style lang="scss" scoped>
.form-add-wrapper {
  max-height: 500px;
  overflow-y: scroll;
  .order-wrapper {
    position: relative;
    .operation-btn-wrapper {
      position: absolute;
      display: flex;
      top: 15px;
      right: 26px;
    }
  }
  .confirm-add-btn {
    position: absolute;
    bottom: 7px;
    right: 30px;
  }
  ::v-deep .el-radio {
    margin-left: 0 !important;
  }
  ::v-deep .el-dialog__header {
    .el-dialog__title {
      color: white;
    }
    .el-dialog__close {
      color: white;
    }

    background: lightslategrey;
  }
  ::v-deep .el-dialog__body {
    padding: 10px 20px;
  }
  ::v-deep .el-button--mini {
    padding: 7px 8px;
  }
  ::v-deep .el-input-group__append {
    padding: 0 10px 0 20px;
  }
}
#demo-ruleForm {
  ::v-deep .el-form {
    padding: 5px;
    // margin-bottom: 2px;
  }
  ::v-deep .el-form-item__label {
    line-height: 30px;
  }
  ::v-deep .el-form-item__content {
    line-height: 30px;
  }
  ::v-deep .el-form-item {
    margin: 10px 0;
  }
}
.search-wrapper {
  display: flex;
  align-items: center;
}
.rowStyle {
  ::v-deep .el-form-item {
    margin: 3px 1px !important;
  }
}

.openClass {
  ::v-deep.el-collapse-item__header {
    padding-left: 50px;
  }
}
.soluClass {
  border: 1px silver solid;
  border-radius: 5px;
  padding: 0 10px;
}
.addClass {
  border: 1px silver solid;
  padding: 5px;
  // margin-left: 20px;
}
.showSort {
  float: right;
  height: 30px;
  width: 130px;
  line-height: 30px;
  font-size: 24px;
}
</style>


      