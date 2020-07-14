<template>
  <div>
    <el-table
      ref="mainTable"
      :key="key"
      :data="list"
      v-loading="listLoading"
      border
      fit
      highlight-current-row
      style="width: 100%;"
      @row-click="rowClick"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" fixed align="center" width="55"></el-table-column>
      <el-table-column fixed align="center" label="服务ID" width="100">
        <template slot-scope="scope">
          <el-link type="primary" @click="openTree">{{scope.row.serveid}}</el-link>
        </template>
      </el-table-column>

      <el-table-column
        show-overflow-tooltip
        v-for="fruit in defaultFormThead"
        align="center"
        :key="fruit"
        style="background-color:silver;"
        :label="headLabel[fruit]"
      >
        <template slot-scope="scope">
          <span
            v-if="fruit === 'status'"
            :class="[scope.row[fruit]===1?'greenWord':(scope.row[fruit]===2?'orangeWord':'redWord')]"
          >{{stateValue[scope.row[fruit]-1]}}</span>
          <span v-if="fruit === 'subject'">{{scope.row[fruit]}}</span>
          <span v-if="!(fruit ==='status'||fruit ==='subject')">{{scope.row[fruit]}}</span>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
 import { callserve, count } from "@/mock/serve";
import * as serveTable from "@/api/serve/serveTable";

export default {
  data() {
    return {
      key: 1, // table key
      defaultFormThead: [
        "priority",
        "calltype",
        "callstatus",
        "moneyapproval",
        "kehidaima",
        "kehumingcheng"
      ],
      formTheadOptions: [
        // { name: "serveid", label: "ID" },
        { name: "priority", label: "优先级" },
        { name: "calltype", label: "呼叫类型" },
        { name: "callstatus", label: "呼叫状态" },
        { name: "kehidaima", label: "客户代码" },
        { name: "jiedanyuan", label: "接单员" },
        { name: "moneyapproval", label: "费用审核" },
        { name: "kehumingcheng", label: "客户名称" },
        { name: "zhuti", label: "主题" }
      ],
      // this.dialogTable = true;
      dialogFormView: false,
      headLabel: {
        // serveid: "ID",
        priority: "优先级",
        calltype: "呼叫类型",
        callstatus: "呼叫状态",
        kehidaima: "客户代码",
        jiedanyuan: "接单员",
        moneyapproval: "费用审核",
        kehumingcheng: "客户名称",
        zhuti: "主题"
      }
    };
  },
  created() {
    this.getList();
  },
  methods: {
    getList() {
      this.listLoading = true;
      //此处接入模拟数据 mock
      this.list = callserve;
      this.total = count;
      this.listLoading = false;
        serveTable.getTableList(this.listQuery).then(res => {
      console.log(res)
        });
    },
    openTree() {
      this.$emit("openList", true);
      // this.dialogTree = true;  树形图
      //   this.dialogFormView = true
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.$emit("handleSelection", val);
      //   this.multipleSelection = val;
    }
  }
};
</script>

<style>
</style>