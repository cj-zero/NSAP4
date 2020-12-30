<template>
  <div class="salerorder-detail-wrapper">
    <!-- 标题 -->
    <div class="head-title-wrapper">
      <el-row type="flex" align="middle">
        <p class="bold">销售单号：<span>26262</span></p>
        <p>销售员：<span>遍历群</span></p>
      </el-row>
    </div>
    <!-- form表单 -->
    <el-form
          :model="formData"
          ref="form"
          class="my-form-wrapper"
          :class="{ 'uneditable': status === 'view' }"
          :disabled="status === 'view'"
          label-width="100px"
          size="mini"
          label-position="left"
          :show-message="false"
        >
          <!-- 普通控件 -->
          <el-row 
            type="flex" 
            v-for="(config, index) in formConfig"
            :key="index">
            <el-col 
              :span="item.col"
              v-for="item in config"
              :key="item.prop"
            >
              <el-form-item
                :prop="item.prop"
                :label="item.label"
                :rules="{ required: true }">
                <template v-if="!item.type">
                  <el-input v-model="formData[item.prop]" :disabled="item.disabled"></el-input>
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
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
    <!-- 历史操作记录 -->
    <template v-if="status === 'view'">
      <div class="history-wrapper">
        <common-table 
          ref="historyTable" 
          max-height="300px"
          :data="historyTableData" 
          :columns="historyColumns">
        </common-table>
      </div>
    </template>
  </div>
</template>

<script>
import { normalizeFormConfig } from '@/utils/format'
export default {
  props: {
    status: String
  },
  computed: {
    formConfig () {
      let config  = [
        { label: '客户代码', prop: 'terminalCustomerId', col: 8, disabled: true },
        { label: '客户名称', prop: 'terminalCustomer', col: 16, disabled: true, isEnd: true },
        { label: '备注', prop: 'remark', placeholder: '请输入', col: 24, disabled: true, isEnd: true },
        { label: '交货日期', prop: 'deliveryDate', placeholder: '请输入内容', col: 8, type: 'date', disabled: this.status === 'view'  },
        { label: '保修时间日期', prop: 'warrntyDate', placeholder: '请输入内容', col: 8, type: 'date', disabled: this.status === 'view'  },
      ]
      return normalizeFormConfig(config)
    }
  },
  data () {
    return {
      formData: {
        terminalCustomer: '',
        terminalCustomerId: '',
        remark: '',
        deliveryDate: '',
        warrntyDate: ''
      },
      // 操作记录
      historyTableData: [],
      historyColumns: [
        { label: '操作记录', width: 200 },
        { label: '操作人', width: 100 },
        { label: '操作时间', width: 100 }
      ]
    }
  },
  methods: {
    resetInfo () {
      this.formData = {
        terminalCustomer: '',
        terminalCustomerId: '',
        remark: '',
        deliveryDate: '',
        warrntyDate: ''
      }
      this.$refs.form.clearValidate()
      this.$refs.form.resetFields()
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.salerorder-detail-wrapper {
  position: relative;
  .head-title-wrapper {
    position: absolute;
    top: -35px;
    left: 90px;
    font-size: 12px;
    p {
      min-width: 60px;
      margin-right: 10px;
      &.bold {
        color: red;
        font-weight: bold;
      }
    }
  }
}
</style>