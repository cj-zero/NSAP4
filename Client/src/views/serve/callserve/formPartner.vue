<template>
<div>
    <el-table
    :data="partnerList"
    border
    ref="singleTable"
   highlight-current-row
    @current-change="getPartner"
    @row-dblclick="handleCurrentChange"
    style="width: 100%"
  >
    <!-- <el-table-column>
      <template slot-scope='scope'>
              <el-radio v-model="radio" fixed :label="scope.row.cardCode" @click="checkOne(scope.row)"></el-radio>
      </template>
    </el-table-column> -->
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

       <el-dialog title="最近呼叫ID" width="90%"  @open="openDialog" :visible.sync="dialogCallId">
<callId ></callId>
      <!-- <callId :toCallList="CallList"></callId> -->
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogCallId = false">取 消</el-button>
        <el-button type="primary" @click="dialogCallId = false">确 定</el-button>
      </span>
    </el-dialog>
</div>

</template>

<script>
import callId from "./callId";
export default {
  props: ["partnerList" ,'count' ],
    components: { callId},

  data() {
    return {
       currentRow: [] ,//选择项
       dialogPartner:'',
       dialogCallId:false,
         listQuery: {
        // 查询条件
        page: 1,
        radio:'1',
        CallList:[],
        toCallList:[],
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
      // console.log(this.partnerList)
  },
    watch: {
    partnerList:function(a){
         console.log(a);
    }
  },
  methods:{
          openDialog() {   //打开前赋值给近期服务单

      // this.CallList = this.partnerList
    },
    // checkOne(value){
    //   console.log(value)
    // },

    getPartner(val){
      this.$emit('getChildValue',val)
    },
   handleCurrentChange(val) {
        this.currentRow = val;
        if(val.frozenFor=='Y'){
              this.$message({
              message: `${val.cardName}账户被冻结，无法操作`,
              type: 'error'
            })
        }else{
             this.$message({
              message: `抱歉，${val.cardName}没有呼叫ID数据`,
              type: 'warning'
            })
//  this.dialogCallId=true
        }
        
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