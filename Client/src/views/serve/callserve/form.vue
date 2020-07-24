<template>
  <div>
    <div class="form" style="border:1px solid silver;">
      <el-row :gutter="20">
        <!-- <el-col :span="isEdit?24:18">右边是派单的 -->
        <el-col :span="24">
          <el-form
            :model="form"
            :rules="rules"
            :ref="form"
            class="rowStyle"
            :disabled="!isEdit"
            :label-width="labelwidth"
          >
            <div
              style="font-size:22px;text-align:center;padding-bottom:10px ; margin-bottom:10px ;border-bottom:1px solid silver;"
            >{{formName}}呼叫服务单</div>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="客户代码" prop="customerId">
                  <!-- <el-input size="mini" v-model="form.customerId" ><i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i></el-input> -->

                  <el-autocomplete
                    popper-class="my-autocomplete"
                    v-model="form.customerId"
                    size="small"
                    :fetch-suggestions="querySearch"
                    placeholder="请输入内容"
                    class="myAuto"
                    @select="handleSelect"
                  >
                    <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
                    <template slot-scope="{ item }">
                      <div class="name">
                        <p style="height:20px;margin:2px;">{{ item.cardCode }}</p>
                        <p style="font-size:12px;height:20px;margin:2px;color:silver;">{{ item.cardName }}</p>
                      </div>
                      <!-- <span class="addr">{{ item.cardName }}</span> -->
                    </template>
                  </el-autocomplete>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="服务ID">
                  <el-select size="mini" v-model="form.id" disabled placeholder="请选择">
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
                  <el-input size="mini" v-model="form.recepUserName" disabled></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="客户名称" prop="customerName">
                  <el-input size="mini" v-model="form.customerName" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="联系人" prop="contacter">
                  <el-select size="mini" v-model="form.contacter" placeholder="请选择">
                    <el-option
                      v-for="(item,index) in cntctPrsnList"
                      :key="`inx${index}`"
                      :label="item.name"
                      :value="item.name"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="最近联系人">
                  <el-input size="mini" v-model="form.newestContacter"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="终端客户">
                  <el-input size="mini" v-model="form.terminalCustomer"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="电话号码">
                  <el-input size="mini" v-model.number="form.contactTel"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <!-- <el-form-item
                  label="最新电话号码"
                  prop="newestContactTel"
                  :rules="[
                    { type: 'number', message: '电话号码格式有误'}
                  ]"
                >-->
                <el-form-item label="最新电话号码">
                  <el-input size="mini" v-model.number="form.newestContactTel"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="20">
              <el-col :span="8">
                <el-form-item label="呼叫来源" prop="fromId">
                  <el-select size="mini" v-model="form.fromId" placeholder="请选择">
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
                <el-form-item label="创建时间" prop="createTime">
                  <el-date-picker
                    size="mini"
                    v-model="form.createTime"
                    style="width: 100%;"
                    type="datetime"
                    value-format="yyyy-MM-dd hh-mm-ss"
                    placeholder="选择日期时间"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label>
                  <el-radio-group v-model="form.name" disabled>
                    <el-radio style="color:red;">售后审核:{{form.supervisor}}</el-radio>
                    <el-radio style="color:red;">销售审核:{{form.salesMan}}</el-radio>
                  </el-radio-group>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="地址标识">
                  <!-- <el-input v-model="form.addressDesignator"></el-input> -->
                  <el-select
                    size="mini"
                    v-model="form.addressDesignator"
                    @change="changeAddr"
                    placeholder="请选择"
                  >
                    <el-option
                      v-for="(item,index) in addressList"
                      :key="`inx${index}`"
                      :label="item.address"
                      :value="item.address"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="16">
                <el-input
                  size="mini"
                  style="height:40px;line-height:40px;margin-bottom:10px;"
                  v-model="form.address"
                ></el-input>
              </el-col>
            </el-row>
            <el-row :gutter="10">
              <el-col :span="8">
                <el-form-item label="现地址">
                  <el-input size="mini" v-model="form.city"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="16" style="height:40px;line-height:40px;margin-bottom:10px;">
                <el-input size="mini" v-model="form.addr">
                  <el-button size="mini" slot="append" icon="el-icon-position" @click="openMap"></el-button>
                </el-input>
              </el-col>
            </el-row>
            <!-- //上传图片组件暂时放在这里 -->
            <el-row
              v-if="isEdit"
              :gutter="10"
              type="flex"
              style="margin:0 0 10px 0 ;"
              class="row-bg"
            >
              <el-col :span="1" style="line-height:40px;"></el-col>
              <el-col :span="2" style="line-height:40px;">
                <div style="font-size:12px;color:#606266;width:100px;">上传图片</div>
              </el-col>
              <el-col :span="18">
                <upLoadImage setImage="100px" @get-ImgList="getImgList"></upLoadImage>
              </el-col>
              <el-col :span="2" style="line-height:40px;">
                <el-button
                  type="primary"
                  size="small"
                  v-if="isEditForm"
                  icon="el-icon-share"
                  @click="postService"
                >确定修改</el-button>
              </el-col>
            </el-row>
            <el-row
              v-if="!isEdit&&form.serviceOrderPictures.length"
              :gutter="10"
              type="flex"
              style="margin:0 0 10px 0 ;"
              class="row-bg"
            >
              <el-col :span="1" style="line-height:40px;"></el-col>
              <el-col :span="2" style="line-height:40px;">
                <div style="font-size:12px;color:#606266;width:100px;">上传图片列表</div>
              </el-col>
              <el-col :span="20" v-if="form.serviceOrderPictures.length">
                <div class="demo-image__lazy">
                  <el-image
                    style="width:60px;height:50px;display:inline-block;margin:0 10px;"
                    @click="handlePreviewFile(`${baseURL}/files/Download/${url.pictureId}?X-Token=${tokenValue}`)"
                    v-for="url in form.serviceOrderPictures"
                    :key="url.id"
                    :src="`${baseURL}/files/Download/${url.pictureId}?X-Token=${tokenValue}`"
                    lazy
                  ></el-image>
                </div>
              </el-col>
            </el-row>
            <el-row
              :gutter="10"
              type="flex"
              style="margin:0 0 10px 0 ;"
              class="row-bg"
              justify="space-around"
            >
              <el-col :span="20"></el-col>
            </el-row>
            <!-- 选择制造商序列号 -->
            <formAdd
              :isEdit="isEditAdd"
              :isEditForm="isEditForm"
              @change-form="changeForm"
              :serviceOrderId="serviceOrderId"
              :propForm="propForm"
            ></formAdd>
            <!-- <formAdd :SerialNumberList="SerialNumberList"></formAdd> -->
          </el-form>
        </el-col>
        <!-- <el-col :span="6" class="lastWord" v-if="!isEdit" >   暂时不用派单
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
        </el-col>-->
      </el-row>
      <el-dialog title="选择地址" width="1000px" :visible.sync="drawerMap" direction="ttb">
        <zmap @drag="dragmap"></zmap>
        <el-row :gutter="12" slot="footer" class="dialog-footer" style="height:30px;">
          <el-col :span="20">当前选择:{{allAddress.address?allAddress.address:'暂未选择地点'}}</el-col>
          <el-col :span="4" style="height:30px;line-height:30px;">
            <el-button type="primary" size="mini" style="margin-top:10px;" @click="chooseAddre">确定选择</el-button>
          </el-col>
        </el-row>
      </el-dialog>
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
          <el-button type="primary" @click="sureVal">确 定</el-button>
        </span>
      </el-dialog>
      <Model
        :visible="previewVisible"
        @on-close="previewVisible = false"
        ref="formPreview"
        width="600px"
        form
      >
        <img :src="previewUrl" alt style="display: block;width: 80%;margin: 0 auto;" />
        <template slot="action">
          <el-button size="mini" @click="previewVisible = false">关闭</el-button>
        </template>
      </Model>
    </div>
  </div>
</template>

<script>
import { getPartner } from "@/api/callserve";
import * as callservesure from "@/api/serve/callservesure";
import Pagination from "@/components/Pagination";
import zmap from "@/components/amap";
import upLoadImage from "@/components/upLoadFile";
import Model from "@/components/Formcreated/components/Model";

import formPartner from "./formPartner";
import formAdd from "./formAdd";
export default {
  name: "formTable",
  components: { formPartner, formAdd, zmap, Pagination, upLoadImage, Model },
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
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
      previewUrl: "", //预览图片的定义

      partnerList: [],
      previewVisible: false, //图片预览的dia
      setImage: {
        width: "100px",
        height: "100px"
      },
      drawerMap: false, //地图控件
      filterPartnerList: [],
      inputSearch: "",
      inputSerial: "",
      // SerialNumberList: [], //序列号列表
      state: "",
      addressList: [], //用户地址列表
      cntctPrsnList: [], //联系人列表
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
        { label: "电话", value: 1 },
        { label: "钉钉", value: 2 },
        { label: "QQ", value: 3 },
        { label: "微信", value: 4 },
        { label: "邮件", value: 5 },
        { label: "APP", value: 6 },
        { label: "web", value: 7 }
      ],
      serviceOrderId: "",
      checkVal:{},
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
        recepUserName: "System", //接单员
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
        fromId: 1, //integer($int32)呼叫来源 1-电话 2-APP
        pictures: [], //
        serviceWorkOrders: []
      },
      isEditAdd: true, //add页面的编辑状态
      allAddress: {}, //选择地图的合集
      propForm: [],
      rules: {
        customerId: [
          { required: true, message: "请输入客户代码", trigger: "change" }
        ],
        customerName: [
          { required: true, message: "请输入客户名称", trigger: "change" }
        ],
        fromId: [
          { required: true, message: "请输入呼叫来源", trigger: "change" }
        ],
        contacter: [
          { required: true, message: "请选择联系人", trigger: "change" }
        ],
        createTime: [
          { required: true, message: "请选择创建时间", trigger: "change" }
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
        this.setForm(val);
      }
    },
    sure: {
      handler(val) {
        if (val) {
          //接受确定通知，开始提交订单
          this.postServe("form");
        }
      }
    },

    "form.customerId": {
      handler(val) {
        this.getPartnerInfo(val);
      }
    },
    refValue: {
      deep: true,
      handler(val) {
        if (val) {
          Object.assign(this.form, val);
          this.propForm = this.refValue.serviceWorkOrders;
        }

        // this.propForm = this.refValue.serviceWorkOrders
      },
      immediate: true
    }
  },
  created() {
    //  Object.assign(this.form,this.refValue)
    // this.propForm = this.refValue.serviceWorkOrders
  },
  provide: function() {
    return {
      form: this.form
    };
  },
  mounted() {
    this.getPartnerList();
    this.setForm(this.customer);
    this.isEditAdd = this.isEdit;
  },
  methods: {
    handlePreviewFile(item) {
      //预览图片
      this.previewVisible = true;
      this.previewUrl = item;
    },
    getImgList(val) {
      //获取图片列表

      this.form.pictures = val;
      console.log(this.form);
    },
    dragmap(res) {
      this.allAddress = res;
    },
    chooseAddre() {
      this.form.city = this.allAddress.regeocode.addressComponent.city;
      this.form.addr = this.allAddress.address;
      this.longitude = this.allAddress.position.lng;
      this.latitude = this.allAddress.position.latss;
      this.drawerMap = false;
    },
    setForm(val) {
      Object.assign(this.form, val);
      this.form.recepUserName = this.$store.state.user.name;
    },

    postServe() {
      //创建整个工单

      if (this.form.serviceWorkOrders.length > 0) {
        let chec = this.form.serviceWorkOrders.every(
          item =>
            item.fromTheme !== "" &&
            item.fromType !== "" &&
            item.problemTypeId !== ""
        );
        this.form.serviceWorkOrders = this.form.serviceWorkOrders.map(item => {
          item.problemTypeId = item.problemType;
          item.solutionId = item.solution;
          return item;
        });
        if (chec) {
          if (this.$route.path === "/serve/callserve") {
            callservesure
              .CreateOrder(this.form)
              .then(() => {
                this.$message({
                  message: "创建服务单成功",
                  type: "success"
                });
                this.$emit("close-Dia", "y");
              })
              .catch(res => {
                this.$message({
                  message: `${res}`,
                  type: "error"
                });
              });
          } else {
            callservesure
              .CreateWorkOrder(this.form)
              .then(() => {
                this.$message({
                  message: "创建服务单成功",
                  type: "success"
                });
                this.$emit("close-Dia", "y");
              })
              .catch(res => {
                this.$message({
                  message: `${res}`,
                  type: "error"
                });
              });
          }
        } else {
          this.$message({
            message: `请将必填项填写完整`,
            type: "error"
          });
        }
      } else {
        this.$message({
          message: `请将必填项填写完整`,
          type: "error"
        });
      }
    },
    getPartnerInfo(num) {
      callservesure
        .forServe(num)
        .then(res => {
          this.addressList = res.result.addressList;
          this.cntctPrsnList = res.result.cntctPrsnList;
          this.form.supervisor = res.result.techID;
          // this.$message({
          //   message: "修改服务单成功",
          //   type: "success"
          // });
        })
        .catch(() => {
          // this.$message({
          //   message: `未找到此客户代码对应信息，请检查`,
          //   type: "error",
          //   duration: "5000"
          // });
        });
    },
    changeAddr(val) {
      let res = this.addressList.filter(item => item.address == val);
      this.form.address = res[0].building;
    },
    changeForm(val) {
      this.form.serviceWorkOrders = val;
      console.log(this.form);
    },
    postService() {
      //更新服务单
      callservesure
        .updateService(this.form)
        .then(() => {
          this.$message({
            message: "修改服务单成功",
            type: "success"
          });
          this.$emit("close-Dia", 1);
          // this.formUpdate = false
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
      this.form.customerId = this.inputSearch;

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
    },
    querySearch(queryString, cb) {
      this.listQuery.CardCodeOrCardName = queryString;
      this.inputSearch = queryString;

      this.getPartnerList();
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
      this.inputSearch = item.customerId;
      this.form.customerId = item.cardCode;
      this.form.customerName = item.cardName;
      this.form.contacter = item.cntctPrsn;
      this.form.contactTel = item.cellular;
      // this.form.addressDesignator = item.address;
      this.form.address = item.address;
      this.form.salesMan = item.slpName;
    },
    sureVal(){
      this.dialogPartner = false
      console.log(this.checkVal)
      let val = this.checkVal
            this.form.customerId = val.cardCode;
      this.form.customerName = val.cardName;
      this.form.contacter = val.cntctPrsn;
      this.form.contactTel = val.cellular;
      // this.form.addressDesignator = val.address;
      this.form.address = val.address;
      this.form.salesMan = val.slpName;
    },
    ChildValue(val) {
      this.checkVal =val
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
      // this.$nextTick(() => {
      //   this.$refs["form"].clearValidate();
      // });
    }
  },
  handleCreate() {
    // 弹出添加框
    this.resetTemp();
    this.dialogStatus = "create";
    this.dialogFormVisible = true;
    // this.$nextTick(() => {
    //   this.$refs["form"].clearValidate();
    // });
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
.rowStyle {
  ::v-deep .el-form-item {
    margin-bottom: 5px;
  }
}
.myAuto {
  ::v-deep.el-autocomplete-suggestion {
    li {
      height: 20px;
      line-height: 20px;
    }
  }
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
