<template>
  <div class="addClass1">
    <!-- form数组，不包括第一项 -->
    <div
      v-for="(item,index) in formList"
      :key="`key_${index}`"
      style="border:1px solid silver;padding:5px;margin-left:20px;"
    >
      <el-form
        :model="item"
        :disabled="!isEdit"
        label-width="90px"
        class="rowStyle"
        :ref="'itemForm'+ index"
      >
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="工单ID">
              <el-input size="small" disabled v-model="item.id"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="7">
            <el-form-item label="服务类型">
              <el-radio-group size="small" v-model="item.feeType">
                <el-radio :label="1">免费</el-radio>
                <el-radio :label="2">收费</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>

          <el-col :span="5" style="height:40px;line-height:40px;font-size:13px;">
            <el-switch size="small" v-model="item.edit" active-text="修改后续订单" :width="20"></el-switch>
          </el-col>
          <el-col :span="2" style="height:40px;line-height:40px;">
            <el-button
              type="danger"
              v-if="formList.length>1"
              size="mini"
              style="margin-right:20px;"
              icon="el-icon-delete"
              @click="deleteForm(item)"
            >删除</el-button>
          </el-col>
          <el-col :span="2" style="height:40px;line-height:40px;">
            <el-button
              type="success"
              v-if="isEditForm"
              size="mini"
              icon="el-icon-share"
              @click="handleIconClick"
            >新增</el-button>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="制造商序列号">
                <el-autocomplete
          popper-class="my-autocomplete"
          v-model="inputSearch"
          :fetch-suggestions="querySearch"
          placeholder="制造商序列号"
          @select="searchSelect"
        >
          <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
          <template slot-scope="{ item }">
             <div class="name" >
                        <p style="height:20px;margin:2px;">{{ item.manufSN }}</p>
                        <p style="font-size:12px;height:20px;margin:2px 0 5px 0 ;color:silver;">{{ item.custmrName }}</p>
                        </div>
            <!-- <div class="name">{{ item.manufSN }}</div>
            <span class="addr">{{ item.custmrName }}</span> -->
          </template>
        </el-autocomplete>
              <!-- <el-input
                size="small"
                maxlength="0"
                v-model="item.manufacturerSerialNumber"
                :disabled="isEditForm"
              >
                <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
              </el-input> -->
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="内部序列号">
              <el-input size="small" v-model="item.internalSerialNumber" disabled></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="服务合同">
              <el-input size="small" v-model="item.contractId" disabled></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="物料编码">
              <el-input size="small" v-model="item.materialCode" disabled></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="16">
            <el-form-item label="物料描述">
              <el-input size="small" disabled v-model="item.materialDescription"></el-input>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="24">
            <el-form-item
              label="呼叫主题"
              prop="fromTheme"
              :rules="{
              required: true, message: '呼叫主题不能为空', trigger: 'blur'
            }"
            >
              <el-input size="small" v-model="item.fromTheme"></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item
              label="呼叫类型"
              prop="fromType"
              :rules="{
              required: true, message: '呼叫类型不能为空', trigger: 'blur'
            }"
            >
              <el-select v-model="item.fromType" size="small" clearable>
                <el-option
                  v-for="item in options_type"
                  :key="item.label"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="呼叫状态">
              <!-- <el-input v-model="form.status" disabled></el-input> -->
              <el-select
                size="small"
                v-model="item.status"
                :disabled="isEdit"
                clearable
                placeholder="请选择"
              >
                <el-option
                  v-for="ite in options_status"
                  :key="ite.value"
                  :label="ite.label"
                  :value="ite.value"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="清算日期">
              <el-date-picker
                size="small"
                disabled
                type="date"
                placeholder="选择日期"
                v-model="item.liquidationDate"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="问题类型" prop="problemTypeId">
              <el-input
                v-model="item.problemTypeId"
                readonly
                size="small"
                prop="problemTypeId"
                :rules="{
              required: true, message: '问题类型不能为空', trigger: 'blur' }"
                @focus="()=>{proplemTree=true,sortForm=index+1}"
              >
                <el-button
                  size="mini"
                  slot="append"
                  icon="el-icon-search"
                  @click="()=>{proplemTree=true,sortForm=index+1}"
                ></el-button>
              </el-input>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="优先级">
              <!-- <el-input v-model="item.priority"></el-input> -->
              <el-select v-model="item.priority" size="small" clearable placeholder="请选择">
                <el-option
                  v-for="ite in options_quick"
                  :key="ite.value"
                  :label="ite.label"
                  :value="ite.value"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="预约时间">
              <el-date-picker
                disabled
                size="small"
                type="date"
                placeholder="选择开始日期"
                v-model="item.bookingDate"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="8">
            <el-form-item label="技术员">
              <el-input size="small" disabled></el-input>
            </el-form-item>
          </el-col>

          <el-col :span="8">
            <el-form-item label="保修结束日期">
              <el-date-picker
                disabled
                size="small"
                type="date"
                placeholder="选择开始日期"
                v-model="item.warrantyEndDate"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="结束时间">
              <el-date-picker
                size="small"
                disabled
                type="date"
                placeholder="选择日期"
                v-model="item.startTime"
                style="width: 100%;"
              ></el-date-picker>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="解决方案">
          <el-input
            v-model="item.solutionId"
            @focus="()=>{solutionOpen=true,sortTable=index+1}"
            :disabled="item.fromType!==2"
            readonly
          >
            <el-button
              size="mini"
              slot="append"
              icon="el-icon-search"
              @click="()=>{solutionOpen=true,sortTable=index+1}"
            ></el-button>
          </el-input>
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input type="textarea" size="small" v-model="item.remark"></el-input>
        </el-form-item>
        <el-form-item v-if="isEditForm">
          <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
            <el-col :span="6"></el-col>
            <el-col :span="3">
              <el-button
                type="success"
                size="small"
                icon="el-icon-share"
                @click="postWorkOrder(item)"
              >确定新增</el-button>
            </el-col>
            <el-col :span="3">
              <el-button
                size="small"
                type="primary"
                icon="el-icon-share"
                @click="postWorkOrder(item)"
              >确定修改</el-button>
            </el-col>
            <el-col :span="3">
              <div class="showSort">{{index+1}}/{{formList.length}}</div>
            </el-col>
          </el-row>
        </el-form-item>
      </el-form>
    </div>

    <!--  -->
    <el-dialog
      class="addClass1"
      :title="`第${sortForm}工单`"
      center
      :visible.sync="proplemTree"
      width="250px"
    >
      <problemtype @node-click="NodeClick" :dataTree="dataTree"></problemtype>
    </el-dialog>
    <el-dialog
      :title="`第${sortTable}个工单的解决方案`"
      center
      class="addClass1"
      loading
      :visible.sync="solutionOpen"
      width="1000px"
    >
      <solution
        @solution-click="solutionClick"
        :list="datasolution"
        :total="solutionCount"
        :listLoading="listLoading"
        @page-Change="pageChange"
      ></solution>
      <!-- <span slot="footer" class="dialog-footer">
        <el-button size="small" @click="solutionOpen = false">取 消</el-button>
        <el-button size="small" type="primary" @click="solutionOpen=false">确 定</el-button>
      </span>-->
    </el-dialog>
    <el-dialog
      :append-to-body="true"
      destroy-on-close
      class="addClass1"
      title="选择制造商序列号"
      @open="openDialog"
      width="90%"
      :visible.sync="dialogfSN"
    >
      <div style="width:400px;margin:10px 0;">

                <el-input @input="searchList" style="width:150px;margin:0 20px;display:inline-block;" v-model="inputSearch"  placeholder="制造商序列号"> 

                          <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>

              </el-input>
              <el-input @input="searchList" style="width:150px;margin:0 20px;display:inline-block;" v-model="inputItemCode"  placeholder="输入物料编码"> 

                          <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>

              </el-input>

      </div>
      <fromfSN
        :SerialNumberList="filterSerialNumberList"
        :serLoading="serLoad"
        @change-Form="changeForm"
      ></fromfSN>
      <pagination
        v-show="SerialCount>0"
        :total="SerialCount"
        :page.sync="listQuery.page"
        :limit.sync="listQuery.limit"
        @pagination="handleChange"
      />
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogfSN = false">取 消</el-button>
        <el-button type="primary" @click="pushForm">确 定</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import { getSerialNumber } from "@/api/callserve";
import Pagination from "@/components/Pagination";
import * as callservesure from "@/api/serve/callservesure";

import fromfSN from "./fromfSN";
import * as problemtypes from "@/api/problemtypes";
import * as solutions from "@/api/solutions";
import problemtype from "./problemtype";
import solution from "./solution";
export default {
  components: { fromfSN, problemtype, solution, Pagination },
  props: ["isEdit", "isEditForm", "serviceOrderId", "propForm"],
  data() {
    return {
      defaultProps: {
        label: "name",
        children: "childTypes"
      }, //树形控件的显示状态
      sortForm: "",
      sortTable: "", //点击解决方案的排序
      solutionOpen: false, //显示解决方案
      problemLabel: "",
      dataTree: [], //问题类型的组合
      datasolution: [], //解决方案集合
      solutionCount: "",
      listLoading: true,
      proplemTree: false,
      serLoad: false,
      addressList: [], //客户地址集合
      cntctPrsnList: [], //客户联系人集合
      SerialNumberList: [],
      filterSerialNumberList: [],
      formListStart: [], //选择的表格数据
      formList: [
        {
          // serviceOrderId:'',
          priority: 1, //优先级 4-紧急 3-高 2-中 1-低
          feeType: 1, //服务类型 1-免费 2-收费
          submitDate: "", //工单提交时间
          recepUserId: "", //接单人用户Id
          remark: "", //备注
          status: 1, //呼叫状态 1-待确认 2-已确认 3-已取消 4-待处理 5-已排配 6-已外出 7-已挂起 8-已接收 9-已解决 10-已回访
          currentUserId: "", //App当前流程处理用户Id
          fromTheme: "", //呼叫主题
          fromId: 1, //呼叫来源 1-电话 2-APP
          problemTypeId: "", //问题类型Id
          fromType: 2, //呼叫类型1-提交呼叫 2-在线解答（已解决）
          materialCode: "", //物料编码
          materialDescription: "", //物料描述
          manufacturerSerialNumber: "", //制造商序列号
          internalSerialNumber: "", //内部序列号
          warrantyEndDate: "", //保修结束日期
          bookingDate: "", //预约时间
          visitTime: "", //上门时间
          liquidationDate: "", //清算日期
          contractId: "", //服务合同
          solutionId: "", //解决方案
          troubleDescription: "",
          processDescription: ""
        }
      ], //表单依赖的表格数据

      dialogfSN: false,
      inputSearch: "",
      inputItemCode:'',//物料编码
      activeNames: ["1"], //活跃名称

      options_sourse: [
        { value: "电话", label: "电话" },
        { value: "钉钉", label: "钉钉" },
        { value: "QQ", label: "QQ" },
        { value: "微信", label: "微信" },
        { value: "邮件", label: "邮件" },
        { value: "APP", label: "APP" },
        { value: "Web", label: "Web" }
      ],
      options_status: [
        { label: "已回访", value: 10 },
        { label: "已解决", value: 9 },
        { label: "已接收", value: 8 },
        { label: "已挂起", value: 7 },
        { label: "已外出", value: 6 },
        { label: "已排配", value: 5 },
        { label: "待处理", value: 4 },
        { label: "已取消", value: 3 },
        { label: "已确认", value: 2 },
        { label: "待确认", value: 1 }
      ],
      options_type: [
        { value: 1, label: "提交呼叫" },
        { value: 2, label: "在线解答" }
      ], //呼叫类型
      options_quick: [
        { label: "高", value: 3 },
        { label: "中", value: 2 },
        { label: "低", value: 1 }
      ],
      listQuery: {
        page: 1,
        limit: 10,
        CardCode: ""
      },
      SerialCount: "",
      ifFormPush: false //表单是否被动态添加过
    };
  },
  created() {},

  mounted() {
    this.listLoading = true;
    this.getSerialNumberList();
    //获取问题类型的数据
    problemtypes
      .getList()
      .then(res => {
        this.dataTree = res.data;
      })
      .catch(error => {
        console.log(error);
      });
    solutions.getList().then(response => {
      this.datasolution = response.data;
      this.solutionCount = response.count;
      this.listLoading = false;
    });
  },
  watch: {
    formList: {
      deep: true,
      handler(val) {
        this.$emit("change-form", val);
      }
    },
    propForm: {
      deep: true,
      immediate: true,
      handler(val) {
        console.log(val);
        if (val.length) {
          this.formList = val;
        }
      }
    },
    "form.customerId": {
      deep: true,
      handler(val) {
        this.listQuery.CardCode = val;
        getSerialNumber(this.listQuery)
          .then(res => {
            this.SerialNumberList = res.data;
            this.filterSerialNumberList = this.SerialNumberList;
            this.SerialCount = res.count;
          })
          .catch(error => {
            console.log(error);
          });
      }
    }
  },
  // updated(){

  //     this.listQuery.customerId=this.form.customerId
  //   // }
  //   // console.log(this.form.customerId)
  // },
  inject: ["form"],
  methods: {
    getSerialNumberList() {
      this.listLoading = true;
      this.serLoad = true;
      getSerialNumber(this.listQuery)
        .then(res => {
          this.SerialNumberList = res.data;
          this.filterSerialNumberList = this.SerialNumberList;
          this.SerialCount = res.count;
          this.serLoad = false;
          this.listLoading = false;
        })
        .catch(error => {
          console.log(error);
        });
    },
    handleChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      this.getSerialNumberList();
    },
    pageChange(res) {
      this.listLoading = true;
      solutions.getList(res).then(response => {
        this.datasolution = response.data;
        this.solutionCount = response.count;
        this.listLoading = false;
      });
    },
    solutionget(res) {
      this.datasolution = res;
    },
    NodeClick(res) {
       this.formList[this.sortForm - 1].problemType = res.id;
      this.formList[this.sortForm - 1].problemTypeId = res.name;
      this.problemLabel = res.name;
      this.proplemTree = false;
    },
    solutionClick(res) {
       this.formList[this.sortTable - 1].solution = res.id;
      this.formList[this.sortTable - 1].solutionId = res.subject;
      // this.problemLabel = res.name;
      this.solutionOpen = false;
    },
    switchType(val) {
      let vall = "";
      this.dataTree.filter(item => {
        if (item.id == val) {
          vall = item.name;
          return vall;
        }
        item.childTypes.filter(item => {
          if (item.id == val) {
            vall = item.name;
            return vall;
          }
          return vall;
        });
      });
      return vall;
    },
    switchSo(val) {
      let res = this.datasolution.filter(item => item.id == val);
      return res.length ? res[0].subject : "请选择";
    },
    openDialog() {
      this.filterSerialNumberList = this.SerialNumberList;
    },
    changeForm(res) {
      this.formListStart = res;

      // console.log(res,this.formListStart);
    },

    pushForm() {
      this.dialogfSN = false;
      console.log(this.formListStart);
      // let serviceOrder = localStorage.getItem('serviceOrderId')
      if (!this.ifFormPush) {
        this.formList[0].manufacturerSerialNumber = this.formListStart[0].manufSN;
        this.formList[0].internalSerialNumber = this.formListStart[0].internalSN;
        this.formList[0].materialCode = this.formListStart[0].itemCode;
        //  this.formList[0].serviceOrderId = serviceOrder
        this.formList[0].materialDescription = this.formListStart[0].itemName;
        const newList = this.formListStart.splice(1, this.formListStart.length);
        console.log(newList);
        for (let i = 0; i < newList.length; i++) {
          this.formList.push({
            manufacturerSerialNumber: newList[i].manufSN,
            // serviceOrderId:serviceOrder,
            internalSerialNumber: newList[i].internalSN,
            materialCode: newList[i].itemCode,
            materialDescription: newList[i].itemName
          });
        }
        // this.formList=this.formListStart
        this.ifFormPush = true;
      } else {
        this.ifFormPush = true;
        for (let i = 0; i < this.formListStart.length; i++) {
          this.formList.push({
            manufacturerSerialNumber: this.formListStart[i].manufSN,
            internalSerialNumber: this.formListStart[i].internalSN,
            // serviceOrderId:serviceOrder,
            materialCode: this.formListStart[i].itemCode,
            materialDescription: this.formListStart[i].itemName
          });
        }
      }
    },
    handleIconClick() {
      this.dialogfSN = true;
    },
    postWorkOrder(result) {
      console.log(result); //新增工单
      callservesure
        .addWorkOrder(result)
        .then(() => {
          this.$message({
            message: "新增工单成功",
            type: "success"
          });
        })
        .catch(res => {
          this.$message({
            message: `${res}`,
            type: "error"
          });
        });
    },
    searchList() {
      this.listQuery.ManufSN = this.inputSearch;
      this.listQuery.ItemCode = this.inputItemCode;
      this.getSerialNumberList();
      // if (!res) {
      //   this.filterSerialNumberList = this.SerialNumberList;
      // } else {
      //   let list = this.SerialNumberList.filter(item => {
      //     return item.manufSN.indexOf(res) > 0;
      //   });
      //   this.filterSerialNumberList = list;
      // }
    },
        searchSelect(res) {
      let newList = this.filterSerialNumberList.filter(
        item => item.manufSN === res.manufSN
      );
      this.inputSearch = res.manufSN;
      this.filterSerialNumberList = newList;
    },
    querySearch(queryString, cb) {
       this.listQuery.ManufSN = queryString
      this.getSerialNumberList();
      var filterSerialNumberList = this.SerialNumberList;
      var results = queryString
        ? filterSerialNumberList.filter(this.createFilter(queryString))
        : filterSerialNumberList;
      // console.log(results);
      // 调用 callback 返回建议列表的数据
      cb(results);
    },
    createFilter(queryString) {
      return filterSerialNumberList => {
        return (
          filterSerialNumberList.manufSN
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    handleSelect(item) {
      console.log(item);
    },
    deleteForm(res) {
      this.$confirm(
        `此操作将删除序列商序列号为${res.manufacturerSerialNumber}的表单, 是否继续?`,
        "提示",
        {
          confirmButtonText: "确定",
          cancelButtonText: "取消",
          type: "warning"
        }
      )
        .then(() => {
          this.formList = this.formList.filter(item => {
            return (
              item.manufacturerSerialNumber != res.manufacturerSerialNumber
            );
          });
          this.$message({
            type: "success",
            message: "删除成功!"
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消删除"
          });
        });
    },

  }
};
</script>

<style lang="scss" scoped>
.addClass1 {
  ::v-deep .el-dialog__header {
    .el-dialog__title {
      color: white;
    }
    .el-dialog__close {
      color: white;
    }

    background: lightslategrey;
  }
  ::v-deep .el-dialog__body {
    padding: 10px 20px;
  }
  ::v-deep .el-button--mini {
    padding: 7px 8px;
  }
  //   ::v-deep .el-dialog__footer{
  //   background: lightslategrey;
  // }
}
.rowStyle {
  ::v-deep .el-form-item {
    margin-bottom: 10px;
  }
  ::v-deep .el-switch__label {
    font-size: 10px;
  }
}
.my-autocomplete {
  li {
    line-height: normal;
    padding:2px 7px;
    height:20px;
    .name {
      text-overflow: ellipsis;
      overflow: hidden;
    }
    .addr {
      font-size: 12px;
      color: #b4b4b4;
    }

    .highlighted .addr {
      color: #ddd;
    }
  }
}
.soluClass {
  border: 1px silver solid;
  border-radius: 5px;
  padding: 0 10px;
  // overflow-x: hidden;
  // height: 40px;
  // ::-webkit-scrollbar {
  //   width: 1px !important;
  // }
}
.addClass {
  border: 1px silver solid;
  padding: 5px;
  margin-left: 20px;
}
.showSort {
  float: right;
  height: 30px;
  width: 130px;
  line-height: 30px;
  font-size: 24px;
}
</style>


      