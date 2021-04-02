<template>
  <div class="order-wrapper" v-loading="orderLoading">
    <!-- 标题头 -->
    <el-row 
      type="flex" 
      class="head-title-wrapper" 
      :class="{ 'general': title === 'approve' }"
      v-if="title !== 'approve'"
    >
      <p style="color: red;" v-if="formData.mainId">报销单号: <span>{{ formData.mainId }}</span></p>
      <p :class="{ 'pointer underline': !isGeneralManager }" @click="_openServiceOrHistory(true)" v-if="title === 'approve'">
        服务ID: <span>{{ formData.serviceOrderSapId }}</span>
      </p>
      <p>报销人:<span>{{ formData.orgName }} {{ formData.userName }}</span></p>
      <!-- <p v-if="!isGeneralStatus">部门: <span>{{ formData.orgName }}</span></p> -->
      <p>劳务关系: <span>{{ formData.serviceRelations }}</span></p>
      <p>创建时间: <span>{{ formData.createTime && formData.createTime.split(' ')[0] }}</span></p>
    </el-row>
    <el-row v-else type="flex" class="head-title-wrapper general-manager">
      <div style="margin-right: 15px;">
        <p style="margin-bottom: 4px">报销ID: <span>{{ formData.mainId }}</span></p>
        <p>服务ID: <span>{{ formData.serviceOrderSapId }}</span></p>
      </div>
      <div style="text-decoration: underline;">{{ formData.serviceRelations }}-{{ formData.orgName }}-{{ formData.userName }}</div>
    </el-row>
    <!-- 时间进度轴，仅总经理可看 timelineList -->
    <template v-if="title === 'approve'">
      <div class="timeline-progress-wrapper isGeneral">
        <el-row class="content-wrapper" type="flex" align="middle" justify="space-between">
          <el-row type="flex" class="interval-content" v-if="isGeneralManager">
            <template v-for="item in timelineList" >
              <span class="interval-time success" :key="item.text">{{ item.intervalTime | s2HMS }}</span>
            </template>
          </el-row>
          <template v-for="(item, index) in timelineList">
            <div class="content-item" :key="item.text">
              <div class="line" v-show="index !== 0" :class="{ 'finished': item.isFinished, 'current': item.isCurrent }"></div>
              <el-tooltip  palcement="top-center" :disabled="!item.isFinished" :content="item.createTime">
                <!-- <div> -->
                  <!-- <i class="icon-status" :class="processStatusIcon(item)"></i> -->
                  <!-- <div class=""> -->
                    <svg-icon class="icon-status finished" iconClass="white-tick" v-if="item.isFinished"></svg-icon>
                    <div v-else class="icon-status" :class="{ 'not-current': !item.isCurrent }"></div>
                  <!-- </div> -->
                <!-- </div> -->
              </el-tooltip>
              <p class="text">{{ item.text }}</p>
              <div class="time">
                <div>{{ item.ymd }}</div>
                <div>{{ item.hms }}</div>
              </div>
            </div>
          </template>
        </el-row>
      </div>
    </template>
    <el-scrollbar class="scroll-bar" :wrapStyle="wrapStyle">
      <!-- 总经理审批查看 -->
      <div class="general-order-wrapper" v-if="title === 'approve'">
        <el-row type="flex" :gutter="20">
          <el-col :span="12">
            <h2 class="bold">基本信息</h2>
            <el-form
              style="margin-top: 5px;"
              :model="formData"
              ref="form"
              class="general-form-wrapper manager"
              :label-width="labelWidth"
              size="mini"
              :label-position="labelPosition"
              :show-message="false"
              >
                <el-row type="flex" class="item">
                  <div class="customer-id">
                    <span class="title">客户代码</span>
                    <span>{{ formData.terminalCustomerId }}</span>
                  </div>
                  <div>
                    <el-row type="flex" align="start">
                      <span class="title">客户名称</span>
                      <p class="content">{{ formData.terminalCustomer }}</p>
                    </el-row>
                  </div>
                  <!-- <div>
                    <el-row type="flex" align="start">
                      <span>客户地址</span>
                      <p class="content-long">{{ formData.completeAddress }}</p>
                    </el-row>
                  </div> -->
                </el-row>
                <el-row type="flex" class="item">
                  <span class="title">呼叫主题</span>
                  <div v-if="formData.themeList && formData.themeList.length">
                    <div class="theme-item" v-for="(item, index) in formData.themeList" :key="item.description">
                      <div class="order-icon">{{ index + 1 }}</div>
                      <span>{{ item.description }}</span>
                    </div>
                  </div>
                </el-row>
                <el-row type="flex" class="item">
                  <span class="title">地址</span>
                  <p>{{ formData.becity }}-{{ formData.destination }}</p>
                </el-row>
                <el-row type="flex" class="item">
                  <span class="title">备注</span>
                  <p>{{ formData.remark }}</p>
                </el-row>
              </el-form>
          </el-col>
          <el-col :span="12">
            <el-row type="flex" justify="space-between">
              <h2 class="bold">客户历史费用</h2>
              <span class="underline pointer" @click="openHistory">查看更多</span>
            </el-row>
            <common-table 
              style="margin-top: 5px;"
              class="history-table-wrapper"
              :data="historyCostData.slice(0, 3)"
              :columns="lowHistoryCostColumns"
              :stripe="false"
              max-height="300px"
              :loading="historyCostLoading"
              :header-cell-style="historyCell"
              :cell-style="historyCell"
            >
              <!-- 总金额 -->
              <template v-slot:totalMoney="{ row }">
                {{ row.totalMoney | toThousands }}
              </template>
              <!-- 交通费用 -->
              <template v-slot:faresMoney="{ row }">
                {{ row.faresMoney | toThousands }}
              </template>
              <!-- 住宿补贴 -->
              <template v-slot:acc="{ row }">
                {{ row.accommodationSubsidiesMoney | toThousands }}
              </template>
              <!-- 出差补贴 -->
              <template v-slot:travel="{ row }">
                {{ row.travellingAllowancesMoney | toThousands }}
              </template>
              <!-- 其它费用 -->
              <template v-slot:other="{ row }">
                {{ row.otherChargesMoney | toThousands }}
              </template>
            </common-table>
          </el-col>
        </el-row>
      </div>
      <el-form
        v-else
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
                    <img :src="rightImg" @click="_openServiceOrHistory(false)" class="pointer">
                  </div>
                </template>
                <template v-else>
                  <span :class="{ 'upload-title money': item.label === '总金额'}">{{ item.label }}{{ item.label === '总金额' ? ':' : '' }}</span>
                </template>
              </span>
                <!-- 呼叫主题 -->
              <template v-if="item.label === '呼叫主题'">
                <div class="form-theme-content" :class="{ 'uneditable': !ifFormEdit }">
                  <el-scrollbar wrapClass="scroll-wrap-class">
                    <div class="form-theme-list">
                      <transition-group name="list" tag="ul">
                        <li class="form-theme-item" v-for="themeItem in formData.themeList" :key="themeItem.id" >
                          <!-- <el-tooltip popper-class="form-theme-toolip" effect="dark" :content="themeItem.description" placement="top"> -->
                            <p class="text" v-infotooltip.ellipsis="themeItem.description">{{ themeItem.description }}</p>
                          <!-- </el-tooltip> -->
                        </li>
                      </transition-group>
                    </div>
                  </el-scrollbar>
                </div>
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
      <template v-if="title === 'approve'">
        <!-- 费用详情 -->
        <div class="expense-detail-wrapper">
          <el-row class="title-wrapper" type="flex" justify="space-between" align="middle" :class="{ 'gerneral': isGeneralManager }">
            <h2 style="font-weight: bold;">费用详情</h2>
            <!-- <div class="area">
              <div>开始时间: <span>{{ formData.businessTripDate }}</span></div>
              <div>结束时间: <span>{{ formData.endDate }}</span></div>
            </div> -->
             <div class="area">
              <div v-if="ifShowAddTravelBtn"><el-button type="primary" @click="openTravelExpense" size="mini">新增差补</el-button></div>
              <div>出发地点: <span>{{ formData.becity }}</span></div>
              <div>到达地点: <span>{{ formData.destination }}</span></div>
            </div>
          </el-row>
          <!-- 费用详情列表 -->
          <div class="general-table-wrapper">
            <el-form ref="expenseForm" :model="expenseFormData" :show-message="false" size="mini">
              <common-table
                class="table-container"
                :data="expenseFormData.expenseCategoryList"
                :columns="expenseCategoryColumns"
                max-height="400px"
                :header-cell-style="headerCellStyle"
                :cell-style="cellStyle"
                @row-click="onExpenseClick"
              >
                <!-- 费用详情 -->
                <template v-slot:expenseDetail="{ row }">
                  <div class="detail-content">
                    <div>
                      <span style="display: inline-block;margin-right: 5px;" v-if="row.sellerName">{{ row.sellerName }}</span>
                      <span style="display: inline-block;margin-right: 5px;">{{ row.expenseDetail }}</span>
                      <el-tooltip 
                        :content="row.remark">
                        <i class="remark el-icon-chat-dot-round" v-if="row.remark"></i>
                      </el-tooltip>
                      <template v-if="row.otherFileList && normalizeOtherFileList(row).length">
                        <div
                          style="display: inline-block; margin-left: 2px;"
                          v-for="(item, index) in normalizeOtherFileList(row)" 
                          :key="item.id"
                        >
                          <!-- <span class="pointer" @click="openFile(item)">附件{{ index + 1 }}</span> -->
                          <i class="el-icon-document pointer" @click="openFile(item)">{{ index + 1 }}</i>
                        </div>
                      </template>
                    </div>
                  </div>
                </template>
                <!-- 发票号码 -->
                <template v-slot:invoiceNumber="{ row }">
                  <el-row class="invoice-number-wrapper" type="flex" align="middle" v-if="row.invoiceNumber" justify="space-between">
                    <el-row type="flex" align="middle">
                      <img class="pointer" :src="rightImg" alt="" @click="openFile(row, true)">
                      <span style="margin-right: 5px;">{{ row.invoiceNumber }}</span>
                    </el-row>
                    <el-tooltip content="无发票附件" :disabled="row.isValidInvoice">
                      <i calss="invoice-icon" :class="[row.isValidInvoice ? 'el-icon-upload-success el-icon-circle-check success' : 'el-icon-warning-outline warning']"></i>
                    </el-tooltip>
                  </el-row>
                </template>
                <!-- 费用归属 -->
                <template v-slot:belongin="{ row, index }">
                  <div class="bold">
                    <template v-if="row.moneyType === 'totalMoney'">总金额</template>
                    <template v-else-if="row.moneyType === 'org'">部门</template>
                    <template v-else-if="row.moneyType === 'company'">公司</template>
                    <template v-else-if="!row.moneyType">
                      <el-form-item :prop="'expenseCategoryList.' + index + '.'+ 'expenseOrg'" :rules="expenseRules['expenseOrg']">
                        <!-- <div @click.stop> -->
                          <el-cascader
                            :class="{ 'disabled': ifOrgDisabled }"
                            :disabled="ifOrgDisabled"
                            v-model="row.expenseOrg"
                            :options="reimburseOrgsList"
                            :props="{ checkStrictly: true }"
                            @change="onOrgChange({ row, index })"
                            clearable>
                          </el-cascader>
                        <!-- </div> -->
                      </el-form-item>
                    </template>
                  </div>
                </template>
                <!-- 金额 -->
                <template v-slot:money="{ row }">
                  <div v-infotooltip:200.top.ellipsis>
                    <template v-if="!row.moneyType">{{ row.money | toThousands }}</template>
                    <template v-else-if="row.moneyType === 'org'">{{ orgMoney | toThousands }}</template>
                    <template v-else-if="row.moneyType === 'company'">{{ companyMoney | toThousands }}</template>
                    <template v-else-if="row.moneyType === 'totalMoney'">{{ totalMoney | toThousands }}</template>
                  </div>
                </template>
              </common-table>
            </el-form>
          </div>
          <!-- <el-row type="flex" justify="end" class="general-total-money">总金额：{{ totalMoney | toThousands }}</el-row> -->
          <!-- <el-button size="mini" @click="toggleAfterEva">售后评价</el-button> -->
        </div>

        <div class="daily-report-wrapper">
          <el-row class="title-wrapper" type="flex" justify="space-between" align="middle">
            <h2 style="font-weight: bold;">工作日报</h2>
          </el-row>
          <common-table 
            style="margin-top: 10px;"
            :data="reportTableData"
            :columns="dailyReportColumns"
            max-height="300px"
          >
            <template v-slot:troubleDescription="{ row }">
              <div v-infotooltip:200.top.ellipsis>
                {{ row.troubleDescription.join('/') }}
              </div>
            </template>
            <template v-slot:processDescription="{ row }">
              <div v-infotooltip:200.top.ellipsis>
                {{ row.processDescription.join('/') }}
              </div>
            </template>
          </common-table>
        </div>
        <div style="margin-top: 5px;">
          <el-row type="flex" align="middle">
            <h2 style="margin-right: 10px; font-weight: bold;">服务评价</h2>
            总分
            <el-rate
              v-model="totalStart"
              disabled
              text-color="#ff9900"
            ></el-rate>
          </el-row>
          <common-table 
            style="margin-top: 10px;"
            :data="afterEvaluationList"
            :columns="afterEvaluationColumns"
            max-height="300px"
            :loading="afterEvaLoading"
          >
            <template v-slot:responseSpeed="{ row }">
              <el-rate
                v-model="row.responseSpeed"
                disabled
                text-color="#ff9900"
              >
              </el-rate>
            </template>
            <template v-slot:schemeEffectiveness="{ row }">
              <el-rate
                v-model="row.schemeEffectiveness"
                disabled
                text-color="#ff9900"
              >
              </el-rate>
            </template>
            <template v-slot:serviceAttitude="{ row }">
              <el-rate
                v-model="row.serviceAttitude"
                disabled
                text-color="#ff9900"
              >
              </el-rate>
            </template>
            <template v-slot:productQuality="{ row }">
              <el-rate
                v-model="row.productQuality"
                disabled
                text-color="#ff9900"
              >
              </el-rate>
            </template>
            <template v-slot:servicePrice="{ row }">
              <el-rate
                v-model="row.servicePrice"
                disabled
                text-color="#ff9900"
              >
              </el-rate>
            </template>
          </common-table>
        </div>
        <div class="pay-time" v-if="formData.payTime">支付时间： <span>{{ formData.payTime }}</span></div>
      </template>
      
      <template v-else>
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
        </el-row>
        <div v-loading="dailyExpendLoading">
          <!-- 出差 -->
          <div class="form-item-wrapper travel" :class="{ 'uneditable global-unused': !this.ifFormEdit }" v-if="formData.reimburseTravellingAllowances.length">
            <!-- <el-button v-if="ifShowTravel" @click="showForm(formData.reimburseTravellingAllowances, 'ifShowTravel')">添加出差补贴</el-button> -->
            <el-form 
              ref="travelForm" 
              :model="formData" 
              size="mini" 
              :show-message="false"
              class="form-wrapper"
              :disabled="true"
              :class="{ 'uneditable': title !== 'create' }"
            >
              <div class="title-wrapper">
                <div class="number-count">总数量:{{ travelCount }}个</div>
                <div class="title">
                  <span>出差补贴</span>
                  <p class="total-money">小计: ￥{{ travelTotalMoney | toThousands }}</p>
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
                        <el-input 
                          v-model="scope.row[item.prop]" 
                          :disabled="item.disabled" 
                          :placeholder="item.placeholder" 
                          v-infotooltip:200.top-start>
                        </el-input>
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
                          :disabled="item.prop === 'money'"
                          @input="onTravelInput"
                          :class="{ 'money-class': item.prop === 'money' || item.prop === 'days' }"
                        ></el-input>
                      </el-form-item>
                    </template>
                    <!-- <template v-else-if="item.type === 'operation'">
                      <template v-for="iconItem in item.iconList">
                        <i 
                          :key="iconItem.icon"
                          :class="iconItem.icon" 
                          class="icon-item"
                          @click="iconItem.handleClick(scope, formData.reimburseTravellingAllowances, 'travel', iconItem.operationType)">
                        </i>
                      </template>
                    </template> -->
                  </template>
                </el-table-column>
              </el-table>
            </el-form>
          </div>
          <!-- 交通 -->
          <div class="form-item-wrapper" v-if="ifCOrE || formData.reimburseFares.length">
            <el-button v-if="ifShowTraffic && !formData.reimburseFares.length" @click="showForm(formData.reimburseFares, 'ifShowTraffic')">添加交通费用</el-button>
            <el-form 
              v-else
              ref="trafficForm" 
              :model="formData" 
              size="mini" 
              :show-message="false"
              class="form-wrapper"
              :disabled="!ifFormEdit"
              :class="{ 'uneditable global-unused': !this.ifFormEdit }"
            >
              <div class="title-wrapper">
                <div class="number-count">总数量:{{ trafficCount }}个</div>
                <div class="title">
                  <span>交通费用</span>
                  <p class="total-money">小计: ￥{{ trafficTotalMoney | toThousands }}</p>
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
                            v-model.trim="scope.row[item.prop]" 
                            :disabled="item.disabled || (item.prop === 'invoiceNumber' && scope.row.isValidInvoice)" 
                            :readonly="item.readonly || false"
                            :placeholder="item.placeholder">
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
                        </div>                   
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'area'">
                      <el-form-item
                        :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                        :rules="scope.row.isAdd ? (trafficRules[item.prop] || { required: false }) : { required: false }"
                      >
                        <my-new-area-selector
                          v-infotooltip.top.ellipsis
                          class="my-area-selector"
                          v-model="scope.row[item.prop]"
                          :processValue="areaProcessValue"
                          @change="onAreaChange" 
                          :options="{ prop: item.prop, index: scope.$index }">
                        </my-new-area-selector>
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
                        :isInline="isGeneralManager && isCustomerSupervisor"
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
                    <template v-else-if="item.type === 'date'">
                      <el-form-item 
                        :prop="'reimburseFares.' + scope.$index + '.'+ item.prop"
                        :rules="scope.row.isAdd ? (trafficRules[item.prop] || { required: false }) : { required: false }">
                        <el-date-picker
                          class="invoice-time"
                          size="mini"
                          v-model="scope.row[item.prop]"
                          type="datetime"
                          style="width: 100%;"
                          value-format="yyyy-MM-dd HH:mm:ss"
                          :clearable="false"
                          placeholder="选择日期时间">
                        </el-date-picker>
                      </el-form-item>
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
          <div class="form-item-wrapper acc" :class="{ 'uneditable global-unused': !this.ifFormEdit }" v-if="ifCOrE || formData.reimburseAccommodationSubsidies.length">
            <el-button v-if="ifShowAcc && !formData.reimburseAccommodationSubsidies.length" @click="showForm(formData.reimburseAccommodationSubsidies, 'ifShowAcc')">添加住宿补贴</el-button>
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
                  <p class="total-money">小计: ￥{{ accTotalMoney | toThousands }}</p>
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
                          v-model.trim="scope.row[item.prop]" 
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
                          :class="{ 'money-class': item.prop === 'money' || item.prop === 'totalMoney' || item.prop === 'days' }"
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
                    <template v-else-if="item.type === 'date'">
                      <el-form-item 
                        :prop="'reimburseAccommodationSubsidies.' + scope.$index + '.'+ item.prop"
                        :rules="scope.row.isAdd ? (accRules[item.prop] || { required: false }) : { required: false }">
                        <el-date-picker
                          class="invoice-time"
                          size="mini"
                          v-model="scope.row[item.prop]"
                          type="datetime"
                          style="width: 100%;"
                          value-format="yyyy-MM-dd HH:mm:ss"
                          :clearable="false"
                          placeholder="选择日期时间">
                        </el-date-picker>
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
                        :isInline="isGeneralManager || isCustomerSupervisor"
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
          <div class="form-item-wrapper other" :class="{ 'uneditable global-unused': !this.ifFormEdit }" v-if="ifCOrE || formData.reimburseOtherCharges.length">
            <el-button v-if="ifShowOther && !formData.reimburseOtherCharges.length" @click="showForm(formData.reimburseOtherCharges, 'ifShowOther')">添加其他费用</el-button>
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
                  <p class="total-money">小计: ￥{{ otherTotalMoney | toThousands }}</p>
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
                          v-model.trim="scope.row[item.prop]" 
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
                    <template v-else-if="item.type === 'date'">
                      <el-form-item 
                        :prop="'reimburseOtherCharges.' + scope.$index + '.'+ item.prop"
                        :rules="scope.row.isAdd ? (otherRules[item.prop] || { required: false }) : { required: false }">
                        <el-date-picker
                          class="invoice-time"
                          size="mini"
                          v-model="scope.row[item.prop]"
                          type="datetime"
                          style="width: 100%;"
                          value-format="yyyy-MM-dd HH:mm:ss"
                          :clearable="false"
                          placeholder="选择日期时间">
                        </el-date-picker>
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
                        :isInline="isGeneralManager && isCustomerSupervisor"
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
        </div>
      </template>
      <!-- 操作记录 -->
      <template v-if="!this.ifFormEdit && this.formData.reimurseOperationHistories.length">
        <!-- 总经理操作记录 -->
        <template v-if="title !== 'approve'">
          <div class="history-wrapper">
            <el-table 
              style="width: 989px;"
              :data="formData.reimurseOperationHistories"
              border
              max-height="200px"
            >
              <el-table-column label="操作记录" prop="action" width="150px" show-overflow-tooltip></el-table-column>
              <el-table-column label="操作人" prop="createUser" width="100px" show-overflow-tooltip></el-table-column>
              <el-table-column label="操作时间" prop="createTime" width="150px" show-overflow-tooltip></el-table-column>
              <el-table-column label="审批时长" prop="intervalTime" width="150px" show-overflow-tooltip>
                <template slot-scope="scope">
                  {{ scope.row.intervalTime | m2DHM }}
                </template>
              </el-table-column>
              <el-table-column label="审批结果" prop="approvalResult" width="80px" show-overflow-tooltip></el-table-column>
              <el-table-column label="备注" prop="remark" show-overflow-tooltip></el-table-column>
            </el-table>
          </div>
        </template>
      </template>
    </el-scrollbar>
    <!-- 客户选择列表 -->
    <my-dialog 
      title="服务单列表"
      ref="customerDialog" 
      width="621px" 
      :append-to-body="true"
      :btnList="customerBtnList"
      @closed="closeDialog">
      <common-table 
        ref="customerTable"
        max-height="500px"
        :data="customerInfoList"
        :columns="customerColumns"
        radioKey="id"
      >
        <template v-slot:fromTheme="{ row }">
           <!-- 呼叫主题显示 -->
           <p class="text" v-infotooltip.top-start.ellipsis="row.themeList">{{ row.themeList.join(' ') }}</p>
        </template>
      </common-table>
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
      :append-to-body="true"
      :btnList="costBtnList"
      :loading="costLoading"
      @closed="closeCostDialog">
      <div style="height: 400px;">
        <common-table 
          ref="costTable"
          max-height="400px"
          row-key="id"
          :data="costData"
          :columns="costColumns"
          :selectedList="selectedList"
          selectedKey="id"
        ></common-table>
      </div>
      <pagination
        v-show="costTotal > 0"
        :total="costTotal"
        :page.sync="listQueryCost.page"
        :limit.sync="listQueryCost.limit"
        @pagination="costCurrentChange"
      />
    </my-dialog>
    <!-- 客服新增差补 -->
    <my-dialog
      v-loading.fullscreen="travelLoading"
      title="新增出差补贴"
      top="300px"
      ref="travelExpenseDialog"
      width="400px"
      :btnList="travelBtnList"
      :append-to-body="true"
      @closed="travelClosed"
    >
      <div>请输入增补的天数</div>
      <el-input-number size="mini" v-model="travelDays" :min="1" :precision="0" :controls="false"></el-input-number>
    </my-dialog>
    <!-- 完工报告 -->
    <my-dialog
      width="983px"
      title="服务行为报告单"
      ref="reportDialog"
      :append-to-body="true"
      @closed="resetReport">
      <Report :data="reportData" ref="report"/>
    </my-dialog>
    <!-- 确认审批弹窗 -->
    <my-dialog
      ref="approve"
      :center="true"
      :title="remarkTitle"
      top="200px"
      :append-to-body="true"
      :btnList="remarkBtnList"
      @closed="onApproveClose"
      v-loading="remarkLoading"
      width="350px">
      <remark ref="remark" @input="onRemarkInput" :tagList="reimburseTagList" :title="title"></remark>
    </my-dialog>
    <!-- 只能查看的表单 -->
    <my-dialog
      ref="serviceDetail"
      width="1210px"
      title="服务单详情"
      :append-to-body="true"
    >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
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
    <!-- 历史费用弹窗 -->
    <my-dialog
      ref="historyDialog"
      width="1025px"
      :title="historyTitle"
      top="200px"
      :append-to-body="true"
    >
      <!-- 历史费用 -->
      <div>
        <common-table 
          class="history-table-wrapper"
          :data="historyCostData"
          :columns="historyCostColumns"
          :stripe="false"
          max-height="300px"
          :loading="historyCostLoading"
          :header-cell-style="historyCell"
          :cell-style="historyCell"
        >
          <!-- 总金额 -->
          <template v-slot:totalMoney="{ row }">
            {{ row.totalMoney | toThousands }}
          </template>
          <!-- 交通费用 -->
          <template v-slot:faresMoney="{ row }">
            {{ row.faresMoney | toThousands }}
          </template>
          <!-- 住宿补贴 -->
          <template v-slot:acc="{ row }">
            {{ row.accommodationSubsidiesMoney | toThousands }}
          </template>
          <!-- 出差补贴 -->
          <template v-slot:travel="{ row }">
            {{ row.travellingAllowancesMoney | toThousands }}
          </template>
          <!-- 其它费用 -->
          <template v-slot:other="{ row }">
            {{ row.otherChargesMoney | toThousands }}
          </template>
        </common-table>
      </div>    
    </my-dialog>
    <file-viewer
      v-if="previewVisible"
      :zIndex="99999"
      :file-list="previewImageUrl"
      :on-close="closeViewer"
    >
    </file-viewer>
    <!-- 预览图片 -->
    <!-- <el-image-viewer
      v-if="previewVisible"
      :zIndex="99999"
      :url-list="[previewImageUrl]"
      :on-close="closeViewer"
    > -->
    <!-- </el-image-viewer> -->
    <!-- 百度地图实例化 -->
    <template v-if="title === 'approve'">
      <el-row type="flex" justify="end" class="date-map-container">
        <div id="date-picker-wrapper" >
          <date-picker v-model="timeList" :markedDateList="reportDailyList || []" @click="onDatePicker" @expand-click="onExpandClick"></date-picker>
        </div>
        <div id="map-container" style="width:500px;height:550px;"></div>
        <!-- 控件提示信息 -->
        <div class="control-info" v-show="isShowControl" :style="controlInfoStyle">{{ controlInfo }}</div>
      </el-row>
    </template>
    <!-- <PDF :pdfURL="pdfURL" :on-close="closePDF" v-if="pdfVisible" /> -->
  </div>
</template>

<script>
import { 
  addOrder,
  getOrder, 
  updateOrder, 
  approve, 
  isSole, 
  getHistoryReimburseInfo, 
  getUserDetail,
  deleteCost,
  getReimburseOrgs,
  addTravellingAllowance,
  getServiceDailyExpendSum
} from '@/api/reimburse'
import markerIcon from '@/assets/bmap/marker.png'
import { on, off } from '@/utils/dom'
import { getList as getAfterEvaluaton } from '@/api/serve/afterevaluation'
import { getList } from '@/api/reimburse/mycost'
import { getDailyReport } from '@/api/serve/callservesure'
import upLoadFile from "@/components/upLoadFile";
// import PDF from './pdf'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
import Report from './report'
import Remark from './remark'
// import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
// import Bmap from '@/components/bmap'
import { toThousands } from '@/utils/format'
import { findIndex, accAdd } from '@/utils/process'
import { deepClone } from '@/utils'
import { formatDate, collections } from '@/utils/date'
import { travelRules, trafficRules, accRules, otherRules } from '../js/customerRules'
import { customerColumns, costColumns } from '../js/config'
import { noop } from '@/utils/declaration'
import { categoryMixin, reportMixin, attachmentMixin, chatMixin } from '../js/mixins'
import { REIMBURSE_TYPE_MAP, IF_SHOW_MAP, REMARK_TEXT_MAP } from '../js/map'
// import { m2DHM } from '@/filters/time'
import { s2HMS } from '@/filter/time'
import rightImg from '@/assets/table/right.png'
import DatePicker from './DatePicker.vue'
const PROGRESS_TEXT_LIST = ['提交', '客服审批', '财务初审', '财务复审', '总经理审批', '出纳'] // 进度条文本
const AFTER_EVALUTION_KEY = ['responseSpeed', 'schemeEffectiveness', 'serviceAttitude', 'productQuality', 'servicePrice']
const TEXT_REG = /[\r|\r\n|\n\t\v]/g
const NOW_DATE = new Date()

// const AFTER_EVALUTION_STATUS = {
//   0: '未统计',
//   1: '非常差',
//   2: '差',
//   3: '一般',
//   4: '满意',
//   5: '非常满意'
// }
export default {
  inject: ['parentVm'],
  mixins: [categoryMixin, reportMixin, attachmentMixin, chatMixin],
  components: {
    upLoadFile,
    Report,
    Remark,
    zxform,
    zxchat,
    // ElImageViewer,
    DatePicker,
    // Bmap
    // PDF
  },
  filters: {
    s2HMS
  },
  props: {
    title: {
      type: String,
      default: ''
    },
    BMap: {
      type: Object
    },
    map: {
      type: Object
    },
    isProcessed: Boolean, // 是否已经处理
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
      travelDays: 1, // 增补天数
      travelLoading: false,
      newTravelInfo: {}, // 当前客服审批阶段，新增的出差补贴信息
      travelBtnList: [
        { btnText: '确认', handleClick: this.addTravelExpense, type: 'primary' }
      ],
      historyTitle: '历史费用',
      pdfURL: '',
      pdfVisible: false,
      // 费用详情
      expenseCategoryColumns: [
        { label: '#', type: 'index' },
        { label: '日期', prop: 'invoiceTime' },
        { label: '费用名称', prop: 'expenseName' },
        { label: '费用详情', slotName: 'expenseDetail', 'show-overflow-tooltip': false },
        { label: '发票号码', slotName: 'invoiceNumber' },
        { label: '费用归属', slotName: 'belongin' },
        { label: '金额（元）', slotName: 'money', align: 'right' },
      ],
      // 售后评价
      totalStart: 0,
      afterEvaluationList: [],
      afterEvaluationColumns: [
        // { label: '技术员', prop: 'technician', width: 50 },
        { label: '响应速度', prop: 'responseSpeed',  slotName: 'responseSpeed' },
        { label: '方案有效性', prop: 'schemeEffectiveness', slotName: 'schemeEffectiveness' },
        { label: '服务态度', prop: 'serviceAttitude',  slotName: 'serviceAttitude' },
        { label: '产品质量', prop: 'productQuality',  slotName: 'productQuality' },
        { label: '服务价格', prop: 'servicePrice',  slotName: 'servicePrice' },
        { label: '回访人', prop: 'visitPeople' },
        { label: '评价日期', prop: 'commentDate', width: 137 }
      ],
      afterEvaLoading: false,
      // 服务报告
      reportTableData: [],
      reportTableColumns: [
        { label: '制造商序列号', prop: 'manufacturerSerialNumber', width: 120 },
        { label: '物料编码', prop: 'materialCode', width: 120 },
        // { label: '问题类型', prop: 'troubleDescription', width: 180 },
        { label: '解决方案', prop: 'processDescription', width: 180 },
        { label: "备注", prop: 'remark', width: 609 }
      ],
      dailyReportColumns: [
        { type: 'index', label: '#' },
        { label: '日期', prop: 'date', width: 110 },
        { label: '物料编码', prop: 'materialCode', width: 120 },
        { label: '设备序列号', prop: 'manufacturerSerialNumber', width: 120 },
        // { label: '问题类型', prop: 'troubleDescription', width: 180 },
        { label: '问题描述', slotName: 'troubleDescription', 'show-overflow-tooltip': false },
        { label: '解决方案', slotName: 'processDescription', 'show-overflow-tooltip': false },
        // { label: "备注", prop: 'remark', width: 609 }
      ],
      reportDetailLoading: false,
      // 历史费用
      historyCostData: [],
      historyCostColumns: [
        { label: '报销单号', prop: 'mainId', width: '60px' },
        { label: '总天数', prop: 'days', align: 'right', width: '50px' },
        { label: '总金额', prop: 'totalMoney', slotName: 'totalMoney', align: 'right', width: '63px' },
        { label: '交通费用', prop: 'faresMoney', slotName: 'faresMoney', align: 'right', width: '63px' },
        { label: '交通占比', prop: 'fmProportion', width: '63px', align: 'right' },
        { label: '住宿补贴', prop: 'accommodationSubsidiesMoney', slotName: 'acc', align: 'right', width: '63px' },
        { label: '住宿占比', prop: 'asProportion', width: '63px', align: 'right' },
        { label: '出差补贴', prop: 'travellingAllowancesMoney', slotName: 'travel', align: 'right', width: '63px' },
        { label: '出差占比', prop: 'taProportion', width: '63px', align: 'right' },
        { label: '其他费用', prop: 'otherChargesMoney', slotName: 'other', align: 'right', width: '63px' },
        { label: '其他占比', prop: 'ocProportion', width: '63px', align: 'right' },
        { label: '出发时间', prop: 'businessTripDate', width: '126px' },
        { label: '结束时间', prop: 'endDate', width: '126px' },
        { label: '报销人', prop: 'userName', width: '75px' }
      ],
      lowHistoryCostColumns: [
        { label: '报销单号', prop: 'mainId', width: '60px' },
        { label: '总天数', prop: 'days', align: 'right' },
        { label: '总金额', prop: 'totalMoney', slotName: 'totalMoney', align: 'right' },
        { label: '交通费用', prop: 'faresMoney', slotName: 'faresMoney', align: 'right' },
        { label: '住宿补贴', prop: 'accommodationSubsidiesMoney', slotName: 'acc', align: 'right' },
        { label: '出差补贴', prop: 'travellingAllowancesMoney', slotName: 'travel', align: 'right' },
        { label: '其他费用', prop: 'otherChargesMoney', slotName: 'other', align: 'right' },
      ],
      historyCostLoading: false,
      generalStyle: { // 总经理头部style
        fontSize: 'bold'
      },
      previewVisible: '',
      previewImageUrl: '', // 预览图片
      timelineList: [], // 总经理时间进度条
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
      reportDailyList: [], // 
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
        themeList: [],
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
      // customerLoading: false, // 用户选择按钮的loading
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
      prevAreaData: null, // 上一次点击地址框的是时候，交通表格的行数据
      currentTime: new Date(), // 完工报告首日期
      nextTime: new Date(),
      timeList: [NOW_DATE, (collections.addMonth(+NOW_DATE, 1)).toDate()],
      expenseCategoryList: [], // 发票详情列表
      controlInfo: '', // 自定义控件的提示文案
      controlInfoStyle: '', // 控制自定义控件的位置
      isShowControl: false, // 控制自定义控件的显示和隐藏
      reimburseOrgsList: [], // 费用归属列表
      expenseRules: {
        expenseOrg: [ { required: true, trigger: ['blur', 'change'] } ],
      },
      cancelRequestDailyExpend: null, // 用来取消获取日费的请求方法
      dailyExpendLoading: false
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
          this._getSubsidies() // 获取出差补贴金额    
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
        console.log(this.formData, 'detailData')
        // if (this.isGeneralStatus) { // 总经理审批和查看的时候才执行
          
        // }
        if (this.title === 'approve') { // 审批的时候要告诉审批人 住宿金额补贴是否符合标准
          if (this.formData.businessTripDate) {
            this.currentTime = new Date(formatDate(this.formData.businessTripDate))
            this.nextTime = new Date(collections.addMonth(this.formData.businessTripDate, 1))
            // this.nextTime = new Date(+this.currentTime + 24 * 60 * 60 * 1000)
            this.timeList = [this.currentTime, this.nextTime]
          } else {
            const date = new Date()
            this.timeList = [date, (collections.addMonth(+date, 1)).toDate()]
          }
          this._checkAccMoney()
          this._getAfterEvaluation() // 获取售后评价
          this._getReportDetail() // 获取服务报告
          this._getHistoryCost() // 获取历史费用
          this._getReimburseOrgs() // 查询费用归属列表
          this.expenseCategoryList = this.formData.expenseCategoryList
          // this.addSerialNumber(this.expenseCategoryList)
          
          this.timelineList = this._normalizeTimelineList(this.formData.reimurseOperationHistories)
        }
        if (this.isCustomerSupervisor) {
          this._getSubsidies() // 获取出差补贴金额 
        }
        if (this.title === 'create' || this.title === 'edit') { // 只有在create或者edit的时候，才可以导入费用模板
          this._getCostList() // 获取费用模板
          this._getSubsidies() // 获取出差补贴金额 
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
    ifShowAddTravelBtn () {
      return this.isCustomerSupervisor && this.$route.path === '/reimbursement/toProcess'
    },
    expenseFormData () { // 审批阶段费用详情form表单数据
      return {
        expenseCategoryList: this.expenseCategoryList || []
      }
    },
    ifOrgDisabled () { // 客户审批阶段
      return !(this.isCustomerSupervisor && this.$route.path === '/reimbursement/toProcess')
    },
    wrapStyle () {
      return this.title === 'approve' ? [{ height: '700px' }] : [{ maxHeight: '700px' }]
    },
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
        return this.getTotal(reimburseTravellingAllowances, true)
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
    companyMoney () { // 公司费用
      return (this.formData.expenseCategoryList || []).reduce((prev, next) => {
        const { expenseOrg, money } = next
        // 如果是公司的话，归纳到公司费用里面
        return accAdd(prev, (expenseOrg && expenseOrg.length && expenseOrg.indexOf('公司') > -1) ? money : 0)
      }, 0)
    },
    orgMoney () { // 部门费用
      return (this.formData.expenseCategoryList || []).reduce((prev, next) => {
        const { expenseOrg, money } = next
        // 如果是部门的话，归纳到部门费用里面
         return accAdd(prev, (expenseOrg && expenseOrg.length && expenseOrg.indexOf('公司') === -1) ? money: 0)
      }, 0)
    },
    totalMoney () {
      let moneyList = [this.travelTotalMoney, this.trafficTotalMoney, this.accTotalMoney, this.otherTotalMoney, (this.newTravelInfo.money || 0)]
      return moneyList.reduce((prev, next) => {
        return accAdd(prev, next)
      }, 0)
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
        // reimburseType: [ { required: true, trigger: ['change', 'blur'] } ],
        // shortCustomerName: [{ required: true, trigger: 'blur' }],
        // projectName: [ { required: true, trigger: ['change', 'blur'] } ],
        // bearToPay: [ { required: this.isCustomerSupervisor, trigger: ['change', 'blur']} ],
        // responsibility: [ { required: true, trigger: ['change', 'blur'] } ],
        // serviceRelations: [ { required: true, trigger: ['change', 'blur'] } ]
      }
    },
    remarkTitle () {
      return `确认${REMARK_TEXT_MAP[this.remarkType]}此次报销`
    },
    customerBtnList () {
      return [
        { btnText: '确认', handleClick: this.confirm },
        { btnText: '取消', handleClick: this.closeDialog }
      ]
    },
    // travelMoney () {
    //   // 以R或者M开头都是65
    //   return  this.isROrM ? '65' : '50'
    // },
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
  mounted () {
    console.log('order mounted')
  },
  destroyed () {
    off(this.multiple, 'mouseenter', this.multipleEnter)
    off(this.single, 'mouseenter', this.singleEnter)
    off(this.multiple, 'mouseleave', this.hideControlInfo)
    off(this.single, 'mouseleave', this.hideControlInfo)
    off(this.single, 'click', this.singleClick)
    off(this.multiple, 'click', this.multipleClick)
  },
  methods: {
    initMap () { // 初始化地图
      let BMap = this._BMap = global.BMap
      let map = this._map = new BMap.Map("map-container", { enableMapClick: false })  //新建地图实例，enableMapClick:false ：禁用地图默认点击弹框
      let point = new BMap.Point(113.30765, 23.12005);
      map.addControl(new BMap.NavigationControl({
          anchor: window.BMAP_ANCHOR_BOTTOM_RIGHT,
          type: window.BMAP_NAVIGATION_CONTROL_ZOOM,
          offset: new BMap.Size(10, 20)
      }))
      map.addControl(this.createPathControl())
      map.centerAndZoom(point, 18)
      map.enableScrollWheelZoom() // 滚轮缩放
      map.clearOverlays();                        //清除地图上所有的覆盖物  
      // 生成坐标点
      this.createPointArr(this.formData.pointArr)
      this.ifCreateDrivePath = false // 标识是否绘制全路径
      this.ifCreatePath = false // 标识是否绘制最后一条路径
      this.ifDateClick = false // 标识通过点击日期进行绘制
      this.isSearchCompelete = false // 标识绘制全路径是否检索完成
      this.createDrivePath()
      console.log('initMap', this.formData.pointArr)
    },
    createLastPath () { // 绘制最后一条线
      if (this.ifCreatePath) {
        this.ifCreatePath = !this.ifCreatePath
        return this.removeOverlay(true)
      }
      this.removeOverlay(true)
      this.ifCreatePath = !this.ifCreatePath
      let icon = new this._BMap.Icon(
        markerIcon,
        new this._BMap.Size(25, 25)
      )
      icon.setImageSize(new this._BMap.Size(25, 25))
      for (let i = 0; i < this.trackPoint.length; i++) {   
        // 最后一个点 闪烁
        let id = this.formData.pointArr[i].id
        if (id) {
          let index = findIndex(this.formData.expenseCategoryList, item => item.id === id)
          let marker = new this._BMap.Marker(this.trackPoint[i], {
            icon,
            offset: new this._BMap.Size(0, -5)
          })
          marker.setZIndex(1000)
          marker.isLast = true
          this._map.addOverlay(marker) 
          let label = this.createMarkerLabel(index + 1, this.trackPoint[i])
          label.setZIndex(10001)
          label.isLast = true
          this._map.addOverlay(label)
        }
        this._map.getViewport(this.trackPoint)
      }
      // let icons = this.createArrow()
      let polyline = new this._BMap.Polyline(this.trackPoint.slice(-2), {
        strokeColor: '#419fff',
        strokeWeight:2 ,//宽度
        strokeOpacity:0.8,//折线的透明度，取值范围0 - 1
        enableEditing: false,//是否启用线编辑，默认为false
        enableClicking: false,//是否响应点击事件，默认为true
        // icons: [icons]
      })
      polyline.isLast = true
      this._map.addOverlay(polyline)
      this._map.setViewport(this.trackPoint)
      
    },
    createDrivePath (isDateClick) { // 创建trackPoint数组的所有路径
      if (!isDateClick && this.ifCreateDrivePath) {
        // 如果已经绘制过了，再次调用就移除
        this.ifCreateDrivePath = !this.ifCreateDrivePath
        return this.removeOverlay()
      }
      if (!this.trackPoint || !this.trackPoint.length) { // 数组坐标为空
        return this._map.clearOverlays()
      }
      if (isDateClick) { // 点击日期创建途径
        this.ifDateClick = true
        this._map.clearOverlays()
      } else {
        this.removeOverlay()
      }
      this.ifCreateDrivePath = isDateClick ? false : !this.ifCreateDrivePath
      let driving = this.driving = new this._BMap.DrivingRoute(this._map);    //创建驾车实例  
      for (let i = 0; i < this.trackPoint.length; i++) {
        if(i !== this.trackPoint.length - 1){
          driving.search(this.trackPoint[i], this.trackPoint[i+1])
        }
        if (i === this.trackPoint.length - 1) {
          let marker =  this.createFlash(this.trackPoint[i], 10, 10)
          marker.setZIndex(999)
          this._map.addOverlay(marker)
        }
      }
      this.isSearchCompelete = false
      let successCount = 0
      driving.setSearchCompleteCallback(() => {  
        try {
          let pts = driving.getResults().getPlan(0).getRoute(0).getPath()   //通过驾车实例，获得一系列点的数组 
          let polyline = new this._BMap.Polyline(pts, {
            strokeColor: '#1bac2e',
            strokeWeight: 8 ,//宽度
            strokeOpacity:0.8,//折线的透明度，取值范围0 - 1
            enableEditing: false, // 是否启用线编辑，默认为false
            enableClicking: false, // 是否响应点击事件，默认为true,
            // icons: [icons]
          })
          this._map.addOverlay(polyline)
          this._map.setViewport(this.trackPoint)
          successCount++
          if (successCount === this.trackPoint.length - 1) { // 全部都检索完成了
            this.isSearchCompelete = true
          }
        } catch (err) {
          console.log(err)
        }
      })
    },
    createArrow () {
      let sy = new this._BMap.Symbol(window.BMap_Symbol_SHAPE_BACKWARD_OPEN_ARROW, {
        scale: 0.6,//图标缩放大小
        strokeColor:'#fff',//设置矢量图标的线填充颜色
        strokeWeight: 2,//设置线宽
      })
      let icons = new this._BMap.IconSequence(sy, '100%', '10%', false)//设置为true，可以对轨迹进行编辑
      return icons
    },
    removeOverlay (isLast) {
      let allOverlayList = this._map.getOverlays()
      allOverlayList = allOverlayList.filter(overlay => overlay.isLast === isLast || overlay.isCurrent)
      allOverlayList.forEach(overlay => {
        this._map.removeOverlay(overlay)
      })
    },
    createMarkerLabel (text, position, offset) {
      let defaultOffset = new this._BMap.Size(text >= 10 ? -8 : -5, -14)
      offset = offset || defaultOffset
      let label = new this._BMap.Label(text, { position, offset })
      label.setStyle({
        color: "#fff",
        border: "none",
        fontSize: "12px",
        background: "rgba(0,0,0,0)"
      })
      return label
    },
    createFlash (center, width, height) { // 创建一个闪烁点
      let that = this
      function FlashOverlay(center, width, height) {
				this._center = center
        this._width = width
        this._height = height
			}
			// 继承API的BMap.Overlay
			FlashOverlay.prototype = new that._BMap.Overlay()
			// 实现初始化方法  
			FlashOverlay.prototype.initialize = function () {
				// 创建div元素，作为自定义覆盖物的容器
        var div = document.createElement("div")
				div.style.position = "absolute"
				// 可以根据参数设置元素外观
        // div.classList.add('ao')
        div.classList.add('bmap-wave')
				div.style.width = this._width + "px"
				div.style.height = this._height + "px"
        
				// 将div添加到覆盖物容器中
				that._map.getPanes().markerPane.appendChild(div)
				// 保存div实例
				this._ele = div;
				// 需要将div元素作为方法的返回值，当调用该覆盖物的show、
				// hide方法，或者对覆盖物进行移除时，API都将操作此元素。
				return div
			}
			FlashOverlay.prototype.draw = function () {
				// 根据地理坐标转换为像素坐标，并设置给容器    
				var position = that._map.pointToOverlayPixel(this._center)
				console.log(this._center, position)
				this._ele.style.left = position.x - this._width / 2 + "px"
				this._ele.style.top = position.y - this._height / 2 + "px"
      }
      FlashOverlay.prototype.setZIndex = function (zIndex) {
        if (this._ele) {
          this._ele.style.zIndex = zIndex
        }
      }
      FlashOverlay.prototype.show = function () {
				if (this._ele) {
					this._ele.style.display = "";
				}
			}
			// 实现隐藏方法  
			FlashOverlay.prototype.hide = function () {
				if (this._ele) {
					this._ele.style.display = "none";
				}
			}
      return new FlashOverlay(center, width, height)
    },
    showControlInfo (text, e) {
      if (this.isShowControl) {
        return
      }
      this.isShowControl = true
      const { left, top } = e.target.getBoundingClientRect()
      this.controlInfo = text
      this.controlInfoStyle = { left: -60 + left + 'px', top: top + 'px' }
      console.log(text, 'text', left, top, e, this.controlInfoStyle)
    },
    hideControlInfo () {
      this.isShowControl = false
    },
    showPath (isAll) {
      console.log(isAll ? 'multiple click' : 'SINGLE')
      this.createPointArr(this.formData.pointArr)
      this.expenseCategoryList = this.formData.expenseCategoryList
      this.reportTableData = this.originReportTableData
      isAll ? this.isSearchCompelete && this.createDrivePath() : this.createLastPath()
    },
    createPathControl () { // 创建路径控件
      let that = this
      function PathControl () {
				// 设置默认停靠位置和偏移量  
				this.defaultAnchor = window.BMAP_ANCHOR_BOTTOM_RIGHT
				this.defaultOffset = new that._BMap.Size(10, 70)
			}
			// 通过JavaScript的prototype属性继承于BMap.Control   
			PathControl.prototype = new that._BMap.Control()

			// 自定义控件必须实现initialize方法，并且将控件的DOM元素返回   
			// 在本方法中创建个div元素作为控件的容器，并将其添加到地图容器中   
			PathControl.prototype.initialize = function () {
				// 创建一个DOM元素   
				let container = document.createElement('div') // 容器
				let multiple = document.createElement('div') // 展示所有路径
				let single = document.createElement('div') // 展示某一段路径
        let line = document.createElement('div') // 分割线
        // 地图样式定义在/styles/bmap.scss下
				multiple.className = 'bmap-multiple'
        single.className = 'bmap-single'
        // 分割线样式
        line.style.height = '1px'
        line.style.margin = '4px 0'
        line.style.backgroundColor = 'gray'
        // 容器样式
				container.style.padding = '5px'
				container.style.boxShadow = '0px 0px 1px 1px rgba(0, 0, 0, .3)'
        container.style.borderRadius = '4px'
        container.style.backgroundColor = '#fff'
        // 容器添加子元素
				container.appendChild(multiple)
				container.appendChild(line)
        container.appendChild(single)
        that.multiple = multiple
        that.single = single
        that.multipleEnter = e => that.showControlInfo('运动轨迹', e) 
        that.singleEnter = e => that.showControlInfo('发票轨迹', e)
        on(multiple, 'mouseenter', e => that.multipleEnter(e))
        on(single, 'mouseenter', e => that.singleEnter(e))
        on(multiple, 'mouseleave', that.hideControlInfo)
        on(single, 'mouseleave', that.hideControlInfo)
        that.multipleClick = () => that.showPath(true)
        that.singleClick = () => that.showPath()
        on(multiple, 'click', that.multipleClick)
        on(single, 'click', that.singleClick)
				// 添加DOM元素到地图中   
				that._map.getContainer().appendChild(container)
				// 将DOM元素返回  
				return container
			}
			// 创建控件实例    
			return new PathControl();
    },
    createCurrentPath () { // 绘制当前发票的路径
      this.ifCreateDrivePath = false
      this.ifCreatePath = false
      this.ifDateClick = false
      this._map.clearOverlays() // 清空画布上所有的覆盖物
      // let icons = this.createArrow()
      let polyline = new this._BMap.Polyline(this.trackPoint, {
        strokeWeight: 4 ,//宽度
        strokeOpacity:0.8,//折线的透明度，取值范围0 - 1
        enableEditing: false,//是否启用线编辑，默认为false
        enableClicking: false,//是否响应点击事件，默认为true
        // icons: [icons]
      })
      this._map.addOverlay(polyline)
      let icon = new this._BMap.Icon(
        markerIcon,
        new this._BMap.Size(25, 25)
      )
      icon.setImageSize(new this._BMap.Size(25, 25))
      let marker = new this._BMap.Marker(this.trackPoint[1], {
        icon,
        offset: new this._BMap.Size(0, -5)
      })
      // marker.setTop(true)
      let label = this.createMarkerLabel(this.bmapIndex, this.trackPoint[1], new this._BMap.Size(this.bmapIndex >= 10 ? -8 : -6, -14))
      marker.isCurrent = true
      label.isCurrent = true
      this._map.addOverlay(marker)
      this._map.addOverlay(label)
      this._map.setViewport(this.trackPoint)
    },
    createPointArr (pointArr) { // 创建更新绘图坐标点
      this.trackPoint = [];
      for (let i = 0, j = pointArr.length; i < j; i++) {
        this.trackPoint.push(new this._BMap.Point(pointArr[i].lng, pointArr[i].lat));
      }
    },
    onExpandClick () {
      this.expenseCategoryList = this.formData.expenseCategoryList
      this.reportTableData = this.originReportTableData
    },
    onDatePicker (date) { // 点击日历，右侧详情筛选对应的日期，地图也将包含的交通发票进行地图绘制
      let dateTime = formatDate(date)
      this.expenseCategoryList = this.formData.expenseCategoryList.filter(item => {
        // 存在发票时间 并且日期跟选择的日期相同
        return item.invoiceTime && formatDate(item.invoiceTime) === dateTime
      })
      let trafficList = this.expenseCategoryList.filter(item => item.isTraffic) // 找到交通发票列表
      // 日报
      this.reportTableData = this.originReportTableData.filter(item => {
        return item.dailyDate && formatDate(item.dailyDate) === dateTime
      })
      console.log(this.reportTableData, 'reportTable')
      let pointList = []
      trafficList.forEach(item => {
        let { fromLng, fromLat, toLng, toLat } = item
        if (fromLng && toLng) {
          pointList.push({ lng: fromLng, lat: fromLat })
          pointList.push({ lng: toLng, lat: toLat })
        }
      })
      this.createPointArr(pointList)
      if (this.isSearchCompelete) {
        this.createDrivePath(true)
      }
      console.log(date, 'date ondatepicker', dateTime, this.expenseCategoryList)
    },
    onExpenseClick (row) { // 点击交通发票绘制当前发票的路径
      let { fromLng, fromLat, toLng, toLat, id } = row
      if (!fromLng || !toLng) {
        return this.$message.warning('当前发票，未获取到坐标点')
      }
      // 找到当前交通发票的索引号,用于地图展示
      this.bmapIndex = findIndex(this.expenseCategoryList, item => item.id === id) + 1
      if (this._map && this.isSearchCompelete) {
        let pointArr = [{ lng: fromLng, lat: fromLat }, { lng: toLng, lat: toLat }]
        this.createPointArr(pointArr)
        this.createCurrentPath()
      } 
    },
    _getSubsidies () {
      const params = this.title === 'create' ? {} : { userId: this.formData.createUserId }
      getUserDetail(params).then(res => {
        this.travelMoney = res.data.subsidies
      }).catch(err => {
        this.travelMoney = ''
        this.$message.error(err.message)
      })
    },
    historyCell ({ columnIndex }) {
      const grayList = [3, 4, 7, 8]
      return grayList.includes(columnIndex) ? { backgroundColor: '#fafafa' } : {}
    },
    _openServiceOrHistory (isServe) { // 打开服务单详情或者打开历史费用
      if (this.title === 'approve' && !isServe) {
        this.openHistory()
      } else {
        if (!this.isGeneralManager) {
          this.openServiceOrder(this.formData.serviceOrderId, () => this.orderLoading = true, () => this.orderLoading = false)
        }
      } 
    },
    openTravelExpense () { // 新增差补
      this.$refs.travelExpenseDialog.open()
    },
    onOrgChange ({ row, index }) {
      const { expenseOrg } = row
      if (expenseOrg && expenseOrg.length) {
        this.expenseCategoryList.forEach((item, i) => {
          if (i > index) {
            item.expenseOrg = deepClone(expenseOrg)
          }
        })
      }
    },
    async addTravelExpense () {
      if (!this.travelDays) {
        return this.$message.warning('请填写增补的天数')
      }
      this.travelLoading = true
      try {
        // 新增差补
        const { data } = await addTravellingAllowance({
          reimburseInfoId: this.formData.id,
          days: this.travelDays,
          money: this.travelMoney || 0,
          isAdded: true
        })
     
        const { id, money, days, createTime, remark, expenseOrg } = data
        const item = {
          id,
          invoiceTime: formatDate(createTime),
          expenseName: '出差补贴',
          expenseDetail: `${toThousands(money)}元/天*${days}天`,
          money: money * days,
          remark,
          expenseOrg: JSON.parse(expenseOrg || null),
          type: 'travel'
        }
        this.newTravelInfo = item
        this.expenseCategoryList.splice(-3, 0, item)
        this.$refs.travelExpenseDialog.close()
        console.log(data, 'data')
      } catch (err) {
        this.$message.error(err)
      } finally {
        this.travelLoading = false
      }
    },
    travelClosed () {
      this.travelDays = 1
    },
    openHistory () { // 打开历史费用
      if (this.historyCostData && !this.historyCostData.length) {
        return this.$message.warning('暂无历史费用')
      }
      this.$refs.historyDialog.open()
    },
    _getAfterEvaluation () { // 获取售后评价
      if (!this.formData.serviceOrderSapId) {
        return this.$message.error('没有服务ID，无法获取服务评价列表')
      }
      this.afterEvaLoading = true
      getAfterEvaluaton({
        serviceOrderId: this.formData.serviceOrderSapId,
        isReimburse: true,
        technician: this.formData.userName
      }).then(res => {
        res.data.forEach(item => {
          this.totalStart += AFTER_EVALUTION_KEY.reduce((prev, next) => {
            return prev + item[next]
          }, 0)
        })
        const length = res.data.length
        console.log(this.totalStart, length, 'length')
        this.totalStart = this.getTotalStart(this.totalStart / (length * 5))
        console.log(this.totalStart, 'totalStart')
        this.afterEvaluationList = res.data
        this.afterEvaLoading = false
      }).catch(err => {
        this.$message.error(err.message)
        this.afterEvaLoading = false
        console.log('afterEva')
      })
    },
    getTotalStart (totalStart) {
      let str = String(totalStart)
      const [interge, decimal] = str.split('.')
      if (decimal) {
        if (decimal > 0 && decimal < 5) {
          return +interge + .5
        } else if (decimal > 5 && decimal < 10) {
          return +interge + 1
        }
      }
      return +totalStart
    },
    async _getReportDetail () { // 获取服务报告
      this.reportDetaiLoading = true
      try {
        console.log(getDailyReport, collections)
        const res = await getDailyReport({
          serviceOrderId: this.formData.serviceOrderId,
          reimburseId: this.formData.id
        })
        const { dailyDates, reportResults } = res.data
        console.log(dailyDates, reportResults, 'report data')
        this.originReportTableData = []
        if (dailyDates.length) {
          const firstTime = dailyDates[dailyDates.length - 1]
          this.currentTime = new Date(formatDate(firstTime))
          this.nextTime = new Date(collections.addMonth(firstTime, 1))
          this.timeList = [this.currentTime, this.nextTime]
          this.reportDailyList = dailyDates
          console.log(this.reportDailyList, 'reportDailyList')
          reportResults.forEach(item => {
            const { dailyDate, reportDetails } = item
            reportDetails.forEach((materialItem, index) => {
              this.reportTableData.push({
                date: index === 0 ? dailyDate : '',
                dailyDate,
                ...materialItem
              })
            })
          })
          this.originReportTableData = this.reportTableData
          console.log(this.reportTableData, 'reportTableDate')
        }
      } catch (err) {
        this.$message.error(err)
      } finally {
        this.reportDetaiLoading = false
      }
    },
    onCascaderInput (e) {
      e.stopPropogation()
    },
    async _getReimburseOrgs () {
      try {
        const res = await getReimburseOrgs()
        this.reimburseOrgsList = res.data
      } catch (err) {
        this.$message.error(err.message)
      }
    },
    _getHistoryCost () { // 获取历史费用
      this.historyCostLoading = true
      getHistoryReimburseInfo({
        terminalCustomer: this.formData.terminalCustomerId
      }).then(res => {
        this.historyCostLoading = false
        this.historyCostData = res.data
        console.log(res, 'historyList')
      }).catch(err => {
        this.historyCostLoading = false
        this.$message.error(err.message)
        console.log('histroylist')
      })
    },
    normalizeOtherFileList (row) {
      let { isValidInvoice, otherFileList } = row
      return isValidInvoice ? otherFileList : otherFileList.slice(1)
    },
    openFile (row, isInvoiceAttachment) { // 打开发票附件
      console.log(row, 'row')
      let file = isInvoiceAttachment 
        ? (row.isValidInvoice ? row.invoiceFileList[0] : row.otherFileList[0])
        : row
      if (file) {
        let { url, fileType } = file
        if (/^image\/.*$/.test(fileType)) {
          this.previewImage(file) // 预览图片
        } else {
          if (this.isCustomerSupervisor || this.isGeneralManager) {
            this.previewImage(file)
            this.pdfURL = url
          } else {
            window.open(url)
          }
        }
      }
    }, 
    closePDF () {
      this.pdfVisible = false
    },
    previewImage (file) { // 预览附件
      this.previewVisible = true
      this.previewImageUrl = [file]
    },
    closeViewer () {
      this.previewVisible = false
    },
    processStatusIcon (item) { // 处理时间进度条icon
      // iconfont icon-big-circle 阿里巴巴图标
      return item.isFinished 
        ? 'big el-icon-upload-success el-icon-circle-check success' 
        : item.isCurrent
          ? 'iconfont icon-big-circle warning'
          : 'not-current'
    },
    setFinishedStatus (list) { // 设置状态
      return list.map(item => {
        item.isFinished = true // 完成(审批完的状态)
        return item
      })
    },
    _normalizeTimelineList (historyList) { // 格式化操作记录流程
      // 需要展示最新的进度状态，审批到了总经理这一步，每一个步骤最新的状态必然是连续的
      let reimburseStatus = this.formData.remburseStatus // 报销单状态
      console.log(reimburseStatus)
      // key为报销状态 value 截取操作记录的倒数几个
      let statusMap = {
        4: -1,
        5: -2,
        6: -3,
        7: -4,
        8: -5,
        9: -6
      }
      if (statusMap[reimburseStatus]) {
        let length = statusMap[reimburseStatus]
        historyList = this.setFinishedStatus(historyList.slice(length))
      } else {
        historyList = []
      }
      historyList.forEach((item, index) => {
        const { createTime } = item
        const [ymd, hms] = createTime.split(' ')
        item.ymd = ymd
        item.hms = hms
        item.intervalTime = historyList[index + 1] ? historyList[index + 1].intervalTime : ''
      })
      let isCurrent = historyList.length ? true : false // 用来标识当前状态 当前进度后面的都是灰色按钮
      while (historyList.length < 6) { // 进度条一共六个状态
        historyList.push({
          isFinished: false, // 设置进度条未完成的状态
          isCurrent,
          intervalTime: '',
          ymd: '',
          hms: ''
        })
        isCurrent = false
      }
      historyList.forEach((item, index) => {
        item.text = PROGRESS_TEXT_LIST[index]
      })
      console.log('list', historyList)
      return historyList
      // PROGRESS_TEXT_LIST
    },
    rowStyle ({ row }) { // 隐藏删除的表格数据， 因为el-upload的原因
      if (!row.isAdd) {
        return {
          display: 'none'
        }
      }
    },
    headerCellStyle () { 
      let style = {
        color: '#000',
        fontWeight: 'bold',
        fontSize: '12px',
        border: '1px solid #000'
      }
      return style
    },
    cellStyle () {
      return {
        border: 'none'
      }
    },
    noop () {
      noop() 
    },
    getTotal (data, isTravel) { // 获取总金额
      let result = 0
      result += data.reduce((prev, next) => {
        const { days } = next
        const money = (isTravel ? days : 1) * (next.totalMoney || next.money || 0)
        return next.isAdd || isTravel
          ? accAdd(prev, parseFloat(String(money)))
          : prev
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
        console.log('valid', isValid, ifInvoiceAttachment)
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
      if (this.title !== 'create') {
        return
      }
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
    onAreaChange (val) {
      console.log('onAreaChange')
      let { province, city, district, prop, index } = val
      let currentRow = this.formData.reimburseFares[index]
      if (province === '香港特别行政区' || province === '澳门特别行政区') { // 特殊处理
        city = ''
      }
      // 获取对应的坐标点
      this._getPosition(`${province}${city}${district}`.replace('海外', ''), {
        currentRow,
        prop
      })
    },
    changeAddr (scope) { // 交通表格 交换出发地和目的地
      if (!this.ifFormEdit) return
      let { row, $index: index } = scope
      let { from, to, fromLng, fromLat } = row
      let data = this.formData.reimburseFares[index]
      data.from = to
      data.to = from
      data.fromLng = data.toLng
      data.fromLat = data.toLat
      data.toLng = fromLng
      data.toLat = fromLat
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
            invoiceTime: '',
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
            invoiceTime: '',
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
            invoiceTime: '',
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
    toDelete (scope, data, type) {
      if (this.title !== 'create' && type === 'travel') {
        return
      }
      this.$confirm('确认进行删除?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        this.delete(scope, data, type)
      })
    },
    async delete (scope, data, type) {
      if (!this.ifFormEdit) return
      let { id, invoiceFileList, otherFileList } = scope.row
      if (id) { // 说明已经新建过的,新建过的表格数据 invoceFileList 或者otherFileList 是一定存在的
        let params = {
          reimburseInfoId: this.formData.id,
          reimburseCostId: id,
          reimburseType: REIMBURSE_TYPE_MAP[type],
        }
        if (
          (invoiceFileList && invoiceFileList.length) ||
          (otherFileList && otherFileList.length)  
        ) {
          let data = invoiceFileList.length ? invoiceFileList : otherFileList
          if (data[0].reimburseId) { // 导入的数据reimburseId是为空的，所以不需要添加到delteReimburse中
            // this.formData.delteReimburse.push({
            //   deleteId: id,
            //   reimburseType: REIMBURSE_TYPE_MAP[type]
            // })
            await this.deleteCost(params)
          } else {
            // 导入费用
            let index = findIndex(this.selectedList, item => item.id === id) // 找到当前删除行 对应导入之后的数据列表的索引值
            if (index !== -1) {
              this.selectedList.splice(index, 1) // 删除后，让导入的表格回复对应的可选状态
            }
          }
        } else {
          // 出差补贴
          await this.deleteCost(params)
          // this.formData.delteReimburse.push({
          //   deleteId: id,
          //   reimburseType: REIMBURSE_TYPE_MAP[type]
          // })
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
    async deleteCost (params) {
      this.orderLoading = true
      await deleteCost(params).catch(err => {
        this.$message.error(err.message)
        this.orderLoading = false
      })
      this.parentVm._getList()
      this.orderLoading = false
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
      }
      // 删除导入的模板
      file.isAdd = false
      console.log(file, 'file')
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
          invoiceDate: '',
          sellerName: '',
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
        let result = data.map(item => {
          item.themeList = 
          JSON.parse(item.fromTheme.replace(TEXT_REG, ''))
            .map(item => item.description)
          return item
        })
        console.log(result, 'result')
        this.customerInfoList = result
        this.customerTotal = count
      }).catch((err) => {
        console.error(err, 'err')
        this.$message.error('获取用户信息失败')
      })
    },
    closeDialog () {
      this.$refs.customerTable.resetCurrentRow()
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
      console.log(currentRow, 'currentRow')
      if (currentRow) {
        let reg = /[\r|\r\n|\n\t\v]/g
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
        formData.themeList = JSON.parse(formData.fromTheme.replace(reg, ''))
        formData.createUserId = userId
        formData.becity = becity
        formData.businessTripDate = businessTripDate
        formData.endDate = endDate
        formData.destination = destination
        this._checkTravelDays(currentRow)
        this._checkAccMoney()
        this._getServiceDailyExpendSum() // 获取报销单的日费
      }
      this.closeDialog()
    },
    async _getServiceDailyExpendSum () {
      if (this.cancelRequestDailyExpend) {
        this.cancelRequestDailyExpend()
      }
      try {
        this.dailyExpendLoading = true
        const { data } = await getServiceDailyExpendSum({ serviceOrderId: this.formData.serviceOrderId }, this)
        const { travelExpenses, transportExpenses, hotelExpenses, otherExpenses } = data
        travelExpenses.forEach(item => item.isAdd = true)
        this.formData.reimburseTravellingAllowances = travelExpenses

        this._buildAttachment(transportExpenses)
        this._buildAttachment(hotelExpenses)
        this._buildAttachment(otherExpenses)
        this.formData.reimburseFares = transportExpenses
        this.formData.reimburseAccommodationSubsidies = hotelExpenses
        this.formData.reimburseOtherCharges = otherExpenses
        console.log(data, 'dailyExpendSum')
      } catch (err) {
        this.formData.reimburseTravellingAllowances = []
        this.formData.reimburseFares = []
        this.formData.reimburseOtherCharges = []
        this.formData.reimburseAccommodationSubsidies = []
        this.$message.error(err.message)
      } finally {
        this.dailyExpendLoading = false
      }
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
      }).catch((err) => {
        this.costLoading = false
        this.$message.error(err.message)
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
            this.approve()
          } else {
            this.$refs.approve.open() 
          }
        } else {
          this.$message.error('格式错误或必填项未填写')
        }
      })
    },
    closeRemarkDialog () {
      this.onApproveClose()
      this.$refs.approve.close()
    },
    onRemarkInput (val) {
      console.log(val, 'val')
      this.remarkText = val
    },
    onApproveClose () {
      this.remarkType = ''
      this.remarkText = ''
      this.$refs.remark.reset()
    },
    resetInfo () {
      // let { createUserId, userName, orgName } = this.formData
      this.currentTabIndex = 0
      this.$refs.form.clearValidate()
      this.$refs.form.resetFields()
      this.clearFile()
      this.reset()
      if (this.$refs.remark) {
        this.$refs.remark.reset()
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
      const { reimburseTravellingAllowances, reimburseFares, reimburseOtherCharges, reimburseAccommodationSubsidies } = this.formData
      try {
        isFormValid = await this.validate('form')
        if (reimburseTravellingAllowances.length) {
          isTravelValid = await this.validate('travelForm', 'reimburseTravellingAllowances')
        }
        if (reimburseFares.length) {
          isTrafficValid = await this.validate('trafficForm', 'reimburseFares')
        }
        if (reimburseAccommodationSubsidies.length) {
          isAccValid = await this.validate('accForm', 'reimburseAccommodationSubsidies')
        }
        if (reimburseOtherCharges.length) {
          isOtherValid = await this.validate('otherForm', 'reimburseOtherCharges')
        }
        // console.log('checkData', isFormValid, isTravelValid, isTrafficValid, isAccValid, isOtherValid)
        return isFormValid && isTrafficValid && isAccValid && isOtherValid && isTravelValid
      } catch (err) {
        console.log(err)
      }      
    },
    async submit (isDraft) { // 提交
      let formData = deepClone(this.formData)
      let { 
        reimburseTravellingAllowances,
        reimburseAccommodationSubsidies, 
        reimburseOtherCharges, 
        reimburseFares,
        reimburseAttachments,
        attachmentsFileList,
        totalMoney
      } = formData
      if (parseFloat(totalMoney) <= 0) {
        return Promise.reject({ message: '总金额不能为零' })
      }
      formData.reimburseAttachments = [...reimburseAttachments, ...attachmentsFileList]
      this.mergeFileList(reimburseAccommodationSubsidies)
      this.mergeFileList(reimburseOtherCharges)
      this.mergeFileList(reimburseFares)
      this.addSerialNumber(reimburseTravellingAllowances)
      this.addSerialNumber(reimburseAccommodationSubsidies)
      this.addSerialNumber(reimburseOtherCharges)
      this.addSerialNumber(reimburseFares)
      formData.myExpendsIds = this.selectedList.map(item => item.id)
      let isValid = await this.checkData()
      if (!isValid) {
        return Promise.reject({ message: this.errMessage })
      }
      formData.isDraft = !!isDraft
      console.log(addOrder)
      return addOrder(formData)
    },
    async updateOrder (isDraft) { // 编辑
      let formData = deepClone(this.formData)
      let { 
        reimburseTravellingAllowances,
        reimburseAccommodationSubsidies, 
        reimburseOtherCharges, 
        reimburseFares,
        reimburseAttachments,
        attachmentsFileList,
        totalMoney
      } = formData
      if (parseFloat(totalMoney) <= 0) {
        return Promise.reject({ message: '总金额不能为零' })
      }
      formData.reimburseAttachments = [...reimburseAttachments, ...attachmentsFileList]
      this.mergeFileList(reimburseAccommodationSubsidies)
      this.mergeFileList(reimburseOtherCharges)
      this.mergeFileList(reimburseFares)
      this.addSerialNumber(reimburseTravellingAllowances)
      this.addSerialNumber(reimburseAccommodationSubsidies)
      this.addSerialNumber(reimburseOtherCharges)
      this.addSerialNumber(reimburseFares)
      formData.myExpendsIds = this.selectedList.map(item => item.id) // 导入的数据ID
      let isValid = await this.checkData()
      if (!isValid) {
        return Promise.reject({ message: this.errMessage })
      }
      formData.isDraft = !!isDraft
      console.log(updateOrder)
      return updateOrder(formData)
    },
    mergeOrgList (type) { // 归类发票给归属
      return this.formData.expenseCategoryList
        .filter(item => item.type === type)
        .map(item => {
          const { id, expenseOrg: value } = item
          return { id, value: JSON.stringify(value) }
        })
    },
    approve () {
      this._approve()
    },
    async _approve () {
      // this.$refs.form.validate(async isValid => {
      //   if (!isValid) {
      //     return this.$message.error('格式错误或必填项未填写')
      //   }
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
      let isOrgValid = true
      if (this.isCustomerSupervisor && this.remarkType !== 'reject') { // 如果是客服主管必填部门选项并且点击的是同意
        try {
          await this.$refs.expenseForm.validate()
          params.travelOrgResults = this.mergeOrgList('travel')
          params.transportOrgResults = this.mergeOrgList('traffic')
          params.hotelOrgResults = this.mergeOrgList('acc')
          params.otherOrgResults = this.mergeOrgList('other')
        } catch (err) {
          isOrgValid = false
        }
      }
      if (!isOrgValid) {
        return this.$message.error('费用详情列表的费用归属必填')
      }
      console.log(approve, params)
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
        if (this.remarkType === 'reject') {
          this.remarkLoading = false
          this.closeRemarkDialog()
        } else {
          this.parentVm.closeLoading()
        }
        this.parentVm._getList()
        this.parentVm.closeDialog()
      }).catch((err) => {
        this.remarkType === 'reject'
          ? this.remarkLoading = false
          : this.parentVm.closeLoading()
        this.$message.error(err.message)
      })
      // })       
    }
  }
}
</script>
<style lang='scss' scoped>
.order-wrapper {
  position: relative;
  font-size: 12px;
  /* 日历样式 */
  .date-map-container {
    position: absolute;
    flex-direction: column;
    flex-wrap: wrap;
    top: -84px;
    left: -12px;
    // transform: translate3d(-100%, 0, 0);
    margin-left: -500px;
   
    #date-picker-wrapper {

    }
    /* 地图样式 */
    #map-container {}
    
  }
  
  .control-info {
    position: fixed;
    z-index: 100000;
    left: 0;
    top: 0;
    font-size: 12px;
    border: 1px solid #000;
    background-color: #fff;
  }
  /* 地图自定义控件的样式 */
  .success {
    font-size: 14px;
    color: rgba(0, 128, 0, 1);
  }
  .warning {
    font-size: 14px;
    color: rgba(255, 165, 0, 1);
  }
  /* 总经理审批头部 */
  .general-title-wrapper {
    font-weight: bold;
    font-size: 30px;
    .title {
      height: 50px;
      line-height: 50px;
    }
    .info-wrapper {
      position: absolute;
      right: 10px;
      top: 10px;
      font-size: 14px;
      span:nth-child(1) {
        display: inline-block;
        width: 50px;
      }
      span {
        &:nth-child(2) {
          font-weight: normal;
        }
      }
    }
  }
  /* 总经理头部时间进度条 */
  .timeline-progress-wrapper {
    position: absolute;
    left: 71px;
    top: -43px;
    width: 400px;
    height: 1px;
    &.isGeneral {
      width: 690px;
      left: 84px;
      top: -55px;
    }
    .content-wrapper {
      position: absolute;
      z-index: 2;
      left: 0;
      right: 0;
      bottom: 0;
      top: 0;
      .interval-content {
        position: absolute;
        z-index: 2;
        left: 0;
        right: 0;
        bottom: 0;
        top: 0;
        .interval-time {
          position: relative;
          top: -12px;
          flex: 1;
          font-size: 12px;
          text-align: center;
          color: #3ef551;
          &:nth-last-child(1) {
            opacity: 0;
          }
        }
      }
      .content-item {
        display: flex;
        align-items: center;
        position: relative;
        flex: 1;
        &:nth-child(1) {
          .line {
            opacity: 0;
          }
        }
        .line {
          position: absolute;
          left: 0;
          top: 6px;
          width: 100%;
          transform: translate3d(-100%, 0, 0);
          height: 1px;
          background-color: rgba(206, 206, 206, 1); 
          &.finished {
            background-color: #3fcf15;
          }
          &.current {
            background: linear-gradient(to right, #3fcf15 50%, #ffa724 50%);
          }
        }
        .icon-status {
          position: relative;
          z-index: 100;
          display: inline-block;
          width: 12px;
          height: 12px;
          line-height: 12px;
          font-size: 12px;
          background-color: #fff;
          border-radius: 50%;
          border: 1px solid #ffa724;
          &.finished {
            border: none;
            background-color: #3fcf15;
          }
          &.not-current {
            border: 1px solid rgba(206, 206, 206, 1);  
          }
          &.text-content {
            border: 1px solid rgba(206, 206, 206, 1);  
          }
        }
        .text {
          position: absolute;
          left: 0;
          top: -22px;
          font-size: 12px;
          white-space: nowrap;
          color: #000;
          transform: translate3d(-36%, 0, 0);
        }
        .time {
          position: absolute;
          left: 0;
          top: 17px;
          font-size: 12px;
          color: #000;
          text-align: center;
          white-space: nowrap;
          transform: translate3d(-36%, 0, 0);
        }
        // background-color: #fff;
      }
    }
  }
  /* 头部 */
  .head-title-wrapper {
    position: absolute;
    top: -41px;
    left: 76px;
    &.general-manager {
      left: 735px;
      top: -66px;
      font-size: 14px !important;
    }
    &.general {
      left: 480px;
      span {
        // font-weight: bold;
      }
    }
    p {
      min-width: 65px;
      margin-right: 10px;
      font-size: 12px;
      // font-weight: bold;
      color: #BFBFBF;
      &.underline {
        text-decoration: underline;
      }
      span {
        font-weight: normal;
        color: #222;
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
    .form-theme-content {
      position: relative;
      box-sizing: border-box;
      min-height: 30px;
      padding: 1px 15px 1px 1px;
      color: #606266;
      font-size: 12px;
      line-height: 1.5;
      border-radius: 4px;
      border: 1px solid #E4E7ED;
      background-color: #F5F7FA;
      outline: none;
      transition: border-color .2s cubic-bezier(.645, .045, .355, 1);
      cursor: not-allowed;
      &.uneditable {
        border: 1px solid #DCDFE6;
        background-color: #fff;
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
        }
      }
    }
  }
  .scroll-bar {
    &.el-scrollbar {
       ::v-deep {
        .el-scrollbar__wrap {
          // max-height: 700px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
    ::v-deep .el-upload-list__item {
      margin-top: 0;
      &:first-child {
        margin-top: 0;
      }
    }
    /* 总经理审批表单 */
    .general-order-wrapper {
      margin-top: 10px;
      .general-form-wrapper {
        padding: 5px;
        &.manager {
          padding: 14px;
        }
        // border: 1px solid #000;
        .item {
          margin-bottom: 10px;
          &:nth-last-child(1) {
            margin-bottom: 0;
          }
          .title {
            flex: 0 0 50px;
            margin-right: 5px;
            color: #BFBFBF;
          }
          .theme-item {
            display: inline-block;
            margin-right: 0;
            .order-icon {
              display: inline-block;
              width: 12px;
              height: 13px;
              font-size: 12px;
              text-align: center;
              line-height: 10px;
              border: 1px solid black;
              border-radius: 50%;
            }
          }
        }
        .content {
          width: 270px;
        }
        .content-long {
          max-width: 440px;
        }
        div.customer-id {
          display: flex;
          min-width: 120px;
          margin-right: 35px;
          font-size: 12px;
          font-weight: bold;
          span {
            &:nth-child(1) {
              width: 50px;
              margin-right: 10px;
            }
            &:nth-child(2) {
              font-weight: normal;
            }
          }
        }
      }
    }
    /* 支付时间 */
    .pay-time {
      margin-top: 10px;
      span {
        margin-left: 10px;
        color: #BFBFBF;
      }
    }
    /* 总经理审批表格 */
    .general-table-wrapper {
      // padding-right: 10px;
      // width: 828px;
      // width: 1029px;
      margin-top: 5px;
      ::v-deep .cell {
        line-height: 16px;
      }
      .table-container {
        overflow: visible;
        &::v-deep {
          .el-cascader {
            &.disabled {
              input {
                border: none;
                background: transparent !important;
                color: #606266 !important;
                cursor: auto !important;
              }
              .el-input__suffix {
                display: none;
              }
            }
            input {
              height: 28px;
              line-height: 28px;
            }
          }
        }
        .detail-content {
          position: relative;
          display: inline-block;
          max-width: 280px;
          p {
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
          }
          .remark {
            // position: absolute;
            // right: -14px;
            // bottom: 2px;
            color: red;
          }
        }
        .invoice-number-wrapper {
          position: relative;
        }
      }
      ::v-deep .cell {
        line-height: 16px;
      }
      ::v-deep .el-table__header {
        border-collapse: collapse;
      }
      ::v-deep .el-table .el-table__header-wrapper {
        overflow: visible;
      }
      ::v-deep .el-table th, .el-table tr {
        background-color: transparent;
      }
      ::v-deep .el-table .th.is-leaf {
        border-bottom: 1px solid #000 !important;
      }
      ::v-deep .el-table--border, .el-table--group{
        border-color: none !important;
      }
      ::v-deep .el-table--border::after, .el-table--group::after{
        width: 0;
      }
      .table-container:before {
        width: 0;
        height: 0;
      }
    }
    /* 总经理审批总金额 */
    .general-total-money {
      width: 1027px;
      padding-right: 5px;
      margin: 10px 0;
      font-size: 12px;
      font-weight: bold;
      border-top: 1px solid #000;
      border-bottom: 1px solid #000;
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
  /* 发票时间日期选择器 */
  .invoice-time {
    color: red;
    &::v-deep {
      > input {
        padding: 0 5px !important;
      }
      .el-input__prefix {
        display: none;
      }
    }
  }
  ::v-deep .el-input__icon {
    &.success {
      color: rgba(0, 128, 0, 1);
    }
    &.warning {
      color: rgba(255, 165, 0, 1);
    }
  }
  /* 费用详情 */
  .expense-detail-wrapper {
    margin-top: 10px;
    // .title-wrapper {
    //   margin-right: 80px;
    //   &.gerneral {
    //     margin-right: 0;
    //   }
    // }
    .area {
      div {
        display: inline-block;
        &:nth-child(1) {
          margin-right: 20px;
        }
        span {
          color: #BFBFBF;
          margin-left: 10px;
        }
      }
    }
  } 
  /* 工作日报样式 */
  .daily-report-wrapper {
    margin: 10px 0;
    .area {
      div {
        display: inline-block;
        &:nth-child(1) {
          margin-right: 20px;
        }
        span {
          color: #BFBFBF;
          margin-left: 10px;
        }
      }
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
  /* 历史费用弹窗表格 */
  .history-table-wrapper {
  }
  /* 时间线 */

  .el-timeline-scroll-bar {
    &.el-scrollbar {
       ::v-deep {
        .el-scrollbar__wrap {
          max-height: 200px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
    .my-timeline-wrapper {
      margin-top: 3px;
      margin-left: 100px;
      font-size: 12px;
      &.el-timeline {
        ::v-deep {
          .el-timeline-item {
            padding-bottom: 0;
            .el-timeline-item__tail {
              height: 150%;
            }
          } 
          .el-timeline-item__wrapper {
            margin-left: -127px;
            .el-timeline-item__content {
              span {
                margin-right: 20px;
                &:nth-child(1) {
                  width: 100px;
                  margin-right: 30px;
                }
                &.danger {
                  color: red;
                }
                &.bold {
                  min-width: 70px;
                  font-weight: bold;
                }
              }
            }
          }
        }
      }
    }
  }
  .history-wrapper {
    width: 898px;
    &.general {
      width: 830px;
    }
    ::v-deep .el-table {
      &::before {
        height: 0;
      }
    }
  }
  /* 各个表格的样式 */
  .form-item-wrapper {
    &.travel {
      width: 400px;
    }
    &.acc {
      width: 1201px;
      &.uneditable {
        width: 1041px;
      }
    }
    &.other {
      width: 1138px;
      &.uneditable {
        width: 970px;
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
          font-size: 12px;
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
    .my-area-selector {
      ::v-deep {
        input {
          height: 28px;
          line-height: 28px;
        }
      }
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