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
              <el-form-item label="服务ID" size="small">
                <el-input v-model="listQuery.ServiceOrderId" @keyup.enter.native='onSubmit'></el-input>
              </el-form-item>
            </el-col>

            <el-col :span="3">
              <el-form-item label="客户ID" size="small">
                <el-input v-model="listQuery.CustomerId" @keyup.enter.native='onSubmit'></el-input>
              </el-form-item>
            </el-col>

            <el-col :span="3">
              <el-form-item label="技术员ID" size="small">
                <el-input v-model="listQuery.TechnicianId" @keyup.enter.native='onSubmit'></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="3">
              <el-form-item label="回访人ID" size="small">
                <el-input v-model="listQuery.VisitPeopleId" @keyup.enter.native='onSubmit'></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="评价日期" size="small">
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
            <el-col :span="3" style="margin-left:20px;">
              <el-button type="primary" @click="onSubmit" @keyup.enter.native='onSubmit' size="small" icon="el-icon-search">搜 索</el-button>
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
            :width="fruit.width"
          >
            <template slot-scope="scope">
              <el-link
                v-if="fruit.name === 'id'"
                type="primary"
                @click="openTree(scope.row.id)"
              >{{scope.row.id}}</el-link>
              <span :class="colorClass[scope.row[fruit.name]]" v-if="fruit.name==='responseSpeed'||fruit.name==='schemeEffectiveness'||fruit.name==='serviceAttitude'||fruit.name==='productQuality'||fruit.name==='servicePrice'">{{backStatus(scope.row[fruit.name])}}</span>
              <span v-if="fruit.name!=='id'&&fruit.name!=='responseSpeed'&&fruit.name!=='schemeEffectiveness'&&fruit.name!=='serviceAttitude'&&fruit.name!=='productQuality'&&fruit.name!=='servicePrice'">{{scope.row[fruit.name]}}</span>
            </template>
          </el-table-column>
        </el-table>
        <!-- 只能查看的表单 -->
        <el-dialog
          width="800px"
          class="dialog-mini"
          title="服务单详情"
          :destroy-on-close="true"
          :close-on-click-modal="false"
          :visible.sync="dialogFormView"
        >
          <zxform
            :form="temp"
            formName="查看"
            labelposition="right"
            labelwidth="100px"
            :isCreate="false"
            :refValue="dataForm"
          ></zxform>

          <div slot="footer">
            <el-button size="mini" @click="dialogFormView = false">取消</el-button>
            <el-button size="mini" type="primary" @click="dialogFormView = false">确认</el-button>
          </div>
        </el-dialog>
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
import * as afterevaluation from "@/api/serve/afterevaluation";
import * as callservesure from "@/api/serve/callservesure";

import waves from "@/directive/waves"; // 水波纹指令
// import Sticky from "@/components/Sticky";
//  import showImg from "@/components/showImg";
import Pagination from "@/components/Pagination";
import elDragDialog from "@/directive/el-dragDialog";
import zxform from "../callserve/form";

export default {
  name: "afterevaluation",
  components: {
    Pagination,
    zxform
  },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      multipleSelection: [], // 列表checkbox选中的值
      colorClass:['','redWord','redWord','orangeWord','blueWord','greenWord'],
      formTheadOptions: [
        // { name: "id", label: "Id"},
        { name: "id", label: "服务号", width: "80px" },
        { name: "customerId", label: "客户代码", width: "100px" },
        { name: "cutomer", label: "客户名称", width: "100px" },
        { name: "contact", label: "联系人", width: "100px" },
        { name: "caontactTel", label: "联系电话" },
        { name: "technician", label: "技术员" },
        { name: "responseSpeed", label: "响应速度" },
        { name: "schemeEffectiveness", label: "方案有效性" },
        { name: "serviceAttitude", label: "服务态度" },
        { name: "productQuality", label: "产品质量" },
        { name: "servicePrice", label: "服务价格" },
        { name: "comment", label: "客户建议或意见" },
        { name: "visitPeople", label: "回访人" },
        { name: "commentDate", label: "评价日期" }
      ],
      checkList: [],
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
      dataForm: {},
      checkd: "",
      dialogFormView: false,
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
  computed:{

  },
  watch:{
    'listQuery.ServiceOrderId':{
        handler(val){
          let str = val.toString()
          if(str.length>1){
           if(val.indexOf('mp4')){
                console.log(val.split('mp4'))
          }else{
            console.log(222)
          }
    }
    }
    }
  },
  methods: {
        backStatus(res){
      if(res==0){
        return '未统计'
      }else if(res<=2){
        return '差'; 
              }
      else if(res<=3){
        return '一般'; 
              }else if(res<=4){
        return '满意'; 
              }else if(res<=5){
        return '非常满意';
              }
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
    openTree(res) {
      this.listLoading = true;
      callservesure.GetDetails(res).then(res => {
        if (res.code == 200) {
          this.dataForm = res.result;
          this.dialogFormView = true;
        }
        this.listLoading = false;
      });
    },
    getList() {
      this.listLoading = true;
      afterevaluation.getList(this.listQuery).then(res => {
        this.checkList = res.data;
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
.blueWord{
  color: #409EFF;
}
.redWord {
  color: orangered;
}
</style>
