<template>
  <div>
    <ul class="search-section">
      <li>
        <span>资产ID</span>
        <el-input
          size="mini"
          clearable
          v-model="formData.id"
          placeholder="请输入内容"
        ></el-input>
      </li>
      <li>
        <span>类别</span>
        <el-select
          size="mini"
          clearable
          v-model="formData.assetCategory"
          placeholder="请选择"
        >
          <el-option
            v-for="item in assetCategory"
            :value="item.value"
            :label="item.label"
            :key="item.value"
          ></el-option>
        </el-select>
      </li>
      <li>
        <span>型号</span>
        <el-input
          size="mini"
          clearable
          placeholder="请输入内容"
          v-model="formData.assetType"
        ></el-input>
      </li>
      <li>
        <span>出厂编号</span>
        <el-input
          size="mini"
          clearable
          placeholder="请输入内容"
          v-model="formData.assetStockNumber"
        ></el-input>
      </li>
      <li>
        <span>资产编号</span>
        <el-input
          size="mini"
          clearable
          placeholder="请输入内容"
          v-model="formData.assetNumber"
        ></el-input>
      </li>
      <li>
        <span>部门</span>
        <el-input
          size="mini"
          clearable
          placeholder="请输入内容"
          v-model="formData.orgName"
        ></el-input>
      </li>
      <li>
        <span>送检类型</span>
        <el-select
          size="mini"
          clearable
          v-model="formData.assetInspectType"
          placeholder="请选择"
        >
          <el-option
            v-for="item in assetSJType"
            :value="item.value"
            :label="item.label"
            :key="item.value"
          ></el-option>
        </el-select>
      </li>
      <li>
        <span>状态</span>
        <el-select
          size="mini"
          clearable
          v-model="formData.assetStatus"
          placeholder="请选择状态"
        >
          <el-option
            v-for="item in assetStatus"
            :value="item.value"
            :label="item.label"
            :key="item.value"
          ></el-option>
        </el-select>
      </li>
      <li>
        <span>最近校准时间</span>
        <el-date-picker
          size="mini"
          v-model="formData.date"
          type="daterange"
          range-separator="至"
          start-placeholder="开始日期"
          end-placeholder="结束日期"
        >
        </el-date-picker>
      </li>
      <li>
        <el-button
          size="mini"
          type="primary"
          @click="search"
          icon="el-icon-search"
          >查询</el-button
        >
        <el-button size="mini" type="primary" @click="newzc" icon="el-icon-news"
          >新增</el-button
        >
      </li>
    </ul>
    <!-- 弹窗 -->
    <el-dialog
      :visible.sync="dialogFormVisible"
      title="新增资产信息"
      width="900px"
    >
      <add :options="options" @cancel="oncancel"></add>
    </el-dialog>
  </div>
</template>
<script>
import assetMixin from "./mixin/mixin";
import Add from "./add";
export default {
  components: {
    Add,
  },
  props: {
    options: {
      type: Object,
      default() {
        return {};
      },
    },
  },
  mixins: [assetMixin],
  data() {
    return {
      formData: {
        id: "",
        assetCategory: "",
        orgName: "",
        assetStatus: "",
        assetInspectType: "",
        assetType: "",
        assetStockNumber: "",
        assetNumber: "",
        assetStartDate: "",
        assetEndDate: "",
        key: "",
        date: "",
      },
      dialogFormVisible: false,
      //options: {}
    };
  },
  methods: {
    search() {
      this.$emit("search", this.$data.formData);
      //alert("触发了搜索事件："+JSON.stringify(this.$data.FormData))
    },
    newzc() {
      this.$data.dialogFormVisible = true;
    },
    oncancel(val) {
      this.$data.dialogFormVisible = val;
    },
  },
  created() {},
  mounted() {},
};
</script>

<style lang="scss" scoped>
.search-section {
  display: flex;
  flex-wrap: wrap;
  padding: 10px;
  background-color: #fff;
  span {
    flex-shrink: 0;
    min-width: 6em;
    padding: 0 1em;
    font-size: 12px;
    white-space: nowrap;
    text-align: right;
  }
  li {
    display: flex;
    align-items: center;
    margin-right: 10px;
  }
}
</style>
