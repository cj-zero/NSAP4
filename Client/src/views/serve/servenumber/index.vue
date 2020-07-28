<template>
  <div>
    <!-- <sticky :className="'sub-navbar'">
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
         <permission-btn moduleName="servenumber" size="mini" v-on:btn-event="onBtnClicked"></permission-btn> 
      </div>
    </sticky>-->
    <div class="app-container">
      <div class="bg-white">
        <el-form ref="form" :model="listQuery" label-width="80px">
          <div style="padding:10px 0;"></div>
          <el-row :gutter="10">
            <el-col :span="4">
              <el-form-item label="序列号" size="medium">
                <el-input v-model="listQuery.ManufSN"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="4">
              <el-form-item label="客户" size="medium">
                <el-input v-model="listQuery.CardName"></el-input>
              </el-form-item>
            </el-col>
            <!-- <el-col :span="4">
              <el-form-item label="呼叫状态" size="medium">
                <el-select v-model="listQuery.region" placeholder="请选择呼叫状态">
                  <el-option label="待确认" value="1"></el-option>
                  <el-option label="已确认" value="2"></el-option>
                  <el-option label="已取消" value="3"></el-option>
                </el-select>
              </el-form-item>
            </el-col>-->

            <el-col :span="4">
              <el-form-item label="物料编码" size="medium">
                <el-input v-model="listQuery.ItemCode"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="4">
              <el-button type="primary" size="medium" @click="getList">搜索</el-button>
            </el-col>
          </el-row>
        </el-form>
<el-table
          ref="mainTable"
          class="table_label"
          :key="key"
          :data="list"
          v-loading="listLoading"
          border
                      max-height="700px"

          fit
          style="width: 100%;"
          highlight-current-row
        >
          <el-table-column
            show-overflow-tooltip
            v-for="(fruit,index) in formTheadOptions"
            :align="fruit.align"
            :key="`ind${index}`"
            header-align="left"
            :width="fruit.width"
            :fixed="fruit.fixed"
            :sortable="fruit=='chaungjianriqi'?true:false"
            style="background-color:silver;"
            :label="fruit.label"
          >
            <template slot-scope="scope">
              <!-- <el-link
                v-if="fruit.name === 'id'"
                type="primary"
                @click="openTree(scope.row.id)"
              >{{scope.row.id}}</el-link>
              <span
                v-if="fruit.name === 'status'"
                :class="[scope.row[fruit.name]===1?'orangeWord':(scope.row[fruit.name]===2?'greenWord':'redWord')]"
              >{{stateValue[scope.row[fruit.name]-1]}}</span> -->
              <span
              >{{scope.row[fruit.name]?scope.row[fruit.name]:'暂无数据'}}</span>
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
import * as callservesure from "@/api/serve/callservesure.js";

import waves from "@/directive/waves"; // 水波纹指令
// import Sticky from "@/components/Sticky";
// import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
export default {
  name: "servenumber",
  components: {
    Pagination
  },
  directives: {
    waves
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      formTheadOptions: [
         { name: "serveid", label: "编号" ,fixed:true},
        { name: "customer", label: "客户代码" ,align:'left'},
        { name: "custmrName", label: "客户名称" },
        { name: "manufSN", label: "制造商序列号",width:'120px' },
        { name: "internalSN", label: "内部序列号" },
        { name: "itemCode", label: "物料编码" },
        { name: "itemName", label: "物料描述" },
        { name: "", label: "制造日期" },
        { name: "", label: "交货单号" },
        { name: "", label: "交货日期" },
        { name: "", label: "合同号" },
        { name: "", label: "服务费" },
        { name: "", label: "销售员" },
        { name: "", label: "合同开始时间",width:'110px' },
        { name: "", label: "合同结束时间",width:'110px' },
        { name: "", label: "创建时间" }
      ],

      tableKey: 0,
      list: null,
      total: 0,
      listLoading: true,
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
    defaultFormThead(valArr) {
      this.formTheadOptions = this.formTheadOptions.filter(
        i => valArr.indexOf(i) >= 0
      );
      this.key = this.key + 1; // 为了保证table 每次都会重渲 In order to ensure the table will be re-rendered each time
    }
  },
  created() {
    this.getList();
  },

  methods: {
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    // handleSelectionChange(val) {  //多选方法
    //   this.multipleSelection = val;
    //   console.log(val);
    // },
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
      callservesure.SerialList(this.listQuery).then(response => {
        this.list = response.data;
        this.total = response.count;
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
      // this.$refs["dataForm"].validate(valid => {
      //   if (valid) {
      //     solutions.add(this.temp).then(() => {
      //       this.list.unshift(this.temp);
      //       this.dialogFormVisible = false;
      //       this.$notify({
      //         title: "成功",
      //         message: "创建成功",
      //         type: "success",
      //         duration: 2000
      //       });
      //     });
      //   }
      // });
    },
    handleUpdate() {
      // 弹出编辑框
      // this.temp = Object.assign({}, row); // copy obj
      // this.dialogStatus = "update";
      // this.dialogFormVisible = true;
      // this.$nextTick(() => {
      //   this.$refs["dataForm"].clearValidate();
      // });
    },
    updateData() {
      // 更新提交
      // this.$refs["dataForm"].validate(valid => {
      //   if (valid) {
      //     const tempData = Object.assign({}, this.temp);
      //     solutions.update(tempData).then(() => {
      //       for (const v of this.list) {
      //         if (v.id === this.temp.id) {
      //           const index = this.list.indexOf(v);
      //           this.list.splice(index, 1, this.temp);
      //           break;
      //         }
      //       }
      //       this.dialogFormVisible = false;
      //       this.$notify({
      //         title: "成功",
      //         message: "更新成功",
      //         type: "success",
      //         duration: 2000
      //       });
      //     });
      //   }
      // });
    },
    handleDelete() {
      // 多行删除
      // solutions.del(rows.map(u => u.id)).then(() => {
      //   this.$notify({
      //     title: "成功",
      //     message: "删除成功",
      //     type: "success",
      //     duration: 2000
      //   });
      //   rows.forEach(row => {
      //     const index = this.list.indexOf(row);
      //     this.list.splice(index, 1);
      //   });
      // });
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
