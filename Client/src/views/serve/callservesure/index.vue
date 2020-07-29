<template>
  <div style="position:relative;" >
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
        <el-form ref="listQuery" :model="listQuery" label-width="80px" >
          <div style="padding:10px 0;"></div>
          <el-row :gutter="10">
            <el-col :span="4">
              <el-form-item label="服务ID" size="medium">
                <el-input v-model="listQuery.QryServiceOrderId"></el-input>
              </el-form-item>
            </el-col>

            <el-col :span="4">
              <el-form-item label="呼叫状态" size="medium">
                <el-select v-model="listQuery.QryState" placeholder="请选择呼叫状态">
                  <el-option label="全部" value></el-option>
                  <el-option label="待确认" :value='1'></el-option>
                  <el-option label="已确认" :value='2'></el-option>
                  <el-option label="已取消" :value='3'></el-option>
                </el-select>
              </el-form-item>
            </el-col>

            <el-col :span="4">
              <el-form-item label="客户" size="medium">
                <el-input v-model="listQuery.QryCustomer"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="4">
              <el-form-item label="序列号" size="medium">
                <el-input v-model="listQuery.QryManufSN"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="创建日期" size="medium">
                <el-col :span="11">
                  <el-date-picker
                    placeholder="选择开始日期"
                    v-model="listQuery.QryCreateTimeFrom"
                    style="width: 100%;"
                  ></el-date-picker>
                </el-col>
                <el-col class="line" :span="2">至</el-col>
                <el-col :span="11">
                  <el-date-picker
                    placeholder="选择结束时间"
                    v-model="listQuery.QryCreateTimeTo"
                    style="width: 100%;"
                  ></el-date-picker>
                </el-col>
              </el-form-item>
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
          
          style="width: 100%;"
          highlight-current-row
          @row-click="rowClick"
        >

          <el-table-column width="50" >
            <template slot-scope="scope">
              <el-radio v-model="radio" :label="scope.row.id"></el-radio>
            </template>
          </el-table-column>
          <el-table-column
            show-overflow-tooltip
            v-for="(fruit,index) in formTheadOptions"
            :align="fruit.align"
            :key="`ind${index}`"
            header-align="left"
            :sortable="fruit=='chaungjianriqi'?true:false"
            style="background-color:silver;"
            :label="fruit.label"
            :width="fruit.width"
          >
            <template slot-scope="scope">
              <el-link
                v-if="fruit.name === 'id'"
                type="primary"
                @click="openTree(scope.row.id)"
              >{{scope.row.id}}</el-link>
              <span
                v-if="fruit.name === 'status'"
                :class="[scope.row[fruit.name]===1?'orangeWord':(scope.row[fruit.name]===2?'greenWord':'redWord')]"
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
        width="1480px"
        class="dialog-mini"
        @open="openCustoner"
        @close="closeCustoner"
        :destroy-on-close="true"
        :title="textMap[dialogStatus]"
        :visible.sync="dialogFormVisible"
      >
        <el-row :gutter="20" type="flex" class="row-bg" justify="space-around">
          <el-col :span="11">
            <customerupload style="position:sticky;top:0;" :form="formValue"></customerupload>
          </el-col>
          <el-col :span="13">
            <zxform
              :form="temp"
              formName="确认"
              labelposition="right"
              labelwidth="100px"
              :isEdit="true"
              :sure="sure"
              :customer="customer"
              @close-Dia="closeDia"
            ></zxform>
          </el-col>
        </el-row>
        <div slot="footer">
          <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
          <!-- <el-button
            size="mini"
            v-if="dialogStatus=='create'"
            type="primary"
            :loading="loadingBtn"
            @click="createData"
          >确认</el-button>-->
          <el-button size="mini" type="primary" :loading="loadingBtn" @click="updateData">确认</el-button>
          <!-- <el-button size="mini"  >加载中</el-button> -->
        </div>
      </el-dialog>
      <!-- 只能查看的表单 -->
      <el-dialog
        width="800px"
        class="dialog-mini"
        title="服务单详情"
        :destroy-on-close="true"
        @open="openDetail"
        :visible.sync="dialogFormView"
      >
        <zxform
          :form="temp"
          formName="查看"
          labelposition="right"
          labelwidth="100px"
          :isEdit="false"
          :refValue="dataForm"
        ></zxform>

        <div slot="footer">
          <el-button size="mini" @click="dialogFormView = false">取消</el-button>
          <el-button size="mini" type="primary" @click="dialogFormView = false">确认</el-button>
        </div>
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
import * as callservesure from "@/api/serve/callservesure";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";

import elDragDialog from "@/directive/el-dragDialog";
// import zxsearch from "./search";
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
      radio: "", //单选
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      sure: 0,
      formTheadOptions: [
        { name: "id", label: "服务单ID" ,align:'left',width:'100px'},
        { name: "customerId", label: "客户代码",align:'left' },
        { name: "status", label: "状态" ,align:'left',width:'80px'},
        { name: "customerName", label: "客户名称",align:'left' },
        { name: "createTime", label: "创建日期" ,align:'left'},
        { name: "contacter", label: "联系人" ,align:'left'},
        { name: "services", label: "服务内容" ,align:'left'},
        { name: "contactTel", label: "电话号码" ,align:'left',width:'120px'},
        { name: "supervisor", label: "售后主管" ,align:'left'},
        { name: "salesMan", label: "销售员" ,align:'left'},
        { name: "manufSN", label: "制造商序列号",align:'left' },
        { name: "itemCode", label: "物料编码" ,align:'left'}
      ],

      tableKey: 0,
      formValue: {},
      list: null,
      total: 0,
      loadingBtn: false,
      listLoading: true,
      showDescription: false,
      dialogFormView: false,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined,
        QryServiceOrderId: "", //查询服务ID查询条件
        QryState: 1, //呼叫状态查询条件
        QryCustomer: "", //客户查询条件
        QryManufSN: "", // 制造商序列号查询条件
        QryCreateTimeFrom: "", //创建日期从查询条件
        QryCreateTimeTo: "" //创建日期至查询条件
        // QryRecepUser:"",//接单员
        // QryTechName:"",//工单技术员
        // QryProblemType:"",//问题类型
        // QryMaterialTypes:""//物料类别（多选)
      },
      stateValue: ["待确认", "已确认", "已取消"],
      statusOptions: [
        { key: 1, display_name: "待确认" },
        { key: 2, display_name: "已确认" },
        { key: 3, display_name: "已取消" }
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
      customer: {},
      checkd: "",
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
      dataForm: {}, //传递的表单props
      dataForm1: {}, //获取的详情表单
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
    listQuery: {
      deep: true,
      handler(val) {
        callservesure.getTableList(val).then(response => {
          this.total = response.data.count;
          this.list = response.data.data;
          this.listLoading = false;
        });
      }
    },
    formValue: {
      deep: true,
      handler() {
        if (this.formValue && this.formValue.customerId) {
          this.customer = this.formValue;
        } else {
          if (!this.dialogFormVisible) {
            this.$message({
              message: "没有发现客户代码，请手动选择",
              type: "warning"
            });
          }
        }
      }
    }
  },
  created() {
    this.getList();
  },
  mounted() {
    //   console.log(callserve)
  },
  methods: {
    openCustoner() {
      this.loadingBtn = false;
      if (this.formValue && this.formValue.customerId) {
        this.customer = this.formValue;
      } else {
        this.$message({
          message: "没有发现客户代码，请手动选择",
          type: "warning"
        });
      }
    },
    openDetail() {
      this.dataForm = this.dataForm1;
    },
    closeCustoner() {
      this.getList();
    },
    openTree(res) {
      this.listLoading = true;
      callservesure.GetDetails(res).then(res => {
        if (res.code == 200) {
          this.dataForm1 = res.result;
          this.dialogFormView = true;
        }
        this.listLoading = false;
      });
    },
    onSubmit() {
      console.log("submit!");
    },
    changeTable(result) {
      console.log(result);
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.multipleSelection = row;
      this.radio = row.id;
      this.$refs.mainTable.toggleRowSelection(row);
    },
    // handleSelectionChange(val) {
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
          // console.log(this.multipleSelection);
          if (!this.multipleSelection.id) {
            this.$message({
              message: "请选择需要编辑的数据",
              type: "error"
            });
            return;
          }
          if (this.multipleSelection.status === 2) {
            this.$message({
              message: "该服务单已经被确认过",
              type: "warning"
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
      // this.listQuery.page = 1;
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
      // this.$nextTick(() => {
      //   this.$refs["dataForm"].clearValidate();
      // });
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
        // console.log(this.formValue);
        this.dialogStatus = "update";
        this.dialogFormVisible = true;
        // this.$nextTick(() => {
        //   this.$refs["dataForm"].clearValidate();
        // });
      });
    },
    closeDia() {
      this.loadingBtn = false;
      this.dialogFormVisible = false;
    },
    updateData() {
      // 更新提交
      // this.loadingBtn = true;
      // setTimeout(function() {
      //   this.loadingBtn = false;
      // }, 5000);
      this.sure = this.sure + 1; //向form表单发送提交通知
      // this.dialogFormVisible =false
      // this.$refs["dataForm"].validate(valid => {
      //   if (valid) {
      //     const tempData = Object.assign({}, this.temp);
      //     callservesure.update(tempData).then(() => {
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
<style lang="scss" scoped>
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
.table_label {
  ::v-deep.el-radio {
    margin-left: 6px;
  }
  ::v-deep.el-radio__label {
    display: none;
  }
}
// .mainPage {
//   ::v-deep .el-dialog__wrapper {
//     position: absolute;
//        .el-dialog__header {
//         .el-dialog__title {
//           color: white;
//         }
//         .el-dialog__close {
//           color: white;
//         }
//         background: lightslategrey;
//       }
//      .el-dialog__body {
//     padding: 10px 20px;
//   }
//   }

// }


</style>




