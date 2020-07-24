<template>
  <div style="position:relative;">
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
        <permission-btn moduleName="callserve" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <div class="app-container">
      <div class="bg-white">
        <zxsearch @change-Search="changeSearch"></zxsearch>
            <el-table
              ref="mainTable"
              class="table_label"
              :key="key"
              :data="list"
              v-loading="listLoading"
              border
              fit
              style="width: 100%;"
              highlight-current-row
              @row-click="rowClick"
            >
                <el-table-column type="expand">
          <template slot-scope="scope">
                      <el-table
              ref="mainTablechuldren"
              :key="key"
              :data="scope.row.serviceWorkOrders"
              v-loading="listLoading"
              border
              fit
              style="width: 100%;"
              highlight-current-row
              @row-click="rowClickChild"
            >
              <el-table-column
                show-overflow-tooltip
                v-for="(fruit,index) in ChildheadOptions"
                align="center"
                :key="`ind${index}`"
                :sortable="fruit=='chaungjianriqi'?true:false"
                style="background-color:silver;"
                :label="fruit.label"
                :fixed="fruit.ifFixed"
                :width="fruit.width" >
                <template slot-scope="scope">
                  <span
                    v-if="fruit.name === 'status'"
                    :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
                  >{{statusOptions[scope.row[fruit.name]].label}}</span>
                  <span v-if="fruit.name === 'fromType'&&!scope.row.serviceWorkOrders">{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                  <span v-if="fruit.name === 'priority'">{{priorityOptions[scope.row.priority]}}</span>
                  <span
                    v-if="fruit.name!='priority'&&fruit.name!='fromType'&&fruit.name!='status'&&fruit.name!='serviceOrderId'"
                  >{{scope.row[fruit.name]}}</span>
                </template>
              </el-table-column>
                          </el-table>

               </template>
        </el-table-column>
              <el-table-column
                show-overflow-tooltip
                v-for="(fruit,index) in ParentHeadOptions"
                align="center"
                :key="`ind${index}`"
                :sortable="fruit=='chaungjianriqi'?true:false"
                style="background-color:silver;"
                :label="fruit.label"
                :fixed="fruit.ifFixed"
                :width="fruit.width"
              >
                <template slot-scope="scope">
                  <el-link
                    v-if="fruit.name === 'serviceOrderId'"
                    type="primary"
                    @click="openTree(scope.row.serviceOrderId)"
                  >{{scope.row.serviceOrderId}}</el-link>
                  <span
                    v-if="fruit.name === 'status'"
                    :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
                  >{{statusOptions[scope.row[fruit.name]].label}}</span>
                  <span v-if="fruit.name === 'fromType'&&!scope.row.serviceWorkOrders">{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                  <span v-if="fruit.name === 'priority'">{{priorityOptions[scope.row.priority]}}</span>
                  <span
                    v-if="fruit.name!='priority'&&fruit.name!='fromType'&&fruit.name!='status'&&fruit.name!='serviceOrderId'"
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

      <!-- 客服新建服务单 -->
      <el-dialog
        width="800px"
        class="dialog-mini"
        @close="closeCustoner"
        :close-on-click-modal="false"
        :title="textMap[dialogStatus]"
        :visible.sync="dialogFormVisible"
      >
        <zxform
          :form="temp"
          formName="新建"
          labelposition="right"
          labelwidth="100px"
          :isEdit="true"
          :sure="sure"
          :customer="customer"
          @close-Dia="closeDia"
        ></zxform>
        <div slot="footer">
          <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
          <el-button size="mini" type="primary" :loading="loadingBtn" @click="createData">确认</el-button>
        </div>
      </el-dialog>
      <!-- 客服编辑服务单 -->
      <el-dialog
        width="800px"
        class="dialog-mini"
        @close="closeCustoner"
                :close-on-click-modal="false"

        destroy-on-close
        :title="textMap[dialogStatus]"
        :visible.sync="FormUpdate"
      >
        <zxform
          :form="temp"
          formName="编辑"
          labelposition="right"
          labelwidth="100px"
          isEditForm="true"
          :isEdit="true"
          :sure="sure"
          :customer="customer"
          
          @close-Dia="closeDia"
        ></zxform>
        <div slot="footer">
          <el-button size="mini" @click="FormUpdate = false">取消</el-button>
          <el-button size="mini" type="primary" :loading="loadingBtn" @click="FormUpdate">确认</el-button>
        </div>
      </el-dialog>
      <!-- 只能查看的表单 -->
      <el-dialog
        width="800px"
        class="dialog-mini"
        title="服务单详情"
                :close-on-click-modal="false"

        destroy-on-close
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
// import treeTable from "@/components/TreeTable";

import elDragDialog from "@/directive/el-dragDialog";
// import zxsearch from "./search";
// import customerupload from "../callservesure/customerupload";
import zxform from "./form";
import zxsearch from "./search";
import treeList from "./treeList";
// import { callserve } from "@/mock/serve";
export default {
  name: "callservesure",
  components: {
    Sticky,
    permissionBtn,
    Pagination,
    zxform,
    treeList,
    zxsearch
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
      ParentHeadOptions: [
        { name: "serviceOrderId", label: "服务单号", width: "120px"},
        { name: "customerId", label: "客户代码" },
        { name: "customerName", label: "客户名称" },
        { name: "contacter", label: "联系人" },
        { name: "contactTel", label: "电话号码" ,width:'150px'},
        { name: "supervisor", label: "售后主管" },
        { name: "salesMan", label: "销售员" },
      ],
            ChildheadOptions: [
        // { name: "serviceOrderId", label: "服务单号", ifFixed: true },
        { name: "id", label: "工单号" },
        { name: "priority", label: "优先级" },
        { name: "fromType", label: "呼叫类型", width: "100px" },
        // { name: "customerId", label: "客户代码" },
        { name: "status", label: "状态" },
        // { name: "customerName", label: "客户名称" },
         { name: "FromTheme", label: "呼叫主题" },
        { name: "createTime", label: "创建日期" ,width:'160px'},
        { name: "RecepUserName", label: "接单员" },
        { name: "TechName", label: "技术员" },
         { name: "materialCode", label: "制造商序列号",width:'140px' },
         { name: "materialDescription", label: "物料编码",width:'120px' },
        // { name: "contacter", label: "联系人" },
        // { name: "contactTel", label: "电话号码" ,width:'120px'},
        // { name: "supervisor", label: "售后主管" },
        // { name: "salesMan", label: "销售员" },
        { name: "bookingDate", label: "预约时间" },
        { name: "visitTime", label: "上门时间" },
        { name: "warrantyEndDate", label: "结束时间" },
      ],
      // stateValue: ["待确认", "已确认", "已取消"],
      // statusOptions: [
      //   { key: 1, display_name: "待确认" },
      //   { key: 2, display_name: "已确认" },
      //   { key: 3, display_name: "已取消" }
      // ],
            statusOptions: [
        { value: 1, label: "待处理" },
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "已外出" },
        { value: 5, label: "已挂起" },
        { value: 6, label: "已接收" },
        { value: 7, label: "已解决" },
        { value: 8, label: "已回访" }
      ],
      modulesTree: [],
      priorityOptions: ["低", "中", "高"],
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
        QryServiceOrderId: "" //查询服务单号查询条件
        // QryState: "", //呼叫状态查询条件
        // QryCustomer: "", //客户查询条件
        // QryManufSN: "", // 制造商序列号查询条件
        // QryCreateTimeFrom: "", //创建日期从查询条件
        // QryCreateTimeTo: "" //创建日期至查询条件
        // QryRecepUser:"",//接单员
        // QryTechName:"",//工单技术员
        // QryProblemType:"",//问题类型
        // QryMaterialTypes:""//物料类别（多选)
      },

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
      FormUpdate: false, //编辑表单的dialog
      textMap: {
        update: "编辑呼叫服务单",
        create: "新建呼叫服务单",
        info: "查看呼叫服务单"
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
           let arr = [];
      callservesure.rightList(val).then(response => {
            let resul = response.data.data;
        for (let i = 0; i < resul.length; i++) {
          arr[i] = resul[i];
          arr[i].label = `服务${resul[i].serviceOrderId}`;
          resul[i].serviceWorkOrders.map((item1,index) => {
            arr[i].serviceWorkOrders[index].label= `工单${item1.id}` ;
          })
        }
        this.total = response.data.count;
        this.list =resul;
      })
      }
    },
    formValue: {
      deep: true,
      handler() {
        if (this.formValue && this.formValue.customerId) {
          this.customer = this.formValue;
          console.log(this.customer)
          //编辑获得的数据
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
  created() {},
  mounted() {
    //   console.log(callserve)
    this.getList();
    this.getLeftList();
  },
  methods: {
    checkServeId(res) {
      if (res.children) {
        // console.log()
        this.listQuery.QryServiceOrderId = res.label.split("服务单号：")[1];
      }
      if (res === true) {
        // console.log()
        this.radio = "";
        this.listQuery.QryServiceOrderId = "";
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
      this.$refs.mainTable.toggleRowSelection(row);
    },
        rowClickChild(row) {
      this.$refs.mainTablechuldren.clearSelection();
      // this.multipleSelection = row;
      this.$refs.mainTablechuldren.toggleRowSelection(row);
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
          if (!this.multipleSelection.serviceOrderId) {
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
    changeSearch(res) {
      if (res === 1) {
        this.getList();
      } else {
        Object.assign(this.listQuery, res);
      }
    },
    getList() {
      this.listLoading = true;
      // let arr = [];
      callservesure.rightList(this.listQuery).then(response => {
            let resul = response.data.data;
        // for (let i = 0; i < resul.length; i++) {
        //   // arr[i] = resul[i];
        //   arr[i]={}
        //   arr[i].serviceWorkOrders =[]
        //   arr[i].serviceOrderId =[]
        //   arr[i].label = `服务${resul[i].serviceOrderId}`;
        //   arr[i].serviceOrderId = resul[i].serviceOrderId;
        //   let {contactTel,contacter,customerId,customerName,recepUserName,salesMan,serviceCreateTime,serviceStatus,supervisor,techName,terminalCustomer}={...resul[i]}
        //   let newobj=Object.assign({contactTel,contacter,customerId,customerName,recepUserName,salesMan,serviceCreateTime,serviceStatus,supervisor,techName,terminalCustomer})
        //   resul[i].serviceWorkOrders.map((item1,index) => {
        //     // arr[i] = resul[i];
        //     arr[i].serviceWorkOrders[index] =[]
        //     let arrnewobj= resul[i].serviceWorkOrders[index]
        //  Object.assign(arrnewobj,newobj)
        // arr[i].serviceWorkOrders[index]= arrnewobj
        //     arr[i].serviceWorkOrders[index].label= `工单${item1.id}` ;
        //   });
        // }
         this.total = resul.length;
        this.list =resul;
        this.listLoading = false;
      });
    },
    getLeftList() {
    },
    handleNodeClick(res) {
      console.log(res);
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
      this.sure = this.sure + 1; //向form表单发送提交通知

      // 保存提交
      // this.$refs["dataForm"].validate(valid => {
      //   if (valid) {
      //     callservesure.add(this.temp).then(() => {
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
    handleUpdate(row) {
      // 弹出编辑框
      // this.temp = Object.assign({}, row); // copy obj
      callservesure.getForm(row.serviceOrderId).then(response => {
        this.formValue = response.result;
        console.log(this.formValuesss)
        this.dialogStatus = "update";
        this.FormUpdate = true;
      });
    },
    closeDia(a) {
      if (a === 1) {
        // this.FormUpdate = false
        this.getList();
      }

      this.loadingBtn = false;
      this.dialogFormVisible = false;
    },
    updateData() {
      // this.sure = this.sure + 1; //向form表单发送提交通知
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
  ::v-deep.el-table__expanded-cell{
    padding:0 0 0 50px;
  }
}
.bg-head {
  ::v-deep .el-table thead {
    color: green;
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




