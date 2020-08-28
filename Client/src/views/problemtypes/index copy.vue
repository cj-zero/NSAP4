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
          v-waves
          icon="el-icon-search"
          @click="handleFilter"
        >搜索</el-button>
        <permission-btn moduleName="problemtypes" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <div class="app-container">
      <el-row class="fh" :gutter="20" style="height: 100%;">
        <el-col :span="10" class="fh ls-border" style="height: 100%;border: 1px solid #EBEEF5;">
          <!--  -->
          <el-card shadow="never" class="body-small" style="height:100%;overflow:auto;">
            <el-link type="primary">全部类型》》</el-link>
          </el-card>
          <el-table
            ref="singleTable"
            :data="listParent"
            highlight-current-row
            @current-change="handleCurrent"
            style="width: 100%"
          >
            <el-table-column property="id" align="center" label="问题ID"></el-table-column>
            <el-table-column property="description" align="center" label="描述" min-width="50"></el-table-column>
            <el-table-column align="center" label="是否停用" width="80">
              <template slot-scope="scope">{{scope.row.inuseFlag==false?'否':'是'}}</template>
            </el-table-column>
            <el-table-column property="orderIdx" align="center" label="排序" width="50"></el-table-column>
          </el-table>
        </el-col>

        <el-col :span="14" class="fh">
          <el-card shadow="never" class="body-small" style="height: 100%;overflow:auto;">
            <el-link type="primary" @click=" listChild=result1">全部细分类型>></el-link>
            <el-link type="info" v-if="typeQuestion">{{typeQuestion}}</el-link>
          </el-card>
          <el-table
            ref="singleTable"
            :data="listChild_list"
            highlight-current-row
            style="width: 100%"
            align="center"
            @current-change="handleSelectionChange"
          >
            <el-table-column property="id" align="center" label="问题ID"></el-table-column>
            <el-table-column property="description" align="center" label="描述" min-width="50"></el-table-column>
            <el-table-column align="center" label="是否停用" width="80">
              <template slot-scope="scope">{{scope.row.inuseFlag==false?'否':'是'}}</template>
            </el-table-column>
            <el-table-column property="orderIdx" align="center" label="排序" width="50"></el-table-column>
          </el-table>
          <el-pagination
            style="background-color:#ffffff;"
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
            :current-page="currentPage"
            :page-sizes="[5, 10, 20, 30, 40]"
            :page-size="20"
            layout="total, sizes, prev, pager, next, jumper"
            v-show="listChild.length>0"
            :total="listChild.length"
          ></el-pagination>
        </el-col>
      </el-row>

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
import Sticky from "@/components/Sticky";
// import Pagination from "@/components/Pagination";

import permissionBtn from "@/components/PermissionBtn";

import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: "problemtypes",
  components: { Sticky, permissionBtn },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: null, // 列表checkbox选中的值
      tableKey: 0,
      listParent: [],
      listChild_list: [],
      listChild:[],
      total: 0,
      listLoading: true,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined
      },
      currentPage: 1,
      currentSize: 20,
      whoEdit: null,
      statusOptions: [
        { key: 1, display_name: "停用" },
        { key: 0, display_name: "正常" }
      ],
      result1: [],
      temp: {
        name: "", // Name
        description: "", // Description
        inuseFlag: false, // InuseFlag
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
  watch:{
    currentSize:{
      handler(){
    this.listChild_list= this.result1.slice(0,this.currentSize)

      // console.log(this.currentSize)
      }

    }
  },
  methods: {
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
      if (val.parentId == "" || !val.parentId) {
        this.whoEdit = true;
      }
    },
    onBtnClicked: function(domId) {
      // console.log("you click:" + domId);
      switch (domId) {
        case "btnAdd":
          this.handleCreate();
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
        this.listParent = response.data.filter(item => item.parentId == "");
        this.result1 = response.data.filter(item => item.parentId !== "");
        this.listChild = this.result1; //分页的数组
        this.listChild_list= this.result1.slice(0,this.currentSize)
        this.total = response.count;
        this.listLoading = false;
      });
    },
    handleFilter() {
      this.listQuery.page = 1;
      this.getList();
    },
    // handleSizeChange(val) {
    //   this.listQuery.limit = val;
    //   this.getList();
    // },
    handleSizeChange(val) {
      // console.log(`每页 ${val} 条`);
      this.currentSize = val;
    },
    handleCurrentChange(val) {
      // console.log(`当前页: ${val}`, `每页 ${this.currentSize} 条`);

      if (this.multipleSelection) {
        // console.log(1)
        let newList = this.result1.slice((val-1)*this.currentSize,val*this.currentSize)

        this.listChild_list = newList;
      } else {
        //  console.log(2)
               let newList = this.result1.slice((val-1)*this.currentSize,val*this.currentSize)

        this.listChild_list = newList;
        // console.log(this.listChild_list)
      }
    },
    handlePageChange(val) {
      let newList = this.result1.filter(item => item.parentId == val.id);
      this.listChild = newList;
    },
    handleCurrent(val) {
      // this.listQuery.page = val.page;
      // this.listQuery.limit = val.limit;
      this.handleSelectionChange(val);
      this.typeQuestion = val.name;
      let newList = this.result1.filter(item => item.parentId == val.id);
      this.listChild = newList;
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
          problemtypes.add(this.temp).then(() => {
            let newList = [];
            if (this.whoEdit) {
              newList = this.listParent;
            } else {
              newList = this.listChild;
            }
            newList.unshift(this.temp);
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
          console.log(valid);
          const tempData = Object.assign({}, this.temp);
          problemtypes.update(tempData).then(() => {
            let newList = [];
            if (this.whoEdit) {
              newList = this.listParent;
            } else {
              newList = this.listChild;
            }

            for (const v of newList) {
              if (v.id === this.temp.id) {
                const index = newList.indexOf(v);
                newList.splice(index, 1, this.temp);
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
