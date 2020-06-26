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
            <el-table-column type="expand">
              <template slot-scope="props">
                <el-form label-position="left" inline class="demo-table-expand">
                  <el-form-item label="所属应用">
                    <span>{{ props.row.belongApp }}</span>
                  </el-form-item>
                  <el-form-item label="所属应用ID">
                    <span>{{ props.row.belongAppId }}</span>
                  </el-form-item>
                  <el-form-item label="上传人">
                    <span>{{ props.row.createUserId }}</span>
                  </el-form-item>
                  <el-form-item label="文件类型">
                    <span>{{ props.row.fileType }}</span>
                  </el-form-item>
                  <el-form-item label="描述">
                    <span>{{ props.row.description }}</span>
                  </el-form-item>
                  <el-form-item label="扩展名称">
                    <span>{{ props.row.enable }}</span>
                  </el-form-item>
                       <!-- <el-table-column  label="id" >
                  <span>{{ props.row.id }}</span>
               </el-table-column> -->
                  <el-form-item label="商品描述">
                    <span>{{ props.row.desc }}</span>
                  </el-form-item>
          

               <!-- <el-table-column label="是否可用" align="center" >
            <template slot-scope="scope">
              <el-link
                size="mini"
                :type="`${scope.row.enable===false?'danger':'success'}`"
              >{{scope.row.enable?'是':'否'}}</el-link>
            </template>
          </el-table-column> -->
                </el-form>
              </template>
            </el-table-column>
                   <el-table-column prop="id"  label="id" show-overflow-tooltip>
               </el-table-column>
          <el-table-column
            prop="fileName"
            label="文件名称"
            width="120"
            align="center"
            show-overflow-tooltip
          ></el-table-column>
          <el-table-column prop="filePath" label="文件路径" align="center" show-overflow-tooltip></el-table-column>
          <el-table-column prop="fileSize" width="100" label="文件大小" align="center" show-overflow-tooltip>
            <template slot-scope="scope">
              <span>{{scope.row.fileSize}}kB</span>
            </template>
          </el-table-column>
  
          <el-table-column prop="createUserName" align="center" width="100" label="上传人姓名" show-overflow-tooltip></el-table-column>
          <el-table-column prop="createTime" align="center" sortable label="上传时间" show-overflow-tooltip></el-table-column>
          <el-table-column prop="thumbnail" align="center" label="缩略图" show-overflow-tooltip>
            <template slot-scope="scope">
                <img style="width:50px;height:50px;" @click="handlePreviewFile(baseURL +'/files/Download/'+scope.row.id)" :src="baseURL +'/files/Download/'+scope.row.id" alt="">

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
        
        <el-upload
          :action="baseURL +'/Files/Upload/'"
          list-type="picture-card"
          @on-success='submit1'
          :auto-upload="true"
        >
          <i slot="default" class="el-icon-plus"></i>
          <div slot="file" slot-scope="{file}">
            <img class="el-upload-list__item-thumbnail" :src="file.url" alt />
            <span class="el-upload-list__item-actions">
              <span class="el-upload-list__item-preview" @click="handlePictureCardPreview(file)">
                <i class="el-icon-zoom-in"></i>
              </span>
              <span
                v-if="!disabled"
                class="el-upload-list__item-delete"
                @click="handleDownload(file)"
              >
                <i class="el-icon-download"></i>
              </span>
              <span
                v-if="!disabled"
                class="el-upload-list__item-delete"
                @click="handleRemove(file)"
              >
                <i class="el-icon-delete"></i>
              </span>
            </span>
          </div>
        </el-upload>
        <div slot="footer">
          <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
          <el-button size="mini" v-if="dialogStatus=='create'" type="primary" @click="upFile">确认</el-button>
          <el-button size="mini" v-else type="primary" @click="updateData">确认</el-button>
        </div>
      </el-dialog>
        <Model
      :visible="previewVisible"
      @on-close="previewVisible = false"
      ref="formPreview"
      width="600px"
      form
    >
      <img :src="previewUrl" alt="" style="display: block;width: 80%;margin: 0 auto;">
      <template slot="action">
        <el-button size="mini" @click="previewVisible = false">关闭</el-button>
      </template>
    </Model>
    </div>
  </div>
</template>

<script>
import * as certinfos from "@/api/filemanager";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
// import Sortable from 'sortablejs'
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
// import Upload from '@/components/Formcreated/components/ImageUpload'
import elDragDialog from "@/directive/el-dragDialog";
import Model from '@/components/Formcreated/components/Model'
export default {
  name: "certinfos",
  components: { Sticky, permissionBtn, Pagination ,Model },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      multipleSelection: [], // 列表checkbox选中的值
      tableKey: 0,
      previewUrl:'',
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
      dialogImageUrl: "",
      dialogVisible: false,
      disabled: false,
      statusOptions: [
        { key: 1, display_name: "停用" },
        { key: 0, display_name: "正常" }
      ],
previewVisible: false,
      temp: {
        appSecxet: "",
        appKey: "",
        icon: "",
        disable: "",
        createTime: "",
        createUser: ""
      },
      dialogFormVisible: false,
      dialogStatus: "",
      textMap: {
        update: "编辑",
        create: "文件上传"
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
  },
  created() {
    this.getList();
  },
  methods: {
    handleRemove(file) {
      console.log(file);
    },
    handlePictureCardPreview(file) {
      this.dialogImageUrl = file.url;
      this.dialogVisible = true;
    },
    handleDownload(file) {
      console.log(file);
    },
    submit1(res) {
      console.log(res);
    },
    upFile() {},
      //列拖拽
     handlePreviewFile(item) {
      // if (!item.isImg) {
      //  window.location.href = `${item.url}?X-Token=${this.$store.state.user.token}`
      //   return
      // }
      this.previewVisible = true
      this.previewUrl = item
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
        console.log(this.listQuery);
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
        icon: "",
        disable: "",
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
        // this.$refs["dataForm"].clearValidate();
      });
    },
    createData() {
      // 保存提交
      this.$refs["dataForm"].validate(valid => {
        if (valid) {
          certinfos.add(this.temp).then(() => {
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
        // this.$refs["dataForm"].clearValidate();
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
<style scoped>
.dialog-mini .el-select {
  width: 100%;
}
  .demo-table-expand {
    font-size: 0;
  }
  .demo-table-expand label {
    width: 90px;
    color: #99a9bf;
  }
  .demo-table-expand .el-form-item {
    margin-right: 0;
    margin-bottom: 0;
    width: 50%;
  }
</style>
