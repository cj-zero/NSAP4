<template>
  <div class="quotation-wrapper">
    <div class="title-wrapper">
      <p>报价单号: <span></span></p>
      <p>申请人: <span></span></p>
      <p>创建时间: <span></span></p>
    </div>
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
    <!-- 物料添加 -->
    <div class="material-wrapper">
      <el-button class="add-btn" @click="addMaterial">添加</el-button>
      <el-form 
          ref="materialForm" 
          :model="formData" 
          size="mini" 
          :show-message="false"
          class="form-wrapper"
          :disabled="!ifFormEdit"
          :class="{ 'uneditable': !this.ifFormEdit }"
        >
          <el-table 
            :row-style="rowStyle"
            border
            :data="formData.reimburseOtherCharges"
            max-height="250px"
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
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      :key="iconItem.icon"
                      :class="iconItem.icon" 
                      class="icon-item"
                      @click="iconItem.handleClick(scope, formData.reimburseOtherCharges, iconItem.operationType)">
                    </i>
                  </template>
                </template>
              </template>
            </el-table-column>
          </el-table>
        </el-form>
    </div>
    <!-- 操作记录 -->
    <div class="record-wrapper">
      <!-- <el-table 
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
            {{ scope.row.intervalTime | timeFormat }}
          </template>
        </el-table-column>
        <el-table-column label="审批结果" prop="approvalResult" width="150px" show-overflow-tooltip></el-table-column>
        <el-table-column label="备注" prop="remark" width="147px" show-overflow-tooltip></el-table-column>
      </el-table> -->
      <common-table :data="recordData" :columns="recordColumns"></common-table>
    </div>
    <my-dialog 
      ref="materialDialog"
      title="物料编码"
      :btnList="materialBtnList"
    >
      <common-table ref="materialSelectTable" :data="materialTableData"></common-table>
    </my-dialog>
  </div>
</template>

<script>
import CommonTable from '@/components/CommonTable' // 对于不可编辑的表格
import MyDialog from '@/components/Dialog'
export default {
  components: {
    CommonTable,
    MyDialog
  },
  data () {
    return {
      recordColumns: [],
      recordData: [],
      materialBtnList: [
        { btnText: '确定', handleClick: this.selectMaterialItem },
        { btnText: '取消', handleClick: this.closeMaterialDialog }
      ]
    }
  },
  methods: {
    addMaterial () {}, // 添加物料
    deleteMaterialItem () {}, // 删除物料
    selectMaterialItem () {}, // 选择弹窗物料
    materialDialog () { // 关闭弹窗
      this.$refs.materialSelectTable.clearSelection()
      this.$refs.materialDialog.close()
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
  > .title-wrapper {
    position: absolute;
    top: 0;
    left: 0;
    height: 40px;
    line-height: 40px;
    > p {
      min-width: 50px;
    }
  }
}
</style>