<template>
<div>
  <sticky :className="'sub-navbar'">
    <div class="filter-container">
      <el-input @keyup.enter.native="handleFilter" size="mini"  style="width: 200px;" class="filter-item" :placeholder="'名称'" v-model="listQuery.key">
      </el-input>
      
      <el-button class="filter-item" size="mini"  v-waves icon="el-icon-search" @click="handleFilter">搜索</el-button>
      <permission-btn moduleName='solutions' size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
    </div>
     </sticky>
      <div class="app-container ">
     <div class="bg-white">
    <el-table  ref="mainTable"  :key='tableKey' :data="list" v-loading="listLoading" border fit highlight-current-row
      style="width: 100%;" @row-click="rowClick"  @selection-change="handleSelectionChange">
			
       <el-table-column type="selection" align="center"  width="55"></el-table-column>

      
               <el-table-column prop="id" label="Id" show-overflow-tooltip></el-table-column>
            <el-table-column min-width="80px" label="SltCode">
            <template slot-scope="scope">
              <span>{{scope.row.sltCode|filterInt}}</span>
            </template>
          </el-table-column>
               <el-table-column prop="subject" label="Subject" show-overflow-tooltip></el-table-column>
               <el-table-column prop="cause" label="Cause" show-overflow-tooltip></el-table-column>
               <el-table-column prop="symptom" label="Symptom" show-overflow-tooltip></el-table-column>
               <el-table-column prop="descriptio" label="Descriptio" show-overflow-tooltip></el-table-column>
            <el-table-column min-width="80px" label="Status">
            <template slot-scope="scope">
              <span>{{scope.row.status|filterInt}}</span>
            </template>
          </el-table-column>
               <el-table-column prop="createUserId" label="CreateUserId" show-overflow-tooltip></el-table-column>
               <el-table-column prop="createUserName" label="CreateUserName" show-overflow-tooltip></el-table-column>
               <el-table-column prop="createTime" label="CreateTime" show-overflow-tooltip></el-table-column>
               <el-table-column prop="updateUserId" label="UpdateUserId" show-overflow-tooltip></el-table-column>
               <el-table-column prop="updateTime" label="UpdateTime" show-overflow-tooltip></el-table-column>
               <el-table-column prop="updateUserName" label="UpdateUserName" show-overflow-tooltip></el-table-column>


      <el-table-column align="center" label="操作" width="230" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button  type="primary" size="mini" @click="handleUpdate(scope.row)">编辑</el-button>
          <el-button v-if="scope.row.disable!=true" size="mini" type="danger" @click="handleModifyStatus(scope.row,true)">停用</el-button>
        </template>
      </el-table-column>
    </el-table>
		<pagination v-show="total>0" :total="total" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="handleCurrentChange" />
	</div>

    <el-dialog v-el-drag-dialog :destroy-on-close="true"  class="dialog-mini" width="500px" :title="textMap[dialogStatus]" :visible.sync="dialogFormVisible">
      <el-form :rules="rules" ref="dataForm" :model="temp" label-position="right" label-width="100px">
      
              <el-form-item size="small" label="Id" prop="id">
                <el-input v-model="temp.id"></el-input>
              </el-form-item>
            <el-form-item size="small" label="SltCode">
              <el-select class="filter-item" v-model="temp.sltCode" placeholder="Please select">
                 <el-option v-for="item in  statusOptions" :key="item.key" :label="item.display_name" :value="item.key">
                </el-option>
              </el-select>
            </el-form-item>
              <el-form-item size="small" label="Subject" prop="subject">
                <el-input v-model="temp.subject"></el-input>
              </el-form-item>
              <el-form-item size="small" label="Cause" prop="cause">
                <el-input v-model="temp.cause"></el-input>
              </el-form-item>
              <el-form-item size="small" label="Symptom" prop="symptom">
                <el-input v-model="temp.symptom"></el-input>
              </el-form-item>
              <el-form-item size="small" label="Descriptio" prop="descriptio">
                <el-input v-model="temp.descriptio"></el-input>
              </el-form-item>
            <el-form-item size="small" label="Status">
              <el-select class="filter-item" v-model="temp.status" placeholder="Please select">
                 <el-option v-for="item in  statusOptions" :key="item.key" :label="item.display_name" :value="item.key">
                </el-option>
              </el-select>
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
import * as solutions from '@/api/solutions'
import waves from '@/directive/waves' // 水波纹指令
import Sticky from '@/components/Sticky'
import permissionBtn from '@/components/PermissionBtn'
import Pagination from '@/components/Pagination'
import elDragDialog from '@/directive/el-dragDialog'
export default {
  name: 'solutions',
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
      statusOptions: [
        { key: 1, display_name: '停用' },
        { key: 0, display_name: '正常' }
      ],
      temp: {
        id: '', // Id
        sltCode: '', // SltCode
        subject: '', // Subject
        cause: '', // Cause
        symptom: '', // Symptom
        descriptio: '', // Descriptio
        status: '', // Status
        extendInfo: '' // 其他信息,防止最后加逗号，可以删除
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
      downloadLoading: false
    }
  },
  filters: {
    filterInt(val) {
      switch (val) {
        case null:
          return ''
        case 1:
          return '状态1'
        case 2:
          return '状态2'
        default:
          return '默认状态'
      }
    },
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
  },
  methods: {
    rowClick(row) {
      this.$refs.mainTable.clearSelection()
      this.$refs.mainTable.toggleRowSelection(row)
    },
    handleSelectionChange(val) {
      this.multipleSelection = val
    },
    onBtnClicked: function(domId) {
      console.log('you click:' + domId)
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
      solutions.getList(this.listQuery).then(response => {
        this.list = response.data
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
         id: '', 
         sltCode: '', 
         subject: '', 
         cause: '', 
         symptom: '', 
         descriptio: '', 
         status: '', 
         createUserId: '', 
         createUserName: '', 
         createTime: '', 
         updateUserId: '', 
         updateTime: '', 
         updateUserName: '', 
        extendInfo: ''
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
          solutions.add(this.temp).then(() => {
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
          solutions.update(tempData).then(() => {
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
      solutions.del(rows.map(u => u.id)).then(() => {
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
