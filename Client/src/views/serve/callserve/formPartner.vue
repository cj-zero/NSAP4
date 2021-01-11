<template>
  <div>
    <common-table
      :data="partnerList"
      height="500"
      :columns="columns"
      :loading="parLoading"
      ref="singleTable"
      @current-change="getPartner"
      radioKey="cardCode"
    >
      <template v-slot:freezen="{ row }">
        <span :class="[row.frozenFor=='N'?'greenColro':'redColor']">{{row.frozenFor=="N"?"正常":'冻结'}}</span>
      </template>
      <template v-slot:balance="{ row }">
         <span :class="[row.balance>=0?'':'redColor']">{{row.balance}}</span>
      </template>
      <template v-slot:phone="{ row }">
        {{ row.cellular ? row.cellular : row.phone1 }}
      </template>
    </common-table>
    <!-- <el-table
      :data="partnerList"
      border
      height="500"
      v-loading="parLoading"
      ref="singleTable"
      highlight-current-row
      @current-change="getPartner"
      style="width: 100%"
      align="left"
    >
      <el-table-column width="50">
        <template slot-scope="scope">
          <el-radio v-model="radio" :label="scope.row.cardCode">{{&nbsp;}}</el-radio>
        </template>
      </el-table-column>
      <el-table-column prop="cardCode" label="客户代码" width="80"></el-table-column>
      <el-table-column prop="cardName" label="客户名称" align="left" min-width="120"></el-table-column>
      <el-table-column align="left" label="状态冻结" width="120">
        <template slot-scope="scope">
          <span
            :class="[scope.row.frozenFor=='N'?'greenColro':'redColor']"
          >{{scope.row.frozenFor=="N"?"正常":'冻结'}}</span>
        </template>
      </el-table-column>
      <el-table-column prop="cntctPrsn" label="联系人" align="left" width="100"></el-table-column>
      <el-table-column label="电话号码" align="left" width="110">
        <template slot-scope="scope">
          {{ scope.row.cellular ? scope.row.cellular : scope.row.phone1 }}
        </template>
      </el-table-column>
      <el-table-column prop="slpName" align="left" label="销售员" width="100"></el-table-column>
      <el-table-column prop="technician" label="售后主管" align="left"></el-table-column>
      <el-table-column align="left" prop="currency" width="100" label="货币种类"></el-table-column>
      <el-table-column align="right" label="科目余额" width="120">
        <template slot-scope="scope">
          <span :class="[scope.row.balance>=0?'':'redColor']">{{scope.row.balance}}</span>
        </template>
      </el-table-column>
      <el-table-column prop="address" align="left" label="开票地址" min-width="180"></el-table-column>
      <el-table-column prop="address2" label="收货地址" min-width="180"></el-table-column>
      <el-table-column prop="u_FPLB" align="left" width="120px" label="发票类别"></el-table-column>
    </el-table> -->

    <!-- <el-dialog :modal-append-to-body='false'
          :append-to-body="true" title="最近服务单情况" width="90%" @open="openDialog" :visible.sync="dialogCallId">
      <callId :toCallList="CallList"></callId>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogCallId = false">取 消</el-button>
        <el-button type="primary" @click="dialogCallId = false">确 定</el-button>
      </span>
    </el-dialog> -->
  </div>
</template>

<script>
// import * as callformPartner from "@/api/serve/callformPartner";

// import callId from "./callId";
export default {
  props: ["partnerList", "count",'parLoading'],
  // components: { callId },

  data() {
    return {
      columns: [
        { type: 'radio', width: 50 },
        { prop: 'cardCode', label: '客户代码', width: 80 },
        { prop: 'cardName', label: '客户名称', width: 140 },
        { label: '状态冻结', width: 80, slotName: 'freezen' },
        { prop: 'cntctPrsn', label: '联系人', width: 80 },
        { label: '电话号码', slotName: 'phone', width: 110 },        
        { prop: 'slpName', label: '销售员', width: 100 },
        { prop: 'technician', label: '售后主管' },
        { prop: 'currency', label: '货币种类', width: 100 },
        { label: '科目余额', width: 120, slotName: 'balance', align: 'right' },
        { prop: 'address', label: '开票地址', 'min-width': 180 },
        { prop: 'address2', label: '收货地址', 'min-width': 180 },
        { prop: 'u_FPLB', label: '发票类别', width: 120 }
      ],
      currentRow: [], //选择项
      dialogPartner: "",
      dialogCallId: false,
      radio: "",
      CallList: [],
      listQuery: {
        // 查询条件
        page: 1,
        limit: 40,
        key: undefined,
        appId: undefined
      },
      newList: [] //传递给最近呼叫或者未关闭的数据列表
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
  mounted() {},
  // watch: {
  // partnerList:function(){
  // }
  // },
  methods: {
    openDialog() {
      //打开前赋值给近期服务单
    },

    getPartner(val) {
      if (!val) {
        return
      }
      if (val.frozenFor&&val.frozenFor == "Y") {
      this.$message({
        message: `${val.cardName}账户被冻结，无法操作`,
        type: "error"
      });
        this.radio = val.cardCode;
        this.$emit("getChildValue", 1);
      } else  {
        this.checkVal = val;
        this.$emit("getChildValue", val);
        this.$refs.singleTable.clearSelection();
        this.radio = val.cardCode;
        this.$refs.singleTable.toggleRowSelection(val);
      }
    },
    handleCurrentChange(val) {
      this.currentRow = val;
      // this.$emit('currentChange', val)
      // if (val.frozenFor == "Y") {
      //   this.$message({
      //     message: `${val.cardName}账户被冻结，无法操作`,
      //     type: "error"
      //   });
      // } else {
      //    callformPartner.getTableList({ code: val.cardCode }).then(res => {
      //     this.CallList = res.result;
      //       this.dialogCallId = true
      //     // this.newestNotCloseOrder=res.reault.newestNotCloseOrder
      //     // this.newestOrder=res.reault.newestNotCloseOrder
      //   });
      // }
    }
  }
};
</script>

<style lang="scss" scope>
.redColor {
  color: red;
}
.greenColro {
  color: green;
}
</style>