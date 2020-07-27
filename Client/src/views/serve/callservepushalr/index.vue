<template>
  <div>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <el-input
          @keyup.enter.native="handleFilter"
          size="mini"
          style="width: 200px;"
          class="filter-item"
          :placeholder="'名称'"
          v-model="listQuery.key"
        ></el-input>

        <el-button
          class="filter-item"
          size="mini"
          style="margin:0 15px;"
          v-waves
          icon="el-icon-search"
          @click="handleFilter"
        >搜索</el-button>

        <permission-btn moduleName="callservesure" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <div class="app-container">
      <div class="bg-white">
        <zxsearch></zxsearch>
        <el-table
          ref="mainTable"
          :key="key"
          :data="list"
          v-loading="listLoading"
          border
          fit
          style="width: 100%;"
          highlight-current-row
    @current-change="handleSelectionChange"
          @row-click="rowClick"
        >
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
            :sortable="fruit=='chaungjianriqi'?true:false"
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
        <pagination
          v-show="total>0"
          :total="total"
          :page.sync="listQuery.page"
          :limit.sync="listQuery.limit"
          @pagination="handleCurrentChange"
        />
      </div>
      <!--   v-el-drag-dialog
      width="1000px"  新建呼叫服务单-->
      <el-dialog
        fullscreen
        class="dialog-mini"
        :destroy-on-close="true"
        :title="textMap[dialogStatus]"
        :visible.sync="dialogFormVisible"
      >
           <el-row :gutter="20" type="flex" class="row-bg" justify="space-around">
            <el-col :span="12" >
 
            </el-col>
              <el-col :span="12">
        <zxform
        formName="新建"
          :form="temp"
          labelposition="right"
          labelwidth="100px"
          :isEdit="true"
          refValue="dataForm"
        ></zxform>
</el-col>
           </el-row>
        <div slot="footer">
          <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
          <el-button size="mini" v-if="dialogStatus=='create'" type="primary" @click="createData">确认</el-button>
          <el-button size="mini" v-else type="primary" @click="updateData">确认</el-button>
        </div>
      </el-dialog>
      <!-- 只能查看的表单 -->
      <el-dialog width="1200px" class="dialog-mini" :destroy-on-close="true" title="服务单详情" :visible.sync="dialogFormView">
        <zxform
          :form="temp"
          
          formName="查看"
          labelposition="right"
          labelwidth="100px"
          :isEdit="false"
          refValue="dataForm"
        ></zxform>

        <div slot="footer">
          <el-button size="mini" @click="dialogFormView = false">取消</el-button>
          <el-button size="mini" type="primary" @click="dialogFormView = false">确认</el-button>
        </div>
      </el-dialog>
      <el-dialog v-el-drag-dialog :visible.sync="dialogTable" :destroy-on-close="true" center width="800px">
        <DynamicTable
          :formThead.sync="formTheadOptions"
          :defaultForm.sync="defaultFormThead"
          @close="dialogTable=false"
        ></DynamicTable>
        <span slot="footer" class="dialog-footer">
          <el-button @click="dialogTable = false">取 消</el-button>
          <el-button type="primary" @click="dialogTable = false">确 定</el-button>
        </span>
      </el-dialog>
      <el-dialog v-el-drag-dialog :visible.sync="dialogTree" :destroy-on-close="true" center width="300px">
        <treeList @close="dialogTree=false"></treeList>
        <span slot="footer" class="dialog-footer">
          <el-button @click="dialogTree = false">取 消</el-button>
          <el-button type="primary" @click="dialogTree = false">确 定</el-button>
        </span>
      </el-dialog>
    </div>
  </div>
</template>

<script>
import * as solutions from "@/api/solutions";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
import DynamicTable from "@/components/DynamicTable";
import elDragDialog from "@/directive/el-dragDialog";
import zxsearch from "./search";
import zxform from "../callserve/form";
import treeList from "../callserve/treeList";
import { callserve, count } from "@/mock/serve";
export default {
  name: "solutions",
  components: {
    Sticky,
    permissionBtn,
    Pagination,
    DynamicTable,
    zxsearch,
    zxform,
    treeList
  },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      defaultFormThead: [
        "priority",
        "calltype",
        "chaungjianriqi",
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

      headLabel: {
        // serveid: "ID",
        priority: "优先级",
        chaungjianriqi:'创建时间',
        calltype: "呼叫类型",
        callstatus: "呼叫状态",
        kehidaima: "客户代码",
        jiedanyuan: "接单员",
        moneyapproval: "费用审核",
        kehumingcheng: "客户名称",
        zhuti: "主题"
      },

      tableKey: 0,
      list: null,
      total: 0,
      listLoading: true,
      showDescription: false,
      dialogFormView: false,
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
      dialogTree: false,
      dialogStatus: "",
      textMap: {
        update: "确认呼叫服务单",
        create: "新建呼叫服务单"
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
  filters: {
    filterInt(val) {
      switch (val) {
        case null:
          return "";
        case 1:
          return "状态1";
        case 2:
          return "状态2";
        default:
          return "默认状态";
      }
    },
    statusFilter(disable) {
      const statusMap = {
        false: "color-success",
        true: "color-danger"
      };
      return statusMap[disable];
    }
  },
  watch: {
    // checkboxVal(valArr) {
    // this.formThead = this.formTheadOptions.filter(
    //   i => valArr.indexOf(i) >= 0
    // );
    defaultFormThead(valArr) {
      this.formTheadOptions = this.formTheadOptions.filter(
        i => valArr.indexOf(i) >= 0
      );

      // }
      this.key = this.key + 1; // 为了保证table 每次都会重渲 In order to ensure the table will be re-rendered each time
    }
  },
  created() {
    this.getList();
  },
  mounted() {
    //   console.log(callserve)
  },
  methods: {
    openTree() {
      // this.dialogTree = true;  树形图
      this.dialogFormView = true;
    },
    onSubmit() {
      console.log("submit!");
    },
    changeTable(result) {
      console.log(result);
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
      console.log(val)
    },
    onBtnClicked: function(domId) {
      switch (domId) {
        case "btnAdd":
          this.handleCreate();
          break;
        case "btnDetail":
          this.open();
          break;
        case "editTable":
          this.dialogTable = true;
          break;
        case "btnEdit":
          if (!this.multipleSelection) {
            this.$message({
              message: "请点击需要编辑的数据",
              type: "error"
            });
            return;
          }
          this.handleUpdate(this.multipleSelection[0]);
          break;
        case "btnDel":
          if (!this.multipleSelection) {
            this.$message({
              message: "至少删除一个",
              type: "error"
            });
            return;
          }
          this.handleDelete(this.multipleSelection);
          break;
        default:
          break;
      }
    },
    
    getList() {
      this.listLoading = true;
      //此处接入模拟数据 mock
    //   let newList = callserve.filter(item=>{

    //   })
       this.list = callserve;
      this.total = count;
      this.listLoading = false;
      //   solutions.getList(this.listQuery).then(response => {
      //     this.list = response.data;
      //     this.total = response.count;
      //     this.listLoading = false;
      //   });
    },
    open() {
      this.$confirm("确认已完成回访?", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      })
        .then(() => {
          this.$message({
            type: "success",
            message: "操作成功!"
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消操作"
          });
        });
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
      this.getList();
    },
    handleModifyStatus(row, disable) {
      // 模拟修改状态
      this.$message({
        message: "操作成功",
        type: "success"
      });
      row.disable = disable;
    },
    resetTemp() {
      this.temp = {
        id: "",
        sltCode: "",
        subject: "",
        cause: "",
        symptom: "",
        descriptio: "",
        status: "",
        createUserId: "",
        createUserName: "",
        createTime: "",
        updateUserId: "",
        updateTime: "",
        updateUserName: "",
        extendInfo: ""
      };
    },
    handleCreate() {
      // 弹出添加框
      this.resetTemp();
      this.dialogStatus = "create";
      this.dialogFormVisible = true;
      this.$nextTick(() => {
        this.$refs["dataForm"].clearValidate();
      });
    },
    createData() {
      // 保存提交
      this.$refs["dataForm"].validate(valid => {
        if (valid) {
          solutions.add(this.temp).then(() => {
            this.list.unshift(this.temp);
            this.dialogFormVisible = false;
            this.$notify({
              title: "成功",
              message: "创建成功",
              type: "success",
              duration: 2000
            });
          });
        }
      });
    },
    handleUpdate(row) {
      // 弹出编辑框
      this.temp = Object.assign({}, row); // copy obj
      this.dialogStatus = "update";
      this.dialogFormVisible = true;
      this.$nextTick(() => {
        this.$refs["dataForm"].clearValidate();
      });
    },
    updateData() {
      // 更新提交
      this.$refs["dataForm"].validate(valid => {
        if (valid) {
          const tempData = Object.assign({}, this.temp);
          solutions.update(tempData).then(() => {
            for (const v of this.list) {
              if (v.id === this.temp.id) {
                const index = this.list.indexOf(v);
                this.list.splice(index, 1, this.temp);
                break;
              }
            }
            this.dialogFormVisible = false;
            this.$notify({
              title: "成功",
              message: "更新成功",
              type: "success",
              duration: 2000
            });
          });
        }
      });
    },
    handleDelete(rows) {
      // 多行删除
      solutions.del(rows.map(u => u.id)).then(() => {
        this.$notify({
          title: "成功",
          message: "删除成功",
          type: "success",
          duration: 2000
        });
        rows.forEach(row => {
          const index = this.list.indexOf(row);
          this.list.splice(index, 1);
        });
      });
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
