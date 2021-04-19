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
        v-loading="listLoading"
        height="100%"
        @current-change="handleSelectionChange"
        @row-click="rowClick"
      >
        <template v-slot:pictures="{ row }">
          <div class="link-container" 
            v-if="row.attendanceClockPictures && row.attendanceClockPictures.length"
          >
            <img :src="rightImg" @click="toView(row.attendanceClockPictures)" class="pointer">
            <span>查看</span>
          </div>
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
    <el-image-viewer
      v-if="previewVisible"
      :zIndex="99999"
      :url-list="[previewUrl]"
      :on-close="closeViewer"
    >
    </el-image-viewer>
  </div>
</template>

<script>
import * as callservecheck from "@/api/serve/callservecheck";
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
// import showImg from "@/components/showImg";
import Pagination from "@/components/Pagination";
import elDragDialog from "@/directive/el-dragDialog";
import Search from '@/components/Search'
import rightImg from '@/assets/table/right.png'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import { serializeParams } from '@/utils/process'
const W_100 = { width: '100px' }
const W_150 = { width: '150px' }
export default {
  name: "attendanceclock",
  components: {
    Pagination,
    // showImg,
    Search,
    Sticky,
    ElImageViewer
  },
  directives: {
    waves,
    elDragDialog
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'Name', component: { attrs: { style: W_100, placeholder: '姓名' } } },
        { prop: 'Org', component: { attrs: { style: W_100, placeholder: '部门' } } },
        { prop: 'VisitTo', component: { attrs: { style: W_100, placeholder: '拜访对象' } } },
        { prop: 'Location', component: { attrs: { style: W_150, placeholder: '地址' } } },
        { prop: 'DateFrom', component: { tag: 'date', attrs: { style: W_150, placeholder: '起始时间' } } },
        { prop: 'DateTo', component: { tag: 'date', attrs: { style: W_150, placeholder: '结束时间' } } },
        { component: { tag: 's-button', attrs: { btnText: '查询' }, on: { click: this.onSearch } } },
        { component: { tag: 's-button', attrs: { btnText: '导出' }, on: { click: this._export } } }
      ]
    } 
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      formTheadOptions: [
        // { name: "id", label: "Id"},
        { prop: "name", label: "姓名" ,width:'80px'},
        // { prop: "org", label: "职位",width:'100px' },
        { prop: "org", label: "部门" ,width:'100px'},
        // { prop: "clockTime", label: "打卡时间" ,width:'100px'},
         { prop: "clockDate", label: "打卡日期", width: '150' },
        { prop: "location", label: "地址", width: 360 },
        // { prop: "specificLocation", label: "详细地址" },
        { prop: "visitTo", label: "拜访对象", width: 100 },
        { prop: "remark", label: "备注" },
        { prop: "attendanceClockPictures", label: "图片", slotName: 'pictures' }
      ],
      checkList:[],
      total: 0,
      listLoading: true,
      showDescription: false,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined
      },
      temp: {
        id: "", // Id
        sltCode: "", // SltCode
        subject: "", // Subject
        cause: "", // Cause
        symptom: "", // Symptom
        descriptio: "", // Descriptio
        status: "", // Status
        extendInfo: "" // 其他信息,防止最后加逗号，可以删除
      },
      checkd: "",
      dialogFormVisible: false,
      dialogTable: false,
      dialogTree: false,
      dialogStatus: "",
      pvData: [],
      rules: {
        appId: [
          { required: true, message: "必须选择一个应用", trigger: "change" }
        ],
        name: [{ required: true, message: "名称不能为空", trigger: "blur" }]
      },
      downloadLoading: false,
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
      previewUrl: "", //预览图片的定义
      previewVisible: false,
      rightImg
    };
  },

  created() {
    this.getList();
  },

  methods: {
    _export () {
      this.$confirm('确认导出？', '确认信息', {
        confirmButtonText: '确认',
        cancelButtonText: '取消',
        closeOnClickModal: false,
        type: 'warning'
      })
      .then(() => {
        let searchStr = serializeParams(this.listQuery)
        searchStr += `&X-Token=${this.tokenValue}`
        console.log(searchStr)
        window.open(`${process.env.VUE_APP_BASE_API}/serve/AttendanceClock/ExportAttendanceClock?${searchStr}`, '_blank')
      })      
    },
    toView (picturesList) {
      if (picturesList && picturesList.length) {
        let { pictureId, id } = picturesList[0]
        // this.previewUrl = 'https://nsapgateway.neware.work/api/files/Download/ffcd9b63-a0c9-45de-9c83-1ae61d027914?X-Token=db53ea7e'
        this.previewUrl = `${this.baseURL}/files/Download/${pictureId || id}?X-Token=${this.tokenValue}`
        this.previewVisible = true
      }
    },
    closeViewer () {
      // this.previewUrl = false
      this.previewVisible = false
    },
    onSearch () {
      this.listQuery.page = 1
      this.getList()
    },
    changeTable(result) {
      console.log(result);
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
    },
    onSubmit(){
      this.getList()
    },
    getList() {
      this.listLoading = true;
      callservecheck.getList(this.listQuery).then(res => {
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
      console.log(val, 'change')
      this.getList();
    }
  }
};
</script>
<style>
.dialog-mini .el-select {
  width: 100%;
}
.greenWord {
  color: green;
}
.orangeWord {
  color: orange;
}
.redWord {
  color: orangered;
}
</style>
