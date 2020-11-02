<template>
  <div class="laboratory-wrapper">
    <div class="search-wrapper">
      <sticky :className="'sub-navbar'">
        <search @search="onSearch" :options="options"></search>
      </sticky>
    </div>
    <div class="app-container">
      <el-table
        v-loading="loading"
        :data="tableData"
        border
        style="width: 100%;"
      >
        <el-table-column fixed label="序号" width="120">
          <template slot-scope="scope">
            {{ scope.$index + 1 }}
          </template>
        </el-table-column>
        <el-table-column prop="id" label="资产ID" width="120">
        </el-table-column>
        <el-table-column prop="assetCategory" label="类别" width="120">
        </el-table-column>
        <el-table-column prop="assetType" label="型号" width="120">
        </el-table-column>
        <el-table-column
          prop="assetStockNumber"
          label="出厂编号S/N"
          width="120"
        >
        </el-table-column>
        <el-table-column prop="assetNumber" label="资产编号" width="120">
        </el-table-column>
        <el-table-column prop="orgName" label="部门" width="120">
        </el-table-column>
        <el-table-column prop="assetCategorys" label="计量特性" width="370">
        </el-table-column>
        <el-table-column prop="assetInspectType" label="送检类型" width="120">
        </el-table-column>
        <el-table-column prop="assetStartDate" label="最近校准日期" width="140">
        </el-table-column>
        <el-table-column prop="assetEndDate" label="失效日期" width="140">
        </el-table-column>
        <el-table-column prop="assetStatus" label="状态" width="120">
        </el-table-column>
        <el-table-column prop="assetRemarks" label="备注" width="120">
        </el-table-column>
        <el-table-column
          prop="assetCalibrationCertificate"
          label="校准证书"
          width="120"
        >
          <template slot-scope="scope">
            <el-row
              v-if="scope.row.assetCalibrationCertificate"
              type="flex"
              justify="space-around"
            >
              <el-col :span="15">
                <a
                  :href="getImgUrl(scope.row.assetCalibrationCertificate)"
                  target="_blank"
                  class="view"
                  >查看文件</a
                >
              </el-col>
              <el-col :span="9">
                <span
                  class="download"
                  @click="_download(scope.row.assetCalibrationCertificate)"
                  >下载</span
                >
              </el-col>
            </el-row>
          </template>
        </el-table-column>
        <el-table-column prop="assetTCF" label="技术文件" width="120">
          <template slot-scope="scope">
            <el-row
              v-if="scope.row.assetTCF"
              type="flex"
              justify="space-around"
            >
              <el-col :span="15">
                <a
                  :href="getImgUrl(scope.row.assetTCF)"
                  target="_blank"
                  class="view"
                  >查看文件</a
                >
              </el-col>
              <el-col :span="9">
                <span class="download" @click="_download(scope.row.assetTCF)"
                  >下载</span
                >
              </el-col>
            </el-row>
          </template>
        </el-table-column>
        <el-table-column label="校准数据" width="120">
          <template slot-scope="scope">
            <template>
              <el-row
                v-if="scope.row.assetInspectDataOne"
                type="flex"
                justify="space-around"
              >
                <el-col :span="15">
                  <a
                    :href="getImgUrl(scope.row.assetInspectDataOne)"
                    target="_blank"
                    class="view"
                    >查看文件1</a
                  >
                </el-col>
                <el-col :span="9">
                  <span
                    class="download"
                    @click="_download(scope.row.assetInspectDataOne)"
                    >下载</span
                  >
                </el-col>
              </el-row>
              <el-row
                v-if="scope.row.assetInspectDataTwo"
                type="flex"
                justify="space-around"
              >
                <el-col :span="15">
                  <a
                    :href="getImgUrl(scope.row.assetInspectDataTwo)"
                    target="_blank"
                    class="view"
                    >查看文件2</a
                  >
                </el-col>
                <el-col :span="9">
                  <span
                    class="download"
                    @click="_download(scope.row.assetInspectDataTwo)"
                    >下载</span
                  >
                </el-col>
              </el-row>
            </template>
          </template>
        </el-table-column>
        <el-table-column fixed="right" label="操作" width="100">
          <template slot-scope="scope">
            <el-button @click="handleClick(scope.row)" type="text" size="small"
              >查看</el-button
            >
            <el-button type="text" size="small" @click="handleClick(scope.row)"
              >编辑</el-button
            >
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
      <detail ref="detail" :options="options"></detail>
    </div>
  </div>
</template>

<script>
import Sticky from "@/components/Sticky";
import Pagination from "@/components/Pagination";
import Search from "./search";
import Detail from "./detail";
import { getList, getListCategoryName } from "@/api/assetmanagement";
import { download } from "@/utils/file";
export default {
  components: {
    Sticky,
    Pagination,
    Detail,
    Search,
  },
  created() {
    this._getList(this.pageConfig);
    this._getListCategoryName();
  },
  methods: {
    _getList(pageConfig) {
      this.loading = true;
      getList(pageConfig)
        .then((res) => {
          this.loading = false;
          this.tableData = this._normalizeData(res.data);
          this.totalCount = res.count;
        })
        .catch((err) => {
          this.loading = false;
          this.$message.error(err.message);
        });
    },
    /** 格式化tabelData */
    _normalizeData(data) {
      let newList = data.map((item) => {
        let { assetJZData1, assetJZData2, metrological } = item;
        let dataList = [],
          metrologicalList = [];
        assetJZData1 && dataList.push(assetJZData1);
        assetJZData2 && dataList.push(assetJZData2);
        item.assetJZDataList = dataList;
        if (metrological) {
          metrologicalList = metrological.split("\\r\\n");
        }
        item.metrologicalList = metrologicalList;
        return item;
      });
      return newList;
    },
    _getListCategoryName() {
      getListCategoryName().then((res) => {
        this.options = this._initOptions(res.data);
      });
    },
    onSearch(info) {
      if (info.date) {
        info.assetStartDate = info.date[0];
        info.assetEndDate = info.date[1];
      } else {
        info.assetStartDate = "";
        info.assetEndDate = "";
      }
      this.loading = true;
      getList(info)
        .then((res) => {
          this.loading = false;
          this.tableData = this._normalizeData(res.data);
          this.totalCount = res.count;
        })
        .catch((err) => {
          this.loading = false;
          this.$message.error(err.message);
        });
    },
    _initOptions(data) {
      const target = {};
      data.forEach((item) => {
        let { typeId, name } = item;
        let value = { label: name, value: name };
        target[typeId]
          ? target[typeId].push(value)
          : (target[typeId] = []).push(value);
      });
      return target;
    },
    // 点击操作按钮
    handleClick(row) {
      this.$refs.detail.open(row.id);
    },
    handleChange(pageConfig) {
      this._getList({
        ...pageConfig,
      });
    },
    getImgUrl(id) {
      if (!id) return "";
      return `${this.baseURL}/files/Download/${id}?X-Token=${this.tokenValue}`;
    },
    _download(id) {
      const src = this.getImgUrl(id);
      if (src) {
        download(src);
      }
    },
  },
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
      pageConfig: {
        page: 1, // 当前页数
        limit: 20, // 每一页的个数
      },
      totalCount: 0,
      activeName: "first",
      dialogFormVisible: false,
      form: {
        name: "",
        region: "",
        date1: "",
        date2: "",
        delivery: false,
        type: [],
        resource: "",
        desc: "",
      },
      formLabelWidth: "120px",
      tableData: [],
      options: {},
      loading: false,
    };
  },
};
</script>
<style lang="scss" scoped>
.laboratory-wrapper {
  .view {
    color: #409eff;
    text-decoration: underline;
  }
  .search-wrapper {
    padding: 10px;
  }
}
.download {
  cursor: pointer;
}
</style>
