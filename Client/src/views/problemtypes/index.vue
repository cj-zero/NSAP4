<template>
  <div>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <!-- <el-input
          @keyup.enter.native="handleFilter"
          size="mini"
          style="width: 200px;"
          class="filter-item"
          :placeholder="'名称'"
          v-model="listQuery.key"
        ></el-input>-->

        <!-- <el-button
          class="filter-item"
          size="mini"
          v-waves
          icon="el-icon-search"
          @click="handleFilter"
        >搜索</el-button>-->
        <permission-btn moduleName="problemtypes" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <el-card shadow="never" class="card-body-none fh" style="height:100%;">
      <tree-table
        @row-click="rowClick"
        @selection-change="handleSelectionChange"
        highlight-current-row
        :data="modulesTree"
        
        :columns="columns"
        border
      ></tree-table>
    </el-card>
    <div class="app-container">
      <el-dialog
        v-el-drag-dialog
        class="dialog-mini"
        width="500px"
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
          <el-form-item size="small" label="名称" prop="name">
            <el-input v-model="temp.name"></el-input>
          </el-form-item>
          <el-form-item size="small" label="描述" prop="description">
            <el-input v-model="temp.description"></el-input>
          </el-form-item>
          <el-form-item size="small" label="是否停用" prop="inuseFlag">
            <el-switch v-model="temp.inuseFlag" active-text="是" inactive-text="否"></el-switch>
          </el-form-item>

          <el-form-item size="small" label="排序" prop="prblmTypID">
            <el-input-number v-model="temp.orderIdx" :min="0" label="描述文字"></el-input-number>
          </el-form-item>
        </el-form>
        <div slot="footer">
          <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
          <el-button size="mini" v-if="dialogStatus=='create'" type="primary" @click="createData">确认</el-button>
          <el-button size="mini" v-else type="primary" @click="updateData">确认</el-button>
        </div>
      </el-dialog>
    </div>
  </div>
</template>

<script>
import * as problemtypes from "@/api/problemtypes";
import waves from "@/directive/waves"; // 水波纹指令
// import Pagination from '@/components/Pagination'
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import elDragDialog from "@/directive/el-dragDialog";
import treeTable from "@/components/TreeTable";

export default {
  name: "problemtypes",
  components: { Sticky, treeTable, permissionBtn },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: "", // 列表checkbox选中的值
      tableKey: 0,
      modulesTree: [],
      total: 0,
      listLoading: true,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined
      },
      columns: [
   
        {
          text: "描述",
          value: "description",
          align:'left'
        },
        {
          text: "是否停用",
          value: "inuseFlag",
                    align:'left'

        }

      ],
      currentPage: 1,
      currentSize: 20,
      statusOptions: [
        { key: 1, display_name: "停用" },
        { key: 0, display_name: "正常" }
      ],
      result1: [],
      temp: {
        name: "", // Name
        description: "", // Description
        inuseFlag: true, // InuseFlag
        orderIdx: 0 // OrderIdx
      },
      dialogFormVisible: false,
      dialogStatus: "",
      textMap: {
        update: "编辑",
        create: "添加"
      },
      typeQuestion: "",
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
  created() {
    this.getList();
  },

  methods: {
    handleCurrentChange() {
      this.getList();
    },
    rowClick(row) {
      this.multipleSelection = row;
      console.log(this.multipleSelection, 'row')
      // this.$refs.mainTable.clearSelection();
      // this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
      if (val.parentId == "" || !val.parentId) {
        this.whoEdit = true;
      }
    },
    onBtnClicked: function(domId) {
      console.log("you click:" + domId);
      switch (domId) {
        case "btnAdd":
          this.handleCreate(this.multipleSelection);
          break;
        case "btnEdit":
          if (!this.multipleSelection) {
            this.$message({
              message: "请点击需要编辑的项",
              type: "error"
            });
            return;
          }
          this.handleUpdate(this.multipleSelection);
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
      problemtypes.getList(this.listQuery).then(response => {
        let arr1 = response.data;
        this.modulesTree = arr1.map(item => {
          item.children = item.childTypes;
          if (item.inuseFlag == 0) {
            item.inuseFlag = "正常";
          } else {
            item.inuseFlag = "停用";
          }
          if (item.children && item.children.length > 0) {
            for (let i = 0; i < item.children.length; i++) {
              if (item.children[i].inuseFlag == 0) {
                item.children[i].inuseFlag = "正常";
              } else {
                item.children[i].inuseFlag = "停用";
              }
            }
          }
          return item;
        });

        this.total = response.count;
        this.listLoading = false;
      });
    },
    handleFilter() {
      this.listQuery.page = 1;
      this.getList();
    },
    changeSwitchopen() {
      if (this.temp.inuseFlag == "正常") {
        this.temp.inuseFlag = false;
      } else {
        this.temp.inuseFlag = true;
      }
    },
    changeSwitch() {
      if (this.temp.inuseFlag == true) {
        this.temp.inuseFlag = 1;
      } else {
        this.temp.inuseFlag = 0;
      }
    },
    resetTemp() {
      this.temp = {
        name: "",
        description: "",
        inuseFlag: false,
        parentId: "",
        orderIdx: "",
        prblmTypID: "",
        parentTypeID: ""
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
          this.changeSwitch();
          this.multipleSelection && (this.temp.parentId = this.multipleSelection.id)
          problemtypes.add(this.temp).then(() => {
            this.getList();
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
      this.changeSwitchopen();
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
          this.changeSwitch();
          const tempData = Object.assign({}, this.temp);
          // let { parentId, id } = this.multipleSelection
          problemtypes.update(tempData).then(() => {
            this.getList();

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
      problemtypes.del(rows.map(u => u.id)).then(() => {
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
</style>
