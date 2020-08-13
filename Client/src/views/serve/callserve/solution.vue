<template>
  <div>
    <div class="app-container">
      <el-row class="search-wrapper" type="flex" justify="end">
        <el-input v-model="query" style="width: 150px;margin-right: 10px"></el-input>
        <el-button type="primary" size="mini" icon="el-icon-search" @click="handleSearch" style="margin-right: 10px">搜索</el-button>
      </el-row>
      <div class="bg-white" style="margin-top: 20px;">
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
        >
          <el-table-column
            show-overflow-tooltip
            v-for="fruit in defaultFormThead"
            align="left"
            :width="headWidth[fruit]"
            :key="fruit"
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
        <pagination
          v-show="total>0"
          :total="total"
          :page.sync="listQuery.page"
          :limit.sync="listQuery.limit"
          @pagination="handleCurrentChange"
        />
      </div>
    </div>
  </div>
</template>

<script>
import waves from "@/directive/waves"; // 水波纹指令
import Pagination from "@/components/Pagination";
// import * as solutions from "@/api/solutions";

// import DynamicTable from "@/components/DynamicTable";
import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: "solutions",
  props:{list:{
      type:Array,
      default:()=>[]
  },total:{
      type:Number,
      default:0
  },
  listLoading:{
      type:Boolean,
      default:false
  }},
  components: { Pagination },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      defaultFormThead: ["sltCode", "status", "subject"],
      formTheadOptions: [
        { name: "id", label: "ID" },
        { name: "sltCode", label: "编号" },
        { name: "status", label: "症状" },
        { name: "subject", label: "解决方案" }
      ],
      // this.dialogTable = true;
      query: '', // 查询字符串
      headLabel: {
        id: "ID",
        sltCode: "编号",
        status: "症状",
        subject: "解决方案"
      },
      headWidth: {
        subject: "700px"
      },

      // checkboxVal: defaultFormThead, // checkboxVal
      // formThead: defaultFormThead, // 默认表头 Default header
      tableKey: 0,
      showDescription: false,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined
      },
      stateValue: ["发布", "检查", "内部"],
      statusOptions: [
        { key: 1, display_name: "发布" },
        { key: 2, display_name: "检查" },
        { key: 3, display_name: "内部" }
      ],
      temp: {
        id: "", // Id
        sltCode: "", // SltCode
        subject: "", // Subject
        cause: "", // Cause
        symptom: "", // Symptom
        descriptio: "", // Descriptio
        status: "", // Status
        extendInfo: "" // 其他信息,防止最后加逗号，可以删除
      },
      dialogFormVisible: false,
      dialogTable: false,
      dialogStatus: "",
      textMap: {
        update: "编辑",
        create: "添加"
      },
      dialogPvVisible: false,
      pvData: [],
      rules: {
        appId: [
          { required: true, message: "必须选择一个应用", trigger: "change" }
        ],
        name: [{ required: true, message: "名称不能为空", trigger: "blur" }]
      },
      downloadLoading: false
    };
  },

  watch: {
    defaultFormThead(valArr) {
      this.formTheadOptions = this.formTheadOptions.filter(
        i => valArr.indexOf(i) >= 0
      );
      // }
      this.key = this.key + 1; // 为了保证table 每次都会重渲 In order to ensure the table will be re-rendered each time
    },
    total:function(a,b){
        console.log(a,b)
    }
  },
  created() {
    // this.getList();
  },
  //   mounted(){
  //   },
  methods: {
    changeTable(result) {
      console.log(result);
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
      this.$emit("solution-click", row);
    //   this.$emit("solution-get", this.list);
    },
    getList()   {
        //        this.listLoading = true;

        //    solutions.getList(this.listQuery).then(res => {
        //    this.listLoading = false;

    // });
    },
    handleSearch () {
      let { limit } = this.listQuery
      let params = {
        key: this.query,
        page: 1,
        limit
      }
      this.$emit('search', params)
    },
    handleFilter() {
      this.listQuery.page = 1;
      this.getList();
    },
    handleSizeChange(val) {
      this.listQuery.limit = val;
      this.getList();
    },
    handleCurrentChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
    this.$emit("page-Change",this.listQuery)
    },
    handleModifyStatus(row, disable) {
      // 模拟修改状态
      this.$message({
        message: "操作成功",
        type: "success"
      });
      row.disable = disable;
    }
  }
};
</script>
<style>
.dialog-mini .el-select {
  width: 100%;
}
.greenWord {
  color: green;
}
.orangeWord {
  color: orange;
}
.redWord {
  color: orangered;
}
</style>
