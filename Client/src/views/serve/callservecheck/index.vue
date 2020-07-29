<template>
  <div>
    <!-- <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <el-input
          @keyup.enter.native="handleFilter"
          size="mini"
          style="width: 200px;"
          class="filter-item"
          :placeholder="'名称'"
          v-model="listQuery.key"
        ></el-input>
        <el-button
          class="filter-item"
          size="mini"
          style="margin:0 15px;"
          v-waves
          icon="el-icon-search"
          @click="handleFilter"
        >搜索</el-button>
        <permission-btn moduleName="callservesure" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>-->
    <div class="app-container">
      <div class="bg-white">
        <el-form ref="listQuery" :model="listQuery" label-width="80px">
          <div style="padding:10px 0;"></div>
          <el-row :gutter="10">
            <el-col :span="3">
              <el-form-item label="姓名" size="small" >
                         <el-input v-model="listQuery.Name"></el-input>

              </el-form-item>
            </el-col>

            <el-col :span="3">
              <el-form-item label="部门" size="small" >
                <el-input v-model="listQuery.Org"></el-input>

                <!-- <el-select v-model="listQuery.Org" placeholder="请选择部门">
                  <el-option label="全部" value></el-option>
                  <el-option label="部门1" value="1"></el-option>
                  <el-option label="部门2" value="2"></el-option>
                  <el-option label="部门3" value="3"></el-option>
                </el-select> -->
              </el-form-item>
            </el-col>

            <el-col :span="3">
              <el-form-item label="拜访对象" size="small" >
                <el-input v-model="listQuery.VisitTo"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="打卡日期" size="small" >
                <el-col :span="11">
                  <el-time-select
                    placeholder="选择开始日期"
                    v-model="listQuery.DateFrom"
                    style="width: 100%;"
                      :picker-options="{
                        start: '05:30',
                        step: '00:15',
                        end: '23:30'
                      }"
                  ></el-time-select>
                </el-col>
                <el-col class="line" :span="2">至</el-col>
                <el-col :span="11">
                  <el-time-select
                    placeholder="选择结束时间"
                    v-model="listQuery.DateTo"
                    style="width: 100%;"
                      :picker-options="{
                        start: '05:30',
                        step: '00:15',
                        end: '23:30'
                      }"
                  ></el-time-select>
                </el-col>
              </el-form-item>
            </el-col>
              <el-col :span="3" style="margin-left:20px;" >
                    <el-button type="primary" @click="onSubmit" size="small" icon="el-icon-search"> 搜 索 </el-button>
      </el-col>
          </el-row>
        </el-form>
        <el-table
          ref="mainTable"
          :data="checkList"
          v-loading="listLoading"
          border
          fit
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
          >
            <template slot-scope="scope">
              <!-- <span
                v-if="fruit.name === 'status'"
                :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
              >{{stateValue[scope.row[fruit.name]-1]}}</span> -->
              <span v-if="fruit.name === 'attendanceClockPictures'">
                <showImg :PicturesList="scope.row[fruit.name]" 
                ></showImg>
                </span> 
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
  </div>
</template>

<script>
import * as callservecheck from "@/api/serve/callservecheck";
import waves from "@/directive/waves"; // 水波纹指令
// import Sticky from "@/components/Sticky";
 import showImg from "@/components/showImg";
import Pagination from "@/components/Pagination";
import elDragDialog from "@/directive/el-dragDialog";
export default {
  name: "callservecheck",
  components: {
    Pagination,
    showImg
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
        { name: "name", label: "姓名" },
        { name: "org", label: "职位" },
        { name: "org", label: "部门" },
        { name: "clockTime", label: "打卡时间" },
        { name: "location", label: "地点" },
        { name: "specificLocation", label: "详细地址" },
        { name: "visitTo", label: "拜访对象" },
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
      downloadLoading: false
    };
  },

  created() {
    this.getList();
  },

  methods: {
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
        console.log(res)
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
