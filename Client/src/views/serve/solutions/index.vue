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

        <!-- <el-select size="mini" v-model="checkboxVal" multiple collapse-tags placeholder="请选择">
          <el-option
            v-for="item of formTheadOptions"
            :key="`col_${item}`"
            :label="item"
            :value="item"
          ></el-option>
        </el-select>-->

        <permission-btn moduleName="solutions" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <Layer>
      <common-table
        ref="mainTable"
        :data="list"
        :columns="formTheadOptions"
        v-loading="listLoading"
        :key="key"
        height="100%"
        @row-click="rowClick"
        @selection-change="handleSelectionChange"
      >
      </common-table>
      <pagination
        v-show="total>0"
        :total="total"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleCurrentChange"
      />
    </Layer> 
    <el-dialog
      v-el-drag-dialog
      class="dialog-mini"
      :close-on-click-modal="false"
      :modal-append-to-body="false"
      width="500px"
      :modal="false"
      :destroy-on-close="true"
      :title="textMap[dialogStatus]"
      :visible.sync="dialogFormVisible"
    >
      <el-form
        :rules="rules"
        ref="dataForm"
        :model="temp"
        label-position="right"
        label-width="100px"
      >
        <el-form-item size="small" label="Id" prop="id">
          <el-input disabled v-model="temp.id"></el-input>
        </el-form-item>
        <!-- <el-form-item size="small" label="SltCode">
          <el-select class="filter-item" v-model="temp.sltCode" placeholder="Please select">
            <el-option
              v-for="item in  statusOptions"
              :key="item.key"
              :label="item.display_name"
              :value="item.key"
            ></el-option>
          </el-select>
        </el-form-item>-->
        <el-form-item size="small" label="解决方案" prop="subject">
          <el-input v-model="temp.subject"></el-input>
        </el-form-item>
        <el-form-item size="small" label="原因" prop="cause">
          <el-input v-model="temp.cause"></el-input>
        </el-form-item>
        <el-form-item size="small" label="症状" prop="symptom">
          <el-input v-model="temp.symptom"></el-input>
        </el-form-item>
        <el-form-item size="small" label="备注" prop="descriptio">
          <el-input v-model="temp.descriptio"></el-input>
        </el-form-item>
        <el-form-item size="small" label="Status">
          <el-select class="filter-item" v-model="temp.status" placeholder="Please select">
            <el-option
              v-for="item in  statusOptions"
              :key="item.key"
              :label="item.display_name"
              :value="item.key"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <div slot="footer">
        <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
        <el-button size="mini" v-if="dialogStatus=='create'" type="primary" @click="createData">确认</el-button>
        <el-button size="mini" v-else type="primary" @click="updateData">确认</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import * as solutions from "@/api/solutions";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
// import DynamicTable from "@/components/DynamicTable";
import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: "solution",
  components: { Sticky, permissionBtn, Pagination },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      defaultFormThead: [
        "sltCode",
        "status",
        "cause",
        "descriptio",
        "subject",
        "updateUserName"
      ],
      formTheadOptions: [
        // { name: "id" ,label:'ID',align:'right'},
        { prop: "sltCode" ,label:'编号',align:'left' },
        // { name: "status" ,label:'状态' },
        { prop: "symptom" ,label:'症状' },
        { prop: "subject" ,label:'解决方案' },
        // { prop: "cause"  ,label:'原因'},
        // { name: "descriptio"  ,label:'备注'},
        { prop: "updateUserName"  ,label:'更新人名字'},
        { prop: "createTime" ,label:'创建时间' }
      ],
      headLabel: {
        id: "ID",
        sltCode: "编号",
        status: "状态",
        symptom: "症状",
        subject: "解决方案",
        cause: "原因",
        descriptio: "备注",
        updateUserName: "更新人名字",
        createTime: "创建时间"
      },

      // checkboxVal: defaultFormThead, // checkboxVal
      // formThead: defaultFormThead, // 默认表头 Default header
      tableKey: 0,
      list: null,
      total: 0,
      listLoading: true,
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
        sltCode: 0, // SltCode
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
      defaultFormThead(valArr){
            this.formTheadOptions = this.formTheadOptions.filter(
        i => valArr.indexOf(i) >= 0
      )
       
      // }
      this.key = this.key + 1; // 为了保证table 每次都会重渲 In order to ensure the table will be re-rendered each time
    }
  },
  created() {
    this.getList();
  },
  methods: {
    changeTable(result){
      console.log(result)
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
        case "btnEdit":
          // this.dialogTable = true;
          console.log(this.multipleSelection)
          if (!this.multipleSelection.length) {
            this.$message({
              message: "请选择编辑的方案",
              type: "warning"
            });
            return;
          }
          
          this.handleUpdate(this.multipleSelection[0]);
          break;
        case "btnDel":
          if (this.multipleSelection.length < 1) {
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
      solutions.getList(this.listQuery).then(response => {
        this.list = response.data;
        this.total = response.count;
        this.listLoading = false;
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
        sltCode: 0,
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
            this.getList()
          });
        }
      });
    },
    handleUpdate(row) {
      // 弹出编辑框
      console.log(row)
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
            this.getList()
          });
        }
      });
    },
    handleDelete(rows) {
      // 多行删除
              this.$confirm('此操作将永久删除该解决方案, 是否继续?', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        }).then(() => {
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
        this.getList()
      });

        }).catch(() => {
          this.$message({
            type: 'info',
            message: '已取消删除'
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
.greenWord{
  color:green;
}
.orangeWord{
  color:orange;
}
.redWord{
  color:orangered;
}
</style>
