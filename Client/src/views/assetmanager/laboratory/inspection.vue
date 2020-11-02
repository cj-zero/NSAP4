<template>
  <el-table
    :data="tableData"
    :cell-style="cellStyle"
    :header-cell-style="headerCellStyle"
  >
    <el-table-column label="校准日期" prop="inspectStartDate">
    </el-table-column>
    <el-table-column label="失效日期" prop="inspectEndDate"> </el-table-column>
    <el-table-column label="证书">
      <template v-if="scope.row.inspectCertificate" slot-scope="scope">
        <a
          :href="getImgUrl(scope.row.inspectCertificate)"
          class="item"
          target="_blank"
          >查看文件</a
        >
        <span class="item" @click="_download(scope.row.inspectCertificate)"
          >下载</span
        >
      </template>
    </el-table-column>
    <el-table-column label="校准数据">
      <el-table-column label="失效日期">
        <template v-if="scope.row.inspectDataOne" slot-scope="scope">
          <a
            v-if="scope.row.inspectDataOne"
            :href="getImgUrl(scope.row.inspectDataOne)"
            class="item"
            target="_blank"
            >查看文件</a
          >
          <span class="item" @click="_download(scope.row.inspectDataOne)"
            >下载</span
          >
        </template>
      </el-table-column>
      <el-table-column label="失效日期">
        <template v-if="scope.row.inspectDataTwo" slot-scope="scope">
          <a
            v-if="scope.row.inspectDataTwo"
            :href="getImgUrl(scope.row.inspectDataTwo)"
            class="item"
            target="_blank"
            >查看文件</a
          >
          <span class="item" @click="_download(scope.row.inspectDataTwo)"
            >下载</span
          >
        </template>
      </el-table-column>
    </el-table-column>
  </el-table>
</template>

<script>
import { download } from "@/utils/file";
export default {
  props: {
    list: {
      type: Array,
      default: () => [],
    },
  },
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
    };
  },
  computed: {
    tableData() {
      return this.list;
    },
  },
  methods: {
    cellStyle() {
      return {
        "text-align": "center",
        "vertical-align": "middle",
      };
    },
    headerCellStyle({ rowIndex }) {
      let style = {
        "text-align": "center",
        "vertical-align": "middle",
      };
      if (rowIndex === 1) {
        style.display = "none";
      }
      return style;
    },
    _download(id) {
      const src = this.getImgUrl(id);
      if (src) {
        download(src);
      }
    },
    getImgUrl(id) {
      if (!id) return "";
      return `${this.baseURL}/files/Download/${id}?X-Token=${this.tokenValue}`;
    },
  },
};
</script>
<style lang="scss" scoped>
.item {
  color: rgba(102, 177, 255, 1);
  margin: 0 5px;
  text-decoration: underline;
  cursor: pointer;
}
</style>
