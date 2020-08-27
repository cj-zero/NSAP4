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
          v-model="listQuery.CardCodeOrCardName"
        ></el-input> -->
        <el-input
          @keyup.enter.native="handleFilter"
          size="mini"
          style="width: 200px;margin:0 10px;"
          class="filter-item"
          placeholder="按客户代码/客户名称"
          v-model="listQuery.CardCodeOrCardName"
        ></el-input> 
         <el-input
          @keyup.enter.native="handleFilter"
          size="mini"
          style="width: 200px;"
          class="filter-item"
          placeholder="按序列号查询"
          v-model="listQuery.ManufSN"
        ></el-input>
         <el-button
          class="filter-item"
          size="mini"
          v-waves
          icon="el-icon-search"
          @click="handleFilter"
        >搜索</el-button>
        <permission-btn moduleName="businesspartner" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <div class="app-container">
      <div class="bg-white">
        <el-table
          ref="mainTable"
          :key="tableKey"
          :data="list"
          v-loading="listLoading"
          border
          fit
          highlight-current-row
          align="left"
          style="width: 100%;"
          @row-click="rowClick"
          @selection-change="handleSelectionChange"
        >
          <!-- <el-table-column type="selection" align="center" width="55"></el-table-column> -->
          <el-table-column prop="cardCode" label="客户代码" align="left" show-overflow-tooltip></el-table-column>
          <el-table-column
            prop="cardName"
            label="客户名称"
            min-width="120"
            align="left"
            show-overflow-tooltip
          ></el-table-column>
          <el-table-column prop="address" label="客户地址" align="left" show-overflow-tooltip></el-table-column>
     <el-table-column align="left" label="状态冻结" width="120">
        <template slot-scope="scope">
          <span
            :class="[scope.row.frozenFor=='N'?'greenColro':'redColor']"
          >{{scope.row.frozenFor=="N"?"正常":'冻结'}}</span>
        </template>
      </el-table-column>
      <el-table-column prop="cntctPrsn" label="客户联系人" align="left" show-overflow-tooltip width="100"></el-table-column>
      <el-table-column label="客户电话" align="left" show-overflow-tooltip>
        <template slot-scope="scope">
          {{ scope.row.cellular ? scope.row.cellular : scope.row.phone1 }}
        </template>
      </el-table-column>
      <!-- <el-table-column prop="slpName" label="业务员" align="left" show-overflow-tooltip></el-table-column> -->
      <el-table-column prop="slpName" label="销售员" align="left" show-overflow-tooltip></el-table-column>
      <el-table-column prop="technician" label="售后技术员" align="left" show-overflow-tooltip width="100"></el-table-column>
       <el-table-column align="right" label="科目余额" width="120">
        <template slot-scope="scope">
          <!-- <span :class="[scope.row.balance>=0?'redColor':'greenColro']">{{scope.row.balance}}0000</span> -->
          <span :class="[ scope.row.balance >= 0 ? '' : 'redColor' ]">{{scope.row.balance}}</span> 
        </template>
      </el-table-column>
          <el-table-column prop="updateDate" label="更新时间" show-overflow-tooltip></el-table-column>
        </el-table>
        <pagination
          v-show="total>0"
          :total="total"
          :page.sync="listQuery.page"
          :limit.sync="listQuery.limit"
          @pagination="handleCurrentChange"
        />
      </div>
      <el-dialog
        v-el-drag-dialog
        width="800px"
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
          <el-form-item size="small" :label="'客户名称'">
            <el-input v-model="temp.cardName"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'客户代码'">
            <el-input v-model="temp.cardCode"></el-input>
          </el-form-item>

          <el-form-item size="small" :label="'客户地址'">
            <el-input v-model="temp.address"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'客户电话'">
            <el-input v-model="temp.cellular"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'groupName'">
            <el-input v-model="temp.groupName"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'cntctPrsn'">
            <el-input v-model="temp.cntctPrsn"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'slpName'">
            <el-input v-model="temp.slpName"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'更新时间'">
            <el-input v-model="temp.updateDate"></el-input>
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
import * as businesspartner from "@/api/businesspartner";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: "businesspartner",
  components: { Sticky, permissionBtn, Pagination },
  directives: {
    waves,
    elDragDialog,
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      tableKey: 0,
      list: null,
      flowList: null,
      flowList_value: "",
      pullDownList_value: "",
      pullDownList: null,
      total: 0,
      listLoading: true,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 50,
        ManufSN:'',
        key: undefined,
        appId: undefined,
        CardCodeOrCardName:''
      },
      statusOptions: [
        { key: 1, display_name: "停用" },
        { key: 0, display_name: "正常" },
      ],

      temp: {
        cardName: "",
        cardCode: "",
        address: "",
        cellular: "",
        groupName: "",
        cntctPrsn: "",
        slpName: "",
        updateDate: "",
      },
      dialogFormVisible: false,
      dialogStatus: "",
      textMap: {
        update: "编辑",
        create: "添加",
      },
      restaurants: [],
      dialogPvVisible: false,
      pvData: [],
      rules: {
        appId: [
          { required: true, message: "必须选择一个应用", trigger: "change" },
        ],
        name: [{ required: true, message: "名称不能为空", trigger: "blur" }],
      },
      downloadLoading: false,
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
        true: "color-danger",
      };
      return statusMap[disable];
    },
  },
  mounted() {},
  created() {
    this.getList();
  },
  methods: {
    getUrl(result) {
      console.log(result);
    },

    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
    },
    onBtnClicked: function (domId) {
      console.log("you click:" + domId);
      switch (domId) {
        case "btnAdd":
          this.$message({
            message: "暂无数据",
            type: "warning",
          });
          //   this.handleCreate();
          break;
        case "btnEdit":
          this.$message({
            message: "暂无数据",
            type: "warning",
          });
          //   if (this.multipleSelection.length !== 1) {
          //     this.$message({
          //       message: "请点击需要编辑的数据",
          //       type: "error"
          //     });
          //     return;
          //   }
          //   this.handleUpdate(this.multipleSelection[0]);
          break;
        case "btnDel":
          if (this.multipleSelection.length < 1) {
            this.$message({
              message: "至少删除一个",
              type: "error",
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
      businesspartner.getList(this.listQuery).then((response) => {
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
        type: "success",
      });
      row.disable = disable;
    },
    resetTemp() {
      this.temp = {
        cardName: "",
        cardCode: "",
        address: "",
        cellular: "",
        groupName: "",
        cntctPrsn: "",
        slpName: "",
        updateDate: "",
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
      this.$refs["dataForm"].validate((valid) => {
        if (valid) {
          businesspartner.add(this.temp).then(() => {
            this.list.unshift(this.temp);
            this.dialogFormVisible = false;
            this.$notify({
              title: "成功",
              message: "创建成功",
              type: "success",
              duration: 2000,
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
      this.$refs["dataForm"].validate((valid) => {
        if (valid) {
          const tempData = Object.assign({}, this.temp);
          businesspartner.update(tempData).then(() => {
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
              duration: 2000,
            });
          });
        }
      });
    },
    handleDelete(rows) {
      // 多行删除
      businesspartner.del(rows.map((u) => u.id)).then(() => {
        this.$notify({
          title: "成功",
          message: "删除成功",
          type: "success",
          duration: 2000,
        });
        rows.forEach((row) => {
          const index = this.list.indexOf(row);
          this.list.splice(index, 1);
        });
      });
    },
  },
};
</script>
<style>
.dialog-mini .el-select {
  width: 100%;
}
.redColor {
  color: red;
}
.greenColro {
  color: green;
}
</style>
