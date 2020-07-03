<template>
<div>
    <el-table
    :data="SerialNumberList"
    v-if="SerialNumberList.length>0"
    border
    ref="singleTable"
   highlight-current-row
 @selection-change="handleSelectionChange"
     style="width: 100%"
  >
      <el-table-column
      type="selection"
      fixed
      width="55">
    </el-table-column>
    <el-table-column prop="manufSN" fixed align='center' label="制造商序列号" width="120"></el-table-column>
    <el-table-column prop="internalSN" label="内部序列号" align="center" width="120">
          <template slot-scope="scope">
        <span >{{scope.row.internalSN?scope.row.internalSN:'暂无数据'}}</span>
      </template>
    </el-table-column>
        <el-table-column align="center" label="客户代码" width="120">
      <template slot-scope="scope">
        <span >{{scope.row.customer}}</span>
      </template>
    </el-table-column>
    <el-table-column prop="custmrName" label="客户名称" align="center" min-width="100"></el-table-column>
    <el-table-column prop="dlvryDate" align="center" label="保修结束时间" width="160"></el-table-column>
    <el-table-column align="center" prop="itemCode" width="200" label="物料编码"></el-table-column>
    <el-table-column align="center" label="物料描述" min-width="120">
      <template slot-scope="scope">
        <span >{{scope.row.itemName}}</span>
      </template>
    </el-table-column>
   
  </el-table>
        <pagination
          v-show="SerialNumberList.length>0"
          :total="SerialNumberList.length"
          :page.sync="listQuery.page"
          :limit.sync="listQuery.limit"
          @pagination="handleChange"
        />
</div>

</template>

<script>
import Pagination from "@/components/Pagination";

export default {
  props: ["SerialNumberList" ],
    components: {  Pagination},

  data() {
    return {
       currentRow: [] ,//选择项
       dataList:[],
         listQuery: {
        // 查询条件
        page: 1,
        limit: 10,
        key: undefined,
        appId: undefined
      },
    };
  },
  compunted: {
    SerialNumberList: {
      get: function(a) {
        console.log(a);
      },
      set: function(a) {
        console.log(a);
      }
    }
  },
    mounted() {
    //   console.log(this.SerialNumberList)
  },
  methods:{
     handleChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      // this.getList();
    },
   handleSelectionChange(val) {
      this.$emit("change-Form",val)
      }

}
}
</script>

<style lang="scss" scope>
.redColor {
  color: red;
}
.greenColro {
  color: green;
}
</style>