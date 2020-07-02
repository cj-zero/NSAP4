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
        <permission-btn moduleName="certinfos" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
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
          style="width: 100%;"
          @row-click="rowClick"
          @selection-change="handleSelectionChange"
        >
          <el-table-column type="selection" align="center" width="55"></el-table-column>

          <el-table-column prop="id" align="center" label="id" min-width="50" show-overflow-tooltip></el-table-column>
          <el-table-column
            prop="name"
            label="应用名称"
            min-width="120"
            align="center"
            show-overflow-tooltip
          ></el-table-column>
          <el-table-column prop="appSecret" label="应用密匙" align="center" show-overflow-tooltip></el-table-column>
          <el-table-column prop="appKey" width='100px' align="center" label="AppKey" show-overflow-tooltip></el-table-column>
          <el-table-column
            prop="description"
            align="center"
            label="应用描述"
            min-width="120"
            show-overflow-tooltip
          ></el-table-column>
          <el-table-column prop="icon" label="应用图标" align="center" show-overflow-tooltip>
            <template slot-scope="scope">
              <span>{{getUrl(scope.row.id)}}</span>
              <!-- <span>{{scope.row.id}}</span> -->
            </template>
          </el-table-column>
          <el-table-column label="是否可用" align="center" width='100px' show-overflow-tooltip>
            <template slot-scope="scope">
              <el-link
                size="mini"
                :type="`${scope.row.disable===false?'danger':'success'}`"
              >{{scope.row.disable?'是':'否'}}</el-link>
            </template>
          </el-table-column>
          <el-table-column prop="createTime" align="center" label="创建日期" show-overflow-tooltip></el-table-column>
          <el-table-column prop="createUser" align="center" width='100px' label="创建人" show-overflow-tooltip></el-table-column>
          <el-table-column
            align="center"
            label="操作"
            width="120"
            class-name="small-padding fixed-width"
          >
            <template slot-scope="scope">
              <el-button type="primary" size="mini" @click="handleUpdate(scope.row)">编辑</el-button>
              <!-- <el-button
                v-if="scope.row.disable!=true"
                size="mini"
                type="danger"
                @click="handleModifyStatus(scope.row,true)"
              >停用</el-button>-->
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
      <el-dialog
        v-el-drag-dialog
        width="500px"
        :title="textMap[dialogStatus]"
        :visible.sync="dialogFormVisible"
      >
        <el-form
          :rules="rules"
          ref="dataForm"
          :model="temp"
          label-position="right"
          label-width="80px"
        >
          <el-form-item size="small" :label="'应用ID'" v-if="dialogStatus==='update'">
            <el-input v-model="temp.id"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'应用名称'" prop="name">
            <el-input v-model="temp.name"></el-input>
          </el-form-item>

          <el-form-item size="small" :label="'应用描述'">
            <el-input v-model="temp.description"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'应用密匙'">
            <el-input v-model="temp.appSecxet"></el-input>
          </el-form-item>
          <el-form-item size="small" :label="'appKey'">
            <el-input v-model="temp.appKey"></el-input>
          </el-form-item>
          <!-- <el-form-item size="small" :label="'应用图标'">
              <el-autocomplete
               suffix-icon="el-icon-caret-bottom"
              style="width:100%;"
              class="inline-input"
              v-model="temp.icon"
              :fetch-suggestions="querySearch"
              placeholder="请输入应用图标"
             
            >
              <i
    :class="[temp.icon?temp.icon:'']"
    slot="suffix">
  </i>
  </el-autocomplete>
          </el-form-item>-->
          <el-form-item size="small" :label="'是否可用'">
            <el-switch v-model="temp.disable" active-text="是" inactive-text="否"></el-switch>
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
import * as certinfos from "@/api/appmanager";
import { getInfo } from "@/api/login";
import { timeToFormat } from "@/utils";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: "certinfos",
  components: { Sticky, permissionBtn, Pagination },
  directives: {
    waves,
    elDragDialog
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
        limit: 20,
        key: undefined,
        appId: undefined
      },
      statusOptions: [
        { key: 1, display_name: "停用" },
        { key: 0, display_name: "正常" }
      ],

      temp: {
        name: "",
        id: "",
        description: "",
        appSecxet: "",
        appKey: "",
        disable: true,
        createTime: "",
        returnUrl: null,
        createUser: "",
        icon: ""
      },
      dialogFormVisible: false,
      dialogStatus: "",
      textMap: {
        update: "编辑",
        create: "添加"
      },
      restaurants: [],
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
  mounted() {
    this.restaurants = this.loadAll();
    this.getList();
  },
  created() {
    
  },
  methods: {
    getUrl(result) {
      let url = ''
        certinfos
        .getImgUrl(result)
        .then(response => {
          this.listLoading = false;
          url = response .returnUrl
        })
        .catch(() => {
          url = ''
        });
      return url?url:'图片不存在';
    },
    loadAll() {
      return [
        { id: 0, value: "el-icon-s-tools", label: "设置" },
        { id: 1, value: "el-icon-question", label: "问题" },
        { id: 2, value: "el-icon-info", label: "详情" },
        { id: 3, value: "el-icon-circle-plus", label: "添加" },
        { id: 4, value: "el-icon-upload", label: "上传" },
        { id: 5, value: "el-icon-bell", label: "消息" },
        { id: 6, value: "el-icon-s-operation", label: "菜单" },
        { id: 7, value: "el-icon-s-custom", label: "个人中心" },
        { id: 8, value: "el-icon-date", label: "日期" },
        { id: 9, value: "el-icon-edit-outline", label: "编辑" },
        { id: 10, value: "el-icon-folder-opened", label: "文件" }
      ];
    },
    createFilter(queryString) {
      return restaurant => {
        return (
          restaurant.value.toLowerCase().indexOf(queryString.toLowerCase()) ===
          0
        );
      };
    },
    querySearch(queryString, cb) {
      var restaurants = this.restaurants;
      var results = queryString
        ? restaurants.filter(this.createFilter(queryString))
        : restaurants;
      // 调用 callback 返回建议列表的数据
      cb(results);
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
    },
    onBtnClicked: function(domId) {
      console.log("you click:" + domId);
      switch (domId) {
        case "btnAdd":
          this.handleCreate();
          break;
        case "btnEdit":
          if (this.multipleSelection.length !== 1) {
            this.$message({
              message: "只能选中一个进行编辑",
              type: "error"
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
      certinfos.getList(this.listQuery).then(response => {
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
        appSecxet: "",
        appKey: "",
        disable: true,
        createTime: "",
        createUser: ""
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

    async createData() {
      // 保存提交
      this.$refs["dataForm"].validate(valid => {
        if (valid) {
          this.temp.createTime = timeToFormat("ymd");
          this.temp.icon = "";
          this.temp.returnUrl = null;
          getInfo().then(res => {
            this.temp.createUser = res.result;
            certinfos
              .add(this.temp)
              .then(() => {
                this.list.unshift(this.temp);
                this.dialogFormVisible = false;
                this.$notify({
                  title: "成功",
                  message: "创建成功",
                  type: "success",
                  duration: 2000
                });
              })
              .catch(error => {
                this.$notify.error({
                  title: "创建失败",
                  message: error.title,
                  duration: 2000
                });
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
          certinfos.update(tempData).then(() => {
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
      certinfos.del(rows.map(u => u.id)).then(() => {
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
