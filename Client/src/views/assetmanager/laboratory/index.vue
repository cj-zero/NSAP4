<template>
  <div>
    <ul class="search">
      <li class="search-item">
        <span class="search-item-name">资产ID</span>
        <el-input size="mini" clearable v-model="form.id" placeholder="请输入资产ID"></el-input>
      </li>
      <li class="search-item">
        <span class="search-item-name">类别</span>
        <el-select size="mini" clearable v-model="form.assetCategory" placeholder="请选择类别">
          <el-option v-for="item in assetCategory" :key="item.name" :value="item.name"></el-option>
        </el-select>
      </li>
      <li class="search-item">
        <span class="search-item-name">型号</span>
        <el-input size="mini" clearable placeholder="请输入型号" v-model="form.assetType"></el-input>
      </li>
      <li class="search-item">
        <span class="search-item-name">出厂编号</span>
        <el-input size="mini" clearable placeholder="请输入出厂编号" v-model="form.assetStockNumber"></el-input>
      </li>
      <li class="search-item">
        <span class="search-item-name">资产编号</span>
        <el-input size="mini" clearable placeholder="请输入资产编号" v-model="form.assetNumber"></el-input>
      </li>
      <li class="search-item">
        <span class="search-item-name">部门</span>
        <el-input size="mini" clearable placeholder="请输入内部门" v-model="form.orgName"></el-input>
      </li>
      <li class="search-item">
        <span class="search-item-name">送检类型</span>
        <el-select size="mini" clearable v-model="form.assetInspectType" placeholder="请选择送检类型">
          <el-option v-for="item in assetSJType" :key="item.name" :value="item.name"></el-option>
        </el-select>
      </li>
      <li class="search-item">
        <span class="search-item-name">状态</span>
        <el-select size="mini" clearable v-model="form.assetStatus" placeholder="请选择状态">
          <el-option v-for="item in assetStatus" :key="item.name" :value="item.name"></el-option>
        </el-select>
      </li>
      <li class="search-item">
        <span class="search-item-name">最近校准时间</span>
        <el-date-picker size="mini" v-model="form.daterange" type="daterange" value-format="yyyy-MM-dd" start-placeholder="开始日期" end-placeholder="结束日期"> </el-date-picker>
      </li>
      <li class="search-item">
        <el-button size="mini" type="primary" @click="_getList" icon="el-icon-search">查询</el-button>
        <el-button size="mini" type="primary" icon="el-icon-news" @click="handleClick(null, '新增')">新增</el-button>
      </li>
    </ul>
    <div class="app-container">
      <el-table v-loading="loading" :data="tableData" border>
        <el-table-column fixed label="序号" width="120">
          <template slot-scope="scope">
            {{ scope.$index + 1 }}
          </template>
        </el-table-column>
        <el-table-column prop="id" label="资产ID" width="120"></el-table-column>
        <el-table-column prop="assetCategory" label="类别" width="120"></el-table-column>
        <el-table-column prop="assetType" label="型号" width="120"></el-table-column>
        <el-table-column prop="assetStockNumber" label="出厂编号S/N" width="120"></el-table-column>
        <el-table-column prop="assetNumber" label="资产编号" width="120"></el-table-column>
        <el-table-column prop="orgName" label="部门" width="120"></el-table-column>
        <el-table-column prop="assetCategorys" label="计量特性" width="370">
          <template slot-scope="scope">
            <span v-html="scope.row.assetCategorys"></span>
          </template>
        </el-table-column>
        <el-table-column prop="assetInspectType" label="送检类型" width="120"></el-table-column>
        <el-table-column prop="assetStartDate" label="最近校准日期" width="140" :formatter="formatterTime"></el-table-column>
        <el-table-column prop="assetEndDate" label="失效日期" width="140" :formatter="formatterTime"></el-table-column>
        <el-table-column prop="assetStatus" label="状态" width="120"></el-table-column>
        <el-table-column prop="assetRemarks" label="备注" width="120"></el-table-column>
        <el-table-column prop="assetCalibrationCertificate" label="校准证书" width="120">
          <template slot-scope="scope">
            <el-row v-if="scope.row.assetCalibrationCertificate" type="flex" justify="space-around">
              <el-col :span="15">
                <a :href="getImgUrl(scope.row.assetCalibrationCertificate)" target="_blank" class="view">查看文件</a>
              </el-col>
              <el-col :span="9">
                <span class="download" @click="_download(scope.row.assetCalibrationCertificate)">下载</span>
              </el-col>
            </el-row>
          </template>
        </el-table-column>
        <el-table-column prop="assetInspectDataOne" label="技术指标" width="120">
          <template slot-scope="scope">
            <el-row v-if="scope.row.assetInspectDataOne" type="flex" justify="space-around">
              <el-col :span="15">
                <a :href="getImgUrl(scope.row.assetInspectDataOne)" target="_blank" class="view">查看文件</a>
              </el-col>
              <el-col :span="9">
                <span class="download" @click="_download(scope.row.assetInspectDataOne)">下载</span>
              </el-col>
            </el-row>
          </template>
        </el-table-column>
        <el-table-column label="校准数据" width="120">
          <template slot-scope="scope">
            <template>
              <el-row type="flex" justify="space-around">
                <el-col :span="15">
                  <a :href="getImgUrl(scope.row.assetInspectDataTwo)" target="_blank" class="view">查看文件</a>
                </el-col>
                <el-col :span="9">
                  <span class="download" @click="_download(scope.row.assetInspectDataTwo)">下载</span>
                </el-col>
              </el-row>
            </template>
          </template>
        </el-table-column>
        <el-table-column prop="assetTCF" label="技术文件" width="120">
          <template slot-scope="scope">
            <el-row v-if="scope.row.assetTCF" type="flex" justify="space-around">
              <el-col :span="15">
                <a :href="getImgUrl(scope.row.assetTCF)" target="_blank" class="view">查看文件</a>
              </el-col>
              <el-col :span="9">
                <span class="download" @click="_download(scope.row.assetTCF)">下载</span>
              </el-col>
            </el-row>
          </template>
        </el-table-column>
        <el-table-column fixed="right" label="操作" width="140">
          <template slot-scope="scope">
            <div class="table-btns">
              <el-button size="mini" @click="handleClick(scope.row.id, '查看')">查看</el-button>
              <el-button size="mini" type="primary" @click="handleClick(scope.row.id, '编辑')">编辑</el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>
      <pagination v-show="total > 0" :total="total" :page.sync="page" :limit.sync="limit" @pagination="handleChange" />
      <Dialog ref="dialog" @close="dialogClosed" />
    </div>
  </div>
</template>

<script>
import Pagination from '@/components/Pagination'
import Dialog from './dialog'

import { getList, getListCategoryName } from '@/api/assetmanagement'
import { downloadFile } from '@/utils/file'

const SYS_AssetCategory = 'SYS_AssetCategory' //类别
const SYS_AssetStatus = 'SYS_AssetStatus' // 状态
const SYS_AssetSJType = 'SYS_AssetSJType' // 送检类型
const SYS_AssetSJWay = 'SYS_AssetSJWay' // 送检方式
const SYS_CategoryNondeterminacy = 'SYS_CategoryNondeterminacy' // 阻值类型

export default {
  components: {
    Pagination,
    Dialog
  },
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,

      page: 1, // 当前页数
      limit: 20, // 每一页的个数
      total: 0,
      form: {
        id: '', // 资产ID
        assetCategory: '', // 类别
        orgName: '', // 部门
        assetStatus: '', // 状态
        assetInspectType: '', // 送检类型
        assetType: '', // 型号
        assetStockNumber: '', // 出厂编号S/N
        assetNumber: '', // 资产编号
        daterange: [], // 日期
        assetStartDate: '', // 校准日期
        assetEndDate: '' // 失效日期
      },

      tableData: [],
      loading: false,

      assetCategory: [], // 类别
      assetStatus: [], // 状态
      assetSJType: [], // 送检类型
      assetSJWay: [], // 送检方式
      nondeterminacy: [], // 阻值类型

      currentRow: {}
    }
  },
  watch: {
    'form.daterange': function(daterange) {
      this.form.assetStartDate = daterange[0]
      this.form.assetEndDate = daterange[1]
    }
  },
  created() {
    this._getList()
    this._getListCategoryName()
  },
  methods: {
    _getList() {
      this.loading = true
      getList({
        page: this.page,
        limit: this.limit,
        ...this.form
      })
        .then(res => {
          this.loading = false
          this.tableData = res.data.map(item => {
            item.assetCategorys = item.assetCategorys.split('\\r\\n').join('<br>')
            return item
          })
          this.total = res.count
        })
        .catch(err => {
          this.loading = false
          this.$message.error(err.message)
        })
    },
    _getListCategoryName() {
      getListCategoryName().then(res => {
        const data = res.data
        this.assetCategory = data.filter(i => i.typeId === SYS_AssetCategory)
        this.assetStatus = data.filter(i => i.typeId === SYS_AssetStatus)
        this.assetSJType = data.filter(i => i.typeId === SYS_AssetSJType)
        this.assetSJWay = data.filter(i => i.typeId === SYS_AssetSJWay)
        this.nondeterminacy = data.filter(i => i.typeId === SYS_CategoryNondeterminacy)
      })
    },
    formatterTime(_, __, time) {
      return time.split(' ')[0]
    },
    handleChange() {
      this._getList()
    },
    handleClick(id, type) {
      this.$refs.dialog.open(id, type)
    },
    dialogClosed() {
      this._getList()
    },
    getImgUrl(id) {
      if (!id) return ''
      return `${this.baseURL}/files/Download/${id}?X-Token=${this.tokenValue}`
    },
    _download(id) {
      const url = this.getImgUrl(id)
      if (url) {
        downloadFile(url)
      }
    }
  }
}
</script>
<style lang="scss" scoped>
.search {
  display: flex;
  flex-wrap: wrap;
  margin: 10px 10px 0;
  padding: 20px 10px 10px;
  background-color: #fff;
  &-item {
    display: flex;
    align-items: center;
    margin: 0 10px 10px 0;
    &-name {
      flex-shrink: 0;
      min-width: 6em;
      padding: 0 1em;
      font-size: 12px;
      white-space: nowrap;
      text-align: right;
    }
  }
}

.view {
  color: #409eff;
  border-bottom: 1px solid currentColor;
}
.search-wrapper {
  padding: 10px;
}
.download {
  color: #409eff;
  border-bottom: 1px solid currentColor;
  cursor: pointer;
}
.table-btns {
  display: flex;
  padding: 5px 0;
}
</style>
