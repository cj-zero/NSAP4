<template>
  <div>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <Search 
          :listQuery="listQuery"
          :config="searchConfig"
          @search="onSearch">
        </Search>
        <!-- <permission-btn moduleName="callservesure" size="mini" v-on:btn-event="onBtnClicked"></permission-btn> -->
      </div>
    </sticky>
    <Layer>
      <common-table
        ref="mainTable"
        :data="checkList"
        :columns="formTheadOptions"
        :loading="listLoading"
        height="100%"
        @current-change="handleSelectionChange"
        @row-click="rowClick"
      >
        <template v-slot:serviceOrderId="{ row }">
          <div class="link-container">
            <img :src="rightImg" @click="openTree(row.serviceOrderId)" class="pointer">
            <span>{{ row.u_SAP_ID }}</span>
          </div>
        </template>
        <template v-slot:responseSpeed="{ row }">
          <span :class="colorClass[row.responseSpeed]">
            {{ backStatus(row.responseSpeed) }}
          </span>
        </template>
         <template v-slot:schemeEffectiveness="{ row }">
           <span :class="colorClass[row.schemeEffectiveness]">
            {{ backStatus(row.schemeEffectiveness) }}
           </span>
        </template>
         <template v-slot:serviceAttitude="{ row }">
           <span :class="colorClass[row.serviceAttitude]">
            {{ backStatus(row.serviceAttitude) }}
           </span>
        </template>
         <template v-slot:productQuality="{ row }">
           <span :class="colorClass[row.productQuality]">
             {{ backStatus(row.productQuality) }}
           </span>
        </template>
         <template v-slot:servicePrice="{ row }">
           <span :class="colorClass[row.servicePrice]">
            {{ backStatus(row.servicePrice) }}
          </span>
        </template>
      </common-table>
      <pagination
        v-show="total>0"
        :total="total"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleCurrentChange"
      />
    </Layer>
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
const W_100 = { width: '100px' }
const W_150 = { width: '150px' }
const W_170 = { width: '170px' }
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
        { prop: "serviceOrderId", label: "服务单号", width: "80px", slotName: 'serviceOrderId' },
        { prop: "customerId", label: "客户代码", width: "100px" },
        { prop: "cutomer", label: "客户名称", width: "100px" },
        { prop: "contact", label: "联系人", width: "100px" },
        { prop: "caontactTel", label: "联系电话" },
        { prop: "technician", label: "技术员" },
        { prop: "responseSpeed", label: "响应速度", slotName: 'responseSpeed' },
        { prop: "schemeEffectiveness", label: "方案有效性", width: 90, slotName: 'schemeEffectiveness' },
        { prop: "serviceAttitude", label: "服务态度", slotName: 'serviceAttitude' },
        { prop: "productQuality", label: "产品质量", slotName: 'productQuality' },
        { prop: "servicePrice", label: "服务价格", slotName: 'servicePrice' },
        { prop: "comment", label: "客户建议或意见", width: 108 },
        { prop: "visitPeople", label: "回访人" },
        { prop: "commentDate", label: "评价日期" },
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
        { prop: 'ServiceOrderId', component: { attrs: { placeholder: '服务单号', style: W_100 } } },
        { prop: 'CustomerId', component: { attrs: { placeholder: '客户', style: W_170 } } },
        { prop: 'Technician', component: { attrs: { placeholder: '技术员', style: W_100 } } },
        { prop: 'VisitPeople', component: { attrs: { placeholder: '回访人', style: W_100 } } },
        { prop: 'DateFrom', component: { tag: 'date', attrs: { placeholder: '评价起始日期', style: W_150 } } },
        { prop: 'DateTo', component: { tag: 'date', attrs: { placeholder: '评价结束日期', style: W_150 } } },
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } }
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
      } else if (res == 1 ) {
        return "非常差";
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
    rowClick() {
      // this.$refs.mainTable.clearSelection();
      // this.$refs.mainTable.toggleRowSelection(row);
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
      }).catch(err => {
        this.$message.error(err.message)
      }).finally(() => this.listLoading = false)
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
