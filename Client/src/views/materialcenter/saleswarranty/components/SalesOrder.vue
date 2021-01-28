<template>
  <div class="salerorder-detail-wrapper">
    <common-form
      class="form-wrapper"
      ref="form"
      :model="formData" 
      label-width="80px" 
      :formItems="formItems" 
      :isCustomerEnd="true"
      :rowStyle="rowStyle"
      :cellStyle="cellStyle"
    > 
      <template v-slot:id="{ model }">
        <span class="title">客户代码</span>
        <span class="text">{{ model.customerId }}</span>
      </template>
      <template v-slot:name="{ model }">
        <span class="title">客户名称</span>
        <span class="text">{{ model.customerName }}</span>
      </template>
      <template v-slot:deliveryDate="{ model }">
        <span class="title">交货日期</span>
        <span class="text">{{ model.deliveryDate }}</span>
      </template>
      <template v-slot:remark="{ model }">
        <span class="title">备注</span>
        <span class="text">{{ model.remark }}</span>
      </template>
    </common-form>
    <!-- 历史操作记录 -->
    <template>
      <div class="history-wrapper">
        <common-table 
          style="width: 502px;"
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
export default {
  props: {
    currentRow: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  watch: {
    currentRow: {
      immediate: true,
      handler () {
        this.formData = JSON.parse(JSON.stringify(this.currentRow))
        this.historyTableData = this.formData.salesOrderWarrantyDateRecords || []
      }
    }
  },
  computed: {
    formItems () {
      return [
        { slotName: 'id', attrs: { prop: 'customerId' }, itemAttrs: { label: '客户代码' } },
        { slotName: 'name', attrs: { prop: 'customerName' }, itemAttrs: { label: '客户名称' } },
        { slotName: 'deliveryDate', attrs: { prop: 'deliveryDate' }, itemAttrs: { label: '交货日期' }, isEnd: true },
        { slotName: 'remark', attrs: { prop: 'remark' }, itemAttrs: { label: '备注' }, isEnd: true },
        { tag: 'date', attrs: { prop: 'warrantyPeriod', 'value-format': 'yyyy-MM-dd', 'picker-options': { disabledDate: this.disabledDate } }, 
          itemAttrs: { label: '保修时间', prop: 'warrantyPeriod', rules: [{ required: true }] }, isEnd: true 
        }
      ]
    }
  },
  data () {
    return {
      formData: {},
      // 操作记录
      historyTableData: [],
      historyColumns: [
        { label: '#', type: 'index', width: 50 },
        { label: '操作记录', prop: 'action', width: 200 },
        { label: '操作人', prop: 'createUser', width: 100 },
        { label: '操作时间', prop: 'createTime', width: 150 }
      ]
    }
  },
  methods: {
    setFormData (currentRow) {
      this.formData = JSON.parse(JSON.stringify(currentRow))
      this.historyTableData = this.formData.salesOrderWarrantyDateRecords || []
    },
    disabledDate (date) {
      if (this.formData.warrantyPeriod) {
        return date.getTime() < new Date(this.formData.deliveryDate).getTime()
      }
      return true
    },
    resetInfo () {
      this.reset()
      this.$nextTick(() => {
        this.$refs.form.clearValidate()
        this.$refs.form.resetFields()
      })
    },
    rowStyle (rowArray, rowIndex) {
      console.log(rowArray, rowIndex, 'rowStyle')
      return {
        marginBottom: '15px'
      }
    },
    cellStyle (col, colIndex) {
      console.log(col, colIndex, 'colStyle')
      return {
        marginRight: '5px',
        width: 'auto'
      }
    },
    updateWarrantyDate (callback) {
      this.$refs.form.validate(isValid => {
        if (!isValid) {
          return this.$message.error('请将必填项填写完成')
        }
        let { id, warrantyPeriod } = this.formData
        // 修改
        callback({
          id,
          warrantyPeriod
        })
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
.salerorder-detail-wrapper {
  position: relative;
  .head-title-wrapper {
    position: absolute;
    top: -41px;
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
  /* 表单样式 */
  .form-wrapper {
    margin-bottom: 20px;
    .title {
      box-sizing: border-box;
      display: inline-block;
      width: 80px;
      padding-right: 12px;
      text-align: right;
      color: rgba(0, 0, 0, .5);
    }
  }
}
</style>