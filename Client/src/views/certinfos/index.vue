<template>
<div>
  <sticky :className="'sub-navbar'">
    <div class="filter-container">
      <el-input @keyup.enter.native="handleFilter" size="mini"  style="width: 200px;" class="filter-item" :placeholder="'名称'" v-model="listQuery.key">
      </el-input>
      
      <el-button class="filter-item" size="mini"  v-waves icon="el-icon-search" @click="handleFilter">搜索</el-button>
      <permission-btn moduleName='certinfos' size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
    </div>
     </sticky>
      <div class="app-container "> 
     <div class="bg-white">
    <el-table  ref="mainTable"  :key='tableKey' :data="list" v-loading="listLoading" border fit highlight-current-row
      style="width: 100%;" @row-click="rowClick"  @selection-change="handleSelectionChange">
			
       <el-table-column type="selection" align="center"  width="55"></el-table-column>

      
               <el-table-column prop="id" label="Id" show-overflow-tooltip></el-table-column>
               <el-table-column prop="certNo" label="CertNo" show-overflow-tooltip></el-table-column>
              <el-table-column prop="createTime" label="CreateTime" show-overflow-tooltip></el-table-column>
               <el-table-column prop="activityName" label="activityName" show-overflow-tooltip></el-table-column>


      <el-table-column align="center" label="操作" width="350" >
        <template slot-scope="scope">
        <el-dropdown style="margin:0 10px;" @command="downFile">
  <el-button type="primary" size="mini">
    下载<i class="el-icon-arrow-down el-icon--right"></i>
  </el-button>
  <el-dropdown-menu slot="dropdown" >
    <el-dropdown-item :command="scope.row.certNo+1">下载证书基础信息</el-dropdown-item>
    <el-dropdown-item :command="scope.row.certNo+2">下载证书PDF</el-dropdown-item>

  </el-dropdown-menu>
</el-dropdown>
    <!-- <el-dropdown  size="small" split-button @change="downFile">
  <el-button type="primary">
    更多菜单<i class="el-icon-arrow-down el-icon--right"></i>
  </el-button>
  <el-dropdown-menu slot="dropdown">
    <el-dropdown-item>下载证书基础信息</el-dropdown-item>
    <el-dropdown-item>下载证书PDF</el-dropdown-item>
  </el-dropdown-menu>
</el-dropdown> -->
       
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
              <el-form-item size="small" label="CertNo" prop="certNo">
                <el-input v-model="temp.certNo"></el-input>
              </el-form-item>
              <el-form-item size="small" label="CertPath" prop="certPath">
                <el-input v-model="temp.certPath"></el-input>
              </el-form-item>
              <el-form-item size="small" label="PdfPath" prop="pdfPath">
                <el-input v-model="temp.pdfPath"></el-input>
              </el-form-item>
              <el-form-item size="small" label="BaseInfoPath" prop="baseInfoPath">
                <el-input v-model="temp.baseInfoPath"></el-input>
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
import * as certinfos from '@/api/certinfos'
import waves from '@/directive/waves' // 水波纹指令
import Sticky from '@/components/Sticky'
import permissionBtn from '@/components/PermissionBtn'
import Pagination from '@/components/Pagination'
import elDragDialog from '@/directive/el-dragDialog'
export default {
  name: 'certinfos',
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
      down_value:'',
      total: 0,
      listLoading: true,
      listQuery: { // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined
      },
      // options_down:[
      //   {value: `${process.env.VUE_APP_BASE_API}/api/Cert/DownloadBaseInfo/`,label:'下载证书基础信息'},
      //   {value: `${process.env.VUE_APP_BASE_API}/api/Cert/DownloadCertPdf/`,label:'下载证书PDF'},
      // ],
      statusOptions: [
        { key: 1, display_name: '停用' },
        { key: 0, display_name: '正常' }
      ],
      temp: {
        id: '', // Id
        certNo: '', // CertNo
        certPath: '', // CertPath
        pdfPath: '', // PdfPath
        baseInfoPath: '', // BaseInfoPath
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
    downFile(result){
      const len = result.length
       let num1 = result.slice(0,len-1)
       let num2 = result.slice(len-1,len)
      if(num2==1){
       console.log(`${process.env.VUE_APP_BASE_API}/api/Cert/DownloadBaseInfo/${num1}?token=${this.$store.state.user.token}`)
        window.location.href = `${process.env.VUE_APP_BASE_API}/Cert/DownloadBaseInfo/${num1}?token=${this.$store.state.user.token}`
      }else{
              //  console.log(`${process.env.VUE_APP_BASE_API}/Cert/DownloadBaseInfo/${num1}?token=${this.$store.state.user.token}`)
        console.log(num1,num2)
        window.location.href = `${process.env.VUE_APP_BASE_API}/Cert/DownloadCertPdf/${num1}?token=${this.$store.state.user.token}`

      }
    },
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
      certinfos.getList(this.listQuery).then(response => {
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
         certNo: '', 
         certPath: '', 
         pdfPath: '', 
         baseInfoPath: '', 
         createTime: '', 
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
          certinfos.add(this.temp).then(() => {
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
          certinfos.update(tempData).then(() => {
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
      certinfos.del(rows.map(u => u.id)).then(() => {
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
