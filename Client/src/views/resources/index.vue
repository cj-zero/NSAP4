<template>
<div class="flex-column">
  <sticky :className="'sub-navbar'">
    <div class="filter-container">
      <el-input @keyup.enter.native="handleFilter" size="mini"  style="width: 200px;" class="filter-item" :placeholder="'名称'" v-model="listQuery.key">
      </el-input>
      
      <el-select clearable size="mini"  class="filter-item" style="width: 200px" v-model="listQuery.appId" :placeholder="'所属应用'">
       <el-option value="" label="">请选择</el-option>
				<el-option v-for="item in  apps" :key="item.id" :label="item.name+'('+item.id+')'" :value="item.id">
        </el-option>
      </el-select>

      <el-button class="filter-item" size="mini"  v-waves icon="el-icon-search" @click="handleFilter">搜索</el-button>
      <permission-btn moduleName='resources' size="mini" v-on:btn-event="onBtnClicked"></permission-btn>

      <el-checkbox class="m-l-15" size="mini" @change='tableKey=tableKey+1' v-model="showDescription">描述</el-checkbox>
    </div>
     </sticky>
     <div class="app-container flex-item">
		
     <div class="bg-white" style="height: 100%;">

     <el-table  ref="mainTable" height="calc(100% - 52px)" :key='tableKey' :data="list" v-loading="listLoading" border fit highlight-current-row
      style="width: 100%;" @row-click="rowClick"  @selection-change="handleSelectionChange">
			
       <el-table-column type="selection" align="center"  width="55"></el-table-column>

      <template v-for="(headerItem,index) in headerList">
          <el-table-column  :label="headerItem.description" min-width="120px" :key="index">
            <template slot-scope="scope">
              <span>{{scope.row[headerItem.key]}}</span>
            </template>
          </el-table-column>
      </template>

      <el-table-column align="center" :label="'操作'" width="230" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button  type="primary" size="mini" @click="handleUpdate(scope.row)">编辑</el-button>
          <el-button v-if="scope.row.disable!=true" size="mini" type="danger" @click="handleModifyStatus(scope.row,true)">停用</el-button>
        </template>
      </el-table-column>
    </el-table>
		<pagination v-show="total>0" :total="total" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="handleCurrentChange" />
	</div>
    <el-dialog v-el-drag-dialog   class="dialog-mini" width="500px" :title="textMap[dialogStatus]" :visible.sync="dialogFormVisible">
      <el-form :rules="rules" ref="dataForm" :model="temp" label-position="right" label-width="100px">
        <el-form-item size="small" :label="'所属应用'" prop="appId">
          <el-select class="filter-item" v-model="temp.appId" placeholder="请选择" @change="changeApp">
            <el-option v-for="item in  apps" :key="item.id" :label="item.name" :value="item.id">
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item size="small" :label="'Id'" prop="id">
          <el-input v-model="temp.id"></el-input>
        </el-form-item>

        <el-form-item size="small" :label="'名称'" prop="name">
          <el-input v-model="temp.name"></el-input>
        </el-form-item>
        <el-form-item size="small" :label="'状态'">
          <el-select class="filter-item" v-model="temp.disable" placeholder="Please select">
             <el-option v-for="item in  statusOptions" :key="item.key" :label="item.display_name" :value="item.key">
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item size="small" :label="'描述'">
          <el-input type="textarea" :autosize="{ minRows: 2, maxRows: 4}" placeholder="Please input" v-model="temp.description">
          </el-input>
        </el-form-item>
      </el-form>
      <div slot="footer" >
        <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
        <el-button size="mini" v-if="dialogStatus=='create'" type="primary" @click="createData">确认</el-button>
        <el-button size="mini" v-else type="primary" @click="updateData">确认</el-button>
      </div>
    </el-dialog>
  </div>
</div>
 
</template>

<script>
import * as resources from '@/api/resources'
import * as applications from '@/api/applications'
import waves from '@/directive/waves' // 水波纹指令
import Sticky from '@/components/Sticky'
import permissionBtn from '@/components/PermissionBtn'
import Pagination from '@/components/Pagination'
import elDragDialog from '@/directive/el-dragDialog'
export default {
  name: 'resource',
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
      total: 0,
      listLoading: true,
      listQuery: { // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined
      },
      apps: [],
      statusOptions: [
        { key: true, display_name: '停用' },
        { key: false, display_name: '正常' }
      ],
      showDescription: false,
      temp: {
        id: undefined,
        description: '',
        name: '',
        appId: '',
        appName: '',
        disable: false
      },
      dialogFormVisible: false,
      dialogStatus: '',
      textMap: {
        update: '编辑',
        create: '添加'
      },
      dialogPvVisible: false,
      pvData: [],
      rules: {
        appId: [{ required: true, message: '必须选择一个应用', trigger: 'change' }],
        name: [{ required: true, message: '名称不能为空', trigger: 'blur' }]
      },
      downloadLoading: false,
      headerList: []
    }
  },
  filters: {
    statusFilter(disable) {
      const statusMap = {
        false: 'color-success',
        true: 'color-danger'
      }
      return statusMap[disable]
    }
  },
  created() {
    this.getList()
    applications.getList().then(response => {
      this.apps = response.data
    })
  },
  methods: {
    changeApp(value) {
      this.temp.appName = this.apps.find((e) => { // 把应用名称加上
        return e.id === value
      }).name
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection()
      this.$refs.mainTable.toggleRowSelection(row)
    },
    handleSelectionChange(val) {
      this.multipleSelection = val
    },
    onBtnClicked: function(domId) {
      // console.log('you click:' + domId)
      switch (domId) {
        case 'btnAdd':
          this.handleCreate()
          break
        case 'btnEdit':
          if (this.multipleSelection.length !== 1) {
            this.$message({
              message: '只能选中一个进行编辑',
              type: 'error'
            })
            return
          }
          this.handleUpdate(this.multipleSelection[0])
          break
        case 'btnDel':
          if (this.multipleSelection.length < 1) {
            this.$message({
              message: '至少删除一个',
              type: 'error'
            })
            return
          }
          this.handleDelete(this.multipleSelection)
          break
        default:
          break
      }
    },
    getList() {
      this.listLoading = true
      resources.getList(this.listQuery).then(response => {
        this.list = response.data
        response.columnHeaders.forEach((item) => {
          item.key = item.key.substring(0, 1).toLowerCase() + item.key.substring(1)
        })
        this.headerList = response.columnHeaders.filter(u => u.browsable)
        this.total = response.count
        this.listLoading = false
      })
    },
    handleFilter() {
      this.listQuery.page = 1
      this.getList()
    },
    handleSizeChange(val) {
      this.listQuery.limit = val
      this.getList()
    },
    handleCurrentChange(val) {
      this.listQuery.page = val.page
      this.listQuery.limit = val.limit
      this.getList()
    },
    handleModifyStatus(row, disable) { // 模拟修改状态
      this.$message({
        message: '操作成功',
        type: 'success'
      })
      row.disable = disable
    },
    resetTemp() {
      this.temp = {
        id: undefined,
        description: '',
        name: '',
        disable: false,
        appId: '',
        appName: ''
      }
    },
    handleCreate() { // 弹出添加框
      this.resetTemp()
      this.dialogStatus = 'create'
      this.dialogFormVisible = true
      this.$nextTick(() => {
        this.$refs['dataForm'].clearValidate()
      })
    },
    createData() { // 保存提交
      this.$refs['dataForm'].validate((valid) => {
        if (valid) {
          resources.add(this.temp).then(() => {
            this.list.unshift(this.temp)
            this.dialogFormVisible = false
            this.$notify({
              title: '成功',
              message: '创建成功',
              type: 'success',
              duration: 2000
            })
          })
        }
      })
    },
    handleUpdate(row) { // 弹出编辑框
      this.temp = Object.assign({}, row) // copy obj
      this.dialogStatus = 'update'
      this.dialogFormVisible = true
      this.$nextTick(() => {
        this.$refs['dataForm'].clearValidate()
      })
    },
    updateData() { // 更新提交
      this.$refs['dataForm'].validate((valid) => {
        if (valid) {
          const tempData = Object.assign({}, this.temp)
          resources.update(tempData).then(() => {
            for (const v of this.list) {
              if (v.id === this.temp.id) {
                const index = this.list.indexOf(v)
                this.list.splice(index, 1, this.temp)
                break
              }
            }
            this.dialogFormVisible = false
            this.$notify({
              title: '成功',
              message: '更新成功',
              type: 'success',
              duration: 2000
            })
          })
        }
      })
    },
    handleDelete(rows) { // 多行删除
      resources.del(rows.map(u => u.id)).then(() => {
        this.$notify({
          title: '成功',
          message: '删除成功',
          type: 'success',
          duration: 2000
        })
        rows.forEach(row => {
          const index = this.list.indexOf(row)
          this.list.splice(index, 1)
        })
      })
    }
  }
}
</script>
<style>
	.dialog-mini .el-select{
		width:100%;
	}
</style>
