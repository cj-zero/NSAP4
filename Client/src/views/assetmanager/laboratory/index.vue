<template>
  <div class="laboratory-wrapper">
    <div class="search-wrapper">
      <sticky :className="'sub-navbar'">
        <search @search="onSearch" :options="options"></search>
      </sticky>
    </div>
    <div class="app-container" >
      <el-table
        :data="tableData"
        border
        style="width: 100%; 100%">
        <el-table-column
          fixed
          label="序号"
          width="150">
          <template slot-scope="scope">
            {{ scope.$index }}
          </template>
        </el-table-column>
        <el-table-column
          prop="id"
          label="资产ID"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetCategory"
          label="类别"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetCCNumber"
          label="出厂编号S/N"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetZCNumber"
          label="资产编号"
          width="120">
        </el-table-column>
        <el-table-column
          prop="orgId"
          label="部门"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetJZDate"
          label="最近校准日期"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetSXDate"
          label="失效日期"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetStatus"
          label="状态"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetRemarks"
          label="备注"
          width="120">
        </el-table-column>
        <el-table-column
          prop="assetJZCertificate"
          label="校准证书"
          width="120">
          <template slot-scope="scope">
            <el-row type="flex" justify="space-around">
              <el-col :span="15">
                <a :href="scope.row.assetJZCertificate" target="_blank" class="view">查看文件</a>
              </el-col>
              <el-col :span="9">
                <a :href="scope.row.assetJZCertificate" download>下载</a>
              </el-col>
            </el-row>
          </template>
        </el-table-column>
        <el-table-column
          prop="assetJSFile"
          label="技术文件"
          width="120">
          <template slot-scope="scope">
            <el-row type="flex" justify="space-around">
              <el-col :span="15">
                <a :href="scope.row.assetJSFile" target="_blank" class="view">查看文件</a>
              </el-col>
              <el-col :span="9">
                <a :href="scope.row.assetJSFile" download>下载</a>
              </el-col>
            </el-row>
          </template>
        </el-table-column>
        <el-table-column
          prop="assetJZDataList"
          label="校准数据"
          width="120">
          <template slot-scope="scope">
            <template v-for="item in scope.row.assetJZDataList">
              <el-row type="flex" justify="space-around" :key="item">
                <el-col :span="15">
                  <a :href="item" target="_blank" class="view">查看文件</a>
                </el-col>
                <el-col :span="9">
                  <a :href="item" download>下载</a>
                </el-col>
              </el-row>
            </template>
          </template>
        </el-table-column>
        <el-table-column
          fixed="right"
          label="操作"
          width="100">
          <template slot-scope="scope">
            <el-button @click="handleClick(scope.row, 'view')" type="text" size="small">查看</el-button>
            <el-button type="text" size="small" @click="handleClick(scope.row, 'edit')">编辑</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div style="margin-top: 20px"></div>
      <pagination
        v-show="totalCount > 0"
        :total="totalCount"
        :page.sync="pageConfig.page"
        :limit.sync="pageConfig.limit"
        @pagination="handleChange"
      />
      <!-- 弹窗 -->
      <el-dialog :visible.sync="dialogFormVisible" width="60%">
        <el-tabs v-model="activeName" type="card" @tab-click="handleClick">
          <el-tab-pane label="详情" name="first">
            <detail :options="options"></detail>
          </el-tab-pane>
          <el-tab-pane label="送检记录" name="second">
            <inspection></inspection>
          </el-tab-pane>
          <el-tab-pane label="操作记录" name="third">
            <opertation></opertation>
          </el-tab-pane>
        </el-tabs>
        <div slot="footer" class="dialog-footer">
          <el-button @click="dialogFormVisible = false" size="mini">取 消</el-button>
          <el-button type="primary" @click="dialogFormVisible = false" size="mini">确 定</el-button>
        </div>
      </el-dialog>
    </div>
  </div>
</template>

<script>
import Sticky from "@/components/Sticky";
import Pagination from '@/components/Pagination'
import Search from './search'
import Opertation from './operation'
import Inspection from './inspection'
import Detail from './detail'
import { getList, getListCategoryName } from '@/api/assetmanagement'
export default {
  components: {
    Sticky,
    Pagination,
    Opertation,
    Inspection,
    Detail,
    Search
  },
  created () {
    this._getList(this.pageConfig)
    this._getListCategoryName()
  },
  methods: {
    _getList (pageConfig) {
      getList(pageConfig).then(res => {
        console.log(res)
        this.tableData = this._normalizeData(res.data)
        // this.tableData = res.data
        console.log(this.tableData)
      })
    },
    /** 格式化tabelData */
    _normalizeData (data) {
      let newList = data.map(item => {
        let { assetJZData1, assetJZData2 } = item
        let dataList = []
        assetJZData1 && dataList.push(assetJZData1)
        assetJZData2 && dataList.push(assetJZData2)
        item.assetJZDataList = dataList
        return item
      })
      return newList
    },
    _getListCategoryName () {
      getListCategoryName().then(res => {
        this.options = this._initOptions(res.data)
        console.log(this.options, 'options')
      })
    },
    _initOptions (data) {
      const target = {}
      data.forEach(item => {
        let { typeId, name } = item
        if (!target[typeId]) {
          target[typeId] = []
          target[typeId].push()
        }
        target[typeId] ?
          target[typeId].push(name) :
          target[typeId] = []
      })
      return target
    },
    // 点击操作按钮
    handleClick(row) {
      console.log(row);
      this.dialogFormVisible = true
    },
    onTabChange (tab, event) {
      console.log(tab, event)
    },
    handleChange (pageConfig) {
      this._getList({
        ...pageConfig
      })
    }
  },
  computed: {
    totalCount () {
      return this.tableData.length
    }
  },
  data() {
    return {
      pageConfig: {
        page: 1, // 当前页数
        limit: 20 // 每一页的个数
      },
      activeName: 'first',
      dialogFormVisible: true,
      form: {
        name: '',
        region: '',
        date1: '',
        date2: '',
        delivery: false,
        type: [],
        resource: '',
        desc: ''
      },
      formLabelWidth: '120px',
      tableData: [],
      options: []
    }
  }
}
</script>
<style lang='scss' scoped>
.laboratory-wrapper {
  .view {
    color: rgba(181, 217, 255, 1);
    text-decoration: underline;
  }
  .search-wrapper {
    padding: 10px;
  }
}
</style>
