<template>
  <div>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <permission-btn moduleName="problemtypes" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
        <Search 
          :listQuery="listQuery"
          :config="searchConfig"
          @search="onSearch"
          ></Search>
      </div>
    </sticky>
    <el-card v-loading="listLoading" shadow="never" class="card-body-none fh" style="height:100%;overflow-y:auto;">
      <el-tree
        ref="tree"
        :default-expanded-keys="expandedKeys"
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
        @closed="closeDialog"
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
import { getList, add, update, getDetail, deleteAll } from "@/api/serve/knowledge";
import waves from "@/directive/waves"; // 水波纹指令
import Search from '@/components/Search'
import permissionBtn from "@/components/PermissionBtn";
import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: "knowledge",
  components: { permissionBtn, Search },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: "", // 列表checkbox选中的值
      multipleSelectList: [], // 选择列表
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
      listQuerySearch: {
        key: ''
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
      ],
      expandedKeys: [], // 默认展开的数组id
    };
  },
  created() {
    this._getList();
  },
  watch: {
    expandedKeys: {
      deep: true,
      handler (val) {
        console.log(val, 'expandedKeys')
      }
    }
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'key', component: { attrs: { style: { width: '150px' },  placeholder: '请输入内容' } } },
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } }
      ]
    }
  },
  methods: {
    onSearch () {
      this.listQuery.page = 1
      this._getList()
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
          if (!this.multipleSelection) {
            this.$message({
              message: "请选择删除项",
              type: "error"
            });
            return;
          }
          this.handleDelete(this.multipleSelectList);
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
        console.log(this.listQuerySearch.key, !!this.listQuery.key)
        this.expandedKeys = []
        this.modulesTree = this._normalizeList(result)
        this.listLoading = false
        console.log(this.modulesTree, 'moduleTree')
      }).catch(() => {
        this.listLoading = false
        this.$message.error('获取列表信息失败!')
      })
    },
    _normalizeList (data, level = 0) {
      return data.map(dataItem => {
        let { item, children } = dataItem
        dataItem = { ...item, children, disabled: Number(item.type) <= 2 }
        if (level < 2 && !this.listQuery.key) {
          this.expandedKeys.push(dataItem.id)
        } else if (this.listQuery.key) {
          this.expandedKeys.push(dataItem.id)
        }
        if (children && children.length) {
          dataItem.children = this._normalizeList(children, level + 1)
        }
        return dataItem
      })
    },
    handleNodeClick (row, node, component) {
      console.log(row, node, component)
      this.multipleSelection = row;
    },
    handleCheckChange (row, val) {
      console.log(row, val, 'checkChange')
      // if (isSelected) {
      //   this.multipleSelectList.push(row)
      // } else {
      //   let index = this.multipleSelectList.findIndex(item => item === row)
      //   this.multipleSelectList.splice(index, 1)
      // }
      console.log(val.checkedKeys, 'check keys')
      this.multipleSelectList = val.checkedKeys
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
        let { parentId, type, name, content, cascadeId, parentName, id } = res.result
        this.temp = {
          name,
          content,
          type,
          parentId,
          cascadeId,
          parentName,
          id
        }
        this.$refs.myDialog.open()
      }).catch(() => this.$message.error('获取详情失败'))
    },
    async confirm () {
      let isValid = await this.$refs.dataForm.validate()
      console.log(isValid, 'isValid')
      if (isValid) {
        this.dialogTitle === '新增' // 判断是编辑还是新增
          ? this._addData()
          : this._updateData()
      }
      
    },
    _addData () {
      let { type, name, id } = this.multipleSelection
      this.temp.parentId = id
      this.temp.type = type + 1 // 新增就是新增下一级
      this.temp.parentName = name
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
      this.$confirm('确定删除?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        this.listLoading = true
        deleteAll([this.multipleSelection.id]).then(() => {
          this.$message({
            type: 'success',
            message: '删除成功!'
          })
          this.listLoading = false
          this._getList()
        }).catch(() => {
          this.listLoading = false
          this.$message.error('删除失败')
        })
      })
    }
  }
};
</script>
<style lang="scss" scoped>
.dialog-mini .el-select {
  width: 100%;
}
::v-deep .el-tree-node__content {
  border-top: 1px solid rgba(0, 0, 0, .15);
}
</style>
