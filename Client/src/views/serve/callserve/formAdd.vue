<template>
  <div class="form-add-wrapper">
    <!-- form数组，不包括第一项 -->
    <div class="order-wrapper" style="border:1px solid silver;padding:5px;">
      <el-form
        v-loading="waitingAdd "
        :model="formList[0]"
        :class="{ 'form-disabled': !isCreate && isOrderDisabled(formList[0]) }"
        :disabled="!isCreate && isOrderDisabled(formList[0])"
        label-width="72px"
        class="rowStyle"
        ref="itemForm"
        size="mini"
        :show-message="false"
      >
        <el-row class="row-bg">
          <el-col :span="7">
            <el-form-item label="工单ID">
              <el-input disabled v-model="formList[0].workOrderNumber"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="4">
            <el-form-item label="服务类型">
              <el-radio-group
                class="radio-item"
                v-model="formList[0].feeType"
              >
                <el-radio :label="1">免费</el-radio>
                <el-radio :label="2">收费</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="4" style="transform: translate3d(-15px, 0, 0);" v-if="formName === '查看'">
            <el-form-item label="服务方式">
              <el-radio-group
                style="margin-top: -9px;"
                disabled
                class="radio-item right"
                v-model="formList[0].serviceMode"
              >
                <el-radio :label="1">上门服务</el-radio>
                <el-radio :label="2">电话服务</el-radio>
                <el-radio :label="3">返厂维修</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="4" v-if="!ifEdit" style="height:35px;line-height:35px;font-size:13px;transform: translate3d(25px, 0, 0);">
            <el-switch v-model="formList[0].editTrue" active-text="修改后续" :width="40"></el-switch>
          </el-col>
          <!-- <el-col :span="2" v-if="ifEdit"></el-col> -->
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="7">
            <el-form-item
              label="序列号"
              prop="manufacturerSerialNumber"
              :rules="{
              required: true, message: '', trigger: 'blur'
            }"
            >
              <el-input
                @focus="handleIconClick(formList[0], 0)"
                v-model="formList[0].manufacturerSerialNumber"
                readonly
                :disabled="form.selectSerNumberDisabled || isOther(formList[0].manufacturerSerialNumber)"
              
              >
                <!-- <el-button size="mini" slot="append" icon="el-icon-search" @click="handleIconClick(formList[0], 0)"></el-button> -->
                <el-button
                  size="mini"
                  :disabled="form.selectSerNumberDisabled || isOther(formList[0].manufacturerSerialNumber)"
                  slot="append"
                  icon="el-icon-search"
                  @click="handleIconClick(formList[0], 0)"
                ></el-button>
              </el-input>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="物料编码">
              <el-input v-model="formList[0].materialCode" disabled></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="5">
            <el-form-item label="呼叫状态">
              <el-select
                v-model="formList[0].status"
                disabled
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
          <el-col :span="6">
            <el-form-item label="技术员">
              <el-input disabled v-model="formList[0].currentUser"></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-between">
          
          <el-col :span="18">
            <el-form-item label="物料描述">
              <el-input disabled v-model="formList[0].materialDescription"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="保修到期">
              <el-input
                disabled
                placeholder="选择日期"
                v-model="formList[0].warrantyEndDate"
              ></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="18">
            <el-form-item
              label="呼叫主题"
              required
            >
              <!-- <el-input v-model="formList[0].fromTheme" type="textarea" maxlength="255" autosize style="display: none;"></el-input> -->
              <div class="form-theme-content" @click="openFormThemeDialog($event, 0)">
                <!-- <div class="form-theme-mask" v-if="isOrderDisabled(formList[0]) || formName === '查看'"></div> -->
                <el-scrollbar wrapClass="scroll-wrap-class">
                  <div class="form-theme-list">
                    <transition-group name="list" tag="ul">
                      <li class="form-theme-item" v-for="(themeItem, themeIndex) in formList[0].themeList" :key="themeItem.id" >
                        <el-tooltip popper-class="form-theme-toolip" effect="dark" :content="themeItem.description" placement="top">
                          <p class="text">{{ themeItem.description }}</p>
                        </el-tooltip>
                        <i 
                          v-if="isShowDeleteIcon(formList[0])" 
                          class="delete el-icon-error" 
                          @click.stop="deleteTheme(formList[0], themeIndex)">
                        </i>
                      </li>
                    </transition-group>
                  </div>
                </el-scrollbar>
              </div>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="清算日期">
              <el-input
                disabled
                placeholder="选择日期"
                v-model="formList[0].liquidationDate"         
              ></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex">
          
          <el-col :span="7">
            <el-form-item
              label="呼叫类型"
              prop="fromType"
              :rules="{
              required: true, message: '呼叫类型不能为空', trigger: 'blur'
            }"
            >
              <el-select v-model="formList[0].fromType" @change="onFromTypeChange(0)">
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
              <el-input style="display:none;" v-model="formList[0].problemTypeId"></el-input>
              <el-input
                v-model="formList[0].problemTypeName"
                readonly
              
                @focus="()=>{proplemTree=true,sortForm=1}"
              >
                <el-button
                  size="mini"
                  slot="append"
                  icon="el-icon-search"
                  @click="()=>{proplemTree=true,sortForm=1}"
                ></el-button>
              </el-input>
            </el-form-item>
          </el-col>
          <el-col :span="5">
            <el-form-item label="优先级">
              <el-select v-model="formList[0].priority" placeholder="请选择">
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
            <el-form-item label="预约时间">
              <el-input
                disabled
                placeholder="选择日期"
                v-model="formList[0].bookingDate"
              ></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="18">
            <el-form-item
              label="解决方案"
              prop="solutionId"
              :rules="{
              required: formList[0].fromType === 2, message: '解决方案不能为空', trigger: 'clear' }"
            >
              <el-input
                type="textarea"
                style="display:none;"
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
          </el-col>
          <el-col :span="6">
            <el-form-item label="上门时间">
              <el-input
                disabled
                placeholder="选择日期"
                v-model="formList[0].visitTime"
              ></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="18">
            <el-form-item label="客服备注" prop="remark">
              <el-input type="textarea" maxlength="255" v-model="formList[0].remark" autosize></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="结束时间">
              <el-input
                disabled
                placeholder="选择日期"
                v-model="formList[0].completeDate"
              ></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-row type="flex">
          <el-col :span="7">
            <el-form-item label="问题描述" prop="remark" v-if="formName === '查看'">
              <el-input v-model="formList[0].troubleDescription" disabled></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="11">
            <el-form-item label="售后方案" prop="remark" v-if="formName === '查看'">
              <el-input v-model="formList[0].processDescription" disabled></el-input>
            </el-form-item>
          </el-col>
          <!-- <el-col :span="6">
            <el-form-item label="完工报告" prop="remark" v-if="formName === '查看'">
              <el-button type="primary" size="mini" @click="showReport" style="width: 112.5px;">查看</el-button>
            </el-form-item>
          </el-col> -->
        </el-row>
        <el-form-item>
          <el-row :gutter="10" type="flex" class="row-bg" justify="end">
            <!-- <el-col :span="6"></el-col> -->
            <el-col :span="4">
              <div class="showSort" style="height:40px;line-height:40px;">{{1}}/{{formList.length}}</div>
            </el-col>
          </el-row>
        </el-form-item>
      </el-form>
      <!-- 新增删除按钮 -->
      <div class="operation-btn-wrapper" v-if="formName !== '查看'">
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
            @click="handleIconClick(formList[0])"
          >新增</el-button>
        </div>
      </div>
      <el-col :span="6" class="report-btn-wrapper" v-if="formName === '查看'">
        <span class="title">完工报告</span>
        <el-button type="primary" size="mini" @click="handleReport(form.id, formList)" style="width: 127.5px;">查看</el-button>
      </el-col>
    </div>
    <el-collapse v-model="activeNames" class="openClass" v-if="formList.length>1" @change="handleCollapseChange">
      <el-collapse-item :title="collapseTitle" name="1">
        <div
          v-for="(item,index) in formList.slice(1)"
          :key="`key_${index}`"
          style="border:1px solid silver;padding:5px;"
          class="order-wrapper"
        >
          <el-form
            :model="item"
            :class="{ 'form-disabled': !isCreate && isOrderDisabled(item) }"
            :disabled="!isCreate && isOrderDisabled(item)"
            label-width="72px"
            class="rowStyle"
            ref="itemFormList"
            size="mini"
            :show-message="false"
          >
            <el-row class="row-bg">
              <el-col :span="7">
                <el-form-item label="工单ID">
                  <el-input disabled v-model="item.workOrderNumber"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="4">
                <el-form-item label="服务类型">
                  <el-radio-group
                    class="radio-item"
                    v-model="item.feeType"
                  >
                    <el-radio :label="1">免费</el-radio>
                    <el-radio :label="2">收费</el-radio>
                  </el-radio-group>
                </el-form-item>
              </el-col>
              <el-col :span="4" style="transform: translate3d(-15px, 0, 0);" v-if="formName === '查看'">
                <el-form-item label="服务方式">
                  <el-radio-group
                    style="margin-top: -9px;"
                    class="radio-item right"
                    v-model="item.serviceMode"
                  >
                    <el-radio :label="1">上门服务</el-radio>
                    <el-radio :label="2">电话服务</el-radio>
                    <el-radio :label="3">返厂维修</el-radio>
                  </el-radio-group>
                </el-form-item>
              </el-col>
              <el-col :span="4" v-if="!ifEdit" style="height:35px;line-height:35px;font-size:13px;transform: translate3d(25px, 0, 0);">
                <el-switch v-model="item.editTrue" active-text="修改后续" :width="40"></el-switch>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="7">
                <el-form-item
                  label="序列号"
                  prop="manufacturerSerialNumber"
                  :rules="{
                  required: true, message: '', trigger: 'blur'
                }"
                >
                  <el-input
                    @focus="handleIconClick(item, index + 1)"
                    v-model="item.manufacturerSerialNumber"
                    readonly
                    :disabled="form.selectSerNumberDisabled || isOther(item.manufacturerSerialNumber)"
                  >
                    <!-- <el-button size="mini" slot="append" icon="el-icon-search" @click="handleIconClick(item, 0)"></el-button> -->
                    <el-button
                      size="mini"
                      :disabled="form.selectSerNumberDisabled || isOther(item.manufacturerSerialNumber)"
                      slot="append"
                      icon="el-icon-search"
                      @click="handleIconClick(item, index + 1)"
                    ></el-button>
                  </el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="物料编码">
                  <el-input v-model="item.materialCode" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="5">
                <el-form-item label="呼叫状态">
                  <el-select
                    v-model="item.status"
                    disabled
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
              <el-col :span="6">
                <el-form-item label="技术员">
                  <el-input disabled v-model="item.currentUser"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-between">
              
              <el-col :span="18">
                <el-form-item label="物料描述">
                  <el-input disabled v-model="item.materialDescription"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="保修到期">
                  <el-input
                    disabled
                    placeholder="选择日期"
                    v-model="item.warrantyEndDate"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="18">
                <el-form-item
                  label="呼叫主题"
                  required
                >
                  <!-- <el-input type="textarea" maxlength="255" v-model="item.fromTheme" autosize></el-input> -->
                  <div class="form-theme-content" @click="openFormThemeDialog($event, index + 1)">
                    <el-scrollbar wrapClass="scroll-wrap-class">
                      <div class="form-theme-list">
                        <transition-group name="list" tag="ul">
                          <li class="form-theme-item" v-for="(themeItem, themeIndex) in item.themeList" :key="themeItem.id">
                            <el-tooltip popper-class="form-theme-toolip" effect="dark" placement="top">
                              <div slot="content">{{ themeItem.description }}</div>
                              <p class="text">{{ themeItem.description }}</p>
                            </el-tooltip>
                            <i 
                              v-if="isShowDeleteIcon(item)"  
                              class="delete el-icon-error" 
                              @click.stop="deleteTheme(item, themeIndex)">
                            </i>
                          </li>
                        </transition-group>
                      </div>
                    </el-scrollbar>
                  </div>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="清算日期">
                  <el-input
                    disabled
                    placeholder="选择日期"
                    v-model="item.liquidationDate"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex">
              <el-col :span="7">
                <el-form-item
                  label="呼叫类型"
                  prop="fromType"
                  :rules="{
                  required: true, message: '呼叫类型不能为空', trigger: 'blur'
                }"
                >
                  <el-select v-model="item.fromType" @change="onFromTypeChange(index + 1)">
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
                  <el-input style="display:none;" v-model="item.problemTypeId"></el-input>
                  <el-input
                    v-model="item.problemTypeName"
                    readonly
                  
                    @focus="()=>{proplemTree=true,sortForm= index + 2 }"
                  >
                    <el-button
                      size="mini"
                      slot="append"
                      icon="el-icon-search"
                      @click="()=>{proplemTree=true,sortForm= index + 2 }"
                    ></el-button>
                  </el-input>
                </el-form-item>
              </el-col>
              <el-col :span="5">
                <el-form-item label="优先级">
                  <el-select v-model="item.priority" placeholder="请选择">
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
                <el-form-item label="预约时间">
                  <el-input
                    disabled
                    placeholder="选择日期"
                    v-model="item.bookingDate"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="18">
                <el-form-item
                  label="解决方案"
                  prop="solutionId"
                  :rules="{
                  required: item.fromType === 2, message: '解决方案不能为空', trigger: 'clear' }"
                >
                  <el-input
                    type="textarea"
                    style="display:none;"
                    v-model="item.solutionId"
                  ></el-input>
                  <el-input
                    v-model="item.solutionsubject"
                    @focus="()=>{solutionOpen=true,sortForm= index + 2 }"
                    :disabled="item.fromType!==2"
                    readonly
                  >
                    <el-button
                      :disabled="item.fromType!==2"
                      size="mini"
                      slot="append"
                      icon="el-icon-search"
                      @click="()=>{solutionOpen=true,sortForm= index + 2 }"
                    ></el-button>
                  </el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="上门时间">
                  <el-input
                    disabled
                    placeholder="选择日期"
                    v-model="item.visitTime"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="18">
                <el-form-item label="客服备注" prop="remark">
                  <el-input type="textarea" maxlength="255" v-model="item.remark" autosize></el-input>
              </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="结束时间">
                  <el-input
                    disabled
                    placeholder="选择日期"
                    v-model="item.completeDate"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex">
              <el-col :span="7">
                <el-form-item label="问题描述" prop="remark" v-if="formName === '查看'">
                  <el-input v-model="item.troubleDescription" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="售后方案" prop="remark" v-if="formName === '查看'">
                  <el-input v-model="item.processDescription" disabled></el-input>
                </el-form-item>
              </el-col>
              <!-- <el-col :span="6">
                <el-form-item label="完工报告" prop="remark" v-if="formName === '查看'">
                  <el-button type="primary" size="mini" @click="showReport" style="width: 112.5px;">查看</el-button>
                </el-form-item>
              </el-col> -->
            </el-row>
            <el-form-item>
              <el-row :gutter="10" type="flex" class="row-bg" justify="end">
                <!-- <el-col :span="6"></el-col> -->
                <el-col :span="4">
                  <div class="showSort" style="height:40px;line-height:40px;">{{index+2}}/{{formList.length}}</div>
                </el-col>
              </el-row>
            </el-form-item>
          </el-form>
          <div class="operation-btn-wrapper" v-if="formName !== '查看'">
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
                @click="handleIconClick(item)"
              >新增</el-button>
            </div>
          </div>
          <el-col :span="6" class="report-btn-wrapper" v-if="formName === '查看'">
            <span class="title">完工报告</span>
            <el-button type="primary" size="mini" @click="handleReport(form.id, formList)" style="width: 127.5px;">查看</el-button>
          </el-col>
        </div>
      </el-collapse-item>
    </el-collapse>
    <!-- </div> -->
    <el-dialog
      v-el-drag-dialog
      :modal-append-to-body="false"
      :append-to-body="true"
      :close-on-click-modal="false"
      class="dialog-mini"
      title="问题类型"
      center
      :modal="false"
      :destroy-on-close="true"
      :visible.sync="proplemTree"
      width="250px"
    >
      <problemtype @node-click="NodeClick" :dataTree="dataTree"></problemtype>
    </el-dialog>
    <el-dialog
      v-el-drag-dialog
      title="解决方案"
      center
      :modal-append-to-body="false"
      :append-to-body="true"
      class="dialog-mini"
      loading
      :modal="false"
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
      <!-- <span slot="footer" class="dialog-footer">
        <el-button @click="solutionOpen = false">取 消</el-button>
        <el-button type="primary" @click="solutionOpen=false">确 定</el-button>
      </span>-->
    </el-dialog>
    <el-dialog
      v-el-drag-dialog
      :append-to-body="true"
      :destroy-on-close="true"
      :modal-append-to-body="false"
      :modal="false"
      class="dialog-mini"
      title="选择制造商序列号"
      @open="openDialog"
      width="1196px"
      top="8vh"
      :close-on-click-modal="false"
      :visible.sync="dialogfSN">
      <div style="width:600px;margin:10px 0;" class="search-wrapper">
        <el-input
          @keyup.enter.native="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model.trim="inputSearch"
          placeholder="制造商序列号"
        >
          <i class="el-icon-search el-input__icon" slot="suffix"></i>
        </el-input>
        <el-input
          @keyup.enter.native="searchList"
          style="width:150px;margin:0 20px;display:inline-block;"
          v-model.trim="inputItemCode"
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
        <el-button @click="onClose">取 消</el-button>
        <el-button type="primary" @click="pushForm" :disabled="isDisalbed">确 定</el-button>
      </span>
    </el-dialog>
    <el-dialog
      v-el-drag-dialog
      width="983px"
      class="dialog-mini"
      :append-to-body="true"
      :modal-append-to-body="false"
      :modal="false"
      :close-on-click-modal="false"
      title="服务行为报告单"
      :visible.sync="dialogReportVisible"
      @closed="onReportClosed"
    >
      <Report
        ref="report"
        :data="reportData"
      ></Report>
    </el-dialog>
    <!-- 呼叫主题弹窗 -->
    <my-dialog 
      ref="formTheme"
      width="767px"
      title="呼叫主题"
      :btnList="themeBtnList"
      :append-to-body="true"
      @closed="closeFormTheme"
    >
      <el-input
        style="width: 200px; margin-bottom: 10px;"
        type="primary"
        size="mini"
        @keyup.enter.native="queryTheme" 
        v-model="listQueryTheme.key" 
        placeholder="呼叫主题内容">
      </el-input>
        <common-table 
          height="400px"
          :loading="themeLoading"
          row-key="id"
          ref="formThemeTable" 
          :data="themeList" 
          :columns="columns" 
          :selectedList="selectedList"
          selectedKey="id"
        ></common-table>
      <pagination
        v-show="themeTotal > 0"
        :total="themeTotal"
        :page.sync="listQueryTheme.page"
        :limit.sync="listQueryTheme.limit"
        layout="total, sizes, prev, next, jumper"
        @pagination="handleChangeTheme"
      /> 
    </my-dialog>
  </div>
</template>

<script>
import { getSerialNumber } from "@/api/callserve";
import { getListByType } from '@/api/serve/knowledge'
import * as callservesure from "@/api/serve/callservesure";
import fromfSN from "./fromfSN";
import fromfSNC from "./fromfSNC";
import * as problemtypes from "@/api/problemtypes";
import * as solutions from "@/api/solutions";
import problemtype from "./problemtype";
import solution from "./solution";
import { mapMutations } from 'vuex'
import Report from '../common/components/report'
import { reportMixin } from '../common/js/mixins'
import elDragDialog from '@/directive/el-dragDialog'
import { isCustomerCode } from '@/utils/validate'
import { deepClone } from  '@/utils'
export default {
  components: { fromfSN, problemtype, solution, fromfSNC, Report },
  mixins: [reportMixin],
  provide() {
    let that = this;
    return {
      vm: that,
    };
  },
  directives: {
    elDragDialog
  },
  props: ["isCreate", "ifEdit", "serviceOrderId", "propForm", "formName", "form", "inputSerial", "businessType"],
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
        status: 1, //呼叫状态  工单状态 1-待处理 2-已排配 3-已预约 4-已外出 5-已挂起 6-已接收 7-已解决 8-已回访  服务单 1-待确认 2-已确认 3-已取消
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
          fromType: "", //呼叫类型1-提交呼叫 2-在线解答（已解决）
          materialCode: "", //物料编码
          materialDescription: "", //物料描述
          manufacturerSerialNumber: "", //制造商序列号
          internalSerialNumber: "", //内部序列号
          warrantyEndDate: "", //保修到期
          bookingDate: "", //预约时间
          visitTime: "", //上门时间
          liquidationDate: "", //清算日期
          completeDate: "", // 结束时间
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
      inputname: false, //是否为其他设备
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
        { label: "已回访", value: 8 },
        { label: "已完成", value: 7 },
        { label: "已寄回", value: 6 },
        { label: "在维修", value: 5 },
        { label: "在上门", value: 4 },
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
      collapseTitle: '展开更多订单',
      dialogReportVisible: false,
      themeList: [], // 呼叫主题列表
      themeTotal: 0, // 呼叫主题总数
      themeLoading: false, // 表格loading
      listQueryTheme: {
        page: 1,
        limit: 20,
        type: 4, // 呼叫主题
        key: '' // 搜搜呼叫主题
      },
      formThemeData: [],
      columns: [
        { type: 'selection' },
        { label: '呼叫主题', prop: 'name' }
      ],
      selectedList: [] // 当前呼叫主题框存在的数组
    };
  },
  created() {
    this.setFormList(this.formList)
  },

  mounted() {
    this.listLoading = true;
    if (this.formName === '编辑' || this.formName === '确认') {
      this.getSerialNumberList(this.form.customerId);
    }
    // this.getSerialNumberList();
    //获取问题类型的数据
    problemtypes
      .getList()
      .then((res) => {
        this.dataTree = res.data;
      })
      .catch((error) => {
        console.log(error);
      });
    solutions.getList({
      page: 1,
      limit: 20
    }).then((response) => {
      this.datasolution = response.data;
      this.solutionCount = response.count;
      this.listLoading = false;
    });

    // const list = [
    //   '客户咨询如何设置工步？',
    //   '如何设置18650电池的工步？',
    //   '如何设置动力电池的工步',
    //   'R部工程师新需求沟通，出差报备',
    //   '寄出物料至客户现场',
    //   '联机异常',
    //   '新购工装需上门指导使用',
    //   '通道保护，需上门检修。',
    //   '设备需返修处理。',
    //   '采购下位机板，询价/报价',
    //   '登录用户时提示未知的错误，原因？',
    //   '安装BTS8.0软件提示未注册，原因？',
    //   '不会联机，需要指导。',
    //   '8512分容柜无法联机'
    // ]
    // if (this.formName !== '查看') {
    //   for (let i = 0; i < list.length; i++) {
    //     this.formThemeData.push({
    //       id: i + 1,
    //       description: list[i]
    //     })
    //   }
      
    // }
    this._getFormThemeList()
  },
  computed: {
    themeBtnList () {
      return [
        { btnText: '确认', handleClick: this.confirmTheme },
        { btnText: '取消', handleClick: this.closeFormTheme }
      ]
    },
    newValue() {
      return JSON.stringify(this.formList);
    },
    hasCreateOtherOrder () {
      // 是否已经创建了其他设备工单，用来判断其他设备按钮是否可选
      return this.formList.some(item => {
        return item.manufacturerSerialNumber === '其他设备'
      })
    }
  },
  watch: {
    newValue: {
      handler: function (Val, Val1) {
        // let _this = this
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
                for (let item1 in newValChild) {
                  if (JSON.stringify(newValChild[item1]) !== JSON.stringify(oldValChild[item1])) {
                    //如果新值和旧值不一样
                    let sliceList = this.formList.slice(index);
                    if (this.formList[index].editTrue) {
                      //如果可以修改
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
                      } else if (item1 === 'themeList') {
                        console.log('themeList')
                        sliceList.map((itemF, ind) => {
                          if (ind !== 0) {
                            this.$set(itemF, 'themeList', deepClone(newValChild.themeList))
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
        newVal.forEach((item, index) => {
          if (oldVal[index] !== undefined) {
            let oldItem = oldVal[index]
            if (item.fromType !== oldItem.fromType) {
              if (this.isChangeStatus(item)) {
                this.formList[index].status = this.getStatus(item.fromType)
              }
            }
          }
        })
        this.$emit("change-form", newVal);
      },
      deep: true,
      // immediate: true
    },
    'formList.0' (newVal, oldVal) {
      if (newVal.fromType !== oldVal.fromType) {
        if (this.isChangeStatus(newVal)) {
          newVal.status = this.getStatus(newVal.fromType)
        }
      }
    },
    propForm: {
      deep: true,
      immediate: true,
      handler(val) {
        if (val && val.length) {
          this.formList = JSON.parse(JSON.stringify(val))
          this.formInitailList = JSON.parse(JSON.stringify(val))
          this.setFormList(this.formList)
        }
      },
    },
    "form.customerId": {
      deep: true,
      handler(val) {
        let { inputSerial: manufSN, businessType } = this
        if (isCustomerCode(val)) {
          this.listQuery.CardCode = String(val).toUpperCase()
          this.listQuery.ManufSN = businessType === 'search' ? manufSN : ''
          getSerialNumber(this.listQuery)
            .then((res) => {
              this.SerialNumberList = res.data;
              this.filterSerialNumberList = this.SerialNumberList;
              this.SerialCount = res.count;
              this.serLoad = false;
              this.listLoading = false;
              if ( // 当搜索出来的结果有数据，并且manufSN不为空且工单的第一个数据没被填写过，才对第一个工单进行填写
                this.filterSerialNumberList &&
                this.filterSerialNumberList.length &&
                !this.formList[0].manufacturerSerialNumber &&
                manufSN
              ) {
                let data = this.filterSerialNumberList[0]
                this.formList[0].manufacturerSerialNumber = data.manufSN
                this.formList[0].internalSerialNumber = data.internalSN
                this.formList[0].materialCode = data.itemCode
                this.formList[0].materialDescription = data.itemName
                this.formList[0].feeType = 1
                this.formList[0].editTrue = true
                this.formList[0].fromType =  this.formList[0].fromType || ""
                this.formList[0].priority =  this.formList[0].priority || 1
                this.formList[0].status = 1
              }
            })
            .catch((error) => {
              console.log(error);
            });
        }
      },
    },
    formList: {
      deep: true,
      handler (val) {
        this.setFormList(val)
      }
    },
    dialogfSN() {
      this.dialogChange = !this.dialogChange;
    },
  },
  // inject: ["form"],
  methods: {
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
      let data = this.formList[this.currentFormIndex] // 当前选中的呼叫主题对应formList的第几项
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
    openFormThemeDialog (e, formIndex) {
      if (this.formName === '查看') {
        return
      }
      if (!this.form.customerId) {
        return this.$message.error('客户代码不能为空!')
      }
      let data = this.formList[formIndex]
      if (this.isOrderDisabled(data) && this.formName === '编辑') {
        return
      }
      if (data.themeList && data.themeList.length > 10) {
        return this.$message.warning('最多选择十条数据!')
      }
      this.currentFormIndex = formIndex // 记录当前选中数据对应的索引值
      this.selectedList = data.themeList || []
      this.$refs.formTheme.open()
    },
    deleteTheme (data, themeIndex) { // formIndex 是formList的索引 themeIndex主题呼叫中标签的索引
      data.themeList.splice(themeIndex, 1)
    },
    isShowDeleteIcon (data) { // 是否展示delteIcon
      return (this.isOrderDisabled(data) && this.formName === '编辑')
        ? false
        : this.formName !== '查看'
    },
    isChangeStatus (val) { // 是否可以改变状态
      return (this.formName === '编辑' && !this.formInitailList.every(item => item.id === val.id)) 
      || this.formName === '新建' 
      || this.formName === '确认'
    },
    onClose () {
      this.dialogfSN = false
      this.inputSearch = ''
    },
    onReportClosed () {
      this.$refs.report.reset()
    },
    ...mapMutations({
      setFormList: 'SET_FORM_LIST'
    }),
    isOrderDisabled (val) {
      return (this.formInitailList || []).some(item => item.manufacturerSerialNumber === val.manufacturerSerialNumber)
    },
    isOther (val) {
      // 工单是否为其他设备
      return val === '其他设备'
    },
    onFromTypeChange (index) {
      console.log(index, 'index')
      let data = this.formList[index]
      console.log(data.fromType, 'formTYpe')
      if (Number(data.fromType) === 1) {
        data.solutionId = ''
        data.solutionsubject = ''
      }
      console.log(data, 'data')
    },
    toggleDisabledClick(val) {
      this.isDisalbed = val;
    },
    getStatus (formType) { // 根据呼叫类型 来改变状态\
      return Number(formType) === 1 ? 1 : 7
    },  
    getSerialNumberList(code) {
      this.listLoading = true;
      this.serLoad = true;
      code && (this.listQuery.CardCode = code)
      // this.listQuery.CardCode = code || ''
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
    handleCollapseChange (val) {
      this.collapseTitle = val.length ? '折叠' : '展开更多订单'
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
      this.copyForm.problemTypeName = res.name;
      this.copyForm.problemTypeId = res.id;
      this.formList[this.sortForm - 1].problemTypeName = res.name;
      this.formList[this.sortForm - 1].problemTypeId = res.id;
      this.problemLabel = res.name;
      this.proplemTree = false;
    },
    solutionClick(res) {
      this.copyForm.solutionsubject = res.subject;
      this.copyForm.solutionId = res.id;
      this.formList[this.sortForm - 1].solutionsubject = res.subject;
      this.formList[this.sortForm - 1].solutionId = res.id;
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
    },
    pushForm() {
        if (!this.formList.length) {
          this.dialogfSN = false;
          return
        }
        let { fromTheme, 
          fromType, 
          feeType, 
          problemTypeName, 
          problemTypeId, 
          priority, 
          remark, 
          solutionId, 
          status, 
          solutionsubject,
          themeList
        } = this.currentTarget || {}
        if (!this.formList[0].manufacturerSerialNumber) {
          //判断从哪里新增的依据是第一个工单是否有id
          if (this.inputname) {
            //是否有新增其他设备选项
            this.inputname = false
            this.formListStart.push({
              manufSN: "其他设备",
              editTrue: false,
              internalSN: "",
              itemCode: "其他设备",
              itemName: "",
              feeType: 1,
              fromTheme: "",
              fromType: "",
              problemTypeName:  "",
              problemTypeId: "",
              priority: 1,
              remark: "",
              solutionId: "",
              status: 1,
              solutionsubject:  "",
            });
          }
            this.formList[0].manufacturerSerialNumber = this.formListStart[0].manufSN
            this.formList[0].internalSerialNumber = this.formListStart[0].internalSN
            this.formList[0].materialCode = this.formListStart[0].itemCode
            this.formList[0].materialDescription = this.formListStart[0].itemName
            this.formList[0].feeType = 1
            this.formList[0].editTrue = true
            // this.formList[0].fromTheme = ""
            this.formList[0].fromType =  this.formList[0].fromType || ""
            // this.formList[0].problemTypeName = ""
            // this.formList[0].problemTypeId = ""
            this.formList[0].priority =  this.formList[0].priority || 1
            // this.formList[0].remark = ""
            // this.formList[0].solutionId = ""
            this.formList[0].status = 1
            // this.formList[0].solutionsubject = ""
          
          const newList = this.formListStart.splice(1,this.formListStart.length);
          for (let i = 0; i < newList.length; i++) {
            this.formList.push({
              manufacturerSerialNumber: newList[i].manufSN,
              editTrue: false,
              internalSerialNumber: newList[i].internalSN,
              materialCode: newList[i].itemCode,
              materialDescription: newList[i].itemName,
              feeType: feeType || 1,
              fromTheme: fromTheme || "",
              themeList: deepClone(themeList) || [],
              fromType: fromType || "",
              problemTypeName: problemTypeName || "",
              problemTypeId: problemTypeId || "",
              priority: priority || 1,
              remark: remark || "",
              solutionId: solutionId || "",
              status: status || 1,
              solutionsubject: solutionsubject || "",
            });
          }
          this.ifFormPush = true;
        } else {
          this.ifFormPush = true;
          if(this.isEditOperation) {
            if (this.inputname) {
              this.inputname = false
              this.formListStart={
                manufSN: "其他设备",
                editTrue: false,
                internalSN: "",
                itemCode: "其他设备",
                itemName: "",
                feeType: 1,
                fromTheme:  "",
                themeList: [],
                fromType:  "",
                problemTypeName:  "",
                problemTypeId:  "",
                priority:  1,
                remark:  "",
                solutionId:  "",
                status:  1,
                solutionsubject:  "",
              }
            }
            this.formList[this.thisPage].manufacturerSerialNumber = this.formListStart.manufSN
            this.formList[this.thisPage].internalSerialNumber = this.formListStart.internalSN
            this.formList[this.thisPage].materialCode = this.formListStart.itemCode
            this.formList[this.thisPage].materialDescription = this.formListStart.itemName
            // (this.formList[this.thisPage].feeType = 1),
            // (this.formList[this.thisPage].serviceMode = 1),
            // (this.formList[this.thisPage].editTrue = false),
            // (this.formList[this.thisPage].fromTheme =""),
            // (this.formList[this.thisPage].fromType =  1),
            // (this.formList[this.thisPage].problemTypeName = ""),
            // (this.formList[this.thisPage].problemTypeId =  ""),
            // (this.formList[this.thisPage].priority =  1),
            // (this.formList[this.thisPage].remark = ""),
            // (this.formList[this.thisPage].solutionId =  ""),
            // (this.formList[this.thisPage].status = 1),
            // (this.formList[this.thisPage].solutionsubject =  "");
          }else{
            if (this.inputname) {
              this.inputname = false
              this.formListStart.push({
                manufSN: "其他设备",
                editTrue: false,
                internalSN: "",
                itemCode: "其他设备",
                materialDescription: "",
                feeType: feeType || 1,
                fromTheme: fromTheme || "",
                themeList: deepClone(themeList) || [],
                fromType: fromType || "",
                problemTypeName: problemTypeName || "",
                problemTypeId: problemTypeId || "",
                priority: priority || 1,
                remark: remark || "",
                solutionId: solutionId || "",
                status: status || 1,
                solutionsubject: solutionsubject || "",
              });
            }
            for (let i = 0; i < this.formListStart.length; i++) {
              this.formList.push({
                manufacturerSerialNumber: this.formListStart[i].manufSN,
                editTrue: false,
                internalSerialNumber: this.formListStart[i].internalSN,
                materialCode: this.formListStart[i].itemCode,
                materialDescription: this.formListStart[i].itemName,
                feeType: feeType || 1,
                fromTheme: fromTheme || "",
                themeList: deepClone(themeList) || [],
                fromType: fromType || "",
                problemTypeName: problemTypeName || "",
                problemTypeId: problemTypeId || "",
                priority: priority || 1,
                remark: remark || "",
                solutionId: solutionId || "",
                status: status || 1,
                solutionsubject: solutionsubject || "",
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
        this.isEditOperation = index !== undefined
          ? Boolean(value.manufacturerSerialNumber) 
          : false
        this.dialogfSN = true;
      });
    },
    addWorkOrder(result, index) {
      const { itemForm, itemFormList } = this.$refs;
      const targetForm = index !== undefined ? itemFormList[index] : itemForm;
      targetForm.validate((valid) => {
        if (valid) {
          console.log('新增工单陈工')
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
      this.getSerialNumberList();
    },
    searchSelect(res) {
      if (res.count) {
        this.filterSerialNumberList = this.filterSerialNumberList.filter(
          (item) => item.manufSN === res.manufSN
        );
      }
      this.formList[this.thisPage].manufacturerSerialNumber = res.manufSN;
      this.formList[this.thisPage].internalSerialNumber = res.internalSN;
      this.formList[this.thisPage].contractId = res.contractID;
      this.formList[this.thisPage].materialCode = res.itemCode;
      this.formList[this.thisPage].materialDescription = res.itemName;
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
  padding-top: 2px;
  border-top: 1px solid silver;
  overflow-y: scroll;
  overflow-x: hidden;
  .order-wrapper {
    position: relative;
    .form-disabled {
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
    .operation-btn-wrapper {
      position: absolute;
      display: flex;
      top: 15px;
      right: 26px;
    }
    .report-btn-wrapper {
      position: absolute;
      right: 12px;
      bottom: 53px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 12px;
    }
  }
  .radio-item {
    display: flex;
    flex-direction: column;
  }
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
  .confirm-add-btn {
    position: absolute;
    bottom: 7px;
    right: 30px;
  }
  ::v-deep .el-date-editor {
    .el-input__inner {
      width: 100% !important;
    }
  }
  ::v-deep .el-radio {
    margin-left: 0 !important;
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
<style lang="scss">
/* 设置呼叫主题文字提示的最大宽度 */
body {
  .el-tooltip__popper  {
    &.form-theme-toolip {
      max-width: 480px;
    }
  }
}
</style>


      