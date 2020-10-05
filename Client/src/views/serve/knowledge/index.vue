<template>
  <div>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <permission-btn moduleName="problemtypes" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <el-card shadow="never" class="card-body-none fh" style="height:100%;overflow-y:auto;">
      <el-tree
        :props="defualtProps"
        :data="modulesTree"
        node-key="id"
        :expand-on-click-node="false"
        @node-click="handleNodeClick">
      </el-tree>
    </el-card>
    <div class="app-container">
      <my-dialog
        ref="myDialog"
        width="400px"
        :title="dialogTitle"
        :btnList="btnList"
        :loading="dialogLoading"
        :onClosed="closeDialog"
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
          <el-form-item size="small" label="内容" prop="description">
            <el-input v-model="temp.content"></el-input>
          </el-form-item>
        </el-form>
      </my-dialog>
      <!-- <el-dialog
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
      </el-dialog> -->
    </div>
  </div>
</template>

<script>
import { getList, add, update, getDetail } from "@/api/serve/knowledge";
import waves from "@/directive/waves"; // 水波纹指令
// import Pagination from '@/components/Pagination'
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import elDragDialog from "@/directive/el-dragDialog";
import MyDialog from '@/components/Dialog'
export default {
  name: "knowledge",
  components: { Sticky, permissionBtn, MyDialog },
  directives: {
    waves,
    elDragDialog,
    MyDialog
  },
  data() {
    return {
      multipleSelection: "", // 列表checkbox选中的值
      modulesTree: [],
      total: 0,
      dialogTitle: '',
      defualtProps: {
        label: 'name',
        children: 'children'
      },
      temp: { // 弹窗填写信息
        name: '',
        content: '',
        type: '',
        parentId: ''
      },
      listLoading: true,
      dialogLoading: false,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20
      },
      columns: [
        {
          text: "描述",
          value: "description",
          align:'left'
        }
      ],
      rules: {
        appId: [
          { required: true, message: "必须选择一个应用", trigger: "change" }
        ],
        name: [{ required: true, message: "名称不能为空", trigger: "blur" }]
      },
      btnList: [
        { btnText: '确认', handleClick: this.confirm },
        { btnText: '取消', handleClick: this.closeDialog }
      ]
    };
  },
  created() {
    this._getList();
  },

  methods: {
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
          if (!this.multipleSelection) {
            return this.$message({
              message: "请点击需要新增项的父节点",
              type: "error"
            });
          }
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
    _getList() {
      this.listLoading = true
      getList(this.listQuery).then(res => {
        console.log(res, 'resd')
        let { result } = res
        // this._normalizeList(result)
        this.modulesTree = this._normalizeList(result)
        this.listLoading = false
        console.log(this.modulesTree, 'moduleTree')
      }).catch(() => {
        this.listLoading = false
        this.$message.error('获取列表信息失败!')
      })
    },
    _normalizeList (data) {
      return data.map(dataItem => {
        let { item, children } = dataItem
        dataItem = { ...item, children }
        if (children && children.length) {
          dataItem.children = this._normalizeList(children)
        }
        return dataItem
      })
    },
    handleNodeClick (row, node, component) {
      console.log(row, node, component)
      this.multipleSelection = row;
    },
    closeDialog () {
      this.resetTemp()
      this.$refs.myDialog.close()
    },
    handleFilter() {
      this.listQuery.page = 1;
      this.getList();
    },
    resetTemp() {
      this.temp = {
        name: '',
        content: '',
        type: '',
        parentId: ''
      }
    },
    handleCreate(row) {
      // 弹出添加框
      if (Number(row.type) === 1 || Number(row.type) === 5) {
        return this.$message({
          type: 'warning',
          message: '当前选中项不可添加子节点'
        })
      }
      console.log(this.$refs, 'refs')
      this.dialogTitle = '新增'
      this.$refs.myDialog.open()
    },
    handleUpdate(row) {
      if (Number(row.type) === 1 || Number(row.type) === 2) {
        return this.$message({
          type: 'warning',
          message: '当前选中项不可进行编辑'
        })
      }
      this.dialogTitle = '编辑'
      let { id } = this.multipleSelection
      console.log(id, 'id')
      getDetail({ id }).then(res => {
        let { parentId, type, name, content, cascadeId, parentName } = res.result
        this.temp = {
          name,
          content,
          type,
          parentId,
          cascadeId,
          parentName
        }
        this.$refs.myDialog.open()
      }).catch(() => this.$message.error('获取详情失败'))
    },
    confirm () {
      this.dialogTitle === '新增' // 判断是编辑还是新增
        ? this._addData()
        : this._updateData()
    },
    _addData () {
      let { parentId, type, parentName } = this.multipleSelection
      this.temp.parentId = parentId
      this.temp.type = type + 1 // 新增就是新增下一级
      this.temp.parentName = parentName
      console.log(add, this.temp, 'this.temp')
      this.dialogLoading = true
      add(this.temp).then(() => {
        this.$message({
          type: 'success',
          message: '新增成功'
        })
        this._getList()
        this.dialogLoading = false
        this.closeDialog()
      }).catch(() => {
        this.dialogLoading = false
        this.$message.error('新增失败')
      })
    },
    _updateData () {
      this.dialogLoading = true
      update(this.temp).then(() => {
        this.$message({
          type: 'success',
          message: '编辑成功'
        })
        this._getList()
        this.dialogLoading = false
        this.closeDialog()
      }).catch(() => {
        this.dialogLoading = false
        this.$message.error('编辑失败')
      })
    },
    handleDelete() {
      // 多行删除
    }
  }
};
</script>
<style>
.dialog-mini .el-select {
  width: 100%;
}
</style>
