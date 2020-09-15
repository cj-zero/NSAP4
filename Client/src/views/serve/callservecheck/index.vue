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
                <!-- <span
                  v-if="fruit.name === 'status'"
                  :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
                >{{stateValue[scope.row[fruit.name]-1]}}</span> -->
                <div class="link-container" 
                  v-if="fruit.name === 'attendanceClockPictures' && scope.row[fruit.name] && scope.row[fruit.name].length"
                >
                  <img :src="rightImg" @click="toView(scope.row[fruit.name])" class="pointer">
                  <span>查看</span>
                </div>
                <span
                  v-if="fruit.name !== 'attendanceClockPictures'"
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
      </div>
      <el-image-viewer
        v-if="previewVisible"
        :url-list="[previewUrl]"
        :on-close="closeViewer"
      >
      </el-image-viewer>
    </div>
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
export default {
  name: "callservecheck",
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
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      formTheadOptions: [
        // { name: "id", label: "Id"},
        { name: "name", label: "姓名" ,width:'80px'},
        // { name: "org", label: "职位",width:'100px' },
        { name: "org", label: "部门" ,width:'100px'},
        // { name: "clockTime", label: "打卡时间" ,width:'100px'},
         { name: "clockDate", label: "打卡日期", width: '150' },
        { name: "location", label: "地点", width: 360 },
        // { name: "specificLocation", label: "详细地址" },
        { name: "visitTo", label: "拜访对象", width: 100 },
        { name: "remark", label: "备注" },
        { name: "attendanceClockPictures", label: "图片" }
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
      searchConfig: [
        { width: 100, placeholder: '姓名', prop: 'Name' },
        { width: 100, placeholder: '部门', prop: 'Org' },
        { width: 100, placeholder: '拜访对象', prop: 'VisitTo' },
        { width: 150, placeholder: '起始日期', prop: 'DateFrom', type: 'date' },
        { width: 150, placeholder: '结束日期', prop: 'DateTo', type: 'date' },
        { type: 'search' }
      ],
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
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
      console.log('changeForm')
      // this.listQuery.page = 1
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
