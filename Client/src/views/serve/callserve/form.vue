<template>
  <div>
    <div class="form" style="border:1px solid silver;padding:0 5px;">
      <el-row :gutter="20">
        <!-- <el-col :span="isCreate?24:18"> -->
        <el-col :span="24">
          <el-form
            :model="form"
            :rules="rules"
            ref="form"
            class="rowStyle1"
            :disabled="!isCreate"
            :label-width="labelwidth"
            :inline-message="true"
            :show-message="false"
          >
            <div
              style="font-size:22px;color:#67C23A;text-align:center;height:40px;line-height:35px;border-bottom:1px solid silver;margin-bottom:10px;"
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
                        <p
                          style="font-size:12px;height:20px;margin:2px;color:silver;"
                        >{{ item.cardName }}</p>
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
                  <el-select
                    size="mini"
                    @change="choosePeople"
                    v-model="form.contacter"
                    placeholder="请选择"
                  >
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
                  <el-input size="mini" v-model.number="form.contactTel" disabled></el-input>
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
                <el-form-item label="最新电话号码" prop="newestContactTel">
                  <el-input size="mini" v-model="form.newestContactTel"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="20">
              <el-col :span="8">
                <el-form-item label="呼叫来源" prop="fromId">
                  <el-select
                    size="mini"
                    style="width:150px;"
                    v-model="form.fromId"
                    placeholder="请选择"
                  >
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
                <el-form-item label="创建时间" label-width="95px" prop="createTime">
                  <el-date-picker
                    @focus="setThisTime"
                    :clearable="false"
                    size="mini"
                    v-model="form.createTime"
                    :default-time="newDate"
                    style="width:150px;"
                    type="datetime"
                    format="yyyy-MM-dd HH:mm"
                    value-format="yyyy-MM-dd HH-mm-ss"
                    placeholder="选择日期时间"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label label-width="70px">
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
                  disabled
                  style="height:30px;line-height:30px;padding:2px 0 0 0;"
                  v-model="form.address"
                ></el-input>
              </el-col>
            </el-row>
            <el-row :gutter="10">
              <el-col :span="8">
                <el-form-item label="现地址">
                  <!-- <el-input size="mini" v-model="form.city"></el-input> -->
                  <p
                    style="border: 1px solid silver; border-radius:5px;height:30px;margin:0;padding-left:10px;font-size:12px;"
                  >{{allArea}}</p>
                </el-form-item>
              </el-col>
              <el-col :span="16" style="height:30px;line-height:30px;padding:2px 0 0 0;">
                <el-input size="mini" v-model="form.addr">
                  <el-button size="mini" slot="append" icon="el-icon-position" @click="openMap"></el-button>
                </el-input>
              </el-col>
            </el-row>
            <!-- //上传图片组件暂时放在这里 -->
            <el-row
              v-if="isCreate"
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
                <upLoadImage :setImage="setImage" @get-ImgList="getImgList"></upLoadImage>
              </el-col>
              <!-- <el-col :span="2" style="line-height:40px;">  暂时取消
                <el-button
                  type="primary"
                  size="small"
                  v-if="ifEdit"
                  icon="el-icon-share"
                  @click="postService"
                >确定修改</el-button>
              </el-col>-->
            </el-row>
            <el-row
              v-if="form.serviceOrderPictures&&form.serviceOrderPictures.length"
              :gutter="10"
              type="flex"
              style="margin:0 0 10px 0 ;"
              class="row-bg"
            >
              <el-col :span="1" style="line-height:40px;"></el-col>
              <el-col :span="2" style="line-height:40px;">
                <div style="font-size:12px;color:#606266;width:120px;">已上传图片</div>
              </el-col>
              <el-col :span="20" v-if="form.serviceOrderPictures&&form.serviceOrderPictures.length">
                <div class="demo-image__lazy">
                  <el-image
                    style="width:60px;height:50px;display:inline-block;margin:0 10px;"
                    v-for="url in form.serviceOrderPictures"
                    @click="handlePreviewFile(`${baseURL}/files/Download/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`)"
                    :key="url.id"
                    :src="`${baseURL}/files/Download/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`"
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
              :isCreate="isCreateAdd"
              :ifEdit="ifEdit"
              @change-form="changeForm"
              :serviceOrderId="serviceOrderId"
              :propForm="propForm"
            ></formAdd>
            <!-- <formAdd :SerialNumberList="SerialNumberList"></formAdd> -->
          </el-form>
        </el-col>
      </el-row>
      <el-dialog
        title="选择地址"
        width="1000px"
        :destroy-on-close="true"
        :visible.sync="drawerMap"
        direction="ttb"
      >
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
        :destroy-on-close="true"
        :visible.sync="dialogPartner"
      >
        <el-form :inline="true" class="demo-form-inline">
          <el-form-item label="客户">
            <el-input @input="searchList" v-model="inputSearch" placeholder="客户"></el-input>
          </el-form-item>
          <el-form-item label="制造商序列号:">
            <el-input @input="searSerial" v-model="inputSerial" placeholder="制造商序列号"></el-input>
          </el-form-item>
        </el-form>
        <formPartner
          :partnerList="filterPartnerList"
          :count="parentCount"
          :parLoading="parentLoad"
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
          <el-button type="primary" @click="sureVal" :disabled="disableBtn">确 定</el-button>
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
import http from "@/api/serve/whiteHttp";
import * as callservesure from "@/api/serve/callservesure";
import Pagination from "@/components/Pagination";
import zmap from "@/components/amap";
import upLoadImage from "@/components/upLoadFile";
import Model from "@/components/Formcreated/components/Model";
import { timeToFormat } from "@/utils";
import { isMobile, isPhone } from "@/utils/validate";

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
    "isCreate",
    "formName",
    "ifEdit", //是否是编辑页面
    "customer",
    "sure",
  ],
  //  ##isCreate是否可以编辑  ##look只能看   ##create新增页  ##customer获取服务端对比的信息
  //customer确认订单时传递的信息
  data() {
    var checkTelF = (rule, value, callback) => {
      if (!value) {
        return callback();
      }
      setTimeout(() => {
        // let reg = RegExp(/^[\d-]+$/);
        if (isMobile(value) || isPhone(value)) {
          callback();
        } else {
          // callback(new Error('请输入正确的格式'));
          callback(this.$message.error("请输入正确的电话格式"));
        }
      }, 500);
    };
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
      previewUrl: "", //预览图片的定义

      partnerList: [],
      previewVisible: false, //图片预览的dia
      setImage: '50px',
      drawerMap: false, //地图控件
      filterPartnerList: [],
      inputSearch: "",
      inputSerial: "",
      // SerialNumberList: [], //序列号列表
      state: "",
      addressList: [], //用户地址列表
      cntctPrsnList: [], //联系人列表
      parentCount: 0,
      dialogPartner: false,
      activeName: 1,
      // dataModel: this.models[this.formData.model],
      dataModel: null,
      parentLoad: false,
      callSourse: [
        { label: "电话", value: 1 },
        { label: "钉钉", value: 2 },
        { label: "QQ", value: 3 },
        { label: "微信", value: 4 },
        { label: "邮件", value: 5 },
        { label: "APP", value: 6 },
        { label: "web", value: 7 },
      ],
      serviceOrderId: "",
      checkVal: {},
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
        recepUserName: "", //接单员
        addressDesignator: "", //地址标识
        recepUserId: "", //接单人用户ID
        address: "", //详细地址
        createTime: timeToFormat("yyyy-MM-dd HH-mm-ss"),
        id: "", //服务单id
        province: "", //省
        city: "", //市
        area: "", //区
        addr: "", //地区
        longitude: "", //number经度
        latitude: "", //	number纬度
        fromId: 1, //integer($int32)呼叫来源 1-电话 2-APP
        pictures: [], //
        serviceWorkOrders: [],
      },
      disableBtn: false,
      newDate: [],
      isCreateAdd: true, //add页面的编辑状态
      allAddress: {}, //选择地图的合集
      propForm: [],
      rules: {
        customerId: [
          { required: true, message: "请输入客户代码", trigger: "change" },
        ],
        customerName: [
          { required: true, message: "请输入客户名称", trigger: "change" },
        ],
        fromId: [
          { required: true, message: "请输入呼叫来源", trigger: "change" },
        ],
        contacter: [
          { required: true, message: "请选择联系人", trigger: "change" },
        ],
        createTime: [
          { required: true, message: "请选择创建时间", trigger: "change" },
        ],
        newestContactTel: [{ validator: checkTelF, trigger: "blur" }],
      },
      listQuery: {
        page: 1,
        limit: 40,
      },
    };
  },
  computed: {
    allArea() {
      return this.form.province + this.form.city + this.form.area;
    },
  },
  watch: {
    ifEdit: {
      handler(val) {
        console.log(val);
      },
    },
    customer: {
      handler(val) {
        this.setForm(val);
      },
    },
    sure: {
      handler(val) {
        if (val) {
          //接受确定通知，开始提交订单
          this.postServe("form");
        }
      },
    },
    "form.addr":{
      handler(val){
      if (val) {
  
             let addre = this.form.province + this.form.city + this.form.area +this.form.addr    
this.getPosition(addre)
        }
              }
    },
    "form.address": {
      handler(val) {
        if (!this.form.addr) {
           this.getPosition(val)

        }
      },
    },
    "form.customerId": {
      handler(val) {
        this.getPartnerInfo(val);
      },
    },
    refValue: {
      deep: true,
      handler(val) {
        if (val) {
          Object.assign(this.form, val);
          if (val.serviceWorkOrders.length > 0) {
            val.serviceWorkOrders.map((item, index) => {
              this.form.serviceWorkOrders[index].solutionsubject =
                item.solution && item.solution.subject;
              this.form.serviceWorkOrders[index].problemTypeName =
                item.problemType && item.problemType.name;
            });
          }
          this.propForm = this.form.serviceWorkOrders;
        }
        // this.propForm = this.refValue.serviceWorkOrders
      },
      immediate: true,
    },
  },
  created() {
    //  Object.assign(this.form,this.refValue)
    // this.propForm = this.refValue.serviceWorkOrders
  },
  provide: function () {
    return {
      form: this.form,
    };
  },
  mounted() {
    this.getPartnerList();
    this.setForm(this.customer);
    this.isCreateAdd = this.isCreate;
  },
  methods: {
    setThisTime() {
      console.log(11);
    },
    handlePreviewFile(item) {
      //预览图片
      this.previewVisible = true;
      this.previewUrl = item;
    },
    getPosition(val){
          let that = this;
          let url = `https://restapi.amap.com/v3/geocode/geo?key=c97ee5ef9156461c04b552da5b78039d&address=${encodeURIComponent(val)}`;
          http.get(url, function (err, result) {
            if (result.geocodes.length) {
              let res = result.geocodes[0];
              that.form.province = res.province;
              that.form.city = res.city;
              that.form.area = res.district;
              that.form.addr = res.formatted_address;
              that.form.latitude = res.location.split(",")[1];
              that.form.longitude = res.location.split(",")[0];
            } else {
              if(that.isCreate||that.ifEdit){
              that.$message({
                message: "未识别到地址，请手动选择",
                type: "error",
              });
              }

              that.form.province = "";
              that.form.city = "";
              that.form.area = "";
              that.form.addr = "";
              that.form.latitude = "";
              that.form.longitude = "";
            }
            // 这里对结果进行处理
          });
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
      this.form.province = this.allAddress.regeocode.addressComponent.province;
      this.form.area = this.allAddress.regeocode.addressComponent.district;
      this.form.addr = this.allAddress.address;
      this.form.longitude = this.allAddress.position.lng;
      this.form.latitude = this.allAddress.position.lat;
      this.drawerMap = false;
      console.log(this.form);
    },
    async setForm(val) {
      if (val) {
        val.serviceOrderPictures = await this.getServeImg(val.id);
      }
      Object.assign(this.form, val);
      this.form.recepUserName = this.$store.state.user.name;
    },
    async getServeImg(val) {
      let params = {
        id: val,
        type: 1,
      };
      let imgList = [];
      await callservesure.GetServiceOrderPictures(params).then((res) => {
        imgList = res.result ? res.result : [];
      });
      return imgList;
    },
    async postServe() {
      //创建整个工单
      if (!this.form.serviceWorkOrders.length) {
        this.$message({
          message: `请将必填项填写完整`,
          type: "error",
        });
        return;
      }
      if(!this.form.longitude){
          this.$message({
          message: `请手动选择地址`,
          type: "error",
        });
        return;
      }
      if (this.form.serviceWorkOrders.length >= 0) {
        let chec = this.form.serviceWorkOrders.every(
          (item) =>
            item.fromTheme !== "" &&
            item.fromType !== "" &&
            item.problemTypeId !== "" &&
            item.manufacturerSerialNumber !== "" &&
            (item.fromType === 2 ? item.solutionId !== "" : true)
        );
        // this.form.serviceWorkOrders = this.form.serviceWorkOrders.map(item => {
        //   item.problemTypeId = item.problemTypeName;
        //   item.solutionId = item.solution;
        //   return item;
        // });
        this.isValid = await this.$refs.form.validate();

        if (chec && this.isValid) {
          if (this.$route.path === "/serve/callserve") {
            callservesure
              .CreateOrder(this.form)
              .then(() => {
                this.$message({
                  message: "创建服务单成功",
                  type: "success",
                });
                this.$emit("close-Dia", "y");
              })
              .catch((res) => {
                this.$message({
                  message: `${res}`,
                  type: "error",
                });
              });
          } else {
            console.log(this.form);
            callservesure
              .CreateWorkOrder(this.form)
              .then(() => {
                this.$message({
                  message: "创建服务单成功",
                  type: "success",
                });
                this.$emit("close-Dia", "y");
              })
              .catch((res) => {
                this.$message({
                  message: `${res}`,
                  type: "error",
                });
              });
          }
        } else {
          this.$message({
            message: `请将必填项填写完整`,
            type: "error",
          });
        }
      }
    },
    getPartnerInfo(num) {
      callservesure
        .forServe(num)
        .then((res) => {
          this.addressList = res.result.addressList;
          this.cntctPrsnList = res.result.cntctPrsnList;
          this.form.supervisor = res.result.techID;
          if (this.addressList.length) {
            let { address, building } = this.addressList[0];
            this.form.addressDesignator = address;
            this.form.address = building;
            // this.form.address = building;
          }
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
      let res = this.addressList.filter((item) => item.address == val);
      this.form.address = res[0].building;
    },
    choosePeople(val) {
      let res = this.cntctPrsnList.filter((item) => item.name == val);
      this.form.contactTel = res[0].tel1;
    },
    changeForm(val) {
      this.form.serviceWorkOrders = val;
    },
    postService() {
      //更新服务单
      callservesure
        .updateService(this.form)
        .then(() => {
          this.$message({
            message: "修改服务单成功",
            type: "success",
          });
          this.$emit("close-Dia", 1);
          // this.formUpdate = false
        })
        .catch((res) => {
          this.$message({
            message: `${res}`,
            type: "error",
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
      return (partnerList) => {
        return (
          partnerList.cardCode
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    getPartnerList() {
      this.parentLoad = true;
      getPartner(this.listQuery)
        .then((res) => {
          this.partnerList = res.data;
          this.filterPartnerList = this.partnerList;
          this.parentCount = res.count;
          this.parentLoad = false;
        })
        .catch((error) => {
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
      // this.form.address = item.address;

      this.form.salesMan = item.slpName;
    },
    sureVal() {
      this.dialogPartner = false;
      let val = this.checkVal;
      this.form.customerId = val.cardCode;
      this.form.customerName = val.cardName;
      this.form.contacter = val.cntctPrsn;
      this.form.contactTel = val.cellular;
      // this.form.addressDesignator = val.address;
      // this.form.address = val.address;
      this.form.salesMan = val.slpName;
    },
    ChildValue(val) {
      if (val == 1) {
        this.disableBtn = true;
        console.log(11);
      } else {
        this.checkVal = val;
        this.disableBtn = false;
      }
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
    },
  },
  handleCreate() {
    // 弹出添加框
    this.resetTemp();
    this.dialogStatus = "create";
    this.dialogFormVisible = true;
    // this.$nextTick(() => {
    //   this.$refs["form"].clearValidate();
    // });
  },
};
</script>

<style lang="scss" scoped>
.form {
  .lastWord {
    position: sticky;
    top: 0;
    // width: 200px;
  }
  ::v-deep .el-radio {
    margin-left: 0 !important;
  }
  ::v-deep .el-input__inner {
    padding-right: 5px;
    // padding-left:25px;
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
.rowStyle1 {
  ::v-deep .el-form {
    padding: 5px;
    // margin-bottom: 2px;
  }
  ::v-deep .el-form-item__label {
    line-height: 30px;
  }
  ::v-deep .el-form-item__content {
    line-height: 30px;
  }
  ::v-deep .el-form-item {
    margin: 1px 1px;
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
    padding: 2px 7px;
    height: 20px;
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
