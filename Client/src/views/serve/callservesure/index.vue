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
          <!-- <el-table-column     v-for="(fruit,index) in formTheadOptions"  :key="`ind${index}`">
              <el-radio v-model="fruit.id" ></el-radio>
          </el-table-column> -->

          <el-table-column
            show-overflow-tooltip
            v-for="(fruit,index) in formTheadOptions"
            align="center"
            :key="`ind${index}`"
            :sortable="fruit=='chaungjianriqi'?true:false"
            style="background-color:silver;"
            :label="fruit.label"
          >
            <template slot-scope="scope">
              <el-link v-if="fruit.name === 'id'" type="primary" @click="openTree(scope.row.id)">{{scope.row.id}}</el-link>
              <span
                v-if="fruit.name === 'status'"
                :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
              >{{stateValue[scope.row[fruit.name]-1]}}</span>
              <span v-if="fruit.name === 'subject'">{{scope.row[fruit.name]}}</span>
              <span
                v-if="!(fruit.name ==='status'||fruit.name ==='subject'||fruit.name ==='id')"
              >{{scope.row[fruit.name]}}</span>
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
        width="90%"
        class="dialog-mini"
        @open="openCustoner"
        :title="textMap[dialogStatus]"
        :visible.sync="dialogFormVisible"
      >
        <el-row :gutter="20" type="flex" class="row-bg" justify="space-around">
          <el-col :span="12">
            <customerupload style="position:sticky;top:0;" :form="formValue"></customerupload>
          </el-col>
          <el-col :span="12">
            <zxform
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
      <el-dialog width="1200px" class="dialog-mini" title="服务单详情" :visible.sync="dialogFormView">
        <zxform
          :form="temp"
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

      <el-dialog v-el-drag-dialog :visible.sync="dialogTree" center width="300px">
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
import * as callservesure from "@/api/serve/callservesure";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";

import elDragDialog from "@/directive/el-dragDialog";
import zxsearch from "./search";
import customerupload from "./customerupload";
import zxform from "../callserve/form";
import treeList from "../callserve/treeList";
// import { callserve } from "@/mock/serve";
export default {
  name: "callservesure",
  components: {
    Sticky,
    permissionBtn,
    Pagination,

    zxsearch,
    zxform,
    treeList,
    customerupload
  },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      formTheadOptions: [
        { name: "id", label: "ID" },
        { name: "customerId", label: "客户代码" },
        { name: "customerName", label: "客户名称" },
        { name: "createTime", label: "创建日期" },
        { name: "contacter", label: "联系人" },
        { name: "services", label: "服务内容" },
        { name: "contactTel", label: "电话号码" },
        { name: "supervisor", label: "售后主管" },
        { name: "salesMan", label: "销售员" },
        { name: "manufSN", label: "制造商序列号" },
        { name: "itemCode", label: "物料编码" }
      ],
      tableKey: 0,
      formValue:{},
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
      checkd:'',
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
  },
  created() {
    this.getList();
  },
  mounted() {
    //   console.log(callserve)
  },
  methods: {
 openCustoner(){
      
//  this.formValu = this.formValue
      // this.dialogTree = true;  树形图
    },
    openTree() {
        
     
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
          if (!this.multipleSelection.id) {
            this.$message({
              message: "请点击需要编辑的数据",
              type: "error"
            });
            return;
          }
          this.handleUpdate(this.multipleSelection);
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

      
      callservesure.getTableList(this.listQuery).then(response => {
        this.total = response.data.count;
        this.list = response.data.data;
        this.listLoading = false;
      });
     
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
          callservesure.add(this.temp).then(() => {
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
        callservesure.getForm(row.id).then(response => {
        this.formValue = response.result;
        console.log(this.formValue)
           this.dialogStatus = "update";
      this.dialogFormVisible = true;
      this.$nextTick(() => {
        this.$refs["dataForm"].clearValidate();
      });
      });
   
    },
    updateData() {
      // 更新提交
      this.$refs["dataForm"].validate(valid => {
        if (valid) {
          const tempData = Object.assign({}, this.temp);
          callservesure.update(tempData).then(() => {
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
      callservesure.del(rows.map(u => u.id)).then(() => {
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
