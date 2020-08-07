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
    >
      <template v-if="ifEdit">
        <el-table-column width="50" fixed>
          <template slot-scope="scope">
            <el-radio v-model="radio" :label="scope.row.manufSN">{{&nbsp;}}</el-radio>
          </template>
        </el-table-column>
      </template>
      <template v-else>
        <el-table-column type="selection" fixed width="55"></el-table-column>
      </template>
      <el-table-column prop="manufSN" fixed align="center" label="制造商序列号" width="120"></el-table-column>
      <el-table-column prop="internalSN" label="内部序列号" align="center" width="120">
        <template slot-scope="scope">
          <span>{{scope.row.internalSN?scope.row.internalSN:'暂无数据'}}</span>
        </template>
      </el-table-column>
      <el-table-column align="center" label="客户代码" width="120">
        <template slot-scope="scope">
          <span>{{scope.row.customer}}</span>
        </template>
      </el-table-column>
      <el-table-column prop="custmrName" label="客户名称" align="center" min-width="100"></el-table-column>
      <el-table-column prop="dlvryDate" align="center" label="保修结束时间" width="160"></el-table-column>
      <el-table-column align="center" prop="itemCode" width="200" label="物料编码"></el-table-column>
      <el-table-column align="center" label="物料描述" min-width="120">
        <template slot-scope="scope">
          <span>{{scope.row.itemName}}</span>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
export default {
  props: ["SerialNumberList", "serLoading", "ifEdit"],
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
      },
    };
  },
  compunted: {
    SerialNumberList: {
      get: function (a) {
        console.log(a);
      },
      set: function (a) {
        console.log(a);
      },
    },
  },
  watch: {
    //     SerialNumberList: function(){
    // }
  },

  mounted() {
    //   console.log(this.SerialNumberList)
  },
  methods: {
    //  handleChange(val) {
    //   this.listQuery.page = val.page;
    //   this.listQuery.limit = val.limit;
    //   // this.getList();
    // },
    handleSelectionChange(val) {      
        // this.$refs.singleTable.toggleRowSelection(val)
      this.$emit("change-Form", val);
    },
    onRowClick (row) {
      if (!this.ifEdit) {
        const { index } = row
        // 创建多选
        this.$refs.singleTable.toggleRowSelection(this.SerialNumberList[index])
      }
    },
    getCurrent (val) {
      if (this.ifEdit) {
        this.$refs.singleTable.clearSelection();
        this.radio = val.manufSN;
        this.$refs.singleTable.toggleRowSelection(val);
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
</style>