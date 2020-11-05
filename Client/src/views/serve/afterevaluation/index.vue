<template>
  <div>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="listQuery" 
          :config="searchConfig"
          @changeForm="onChangeForm" 
          @search="onSearch">
        </Search>
        <!-- <permission-btn moduleName="callservesure" size="mini" v-on:btn-event="onBtnClicked"></permission-btn> -->
      </div>
    </sticky>
    <div class="app-container">
      <div class="bg-white">
        <div class="content-wrapper">
          <el-table
            ref="mainTable"
            :data="checkList"
            v-loading="listLoading"
            border
            fit
            height="100%"
            style="width: 100%;"
            highlight-current-row
            @current-change="handleSelectionChange"
            @row-click="rowClick"
          >
            <!-- <el-table-column     v-for="(fruit,index) in formTheadOptions"  :key="`ind${index}`">
                <el-radio v-model="fruit.id" ></el-radio>
            </el-table-column>-->

            <el-table-column
              show-overflow-tooltip
              v-for="(fruit,index) in formTheadOptions"
              :align="fruit.align?fruit.align:'left'"
              :key="`ind${index}`"
              :sortable="fruit=='chaungjianriqi'?true:false"
              style="background-color:silver;"
              :label="fruit.label"
              :width="fruit.width"
            >
              <template slot-scope="scope">
                <div class="link-container" v-if="fruit.name === 'serviceOrderId'">
                  <img :src="rightImg" @click="openTree(scope.row.serviceOrderId)" class="pointer">
                  <span>{{ scope.row.u_SAP_ID }}</span>
                </div>
                <!-- <el-link
                  v-if="fruit.name === 'serviceOrderId'"
                  type="primary"
                  @click="openTree(scope.row.serviceOrderId)"
                >{{scope.row.serviceOrderId}}</el-link> -->
                <span
                  :class="colorClass[scope.row[fruit.name]]"
                  v-if="fruit.name==='responseSpeed'||fruit.name==='schemeEffectiveness'||fruit.name==='serviceAttitude'||fruit.name==='productQuality'||fruit.name==='servicePrice'"
                >{{backStatus(scope.row[fruit.name])}}</span>
                <span
                  v-if="fruit.name!=='serviceOrderId'&&fruit.name!=='responseSpeed'&&fruit.name!=='schemeEffectiveness'&&fruit.name!=='serviceAttitude'&&fruit.name!=='productQuality'&&fruit.name!=='servicePrice'"
                >{{scope.row[fruit.name]}}</span>
              </template>
            </el-table-column>
          </el-table>
          <pagination
            v-show="total>0"
            :total="total"
            :page.sync="listQuery.page"
            :limit.sync="listQuery.limit"
            @pagination="handleCurrentChange"
          />
        </div>
        <!-- 只能查看的表单 -->
        <el-dialog
          v-el-drag-dialog
          top="5vh"
          width="1210px"
          class="dialog-mini"
          :modal-append-to-body="false"
          title="服务单详情"
          :destroy-on-close="true"
          :close-on-click-modal="false"
          :visible.sync="dialogFormView"
          @open="openDetail"
        >
        <el-row :gutter="20" class="position-view">
          <el-col :span="18" >
            <zxform
              :form="temp"
              formName="查看"
              labelposition="right"
              labelwidth="72px"
              :isCreate="false"
              :refValue="dataForm"
            ></zxform>
          </el-col>
          <el-col :span="6" class="lastWord">   
            <zxchat :serveId='serveId' formName="查看"></zxchat>
          </el-col>
        </el-row>

          <div slot="footer">
            <el-button size="mini" @click="dialogFormView = false">取消</el-button>
            <el-button size="mini" type="primary" @click="dialogFormView = false">确认</el-button>
          </div>
        </el-dialog>
      </div>
    </div>
  </div>
</template>

<script>
import * as afterevaluation from "@/api/serve/afterevaluation";
// import * as callservesure from "@/api/serve/callservesure";

import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
//  import showImg from "@/components/showImg";
import Pagination from "@/components/Pagination";
import elDragDialog from "@/directive/el-dragDialog";
import zxform from "../callserve/form";
import zxchat from '../callserve/chat'
import { chatMixin } from '../common/js/mixins'
import rightImg from '@/assets/table/right.png'
import Search from '@/components/Search'
export default {
  name: "serviceevaluate",
  components: {
    Pagination,
    zxform,
    zxchat,
    Sticky,
    Search
  },
  directives: {
    waves,
    elDragDialog,
  },
  mixins: [chatMixin],
  data() {
    return {
      rightImg,
      multipleSelection: [], // 列表checkbox选中的值
      colorClass: [
        "",
        "redWord",
        "redWord",
        "orangeWord",
        "blueWord",
        "greenWord",
      ],
      formTheadOptions: [
        // { name: "id", label: "Id"},
        { name: "serviceOrderId", label: "服务单号", width: "80px" },
        { name: "customerId", label: "客户代码", width: "100px" },
        { name: "cutomer", label: "客户名称", width: "100px" },
        { name: "contact", label: "联系人", width: "100px" },
        { name: "caontactTel", label: "联系电话" },
        { name: "technician", label: "技术员" },
        { name: "responseSpeed", label: "响应速度" },
        { name: "schemeEffectiveness", label: "方案有效性", width: 90 },
        { name: "serviceAttitude", label: "服务态度" },
        { name: "productQuality", label: "产品质量" },
        { name: "servicePrice", label: "服务价格" },
        { name: "comment", label: "客户建议或意见", width: 108 },
        { name: "visitPeople", label: "回访人" },
        { name: "commentDate", label: "评价日期" },
      ],
      checkList: [],
      total: 0,
      listLoading: true,
      showDescription: false,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
      },
      temp: {
        id: "", // Id
        sltCode: "", // SltCode
        subject: "", // Subject
        cause: "", // Cause
        symptom: "", // Symptom
        descriptio: "", // Descriptio
        status: "", // Status
        extendInfo: "", // 其他信息,防止最后加逗号，可以删除
      },
      // dataForm: {},
      checkd: "",
      dialogFormView: false,
      dialogTable: false,
      dialogTree: false,
      dialogStatus: "",
      pvData: [],
      rules: {
        appId: [
          { required: true, message: "必须选择一个应用", trigger: "change" },
        ],
        name: [{ required: true, message: "名称不能为空", trigger: "blur" }],
      },
      downloadLoading: false,
      serveId: '',
      searchConfig: [
        { width: 100, placeholder: '服务单号', prop: 'ServiceOrderId' },
        { width: 170, placeholder: '客户', prop: 'CustomerId' },
        { width: 100, placeholder: '技术员', prop: 'TechnicianId' },
        { width: 100, placeholder: '回访人', prop: 'VisitPeopleId' },
        { width: 150, placeholder: '评价起始日期', prop: 'DateFrom', type: 'date' },
        { width: 150, placeholder: '评价结束日期', prop: 'DateTo', type: 'date' },
        { type: 'search' },
      ]
    };
  },

  created() {
    this.getList();
  },
  computed: {},
  watch: {
    "listQuery.ServiceOrderId": {
      handler(val) {
        let str = val.toString();
        if (str.length > 1) {
          if (val.indexOf("mp4")) {
            console.log(val.split("mp4"));
          } else {
            console.log(222);
          }
        }
      },
    },
  },
  methods: {
    backStatus(res) {
      if (res == 0) {
        return "未统计";
      } else if (res <= 2) {
        return "差";
      } else if (res <= 3) {
        return "一般";
      } else if (res <= 4) {
        return "满意";
      } else if (res <= 5) {
        return "非常满意";
      }
    },
    onSearch () {
      this.listQuery.page = 1
      this.getList()
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
      // this.listQuery.page = 1
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
    },
    onSubmit() {
      this.getList();
    },
    // openTree(res) {
    //   this.listLoading = true;
    //   console.log(res)
    //   this.serveId = res
    //   callservesure.GetDetails(res).then((res) => {
    //     if (res.code == 200) {
    //       this.dataForm = res.result;
    //       this.dialogFormView = true;
    //     }
    //     this.listLoading = false;
    //   });
    // },
    getList() {
      this.listLoading = true;
      afterevaluation.getList(this.listQuery).then((res) => {
        this.checkList = res.data
        this.total = res.count;
        // this.list = response.data.data;
        this.listLoading = false;
      });
    },

    handleFilter() {
      this.listQuery.page = 1;
      this.getList();
    },
    handleSizeChange(val) {
      this.listQuery.limit = val;
      this.getList();
    },
    handleCurrentChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      this.getList();
    },
  },
};
</script>
<style scoped lang="scss">
.dialog-mini .el-select {
  width: 100%;
}
.greenWord {
  color: green;
}
.orangeWord {
  color: orange;
}
.blueWord {
  color: #409eff;
}
.redWord {
  color: orangered;
}
.input-item {
  width: 100px;
}
</style>
