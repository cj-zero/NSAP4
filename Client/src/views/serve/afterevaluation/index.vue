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
        <!-- ????????????????????? -->
    <el-dialog
      v-el-drag-dialog
      top="5vh"
      width="1210px"
      class="dialog-mini"
      :modal-append-to-body="false"
      title="???????????????"
      :destroy-on-close="true"
      :close-on-click-modal="false"
      :visible.sync="dialogFormView"
      @open="openDetail"
    >
    <el-row :gutter="20" class="position-view">
      <el-col :span="18" >
        <zxform
          :form="temp"
          formName="??????"
          labelposition="right"
          labelwidth="72px"
          :isCreate="false"
          :refValue="dataForm"
        ></zxform>
      </el-col>
      <el-col :span="6" class="lastWord">   
        <zxchat :serveId='serveId' formName="??????"></zxchat>
      </el-col>
    </el-row>

      <div slot="footer">
        <el-button size="mini" @click="dialogFormView = false">??????</el-button>
        <el-button size="mini" type="primary" @click="dialogFormView = false">??????</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import * as afterevaluation from "@/api/serve/afterevaluation";
// import * as callservesure from "@/api/serve/callservesure";

import waves from "@/directive/waves"; // ???????????????
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
      multipleSelection: [], // ??????checkbox????????????
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
        { prop: "serviceOrderId", label: "????????????", width: "80px", slotName: 'serviceOrderId' },
        { prop: "customerId", label: "????????????", width: "100px" },
        { prop: "cutomer", label: "????????????", width: "100px" },
        { prop: "contact", label: "?????????", width: "100px" },
        { prop: "caontactTel", label: "????????????" },
        { prop: "technician", label: "?????????" },
        { prop: "responseSpeed", label: "????????????", slotName: 'responseSpeed' },
        { prop: "schemeEffectiveness", label: "???????????????", width: 90, slotName: 'schemeEffectiveness' },
        { prop: "serviceAttitude", label: "????????????", slotName: 'serviceAttitude' },
        { prop: "productQuality", label: "????????????", slotName: 'productQuality' },
        { prop: "servicePrice", label: "????????????", slotName: 'servicePrice' },
        { prop: "comment", label: "?????????????????????", width: 108 },
        { prop: "visitPeople", label: "?????????" },
        { prop: "commentDate", label: "????????????" },
      ],
      checkList: [],
      total: 0,
      listLoading: true,
      showDescription: false,
      listQuery: {
        // ????????????
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
        extendInfo: "", // ????????????,????????????????????????????????????
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
          { required: true, message: "????????????????????????", trigger: "change" },
        ],
        name: [{ required: true, message: "??????????????????", trigger: "blur" }],
      },
      downloadLoading: false,
      serveId: '',
      searchConfig: [
        { prop: 'ServiceOrderId', component: { attrs: { placeholder: '????????????', style: W_100 } } },
        { prop: 'CustomerId', component: { attrs: { placeholder: '??????', style: W_170 } } },
        { prop: 'Technician', component: { attrs: { placeholder: '?????????', style: W_100 } } },
        { prop: 'VisitPeople', component: { attrs: { placeholder: '?????????', style: W_100 } } },
        { prop: 'DateFrom', component: { tag: 'date', attrs: { placeholder: '??????????????????', style: W_150 } } },
        { prop: 'DateTo', component: { tag: 'date', attrs: { placeholder: '??????????????????', style: W_150 } } },
        { component: { tag: 's-button', attrs: { btnText: '??????' }, on: { click: this.onSearch } } }
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
        return "?????????";
      } else if (res == 1 ) {
        return "?????????";
      } else if (res <= 2) {
        return "???";
      } else if (res <= 3) {
        return "??????";
      } else if (res <= 4) {
        return "??????";
      } else if (res <= 5) {
        return "????????????";
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
