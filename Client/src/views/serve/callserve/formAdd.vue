<template>
  <div class="addClass1">
    <!-- form数组，不包括第一项 -->
    <div style="border:1px solid silver;padding:5px;">
      <el-form
        v-loading="waitingAdd "
        :model="formList[0]"
        :disabled="!isCreate"
        label-width="90px"
        class="rowStyle"
        ref="itemForm"
        size="small"
      >
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="工单ID">
              <el-input disabled v-model="formList[0].id"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="ifEdit?8:7">
            <el-form-item label="服务类型">
              <el-radio-group size="small" :class="ifEdit?'editRodia':''" v-model="formList[0].feeType">
                <el-radio :label="1">免费</el-radio>
                <el-radio :label="2">收费</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>

          <el-col :span="4" v-if="!ifEdit" style="height:35px;line-height:35px;font-size:13px;">
            <el-switch size="small" v-model="formList[0].editTrue" active-text="修改后续" :width="40"></el-switch>
          </el-col>
          <el-col :span="2" v-if="ifEdit"></el-col>
          <el-col :span="ifEdit?3:2" style="height:40px;line-height:30px;">
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
              v-if="ifEdit"
              size="mini"
              icon="el-icon-share"
              @click="handleIconClick(formList[0])"
            >新增</el-button>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="序列号"
                prop="manufacturerSerialNumber"
              :rules="{
              required: true, message: '制造商序列号不能为空', trigger: 'blur'
            }">
              <el-autocomplete
                popper-class="my-autocomplete"
                v-model="formList[0].manufacturerSerialNumber"
                size="small"
               
                :fetch-suggestions="querySearch"
                placeholder="制造商序列号"
                @focus="thisPage=0"
                @select="searchSelect"
              >
                <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
                <template slot-scope="{ item }">
                  <div class="name">
                    <p style="height:20px;margin:2px;">{{ item.manufSN }}</p>
                    <p
                      style="font-size:12px;height:20px;margin:2px 0 5px 0 ;color:silver;"
                    >{{ item.custmrName }}</p>
                  </div>
                </template>
              </el-autocomplete>
              <!-- <el-input
                size="small"
                maxlength="0"
                v-model="formList[0].manufacturerSerialNumber"
                :disabled="ifEdit"
              >
                <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
              </el-input>-->
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

              <el-input v-model="formList[0].problemTypeName" readonly size="small">
                <el-button
                  size="mini"
                  slot="append"
                  icon="el-icon-search"
                  @click="()=>{proplemTree=true,sortForm=1}"
                ></el-button>
              </el-input>
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
    </div>
    <el-collapse
      v-model="activeNames"
      class="openClass"
      v-if="formList.length>1"
    >
      <el-collapse-item title="展开更多订单" name="1">
        <div
          v-for="(item,index) in formList.slice(1)"
          :key="`key_${index}`"
          style="border:1px solid silver;padding:5px;"
        >
          <el-form
            :model="item"
            :disabled="!isCreate"
            label-width="90px"
            class="rowStyle"
            ref="itemFormList"
          >
            <el-row type="flex" class="row-bg" justify="space-around">
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

              <el-col :span="ifEdit?3:2" style="height:40px;line-height:30px;">
                <el-button
                  type="danger"
                  v-if="formList.length>1"
                  size="mini"
                  style="margin-right:20px;"
                  icon="el-icon-delete"
                  @click="deleteForm(item)"
                >删除</el-button>
              </el-col>
              <el-col :span="ifEdit?3:2" style="height:40px;line-height:30px;">
                <el-button
                  type="success"
                  v-if="ifEdit"
                  size="mini"
                  icon="el-icon-share"
                  @click="handleIconClick"
                >新增</el-button>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="序列号"
                       size="small"
                       :rules="{
              required: true, message: '制造商序列号不能为空', trigger: 'blur'
            }">
                  <el-autocomplete
                    popper-class="my-autocomplete"
                    v-model="item.manufacturerSerialNumber"
             
                    :fetch-suggestions="querySearch"
                    placeholder="制造商序列号"
                    @focus="thisPage=index+1"
                    @select="searchSelect"
                  >
                    <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
                    <template slot-scope="{ item }">
                      <div class="name">
                        <p style="height:20px;margin:2px;">{{ item.manufSN }}</p>
                        <p
                          style="font-size:12px;height:20px;margin:2px 0 5px 0 ;color:silver;"
                        >{{ item.custmrName }}</p>
                      </div>
                      <!-- <div class="name">{{ item.manufSN }}</div>
                      <span class="addr">{{ item.custmrName }}</span>-->
                    </template>
                  </el-autocomplete>
                  <!-- <el-input
                size="small"
                maxlength="0"
                v-model="item.manufacturerSerialNumber"
                :disabled="ifEdit"
              >
                <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
                  </el-input>-->
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

                  <el-input v-model="item.problemTypeName" readonly size="small">
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

                <!-- <el-col :span="5">
              <el-button
                size="small"
                type="primary"
                icon="el-icon-share"
                @click="postWorkOrder(item)"
              >确定修改</el-button>
                </el-col>-->
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
        </div>
      </el-collapse-item>
    </el-collapse>
    <!-- </div> -->
    <el-dialog
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
      class="addClass1"
      title="选择制造商序列号"
      @open="openDialog"
      width="90%"
      :visible.sync="dialogfSN"
    >
      <div style="width:600px;margin:10px 0;" class="search-wrapper">
        <el-input
          @input="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model="inputSearch"
          placeholder="制造商序列号"
        >
          <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
        </el-input>
        <el-input
          @input="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model="inputItemCode"
          placeholder="输入物料编码"
        >
          <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
        </el-input>
                <el-input
          @input="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model="inputname"
          placeholder="客户名称"
        >
          <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
        </el-input>
        <!-- <el-checkbox v-model="checked" :disalbed="isDisabled">其它</el-checkbox>         -->
      </div>
      <fromfSN
        :SerialNumberList="filterSerialNumberList"
        :serLoading="serLoad"
        @change-Form="changeForm"
      ></fromfSN>
      <pagination
        v-show="SerialCount>0"
        :total="SerialCount"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleChange"
      />
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogfSN = false">取 消</el-button>
        <el-button type="primary" @click="pushForm">确 定</el-button>
      </span>
    </el-dialog>
    <!--  -->
  </div>
</template>

<script>
import { getSerialNumber } from "@/api/callserve";
import Pagination from "@/components/Pagination";
import * as callservesure from "@/api/serve/callservesure";
import fromfSN from "./fromfSN";
import * as problemtypes from "@/api/problemtypes";
import * as solutions from "@/api/solutions";
import problemtype from "./problemtype";
import solution from "./solution";
export default {
  components: { fromfSN, problemtype, solution, Pagination },
  props: ["isCreate", "ifEdit", "serviceOrderId", "propForm"],
  // ##propForm编辑或者查看详情传过来的数据
  data() {
    return {
      defaultProps: {
        label: "name",
        children: "childTypes",
      }, //树形控件的显示状态
      sortForm: "", //点击解决方案的排序
      solutionOpen: false, //显示解决方案
      problemLabel: "",
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
      inputname:'',//客户名称
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
    };
  },
  created() {},

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
      console.log(22)
      this.formList = this.propForm;
    }
  
  },
  computed: {
    newValue() {
      return JSON.stringify(this.formList);
    },
  },
  watch: {
    newValue: {
      handler: function (Val, Val1) {
        let newVal = JSON.parse(Val);
        let oldVal = JSON.parse(Val1);
        if(!this.ifEdit){
        newVal.map((item, index) => {
          //循环新数组的每一项对象
          console.log(11)
          if (JSON.stringify(newVal[index]) !== JSON.stringify(oldVal[index])) {
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
                    if (item1 == "editTrue") {
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
            // this.SerialNumberList = res.data;
            // this.filterSerialNumberList = this.SerialNumberList;
            // this.SerialCount = res.count;
            this.SerialNumberList = res.data;
            this.filterSerialNumberList = this.SerialNumberList.filter(
              // (item) => item.manufSN === res.manufSN
              item => {
                return this.formList.length ?
                  this.formList.every(formItem => formItem.manufacturerSerialNumber !== item.manufSN) :
                  true
              }
            );
            this.SerialCount = res.count;
            console.log(this.SerialCount, 'total count')
            })
          .catch((error) => {
            console.log(error);
          });
      },
    },
  },
  // updated(){

  //     this.listQuery.customerId=this.form.customerId
  //   // }
  //   // console.log(this.form.customerId)
  // },
  inject: ["form"],
  methods: {
    getSerialNumberList() {
      this.listLoading = true;
      this.serLoad = true;
      getSerialNumber(this.listQuery)
        .then((res) => {
          this.SerialNumberList = res.data;
          // this.filterSerialNumberList = this.SerialNumberList;
          // this.SerialCount = res.count;
          this.filterSerialNumberList = this.SerialNumberList.filter(
            // (item) => item.manufSN === res.manufSN
            item => {
              return this.formList.length ?
                this.formList.every(formItem => formItem.manufacturerSerialNumber !== item.manufSN) :
                true
            }
          );
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
    onSearch (params) {
      this.listLoading = false
      solutions.getList(params).then((response) => {
        this.datasolution = response.data;
        this.solutionCount = response.count;
        this.listLoading = false;
      }).catch(() => {
        this.listLoading = false
      });
    },
    NodeClick(res) {
      console.log(this.formList, this.sortForm - 1);
      this.formList[this.sortForm - 1].problemTypeName = res.name;
      this.formList[this.sortForm - 1].problemTypeId = res.id;
      this.problemLabel = res.name;
      this.proplemTree = false;
    },
    solutionClick(res) {
       console.log(this.formList)
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
      console.log(this.formList, 'formList', this.SerialNumberList)
      this.filterSerialNumberList = this.SerialNumberList.filter(
        // (item) => item.manufSN === res.manufSN
        item => {
          return this.formList.length ?
            this.formList.every(formItem => formItem.manufacturerSerialNumber !== item.manufSN) :
            true
        }
      );
    },
    changeForm(res) {
      this.formListStart = res;

      // console.log(res,this.formListStart);
    },

    pushForm() {
      if (!this.formListStart.length) { // 没有对列表进行选择
        this.dialogfSN = false;
        return
      } // 没有添加直接退出
      this.dialogfSN = false;
      this.waitingAdd = true;
      if (!this.formList[0].manufacturerSerialNumber) {
        //判断从哪里新增的依据是第一个工单是否有id
        this.formList[0].manufacturerSerialNumber = this.formListStart[0].manufSN;
        this.formList[0].internalSerialNumber = this.formListStart[0].internalSN;
        this.formList[0].materialCode = this.formListStart[0].itemCode;
        this.formList[0].materialDescription = this.formListStart[0].itemName;
        const newList = this.formListStart.splice(1, this.formListStart.length);
        for (let i = 0; i < newList.length; i++) {
          this.formList.push({
            manufacturerSerialNumber: newList[i].manufSN,
            editTrue: false,
            internalSerialNumber: newList[i].internalSN,
            materialCode: newList[i].itemCode,
            materialDescription: newList[i].itemName,
            feeType: 1,
            fromTheme: "",
            fromType: 1,
            problemTypeName: "",
            problemTypeId: "",
            priority: 1,
            remark: "",
            solutionId: "",
            status: 1,
            solutionsubject: "",
          });
        }
        this.ifFormPush = true;
      } else {
        this.ifFormPush = true;
        for (let i = 0; i < this.formListStart.length; i++) {
          this.formList.push({
            manufacturerSerialNumber: this.formListStart[i].manufSN,
            internalSerialNumber: this.formListStart[i].internalSN,
            editTrue: false,
            materialCode: this.formListStart[i].itemCode,
            materialDescription: this.formListStart[i].itemName,
            feeType: 1,
            fromTheme: "",
            fromType: 1,
            problemTypeName: "",
            problemTypeId: "",
            priority: 1,
            remark: "",
            solutionId: "",
            status: 1,
            solutionsubject: "",
          });
        }
      }
      this.waitingAdd = false;
    },
    handleIconClick() {
      this.dialogfSN = true;
    },
    addWorkOrder(result, index) {
      const { itemForm, itemFormList } = this.$refs
      const targetForm = index !== undefined ? itemFormList[index] : itemForm
      targetForm.validate(valid => {
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
      })
    },
    searchList() {
      this.listQuery.ManufSN = this.inputSearch;
      this.listQuery.ItemCode = this.inputItemCode;
       this.listQuery.CardName =this.inputname
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
    async querySearch(queryString, cb) {
      this.listQuery.ManufSN = queryString;
      await this.getSerialNumberList();
      var filterSerialNumberList = this.SerialNumberList;
      console.log(filterSerialNumberList, this.listQuery.ManufSN);
      var results = queryString
        ? filterSerialNumberList.filter(this.createFilter(queryString))
        : filterSerialNumberList;
      console.log(results);
      // 调用 callback 返回建议列表的数据
      cb(results);
    },
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
    deleteForm(res) {
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
          this.formList = this.formList.filter((item) => {
            return (
              item.manufacturerSerialNumber != res.manufacturerSerialNumber
            );
          });
          this.$message({
            type: "success",
            message: "删除成功!",
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消删除",
          });
        });
    },
    // deleteForm(res) {
    //   this.$confirm(
    //     `此操作将删除序列商序列号为${res.manufacturerSerialNumber}的表单, 是否继续?`,
    //     "提示",
    //     {
    //       confirmButtonText: "确定",
    //       cancelButtonText: "取消",
    //       type: "warning"
    //     }
    //   )
    //     .then(() => {
    //       callservesure
    //         .delWorkOrder({ id: res.id })
    //         .then(() => {
    //           this.$message({
    //             message: "删除工单成功",
    //             type: "success"
    //           });
    //           this.formList = this.formList.filter(item => {
    //             return (
    //               item.manufacturerSerialNumber != res.manufacturerSerialNumber
    //             );
    //           });
    //         })
    //         .catch(() => {
    //           this.$message({
    //             type: "error",
    //             message: "删除失败"
    //           });
    //         });
    //     })
    //     .catch(() => {
    //       this.$message({
    //         type: "info",
    //         message: "已取消删除"
    //       });
    //     });
    // }
  },
};
</script>

<style lang="scss" scoped>
.addClass1 {
      ::v-deep .el-radio{
    margin-left:0 !important;
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

  //   ::v-deep .el-dialog__footer{
  //   background: lightslategrey;
  // }
}
// .editRodia{
 
// }
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


      