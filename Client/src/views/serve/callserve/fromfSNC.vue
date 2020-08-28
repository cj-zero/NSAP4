<template>
  <div>
    <el-table
      :data="SerialNumberList"
      border
      max-height="500"
      v-loading="serLoading"
      ref="singleTable"
      highlight-current-row
      @row-click="onRowClick"
      :row-class-name="tableRowClassName"
      @current-change="getCurrent"
      empty-text="抱歉，找不到该客户代码所属的制造商序列号"
      @selection-change="handleSelectionChange"
      style="width: 100%"
      row-key="manufSN"
      align="left"
    >
      <el-table-column  width="50" fixed>
        <template slot-scope="scope">
          <el-radio v-model="radio" :label="scope.row.manufSN">{{&nbsp;}}</el-radio>
        </template>
      </el-table-column>
      <el-table-column prop="manufSN" fixed label="制造商序列号" width="120"></el-table-column>
      <el-table-column prop="itemCode" width="120" label="物料编码"></el-table-column>
      <el-table-column label="物料描述" width="400">
        <template slot-scope="scope">
          <span>{{scope.row.itemName}}</span>
        </template>
      </el-table-column>
      <!-- <el-table-column prop="internalSN" label="内部序列号" width="120"> -->
        <!-- <template slot-scope="scope">
          <span>{{scope.row.internalSN?scope.row.internalSN:'暂无数据'}}</span>
        </template>
      </el-table-column> -->
      <el-table-column label="客户代码" width="100">
        <template slot-scope="scope">
          <span>{{scope.row.customer}}</span>
        </template>
      </el-table-column>
      <el-table-column prop="custmrName" label="客户名称" width="200"></el-table-column>
      <el-table-column prop="dlvryDate" label="保修结束时间" width="160"></el-table-column>
    </el-table>
    <!-- {{ dialogChange }} -->
  </div>
</template>

<script>
export default {
  props: {  
    SerialNumberList: {
      type: Array,
      default: function () {
        return []
      }
    },
    serLoading: {
      type: Boolean,
      default: false
    },
    ifEdit: {
      type: Boolean,
      default: false
    },
    formList: {
      type: Array,
      default: function () {
        return []
      }
    },
    visible: {
      type: Boolean,
      defualt: false // 弹窗是否打开
    },
    currentTarget: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data() {
    return {
      currentRow: [], //选择项
      radio: '',
      dataList: [],
      listQuery: {
        // 查询条件
        page: 1,
        limit: 10,
        key: undefined,
        appId: undefined,
      }
    };
  },
  watch: {
    visible () {
      // if (val)
      let { manufacturerSerialNumber } = this.currentTarget
      manufacturerSerialNumber ? 
        (this.radio = manufacturerSerialNumber) : 
        (this.radio = '')
    }
  },
  computed: {
  },
  mounted() {
    //   console.log(this.SerialNumberList)
  },
  methods: {
    checkIsSelectAble (row) {
      return this.formList.length ? 
        this.formList.every(formItem => formItem.manufacturerSerialNumber !== row.manufSN) :
        true
    },
    checkSelectable (row) {
      let isSelectAble = this.checkIsSelectAble(row)
      row.isSelectAble = isSelectAble
      return isSelectAble
    },
    handleSelectionChange(val) {
      this.$emit("change-Form", val);
    },
    onRowClick (row) {
      // console.log(row, 'rowClick')
      if (!this.ifEdit && row.isSelectAble) {
        const { index } = row
        // 创建多选
        this.$refs.singleTable.toggleRowSelection(this.SerialNumberList[index])
      }
    },
    getCurrent (val) {
      if (this.ifEdit) {
        this.radio = val.manufSN;
        if (!this.checkIsSelectAble(val) && val.manufSN !== this.currentTarget.manufacturerSerialNumber) {
          // if (val.manufSN !== this.currentTarget.manufacturerSerialNumber) {
            this.$message.error('重复项不可选,请重新选择')
            this.$emit('toggleDisabledClick', true)
            return
          // }
        } else {
          // console.log('false')
          this.$emit('singleSelect', val)
          this.$emit('toggleDisabledClick', false)
        } 
      }
    },
    tableRowClassName ({ row, rowIndex }) {
      // 把每一行的index加到row中
      row.index = rowIndex
    }
  },
};
</script>

<style lang="scss" scope>
.redColor {
  color: red;
}
.greenColro {
  color: green;
}
.el-form /deep/ .el-collapse-item__header {
  color: green;
}
.my-hidden {
  display: none;
}
</style>