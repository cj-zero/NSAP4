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
            :disabled="this.formName !== '新建'"
            :label-width="labelwidth"
            :inline-message="true"
            :show-message="false"
            size="mini"
            :label-position="labelposition"
          >
            <!-- <div
              style="font-size:22px;color:#67C23A;text-align:center;height:40px;line-height:35px;border-bottom:1px solid silver;margin-bottom:10px;"
            >{{formName}}呼叫服务单</div> -->
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="6">
                <el-form-item label="客户代码" prop="customerId">
                  <!-- <el-input size="mini" v-model="form.customerId" ><i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i></el-input> -->
                  <el-autocomplete
                    popper-class="my-autocomplete"
                    v-model="form.customerId"
                    :fetch-suggestions="querySearch"
                    placeholder="请输入内容"
                    class="myAuto"
                    @select="handleSelectCustomer"
                  >
                    <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick('customer')"></i>
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
              <el-col :span="6">
                <el-form-item label="客户名称" prop="customerName">
                  <el-input size="mini" v-model="form.customerName" disabled style="width: 140px;"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="服务ID">
                  <el-input size="mini" v-model="form.u_SAP_ID" disabled placeholder="请选择">
                  </el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <!-- <el-form-item label="服务合同">
                <el-input v-model="form.contractId" disabled></el-input>
                </el-form-item>-->
                <el-form-item label="接单员">
                  <el-input size="mini" v-model="form.recepUserName" disabled></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="6">
                <el-form-item label="终端客户代码">
                  <el-autocomplete
                    popper-class="my-autocomplete"
                    v-model="form.terminalCustomerId"
                    :fetch-suggestions="querySearch"
                    placeholder="请输入内容"
                    class="myAuto"
                    @select="handleSelectTerminal"
                  >
                    <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick('terminalCustomer')"></i>
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
              <el-col :span="6">
                <el-form-item label="终端客户名称">
                  <el-input size="mini" v-model="form.terminalCustomer" disabled style="width: 140px;"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
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
              <el-col :span="6">
                <el-form-item label="最近联系人">
                  <el-input size="mini" v-model="form.newestContacter"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <!-- <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="8">
                <el-form-item label="终端客户">
                  <el-input size="mini" v-model="form.terminalCustomer"></el-input>
                </el-form-item>
              </el-col>
              
            </el-row> -->
            <el-row>
              <el-col :span="6">
                <el-form-item label="呼叫来源" prop="fromId">
                  <el-select
                    size="mini"
                    style="width: 110px;"
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
              <el-col :span="6">
                <el-form-item label="创建时间">
                  <el-date-picker v-if="isCreate"
                    @focus="setThisTime"
                    :clearable="false"
                    size="mini"
                    v-model="form.createTimeNow"
                    type="datetime"
                    format="yyyy-MM-dd HH:mm"
                    placeholder="选择日期时间"
                    @change="onDateChange"
                    prefix-icon
                  ></el-date-picker>
                  <el-date-picker v-else
                    @focus="setThisTime"
                    :clearable="false"
                    size="mini"
                    v-model="form.createTime"
                    type="datetime"
                    format="yyyy-MM-dd HH:mm"
                    placeholder="选择日期时间"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="电话号码">
                  <el-input size="mini" v-model.number="form.contactTel" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="最新电话号码" prop="newestContactTel">
                  <el-input size="mini" v-model="form.newestContactTel"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="10" type="flex" class="row-bg">
              <el-col :span="6">
                <el-form-item label="地址标识">
                  <!-- <el-input v-model="form.addressDesignator"></el-input> -->
                  <el-select
                    size="mini"
                    style="width: 110px;"
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
              <el-col :span="14">
                <el-input
                  size="mini"
                  disabled
                  style="height:30px;line-height:30px;padding:2px 0 0 2px;"
                  v-model="form.address"
                ></el-input>
              </el-col>
              <el-col :span="2">
                <el-form-item label label-width="0">
                  <el-radio style="color:red;" disabled>售后审核:{{form.supervisor}}</el-radio>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="10">
              <el-col :span="6">
                <el-form-item label="现地址">
                  <el-input size="mini" v-model="allArea" readonly style="width: 110px;" >
                    <el-button size="mini" slot="append" icon="el-icon-position" @click="onAreaClick"></el-button>
                  </el-input>
                  <area-selector
                    v-show="areaVisible"
                    class="area-content-wrapper"
                    @change="onAreaChange"
                    @close="onAreaClose"
                  ></area-selector>
                  <!-- <div >我是</div> -->
                  <!-- <p
                    style="overflow-x:hidden;border: 1px solid silver; border-radius:5px;height:30px;margin:0;padding-left:10px;font-size:12px;"
                  >{{allArea}}</p> -->
                </el-form-item>
              </el-col>
              <el-col :span="14" style="height:30px;line-height:30px;padding:2px 0 0 7px;">
                <el-input size="mini" v-model="form.addr">
                  <!-- <el-button size="mini" slot="append" icon="el-icon-position" @click="openMap"></el-button> -->
                </el-input>
              </el-col>
              <el-col :span="2">
                <el-form-item label label-width="0">
                  <el-radio style="color:red;" disabled>销售审核:{{form.salesMan}}</el-radio>
                </el-form-item>
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
              <!-- <el-col :span="1" style="line-height:40px;"></el-col> -->
              <el-col class="upload-text">
                上传图片
              </el-col>
              <el-col :span="22">
                <upLoadImage :setImage="setImage" @get-ImgList="getImgList" :limit="limit"></upLoadImage>
              </el-col>
              <!-- <el-col :span="2" style="line-height:40px;">  暂时取消
                <el-button
                  type="primary"
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
              <el-col class="upload-text">
                已上传图片
              </el-col>
              <el-col :span="22" v-if="form.serviceOrderPictures&&form.serviceOrderPictures.length">
                <div class="demo-image__lazy">
                  <div class="img-list" v-for="url in form.serviceOrderPictures" :key="url.id">
                    <el-image
                      style="width:60px;height:50px;display:inline-block;"
                      :src="`${baseURL}/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`"
                      lazy
                    ></el-image>
                    <div class="operation-wrapper">
                      <i
                        class="el-icon-zoom-in"
                        @click="handlePreviewFile(`${baseURL}/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`)"
                      ></i>
                      <i
                        class="el-icon-download"
                        @click="downloadFile(`${baseURL}/${url.pictureId?url.pictureId:url.id}?X-Token=${tokenValue}`)"
                      ></i>
                    </div>
                  </div>
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
            <!-- <formAdd :SerialNumberList="SerialNumberList"></formAdd> -->
          </el-form>
          <formAdd
            :isCreate="isCreateAdd"
            :ifEdit="ifEdit"
            @change-form="changeForm"
            :serviceOrderId="serviceOrderId"
            :propForm="propForm"
            ref="formAdd"
            :formName="formName"
            :form="form"
          ></formAdd>
        </el-col>
      </el-row>
      <el-dialog
        title="选择地址"
        width="1000px"
        :modal-append-to-body="false"
        :append-to-body="true"
        :destroy-on-close="true"
        :visible.sync="drawerMap"
        direction="ttb"
      >
        <zmap @drag="dragmap"></zmap>
        <!-- <bmap @mapInitail="onMapInitail"></bmap> -->
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
        width="70%"
        @open="openDialog"
        :modal-append-to-body="false"
        :append-to-body="true"
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
          ref="formPartner"
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
      <el-dialog 
        :modal-append-to-body='false'
        :append-to-body="true" 
        title="最近服务单情况" 
        width="450px" 
        @open="openDialog" 
        :visible.sync="dialogCallId"
        :center="true"
      >
        <callId :toCallList="CallList"></callId>
        <span slot="footer" class="dialog-footer">
          <el-button @click="dialogCallId = false">关闭</el-button>
          <!-- <el-button type="primary" @click="dialogCallId = false">确 定</el-button> -->
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
import { getPartner, getSerialNumber } from "@/api/callserve";
// 
import * as callformPartner from "@/api/serve/callformPartner";
import callId from "./callId";
// import BMap from 'BMap'
// import http from "@/api/serve/whiteHttp";
import * as callservesure from "@/api/serve/callservesure";
import Pagination from "@/components/Pagination";
import zmap from "@/components/amap";
// import bmap from '@/components/bmap'
import upLoadImage from "@/components/upLoadFile";
import Model from "@/components/Formcreated/components/Model";
import { timeToFormat } from "@/utils";
import { isCustomerCode } from '@/utils/validate'
import { debounce } from '@/utils/process'
// import { isMobile, isPhone } from "@/utils/validate";
import { download } from "@/utils/file";
import formPartner from "./formPartner";
import formAdd from "./formAdd";
import AreaSelector from '@/components/AreaSelector'
import jsonp from '@/utils/jsonp'
// import { delete } from 'vuedraggable';
export default {
  name: "formTable",
  components: { 
    formPartner, 
    formAdd, 
    zmap, 
    Pagination, 
    upLoadImage, 
    Model, 
    // bmap,
    callId,
    AreaSelector 
  },
  props: [
    "modelValue",
    "refValue",
    "labelposition",
    "labelwidth",
    "isCreate", //是否是可编辑页面
    "formName", //
    "ifEdit", //是否是编辑页面
    "customer", //待确认页面app传入的数据
    "sure", //用于待确认，新建页面确定之后的响应
    "ifFirstLook", //是否是待确认页面
    "serviceOrderId", // 服务单ID
  ],
  //  ##isCreate是否可以编辑  ##look只能看   ##create新增页  ##customer获取服务端对比的信息
  //customer确认订单时传递的信息
  data() {
    // var checkTelF = (rule, value, callback) => {
    //   if (!value) {
    //     return callback();
    //   }
    //   setTimeout(() => {
    //     // let reg = RegExp(/^[\d-]+$/);
    //     if (isMobile(value) || isPhone(value)) {
    //       callback();
    //     } else {
    //       // callback(new Error('请输入正确的格式'));
    //       callback(this.$message.error("请输入正确的电话格式"));
    //     }
    //   }, 500);
    // };
    // let checkCustomerId = (rule, value, callback) => {
    //   if (!value || isCustomerCode(value)) {
    //     return callback()
    //   }
    //   return callback(this.$message.error("请输入正确的代码"))
    // }
    return {
      baseURL: process.env.VUE_APP_BASE_API + "/files/Download",
      tokenValue: this.$store.state.user.token,
      previewUrl: "", //预览图片的定义

      partnerList: [],
      previewVisible: false, //图片预览的dia
      setImage: "50px",
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
      checkVal: {},
      form: {
        customerId: "", //客户代码,
        customerName: "", //客户名称,
        terminalCustomerId: "", // 终端客户代码
        terminalCustomer: "", // 终端客户名称
        contacter: "", //联系人,
        contactTel: "", //联系人电话,
        supervisor: "", //主管名字,
        supervisorId: "", //主管用户Id,
        salesMan: "", //销售名字,
        salesManId: "", //销售用户Id,fv
        newestContacter: "", //最新联系人,
        newestContactTel: "", //最新联系人电话号码,
        contractId: "", //服务合同
        recepUserName: "", //接单员
        addressDesignator: "", //地址标识
        recepUserId: "", //接单人用户ID
        address: "", //详细地址
        // createTime: timeToFormat("yyyy-MM-dd HH-mm-ss"),
        createTimeNow: new Date(),
        createTime: timeToFormat("yyyy-MM-dd HH-mm-ss", this.createTimeNow),
        id: "", //服务单id
        province: "", //省
        city: "", //市
        area: "", //区
        addr: "", //地区
        longitude: "", //number经度
        latitude: "", //	number纬度
        fromId: this.formName === '确认' ? 6 : 1, //integer($int32)呼叫来源 1-电话 2-APP
        pictures: [], //
        serviceWorkOrders: [],
      },
      disableBtn: false,
      newDate: [],
      isCreateAdd: true, //add页面的编辑状态
      allAddress: {}, //选择地图的合集
      propForm: [],
      rules: { // 表单规则
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
        createTimeNow: [
          { required: true, message: "请选择创建时间", trigger: "change" },
        ]
        // newestContactTel: [{ validator: checkTelF, trigger: "blur" }],
      },
      listQuery: {
        page: 1,
        limit: 40,
      },
      needPos: false,
      dialogCallId: false, // 最近十个服务单弹窗
      CallList: [], // 最近十个服务单列表
      selectSerNumberDisabled: true, // 用于选择客户代码后，工单序列号是否可以操作
      limit: 9, // 图片上传限制
      handleSelectType: '', // 用来区分选择的是客户代码还是终端客户代码
      areaVisible: false, // 地址选择器弹窗
      parasePositionURL: 'http://api.map.baidu.com/geocoding/v3/?',
      bMapkey: 'uGyEag9q02RPI81dcfk7h7vT8tUovWfG'
    };
  },
  computed: {
    allArea() {
      return this.form.province + this.form.city + this.form.area;
    },
  },
  watch: {
    // ifEdit: {
    //   handler(val) {
    //     console.log(val);
    //   },
    // },
    customer: {
      handler(val) {
        this.getPartnerInfo(val.customerId)
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
    // "form.addr": {
    //   //现地址详细地址
    //   handler(val) {
    //     if (val && this.formName === '新建') {
    //       if (!this.ifFirstLook) {
    //         this.needPos = true;
    //       } else {
    //         if (this.customer.addr != val) {
    //           this.needPos = true;
    //         } else {
    //           this.needPos = false;
    //         }
    //       }
    //       let addre =
    //         this.form.province +
    //         this.form.city +
    //         this.form.area +
    //         this.form.addr;
    //       if (this.needPos) this.getPosition(addre);
    //     }
    //   },
    // },
    // "form.address": {
    //   //地图标识地址

    //   handler(val, oldVal) {
    //     if (val && this.formName === '新建') {
    //       if (this.ifFirstLook) {
    //         if (oldVal) {
    //           this.needPos = true;
    //         } else {
    //           this.needPos = false;
    //         }
    //       } else {
    //         this.needPos = true;
    //       }
    //       if (this.needPos) this.getPosition(val);
    //     }
    //   },
    //   immediate: true,
    // },
    "form.customerId": {
      handler(val) {
        // if (val && this.isCreate) {
        if (!val) {
          this.selectSerNumberDisabled = true
        }
        // }
        console.log('form.customerId change')
        this.getPartnerInfo(val);
      },
    },
    "form.terminalCustomerId" (val) {
      if (!val) {
        console.log('reset Info')
        this.resetInfo()
        if (isCustomerCode(this.form.customerId)) {
          this.getPartnerInfo(this.form.customerId)
        } else {
          this.$message.error('请填入正确的客户代码')
        }
      }
    },
    refValue: {
      deep: true,
      handler(val) {
        if (val) {
          this.form.serviceWorkOrders = []; // 清空数组
          this.form = Object.assign({}, this.form, val);
          if (val.serviceWorkOrders.length > 0) {
            val.serviceWorkOrders.map((item, index) => {
              this.form.serviceWorkOrders[index].solutionsubject =
                item.solution && item.solution.subject;
              this.form.serviceWorkOrders[index].problemTypeName =
                item.problemType && item.problemType.name;
            });
          }
          // console.log(val, 'refValueChange', this.form)
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
    if (this.customer) {
      this.setForm(this.customer);
    }
    this.isCreateAdd = this.isCreate;
    // if (this.formName === '确认') {
    //   // 确认页面的话，直接调用
    //   console.log('look')
    //   this.getPartnerInfo(this.customer.customerId)
    // }
    console.log(this.isCreateAdd, "isCreatedAdd");
  },
  updated () {
    // if (this.formName === '确认') {
    //   // 确认页面的话，直接调用
    //   console.log('look')
    //   this.getPartnerInfo(this.customer.customerId)
    // }
  },
  destroyed() {
    window.removeEventListener("resize", this.resizeWin);
  },
  methods: {
    onMapInitail (map) {
      console.log(map, 'mapBAIDU')
    },
    onDateChange(val) {
      console.log(typeof val);
      this.form.createTime = timeToFormat("yyyy-MM-dd HH-mm-ss", val);
      console.log("date", this.form.createTime);
    },
    downloadFile(url) {
      download(url);
    },
    setThisTime() {},
    handlePreviewFile(item) {
      //预览图片
      this.previewVisible = true;
      this.previewUrl = item;
    },
    onAreaChange (val) { // 地址发生变化
      console.log(val)
      let { province, city, district } = val
      this.form.province = province
      this.form.city = city
      this.form.district = district
      this._getPosition()
    },
    _getPosition () {
      // address=北京市海淀区上地十街10号&output=json&ak=您的ak&callback=showLocation
      let params = `address=${encodeURIComponent(this.allArea.replace('海外', ''))}&output=json&ak=${this.bMapkey}`
      jsonp(`${this.parasePositionURL}${params}`).then(res => {
        let { status, result } = res
        console.log(res, status, result, 'status', status == 0)
        if (status == 0) {
          let { lat, lng } = result.location
          console.log(res, lat, lng, 'lng')
        } else {
          console.log('错误')
          this.$message.error('地址解析错误')
        }
      }).catch((err) => {
        console.log(err, 'err')
        this.$message.error('地址解析错误')
      })
    },
    onAreaClose () {
      this.areaVisible = false
    },
    onAreaClick () {
      this.areaVisible = true
    },
    chooseAddre() {
      //地图选择赋值地址
      let getArr = this.allAddress.regeocode.addressComponent;
      // let str = getArr.province +getArr.city+getArr.district
      this.form.city = Array.isArray(getArr.city) ? "" : getArr.city;
      this.form.province = getArr.province;
      this.form.area = Array.isArray(getArr.district) ? "" : getArr.district;
      this.form.addr = this.allAddress.address
        .replace(getArr.province, "")
        .replace(getArr.city, "")
        .replace(getArr.district, "");
      this.form.longitude = this.allAddress.position.lng;
      this.form.latitude = this.allAddress.position.lat;
      this.drawerMap = false;
    },
    // getPosition(val) {
    //   //从接口获取地址
    //   let that = this;
    //   let url = `https://restapi.amap.com/v3/geocode/geo?key=c97ee5ef9156461c04b552da5b78039d&address=${encodeURIComponent(
    //     val
    //   )}`;
    //   http.get(url, function (err, result) {
    //     if (result.geocodes.length) {
    //       let res = result.geocodes[0];
    //       that.form.province = res.province;
    //       that.form.city = res.city;
    //       that.form.area = res.district;
    //       that.form.addr = val
    //         .replace(res.province, "")
    //         .replace(res.city, "")
    //         .replace(res.district, "");
    //       that.form.latitude = res.location.split(",")[1]; // 维度
    //       that.form.longitude = res.location.split(",")[0]; // 精度
    //     } else {
    //       if (that.isCreate || that.ifEdit) {
    //         that.$message({
    //           message: "未识别到地址，请手动选择",
    //           type: "error",
    //         });
    //       }
    //       that.form.province = "";
    //       that.form.city = "";
    //       that.form.area = "";
    //       that.form.addr = "";
    //       that.form.latitude = "";
    //       that.form.longitude = "";
    //     }
    //     // 这里对结果进行处理
    //   });
    // },
    getImgList(val) {
      //获取图片列表

      this.form.pictures = val;
    },
    dragmap(res) {
      this.allAddress = res;
    }, 
    resetInfo () { // 清空终端客户相关的数据
      this.addressList = [];
      this.cntctPrsnList = [];
      this.form.contactTel = ''
      this.form.terminalCustomer = ''
      this.form.supervisor = '';
      this.form.contacter = ''
      this.form.contactTel = ''
      this.form.addressDesignator = '';
      this.form.address = '';
      console.log(this.addressList, this.cntctPrsnList, this.form)
    },
    async setForm(val) {
      val = JSON.parse(JSON.stringify(val));
      console.log(val, 'json.stringify')
      if (val) {
        val.serviceOrderPictures = await this.getServeImg(val.id);
        this.$emit("imgChange", val.serviceOrderPictures); // 告诉父组件
      }
      if (val.province && val.city && val.area && val.addr) {
        this.needPos = false;
      } else {
        this.needPos = true;
      }
      let { newestContactTel, newestContacter, problemTypeName, problemTypeId } = val;
      // let newVal = Object.create(null);
      // for (let key in val) {
      //   if (key == "contactTel" || key !== "contacter") {
      //     newVal[key] = val[key];
      //   }
      // }
      Object.assign(this.form, val);
      this.form.newestContacter = newestContacter; //最新联系人,
      this.form.newestContactTel = newestContactTel;
      // let that =  this
      //  let addre = val.province + val.city + val.area +val.addr
      // setTimeout(function(){
      //  that.getPosition(addre)
      // },800)
      this.form.recepUserName = this.$store.state.user.name;
      let listQuery = {
        page: 1,
        limit: 10
      }
      let CardCode = val.customerId
      let manufSNList = val.serviceOrderSNs.map(item => {
        return item.manufSN
      })
      let promiseList = [], otherIndex = 0, hasOther = false
      for (let i = 0; i < manufSNList.length; i++) {
        let manufSN = manufSNList[i]
        if (manufSN !== '其他设备') {
          promiseList.push(getSerialNumber({
            ...listQuery,
            CardCode,
            ManufSN: manufSN
          }))
        } else {
          hasOther = true
          otherIndex = i
        }
      }
      Promise.all(promiseList).then(res => {
        let targetList = []
        for (let i = 0; i < res.length; i++) {
          targetList.push({
            manufacturerSerialNumber: res[i].data[0].manufSN,
            editTrue: i === 0,
            internalSerialNumber: res[i].data[0].internalSN,
            materialCode: res[i].data[0].itemCode,
            materialDescription: res[i].data[0].itemName,
            feeType: 1,
            orderTakeType: 1,
            fromTheme:  "",
            fromType:  1,
            problemTypeName,
            problemTypeId,
            priority:  1,
            remark:  "",
            solutionId:  "",
            status:  1,
            solutionsubject:  "",
          })
        }
        if (hasOther) {
          targetList.splice(otherIndex, 0, {
            manufacturerSerialNumber: "其他设备",
            editTrue: false,
            internalSerialNumber: "",
            materialCode: "其他设备",
            materialDescription: "",
            feeType: 1,
            orderTakeType: 1,
            fromTheme:  "",
            fromType:  1,
            problemTypeName,
            problemTypeId,
            priority:  1,
            remark:  "",
            solutionId:  "",
            status:  1,
            solutionsubject:  "",
          })
        }
        this.propForm = targetList
        console.log(targetList, 'taragetList')
      }).catch((err) => {
        console.log(err, 'err')
        this.$message.error('查询序列号失败')
      })
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
        this.$emit("close-Dia", "N");
        return;
      }
      if (!this.form.longitude) {
        this.$message({
          message: `请手动选择地址`,
          type: "error",
        });
        this.$emit("close-Dia", "N");
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
        if (!isCustomerCode(this.form.customerId)) {
          this.$message('请填入正确的客户代码格式')
          return this.$emit('close-Dia', 'closeLoading')
        }
        if (this.form.terminalCustomerId && !isCustomerCode(this.form.terminalCustomerId)) {
          this.$message.error('请输入正确的终端代码格式')
          return this.$emit('close-Dia', 'closeLoading')
        }
        console.log(chec, this.isValid, "isValid", this.$router.path);
        if (chec && this.isValid) {
          if (this.$route.path === "/serve/callserve") {
            if (this.isCreate) {
              // 新建服务单
              if (Array.isArray(this.form.area)) {
                this.form.area = "";
              }
              if (Array.isArray(this.form.city)) {
                this.form.city = ""
              }
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
                  this.$emit("close-Dia", "N");
                });
            } else {
              let formInitailList = this.$refs.formAdd.formInitailList;
              let targetList = this.form.serviceWorkOrders.filter((item) => {
                return !formInitailList.some((serviceItem) => {
                  return (
                    serviceItem.manufacturerSerialNumber ===
                    item.manufacturerSerialNumber
                  );
                });
              });
              let promiseList = [];
              for (let i = 0; i < targetList.length; i++) {
                let item = targetList[i];
                let promise = callservesure.addWorkOrder({
                  serviceOrderId: this.serviceOrderId,
                  ...item,
                });
                promiseList.push(promise);
              }
              Promise.all([...promiseList])
                .then(() => {
                  this.$message({
                    message: "新增工单成功",
                    type: "success",
                  });
                  this.$emit("close-Dia", "y");
                })
                .catch(() => {
                  this.$message({
                    message: "新增工单失败",
                    type: "error",
                  });
                  this.$emit("close-Dia", "N");
                });
            }
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
                this.$emit("close-Dia", "N");
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
          this.$emit("close-Dia", "N");
        }
      }
    },
    getPartnerInfo(num) {
      callservesure
        .forServe(num)
        .then((res) => {
          console.log(res, "售后主管", this.handleSelectType, isCustomerCode(this.form.terminalCustomerId));
          if (this.handleSelectType === 'customer') { // 如果点击客户代码的时候
            if (this.form.terminalCustomerId) { // 如果终端客户存在，则不进行值的覆盖
              return 
            }
          }
          this.addressList = res.result.addressList;
          this.cntctPrsnList = res.result.cntctPrsnList;
          console.log(this.cntctPrsnList, 'this.cntctPrsnList')
          this.form.supervisor = res.result.techName;
          if (this.formName !== '编辑' && this.formName !== '查看') {
            if (this.cntctPrsnList && this.cntctPrsnList.length) {
            let firstValue = res.result.cntctPrsnList[0]
              let { tel1, tel2, cellolar, name } = firstValue
              console.log(name, 'name')
              this.form.contacter = name
              this.form.contactTel = tel1 || tel2 || cellolar
            }
            // this.form.contacter = item.cntctPrsn;
            // this.form.contactTel
            if (this.addressList.length) {
              let { address, building } = this.addressList[0];
              this.form.addressDesignator = address;
              this.form.address = building;
              // this.form.address = building;
            }
          }
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
      this.form.contactTel = res[0].tel1 || res[0].tel2 || res[0].cellolar;
    },
    changeForm(val) {
      this.form.serviceWorkOrders = val;
      console.log(this.form.serviceWorkOrders, "serviceWorkOrders");
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
          this.$emit("close-Dia", 'y');
          // this.formUpdate = false
        })
        .catch((res) => {
          this.$emit("close-Dia", "N");
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
      // this.drawerMap = true;
    },
    openDialog() {
      //打开前赋值
      this.filterPartnerList = this.partnerList;
    },
    searchList: debounce(function() {
      console.log(this, 'this')
      this.listQuery.CardCodeOrCardName = this.inputSearch;
      this.form.customerId = this.inputSearch;
      console.log('searchList')
      this.getPartnerList();
      // if (!res) {
      //   this.filterPartnerList = this.partnerList;
      // } else {
      //   let list = this.partnerList.filter(item => {
      //     return item.cardCode.toLowerCase().indexOf(res.toLowerCase()) === 0;
      //   });
      //   this.filterPartnerList = list;
      // }
    }, 400),
    searSerial: debounce(function() {
      // SerialList
      this.listQuery.ManufSN = this.inputSerial;
      this.getPartnerList();
    }, 400),
    async querySearch(queryString, cb) {
      this.listQuery.CardCodeOrCardName = queryString;
      this.inputSearch = queryString;

      await this.getPartnerList();
      console.log(this.partnerList, 'partnerList')
      // var results = queryString
      //   ? partnerList.filter(this.createFilter(queryString))
      //   : partnerList;
      // 调用 callback 返回建议列表的数据
      cb(this.partnerList);
    },
    // createFilter(queryString) {
    //   return (partnerList) => {
    //     return (
    //       partnerList.cardCode
    //         .toLowerCase()
    //         .indexOf(queryString.toLowerCase()) === 0
    //     );
    //   };
    // },
    getPartnerList() {
      this.parentLoad = true;
      return getPartner(this.listQuery)
        .then((res) => {
          this.partnerList = res.data;
          // console.log(res.data, '返回')
          this.filterPartnerList = this.partnerList;
          this.parentCount = res.count;
          this.parentLoad = false;
        })
        .catch((error) => {
          console.log(error);
          this.parentLoad = false
        });
    },
    handleSelectCustomer(item) {
      console.log(item)
      this.handleSelect(item, 'customer')
    },
    handleSelectTerminal (item) {
      this.handleSelect(item, 'termianlCustomer')
    },
    handleSelect (item, type) {
      // if (!this.form.customerId && type === 'terminalCustomer') {
      //   return this.$message.error('请先选择客户代码')
      // }
      this.inputSearch = item.customerId;
      this.handleSelectType = type // 选择的类型
      if (type === 'customer') { // 客户代码
        this.form.customerId = item.cardCode;
        this.form.customerName = item.cardName;
        this.form.salesMan = item.slpName;
        if (!this.form.terminalCustomerId) {
          this.form.contactTel = item.cellular;
        }
      } else { // 终端客户代码
        this.form.terminalCustomerId = item.cardCode
        this.form.terminalCustomer = item.cardName
      }
      this.handleCurrentChange(item)
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
      console.log('sureVal', 'val')
      this.handleCurrentChange(val)
      // this.$refs.formPartner.handleCurrentChange(val)
    },
    handleCurrentChange (val) {
      if (val.frozenFor == "Y") {
        this.$message({
          message: `${val.cardName}账户被冻结，无法操作`,
          type: "error"
        });
      } else {
        this.selectSerNumberDisabled = false
        this.getPartnerInfo(val.cardCode)
        callformPartner.getTableList({ code: val.cardCode }).then(res => {
          this.CallList = res.result;
          this.dialogCallId = true
          // this.newestNotCloseOrder=res.reault.newestNotCloseOrder
          // this.newestOrder=res.reault.newestNotCloseOrder
        });
      }
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
    handleIconClick(type) {
      if (!this.isCreate) {
        return
      }
      if (this.formName !== '新建') {
        return
      }
      this.handleSelectType = type // 设置选择的类型

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
  ::v-deep .el-date-editor {
    .el-input__inner {
      width: 140px;
    }
  }
  .upload-text {
    width: 95px;
    line-height: 40px; 
    text-align: right;
    font-size: 12px;
    color: #606266;
  }
  .area-content-wrapper {
    position: absolute;
    top: 28px;
    z-index: 2;
    // background-color: red;
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
/* 图片样式 */
.demo-image__lazy {
  .img-list {
    position: relative;
    display: inline-block;
    width: 60px;
    height: 50px;
    margin: 0 10px;
    .operation-wrapper {
      position: absolute;
      display: flex;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      opacity: 0;
      justify-content: space-around;
      align-items: center;
      transition: opacity 0.5s;
      background-color: rgba(0, 0, 0, 5);
      .el-icon-download,
      .el-icon-zoom-in {
        color: white;
      }
    }
    &:hover {
      .operation-wrapper {
        opacity: 1;
      }
    }
  }
}
</style> 
