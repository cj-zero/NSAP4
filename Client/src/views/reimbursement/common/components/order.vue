<template>
  <div class="order-wrapper" v-loading.fullscreen="orderLoading">
    <el-row type="flex" class="head-title-wrapper">
      <p>报销单号: <span>{{ formData.mainId }}</span></p>
      <p>报销人: <span>{{ formData.userName }}</span></p>
      <p>部门: <span>{{ formData.orgName }}</span></p>
      <p>劳务关系: <span>{{ formData.serviceRelations }}</span></p>
      <p>创建时间: <span>{{ formData.createTime }}</span></p>
    </el-row>
    <!-- 主表单 -->
    <el-scrollbar class="scroll-bar">
      <el-form
        :model="formData"
        ref="form"
        class="my-form-wrapper"
        :class="{ 'uneditable': !this.ifFormEdit }"
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
            <el-form-item
              :prop="item.prop"
              :rules="rules[item.prop] || { required: false }">
              <span slot="label">
                <template v-if="item.prop === 'serviceOrderSapId' && title !== 'create'">
                  <div class="link-container" style="display: inline-block">
                    <span>{{ item.label }}</span>
                    <img :src="rightImg" @click="openTree(formData, false)" class="pointer">
                  </div>
                </template>
                <template v-else>
                  <span :class="{ 'upload-title money': item.label === '总金额'}">{{ item.label }}</span>
                </template>
              </span>
              <template v-if="!item.type">
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
                  :style="{ width: item.width }"
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
                  class="customer-btn-class"
                  type="primary" 
                  style="width: 100%;" 
                  @click="item.handleClick(formData)"
                  :loading="reportBtnLoading">{{ item.btnText }}</el-button>
              </template>
              <template v-else-if="item.type === 'money'">
                <span class="money-text">￥{{ totalMoney | toThousands }}</span>
              </template>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <!-- 附件上传 -->
      <el-row type="flex" class="upload-wrapper">
        <el-col :span="this.ifFormEdit ? 15 : 18">
          <el-row type="flex" v-if="ifCOrE || formData.attachmentsFileList.length">
            <span class="upload-title">上传附件</span>
            <upLoadFile 
              :disabled="!ifFormEdit"
              @get-ImgList="getFileList" 
              uploadType="file" 
              ref="uploadFile" 
              :maxSize="maxSize"
              :ifShowTip="ifFormEdit"
              :fileList="formData.attachmentsFileList || []"
              @deleteFileList="deleteFileList"></upLoadFile>
          </el-row>
        </el-col>
        <!-- <el-col :span="this.ifFormEdit ? 9: 6" style="position: relative;top: -35px;">
          <el-row type="flex" >
            <span class="upload-title money">总金额</span>
            <span class="money-text">￥{{ totalMoney | toThousands }}</span>
          </el-row>
        </el-col> -->
      </el-row>
      <!-- 出差 -->
      <div class="form-item-wrapper travel" :class="{ uneditable: !this.ifFormEdit }" v-if="ifCOrE || formData.reimburseTravellingAllowances.length">
        <el-button v-if="ifShowTravel" @click="showForm(formData.reimburseTravellingAllowances, 'ifShowTravel')">添加出差补贴</el-button>
        <el-form 
          v-else
          ref="travelForm" 
          :model="formData" 
          size="mini" 
          :show-message="false"
          class="form-wrapper"
          :disabled="!ifFormEdit"
          :class="{ 'uneditable': !this.ifFormEdit }"
        >
          <div class="title-wrapper">
            <div class="number-count">总数量:{{ travelCount }}个</div>
            <div class="title">
              <span>出差补贴</span>
              <p class="total-money">总金额: ￥{{ travelTotalMoney | toThousands }}</p>
            </div>
          </div>
          <el-table 
            border
            :data="formData.reimburseTravellingAllowances"
            @cell-click="onTravelCellClick"
          >
            <el-table-column
              v-for="item in travelConfig"
              :key="item.label"
              :label="item.label"
              :width="item.width"
              :align="item.align || 'left'"
              :prop="item.prop"
              :fixed="item.fixed"
              :resizable="false"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseTravellingAllowances.' + scope.$index + '.'+ item.prop"
                    :rules="travelRules[item.prop] || { required: false }"
                  >
                    <el-input v-model="scope.row[item.prop]" :disabled="item.disabled" :placeholder="item.placeholder"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseTravellingAllowances.' + scope.$index + '.'+ item.prop"
                    :rules="travelRules[item.prop] || { required: false }"
                  >
                    <el-input 
                      v-model="scope.row[item.prop]" 
                      :type="item.type" :min="0" 
                      :disabled="item.disabled" 
                      @input="onTravelInput"
                      :class="{ 'money-class': item.prop === 'money'}"
                    ></el-input>
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
          </el-table>
        </el-form>
      </div>
      <!-- 交通 -->
      <div class="form-item-wrapper" v-if="ifCOrE || formData.reimburseFares.length">
        <el-button v-if="ifShowTraffic" @click="showForm(formData.reimburseFares, 'ifShowTraffic')">添加交通费用</el-button>
        <el-form 
          v-else
          ref="trafficForm" 
          :model="formData" 
          size="mini" 
          :show-message="false"
          class="form-wrapper"
          :disabled="!ifFormEdit"
          :class="{ 'uneditable': !this.ifFormEdit }"
        >
          <div class="title-wrapper">
            <div class="number-count">总数量:{{ trafficCount }}个</div>
            <div class="title">
              <span>交通费用</span>
              <p class="total-money">总金额: ￥{{ trafficTotalMoney | toThousands }}</p>
            </div>
          </div>
          <el-table 
            :row-style="rowStyle"
            border
            :data="formData.reimburseFares"
            max-height="10000px"
            @cell-click="onTrafficCellClick"
          >
            <el-table-column
              v-for="item in trafficConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :width="item.width"
              :fixed="item.fixed"
              :resizable="false"
              
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (trafficRules[item.prop] || { required: false }) : { required: false }"
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
                      <template v-if="ifFormEdit && (item.prop === 'from' || item.prop === 'to')">  
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
                    :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (trafficRules[item.prop] || { required: false }) : { required: false }"
                  >
                    <el-input 
                      v-model="scope.row[item.prop]" 
                      :type="item.type" 
                      :disabled="item.disabled" 
                      :min="0"
                      :class="{ 'money-class': item.prop === 'money'}"
                      @input="onTrafficInput" 
                      @focus="onFocus({ prop: item.prop, index: scope.$index })"
                      :placeholder="item.placeholder"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (trafficRules[item.prop] || { required: false }) : { required: false }"
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
                    :options="{ prop: item.prop, index: scope.$index, type: 'traffic' }" 
                    :ifShowTip="ifFormEdit"
                    @deleteFileList="deleteFileList"
                    :onAccept="onAccept"
                    :fileList="
                      formData.reimburseFares[scope.$index] 
                        ? (item.prop === 'invoiceAttachment' 
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
          </el-table>
        </el-form>
      </div>
      <!-- 住宿 -->
      <div class="form-item-wrapper acc" :class="{ uneditable: !this.ifFormEdit }" v-if="ifCOrE || formData.reimburseAccommodationSubsidies.length">
        <el-button v-if="ifShowAcc" @click="showForm(formData.reimburseAccommodationSubsidies, 'ifShowAcc')">添加住宿补贴</el-button>
        <el-form 
        v-else
        ref="accForm" 
        :model="formData" 
        size="mini" 
        :show-message="false"
        class="form-wrapper"
        :disabled="!ifFormEdit"
        :class="{ 'uneditable': !this.ifFormEdit }"
        >
          <div class="title-wrapper">
            <div class="number-count">总数量:{{ accCount }}个</div>
            <div class="title">
              <span>住宿补贴</span>
              <p class="total-money">总金额: ￥{{ accTotalMoney | toThousands }}</p>
            </div>
          </div>
          <el-table 
            :row-style="rowStyle"
            border
            :data="formData.reimburseAccommodationSubsidies"
            max-height="10000px"
            @cell-click="onAccCellClick"
          >
            <el-table-column
              v-for="item in accommodationConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :width="item.width"
              :fixed="item.fixed"
              :resizable="false"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseAccommodationSubsidies.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (accRules[item.prop] || { required: false }) : { required: false }"
                  >
                    <el-input 
                      v-model="scope.row[item.prop]" 
                      :placeholder="item.placeholder"
                      :disabled="item.disabled || (item.prop === 'invoiceNumber' && scope.row.isValidInvoice)" 
                      @change="onChange">
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
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseAccommodationSubsidies.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (accRules[item.prop] || { required: false }) : { required: false }"
                  >
                    <el-input 
                      :class="{ 'money-class': item.prop === 'money' || item.prop === 'totalMoney' }"
                      v-model="scope.row[item.prop]" 
                      :type="item.type" 
                      :disabled="item.disabled" 
                      :min="0" 
                      @change="onChange"
                      @blur="onBlur"
                      @input="onAccInput"
                      @focus="onFocus({ prop: item.prop, index: scope.$index })"
                      :placeholder="item.placeholder"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'reimburseAccommodationSubsidies.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (accRules[item.prop] || { required: false }) : { required: false }"
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
                    :options="{ prop: item.prop, index: scope.$index, isAcc: true, type: 'acc' }"
                    :ifShowTip="ifFormEdit"
                    @deleteFileList="deleteFileList"
                    :onAccept="onAccept"
                    :fileList="
                      formData.reimburseAccommodationSubsidies[scope.$index] 
                        ? (item.prop === 'invoiceAttachment' 
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
          </el-table>
        </el-form>
      </div>
      <!-- 其它 -->
      <div class="form-item-wrapper other" :class="{ uneditable:!this.ifFormEdit }" v-if="ifCOrE || formData.reimburseOtherCharges.length">
        <el-button v-if="ifShowOther" @click="showForm(formData.reimburseOtherCharges, 'ifShowOther')">添加其他费用</el-button>
        <el-form 
          v-else
          ref="otherForm" 
          :model="formData" 
          size="mini" 
          :show-message="false"
          class="form-wrapper"
          :disabled="!ifFormEdit"
          :class="{ 'uneditable': !this.ifFormEdit }"
        >
          <div class="title-wrapper">
            <div class="number-count">总数量:{{ otherCount }}个</div>
            <div class="title">
              <span>其他费用</span>
              <p class="total-money">总金额: ￥{{ otherTotalMoney | toThousands }}</p>
            </div>
          </div>
          <el-table 
            :row-style="rowStyle"
            border
            :data="formData.reimburseOtherCharges"
            max-height="10000px"
            @cell-click="onOtherCellClick"
          >
            <el-table-column
              v-for="item in otherConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
              :width="item.width"
              :fixed="item.fixed"
              :resizable="false"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                  <el-form-item
                    :prop="'reimburseOtherCharges.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (otherRules[item.prop] || { required: false }) : { required: false }"
                  >
                    <el-input 
                      :placeholder="item.placeholder"
                      v-model="scope.row[item.prop]" 
                      :disabled="item.disabled || (item.prop === 'invoiceNumber' && scope.row.isValidInvoice)"
                    >
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
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'number'">
                  <el-form-item
                    :prop="'reimburseOtherCharges.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (otherRules[item.prop] || { required: false }) : { required: false }"
                  >
                    <el-input 
                      :class="{ 'money-class': item.prop === 'money'}"
                      v-model="scope.row[item.prop]" 
                      :type="item.type" 
                      :disabled="item.disabled" 
                      :min="0" 
                      @focus="onFocus({ prop: item.prop, index: scope.$index })" 
                      @input="onOtherInput"
                      :placeholder="item.placeholder"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'reimburseOtherCharges.' + scope.$index + '.'+ item.prop"
                    :rules="scope.row.isAdd ? (otherRules[item.prop] || { required: false }) : { required: false }"
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
                    :options="{ prop: item.prop, index: scope.$index, type: 'other' }"
                    :ifShowTip="ifFormEdit"
                    @deleteFileList="deleteFileList"
                    :onAccept="onAccept"
                    :fileList="
                      formData.reimburseOtherCharges[scope.$index] 
                        ? (item.prop === 'invoiceAttachment' 
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
          </el-table>
        </el-form>
      </div> 
      <!-- 操作记录 -->
      <template v-if="this.title !== 'create' && this.title !== 'edit' && this.formData.reimurseOperationHistories.length">
        <div style="width: 898px;" class="history-wrapper">
          <el-table 
            style="width: 989px;"
            :data="formData.reimurseOperationHistories"
            border
            max-height="200px"
          >
            <el-table-column label="操作记录" prop="action" width="150px" show-overflow-tooltip></el-table-column>
            <el-table-column label="操作人" prop="createUser" width="150px" show-overflow-tooltip></el-table-column>
            <el-table-column label="操作时间" prop="createTime" width="150px" show-overflow-tooltip></el-table-column>
            <el-table-column label="审批时长" prop="intervalTime" width="150px" show-overflow-tooltip>
              <template slot-scope="scope">
                {{ scope.row.intervalTime | m2DHM }}
              </template>
            </el-table-column>
            <el-table-column label="审批结果" prop="approvalResult" width="150px" show-overflow-tooltip></el-table-column>
            <el-table-column label="备注" prop="remark" width="147px" show-overflow-tooltip></el-table-column>
          </el-table>
        </div>
      </template>
    </el-scrollbar>
    
    <!-- 客户选择列表 -->
    <my-dialog 
      ref="customerDialog" 
      width="621px" 
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
      :loading="costLoading"
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
      :onClosed="resetReport">
      <Report :data="reportData" ref="report"/>
    </my-dialog>
    <!-- 确认审批弹窗 -->
    <my-dialog
      ref="approve"
      :center="true"
      :title="remarkTitle"
      :mAddToBody="true" 
      :appendToBody="true"
      :btnList="remarkBtnList"
      :closed="onApproveClose"
      v-loading="remarkLoading"
      width="350px">
      <remark ref="remark" @input="onRemarkInput" :tagList="reimburseTagList" :title="title"></remark>
    </my-dialog>
    <!-- 只能查看的表单 -->
    <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
      :mAddToBody="true" 
      :appendToBody="true"
    >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
            :form="temp"
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
  </div>
</template>

<script>
import { addOrder, getOrder, updateOrder, approve, isSole } from '@/api/reimburse'
import { getList } from '@/api/reimburse/mycost'
import { forServe } from '@/api/serve/callservesure'
import upLoadFile from "@/components/upLoadFile";
import Pagination from '@/components/Pagination'
import MyDialog from '@/components/Dialog'
import CommonTable from '@/components/CommonTable'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import Report from './report'
import Remark from './remark'
import AreaSelector from '@/components/AreaSelector'
import { toThousands } from '@/utils/format'
import { findIndex } from '@/utils/process'
import { deepClone } from '@/utils'
import { travelRules, trafficRules, accRules, otherRules } from '../js/customerRules'
import { customerColumns, costColumns } from '../js/config'
import { noop } from '@/utils/declaration'
import { categoryMixin, reportMixin, attachmentMixin, chatMixin } from '../js/mixins'
import { REIMBURSE_TYPE_MAP, IF_SHOW_MAP, REMARK_TEXT_MAP } from '../js/map'
import rightImg from '@/assets/table/right.png'
export default {
  inject: ['parentVm'],
  mixins: [categoryMixin, reportMixin, attachmentMixin, chatMixin],
  components: {
    upLoadFile,
    Pagination,
    MyDialog,
    CommonTable,
    Report,
    Remark,
    AreaSelector,
    zxform,
    zxchat
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
      orderLoading: false, // orderWrapper loading
      rightImg, // 箭头图标
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
        myExpendsIds: [] // 需要删除的导入数据（我的费用ID）
      },
      labelWidth: '80px',
      disabled: false,
      labelPosition: 'right',
      limit: 8,
      travelRules,
      trafficRules,
      accRules,
      otherRules,
      deleteList: [], // 删除表单列项
      customerColumns, // 用户列表表格配置
      customerInfoList: [], // 用户信息列表
      customerTotal: 0, // 用户列表总数
      customerLoading: false, // 用户选择按钮的loading
      listQuery: { // 用户列表的查询参数
        page: 1,
        limit: 30
      },
      costColumns,
      costBtnList: [
        { btnText: '确认', handleClick: this.importConfirm },
        { btnText: '取消', handleClick: this.closeCostDialog }
      ],
      costTotal: 0, // 费用列表总数
      costData: [], // 费用列表信息
      costLoading: false, // 点击导入之后按钮loading
      listQueryCost: {
        page: 1,
        limit: 30
      },
      remarkBtnList: [
        { btnText: '确认', handleClick: this.approve },
        { btnText: '取消', handleClick: this.closeRemarkDialog, className: 'close' }
      ],
      selectedList: [], // 费用列表导出的数据，用来后续判断导出列表中是否可选
      remarkType: '', // 
      remarkText: '', // 弹窗备注
      remarkLoading: false,
      prevAreaData: null // 上一次点击地址框的是时候，交通表格的行数据
    }
  },
  watch: {
    customerInfo: {
      immediate: true,
      deep: true,
      handler (val) {
        this.formData.createUserId = val.createUserId
        this.formData.userName = val.userName
        this.formData.orgName = val.orgName
        this.formData.serviceRelations = val.serviceRelations
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
        if (this.title === 'approve') { // 审批的时候要告诉审批人 住宿金额补贴是否符合标准
          this._checkAccMoney()
        }
        if (this.title === 'create' || this.title === 'edit') { // 只有在create或者edit的时候，才可以导入费用模板
          this._getCostList() // 获取费用模板
        }
      }
    },
    totalMoney (val) {
      this.formData.totalMoney = val.toFixed(2)
    },
    formData: {
      deep: true,
      handler () {
        // console.log(this.formData, 'formData last')
      }
    }
  },
  computed: {
    ifFormEdit () { // 是否可以编辑
      return this.title === 'view'
        ? false
        : this.title === 'create' || this.title === 'edit'
    },
    ifCOrE () { // 审核的时候如果没有值就不显示 在新增或者编辑的时候一定会展示
      return (this.title === 'create' || this.title === 'edit')
    },
    travelCount () { // 出差表格的总行数
      return this.formData.reimburseTravellingAllowances.length
    },
    trafficCount () { // 交通表格的总行数
      return this.formData.reimburseFares.filter(item => item.isAdd).length
    },
    accCount () { // 住房表格的总行数
      return this.formData.reimburseAccommodationSubsidies.filter(item => item.isAdd).length
    },
    otherCount () { // 其他表格的总行数
      return this.formData.reimburseOtherCharges.filter(item => item.isAdd).length
    },
    travelTotalMoney () {
      let { reimburseTravellingAllowances } = this.formData
      if (reimburseTravellingAllowances.length) {
        return this.getTotal(reimburseTravellingAllowances) * (reimburseTravellingAllowances[0].days || 0)
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
        return this.getTotal(reimburseOtherCharges)
      }
      return 0
    },
    totalMoney () {
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
    rules () { // 报销单上面的表单规则
      return {
        serviceOrderSapId: [ { required: true } ],
        reimburseType: [ { required: true, trigger: ['change', 'blur'] } ],
        shortCustomerName: [{ required: true, trigger: 'blur' }],
        // projectName: [ { required: true, trigger: ['change', 'blur'] } ],
        bearToPay: [ { required: this.isCustomerSupervisor, trigger: ['change', 'blur']} ],
        // responsibility: [ { required: true, trigger: ['change', 'blur'] } ],
        serviceRelations: [ { required: true, trigger: ['change', 'blur'] } ]
      }
    },
    remarkTitle () {
      return `确认${REMARK_TEXT_MAP[this.remarkType]}此次报销`
    },
    customerBtnList () {
      return [
        { btnText: '确认', handleClick: this.confirm, loading: this.customerLoading },
        { btnText: '取消', handleClick: this.closeDialog }
      ]
    },
    travelMoney () {
      // 以R或者M开头都是65
      return  this.isROrM ? '65' : '50'
    },
    isROrM () { // 判断报销人部门是不是R/M
      return /^[R|M]/i.test(this.userOrgName)
    },
    accMaxMoney () { // 住宿补贴的最大值
      // R、M部人员
      let isMatched = false
      if (this.formData.destination) {
        for (let key in this.reimburseAccCityList) {
          // key 城市名称
          let reg = new RegExp(key)
          let { dtValue, description } = this.reimburseAccCityList[key]
          if (reg.test(this.formData.destination)) { // 如果能匹配到对应的城市
            isMatched = true
            return this.isROrM ? dtValue : description
          }
        }
        if (!isMatched) {
          let { dtValue, description } = this.reimburseAccCityList['其他城市']
          return this.isROrM ? dtValue : description
        }
      }
      return 0
    }
  },
  methods: {
    rowStyle ({ row }) {
      if (!row.isAdd) {
        return {
          display: 'none'
        }
      }
    },
    noop () {
      noop() 
    },
    getTotal (data) { // 获取总金额
      let result = 0
      result += data.reduce((prev, next) => {
        return next.isAdd 
          ? prev + parseFloat(String(next.totalMoney || next.money || 0)) 
          : prev + 0
      }, 0)
      return this.isValidNumber(result) ? result : 0
    },
    showForm (data, type) { // 展示表格
      if (!this.ifFormEdit) return
      let { businessTripDate, endDate } = this.formData
      switch (type) {
        case 'ifShowTravel':
          data.push({
            id: '',
            isAdd: true,
            days: this.calculateDays(businessTripDate, endDate),
            money: this.travelMoney,
            remark: '',
          })
          break
        case 'ifShowTraffic':
          data.push({
            id: '',
            isAdd: true,
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
            isAdd: true,
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
            isAdd: true,
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
        let ifInvoiceAttachment = true
        if (ref !== 'travelForm') {
          for (let i = 0; i < this.formData[data].length; i++) {
            let dataItem = this.formData[data][i]
            let { isAdd, otherFileList, otherAttachment } = dataItem
            let hasAttachment = this.hasAttachment(dataItem)
            if (isAdd) { // 被删除的就不做校验判断
              // 新增的时候
              if (hasAttachment) { // 说明一定要有附件发票,并且有了发票
                ifInvoiceAttachment = true
              } else { 
                // 一定不能有附件发票，但必须至少有一个其它发票
                if (otherFileList.length) {
                  let fileListDeleted = true
                  for (let i = 0; i < this.formData.fileId.length; i++) {
                    let fileId = this.formData.fileId[i]
                      if (!otherFileList.some(item => item.id === fileId)) {
                        fileListDeleted = false
                        break
                      }
                  }
                  let ifDeleted = otherFileList[0].reimburseId // 判断是导入数据还是编辑的数据
                    ? this.formData.fileId.length && fileListDeleted
                    : otherFileList.every(item => !item.isAdd)
                  ifInvoiceAttachment = ifDeleted
                    ? Boolean(otherAttachment && otherAttachment.length) 
                    : true
                } else {
                  ifInvoiceAttachment = Boolean(otherAttachment && otherAttachment.length)
                }
                if (!ifInvoiceAttachment) break
              }
            }
          }
        }
        let isValid = await this.$refs[ref].validate()
        // console.log('valid', isValid, this.formData[data], ifInvoiceAttachment)
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
    getTrafficList (val, { prop, index, fileId, uploadVm, operation }) {
      let data = this.formData.reimburseFares
      let currentRow = data[index]
      let attachmentConfig = {
        data,
        index,
        prop,
        val,
        reimburseType: 2
      }
      // 删除操作也不进行识别
      if (fileId && prop === 'invoiceAttachment' && !operation) { // 图片上传成功会返回当前的pictureId, 并且只识别发票附件 
        this.identifyLoading = this.$loading({
          lock: true,
          text: '发票识别中',
          background: 'rgba(0, 0, 0, 0.7)'
        })
        this._identifyInvoice({ // 先进行识别再进行赋值
          fileId, 
          currentRow, 
          uploadVm,
          tableType: 'traffic'
        }).then(isValid => {
          this.identifyLoading.close()
          isValid 
            ? this._setAttachmentList(attachmentConfig) 
            : this._setAttachmentList({ ...attachmentConfig, ...{ val: [] }})
        })
      } else {
        this._setAttachmentList(attachmentConfig)
      }
    },
    getAccList (val, { prop, index, fileId, uploadVm, operation }) {
      let data = this.formData.reimburseAccommodationSubsidies
      let currentRow = data[index]
      let attachmentConfig = {
        data,
        index,
        prop,
        val,
        reimburseType: 3
      }
      // 删除操作也不进行识别
      if (fileId && prop === 'invoiceAttachment' && !operation) { // 图片上传成功会返回当前的pictureId, 并且只识别发票附件 
        this.identifyLoading = this.$loading({
          lock: true,
          text: '发票识别中',
          background: 'rgba(0, 0, 0, 0.7)'
        })
        this._identifyInvoice({ // 先进行识别再进行赋值
          fileId, 
          currentRow, 
          uploadVm,
          tableType: 'acc'
        }, true).then(isValid => {
          this.identifyLoading.close()
          isValid 
            ? this._setAttachmentList(attachmentConfig) 
            : this._setAttachmentList({ ...attachmentConfig, ...{ val: [] }})
        })
      } else {
        this._setAttachmentList(attachmentConfig)
      }
    },
    getOtherList (val, { prop, index, fileId, uploadVm, operation }) {
      let data = this.formData.reimburseOtherCharges
      let currentRow = data[index]
      let attachmentConfig = {
        data,
        index,
        prop,
        val,
        reimburseType: 4
      }
      // 删除操作也不进行识别
      if (fileId && prop === 'invoiceAttachment' && !operation) { // 图片上传成功会返回当前的pictureId, 并且只识别发票附件 
        this.identifyLoading = this.$loading({
          lock: true,
          text: '发票识别中',
          background: 'rgba(0, 0, 0, 0.7)'
        })
        this._identifyInvoice({ // 先进行识别再进行赋值
          fileId, 
          currentRow, 
          uploadVm,
          tableType: 'other'
        }).then(isValid => {
          this.identifyLoading.close()
          isValid 
            ? this._setAttachmentList(attachmentConfig) 
            : this._setAttachmentList({ ...attachmentConfig, ...{ val: [] }})
        })
      } else {
        this._setAttachmentList(attachmentConfig)
      }
    },
    setCurrentIndex (data, row) {
      this.currentRow = row
      this.currentIndex = findIndex(data, item => item === row)
    },
    onTravelCellClick (row, column) {
      this.tableType = 'travel'
      this.setCurrentProp(column, row)
    },
    onTrafficCellClick (row, column) {
      this.tableType = 'traffic' // 判断当前点击的是哪个表格
      this.setCurrentProp(column, row)
      this.setCurrentIndex(this.formData.reimburseFares, row)
    },
    onAccCellClick (row, column) {
      this.tableType = 'acc' // 判断当前点击的是哪个表格
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
    onTravelInput (value) {
      let { businessTripDate, endDate } = this.formData
      let actDays = this.calculateDays(businessTripDate, endDate)
      if (actDays && value > actDays) { // 限制最大的天数
        this.formData.reimburseTravellingAllowances[0].days = actDays
      }
    },
    onChange (value) { // 天数 总金额 计算
      this.changeMoneyByDaysOrTotalMoney(value)
    },
    onBlur () {
      // let value = e.target.value
      // this.changeMoneyByDaysOrTotalMoney(value)
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
        if (Number((totalMoney / days).toFixed(2)) > Number(this.accMaxMoney)) {
          this.$message.warning(`所填金额大于住宿补贴标准(${this.accMaxMoney}元)`)
        }
        this.$set(data, 'money', (totalMoney / days).toFixed(2))
      }
    },
    onTrafficInput (value) {
      let data = this.formData.reimburseFares
      this.onInput(data, value)
    },
    onAccInput (value) {
      let data = this.formData.reimburseAccommodationSubsidies
      this.onInput(data, value, 'acc')
    },
    onOtherInput (value) {
      let data = this.formData.reimburseOtherCharges
      this.onInput(data, value)
    },
    onInput (data, value, type) {
      let currentRow = data[this.currentIndex]
      let { invoiceFileList, invoiceAttachment, maxMoney, invoiceNumber, id } = currentRow
      let selectedIdList = this.selectedList.map(item => item.id)
      if (
        (
          (invoiceFileList.length && !selectedIdList.includes(id) && !this.formData.fileId.includes(invoiceFileList[0].id)) || // 存在回显的文件代表已经新增的，并且还没被删除过
          (invoiceAttachment.length && invoiceNumber) || 
          (invoiceFileList.length && invoiceFileList[0].isAdd && selectedIdList.includes(id)) // 导入的数据
        ) && maxMoney
      ) {
        if (this.currentProp === 'totalMoney' || this.currentProp === 'money') { // 只算修改totalMoney或者money字段
          type === 'acc'
            ? currentRow.totalMoney = Math.min(parseFloat(value), maxMoney)
            : currentRow.money = Math.min(parseFloat(value), maxMoney)
        }
      }
    },
    onFocus ({ prop, index }) {
      this.currentProp = prop
      this.currentIndex = index
    },
    onAreaFocus ({ prop, index }) { // 打开地址选择
      if (this.prevAreaData) {
        this.prevAreaData.ifFromShow = false
        this.prevAreaData.ifToShow = false
      }
      if (prop === 'from' || prop === 'to') {
        let currentRow = this.formData.reimburseFares[index]
        prop === 'from'
          ? this.$set(currentRow, 'ifFromShow', true)
          : this.$set(currentRow, 'ifToShow', true)
        this.prevAreaData = currentRow
      }
    },
    onCloseArea (options) { // 关闭地址选择器
      let { prop, index } = options
      let currentRow = this.formData.reimburseFares[index]
      prop === 'from'
          ? this.$set(currentRow, 'ifFromShow', false)
          : this.$set(currentRow, 'ifToShow', false)
      this.prevAreaData = null
    },
    onAreaChange (val) {
      let { province, city, district, prop, index } = val
      let currentRow = this.formData.reimburseFares[index]
      const countryList = ['北京市', '天津市', '上海市', '重庆市']
      let result = ''
      result = countryList.includes(province)
        ? province + district
        : city + district
      currentRow[prop] = result
      this.prevAreaData = null
    },
    changeAddr (scope) { // 交通表格 交换出发地和目的地
      if (!this.ifFormEdit) return
      let { row, $index: index } = scope
      let { from, to } = row
      let data = this.formData.reimburseFares[index]
      data.from = to
      data.to = from
    },
    addAndCopy (scope, data, type, operationType) {
      if (!this.ifFormEdit) return
      // console.log(scope.row, data, type, operationType, 'operationType') // 判断是新增还是复制
      let { row } = scope
      switch (type) {
        case 'traffic':
          data.push({
            id: '',
            isAdd: true,
            trafficType: operationType === 'add' ? '' : row.trafficType,
            transport: operationType === 'add' ? '' : row.transport,
            from: operationType === 'add' ? '' : row.from,
            to: operationType === 'add' ? '' : row.to,
            money: '',
            maxMoney: '',
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
            isAdd: true,
            days: operationType === 'add' ? '' : row.days,
            money: '',
            totalMoney: '',
            maxMoney: '',
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
            isAdd: true,
            expenseCategory: operationType === 'add' ? '' : row.expenseCategory,
            money: '',
            maxMoney: '',
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
      let { id, invoiceFileList, otherFileList } = scope.row
      if (id) { // 说明已经新建过的,新建过的表格数据 invoceFileList 或者otherFileList 是一定存在的
        if (
          (invoiceFileList && invoiceFileList.length) ||
          (otherFileList && otherFileList.length)  
        ) {
          let data = invoiceFileList.length ? invoiceFileList : otherFileList
          if (data[0].reimburseId) { // 导入的数据reimburseId是为空的，所以不需要添加到delteReimburse中
            this.formData.delteReimburse.push({
              deleteId: id,
              reimburseType: REIMBURSE_TYPE_MAP[type]
            })
          } else {
            let index = findIndex(this.selectedList, item => item.id === id) // 找到当前删除行 对应导入之后的数据列表的索引值
            if (index !== -1) {
              this.selectedList.splice(index, 1) // 删除后，让导入的表格回复对应的可选状态
            }
          }
        } else {
          this.formData.delteReimburse.push({
            deleteId: id,
            reimburseType: REIMBURSE_TYPE_MAP[type]
          })
        }
      } 
      scope.row.isAdd = false // 将行数据设置display: none
      // data.splice(scope.$index, 1)
      let ifAllDeleted = data.every(item => item.isAdd === false)
      if (ifAllDeleted) {
        this[IF_SHOW_MAP[type]] = true
        this.deleteTableList(type)
      }
    },
    deleteTableList (type) { // 当删除完之后需要清空数组，因为直接删除会导致回显
      switch (type) {
        case 'travel':
          this.formData.reimburseTravellingAllowances = []
          break
        case 'traffic':
          this.formData.reimburseFares = []
          break
        case 'accommodation':
          this.formData.reimburseAccommodationSubsidies = []
          break
        case 'other':
          this.formData.reimburseOtherCharges = []
          break
      }
    },
    deleteFileList (file, { index, prop, isAcc, type }) {
      let { reimburseId, id } = file
      // 在编辑的时候，(针对)删除已经新增过的附件, 如果是删除导入的附件，ID会为'' ,直接略过
      if (reimburseId) { // 删除新增过的
        this.formData.fileId.push(id)
      } else {
        // 删除导入的模板
        file.isAdd = false
      }
      if (prop === 'invoiceAttachment') {
        let { reimburseFares, reimburseOtherCharges, reimburseAccommodationSubsidies } = this.formData
        let data = type === 'traffic'
          ? reimburseFares
          : type === 'acc'
            ? reimburseAccommodationSubsidies
            : reimburseOtherCharges
        let currentRow = data[index]
        this._setCurrentRow(currentRow, {
          invoiceNo: '',
          money: '',
          isAcc,
          isValidInvoice: false
        })
      } 
    },
    hasAttachment (data) { // 判断是否存在附件发票   
      let { invoiceFileList, invoiceAttachment, id } = data
      let selectedIdList = this.selectedList.map(item => item.id)
      return (invoiceFileList.length && !selectedIdList.includes(id) && !this.formData.fileId.includes(invoiceFileList[0].id)) || // 存在回显的文件代表已经新增的，并且还没被删除过
        (invoiceFileList.length && invoiceFileList[0].isAdd && selectedIdList.includes(id)) ||// 导入的数据
        invoiceAttachment.length
    },
    isValidNumber (val) { // 判断是否是有效的数字
      val = Number(val)
      return !isNaN(val) && val >= 0
    },
    clearFile () { // 删除上传的文件
      if (this.$refs.uploadFile) {
        this.$refs.uploadFile.clearFiles()
      }
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
        this.$message.error('获取用户信息失败')
      })
    },
    closeDialog () {
      this.$refs.customerTable.resetRadio()
      this.$refs.customerDialog.close()
    },
    calculateDays (start, end) { // 计算出发日期和结束日期的时间间隔 按日计算
      if (start && end) {
        start = +new Date(start.split(' ')[0])
        end = +new Date(end.split(' ')[0])
        return Math.floor((end - start) / 1000 / 60 / 60 / 24) + 1
      }
      return ''
    },
    confirm () {
      let currentRow = this.$refs.customerTable.getCurrentRow()
      if (currentRow) {
        let { 
          userName,
          serviceRelations,
          orgName,
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
        let formData = this.formData // 对报销人的信息进行赋值
        formData.userName = userName
        formData.orgName = orgName
        formData.serviceRelations = serviceRelations
        formData.terminalCustomerId = terminalCustomerId
        formData.terminalCustomer = terminalCustomer
        formData.serviceOrderId = id
        formData.serviceOrderSapId = u_SAP_ID
        formData.fromTheme = fromTheme
        formData.createUserId = userId
        formData.becity = becity
        formData.businessTripDate = businessTripDate
        formData.endDate = endDate
        formData.destination = destination
        this.customerLoading = true
        this._checkTravelDays(currentRow)
        this._checkAccMoney()
        forServe(terminalCustomerId).then(res => {
          formData.shortCustomerName = res.result.u_Name ? res.result.u_Name.slice(0, 6) : ''
          this.customerLoading = false
        }).catch(() => {
          formData.shortCustomerName = ''
          this.customerLoading = false
        })
      }
      this.closeDialog()
    },
    _checkTravelDays (currentRow) {
      let { businessTripDate, endDate } = currentRow
      if (!this.ifShowTravel) { // 如果出差表格出现了并且所填天数大于实际的天数
        let actDays = this.calculateDays(businessTripDate, endDate)
        let days = this.formData.reimburseTravellingAllowances[0].days
        if (days > actDays) {
          this.$message.warning('所填天数超过出差天数')
          this.formData.reimburseTravellingAllowances[0].days = actDays
        }
      }
    },
    _checkAccMoney () { // 出差表格和住宿表格的提示信息
      if (!this.ifShowAcc) { // 如果存在住宿表格，则遍历表格中所有未被删除的项，对totalmoney进行判断
        let accTableList = this.formData.reimburseAccommodationSubsidies
        for (let i = 0; i < accTableList.length; i++) {
          let item = accTableList[i]
          let { money, isAdd } = item
          if (money && Number(money) > Number(this.accMaxMoney) && this.accMaxMoney && isAdd) {
            this.$message.warning(`所填金额大于住宿补贴标准(${this.accMaxMoney}元)`)
            break
          }
        }
      }
    },
    _getCostList () {
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
        return this.$message.warning('请选择费用模板')
      }
      let invoiceNumberList = selectList.map(item => {
        return item.invoiceNumber
      })
      this.costLoading = true
      isSole(invoiceNumberList).then(() => {
        this.selectedList.push(...selectList) // 将选择的数组push到selected中
        const cloneSelectList = deepClone(selectList) // 避免引用造成影响
        this._normalizeSelectList(cloneSelectList) // 因为这些导出的数据相当于新数据，所以需要将附件ID删除
        this._addToTable(cloneSelectList) // 根据报销类型的不同插入到不同的表中
        this.closeCostDialog()
        this.costLoading = false
      }).catch(() => {
        this.costLoading = false
        this.$message.error('发票号码验证失败')
      })
    },
    _normalizeSelectList (selectList) {
      this._buildAttachment(selectList, true)
    },
    _addToTable (selectList) {
      let trafficList = selectList.filter(item => item.reimburseType === Number(2))
      let accList = selectList.filter(item => item.reimburseType === Number(3))
      let otherList = selectList.filter(item => item.reimburseType === Number(4))
      if (trafficList.length) {
        this.ifShowTraffic = false
        this.formData.reimburseFares.push(...trafficList)
      }
      if (accList.length) {
        this.ifShowAcc = false
        this.formData.reimburseAccommodationSubsidies.push(...accList)
        this._checkAccMoney()
      }
      if (otherList.length) {
        this.ifShowOther = false
        this.formData.reimburseOtherCharges.push(...otherList)
      }
    },
    async openRemarkDialog (type) { // 打开备注弹窗，二次确认
      this.remarkType = type
      this.$refs.form.validate(isValid => {
        if (isValid) {
          if (type !== 'reject') {
          this.$confirm(`${type === 'pay' ? '支付' : '同意'}此次报销?`, '提示', {
              confirmButtonText: '确定',
              cancelButtonText: '取消',
              type: 'warning'
            }).then(() => {
              this.approve()
            })
          } else {
            this.$refs.approve.open() 
          }
        } else {
          this.$message.error('格式错误或必填项未填写')
        }
      })
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
      // let { createUserId, userName, orgName } = this.formData
      this.$refs.form.clearValidate()
      this.$refs.form.resetFields()
      this.clearFile()
      this.ifShowTraffic = this.ifShowOther = this.ifShowAcc = this.ifShowTravel = true
      this.selectedList = []
      this.prevAreaData = null
      this.formData = { // 表单参数
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
        reimurseOperationHistories: [],
        isDraft: false, // 是否是草稿
        delteReimburse: [], // 需要删除的行数据
        fileId: [], // 需要删除的附件ID
        myExpendsIds: [] // 需要删除的导入数据（我的费用ID）
      }    
    },
    addSerialNumber (data) { // 为表格的数据添加序号
      data.forEach((item, index) => {
        item.serialNumber = index + 1
      })
    },
    async checkData () { // 校验表单数据是否通过
      let isFormValid = true, isTravelValid = true, isTrafficValid = true, isAccValid = true, isOtherValid = true
      this.errMessage = '格式错误或必填项未填写'
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
        // console.log('checkData', isFormValid, isTravelValid, isTrafficValid, isAccValid, isOtherValid)
        return isFormValid && isTrafficValid && isAccValid && isOtherValid && isTravelValid
      } catch (err) {
        console.log(err)
      }      
    },
    async submit (isDraft) { // 提交
      let { 
        reimburseTravellingAllowances,
        reimburseAccommodationSubsidies, 
        reimburseOtherCharges, 
        reimburseFares,
        reimburseAttachments,
        attachmentsFileList,
        totalMoney
      } = this.formData
      if (parseFloat(totalMoney) <= 0) {
        return Promise.reject({ message: '总金额不能为零' })
      }
      this.formData.reimburseAttachments = [...reimburseAttachments, ...attachmentsFileList]
      this.mergeFileList(reimburseAccommodationSubsidies)
      this.mergeFileList(reimburseOtherCharges)
      this.mergeFileList(reimburseFares)
      this.addSerialNumber(reimburseTravellingAllowances)
      this.addSerialNumber(reimburseAccommodationSubsidies)
      this.addSerialNumber(reimburseOtherCharges)
      this.addSerialNumber(reimburseFares)
      this.formData.myExpendsIds = this.selectedList.map(item => item.id)
      let isValid = await this.checkData()
      if (!isValid) {
        return Promise.reject({ message: this.errMessage })
      }
      this.formData.isDraft = isDraft ? true : false
      return addOrder(this.formData)
    },
    async updateOrder (isDraft) { // 编辑
      let { 
        reimburseTravellingAllowances,
        reimburseAccommodationSubsidies, 
        reimburseOtherCharges, 
        reimburseFares,
        reimburseAttachments,
        attachmentsFileList,
        totalMoney
      } = this.formData
      if (parseFloat(totalMoney) <= 0) {
        return Promise.reject({ message: '总金额不能为零' })
      }
      this.formData.reimburseAttachments = [...reimburseAttachments, ...attachmentsFileList]
      this.mergeFileList(reimburseAccommodationSubsidies)
      this.mergeFileList(reimburseOtherCharges)
      this.mergeFileList(reimburseFares)
      this.addSerialNumber(reimburseTravellingAllowances)
      this.addSerialNumber(reimburseAccommodationSubsidies)
      this.addSerialNumber(reimburseOtherCharges)
      this.addSerialNumber(reimburseFares)
      this.formData.myExpendsIds = this.selectedList.map(item => item.id) // 导入的数据ID
      let isValid = await this.checkData()
      if (!isValid) {
        return Promise.reject({ message: this.errMessage })
      }
      this.formData.isDraft = isDraft ? true : false
      return updateOrder(this.formData)
    },
    approve () {
      this._approve()
    },
    async _approve () {
      this.$refs.form.validate(isValid => {
        if (!isValid) {
          return this.$message.error('格式错误或必填项未填写')
        } 
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
        this.remarkType === 'reject'
          ? this.remarkLoading = true
          : this.parentVm.openLoading()
        approve(params).then(() => {
          this.$message({
            type: 'success',
            message: this.remarkType === 'reject' 
              ? '驳回成功' 
              : (this.remarkType === 'agree' ? '审核成功' : '支付成功')
          })
          this.parentVm._getList()
          this.parentVm.closeDialog()
          if (this.remarkType === 'reject') {
            this.remarkLoading = false
            this.closeRemarkDialog()
          } else {
            this.parentVm.closeLoading()
          }
        }).catch(() => {
          this.remarkType === 'reject'
            ? this.remarkLoading = false
            : this.parentVm.closeLoading()
          this.$message.error('操作失败')
        })
      })       
    }
  }
}
</script>
<style lang='scss' scoped>
.order-wrapper {
  position: relative;
  // max-height: 700px;
  // overflow-y: auto;
  /* 头部 */
  .head-title-wrapper {
    position: absolute;
    top: -34px;
    left: 51px;
    p {
      min-width: 65px;
      margin-right: 10px;
      font-size: 12px;
      font-weight: bold;
      span {
        font-weight: normal;
      }
    }
  }
  .my-form-wrapper {
    .upload-title {
      box-sizing: border-box;
      width: 80px;
      height: 18px;
      line-height: 18px;
      font-size: 12px;
      text-align: right;
      white-space: nowrap;
      &.money {
        font-size: 18px;
        font-weight: bold;
      }
    }
    .money-text {
      font-size: 18px;
      font-weight: bold;
    }
  }
  .scroll-bar {
    &.el-scrollbar {
       ::v-deep {
        .el-scrollbar__wrap {
          max-height: 700px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
  }
  .uneditable {
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
  &::-webkit-scrollbar {
    display: none;
  }
  ::v-deep .el-form-item--mini.el-form-item, .el-form-item--small.el-form-item {
    margin-bottom: 5px;
  }
  ::v-deep .el-input__icon {
    &.success {
      color: rgba(0, 128, 0, 1);
    }
    &.warning {
      color: rgba(255, 165, 0, 1);
    }
  }
  .upload-wrapper {
    margin: 5px 0;
    .upload-title {
      box-sizing: border-box;
      width: 80px;
      height: 18px;
      line-height: 18px;
      padding-right: 12px;
      font-size: 12px;
      text-align: right;
      white-space: nowrap;
    }
  }
  .history-wrapper {
    ::v-deep .el-table {
      &::before {
        height: 0;
      }
    }
  }
  /* 各个表格的样式 */
  .form-item-wrapper {
    &.travel {
      width: 622px;
      &.uneditable {
        width: 471px;
      }
    }
    &.acc {
      width: 1077px;
      &.uneditable {
        width: 916px;
      }
    }
    &.other {
      width: 1014px;
      &.uneditable {
        width: 846px;
      }
    }
    margin-bottom: 5px;
    &::-webkit-scrollbar {
      display: none;
    }
    ::v-deep .el-table  {
      overflow: visible;
      &::before {
        height: 0;
      }
      .el-table__body-wrapper {
        overflow: visible;
      }
      .cell {
        overflow: visible;
      }
    }
    .money-class {
      ::v-deep input {
        text-align: right;
      }
    }
    .title-wrapper {
      display: flex;
      justify-content: center;
      align-items: center;
      position: relative;
      width: 100%;
      height: 30px;
      line-height: 30px;
      color: #606266;
      font-size: 16px;
      background-color: #f5f7fa;
      .number-count {
        position: absolute;
        bottom: 0;
        top: 0;
        left: 10px;
        height: 100%;
        margin: auto 0;
        font-size: 12px;
      }
      .title {
        position: relative;
        font-size: 12px;
        .total-money {
          position: absolute;
          overflow: visible;
          top: 0;
          right: -12px;
          // width: 400px;
          white-space: nowrap;
          transform: translate3d(100%, 0, 0);
          font-size: 16px;
          font-weight: bold;
        }
      }
    }
    .area-wrapper {
      position: relative;
      .selector-wrapper {
        position: absolute;
        top: 5px;
        left: 0;
        z-index: 100;
        transform: translate3d(0, -100%, 0);
      }
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
    &.rotate {
      transform: rotate(-90deg);
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