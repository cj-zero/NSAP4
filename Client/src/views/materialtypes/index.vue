<template>
<div>
  <sticky :className="'sub-navbar'">
    <div class="filter-container">
      <el-input @keyup.enter.native="handleFilter" size="mini"  style="width: 200px;" class="filter-item" :placeholder="'名称'" v-model="listQuery.key">
      </el-input>
      
      <el-button class="filter-item" size="mini"  v-waves icon="el-icon-search" @click="handleFilter">搜索</el-button>
      <permission-btn moduleName='materialtypes' size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
    </div>
     </sticky>
      <div class="app-container ">
     <div class="bg-white">
    <el-table  ref="mainTable"  :key='tableKey' :data="list" v-loading="listLoading" border fit highlight-current-row
      style="width: 100%;" @row-click="rowClick"  @selection-change="handleSelectionChange">
			
       <el-table-column type="selection" align="center"  width="55"></el-table-column>

      
               <el-table-column prop="id" label="Id" show-overflow-tooltip></el-table-column>
               <el-table-column prop="typeAlias" label="TypeAlias" show-overflow-tooltip></el-table-column>
               <el-table-column prop="typeName" label="TypeName" show-overflow-tooltip></el-table-column>
               <el-table-column prop="parentId" label="ParentId" show-overflow-tooltip></el-table-column>
               <el-table-column prop="typeLevel" label="TypeLevel" show-overflow-tooltip></el-table-column>
               <el-table-column prop="orderIdx" label="OrderIdx" show-overflow-tooltip></el-table-column>
               <el-table-column prop="codingExp" label="CodingExp" show-overflow-tooltip></el-table-column>
               <el-table-column prop="descExp" label="DescExp" show-overflow-tooltip></el-table-column>
               <el-table-column prop="valid" label="Valid" show-overflow-tooltip></el-table-column>
               <el-table-column prop="updTime" label="UpdTime" show-overflow-tooltip></el-table-column>
               <el-table-column prop="codeRuleFlag" label="CodeRuleFlag" show-overflow-tooltip></el-table-column>
               <el-table-column prop="userId" label="UserId" show-overflow-tooltip></el-table-column>
               <el-table-column prop="attachFlag" label="AttachFlag" show-overflow-tooltip></el-table-column>
            <el-table-column min-width="80px" label="MacTime">
            <template slot-scope="scope">
              <span>{{scope.row.macTime|filterInt}}</span>
            </template>
          </el-table-column>
               <el-table-column prop="macPrice" label="MacPrice" show-overflow-tooltip></el-table-column>
               <el-table-column prop="forBomAttFlag" label="ForBomAttFlag" show-overflow-tooltip></el-table-column>
            <el-table-column min-width="80px" label="type_id">
            <template slot-scope="scope">
              <span>{{scope.row.typeId|filterInt}}</span>
            </template>
          </el-table-column>
            <el-table-column min-width="80px" label="parent_id">
            <template slot-scope="scope">
              <span>{{scope.row.parentId|filterInt}}</span>
            </template>
          </el-table-column>


      <el-table-column align="center" label="操作" width="230" class-name="small-padding fixed-width">
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
      
              <el-form-item size="small" label="Id" prop="id">
                <el-input v-model="temp.id"></el-input>
              </el-form-item>
              <el-form-item size="small" label="TypeAlias" prop="typeAlias">
                <el-input v-model="temp.typeAlias"></el-input>
              </el-form-item>
              <el-form-item size="small" label="TypeName" prop="typeName">
                <el-input v-model="temp.typeName"></el-input>
              </el-form-item>
              <el-form-item size="small" label="ParentId" prop="parentId">
                <el-input v-model="temp.parentId"></el-input>
              </el-form-item>
              <el-form-item size="small" label="TypeLevel" prop="typeLevel">
                <el-input v-model="temp.typeLevel"></el-input>
              </el-form-item>
              <el-form-item size="small" label="OrderIdx" prop="orderIdx">
                <el-input v-model="temp.orderIdx"></el-input>
              </el-form-item>
              <el-form-item size="small" label="CodingExp" prop="codingExp">
                <el-input v-model="temp.codingExp"></el-input>
              </el-form-item>
              <el-form-item size="small" label="DescExp" prop="descExp">
                <el-input v-model="temp.descExp"></el-input>
              </el-form-item>
              <el-form-item size="small" label="Valid" prop="valid">
                <el-input v-model="temp.valid"></el-input>
              </el-form-item>
              <el-form-item size="small" label="UpdTime" prop="updTime">
                <el-input v-model="temp.updTime"></el-input>
              </el-form-item>
              <el-form-item size="small" label="CodeRuleFlag" prop="codeRuleFlag">
                <el-input v-model="temp.codeRuleFlag"></el-input>
              </el-form-item>
              <el-form-item size="small" label="UserId" prop="userId">
                <el-input v-model="temp.userId"></el-input>
              </el-form-item>
              <el-form-item size="small" label="AttachFlag" prop="attachFlag">
                <el-input v-model="temp.attachFlag"></el-input>
              </el-form-item>
            <el-form-item size="small" label="MacTime">
              <el-select class="filter-item" v-model="temp.macTime" placeholder="Please select">
                 <el-option v-for="item in  statusOptions" :key="item.key" :label="item.display_name" :value="item.key">
                </el-option>
              </el-select>
            </el-form-item>
              <el-form-item size="small" label="MacPrice" prop="macPrice">
                <el-input v-model="temp.macPrice"></el-input>
              </el-form-item>
              <el-form-item size="small" label="ForBomAttFlag" prop="forBomAttFlag">
                <el-input v-model="temp.forBomAttFlag"></el-input>
              </el-form-item>
            <el-form-item size="small" label="type_id">
              <el-select class="filter-item" v-model="temp.typeId" placeholder="Please select">
                 <el-option v-for="item in  statusOptions" :key="item.key" :label="item.display_name" :value="item.key">
                </el-option>
              </el-select>
            </el-form-item>
            <el-form-item size="small" label="parent_id">
              <el-select class="filter-item" v-model="temp.parentId" placeholder="Please select">
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
import * as materialtypes from '@/api/materialtypes'
import waves from '@/directive/waves' // 水波纹指令
import Sticky from '@/components/Sticky'
import permissionBtn from '@/components/PermissionBtn'
import Pagination from '@/components/Pagination'
import elDragDialog from '@/directive/el-dragDialog'
export default {
  name: 'materialtypes',
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
        typeAlias: '', // TypeAlias
        typeName: '', // TypeName
        parentId: '', // ParentId
        typeLevel: '', // TypeLevel
        orderIdx: '', // OrderIdx
        codingExp: '', // CodingExp
        descExp: '', // DescExp
        valid: '', // Valid
        updTime: '', // UpdTime
        codeRuleFlag: '', // CodeRuleFlag
        userId: '', // UserId
        attachFlag: '', // AttachFlag
        macTime: '', // MacTime
        macPrice: '', // MacPrice
        forBomAttFlag: '', // ForBomAttFlag
        typeId: '', // type_id
        // parentId: '', // parent_id
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
      materialtypes.getList(this.listQuery).then(response => {
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
         typeAlias: '', 
         typeName: '', 
         parentId: '', 
         typeLevel: '', 
         orderIdx: '', 
         codingExp: '', 
         descExp: '', 
         valid: '', 
         updTime: '', 
         codeRuleFlag: '', 
         userId: '', 
         attachFlag: '', 
         macTime: '', 
         macPrice: '', 
         forBomAttFlag: '', 
         typeId: '', 
        //  parentId: '', 
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
          materialtypes.add(this.temp).then(() => {
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
          materialtypes.update(tempData).then(() => {
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
      materialtypes.del(rows.map(u => u.id)).then(() => {
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
