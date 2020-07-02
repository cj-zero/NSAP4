<template>
<div>
    <el-table
    :data="partnerList"
    border
    ref="singleTable"
   highlight-current-row
    @current-change="handleCurrentChange"
    style="width: 100%"
  >
    <el-table-column prop="cardCode" fixed label="客户代码" width="80"></el-table-column>
    <el-table-column prop="cardName" label="客户名称" align="center" min-width="120"></el-table-column>
        <el-table-column align="center" label="状态冻结" width="120">
      <template slot-scope="scope">
        <span :class="[scope.row.frozenFor=='N'?'greenColro':'redColor']">{{scope.row.frozenFor=="N"?"正常":'冻结'}}</span>
      </template>
    </el-table-column>
    <el-table-column prop="cntctPrsn" label="联系人" align="center" width="100"></el-table-column>
    <el-table-column prop="slpName" align="center" label="销售员" width="100"></el-table-column>
    <el-table-column align="center" prop="currency" width="100" label="货币种类"></el-table-column>
    <el-table-column align="center" label="科目余额" width="120">
      <template slot-scope="scope">
        <span :class="[scope.row.balance>=0?'redColor':'greenColro']">{{scope.row.balance}}0000</span>
      </template>
    </el-table-column>
    <el-table-column prop="address" align="center" label="开票地址" min-width="180"></el-table-column>
    <el-table-column prop="address2" label="收货地址" min-width="180"></el-table-column>
    <el-table-column prop="u_FPLB" align="center" width="120px" label="发票类别"></el-table-column>
  </el-table>
        <pagination
          v-show="partnerList.length>0"
          :total="partnerList.length"
          :page.sync="listQuery.page"
          :limit.sync="listQuery.limit"
          @pagination="handleChange"
        />
</div>

</template>

<script>
import Pagination from "@/components/Pagination";

export default {
  props: ["partnerList" ],
    components: {  Pagination},

  data() {
    return {
       currentRow: [] ,//选择项
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
    partnerList: {
      get: function(a) {
        console.log(a);
      },
      set: function(a) {
        console.log(a);
      }
    }
  },
    mounted() {
    console.log(this.partnerList);
  },
  methods:{
     handleChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      // this.getList();
    },
   handleCurrentChange(val) {
        this.currentRow = val;
        console.log(val)
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