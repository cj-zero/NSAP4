<template>
  <div>
    <div class="form" style="border:1px solid silver;;">
      <el-row :gutter="20">
        <el-col :span="isEdit?24:18">
          <el-form
            :model="form"
            :rules="rules"
            :ref="refValue"
            :disabled="!isEdit"
            :label-width="labelwidth"
          >
            <div
              style="font-size:22px;text-align:center;padding-bottom:10px ; margin-bottom:10px ;border-bottom:1px solid silver;"
            >{{formName}}呼叫服务单</div>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="客户代码" prop="customerId">
                  <el-autocomplete
                    popper-class="my-autocomplete"
                    v-model="form.customerId"
                    :fetch-suggestions="querySearch"
                    placeholder="请输入内容"
                    @select="handleSelect"
                  >
                    <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
                    <template slot-scope="{ item }">
                      <div class="name">{{ item.cardCode }}</div>
                      <span class="addr">{{ item.cardName }}</span>
                    </template>
                  </el-autocomplete>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="服务ID">
                  <el-select v-model="form.id" disabled placeholder="请选择">
                    <el-option label="服务一" value="shanghai"></el-option>
                    <el-option label="服务二" value="beijing"></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <!-- <el-form-item label="服务合同">
                <el-input v-model="form.contractId" disabled></el-input>
                </el-form-item>-->
                <el-form-item label="接单员">
                  <el-input v-model="form.recepUserName" disabled></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="客户名称" prop="customerName">
                  <el-input v-model="form.customerName" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="联系人">
                  <el-select v-model="form.contacter" placeholder="请选择">
                    <el-option label="服务一" value="shanghai"></el-option>
                    <el-option label="服务二" value="beijing"></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="最近联系人">
                  <el-input v-model="form.newestContacter"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="终端客户">
                  <el-input v-model="form.terminalCustomer"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="电话号码">
                  <el-input v-model="form.contactTel"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="最新电话号码">
                  <el-input v-model="form.newestContactTel"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="20">
              <el-col :span="8">
                <el-form-item label="呼叫来源" prop="fromId">
                  <el-select v-model="form.fromId" placeholder="请选择">
                    <el-option
                      v-for="item in callSourse"
                      :key="item.value"
                      :label="item.label"
                      :value="item.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="创建日期">
                  <el-date-picker v-model="form.createTime" type="date" placeholder="选择日期时间"></el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label>
                  <el-radio-group v-model="form.name" disabled>
                    <el-radio>售后主管:{{form.supervisor}}</el-radio>
                    <el-radio>售后审核:{{form.salesMan}}</el-radio>
                  </el-radio-group>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="地址标识">
                  <el-input v-model="form.addressDesignator"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="16">
                <el-input v-model="form.address"></el-input>
              </el-col>
            </el-row>
            <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="现地址">
                  <el-input v-model="form.city"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="16">
                <el-input v-model="form.addr">
                  <el-button slot="append" icon="el-icon-position" @click="openMap"></el-button>
                </el-input>
              </el-col>
            </el-row>
            <el-row
              :gutter="10"
              type="flex"
              style="margin:0 0 10px 0 ;"
              class="row-bg"
              justify="space-around"
            >
              <el-col :span="20">
                <!-- <el-input v-model="form.addr">
                  <el-button slot="append" icon="el-icon-position" @click="openMap"></el-button>
                </el-input>-->
              </el-col>
              <el-col :span="4" style="line-height:40px;">
                <el-button
                  type="primary"
                  v-if="isEditForm"
                  icon="el-icon-share"
                  @click="postService"
                >确定</el-button>
              </el-col>
            </el-row>
            <!-- 选择制造商序列号 -->
            <formAdd
              :isEdit="isEdit"
              :isEditForm="isEditForm"
              @change-form="changeForm"
              :serviceOrderId="serviceOrderId"
            ></formAdd>
            <!-- <formAdd :SerialNumberList="SerialNumberList"></formAdd> -->

            <el-drawer
              title="我是标题"
              size="70%"
              :with-header="false"
              :visible.sync="drawerMap"
              direction="ttb"
            >
              <zmap></zmap>
            </el-drawer>
            <el-dialog
              title="选择业务伙伴"
              class="addClass1"
              width="90%"
              @open="openDialog"
              :visible.sync="dialogPartner"
            >
              <el-form :inline="true" class="demo-form-inline">
                <el-form-item label="客户代码:">
                  <el-input @input="searchList" v-model="inputSearch" placeholder="客户代码"></el-input>
                </el-form-item>
                <el-form-item label="制造商序列号:">
                  <el-input @input="searSerial" v-model="inputSerial" placeholder="制造商序列号"></el-input>
                </el-form-item>
                <!-- <el-form-item>
    <el-button type="primary" @click="onSubmit">查询</el-button>
                </el-form-item>-->
              </el-form>
              <formPartner
                :partnerList="filterPartnerList"
                :count="parentCount"
                @getChildValue="ChildValue"
              ></formPartner>
              <pagination
                v-show="parentCount>0"
                :total="parentCount"
                :page.sync="listQuery.page"
                :limit.sync="listQuery.limit"
                @pagination="handleChange"
              />
              <span slot="footer" class="dialog-footer">
                <el-button @click="dialogPartner = false">取 消</el-button>
                <el-button type="primary" @click="dialogPartner = false">确 定</el-button>
              </span>
            </el-dialog>
          </el-form>
        </el-col>
        <el-col :span="6" class="lastWord" v-if="!isEdit">
          <el-collapse accordion>
            <el-collapse-item
              :title="`技术员：${item.people}`"
              :key="`key_${index}`"
              v-for="(item, index) in wordList"
              :name="index"
            >
              <el-form label-position="top" label-width="80px" :model="item">
                <el-form-item label="留言">
                  <el-input v-model="item.word"></el-input>
                </el-form-item>
                <el-form-item label="活动形式">
                  <el-input v-model="item.img"></el-input>
                </el-form-item>
                <el-button type="primary">立即创建</el-button>
              </el-form>
            </el-collapse-item>
          </el-collapse>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script>
import { getPartner } from "@/api/callserve";
import * as callservesure from "@/api/serve/callservesure";
import Pagination from "@/components/Pagination";
import zmap from "@/components/amap";
import formPartner from "./formPartner";
import formAdd from "./formAdd";
export default {
  name: "formTable",
  components: { formPartner, formAdd, zmap, Pagination },
  props: [
    "modelValue",
    "refValue",
    "labelposition",
    "labelwidth",
    "isEdit",
    "formName",
    "isEditForm",
    "customer",
    "sure"
  ],
  //  ##isEdit是否可以编辑  ##customer获取服务端的信息
  data() {
    return {
      partnerList: [],
      drawerMap: false, //地图控件
      filterPartnerList: [],
      inputSearch: "",
      inputSerial: "",
      // SerialNumberList: [], //序列号列表
      state: "",

      parentCount: "",
      dialogPartner: false,
      activeName: 1,
      // dataModel: this.models[this.formData.model],
      dataModel: null,
      wordList: [
        { people: "张工", word: "123", img: "" },
        { people: "刘总", word: "123", img: "" },
        { people: "实习生小王", word: "123", img: "" },
        { people: "老刘", word: "123", img: "" },
        { people: "门卫", word: "123", img: "" },
        { people: "实习生小李", word: "123", img: "" }
      ],
      callSourse: [
        { label: "电话" ,value:1},
        { label: "钉钉" ,value:2},
   
      ],
      serviceOrderId: "",
      form: {
        customerId: "", //客户代码,
        customerName: "", //客户名称,
        contacter: "", //联系人,
        contactTel: "", //联系人电话,
        supervisor: "", //主管名字,
        supervisorId: "", //主管用户Id,
        salesMan: "", //销售名字,
        salesManId: "", //销售用户Id,fv
        newestContacter: "", //最新联系人,
        newestContactTel: "", //最新联系人电话号码,
        terminalCustomer: "", //终端客户,recepUserName
        contractId: "", //服务合同
        recepUserName:'System',//接单员
        addressDesignator: "", //地址标识
        recepUserId: "", //接单人用户ID
        address: "", //详细地址
        createTime: "",
        id: "", //服务单id
        province: "深圳", //省
        city: "", //市
        area: "",
        addr: "", //地区
        longitude: "", //number经度
        latitude: "", //	number纬度
        fromId: "", //integer($int32)呼叫来源 1-电话 2-APP
        pictures: [{ pictureId: "" }], //
        serviceWorkOrders: []
      },
      rules: {
        customerId: [
          { required: true, message: "请输入客户代码", trigger: "blur" }
        ],
        customerName: [
          { required: true, message: "请输入客户名称", trigger: "change" }
        ],
        newestContactTel: [
          { required: true, message: "请输入呼叫来源", trigger: "change" }
        ]
      },
      listQuery: {
        page: 1,
        limit: 10
      }
    };
  },
  watch: {
    isEditForm: {
      handler(val) {
        console.log(val);
      }
    },
    customer: {
      handler(val) {
     this.setForm(val)
      }
    },
    sure:{
      handler(val){
        if(val==true){
          this.postServe()
        }
      }
    }
  },
  mounted() {
    this.getPartnerList();
    this.getPartnerInfo()
    this.setForm(this.customer)
  },
  methods: {
    setForm(val){
      Object.assign(this.form,val) 
      this.form.recepUserName= this.$store.state.user.name

      // this.form.
  //  this.form.customerId = val.customerId;
  //       this.form.customerName = val.customerName;
  //       this.form.contacter = val.contacter;
  //       this.form.createTime = val.createTime;
  //       this.form.supervisor = val.supervisor;
  //       this.form.salesMan = val.salesMan;
  //       this.form.city = val.city;
  //       this.form.id = val.id;
        // localStorage.setItem("serviceOrderId", val.id);
    },
    postServe() {
      this.form.province="深圳"
      callservesure
        .CreateWorkOrder(this.form)
        .then(() => {
          this.$message({
            message: "创建服务单成功",
            type: "success"
          });
          this.$emit('close-Dia',"y")
        })
        .catch(res => {
          this.$message({
            message: `${res}`,
            type: "error"
          });
        });
    },
    getPartnerInfo(){
      callservesure.forServe()
    },
    changeForm(val) {
      this.form.serviceWorkOrders = val;
      console.log(this.form);
    },
    postService() {
      callservesure
        .updateService(this.form)
        .then(() => {
          this.$message({
            message: "修改服务单成功",
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
    usecustomerId(val) {
      console.log(val);
    },
    handleChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      this.getPartnerList();
    },
    handleClose() {
      //  ##地图关闭之前执行的操作
      console.log(22);
      //  this.drawerMap=false
    },
    openMap() {
      this.drawerMap = true;
    },
    openDialog() {
      //打开前赋值
      this.filterPartnerList = this.partnerList;
    },
    searchList() {
      this.listQuery.CardCodeOrCardName = this.inputSearch;
      this.getPartnerList();
      // if (!res) {
      //   this.filterPartnerList = this.partnerList;
      // } else {
      //   let list = this.partnerList.filter(item => {
      //     return item.cardCode.toLowerCase().indexOf(res.toLowerCase()) === 0;
      //   });
      //   this.filterPartnerList = list;
      // }
    },
    searSerial() {
      // SerialList
      this.listQuery.ManufSN = this.inputSerial;
      this.getPartnerList();
      // if (!res) {
      //   this.filterPartnerList = this.partnerList;
      // } else {
      //   let list = this.partnerList.filter(item => {
      //     console.log(item)
      //     // return item.internalSerialNumber.toLowerCase().indexOf(res.toLowerCase()) === 0;
      //   });
      //   this.filterPartnerList = list;
      // }
    },
    querySearch(queryString, cb) {
      var partnerList = this.partnerList;
      var results = queryString
        ? partnerList.filter(this.createFilter(queryString))
        : partnerList;
      // 调用 callback 返回建议列表的数据
      cb(results);
    },
    createFilter(queryString) {
      return partnerList => {
        return (
          partnerList.cardCode
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    getPartnerList() {
      getPartner(this.listQuery)
        .then(res => {
          this.partnerList = res.data;
          this.filterPartnerList = this.partnerList;
          this.parentCount = res.count;
          // console.log(res.count)
        })
        .catch(error => {
          console.log(error);
        });
    },
    handleSelect(item) {
      this.form.customerId = item.cardCode;
      this.form.customerName = item.cardName;
      this.form.contacter = item.cntctPrsn;
      this.form.contactTel = item.cellular;
      this.form.addressDesignator = item.address;
      this.form.salesMan = item.slpName;
    },
    ChildValue(val) {
      console.log(val);
      this.form.customerId = val.cardCode;
      this.form.customerName = val.cardName;
      this.form.contacter = val.cntctPrsn;
      this.form.contactTel = val.cellular;
      this.form.addressDesignator = val.address;
      this.form.salesMan = val.slpName;
    },
    handleIconClick() {
      this.dialogPartner = true;
    },
    onSubmit() {
      console.log("submit!");
    },
    handleUpdate(row) {
      // 弹出编辑框
      this.temp = Object.assign({}, row); // copy obj
      this.dialogStatus = "update";
      this.dialogFormVisible = true;
      this.$nextTick(() => {
        // this.$refs["dataForm"].clearValidate();
      });
    }
  },
  handleCreate() {
    // 弹出添加框
    this.resetTemp();
    this.dialogStatus = "create";
    this.dialogFormVisible = true;
    this.$nextTick(() => {
      // this.$refs["dataForm"].clearValidate();
    });
  }
};
</script>

<style lang="scss" scoped>
.form {
  .lastWord {
    position: sticky;
    top: 0;
    // width: 200px;
  }
}
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

  //   ::v-deep .el-dialog__footer{
  //   background: lightslategrey;
  // }
}
.my-autocomplete {
  li {
    line-height: normal;
    padding: 7px;

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
</style> 
