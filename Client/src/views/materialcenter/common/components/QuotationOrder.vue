<template>
  <div class="quotation-wrapper" v-loading.fullscreen="contentLoading">
    <el-row type="flex" class="title-wrapper">
      <p v-if="formData.salesOrderId && isSales"><span>销售订单</span><span>{{ formData.salesOrderId }}</span></p>
      <p v-if="!isSales"><span>{{ isSales ? '报价单号' : '领料单号' }}</span><span>{{ formData.id || '' }}</span></p>
      <p><span>申请人</span><span v-if="formData.createUser">{{ formData.orgName }}-{{ formData.createUser || createUser }}</span></p>
      <p><span>创建时间</span><span>{{ formData.createTime | formatDateFilter }}</span></p>
      <p><span>销售员</span><span>{{ formData.salesMan }}</span></p>
      <p v-if="formData.salesOrderId && !isSales">销售订单：<span>{{ formData.salesOrderId }}</span></p>
    </el-row>
    <!-- 主题内容 -->
    <el-scrollbar class="scroll-bar">
      <!-- 表单 -->
      <common-form
        :model="formData"
        :formItems="formItems"
        ref="form"
        class="my-form-wrapper"
        :class="{ 'my-form-view': !ifEdit && isPreview }"
        label-width="60px"
        :disabled="!ifEdit"
        label-position="right"
        :show-message="false"
        :isCustomerEnd="true"
        :rules="formRules"
        :hide-required-asterisk="true"
      >
        <template v-slot:serviceOrderSapId>
          <el-form-item
            style="height: 18px;"
            prop="serviceOrderSapId"
            :rules="formRules['serviceOrderSapId'] || { required: false }">
            <span slot="label">
              <div class="link-container" style="display: inline-block">
                <img v-if="ifInApprovePage" :src="rightImg" @click="_openServiceOrder" class="pointer">
                <span>服务ID</span>
              </div>
            </span>
            <el-input v-infotooltip.top.ellipsis size="mini" v-model="formData.serviceOrderSapId" @focus="onServiceIdFocus" :disabled="status !== 'create'"></el-input>
          </el-form-item>
        </template>
      </common-form>
      <el-row class="prepay-wrapper" type="flex" align="middle" v-if="ifShowPrepaid" justify="end" :class="{ 'if-not-edit': !ifEdit }">
        <template v-if="ifEdit">
          <el-row type="flex" align="middle">
            预付<el-input-number v-model="formData.prepay" :controls="false" :min="0"></el-input-number>%
          </el-row>
          <el-row type="flex" align="middle">
            发货前<el-input-number v-model="formData.cashBeforeFelivery" :controls="false" :min="0"></el-input-number>%
          </el-row>
          <el-row type="flex" align="middle">
            货到验收<el-input-number v-model="formData.payOnReceipt" :controls="false" :min="0"></el-input-number>%
          </el-row>
          <el-row type="flex" align="middle">
            质保后<el-input-number v-model="formData.paymentAfterWarranty" :controls="false" :min="0"></el-input-number>%
          </el-row>
        </template>
        <template v-else>
          <span>预付{{ formData.prepay || 0 }}%</span>
          <span>发货前{{ formData.cashBeforeFelivery || 0 }}%</span>
          <span>货到验收{{ formData.payOnReceipt || 0 }}%</span>
          <span>质保后{{ formData.paymentAfterWarranty || 0 }}%</span>
        </template>
      </el-row>
      <!-- 技术员回传文件 -->
      <el-row class="upload-file-wrapper" type="flex" v-if="status === 'upload' && isSales">
        <span class="title-text">附件</span>
        <upLoadFile 
          :disabled="false"
          @get-ImgList="getFileList" 
          ref="uploadFile"
          :ifShowTip="true"
          :onAccept="onAccept"
          :fileList="formData.attachmentsFileList || []"
          >
        </upLoadFile>
      </el-row>
      <!-- 分割线 -->
      <div class="divider"></div>
      <!-- 工程和总经理审批报价单 -->
      <template v-if="ifShowSerialTable">
        <div class="approve-class">
          <!-- <el-form v-if="formData.quotationProducts.length" class="approve-search-wrapper" :inline="true" :model="listQueryApprove" size="mini">
            <el-form-item label="序列号">
              <el-input v-model.trim="listQueryApprove.manufacturerSerialNumbers" @keyup.enter.native="_getSerialDetail"></el-input>
            </el-form-item>
            <el-form-item label="物料编码">
              <el-input v-model.trim="listQueryApprove.materialCode" @keyup.enter.native="_getSerialDetail"></el-input>
            </el-form-item>
            <el-button type="primary" @click.native="_getSerialDetail" size="mini">搜索</el-button>
          </el-form> -->
          <ul class="serial-table-list" v-loading="serialLoading">
            <li class="serial-item" v-for="item in formData.quotationProducts" :key="item.id">
              <el-row class="info-title" type="flex">
                <svg-icon iconClass="warranty" v-if="item.isProtected"></svg-icon>
                <div>序列号<span>{{ item.productCode }}</span></div>
                <div>物料编码<span>{{ item.materialCode }}</span></div>
                <div v-if="item.warrantyExpirationTime">保修到期<span>{{ item.warrantyExpirationTime | formatDateFilter }}</span></div>
              </el-row>
              <common-table
                :data="item.quotationMaterials" 
                :columns="approveColumns" 
              >
                <template v-slot:materialCode="{ row }">
                  <el-row type="flex" align="middle">
                    <span v-infotooltip.top.ellipsis :class="{ 'has-icon': row.replaceMaterialCode || row.newMaterialCode }">{{ row.materialCode }}</span>
                    <svg-icon iconClass="replace" v-if="row.replaceMaterialCode"></svg-icon>
                    <svg-icon iconClass="new-material" v-if="row.newMaterialCode"></svg-icon>
                  </el-row>
                </template>
                <template v-slot:materialType="{ row }">
                  <div v-infotooltip.top.ellipsis>{{ materialTypeMap[row.materialType] }}</div>
                </template>
                <template v-slot:count="{ row }">
                  <div v-infotooltip.top.ellipsis class="bold">{{ row.count }}</div>
                </template>
                <template v-slot:unitPrice="{ row }">
                  <div v-infotooltip.top.ellipsis>{{ row.unitPrice | toThousands(3, ',', 4) }}</div>
                </template>
                <template v-slot:salesPrice="{ row }">
                  <div v-infotooltip.top.ellipsis>{{ row.salesPrice | toThousands(3, ',', 4) }}</div>
                </template>
                <template v-slot:discountPrices="{ row }">
                  <div v-infotooltip.top.ellipsis class="bold">{{ row.discountPrices | toThousands(3, ',', 4) }}</div>
                </template>
                <!-- 折扣 -->
                <template v-slot:discount="{ row }">
                  <div v-infotooltip.top.ellipsis>{{ row.discount }}%</div>
                </template>
                <!-- 总价格 -->
                <template v-slot:totalPrice="{ row }">
                  <!-- <div class="bold" v-infotooltip.top.ellipsis style="text-align: right;">{{ isInServiceOrTravel(row.materialCode) || isGeneralManager ? row.totalPrice : (item.isProtected ? 0 : row.totalPrice) | toThousands }}</div> -->
                  <div class="bold" v-infotooltip.top.ellipsis style="text-align: right;">{{ row.totalPrice | toThousands }}</div>
                </template>
              </common-table>
              <!-- <el-row type="flex" justify="end" class="total-line">
                <p v-infotooltip.top.ellipsis>{{ item.quotationMaterials | calcTotalItem(false, 'unitPrice') | toThousands }}</p>
                <p v-infotooltip.top.ellipsis>{{ item.quotationMaterials | calcTotalItem(item.isProtected, 'salesPrice') | toThousands }}</p>
              </el-row> -->
              <el-row class="info-wrapper" type="flex" justify="end" align="middle" style="margin-right: 97px;">
                <!-- <div v-if="!isGeneralManager"> -->
                  <!-- <span class="title">应付</span> -->
                  <!-- <span>{{ item.quotationMaterials | calcTotalItem(isGeneralManager ? false : item.isProtected) | toThousands }}</span> -->
                  <!-- <span>{{ item.quotationMaterials | calcTotalItem('totalPrice', true) | toThousands }}</span> -->
                <!-- </div> -->
                <div>
                  <span class="title">合计</span>
                  <!-- <span>{{ item.quotationMaterials | calcTotalItem(isGeneralManager ? false : item.isProtected) | toThousands }}</span> -->
                  <span>{{ item.quotationMaterials | calcTotalItem | toThousands }}</span>
                </div>
              </el-row>
            </li>
          </ul>
          <!-- 工程总经理审批报价单才出现 -->
          <el-row class="info-wrapper" type="flex" justify="end" align="middle" style="margin-right: 97px;">
            <div v-if="!isGeneralManager">
              <span class="title">应付</span>
              <span>￥{{ formData.quotationProducts | calcTotalDealMoney | toThousands }}</span>
            </div>
            <div>
              <span class="title">总计</span>
              <span>{{ totalMoney | toThousands }}</span>
            </div>
          </el-row>
        </div>
      </template>
      <!-- 报价单模块 -->
      <template v-if="isReceive && (status === 'create' || (status === 'edit' && hasEditBtn))">
        <!-- 非预览 -->
        <template>
          <!-- 序列号查询表格 -->
          <div class="serial-wrapper">
            <el-row type="flex" justify="space-between">
              <div class="form-wrapper">
                <el-form :model="listQuerySearial" size="mini">
                  <el-row type="flex">
                    <el-col>
                      <el-form-item>
                        <el-input 
                          style="width: 150px;"
                          @keyup.enter.native="_getSerialNumberList" 
                          v-model.trim="listQuerySearial.manufacturerSerialNumbers" 
                          placeholder="制造商序列号"
                          size="mini">
                        </el-input>
                      </el-form-item>
                    </el-col>
                    <el-col>
                      <el-form-item>
                        <el-input 
                          style="width: 150px; margin-left: 10px;"
                          @keyup.enter.native="_getSerialNumberList" 
                          v-model.trim="listQuerySearial.materialCode" 
                          placeholder="物料编码"
                          size="mini">
                        </el-input>
                      </el-form-item>
                    </el-col>
                  </el-row>
                </el-form>
              </div>
            </el-row>
            <!-- 设备序列号表格 -->
            <div class="serial-table-wrapper">
              <common-table 
                @row-click="onSerialRowClick"
                ref="serialTable" 
                max-height="200px"
                :data="serialNumberList" 
                :columns="serialColumns" 
                :loading="serialLoading">
                <template v-slot:materialCode="{ row }">
                  <el-row type="flex" align="middle">
                    <div v-infotooltip.top.ellipsis class="ellipsis" :class="{ 'has-icon': row.isProtected }">{{ row.materialCode }}</div>
                    <svg-icon iconClass="warranty" v-if="row.isProtected"></svg-icon>
                  </el-row>
                </template>
                <template v-slot:btn="scope">
                  <el-button 
                    type="success"
                    @click="openMaterialDialog(scope.row)"
                    size="mini">
                      <el-icon class="el-icon-edit">添加</el-icon>
                    </el-button>
                </template>
                <!-- 小计 -->
                <template v-slot:money="scope">
                  {{ scope.row.manufacturerSerialNumber | calcSerialTotalMoney(formData.quotationProducts) | toThousands }}
                </template>
              </common-table>
              <el-row type="flex" class="money-line" justify="end" align="middle">
                <!-- <el-form 
                  ref="serviceForm" 
                  :show-message="false"
                  :model="formData" 
                  label-width="80px" 
                  :inline="true" 
                  :rules="serviceRules"
                >
                  <el-form-item label="服务费" prop="serviceCharge">
                    <el-input-number 
                      v-model="formData.serviceCharge"
                      :controls="false"
                      :min="0"
                      :precision="2"
                      size="mini"
                    ></el-input-number>
                  </el-form-item>
                </el-form> -->
                <div style="margin-left: 10px;">
                  <span class="title">合计</span>
                  {{ totalMaterialMoney | toThousands }}
                </div>
              </el-row>
            </div>
          </div>
          <div class="divider"></div>
          <!-- 物料填写表格 -->
          <div class="material-wrapper">
            <el-row type="flex" style="margin-bottom: 10px;" align="middle">
              <span style="margin-right: 12px;color: #BFBFBF;">序列号</span>
              <span style="margin-right: 10px;">{{ currentSerialNumber }}</span>
              <el-button size="mini" type="primary" :disabled="!currentSerialNumber" @click.native="customAddMaterial">新增物料</el-button>
            </el-row>
            <el-form 
              ref="materialForm" 
              :model="materialData" 
              size="mini" 
              :show-message="false"
              class="form-wrapper"
            >
              <common-table
                class="material-table-wrapper"
                :data="materialData.list"
                :columns="materialConfig"
                max-height="200px"
              >
                <template v-slot:materialCode="{ row }">
                  <el-row type="flex" align="middle">
                    <span v-infotooltip.top.ellipsis :class="{ 'has-icon': row.replaceMaterialCode || row.newMaterialCode }">{{ row.materialCode }}</span>
                    <svg-icon iconClass="replace" v-if="row.replaceMaterialCode"></svg-icon>
                    <svg-icon iconClass="new-material" v-if="row.newMaterialCode"></svg-icon>
                  </el-row>
                </template>
                <template v-slot:materialType="{ row, index }">
                  <el-form-item
                    style="height: 28px;"
                    :prop="'list.' + index + '.' + 'materialType'"
                    :rules="materialRules['materialType']"
                  >
                    <el-select
                      v-infotooltip.top.ellipsis
                      v-model="row.materialType" 
                      style="width: 100%;"
                      clearable
                      @change="onMaterialTypeChange(row, index)"
                      placeholder="请选择">
                      <el-option
                        v-for="item in materialTypeOptions"
                        :key="item.value"
                        :label="item.label"
                        :value="item.value">
                      </el-option>
                    </el-select>
                  </el-form-item>
                </template>
                <!-- 数量 -->
                <template v-slot:count="{ row, index }">
                  <el-form-item
                    style="height: 28px;"
                    :prop="'list.' + index + '.' + 'count'"
                    :rules="materialRules['count']"
                  >
                    <el-input-number
                      v-infotooltip.top.ellipsis
                      size="mini"
                      style="width: 200px;"
                      :controls="false"
                      v-model="row.count"
                      :precision="0"
                      :placeholder="`最大数量${row['maxQuantity']}`"
                      :max="row.newMaterialCode ? Infinity : Math.ceil(row['maxQuantity'])"
                      :min="0"
                      @focus="onCountFocus(index)"
                      @change="onCountChange"
                    ></el-input-number>
                  </el-form-item>
                </template>
                <template v-slot:unitPrice="{ row }">
                  <div v-infotooltip.top.ellipsis>{{ row.unitPrice | toThousands(3, ',', 4) }}</div>
                </template>
                <template v-slot:salesPrice="{ row }">
                  <div v-infotooltip.top.ellipsis>{{ row.salesPrice | toThousands(3, ',', 4) }}</div>
                </template>
                <!-- <template v-slot:discountPrices="{ row }">
                  <div v-infotooltip.top.ellipsis>{{ row.discountPrices | toThousands(3, ',', 4) }}</div>
                </template> -->
                <template v-slot:totalPrice="{ row }">
                  <span v-infotooltip.top.ellipsis style="text-align: right;">{{ row.totalPrice | toThousands }}</span>
                </template>
                <template v-slot:maxQuantity="{ row }">
                  <el-row type="flex" justify="end" align="middle">
                    <span v-infotooltip.top.ellipsis style="width: calc(100% - 12px); padding-right: 3px;">{{ row.maxQuantity }}</span>
                    <el-tooltip effect="dark" placement="top-end">
                      <div slot="content">领取的数量只能是整数，大于等于当前最大数量</div>
                      <i class="notice-icon el-icon-warning-outline" v-if="!isIntegerNumber(+row.maxQuantity)"></i>
                    </el-tooltip>
                  </el-row>
                </template>
                <!-- 备注 -->
                <template v-slot:remark="{ row }">
                  <el-input size="mini" v-infotooltip.top.ellipsis v-model="row.remark"></el-input>
                </template>
                <template v-slot:discountPrices="{ row, index }">
                  <el-form-item
                    style="height: 28px;"
                    :prop="'list.' + index + '.' + 'discountPrices'"
                    :rules="materialRules['discountPrices']"
                  >
                    <el-input-number 
                      v-infotooltip.top.ellipsis
                      size="mini"
                      v-model="row.discountPrices" 
                      @change="onDiscountPricesChange(row)"
                      @focus="onDiscountFocus(index)"
                      :controls="false"
                      :precision="4"
                    ></el-input-number>
                  </el-form-item>
                  <!-- <div v-infotooltip.top.ellipsis>{{ row.discountPrices | toThousands(3, ',', 4) }}</div> -->
                </template>
                <!-- 折扣 -->
                <template v-slot:discount="{ row, index }">
                  <el-form-item
                    style="height: 28px;"
                    :prop="'list.' + index + '.' + 'discount'"
                    :rules="materialRules['discount']"
                  >
                    <el-input
                      v-infotooltip.top.ellipsis
                      size="mini"
                      v-model="row.discount" 
                      placeholder="大于等于40"
                      :readonly="true"
                      @focus="onDiscountFocus(index)"
                    ></el-input>
                  </el-form-item>
                </template>
                <!-- 总价格 -->
                <!-- <template v-slot:totalPrice="{ row }">
                  <span style="text-align: right;">{{ row.totalPrice | toThousands}}</span>
                </template> -->
                <!-- 操作 -->
                <template v-slot:operation="{ index }">
                  <div style="display: inline-block;" @click="deleteMaterialItem(index)">
                    <el-icon class="el-icon-delete icon-item"></el-icon>
                  </div>
                </template>
              </common-table>
            </el-form>
            <el-row class="subtotal-wrapper">
              <span class="title">合计</span>
              {{ materialData.list | calcTotalItem | toThousands }}
            </el-row>
          </div>
          <div class="divider"></div>
          <!-- 服务费用和差旅费用 -->
          <div class="service-travel-wrapper">
            <common-table 
              :data="serviceList"
              :columns="serviceColumns"
            >
              <template v-slot:salesPrice="{ row }">
                <el-input-number  
                  style="width: 100%;"
                  size="mini"
                  :controls="false"
                  :precision="2"
                  :min="0"
                  :disabled="formData.isMaterialType === true"
                  v-model="row.salesPrice">
                </el-input-number>
              </template>    
            </common-table>
            <el-row type="flex" align="middle" justify="end" style="margin-top: 10px;">
              <div style="margin-left: 10px;">
                <span class="title">合计</span>
                {{ totalMoney | toThousands }}
              </div>
            </el-row>
          </div>
        </template>
      </template>
      <!-- 不是新建状和审批和编辑态并且处于预览状态 -->
      <template v-if="ifShowMergedTable">
        <!-- 物料汇总表格 -->
        <div class="material-summary-wrapper">
          <common-table 
            class="material-summary-table"
            row-key="index"
            max-height="300px"
            :data="materialSummaryList" 
            :columns="materialAllColumns"
            :loading="materialAllLoading">
            <template v-slot:materialCode="{ row }">
              <el-row type="flex" align="middle" >
                <span v-infotooltip.top.ellipsis :class="{ 'has-icon': row.isProtected }">{{ row.materialCode }}</span>
                <svg-icon iconClass="warranty" v-if="row.isProtected"></svg-icon>
              </el-row>
            </template>
            <template v-slot:discount="{ row }">
              <div v-infotooltip.top.ellipsis>{{ row.discount }}%</div>
            </template>
          </common-table>
          <el-row type="flex" class="money-wrapper" justify="end">
            <p v-infotooltip.top.ellipsis>{{ summaryTotalPrice | toThousands }}</p>
            <p v-infotooltip.top.ellipsis>{{ costPirceTotal | toThousands}}</p>
            <p v-infotooltip.top.ellipsis>{{ grossProfitTotal | toThousands }}</p>
            <p v-infotooltip.top.ellipsis>{{ grossProfit | toThousands }}%</p>
          </el-row>
        </div>
      </template>
      <div class="divider" v-if="status !== 'create' && status !== 'approve' && status !== 'edit'"></div>
      <!-- 操作记录 不可编辑时才出现 -->
      <template v-if="ifShowHistory && formData.quotationOperationHistorys && formData.quotationOperationHistorys.length">
        <div class="history-wrapper">
          <common-table 
            :data="formData.quotationOperationHistorys" 
            :columns="historyColumns" 
            max-height="200px"
          >
            <template v-slot:createTime="{ row }">
              {{ row. createTime | formatDateFilter }}
            </template>
            <template v-slot:intervalTime="scope">
              {{ scope.row.intervalTime | s2HMS }}
            </template>
          </common-table>
        </div>
        <div class="timeline-progress-wrapper" v-if="isTechnical">
          <el-row class="content-wrapper" type="flex" align="middle" justify="space-between">
            <template v-for="(item, index) in timeList">
              <div class="content-item" :key="index">
                <el-tooltip  palcement="top-center" :disabled="!item.isFinished" :content="item.createTime">
                  <!-- <div> -->
                    <i class="icon-status" :class="processStatusIcon(item)"></i>
                  <!-- </div> -->
                </el-tooltip>
                <p class="text">{{ item.text }}</p>
              </div>
            </template>
          </el-row>
        </div>
      </template>
    </el-scrollbar>
    <!-- 客户弹窗 -->
    <my-dialog
      ref="customerDialog"
      title="服务列表"
      width="770px"
      :btnList="customerBtnList"
      :append-to-body="true"
      :destroy-on-close="true"
    >
      <common-table 
        ref="customerTable"
        v-loading="customerLoading"
        :data="customerData" 
        :columns="customerColumns" 
        max-height="400px"
        radioKey='id'>
      </common-table>
      <pagination
        v-show="customerTotal > 0"
        :total="customerTotal"
        :page.sync="listQueryCustomer.page"
        :limit.sync="listQueryCustomer.limit"
        @pagination="customerCurrentChange"
      />
    </my-dialog>
    <!-- 物料编码弹窗 -->
    <my-dialog 
      ref="materialDialog"
      title="物料编码"
      width="840px"
      :btnList="materialBtnList"
      :append-to-body="true"
      @closed="closeMaterialDialog"
      :destroy-on-close="true"
    >
      <el-row type="flex" justify="space-between">
        <div class="form-wrapper">
          <el-form :model="listQueryMaterial" size="mini" :inline="true">
            <el-form-item label="物料编码">
              <el-input 
                style="width: 150px;"
                @keyup.enter.native="_getMaterialList" 
                v-model.trim="listQueryMaterial.partCode">
              </el-input>
            </el-form-item>
            <el-form-item label="物料描述">
              <el-input 
                style="width: 150px; margin-left: 10px;"
                @keyup.enter.native="_getMaterialList" 
                v-model.trim="listQueryMaterial.partDescribe">
              </el-input>
            </el-form-item>
          </el-form>
        </div>
      </el-row>
      <common-table  
        ref="materialTable" 
        row-key="itemCode"
        height="400px"
        :data="materialList" 
        :columns="materialColumns" 
        :loading="materialLoading"
        :selectedList="selectedMaterialList"
        selectedKey="itemCode">
        <template v-slot:replaceBtn="{ row }">
          <el-icon 
            class="el-icon-sort" 
            style="transform: rotate(90deg); cursor: pointer;" 
            @click.native.stop="openReplaceMaterialDialog(row)">
          </el-icon>
        </template>
      </common-table>
      <pagination
        v-show="materialCount > 0"
        :total="materialCount"
        :page.sync="listQueryMaterial.page"
        :limit.sync="listQueryMaterial.limit"
        @pagination="handleMaterialChange"
      />
      <template v-if="replacedList && replacedList.length">
        <el-divider></el-divider>
        <span style="margin-top: 10px;">替换物料</span>
        <common-table 
          style="margin-top: 10px;" 
          max-height="200px"
          :data="replacedList" 
          :columns="replacedColumns">
          <!-- <template v-slot:deleteBtn="{ index }">
            <el-icon style="cursor: pointer;" class="el-icon-delete" @click.native="deleteReplaceMaterial(index)"></el-icon> -->
          <!-- </template> -->
        </common-table>
      </template>
    </my-dialog>
    <!-- 替换的物料 -->
    <my-dialog
      ref="replaceDialog"
      title="选择替换的物料"
      width="760px"
      :btnList="replaceBtnList"
      :append-to-body="true"
      @closed="onReplaceClosed"
      :destroy-on-close="true"
    >
      <el-form :model="listQueryReplace" :inline="true" size="mini">
        <el-form-item label="物料编码">
          <el-input v-model.trim="listQueryReplace.partCode" @keyup.enter.native="_getAllMaterialList"></el-input>
        </el-form-item>
        <el-form-item label="物料描述">
          <el-input v-model.trim="listQueryReplace.partDescribe" @keyup.enter.native="_getAllMaterialList"></el-input>
        </el-form-item>
        <el-button size="mini" @click.native="_getAllMaterialList" type="primary">搜索</el-button>
      </el-form>
      <common-table
        v-loading="replaceLoading"
        style="margin-top: 10px;"
        ref="replaceTable"
        height="400px"
        :data="replaceList"
        radioKey="itemCode"
        :columns="replaceColumns">
      </common-table>
      <pagination
        v-show="replaceCount > 0"
        :total="replaceCount"
        :page.sync="listQueryReplace.page"
        :limit.sync="listQueryReplace.limit"
        @pagination="handleReplaceChange"
      />
    </my-dialog>
    <!-- 自定义新增物料弹窗 -->
    <my-dialog
      ref="replaceAddDialog"
      title="选择新增的物料"
      width="760px"
      :btnList="replaceBtnList"
      :append-to-body="true"
      @closed="onReplaceClosed"
      :destroy-on-close="true"
    >
      <el-form :model="listQueryReplace" :inline="true" size="mini">
        <el-form-item label="物料编码">
          <el-input v-model.trim="listQueryReplace.partCode" @keyup.enter.native="_getAllMaterialList"></el-input>
        </el-form-item>
        <el-form-item label="物料描述">
          <el-input v-model.trim="listQueryReplace.partDescribe" @keyup.enter.native="_getAllMaterialList"></el-input>
        </el-form-item>
        <el-button size="mini" @click.native="_getAllMaterialList" type="primary">搜索</el-button>
      </el-form>
      <common-table
        v-loading="replaceLoading"
        style="margin-top: 10px;"
        ref="replaceAddTable"
        height="400px"
        :data="replaceList"
        row-key="itemCode"
        :selectedList="selectedMaterialList"
        selectedKey="itemCode"
        :columns="replaceAddColumns">
      </common-table>
      <pagination
        v-show="replaceCount > 0"
        :total="replaceCount"
        :page.sync="listQueryReplace.page"
        :limit.sync="listQueryReplace.limit"
        @pagination="handleReplaceChange"
      />
    </my-dialog>
    
    <!-- 驳回理由弹窗 -->
    <my-dialog
      ref="remarkDialog"
      :title="rejectTitle"
      :append-to-body="true"
      top="250px"
      :btnList="remarkBtnList"
      @closed="onRemarkClose"
      v-loading="remarkLoading"
      width="350px">
      <remark ref="remark" @input="onRemarkInput" :tagList="reimburseTagList" :isShowTitle="false" :placeholder="rejectPlaceholder"></remark>
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
    <!-- 预览图片 -->
    <el-image-viewer
      :zIndex="99999"
      v-if="previewVisible"
      :url-list="previewImageUrlList"
      :on-close="closeViewer"
    >
    </el-image-viewer>
    <!-- <el-button @click="_checkFormData">点击校验</el-button> -->
  </div>
</template>

<script>
import { 
  getServiceOrderList,
  getSerialNumberList, 
  getMaterialList, 
  getAllMaterialList,
  getDetailsMaterial,
  AddQuotationOrder, 
  updateQuotationOrder,
  // getQuotationMaterialsCode,
  approveQuotationOrder
} from '@/api/material/quotation'
import Remark from '@/views/reimbursement/common/components/remark'
import UpLoadFile from '@/components/upLoadFile'
import zxform from "@/views/serve/callserve/form";
import zxchat from '@/views/serve/callserve/chat/index'
// import AreaSelector from '@/components/AreaSelector'
import { configMixin, quotationOrderMixin, categoryMixin, chatMixin, uploadFileMixin, rolesMixin } from '../js/mixins'
// import { timeToFormat } from "@/utils";
import { formatDate } from '@/utils/date'
import { findIndex, accAdd, accMul, accDiv } from '@/utils/process'
// import { toThousands } from '@/utils/format'
import { isNumber, isIntegerNumber } from '@/utils/validate'
import { flatten } from '@/utils/utils'
import rightImg from '@/assets/table/right.png'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import { s2HMS } from '@/filter/time'
const NOT_EDIT_STATUS_LIST = ['edit', 'upload', 'pay'] // 不可编辑的状态 1.查看 2.审批 3.支付
const CONFIRM_TYPE_MAP = {
  upload: '上传',
  pay: '收款'
}
const SUCCESS_TYPE_MAP = {
  upload: '提交',
  pay: '收款'
}
const PREPAY_LIST = ['prepay', 'paymentAfterWarranty', 'cashBeforeFelivery', 'payOnReceipt']
const NOT_MATERIAL_CODE_LIST = ['S111-SERVICE-CLF', 'S111-SERVICE-GSF'] // 不算入物料合计的 物料编码列表
function isInServiceOrTravel (materialCode) {
  return NOT_MATERIAL_CODE_LIST.indexOf(materialCode) > - 1
}
export default {
  inject: ['parentVm'],
  mixins: [configMixin, quotationOrderMixin, categoryMixin, chatMixin, uploadFileMixin, rolesMixin],
  components: {
    Remark,
    zxform,
    zxchat,
    UpLoadFile,
    ElImageViewer
    // AreaSelector
  },
  filters: {
    s2HMS,
    formatDateFilter (val) {
      return val ? formatDate(val, 'YYYY.MM.DD HH:mm:ss') : ''
    },
    // calcTotalItem (val, isProtected, key = 'totalPrice') { // 计算每一个物料表格的总金额
    //   console.log(val, isProtected, 'calcItem')
    //   return (isProtected 
    //     ? val.filter(item => {
    //         return isNumber(Number(item[key])) && isInServiceOrTravel(item.materialCode || '')
    //       }) 
    //       .reduce((prev, next) => accAdd(prev, next[key]), 0)
    //     : 
    //     val.filter(item => isNumber(Number(item[key])))
    //       .reduce((prev, next) => accAdd(prev, next[key]), 0)
    //   )

    // },
    calcTotalItem (val, key = 'totalPrice', isDeal = false) { // 计算每一个物料表格的总金额
     // isDeal 应付 只有购买的展示金额 更换的不展示金额
      return val.filter(item =>  {
        const { materialType } = item
        return isNumber(Number(item[key])) && (isDeal ? materialType === '2' : materialType !== '3')
      })
      .reduce((prev, next) => accAdd(prev, next[key]), 0)
    },
    calcTotalDealMoney (val) {
      let result = 0
      for (let i = 0; i < val.length; i++) {
        result += val[i].quotationMaterials.filter(item => {
          const { materialType } = item
          return (
            isNumber(Number(item.totalPrice)) && 
            (materialType === '2')
          )})
          .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
      }
      return result
    },
    calcSerialTotalMoney (serialNumber, list) {
      let index = findIndex(list, item => {
        return item.productCode === serialNumber
      })
      if (index !== -1) { // 保外的才算钱
        return list[index].
          quotationMaterials.filter(item => {
            const { totalPrice, materialType } = item
            return isNumber(Number(totalPrice)) && materialType !== '3'
          })
          .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
      }
      return 0
    }
  },
  props: {
    customerList: {
      type: Array,
      default () { () => [] }
    },
    detailInfo: { // 详情
      type: Object,
      default () { () => {} }
    },
    hasEditBtn: { // 用来区分报价单的 草稿、驳回等状态的查看和编辑
      type: Boolean
    },
    status: { // 判断当前报价单所处于的状态 view create edit approve pay
      type: String
    },
    isReceive: { // 用来区别报价单模块(true) 和 (销售订单模块/待处理物料模块 | false)
      type: Boolean
    },
    isSalesOrder: Boolean, // 在报价单新建编辑页面，判断显示销售订单还是报价单
    isSales: Boolean, // 用来区分销售订单和领料单
    categoryList: {
      type: Array,
      default: () => []
    }
  },
  watch: {
    customerList: {
      immediate: true,
      handler (val, oldVal) {
        console.log(val, oldVal, 'val')
        if (this.status === 'create') { // 新建时才需要去获取客户信息列表
          this._getServiceOrderList()
        }
      }
    },
    'formData.serviceOrderSapId': {
      immediate: true,
      handler (val, oldVal) {
        console.log(val, 'newVal')
        if (val !== oldVal && val) {
          // if (this.isReceive) {
          if (this.isReceive) {
            if (this.status === 'create') {
              this.isPreview = false
              this._resetMaterialInfo() // 新建的时候,清除所有的跟物料相关的数据
            }
            if ((this.status === 'edit' && this.hasEditBtn ) || this.status === 'create') {
              this.listQuerySearial.page = 1
              this.listQuerySearial.limit = 50
              this._getSerialNumberList()
            }
          }
          if (this.ifShowMergedTable) {
            this._normalizeMaterialSummaryList()
          }
        }
      }
    },
    detailInfo: {
      immediate: true,
      handler (val) {
        if (this.status !== 'create') {
          this.isPreview = true
          Object.assign(this.formData, val)
          // 设置服务费和差旅费
          const { serviceCharge, travelExpense, deliveryMethod } = this.formData
          this.serviceList.forEach((item, index) => {
            item.salesPrice = (index === 0) ? serviceCharge : travelExpense
          })
          if (+deliveryMethod === 3) {
            this.ifShowPrepaid = true
          }
          this.formData.quotationProducts = this.formData.quotationProducts.map(product => {
            product.quotationMaterials.forEach(material => {
              material.discount = String(Number(material.discount).toFixed(6)) // 保证discount是string类型，且跟字典对应上
            })
            return product
          })
          if (this.formData.quotationProducts.length) {
            const { materialCode, productCode } = this.formData.quotationProducts[0]
            this.currentSerialNumber = productCode
            this.listQueryReplace.manufacturerSerialNumber = productCode
            this.listQueryReplace.materialCode = materialCode
          }

          // 构建selected Map
          if (this.status === 'edit') {
            for (let i = 0; i < this.formData.quotationProducts.length; i++) {
              let item = this.formData.quotationProducts[i]
              this.selectedMap[item.productCode] = []
              const list = this._normalizeSelectMap(item.quotationMaterials)
              this.selectedMap[item.productCode] = list
            }
          }
        }
        // 如果有历史记录的话
        if (this.formData.quotationOperationHistorys && this.formData.quotationOperationHistorys.length) {
          this.timeList = this._normalizeTimeList(this.formData.quotationOperationHistorys)
        }
      }
    }
  },
  data () {
    // const SERVICE_CHARGE_VALIDATOR = function (rule, value, callback) { // 服务费校验规则
    //   value = Number(value)
    //   if (!isNaN(value) && value % 50 !== 0) {
    //     console.log('error')
    //     callback(new Error())
    //   } else {
    //     callback();
    //   }
    // }
    const DISCOUNT_VALIDATOR = function (rule, value, callback) {
      value = Number(value)
      console.log(value, 'value DISCOUNT_VALIDATOR')
      if (!isNumber(value) || value < 40) {
        // this.$message.error('折扣不能小于40')
        callback(new Error())
      } else {
        callback();
      }
    }
    return {
      ifShowPrepaid: false, // 付款条件为预付的选项
      timeList: [],
      // 合同图片
      previewImageUrlList: [], // 合同图片列表
      previewVisible: false,
      contentLoading: false, // 弹窗内容loading
      rightImg,
      // 表单数据
      formData: {
        salesOrderId: '', // 销售单号
        serviceOrderSapId: '', // NSAP ID
        serviceOrderId: '', 
        createUser: '',
        orgName: '',
        isMaterialType: '',
        prepay: undefined, // 预付百分比
        cashBeforeFelivery: undefined, // 发货前百分比
        payOnReceipt: undefined, // 货到验货百分比
        paymentAfterWarranty: undefined, // 质保后百分比
        serviceCharge: undefined, // 服务费
        travelExpense: undefined, // 差旅费
        terminalCustomer: '', // 客户名称
        terminalCustomerId: '', // 客户代码
        shippingAddress: '', // 开票地址
        collectionAddress: '', // 收款地址
        deliveryMethod: '', // 发货方式
        invoiceCompany: '', // 开票方式
        salesMan: '', // 销售员
        totalMoney: 0, // 总金额
        newestContactTel: '', // 电话
        newestContacter: '', // 联系人
        acceptancePeriod: 7, // 验收期限
        deliveryDate: '', // 交货日期
        moneyMeans: '', // 货币方式
        acquisitionWay: '', // 领料方式
        quotationProducts: [], // 报价单产品表
        remark: '',// 备注
        shippingDA: '',
        collectionDA: '',
        totalCostPrice: 0
      }, 
      // 表单规则
      formRules: {
        serviceOrderSapId: [{ required: true }],
        terminalCustomerId: [{ required: true }],
        terminalCustomer: [{ required: true }],
        newestContacter: [{ required: true }],
        newestContactTel: [{ required: true }],
        deliveryDate: [{ required: true, trigger: ['change', 'blur'] }],
        acceptancePeriod: [{ required: true, trigger: ['change', 'blur'] }],
        shippingAddress: [{ required: true, trigger: ['change', 'blur'] }],
        acquisitionWay: [{ required: true, trigger: ['change', 'blur'] }],
        moneyMeans: [{ required: true, trigger: ['change', 'blur'] }],
        // invoiceCompany: [{ required: true, trigger: ['change', 'blur'] }],
        deliveryMethod: [{ required: true, trigger: ['change', 'blur'] }],
        collectionAddress: [{ required: true, trigger: ['change', 'blur'] }],
        isMaterialType: [{ required: true, trigger: ['change', 'blur'] }]
      },
      serviceRules: {
        // serviceCharge: [{ validator: SERVICE_CHARGE_VALIDATOR, trigger: ['change', 'blur'] }],
      },
      // createTime: timeToFormat('yyyy-MM-dd HH:mm'),
      // 操作记录
      historyColumns: [
        { label: '#', type: 'index', width: 50 },
        { label: '操作记录', prop: 'action' },
        { label: '操作人', prop: 'createUser' },
        { label: '操作时间', prop: 'createTime', slotName: 'createTime' },
        { label: '审批时长', prop: 'intervalTime', slotName: 'intervalTime' },
        { label: '审批结果', prop: 'approvalResult' },
        { label: '备注', prop: 'remark' }
      ],
      // 差旅费和服务费用
      serviceList: [
        { materialCode: 'S111-SERVICE-GSF', materialDescription: '服务费的物料编码对应的描述', discount: '100.00%', salesPrice: undefined },
        { materialCode: 'S111-SERVICE-CLF', materialDescription: '差旅费的物料编码对应的描述', discount: '100.00%', salesPrice: undefined }
      ],
      serviceColumns: [
        { type: 'index', label: '#' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '数量' },
        { label: '最大数量' },
        { label: '当前库存' },
        { label: '仓库' },
        { label: '成本价', align: 'right' },
        { label: '销售价', slotName: 'salesPrice', align: 'right' },
        { label: '折扣(%)', align: 'right' },
        { label: '小计', slotName: 'totalPrice', align: 'right' },
      ],
      // 物料弹窗列表
      materialList: [],
      materialCount: 0,
      selectedMaterialList: [], // 已经选择了物料列表，再次弹窗时，不能再选
      selectedMap: {}, // 已经选择物料列表
      materialLoading: false,
      listQueryMaterial: {
        page: 1,
        limit: 20,
        partCode: '',
        partDescribe: ''
      },
      materialBtnList: [
        { btnText: '确定', handleClick: this.selectMaterial },
        { btnText: '取消', handleClick: this.closeMaterialDialog }
      ],
      materialColumns: [ 
        { type: 'selection' },
        { label: '物料编码', prop: 'itemCode', width: 100 },
        { label: '物料描述', prop: 'itemName', width: 150 },
        { label: '零件规格', prop: 'buyUnitMsr', width: 80, align: 'right' },
        { label: '库存量', prop: 'onHand', width: 100, align: 'right' },
        { label: '仓库号', prop: 'whsCode', width: 100, align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right' },
        { label: '销售价', prop: 'lastPurPrc', align: 'right' },
        { label: '替换', slotName: 'replaceBtn' }
      ],
      // 替换和被替换的物料的对比表格
      replacedList: [],
      replacedColumns: [
        { label: '#', type: 'index', width: 40 },
        { label: '物料编码', prop: 'itemCode', width: 100 },
        { label: '物料描述', prop: 'itemName' },
        { label: '零件规格', prop: 'buyUnitMsr', width: 80, align: 'right' },
        { label: '库存量', prop: 'onHand', width: 50, align: 'right' },
        { label: '仓库号', prop: 'whsCode', width: 100, align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right' },
        { label: '销售价', prop: 'lastPurPrc', align: 'right' },
        { label: '被替换的物料编码', prop: 'replaceMaterialCode' },
        // { label: '删除', slotName: 'deleteBtn' }
      ],
      // 替换物料表格
      replaceList: [],
      replaceCount: 0,
      replaceLoading: false,
      replaceColumns: [
        { type: 'radio', width: 40 },
        { label: '物料编码', prop: 'itemCode', width: 100 },
        { label: '物料描述', prop: 'itemName' },
        { label: '零件规格', prop: 'buyUnitMsr', width: 100, align: 'right' },
        { label: '库存量', prop: 'onHand', width: 100, align: 'right' },
        { label: '仓库号', prop: 'whsCode', width: 100, align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right' },
        { label: '销售价', prop: 'lastPurPrc', align: 'right' }
      ],
      replaceAddColumns: [
        { type: 'selection', width: 40 },
        { label: '物料编码', prop: 'itemCode', width: 100 },
        { label: '物料描述', prop: 'itemName' },
        { label: '零件规格', prop: 'buyUnitMsr', width: 100, align: 'right' },
        { label: '库存量', prop: 'onHand', width: 100, align: 'right' },
        { label: '仓库号', prop: 'whsCode', width: 100, align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right' },
        { label: '销售价', prop: 'lastPurPrc', align: 'right' }
      ],
      listQueryReplace: {
        partCode: '',
        partDescribe: '',
        page: 1,
        limit: 20
      },
      replaceBtnList: [
        { btnText: '确定', handleClick: this.selectReplace },
        { btnText: '取消', handleClick: this.closeReplaceDialog }
      ],
      // 根据设备序列号生成的物料表格
      // materialTypeOptions: [
      //   { label: '赠送', value: '1' },
      //   { label: '更换', value: '2' },
      //   { label: '购买', value: '3' },
      // ],
      // materialTypeMap: {
      //   1: '赠送',
      //   2: '更换',
      //   3: '购买'
      // },
      materialTypeList: [
        { label: '更换', value: true },
        { label: '购买', value: false }
      ],
      materialConfig:[
        { label: '序号', type: 'index' },
        { label: '类型', slotName: 'materialType' },
        { label: '物料编码', prop: 'materialCode', slotName: 'materialCode', 'show-overflow-tooltip': false },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '数量', prop: 'count', slotName: 'count', align: 'right' },
        { label: '最大数量', prop: 'maxQuantity', align: 'right', slotName: 'maxQuantity', 'show-overflow-tooltip': false },
        { label: '库存量', prop: 'warehouseQuantity', align: 'right' },
        { label: '仓库', prop: 'warehouseNumber', align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right', slotName: 'unitPrice', 'show-overflow-tooltip': false },
        { label: '推荐单价', prop: 'salesPrice', align: 'right', slotName: 'salesPrice', 'show-overflow-tooltip': false },
        { label: '折扣(%)', prop: 'discount', slotName: 'discount', align: 'right' },
        { label: '销售单价', prop: 'discountPrices', align: 'right', slotName: 'discountPrices', 'show-overflow-tooltip': false },
        { label: '小计', prop: 'totalPrice', disabled: true, align: 'right', slotName: 'totalPrice' },
        { label: '备注', slotName: 'remark', prop: 'remark' },
        { label: '操作', slotName: 'operation' }
      ],    
      // 物料填写表格列表 
      materialRules: {
        count: [{ required: true, trigger: ['change', 'blur'] }],
        materialType: [{ required: true, trigger: ['change', 'blur'] }],
        discount: [{ required: true, trigger: ['change', 'blur'], validator: DISCOUNT_VALIDATOR }],
        discountPrices: [{ required: true, trigger: ['change', 'blur'] }]
      },
      // 物料汇总表格
      materialSummaryList: [],
      materialAllColumns: [
        { label: '序号', type: 'index', width: 50 },
        { label: '物料编码', prop: 'materialCode', slotName: 'materialCode', width: 130, 'show-overflow-tooltip': false },
        { label: '物料描述', prop: 'materialDescription', width: 200 },
        { label: '数量', prop: 'count', align: 'right', width: 70 },
        // { label: '单价', prop: '', align: 'right' },
        { label: '单价', prop: 'salesPrice', align: 'right', width: 100 },
        { label: '折扣(%)', prop: 'discount', align: 'right', slotName: 'discount', width: 100, 'show-overflow-tooltip': false },
        { label: '总计', prop: 'totalPrice', align: 'right', width: 100 },
        { label: '成本价', prop: 'costPrice', align: 'right', width: 100 },
        { label: '总成本', prop: 'totalCost', align: 'right', width: 100 },
        { label: '总毛利', prop: 'margin', align: 'right', width: 100 },
        { label: '毛利%', prop: 'profit', align: 'right', width: 80 }
      ],
      materialAllLoading: false,
      // 编辑查看状态、工程、总经理报价单、销售订单审批
      approveColumns: [
        { label: '序号', type: 'index', width: 50 },
        { label: '类型', slotName: 'materialType', width: 50 },
        { label: '物料编码', width: 150, prop: 'materialCode', slotName: 'materialCode', 'show-overflow-tooltip': false },
        { label: '物料描述', prop: 'materialDescription', width: 200 },
        { label: '数量', prop: 'count', align: 'right', width: 70, slotName: 'count', 'show-overflow-tooltip': false },
        { label: '最大数量', prop: 'maxQuantity', align: 'right' },
        // { label: '库存量', prop: 'warehouseQuantity', align: 'right' },
        // { label: '仓库', prop: 'warehouseNumber', align: 'right' },
        { label: '成本价', prop: 'unitPrice', align: 'right', slotName: 'unitPrice', 'show-overflow-tooltip': false },
        { label: '推荐单价', prop: 'salesPrice', align: 'right', slotName: 'salesPrice', 'show-overflow-tooltip': false },
        { label: '销售单价', prop: 'discountPrices', align: 'right', slotName: 'discountPrices', 'show-overflow-tooltip': false },
        { label: '折扣(%)', prop: 'discount', slotName: 'discount', align: 'right', 'show-overflow-tooltip': false },
        { label: '小计', prop: 'totalPrice', slotName: 'totalPrice', align: 'right', 'show-overflow-tooltip': false },
        { label: '备注', prop: 'remark' }
      ],
      // 客户列表
      customerData: [],
      customerLoading: false,
      customerTotal: 0,
      listQueryCustomer: { // 客户列表分页参数
        page: 1,
        limit: 20
      },
      customerColumns: [
        { type: 'radio', width: '50px' },
        { label: '服务单号', prop: 'u_SAP_ID' },
        { label: '客户名称', prop: 'terminalCustomer' },
        { label: '客户代码', prop: 'terminalCustomerId' },
        { label: '销售员', prop: 'salesMan' }
      ],
      customerBtnList: [
        { btnText: '确定', handleClick: this.selectCustomer },
        { btnText: '取消', handleClick: this.closeCustomerDialog }
      ],
      // 序列号列表
      currentSerialNumber: '', // 当前选中的设备序列号
      serialNumberList: [],
      serialCount: 0,
      serialLoading: false,
      serialColumns: [
        { label: '制造商序列号', prop: 'manufacturerSerialNumber' },
        { label: '物料编码', prop: 'materialCode', slotName: 'materialCode', 'show-overflow-tooltip': false },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '保修到期', prop: 'docDate' },
        { label: '生产订单', prop: 'productionOrder' },
        { label: '销售订单', prop: 'salesOrder' },
        { label: '小计', slotName: 'money', align: 'right' },
        { label: '领取物料', slotName: 'btn' }
        // { label: '领取物料',  width: 100, type: 'operation',
        //   actions: [{ btnText: '领取', handleClick: this.openMaterialDialog }] 
        // }
      ],
      listQuerySearial: {
        page: 1,
        limit: 50,
        manufacturerSerialNumbers: '',
        materialCode: ''
      },
      // 预览数据
      isPreview: false, // 是否预览
      // 地址选择器
      isShowCollect: false,
      isShowShipping: false,
      // 驳回理由弹窗
      remark: '',
      remarkLoading: false,
      // 查询物料表格 一个序列号 -> 多个物料
      listQueryApprove: {
        manufacturerSerialNumbers: '',
        materialCode: ''
      },
      // 用来判断是打开替换物料弹窗还是新增物料弹窗
      isCustomAdd: false,
      cancelRequestCustom: null, // 用来取消用户信息列表请求
      cancelRequestMaterial: null, // 用来取消物料列表的请求，防止数紊乱
      cancelRequestSerialList: null, // 用来取消通过客户代码获取的设备序列号列表请求
      cancelRequestAllMaterial: null,
      cancelRequestApprove: null // 用来取消查询审批页面的物料表格
    }
  },
  computed: {
    materialTypeOptions () {
      return (typeof this.formData.isMaterialType !== 'boolean' && !this.formData.isMaterialType)
        ? []
        : this.formData.isMaterialType 
          ?[
              { label: '更换', value: '1' },
              { label: '赠送', value: '3' }
            ]
          : [
              { label: '购买', value: '2' },
              { label: '赠送', value: '3' },
            ]
    },
    materialTypeMap () {
      return (typeof this.formData.isMaterialType !== 'boolean' && !this.formData.isMaterialType)
        ? {}
        : this.formData.isMaterialType
          ? { 1: '更换', 3: '赠送' }
          : { 2: '购买', 3: '赠送'}
    },
    ifInApprovePage () { // 是否在审批页面
      return this.$route.path === '/materialcenter/salesorder/index' || this.$route.path === '/materialcenter/materialapprove/index'
    },
    ifShowMergedTable () {
      return ((this.$route.path === '/materialcenter/quotation/index' && this.isSalesOrder) || 
        (this.$route.path === '/materialcenter/salesorder/index' && this.isTechnical)) && this.isPreview
    },
    ifShowSerialTable () {
      return (
          (this.$route.path === '/materialcenter/quotation/index' && !this.isSalesOrder && !this.hasEditBtn) ||
          (this.$route.path === '/materialcenter/salesorder/index' && !this.isTechnical) ||
          (this.$route.path === '/materialcenter/materialapprove/index')
        ) && this.isPreview
    },
    ifShowHistory () {
      return this.status !== 'create'  && !this.hasEditBtn && this.isPreview
    },
    rejectPlaceholder () {
      return (this.status === 'approveSales' || this.status === 'pay') ? '待定理由' : '驳回理由'
    },
    rejectTitle () {
      return (this.status === 'approveSales' || this.status === 'pay') ? '待定' : '驳回'
    },
    ifNotEdit () {
      console.log(this.status, 'status')
      return NOT_EDIT_STATUS_LIST.includes(this.status)
    },
    totalMaterialMoney () {
      let value = 0
      if (this.formData.quotationProducts.length) {
        let val = this.formData.quotationProducts
        value = this._calcTotalMoney(val)
      } 
      return value
    },
    totalMoney () { // 报价单总金额
      const serviceOrTravelMoney = this.serviceList.reduce((prev, next) => {
        return accAdd(prev, (isNumber(next.salesPrice) ? next.salesPrice : 0))
      }, 0)
      let totalMoney = accAdd(this.totalMaterialMoney, serviceOrTravelMoney)
      return totalMoney
    },
    summaryTotalPrice () { // 汇总物料的销售总计
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.totalPrice)
      }, 0)
    },
    // salesPriceTotal () { // 汇总物料的销售价总计
    //   return this.materialSummaryList.reduce((prev, next) => {
    //     return accAdd(prev, next.salesPrice)
    //   }, 0)
    // },
    costPirceTotal () { // 汇总物料的成本价总计
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.totalCost)
      }, 0)
    },
    grossProfitTotal () { // 汇总物料的总毛利
      return this.materialSummaryList.reduce((prev, next) => {
        return accAdd(prev, next.margin)
      }, 0)
    },
    grossProfit () { // 毛利率
      if (this.grossProfitTotal === 0 || this.summaryTotalPrice === 0) { // 总毛利为零时， 毛利率为零
        return 0 
      }
      return (this.grossProfitTotal / this.summaryTotalPrice) * 100
    },
    materialData () {
      // 找到当前选择的设备序列号对应的物料列表数据
      let index = findIndex(this.formData.quotationProducts, item => {
        return item.productCode === this.currentSerialNumber
      })
      let { quotationMaterials = [], isProtected } = index !== -1 ? this.formData.quotationProducts[index] : {}
      return  { list: quotationMaterials , isProtected }
    },
    remarkBtnList () {
      return [
        { btnText: '确认', handleClick: this.approveByReject },
        { btnText: '取消', handleClick: this.closeRemark }
      ]
    }
  },
  methods: {
    isInServiceOrTravel,
    onFormMaterialTypeChange (val) {
      console.log(val, typeof val, 'onFormMaterialTypeChange')
      this.clearAllMaterialType()
      this.serviceList.forEach(item => item.salesPrice = undefined)
    },
    clearAllMaterialType () {
      this.formData.quotationProducts.forEach(item => {
        const { quotationMaterials } = item
        quotationMaterials.forEach(item => item.materialType = '')
      })
    },
    onMaterialTypeChange (row) {
      if (row.materialType === '3') {
        row.totalPrice = 0
      } else {
        const { count, discountPrices } = row
        // row.totalPrice = Number(((count * salesPrice * accDiv(discount, 100) || 0)).toFixed(2))
        row.totalPrice = Number(accMul(count, discountPrices)).toFixed(2)
      }
      console.log(row, 'row change')
    },
    _normalizeTimeList (timeList) {
      timeList = timeList.filter(item => item.approvalResult !== '暂定') // 把暂定的状态排除掉
      const { quotationStatus, totalMoney } = this.formData
      const HAS_TOTAL_MONEY_MAP = { // 有服务费的
        4: -1,
        5: -2,
        6: -3,
        7: -4,
        8: -5,
        9: -6,
        10: -7,
        11: -8
      }
      const NO_TOTAL_MONEY_MAP = { // 没服务费的
        4: -1,
        5: -2,
        9: -3,
        10: -4,
        11: -5
      }
      const maxLen = Number(totalMoney) ? 9 : 6
      const HAS_TOTAL_MONEY_TEXT = ['报价单提交审批', '工程审批', '总经理审批', '客户确认报价', '销售订单成立', '财务审批', '总经理审批', '待出库', '出库']
      const NO_TOTAL_MONEY_TEXT = ['报价单提交审批', '工程审批', '总经理审批', '总经理审批', '待出库', '出库']
      const TEXT_MAP = totalMoney ? HAS_TOTAL_MONEY_TEXT : NO_TOTAL_MONEY_TEXT
      const TOTAL_MONEY_MAP = totalMoney ? HAS_TOTAL_MONEY_MAP : NO_TOTAL_MONEY_MAP
      if (TOTAL_MONEY_MAP[quotationStatus]) {
        const length = TOTAL_MONEY_MAP[quotationStatus]
        timeList = this.setFinishedStatus(timeList.slice(length))
      } else {
        timeList = []
      }
      let isCurrent = !!timeList.length
      while (timeList.length < maxLen) {
        timeList.push({
          isFinished: false,
          isCurrent
        })
        isCurrent = false
      }
      timeList.forEach((item, index) => {
        item.text = TEXT_MAP[index]
      })
      return timeList
    },
    setFinishedStatus (list) { // 设置状态
      return list.map(item => {
        item.isFinished = true // 完成(审批完的状态)
        return item
      })
    },
    processStatusIcon (item) { // 处理时间进度条icon
      // iconfont icon-big-circle 阿里巴巴图标
      return item.isFinished 
        ? 'big el-icon-upload-success el-icon-circle-check success' 
        : item.isCurrent
          ? 'iconfont icon-big-circle warning'
          : 'not-current'
    },
    isIntegerNumber,
    onDeliveryMethodChange (val) { // 表单付款条件发生变化
      console.log(val, 'deliveryMethodChange')
      const isPrepare = Number(val) === 3
      this.ifShowPrepaid = isPrepare
      if (!isPrepare) {
        this.formData.prepay = undefined
        this.formData.cashBeforeFelivery = undefined
        this.formData.payOnReceipt = undefined
        this.formData.paymentAfterWarranty = undefined
      }
    },
    customAddMaterial () { // 自定义新增物料
      this.isCustomAdd = true
      this.selectedMaterialList = this.selectedMap[this.currentSerialNumber] || []
      this.$nextTick(() => {
        this.$refs.replaceAddDialog.open()
      })
    },
    async _getSerialDetail () {
      if (this.cancelRequestApprove) {
        this.cancelRequestApprove('customer abort')
      }
      this.serialLoading = true
      try {
        const res = await getDetailsMaterial({
          ...this.listQueryApprove,
          quotationId: this.formData.id
        }, this)
        this.serialLoading = false
        this.formData.quotationProducts = res.data
        console.log(res.data, this.formData.quotationProducts)
      } catch (err) {
        if (err.message !== 'customer abort') {
          this.serialLoading = false
          this.$message.error(err.message)
        }
      }
    },
    closeViewer () { // 关闭合同图片
      this.previewVisible = false
    },
    showContract () { // 展示合同图片
      if (!this.formData.quotationPictures.length) {
        return this.$message.warning('暂无合同片')
      }
      this.previewVisible = true
      this.previewImageUrlList = this.formData.quotationPictures 
    },
    getFileList (val) {
      console.log(val)
      let ids = val.map(item => item.pictureId)
      this.pictureIds = ids
    },
    // 计算总金额
    // _calcTotalMoney (val) {
    //   let result = 0
    //   for (let i = 0; i < val.length; i++) {
    //     if (!val[i].isProtected) {
    //       result += val[i].quotationMaterials.filter(item => {
    //         const { materialCode } = item
    //         console.log(item.totalPrice, 'no isProtected')
    //         return isNumber(Number(item.totalPrice)) && NOT_MATERIAL_CODE_LIST.indexOf(materialCode) === -1
    //       })
    //       .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
    //     } else if (val[i].isProtected && this.isGeneralManager) {
    //       result += val[i].quotationMaterials.filter(item => {
    //         const { materialCode } = item
    //         console.log(item.totalPrice, 'isProtected')
    //         return isNumber(Number(item.totalPrice)) && NOT_MATERIAL_CODE_LIST.indexOf(materialCode) === -1
    //       })
    //       .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
    //     }
    //   }
    //   return result
    // },
    _calcTotalMoney (val) {
      let result = 0
      for (let i = 0; i < val.length; i++) {
        result += val[i].quotationMaterials.filter(item => {
          const { materialCode, materialType } = item
          return (
            isNumber(Number(item.totalPrice)) && 
            (NOT_MATERIAL_CODE_LIST.indexOf(materialCode) === -1 &&
            materialType !== '3')
          )})
          .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
        // if (!val[i].isProtected) {
        //   result += val[i].quotationMaterials.filter(item => {
        //     const { materialCode, materialType } = item
        //     console.log(item.totalPrice, 'no isProtected')
        //     return isNumber(Number(item.totalPrice)) && NOT_MATERIAL_CODE_LIST.indexOf(materialCode) === -1
        //   })
        //   .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
        // } else if (val[i].isProtected && this.isGeneralManager) {
        //   result += val[i].quotationMaterials.filter(item => {
        //     const { materialCode } = item
        //     console.log(item.totalPrice, 'isProtected')
        //     return isNumber(Number(item.totalPrice)) && NOT_MATERIAL_CODE_LIST.indexOf(materialCode) === -1
        //   })
        //   .reduce((prev, next) => accAdd(prev, next.totalPrice), 0)
        // }
      }
      return result
    },
    onSerialRowClick (val) { // 点击序列号表格 展示对应的物料表格
      this.currentSerialNumber = val.manufacturerSerialNumber
      this.currentSerialInfo = val
      this.listQueryReplace.manufacturerSerialNumber = val.manufacturerSerialNumber
      this.listQueryReplace.materialCode = val.materialCode
    },
    onServiceIdFocus () {
      if (this.status === 'create') {
        this.$refs.customerDialog.open()
      }
    },
    _openServiceOrder () { // 打开服务单
      this.openServiceOrder(this.formData.serviceOrderId, () => this.contentLoading = true, () => this.contentLoading = false)
    },
    selectCustomer () { // 选择客户数据
      let val = this.$refs.customerTable.getCurrentRow()
      if (!val) {
        return this.$message.warning('请先选择数据')
      }
      let { 
        terminalCustomer, terminalCustomerId, salesMan, id, u_SAP_ID, billingAddress, 
        deliveryAddress, frozenFor, balance, newestContacter, newestContactTel } = val
      this.formData.terminalCustomer = terminalCustomer
      this.formData.terminalCustomerId = terminalCustomerId
      this.formData.salesMan = salesMan
      this.formData.serviceOrderId = id
      this.formData.serviceOrderSapId = u_SAP_ID
      this.formData.shippingAddress = billingAddress
      this.formData.collectionAddress = deliveryAddress
      this.formData.frozenFor = frozenFor
      this.formData.balance = balance
      this.formData.newestContacter = newestContacter
      this.formData.newestContactTel =newestContactTel
      this.closeCustomerDialog()
    },
    closeCustomerDialog () {
      this.$refs.customerTable.resetCurrentRow()
      this.$refs.customerTable.resetRadio() // 清空单选
      this.$refs.customerDialog.close()
    },
    customerCurrentChange ({ page, limit }) {
      this.listQueryCustomer.page = page
      this.listQueryCustomer.limit = limit
      this._getServiceOrderList()
    },
    openReplaceMaterialDialog (row) { // 替换当前物料
      this.$refs.materialTable.setCurrentRow(row)
      const selectionList = this.$refs.materialTable.getSelectionList()
      const selectedList = flatten(this.selectedMaterialList || [])
      const index = findIndex([...selectionList, ...selectedList], item => {
        return item.itemCode === row.itemCode
      })
      console.log(selectionList, selectedList, index, 'selected')
      if (index > -1) {
        return this.$message.warning('当前物料已选中，不可进行替换操作')
      }
      this.currentReplaced = row // 当前被替换的元素
      this.isCustomAdd = false
      this.$nextTick(() => {
        this.$refs.replaceDialog.open()
      })
      
    },
    deleteReplaceMaterial (index) {
      this.replacedList.splice(index, 1)
    },
    selectReplace () {
      if (this.isCustomAdd) {
        let list = this.$refs.replaceAddTable.getSelectionList()
        if (list && !list.length) {
          return this.$message.warning('请先选择新增物料')
        }
        list = list.map(item => {
          item.newMaterialCode = true
          return item
        })
        this._mergeSelectedList(list)
        this.closeReplaceDialog()
      } else {
        const currentRow = this.$refs.replaceTable.getCurrentRow()
        if (!currentRow) {
          return this.$message.warning('请先选择替换物料')
        }
        const selectedList = flatten(this.selectedMaterialList)
        if (selectedList.some(item => item.itemCode === currentRow.itemCode)) {
          return this.$message.warning('不能选择重复的物料编码!!')
        }
        // 将跟当前选中的物料编码中对应选项禁选
        const selectionList = this.$refs.materialTable.getSelectionList()
        const matchRow = selectionList.find(item => item.itemCode === currentRow.itemCode)
        matchRow && this.$refs.materialTable.toggleRowSelection(matchRow, false)
        const { itemCode, quantity } = this.currentReplaced
        const newSelectedList = JSON.parse(JSON.stringify(this.selectedMaterialList))
        newSelectedList.push([{ itemCode }, { itemCode: currentRow.itemCode }])
        this.selectedMaterialList = newSelectedList
        this.replacedList.push({ ...currentRow, replaceMaterialCode: itemCode, quantity })
        this.closeReplaceDialog()
      }
    },
    handleReplaceChange ({ page, limit }) {
      this.listQueryReplace.page = page
      this.listQueryReplace.limit = limit
      this._getAllMaterialList()
    },
    closeReplaceDialog () {
      this.isCustomAdd ? this.$refs.replaceAddDialog.close() : this.$refs.replaceDialog.close()
    },
    onReplaceClosed () {
      this.replaceList = []
      this.replaceCount = 0
      this.listQueryReplace.partCode = ''
      this.listQueryReplace.partDescribe = ''
      this.listQueryReplace.page = 1
      this.listQueryReplace.limit = 20
    },
    _getAllMaterialList () { // 获取当前服务单下所有设备的所有物料
      if (this.cancelRequestAllMaterial) {
        this.cancelRequestAllMaterial('customer abort')
      }
      const { partCode, partDescribe } = this.listQueryReplace
      const itemCode = this.isCustomAdd ? '' : this.currentReplaced.itemCode
      if (!partCode && !partDescribe) {
        return this.$message.warning('请输入物料编码或物料描述')
      }
      this.replaceLoading = true
      getAllMaterialList({ ...this.listQueryReplace, replacePartCode: itemCode }, this).then(res => {
        let { count, data } = res
        this.replaceList = data
        this.replaceCount = count
        this.replaceLoading = false
      }).catch(err => {
        if (err.message !== 'customer abort') {
          this.replaceList = []
          this.creplaceCount = 0
          this.replaceLoading = false
          this.$message.error(err.message)
        }
      }).finally(() => {
        const { replaceTable } = this.$refs
        replaceTable && replaceTable.resetRadio()
        replaceTable && replaceTable.resetCurrentRow()
      })
    },
    _getServiceOrderList () { // 获取用户信息列表
      if (this.cancelRequestCustom) {
        this.cancelRequestCustom('customer abort')
      }
      this.customerLoading = true
      getServiceOrderList(this.listQueryCustomer, this).then(res => {
        let { count, data } = res
        this.customerData = data
        this.customerTotal = count
        this.customerLoading = false
      }).catch(err => {
        if (err.message !== 'customer abort') {
          this.customerData = []
          this.customerTotal = 0
          this.customerLoading = false
          this.$message.error(err.message)
        }
      })
    },
    _getSerialNumberList () { // 获取设备序列号列表
      if (!this.formData.serviceOrderSapId) {
        return this.$message.warning('请先选择服务单!')
      }
      if (this.cancelRequestSerialList) {
        this.cancelRequestSerialList('customer abort')
      }
      this.serialLoading = true
      getSerialNumberList({
        serviceOrderId: this.formData.serviceOrderId,
        ...this.listQuerySearial
      }, this).then(res => {
        let { data, count } = res
        this.serialNumberList = data
        this.serialCount = count
        this.serialLoading = false
        console.log(res, 'res')
      }).catch(err => {
        if (err.message !== 'customer abort') {
          this.serialNumberList = []
          this.serialCount = 0
          this.serialLoading = false
          this.$message.error(err.message)
        }
      })
    },
    _getMaterialList () { // 获取物料列表
      if (this.cancelRequestMaterial) {
        this.cancelRequestMaterial('customer abort')
      }
      const { manufacturerSerialNumber, materialCode } = this.currentSerialInfo
      this.materialLoading = true
      getMaterialList({
        ManufacturerSerialNumbers: manufacturerSerialNumber,
        materialCode,
        ...this.listQueryMaterial
      }, this).then(res => {
        console.log(res, 'res')
        let { data, count } = res
        this.materialList = data
        this.materialCount = count
        this.materialLoading = false
      }).catch(err => {
        if (err.message !== 'customer abort') {
          this.materialList = []
          this.materialCount = 0
          this.materialLoading = false
          this.$message.error(err.message)
        }
      })
    },
    handleMaterialChange ({ page, limit }) {
      this.listQueryMaterial.page = page
      this.listQueryMaterial.limit = limit
      this._getMaterialList()
    },
    openMaterialDialog (val) { // 添加物料
      this.isCustomAdd = false
      let { manufacturerSerialNumber } = val
      if (this.currentSerialNumber !== manufacturerSerialNumber) {
        this.listQueryMaterial.page = 1
        this.listQueryMaterial.limit = 20
        this.currentSerialNumber = manufacturerSerialNumber
      }
      this.currentSerialInfo = val
      this._getMaterialList()
      console.log(this.selectedMap[this.currentSerialNumber], 'opendialog')
      this.selectedMaterialList = this.selectedMap[this.currentSerialNumber] || []
      this.$refs.materialDialog.open()
    }, 
    deleteMaterialItem (index) { // 删除物料
      console.log('delete', index)
      // 执行删除操作时
      // 删除表格数据
      this.materialData.list.splice(index, 1)
      this.selectedMap[this.currentSerialNumber].splice(index, 1)
      if (!this.materialData.list.length) {
        // deleteList
        this.formData.quotationProducts.splice(this.materialIndex, 1)
        delete this.selectedMap[this.currentSerialNumber]
      }
    },
    _normalizeMaterialSummaryList () {
      this.materialSummaryList = this.formData.quotationMergeMaterials.map((item, index) => {
        let { count, costPrice } = item
        item.index = index
        item.discount = String(Number(item.discount).toFixed(6))
        // item.grossProfit = salesPrice - costPrice // 总毛利
        item.totalCost = accMul(costPrice, count) // 总成本
        return item
      })
    },
    onDiscountPricesChange (row) {
      const { discountPrices, salesPrice } = row
      if (discountPrices && salesPrice) {
        row.discount = (accDiv(discountPrices, salesPrice) * 100).toFixed(6)
        if (row.discount < 40) {
          this.$message.warning('折扣不能小于40')
        }
      }
    },
    onDiscountFocus (index) {
      this.materialItemIndex = index
      console.log(this.materilItemIndex, 'discount focus')
    },
    onCountFocus (index) {
      this.materialItemIndex = index // 当前点击的物料表格的第几项
    },
    onCountChange (val) {
      let { list } = this.materialData
      let data = list[this.materialItemIndex]
      // let { salesPrice, discount, materialType, discountPrices } = data
      const { discountPrices, materialType } = data
      const isMoney = materialType === '3'
      data.totalPrice = Number((isMoney ? 0: accMul(val, discountPrices) || 0).toFixed(2))
    },
    _resetMaterialInfo () { // 重置物料相关的变量和数据
      this.formData.quotationProducts = []
      this.selectedMap = {}
      this.selectedMaterialList = []
      this.currentSerialNumber = ''
      this.listQueryMaterial = {
        page: 1,
        limit: 20,
        partCode: '',
        partDescribe: ''
      }
    },
    selectMaterial () { // 选择弹窗物料
      let selectedList = this.$refs.materialTable.getSelectionList()
      const mergedList = [...this.replacedList || [], ...selectedList]
      if (!mergedList.length) {
        return this.$message.warning('请先选择零件')
      }
      this.closeMaterialDialog()
      this._mergeSelectedList(mergedList)
    }, 
    _normalizeSelectedList (selectedList) { // 格式化物料表格数据
      return selectedList.map(selectItem => {
        let item = {}
        // let { isProtected } = this.currentSerialInfo
        let { itemCode, itemName, onHand, quantity,  buyUnitMsr, whsCode, unitPrice, lastPurPrc, replaceMaterialCode, newMaterialCode } = selectItem
        item.unit = buyUnitMsr
        item.materialType = ''
        item.materialDescription = itemName
        item.materialCode = itemCode
        item.remark = ''
        item.unitPrice = unitPrice
        item.salesPrice = lastPurPrc
        item.discount = Number(100).toFixed(6)
        item.discountPrices = item.salesPrice * 1
        item.count = 1
        item.warehouseQuantity = onHand
        item.warehouseNumber = whsCode
        item.maxQuantity = quantity
        // item.maxQuantityText = Math.ceil(quantity)
        item.totalPrice = Number(accMul(item.salesPrice, item.count)).toFixed(2)
        console.log(item.totalPrice, 'totalPrice')
        item.replaceMaterialCode = replaceMaterialCode
        item.newMaterialCode = !!newMaterialCode
        return item
      })
    },
    _normalizeSelectMap (list) { 
      const result = []
      list.forEach(item => {
        const { materialCode, replaceMaterialCode } = item
        replaceMaterialCode 
          ? result.push([{ itemCode: materialCode}, { itemCode: replaceMaterialCode }])
          : result.push({ itemCode: materialCode})
      })
      console.log(result, 'result')
      return result
    },
    _mergeSelectedList (selectedList) { // 整合所有的物料表格数据
      let index = findIndex(this.formData.quotationProducts, item => {
        return item.productCode === this.currentSerialNumber
      })
      let materialList = this._normalizeSelectedList(selectedList)
      // 如果数组中已经存在了这个对象值 则需要将之前选择的跟现在选择的进行合并
      if (index > -1) {
        this.formData.quotationProducts[index].quotationMaterials.push(...materialList)
        this.selectedMap[this.currentSerialNumber].push(...this._normalizeSelectMap(materialList))
      } else {
        this.formData.quotationProducts.push({
          ...this.currentSerialInfo,
          warrantyExpirationTime: this.currentSerialInfo.docDate,
          productCode: this.currentSerialNumber,
          quotationId: this.formData.id,
          quotationMaterials: materialList
        })
        this.selectedMap[this.currentSerialNumber] = []
        this.selectedMap[this.currentSerialNumber].push(...this._normalizeSelectMap(materialList))
      }
      this.materialIndex = index > -1 ? index : this.formData.quotationProducts.length - 1 // 设置当前物料表格对应quotationProducts第几项数据
    },
    closeMaterialDialog () { // 关闭弹窗
      this.$refs.materialDialog.close()
      this.replacedList = []
      this.listQueryMaterial = {
        page: 1,
        limit: 20,
        partCode: '',
        partDescribe: ''
      }
    },
    resetInfo () { // 每次关闭弹窗
      // 预览数据
      this.reset()
      if (this.$refs.uploadFile) {
        this.$refs.uploadFile.clearFiles()
      }
      console.log(this.formData, this.formData.serviceOrderSapId)
      this.$nextTick(() => {
        this.$refs.form.clearValidate()
      })
    },
    togglePreview () { // 预览
      if (!this.formData.quotationProducts.length) {
        return this.$message.warning('请先选择零件')
      }
      this.isPreview = !this.isPreview
    },
    onAreaFocus (prop) {
      this.onCloseArea()
      if (prop === 'shippingAddress') {
        this.isShowShipping = true 
      } else {
        this.isShowCollect = true
      }
    },  
    onCloseArea () { // 关闭地址选择器
      this.isShowCollect = false
      this.isShowShipping = false
    },
    onAreaChange (val) { // 选择地址完毕
      let { province, city, district, prop } = val
      const countryList = ['北京市', '天津市', '上海市', '重庆市']
      let result = ''
      result = countryList.includes(province)
        ? province + district
        : city + district
      console.log(prop, 'prop')
      this.formData[prop] = result
      this.onCloseArea()
    },
    onRemarkInput (value) {
      this.remark = value
    },
    closeRemark () {
      this.remark = ''
      this.$refs.remark.reset()
      this.$refs.remarkDialog.close()
    },
    onRemarkClose () {
      this.remark = ''
      this.$refs.remark.reset()
    },
    async _checkFormData () {
      let isFormValid = false
      try {
        isFormValid = await this.$refs.form.validate()
        console.log(isFormValid, 'isFormValid')
      } catch (err) {
        console.log(isFormValid, 'isFormValid')
      }
      return isFormValid
    },
    checkPrepay () {
      const totalPercent = PREPAY_LIST.reduce((prev, key) => {
        const value = Number(this.formData[key])
        console.log(value, 'checkPrepay')
        return accAdd(prev, (isNumber(value) ? value : 0))
      }, 0)
      console.log(totalPercent, 'totalPercent')
      return totalPercent === 100
    },
    async _operateOrder (isDraft) { // 提交 存稿 针对于报价单
      // 判断表头表单
      console.log(this.formData.quotationProducts)
      let isFormValid = await this._checkFormData()
      console.log(isFormValid, 'isFormValid')
      if (!isFormValid) {
        return Promise.reject({ message: '表单未填写完成或格式错误' })
      }
      let isPrepayValid = true
      if (+this.formData.deliveryMethod === 3) { // 如果付款方式是预付
        isPrepayValid = this.checkPrepay()
      }
      if (!isPrepayValid) {
        return Promise.reject({ message: '预付选项相加必须等于100' })
      }
      // 判断物料列表
      // 只有服务费
      // 有物料零件没有服务费
      // 有零件有服务费
      if (!this.totalMoney && !this.formData.quotationProducts.length) {
        return Promise.reject({ message: '服务费或物料零件数量不能为空' })
      }
      if (this.formData.quotationProducts.length) {
        let isValid = true
        try {
          await this.$refs.materialForm.validate()
        } catch (err) {
          isValid = false
          console.log(err)
        }
        if (!isValid) {
          return Promise.reject({ message: '当前序列号下的物料数量、类型、销售单价不能为空，折扣需大于等于40' })
        }
        console.log(this.formData.quotationProducts.length, length)
        for (let i = 0; i < this.formData.quotationProducts.length; i++) {
          const item = this.formData.quotationProducts[i]
          const { productCode, quotationMaterials } = item
          for (let j = 0; j < quotationMaterials.length; j++) {
            const { materialType, count, discount, discountPrices } = quotationMaterials[j]
            if (!materialType || !count || (discount < 40) || !discountPrices) {
              return Promise.reject({ message: `${productCode}设备序列号下第${j + 1}行物料数量、类型、销售单价不能为空，折扣需大于等于40`})
            }
          }
        }
      }
      this.serviceList.forEach((item, index) => {
        if (index === 0) {
          this.formData.serviceCharge = item.salesPrice
        } else {
          this.formData.travelExpense = item.salesPrice
        }
      })
      console.log(this.formData, 'formData')
      console.log(updateQuotationOrder, AddQuotationOrder, isDraft)
      return this.status !== 'create'
        ? updateQuotationOrder({
            ...this.formData,
            isDraft,
            totalMoney: this.totalMoney
          })
        : AddQuotationOrder({
            ...this.formData,
            isDraft,
            createTime: formatDate(new Date(), 'YYYY.MM.DD HH:mm:ss'),
            totalMoney: this.totalMoney,
            createUser: this.formData.createUser || this.createUser
          })
    },
    async beforeApprove (type) { // 待处理的报价单 审批
      this.$refs.form.validate(isValid => {
        if (isValid) {
          console.log(this.pictureIds)
          if (type === 'reject') { // 驳回
            this.$refs.remarkDialog.open() 
          } else {
            if (type === 'upload') { // 技术员回传，一定要上传图片
              if (!this.pictureIds || (this.pictureIds && !this.pictureIds.length)) {
                return this.$message.warning('必须上传附件')
              }
            }
            const text = `确定${CONFIRM_TYPE_MAP[type] || '通过'}`
            this.$confirm(text + '?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
              }).then(() => {
                this._approve(type)
              })
            }
          } else {
            this.$message.error('请将必填项填写')
          }
      })
    },
    approveByReject () {
      if (!this.remark) {
        return this.$message.warning(`${this.rejectPlaceholder}必填`)
      }
      this._approve('reject')
    },
    async _approve (type) { // 审核
      let isFormValid = await this._checkFormData()
      if (!isFormValid) {
        return Promise.reject({ message: '表单未填写完成或格式错误' })
      }
      const isDetermined = this.status === 'approveSales' || this.status === 'pay'
      let params = {
        id: this.formData.id,
        isReject: (isDetermined ? false : type === 'reject'),
        pictureIds: (this.pictureIds && this.pictureIds.length) ? this.pictureIds : undefined,
        remark: this.remark,
        IsTentative: (isDetermined && type === 'reject')
      }
      type === 'reject'
        ? this.remarkLoading = true
        : this.contentLoading = true
      approveQuotationOrder(params).then(() => {
        this.$message({
          type: 'success',
          message: type === 'reject' 
            ? (isDetermined ? '待定' : '驳回') + '成功' 
            : `${SUCCESS_TYPE_MAP[type] || '审批'}成功`
        })
        this.parentVm._getList()
        this.parentVm.handleClose()
        if (type === 'reject') {
          this.remarkLoading = false
          this.closeRemark()
        } else {
          this.contentLoading = false
          this.parentVm.handleClose()
        }
      }).catch(() => {
        type === 'reject'
          ? this.remarkLoading = false
          : this.contentLoading = false
        this.$message.error('操作失败')
      })
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.quotation-wrapper {
  position: relative;
  font-size: 12px;
  .divider {
    height: 1px;
    margin: 15px auto;
    background: #E6E6E6;
  }
  .success {
    font-size: 14px;
    color: rgba(0, 128, 0, 1);
  }
  .warning {
    font-size: 14px;
    color: rgba(255, 165, 0, 1);
  }
  /* 表头文案 */
  > .title-wrapper { 
    position: absolute;
    top: -51px;
    left: 150px;
    height: 40px;
    line-height: 40px;
    p { 
      margin-right: 10px;
      font-size: 12px;
      span:nth-child(1) {
        color: #BFBFBF;
        margin-right: 10px;
      }
      span:nth-child(2) {
        color: #222222;
      }
    }
  }
  /* 外层滚动 */
  .scroll-bar {
    &.el-scrollbar {
       ::v-deep {
        .el-scrollbar__wrap {
          max-height: 600px; // 最大高度
          overflow-x: hidden; // 隐藏横向滚动栏
          margin-bottom: 0 !important;
        }
      }
    }
    /* 表单内容 */
    .my-form-wrapper {
      flex: 1;
      .area-wrapper {
        position: relative;
        .selector-wrapper {
          position: absolute;
          left: 0;
          top: 27px;
          z-index: 10;
        }
      }
      ::v-deep {
        .el-date-editor.el-input {
          input {
            padding: 0 5px !important;
          }
          span {
            display: none;
          }
        }
        .el-input-number input {
          text-align: left;
        }
      }
    }
    /* 预付 */
    .prepay-wrapper {
      &.if-not-edit {
        margin-right: 290px;
        span {
          margin-right: 10px;
        }
      }
      &::v-deep {
        div {
          height: 20px;
        }
        .el-input-number {
          width: 100px;
          height: 20px;
          input {
            display: block;
            width: 100px;
            height: 20px;
            text-align: right;
            font-size: 12px;
          }
        }
      }
    }
    /* 物料顺序展示列表 */
    .approve-class {
      .approve-search-wrapper {
        
      }
      .has-icon {
        max-width: calc(100% - 12px); 
        padding-right: 3px;
      }
      .info-wrapper {
        margin-top: 10px;
        font-weight: bold;
        & > div {
          margin-left: 10px;
          .title {
            margin-right: 20px;
            color: #d0d0d0;
            font-weight: normal;
          }
        }
      }
      .serial-table-list {
        overflow: hidden;
        margin-top: 10px;
        .serial-item {
          margin-top: 20px;
          &:nth-child(1) {
            margin-top: 0;
          }
          .info-title {
            margin-bottom: 5px;
            & > div {
              &:nth-child(1) {
                margin-left: 0;
              }
              margin-left: 10px;
              color: #cbcbcb;
              & > span {
                margin-left: 5px;
                color: #000;
              }
            }
          }
          .total-line {
            margin-right: 270px;
            text-align: right;
            font-weight: bold;
            color: #000;
            & > p {
              width: 86px;
            }
          }
        }
      }
    }
    /* 回传附件样式 */
    .upload-file-wrapper { 
      .title-text {
        width: 60px;
        padding-right: 4px;
        text-align: right;
        color: #C0C4CC;
      }
    }
    /* 序列号查询表格 */
    .serial-wrapper {
      .form-wrapper {
        
      }
      .serial-table-wrapper {
        margin-top: 10px;
        .has-icon {
          max-width: calc(100% - 12px); 
          padding-right: 3px;
        }
        .money-line {
          margin: 10px 0;
          text-align: right;
          ::v-deep {
            .el-input-number input {
              text-align: right;
            }
          }
          span {
            margin-right: 20px;
            &.title {
              color: #d0d0d0;
            }
          }
        }
      }
    }
    /* 物料填写表格 */
    .material-wrapper {
      .form-wrapper {
        ::v-deep .el-input-number {
          width: 100% !important;
        }
        .material-table-wrapper {
          overflow: hidden;
          white-space: nowrap;
          .has-icon {
            max-width: calc(100% - 12px); 
            padding-right: 3px;
          }
          .notice-icon {
            color: rgb(248, 181, 0);
          }
        }
        .icon-item {
          cursor: pointer;
        }
      }
      .subtotal-wrapper {
        margin-top: 10px;
        text-align: right;
        span {
          margin-right: 20px;
          &.title {
            color: #d0d0d0;
          }
        }
      }
    }
    /* 服务费和差旅费 */
    .service-travel-wrapper {
      .title {
        margin-right: 20px;
        color: #d0d0d0;
        font-weight: normal;
      }
    }
    /* 物料汇总表格 */
    .material-summary-wrapper {
      width: 1132px;
      margin-top: 10px;
      .material-summary-table {
        height: auto !important;
        ::v-deep .el-table__footer-wrapper {
          font-weight: bold;
        }
        .has-icon {
          max-width: calc(100% - 12px); 
          padding-right: 3px;
        }
      }
      /* 物料汇总金额样式 */
      .money-wrapper { 
        font-size: 12px;
        font-weight: bold;
        p {
          width: 100px;
          text-align: right;
          &:nth-child(1) {
            margin-right: 100px;
          }
          &:nth-child(4) {
            width: 80px;
          }
        }
      }
    }
    /* 操作记录表格 */
    .history-wrapper {
      margin-top: 10px;
    }
    .timeline-progress-wrapper {
      position: relative;
      width: 700px;
      height: 5px;
      margin: 10px 0 30px 40px;
      background-color: rgba(206, 206, 206, 1);
      .content-wrapper {
        position: absolute;
        z-index: 2;
        left: 0;
        right: 0;
        bottom: 0;
        top: 0;
        .content-item {
          position: relative;
          .icon-status {
            display: inline-block;
            font-size: 12px;
            background-color: #fff;
            border-radius: 50%;
            &.big {
              font-size: 13px;
            }
            &.not-current {
              position: relative;
              top: 1px;
              width: 12px;
              height: 12px;
              background-color: rgba(206, 206, 206, 1);
            }
          }
          .text {
            position: absolute;
            left: 0;
            top: 17px;
            font-size: 12px;
            white-space: nowrap;
            color: #000;
            transform: translate3d(-36%, 0, 0);
          }
          // background-color: #fff;
        }
      }
    }
  }
}
</style>
