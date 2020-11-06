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
            :class="{ 'form-disabled': this.formName !== '新建' }"
            :disabled="this.formName !== '新建'"
            :label-width="labelwidth"
            :inline-message="true"
            :show-message="false"
            size="mini"
            :label-position="labelposition"
          >
            <el-row class="info-wrapper" type="flex">
              <div>服务ID: <span>{{ form.u_SAP_ID}}</span></div>
              <div>接单员: <span>{{ form.recepUserName }}</span></div>
              <div>创建时间: <span>{{ form.createTime }}</span></div>
              <div class="approve">售后审核: <span>{{ form.supervisor }}</span></div>
              <div class="approve">销售审核: <span>{{  form.salesMan }}</span></div>
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="6">
                <el-form-item label="客户代码" prop="customerId">
                  <!-- <el-input size="mini" v-model="form.customerId" ><i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i></el-input> -->
                  <el-autocomplete
                    popper-class="my-autocomplete"
                    v-model.trim="form.customerId"
                    :fetch-suggestions="querySearch"
                    placeholder="请输入内容"
                    class="myAuto"
                    @select="handleSelectCustomer"
                    @input.native="onCustomerIdChange"
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
              <el-col :span="12">
                <el-form-item label="客户名称" prop="customerName">
                  <el-input size="mini" v-model="form.customerName" disabled style="width: 100%"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="联系人" prop="newestContacter">
                  <el-autocomplete
                    popper-class="my-autocomplete"
                    v-model.trim="form.newestContacter"
                    :fetch-suggestions="queryCntctPrsnList"
                    placeholder="请输入内容"
                    class="myAuto"
                    @select="handleSelectCntct"
                    clearable
                  >
                    <template slot-scope="{ item }">
                      <p class="name" style="height: 20px;line-height: 20px;text-align: center;">{{ item.name }}</p>                      
                    </template>
                  </el-autocomplete>
                  <!-- <el-select
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
                  </el-select> -->
                </el-form-item>
              </el-col>
              <!-- <el-col :span="6">
                <el-form-item label="服务ID">
                  <el-input size="mini" v-model="form.u_SAP_ID" disabled placeholder="请选择">
                  </el-input>
                </el-form-item>
              </el-col> -->
              <!-- <el-col :span="6">
                <el-form-item label="接单员">
                  <el-input size="mini" v-model="form.recepUserName" disabled></el-input>
                </el-form-item>
              </el-col> -->
            </el-row>
            <el-row type="flex" class="row-bg" justify="space-around">
              <el-col :span="6">
                <el-form-item label="终端代码">
                  <el-input
                    v-model="form.terminalCustomerId"
                    readonly
                    @focus="handleIconClick('terminalCustomer')"
                  >
                    <!-- <i class="el-icon-circle-close" slot="suffix" @click="clearTerminalCode"></i> -->
                    <template slot-scope="{ item }">
                      <div class="name">
                        <p style="height:20px;margin:2px;">{{ item.cardCode }}</p>
                        <p
                          style="font-size:12px;height:20px;margin:2px;color:silver;"
                        >{{ item.cardName }}</p>
                      </div>
                      <!-- <span class="addr">{{ item.cardName }}</span> -->
                    </template>
                  </el-input>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="终端客户">
                  <el-input size="mini" v-model="form.terminalCustomer" disabled style="width: 100%"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="联系方式" prop="newestContactTel">
                  <el-input size="mini" v-model.trim="form.newestContactTel" :disabled="formName !== '新建'"></el-input>
                </el-form-item>
              </el-col>
              <!-- <el-col :span="6">
                <el-form-item label="最近联系人">
                  <el-input size="mini" v-model="form.newestContacter"></el-input>
                </el-form-item>
              </el-col> -->
            </el-row>
            <!-- <el-row>
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
                <el-form-item label="最新电话号码" prop="newestContactTel">
                  <el-input size="mini" v-model="form.newestContactTel"></el-input>
                </el-form-item>
              </el-col>
            </el-row> -->
            <!-- <el-row :gutter="10" type="flex" class="row-bg">
              <el-col :span="6">
                <el-form-item label="地址标识">
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
              
            </el-row> -->
            <el-row type="flex" align="top">
              <el-col :span="6" style="padding: 0;">
                <el-form-item label="呼叫来源" prop="fromId">
                  <el-select
                    size="mini"
                    style="width: 100%;"
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
              <el-col :span="8" class="append-address">
                <el-form-item label="现地址">
                  <el-input size="mini" v-model="allArea" readonly @focus="onAreaClick">
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
              <el-col :span="10" style="height:30px;line-height:30px;padding:0 0 0 7px;margin-top: -1px;">
                <el-input size="mini" v-model="form.addr" maxlength="50">
                  <!-- <el-button size="mini" slot="append" icon="el-icon-position" @click="openMap"></el-button> -->
                </el-input>
              </el-col>
              <!-- <el-col :span="3" class="name-class">
                <el-form-item label label-width="0">
                  <el-radio style="color:red;" disabled>售后:{{form.supervisor}}</el-radio>
                </el-form-item>
              </el-col>
              <el-col :span="3" class="name-class">
                <el-form-item label label-width="0">
                  <el-radio style="color:red;" disabled>销售:{{form.salesMan}}</el-radio>
                </el-form-item>
              </el-col> -->
            </el-row> 
            <bmap @mapInitail="onMapInitail" v-if="formName === '新建'"></bmap> 
          </el-form>
          <!-- //上传图片组件暂时放在这里 -->
          <div class="file-wrapper">
            <el-row
              v-if="customerList && customerList.length"
              :gutter="10"
              type="flex"
              style="margin:5px 0 10px 0 ;"
              class="row-bg"
            >
              <el-col class="upload-text">
                客户上传
              </el-col>
              <el-col :span="22" >
                <img-list :imgList="customerList"></img-list>
              </el-col>
            </el-row>
            <el-row
              v-if="formName === '新建' || formName === '确认'"
              :gutter="10"
              type="flex"
              style="margin:5px 0 10px 0 ;"
              class="row-bg"
            >
              <!-- <el-col :span="1" style="line-height:40px;"></el-col> -->
              <el-col class="upload-text">
                上传图片
              </el-col>
              <el-col :span="22">
                <upLoadImage :setImage="setImage" @get-ImgList="getImgList" :limit="limit" ref="uploadImg"></upLoadImage>
              </el-col>
            </el-row>
            <el-row
              v-if="serviceList && serviceList.length"
              :gutter="10"
              type="flex"
              style="margin:5px 0 10px 0 ;"
              class="row-bg"
            >
              <el-col class="upload-text">
                客服上传
              </el-col>
              <el-col :span="22" >
                <img-list :imgList="serviceList"></img-list>
              </el-col>
            </el-row>
            <el-row
              :gutter="10"
              type="flex"
              style="margin:5px 0 10px 0 ;"
              class="row-bg"
              v-if="formName === '新建' || formName === '确认'"
            >
              <el-col class="upload-text">
                附件
              </el-col>
              <el-col :span="22">
                <upLoadImage  @get-ImgList="getFileList" :limit="limit" uploadType="file" ref="uploadFile"></upLoadImage>
              </el-col>
            </el-row>
            <el-row
              v-if="attachmentList && attachmentList.length"
              :gutter="10"
              type="flex"
              style="margin:0 0 10px 0 ;"
              class="row-bg"
            >
              <el-col class="upload-text">
                附件
              </el-col>
              <el-col :span="22">
                <img-list :imgList="attachmentList" listType="file"></img-list>
              </el-col>
            </el-row>
            <el-row
              :gutter="10"
              type="flex"
              style="margin:5px 0 10px 0 ;"
              class="row-bg"
              justify="space-around"
            >
              <el-col :span="20"></el-col>
            </el-row>
          </div>
          <formAdd
            :isCreate="isCreateAdd"
            :ifEdit="ifEdit"
            @change-form="changeForm"
            :serviceOrderId="serviceOrderId"
            :propForm="propForm"
            ref="formAdd"
            :formName="formName"
            :form="form"
            :inputSerial="inputSerial"
            :businessType="businessType"
          ></formAdd>
        </el-col>
      </el-row>
      <el-dialog
        v-el-drag-dialog
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
        v-el-drag-dialog
        title="选择业务伙伴"
        class="addClass1"
        width="70%"
        @open="openDialog"
        :modal-append-to-body="false"
        :close-on-click-modal="false"
        :append-to-body="true"
        :destroy-on-close="true"
        :modal="false"
        :visible.sync="dialogPartner"
      >
        <el-form :inline="true" class="demo-form-inline" size="mini">
          <el-form-item label="客户">
            <el-input @input="searchList" v-model.trim="inputSearch" placeholder="客户"></el-input>
          </el-form-item>
          <el-form-item label="制造商序列号:">
            <el-input @input="searchList" v-model.trim="inputSerial" placeholder="制造商序列号"></el-input>
          </el-form-item>
          <el-form-item label="售后主管">
            <el-input @input="searchList" v-model.trim="inputTech" placeholder="售后主管" style="width: 100px;"></el-input>
          </el-form-item>
          <el-form-item label="销售员:">
            <el-input @input="searchList" v-model.trim="inputSlpName" placeholder="销售员" style="width: 100px;"></el-input>
          </el-form-item>
          <el-form-item label="收货地址:">
            <el-input @input="searchList" v-model.trim="inputAddress" placeholder="收货地址"></el-input>
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
          :page.sync="listQuerySearch.page"
          :limit.sync="listQuerySearch.limit"
          @pagination="handleChange"
        />
        <span slot="footer" class="dialog-footer">
          <el-button @click="dialogPartner = false">取 消</el-button>
          <el-button type="primary" @click="sureVal(checkVal, handleSelectType)" :disabled="disableBtn">确 定</el-button>
        </span>
      </el-dialog>
      <el-dialog 
        v-el-drag-dialog
        :modal-append-to-body='false'
        :append-to-body="true" 
        :modal="false"
        title="最近服务单情况" 
        width="450px" 
        @open="openDialog" 
        :visible.sync="dialogCallId"
        :center="true"
      >
        <callId :toCallList="CallList" :openTree="openTree"></callId>
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
import http from "@/api/serve/whiteHttp";
import elDragDialog from '@/directive/el-dragDialog'
import * as callservesure from "@/api/serve/callservesure";
import Pagination from "@/components/Pagination";
import zmap from "@/components/amap";
import bmap from '@/components/bmap'
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
import ImgList from '@/components/imgList'
// import jsonp from '@/utils/jsonp'
// import { delete } from 'vuedraggable';
const districtReg = /.+?区/  // 用来从百度地图获取到的地址取出 区地址
export default {
  name: "formTable",
  // inject: ['instance'],
  components: { 
    formPartner, 
    formAdd, 
    zmap, 
    Pagination, 
    upLoadImage, 
    Model, 
    bmap,
    callId,
    AreaSelector,
    ImgList
  },
  directives: {
    elDragDialog
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
    'openTree'
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
      cancelBusinessRequestFn: null, // 用于取消获取业务伙伴的请求
      partnerList: [],
      previewVisible: false, //图片预览的dia
      setImage: "50px",
      drawerMap: false, //地图控件
      filterPartnerList: [],
      inputSearch: "",
      inputSerial: "",
      inputTech: "",
      inputAddress: "",
      inputSlpName: "",
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
        createTime: timeToFormat('yyyy-MM-dd HH:mm'),
        // createTime: timeToFormat("yyyy-MM-dd HH-mm-ss", this.createTimeNow),
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
        files: []
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
        newestContacter: [
          { required: true, message: "请选择联系人", trigger: "change" },
        ],
        newestContactTel: [
          { required: true, message: "请选择联系人", trigger: "change" },
        ],
        createTime: [
          { required: true, message: "请选择创建时间", trigger: "change" },
        ]
        // newestContactTel: [{ validator: checkTelF, trigger: "blur" }],
      },
      listQuery: {
        page: 1,
        limit: 40,
      },
      listQuerySearch: {
        page: 1,
        limit: 40
      },
      needPos: false,
      dialogCallId: false, // 最近十个服务单弹窗
      CallList: [], // 最近十个服务单列表
      selectSerNumberDisabled: true, // 用于选择客户代码后，工单序列号是否可以操作
      limit: 9, // 图片上传限制
      handleSelectType: '', // 用来区分选择的是客户代码还是终端客户代码
      areaVisible: false, // 地址选择器弹窗
      parasePositionURL: 'http://api.map.baidu.com/geocoding/v3/?', // 
      bMapkey: 'uGyEag9q02RPI81dcfk7h7vT8tUovWfG', // 百度key
      positionTransformURL: 'https://restapi.amap.com/v3/assistant/coordinate/convert?', // 高德坐标转换地址
      // gdKey: 'cfd8da5cf010c5f7441834ff5e764f5b' // 高德key
      gdKey: '6cacd440b344fa2d3ef098f0fe1ee33b',
      upLoadFileList: [], // 附件列表
      upLoadImgList: [], // 图片列表
      currentItem: {}, // 当前选中的数据
      selectType: '', // 改变客户/终端客户代码的方式
      businessType: '' // input： 通过点击客户代码获取 search: 通过搜索获取 业务伙伴
    };
  },
  computed: {
    allArea() {
      return this.form.province + this.form.city + this.form.area;
    },
    customerList () {
      if (this.form.files && this.form.files.length) {
        return this.form.files.filter(item => {
          return Number(item.pictureType) === 1
        })
      }
      return []
    },
    serviceList () {
      if (this.form.files && this.form.files.length) {
        return this.form.files.filter(item => {
          return Number(item.pictureType) === 2
        })
      }
      return []
    },
    attachmentList () {
      if (this.form.files && this.form.files.length) {
        return this.form.files.filter(item => {
          return Number(item.pictureType) === 3
        })
      }
      return []
    }
  },
  watch: {
    customer: {
      handler(val) {
        this.getPartnerInfo(String(val.customerId).toUpperCase())
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
    "form.address": {
      //地图标识地址
      handler(val, oldVal) {
        if (val && this.formName === '新建') {
          if (this.ifFirstLook) {
            if (oldVal) {
              this.needPos = true;
            } else {
              this.needPos = false;
            }
          } else {
            this.needPos = true;
          }
          if (this.needPos) this._getPosition(val, true);
        }
      },
      immediate: true,
    },
    "form.customerId": {
      handler(val) {
        if (!val) {
          this.selectSerNumberDisabled = true
        }
        this.handleSelectType = 'customer'
        if (this.formName === '新建') {
          if (!this.form.terminalCustomerId) {
            isCustomerCode(val) ?
            this.form.terminalCustomerId = val :
            this.form.terminalCustomerId = ''
          }
        }
        if (isCustomerCode(val)) {
          if (this.formName !== '编辑' && this.formName !== '查看') {         
            let value = this.selectType === 'click'
              ? this.currentItem
              : { cardCode: val }
            this.handleCurrentChange(value)
          }
        } else {
          this.resetInfo()
          this.resetPositionInfo()
        }
      }
    },
    "form.terminalCustomerId" (val) {
      if (!val) {
        this.resetInfo()
        this.resetPositionInfo()
        if (isCustomerCode(this.form.customerId)) {
          this.handleSelectType = 'customer'
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
          this.form.createTime = this.form.createTime.slice(0, -3)
          if (val.serviceWorkOrders.length > 0) {
            val.serviceWorkOrders.map((item, index) => {
              if (item.orderTakeType !== undefined) {
                this.form.serviceWorkOrders[index].orderTakeType = this.processTakeType(item.orderTakeType)
              }
              this.form.serviceWorkOrders[index].solutionsubject =
                item.solution && item.solution.subject;
              this.form.serviceWorkOrders[index].solutionId =
                item.solution && item.solution.id;
              this.form.serviceWorkOrders[index].problemTypeName =
                item.problemType && item.problemType.name;
            });
          }
          this.propForm = this.form.serviceWorkOrders
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
      form: this.form
    };
  },
  mounted() {
    this.getPartnerList(this.listQuery, 'first');
    if (this.customer) {
      this.setForm(this.customer);
    }
    this.isCreateAdd = this.isCreate;
  },
  updated () { },
  destroyed() {
    window.removeEventListener("resize", this.resizeWin);
  },
  methods: {
    clearFiles () {
      this.$refs.uploadImg.clearFiles()
      this.$refs.uploadFile.clearFiles()
      this.form.files = []
      this.upLoadFileList = []
      this.upLoadImgList = []
    },
    onCustomerIdChange (e) {
      let val = e.target.value
      if (!isCustomerCode(val)) {
        this.form.terminalCustomerId = ''
      }
      this.selectType = 'type' // 键盘键入
    },
    // clearTerminalCode () {f
    //   this.form.terminalCustomerId = ''
    // },
    processTakeType (takeType) {
      if (!takeType || !Number(takeType)) {
        return takeType
      }
      return (takeType == 3 || takeType === 1) ? 1 : 2
    },
    onMapInitail (val) {
      this.map = val.map
      this.BMap = val.BMap
    },
    onDateChange(val) {
      this.form.createTime = timeToFormat("yyyy-MM-dd HH-mm-ss", val);
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
      let { province, city, district } = val
      this.form.province = province
      this.form.city = city
      this.form.area = district
      this._getPosition(`${province}${city}${district}`.replace('海外', ''))
    },
    _getPosition (address, auto) {
      let local = new this.BMap.LocalSearch(this.map, { //智能搜索
        onSearchComplete: onSearchComplete.bind(this)
      })
      address = address.replace(/^中国/i, '') // 如果以中国开头会直接搜索北京市
      console.log(address, 'address')
      local.search(address)
      function onSearchComplete () {
        if (!local.getResults().getPoi(0)) {
          this.resetPositionInfo()
          return this.$message.error('无法获取地址，请手动进行选择')
        }
        console.log(local.getResults().getPoi(0), 'position')
        let { point, address, city, province } = local.getResults().getPoi(0) //获取第一个智能搜索的结果
        if (auto) { // 如果是通过客户代码或者终端代码进行选择的
          this.form.province = province
          if (province === city) { // 如果省和市名字一样则直接取省
            this.form.city = ''
          } else {
            this.form.city = city
          }
          let district = address
            .replace(province, '')
            .replace(city, '')
            .match(districtReg)
          if (district) {
            this.form.area = district[0]
          } else {
            this.form.area = ''
          }
          this.form.addr = address
            .replace(province, '')
            .replace(city, '')
            .replace(district, '')
        }
        let { lat, lng } = point
        lat = lat.toFixed(6)
        lng = lng.toFixed(6)
        this._transformPosition(lat, lng)
      }
    },
    _transformPosition (lng, lat) {
      let queryParams = `locations=${lng},${lat}&coordsys=baidu&output=json&key=${this.gdKey}`
      http.get(`${this.positionTransformURL}${queryParams}`, (err, res) => {
        if (err) {
          this.resetPositionInfo()
          return this.$message.error('坐标转换失败')
        }
        if (res.status != 1) {
          this.resetPositionInfo()
          return this.$message.error('坐标转换失败')
        }
        let [lat, lng] = res.locations.split(',')
        this.form.longitude = lng
        this.form.latitude = lat
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
      this.upLoadImgList = val
    },
    getFileList (val) {
      this.upLoadFileList = val
    },
    dragmap(res) {
      this.allAddress = res;
    }, 
    resetInfo () { // 清空终端客户相关的数据
      if (this.handleSelectType === 'customer') {
        this.form.customerName = ''
        this.form.salesMan = ''
      }
      this.addressList = [];
      this.cntctPrsnList = [];
      this.form.terminalCustomer = ''
      this.form.supervisor = '';
      this.form.contacter = ''
      this.form.contactTel = ''
      this.form.newestContacter = ''
      this.form.newestContactTel = ''
      this.form.addressDesignator = '';
      this.form.address = '';
      
    },
    resetPositionInfo () { // 重置地址信息包括经纬度
      this.form.province = ''
      this.form.city = ''
      this.form.area = ''
      this.form.addr = ''
      this.form.longitude = ''
      this.form.latitude = ''
    },
    async setForm(val) {
      val = JSON.parse(JSON.stringify(val));
      if (val) {
        val.files = await this.getServeImg(val.id);
        this.form.files = val.files
        this.$emit("imgChange", val.files); // 告诉父组件
      }
      if (val.province && val.city && val.area && val.addr) {
        this.needPos = false;
      } else {
        this.needPos = true;
      }
      let { newestContactTel, newestContacter, problemTypeName, problemTypeId } = val;
      Object.assign(this.form, val);
      console.log(newestContacter, newestContactTel)
      this.form.newestContacter = newestContacter; //最新联系人,
      this.form.newestContactTel = newestContactTel;
      this.form.createTime = this.form.createTime.slice(0, -3)

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
            fromTheme:  "",
            fromType:  "",
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
          if (targetList.length && otherIndex === 0) {
            targetList[0].editTrue = false
          }
          targetList.splice(otherIndex, 0, {
            manufacturerSerialNumber: "其他设备",
            editTrue: otherIndex === 0,
            internalSerialNumber: "",
            materialCode: "其他设备",
            materialDescription: "",
            feeType: 1,
            fromTheme:  "",
            fromType:  "",
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
          (item) => {
            return (item.fromTheme !== "" &&
            item.fromType !== "" &&
            item.problemTypeId !== "" &&
            item.manufacturerSerialNumber !== "" &&
            (item.fromType === 2 ? item.solutionId !== "" : true))
          }
        );
        try {
          this.isValid = await this.$refs.form.validate()
        } catch (err) {
          this.isValid = err
        }
        if (!isCustomerCode(this.form.customerId)) {
          this.$message('请填入正确的客户代码格式')
          return this.$emit('close-Dia', 'closeLoading')
        }
        if (this.form.terminalCustomerId && !isCustomerCode(this.form.terminalCustomerId)) {
          this.$message.error('请输入正确的终端代码格式')
          return this.$emit('close-Dia', 'closeLoading')
        }
        console.log(chec, this.isValid, 'chec')
        if (chec && this.isValid) {
          if (this.$route.path === "/serve/callserve") {
            if (this.formName === '新建') {
              // 新建服务单
              if (Array.isArray(this.form.area)) {
                this.form.area = "";
              }
              if (Array.isArray(this.form.city)) {
                this.form.city = ""
              }
              this.form.pictures = [...this.upLoadImgList, ...this.upLoadFileList]
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
            this.form.pictures = [...this.upLoadImgList, ...this.upLoadFileList]
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
          if (!res.result) {
            return
          }
          if (this.handleSelectType === 'customer') {
            this.form.salesMan = res.result.slpName
            this.form.customerName = res.result.cardName
            if (this.form.terminalCustomerId === this.form.customerId) {
              this.form.terminalCustomer = res.result.cardName
            }
          } else {
            this.form.terminalCustomer = res.result.cardName
          }
          // 如果已经存在终端客户，则只有改变终端客户时才可以更改相关信息
          // 如果终端客户代码和客户代码一致，则可以更改相关信息
          if ((this.handleSelectType === 'terminalCustomer' && this.form.terminalCustomerId) || 
            (this.handleSelectType === 'customer' && this.form.terminalCustomerId === this.form.customerId) ||
            this.formName === '确认') {
            this.addressList = res.result.addressList;
            this.cntctPrsnList = res.result.cntctPrsnList;
            this.form.supervisor = res.result.techName;
            if (this.cntctPrsnList && this.cntctPrsnList.length) {
            // let firstValue = res.result.cntctPrsnList[0]
              // let { tel1, tel2, cellolar, name } = firstValue
              if (this.formName !== '确认') {
                // this.form.newestContacter = name
                // this.form.newestContactTel = tel1 || tel2 || cellolar
              }
            }
            if (this.addressList.length) {
              let { address, building, city } = this.addressList[0];
              this.form.addressDesignator = address;
              this.form.address = city + building
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
      this.listQuerySearch.page = val.page;
      this.listQuerySearch.limit = val.limit;
      this.getPartnerList(this.listQuerySearch, 'search');
    },
    handleClose() {
      //  ##地图关闭之前执行的操作
      //  this.drawerMap=false
    },
    openMap() {
      // this.drawerMap = true;
    },
    openDialog() {
      //打开前赋值
      // this.filterPartnerList = this.partnerList;
    },
    searchList: debounce(function() {
      this.listQuerySearch.CardCodeOrCardName = this.inputSearch;
      this.listQuerySearch.ManufSN = this.inputSerial
      // this.form.customerId = this.inputSearch;
      this.listQuerySearch.slpName = this.inputName
      this.listQuerySearch.Technician = this.inputTech
      this.listQuerySearch.Address = this.inputAddress
      this.getPartnerList(this.listQuerySearch, 'search')
    }, 400),
    async querySearch(queryString, cb) {
      this.listQuery.CardCodeOrCardName = queryString;
      await this.getPartnerList(this.listQuery);
      // 调用 callback 返回建议列表的数据
      cb(this.partnerList);
    },
    queryCntctPrsnList (queryString, cb) {
      console.log('queryCtctP', this.cntctPrsnList)
      cb(this.cntctPrsnList)
    },
    handleSelectCntct (item) {
      console.log(item, 'cntct')
      let { tel1, tel2, cellolar, name } = item
      this.form.newestContacter = name
      this.form.newestContactTel = tel1 || tel2 || cellolar
      console.log(this.form.contacter, this.form.newestContactTel)
    },
    async getPartnerList(listQuery, type) {
      if (typeof this.cancelBusinessRequestFn === 'function' && type) {
        this.cancelBusinessRequestFn('用户取消请求')
      }
      this.parentLoad = true;
      return getPartner(listQuery, this, type)
        .then((res) => {
          let list = res.data
          if (type === 'first') {
            this.filterPartnerList = list
            this.partnerList = list
            this.parentCount = res.count;
          } else if (type === 'search') {
            this.filterPartnerList = list;
            this.parentCount = res.count;
          } else {
            this.partnerList = list;
          }
          // this.parentCount = res.count;
          this.parentLoad = false;
        })
        .catch((err) => {
          if (err.message !== '用户取消请求') {
            this.parentLoad = false
          }          
        });
    },
    handleSelectCustomer(item) {
      this.handleSelect(item, 'customer')
    },
    handleSelectTerminal (item) {
      this.handleSelect(item, 'terminalCustomer')
    },
    handleSelect (item, type) {
      this.selectType = 'click' // 通过什么方式来填写客户/终端客户代码
      this.businessType = 'input'
      this.handleSelectType = type // 选择的类型
      this.currentItem = item
      if (type === 'customer') { // 客户代码
        this.form.customerId = item.cardCode;
        this.form.customerName = item.cardName;
        this.form.salesMan = item.slpName;
        if (!this.form.terminalCustomerId) {
          this.form.contactTel = item.cellular;
        }
      } 
    },
    sureVal(item, type) {
      this.dialogPartner = false;
      if (!Object.keys(item).length) {
        return
      }
      this.selectType = 'click'
      this.businessType = 'search'
      this.handleSelectType = type // 选择的类型
      this.currentItem = item
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
        this.handleCurrentChange(item)
      }
    },
    handleCurrentChange (val) {
      if (val.frozenFor == "Y") {
        this.$message({
          message: `${val.cardName}账户被冻结，无法操作`,
          type: "error"
        });
      } else {
        this.selectSerNumberDisabled = false
        let cardCode = String(val.cardCode).toUpperCase()
        this.getPartnerInfo(cardCode)
        if (this.formName === '新建') {
          callformPartner.getTableList({ code: cardCode }).then(res => {
            this.CallList = res.result;
            let { newestOrder } = this.CallList
            if (newestOrder && newestOrder.length) {
              this.dialogCallId = true
            }
          }).catch(() => this.$message.error('获取用户最近10个服务单失败'))
        }
      }
    },
    ChildValue(val) {
      if (val == 1) {
        this.disableBtn = true;
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
  ::v-deep .el-upload-list {
    .el-upload-list__item-name {
      font-size: 12px;
      line-height: 1.2 !important;
      margin-top: 0;
      padding-left: 0;
    }
  }
  ::v-deep .el-radio {
    margin-left: 0 !important;
  }
  .name-class {
    ::v-deep .el-radio__label {
      padding: 0;
    }
  }
  ::v-deep .el-input__inner {
    padding-left: 10px;
    padding-right: 5px;
    // padding-left:25px;
  }
  ::v-deep .el-date-editor {
    .el-input__inner {
      width: 140px;
    }
  }
  .upload-text {
    width: 72px;
    line-height: 40px; 
    text-align: right;
    font-size: 12px;
    color: #606266;
    padding-right: 10px !important;
  }
  .area-content-wrapper {
    position: absolute;
    top: 28px;
    z-index: 2;
    // background-color: red;
  }
  .append-address {
    ::v-deep .el-input-group__append {
      padding: 0 13px;
    }
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
  position: relative;
  .info-wrapper {
      position: absolute;
      top: -36px;
      left: 108px;
      font-size: 12px;
      font-weight: bold;
      line-height: normal;
      & > div {
        margin-right: 15px;
        &.approve {
          color: red;
          span {
            color: #000;
          }
        }
        // min-width: 100px;
        span {
          font-weight: normal;
          min-width: 30px;
        }
      }
    }
  ::v-deep .el-form {
    padding: 5px;
    // margin-bottom: 2px;
    
  }
  &.form-disabled {
    ::v-deep .el-input.is-disabled .el-input__inner {
      background-color: #fff;
      cursor: default;
      color: #606266;
      border-color: #DCDFE6;
    }
    ::v-deep .el-textarea.is-disabled .el-textarea__inner {
      background-color: #fff;
      cursor: default;
      color: #606266;
      border-color: #DCDFE6;
    }
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
