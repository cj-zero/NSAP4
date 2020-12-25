<template>
  <div>
    <sticky :className="'sub-navbar'">
      <div class="filter-container">
        <!-- <el-input
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
        >搜索</el-button> -->
        <Search 
          :listQuery="listQuery" 
          :config="searchConfig"
          @changeForm="onChangeForm" 
          @search="onSearch"
          @advanced="onAdvanced"></Search>
        <permission-btn moduleName="callserve" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
        <Search 
          :listQuery="listQuery" 
          :config="searchConfigAdv"
          @changeForm="onAdvChangeForm" 
          @search="onSearch"
          v-show="advancedVisible"></Search>
      </div>
    </sticky>
    <div class="app-container">
      <div class="bg-white serve-table-wrapper">
        <!-- <zxsearch @change-Search="changeSearch" :options="problemOptions"></zxsearch> -->
        <div class="content-wrapper">
          <el-table
            ref="mainTable"
            class="table_label"
            :key="key"
            :data="list"
            v-loading="listLoading"
            border
            fit
            height="100%"
            highlight-current-row
            style="width: 100%;"
            @row-click="rowClick"
            :row-class-name="rowClassName"
          >
            <div class="mr48">
              <el-table-column type="expand">
                <template slot-scope="scope">
                  <el-table
                    ref="mainTablechuldren"
                    :key="key"
                    :data="scope.row.serviceWorkOrders"
                    v-loading="listLoading"
                    border
                    fit

                    style="width: 100%;"
                    highlight-current-row
                    @row-click="rowClickChild"
                    :row-style="rowStyle"
                  >
                    <el-table-column
                      :show-overflow-tooltip="fruit.name !== 'fromTheme'"
                      v-for="(fruit,index) in ChildheadOptions"
                      :align="fruit.align"
                      :key="`ind${index}`"
                      style="background-color:silver;"
                      :label="fruit.label"
                      header-align="left"
                      :fixed="fruit.ifFixed"
                      :width="fruit.width"
                    >
                      <template slot-scope="scope">
                        <span
                          v-if="fruit.name === 'status'"
                          :class="processStatus(scope.row)"
                        >{{statusOptions[scope.row[fruit.name]-1].label}}</span>
                        <span
                          v-else-if="fruit.name === 'fromType'&&!scope.row.serviceWorkOrders"
                          :class="processFromType(scope.row)"
                        >{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                        <span 
                          v-else-if="fruit.name === 'priority'"
                          :class="processPriorityStatus(scope.row)"
                        >{{priorityOptions[scope.row.priority - 1]}}</span>
                        <template v-else-if="fruit.name === 'fromTheme'">
                          <el-tooltip placement="top-start">
                            <div slot="content">
                              <p v-for="(content, index) in scope.row.themeList" :key="index">{{ content }}</p>
                            </div>
                            <span style="white-space: nowrap;">{{ scope.row[fruit.name] }}</span>
                          </el-tooltip>
                        </template>
                        <span v-else>{{scope.row[fruit.name]}}</span>
                        <!-- <span v-if="fruit.name === 'recepUserName'">{{ scope.row.</span> -->
                      </template>
                    </el-table-column>
                  </el-table>
                </template>
              </el-table-column>
            </div>
            <el-table-column
              :show-overflow-tooltip="fruit.name !== 'fromTheme'"
              v-for="(fruit,index) in ParentHeadOptions"
              :align="fruit.align"
              :key="`ind${index}`"
              style="background-color:silver;"
              :label="fruit.label"
              header-align="left"
              :fixed="fruit.ifFixed"
              :width="fruit.width"
            >
              <template slot-scope="scope">
                <!-- <div v-if="fruit.name === 'radio'">
                  <el-radio v-model="radio" :label="scope.row.serviceOrderId"></el-radio>
                </div> -->
                <span v-if="fruit.name === 'order'">
                  {{ scope.$index + 1 }}
                </span>
                <div v-else-if="fruit.name === 'u_SAP_ID'" class="link-container" >
                  <img :src="rightImg" @click="openTree(scope.row.serviceOrderId)" class="pointer" />
                  <span>{{ scope.row.u_SAP_ID }}</span>
                </div>
                <div v-else-if="fruit.name === 'customerId'" class="link-container" >
                  <img :src="rightImg" @click="getCustomerInfo(scope.row.customerId)" class="pointer" />
                  <span>{{ scope.row.customerId }}</span>
                </div>
                <template v-else-if="fruit.name === 'fromTheme'">
                  <el-tooltip placement="top-start">
                    <div slot="content">
                      <p v-for="(content, index) in scope.row.themeList" :key="index">{{ content }}</p>
                    </div>
                    <span style="white-space: nowrap;">{{ scope.row[fruit.name] }}</span>
                  </el-tooltip>
                </template>
                <span v-else-if="fruit.name === 'status'"
                  :class="processStatus(scope.row)"
                >
                  {{ statusOptions[scope.row.status - 1].label }}
                </span>
                <span
                  v-else-if="fruit.name === 'fromType'"
                  :class="processFromType(scope.row)"
                >{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                <span 
                  v-else-if="fruit.name === 'priority'"
                  :class="processPriorityStatus(scope.row)"
                >{{priorityOptions[scope.row.priority - 1]}}</span>
                <span v-else>{{scope.row[fruit.name]}}</span>
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
      <!-- 客户信息 -->
      <el-dialog
        v-el-drag-dialog
        width="800px"
        :close-on-click-modal="false"
        :modal-append-to-body="false"
        :visible.sync="dialogInfoVisible"
        title="客户信息"
        :modal="false"
      >
        <CustomerInfo :formData="customerInfo" />
      </el-dialog>
      <!-- 客服新建服务单 -->
      <el-dialog
        v-el-drag-dialog
        width="900px"
        top="10vh"
        :modal="false"
        class="dialog-mini"
        :modal-append-to-body="false"
        @close="closeCustoner"
        :close-on-click-modal="false"
        :destroy-on-close="true"
        :title="textMap[dialogStatus]"
        :visible.sync="dialogFormVisible"
      >
        <zxform
          :form="temp"
          formName="新建"
          labelposition="right"
          labelwidth="72px"
          :isCreate="true"
          :sure="sure"
          @close-Dia="closeDia"
          :openTree="openTree"
        ></zxform>
        <div slot="footer">
          <span class="order-num">工单数量: {{ formList.length }}</span>
          <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
          <el-button size="mini" type="primary" :loading="loadingBtn" @click="createData">确认</el-button>
        </div>
      </el-dialog>
      <!-- 客服编辑服务单 -->
      <el-dialog
        v-el-drag-dialog
        width="1210px"
        top="10vh"
        :modal="false"
        :modal-append-to-body="false"
        class="dialog-mini"
        @open="openDetail"
        @close="closeCustoner"
        :close-on-click-modal="false"
        :destroy-on-close="true"
        :title="textMap[dialogStatus]"
        :visible.sync="FormUpdate"
      >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
            :form="temp"
            formName="编辑"
            labelposition="right"
            labelwidth="72px"
            ifEdit="true"
            :isCreate="false"
            :sure="sure"
            :refValue="dataForm"
            @close-Dia="closeDia"
            :serviceOrderId="serviceOrderId"
          ></zxform>
        </el-col>
          <el-col :span="6" class="lastWord">   
            <zxchat :serveId="serveId" formName="编辑" :sapOrderId="sapOrderId" :customerId="customerId"></zxchat>
          </el-col>
        </el-row>
       
        <div slot="footer">
          <span class="order-num">工单数量: {{ formList.length }}</span>
          <el-button size="mini" @click="FormUpdate = false">取消</el-button>
          <el-button size="mini" type="primary" :loading="loadingBtn" @click="createData">确认</el-button>
        </div>
      </el-dialog>
      <!-- 只能查看的表单 -->
      <el-dialog
        v-el-drag-dialog
        :modal-append-to-body="false"
        width="1210px"
        top="10vh"
        :modal="false"
        title="服务单详情"
        :close-on-click-modal="false"
        destroy-on-close
        class="addClass1 dialog-mini"
        @open="openDetail"
        :visible.sync="dialogFormView"
      >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
          <zxform
            :form="temp"
            formName="查看"
            labelposition="right"
            labelwidth="72px"
            max-width="800px"
            :isCreate="false"
            :refValue="dataForm"
          ></zxform>
        </el-col>
          <el-col :span="6" class="lastWord">   
            <zxchat :serveId='serveId' formName="查看"></zxchat>
          </el-col>
        </el-row>

        <div slot="footer">
          <el-button size="mini" @click="handlePhone(multipleSelection, true)" v-if="isCallCenter">回访</el-button>
          <el-button size="mini" @click="dialogFormView = false">取消</el-button>
          <el-button size="mini" type="primary" @click="dialogFormView = false">确认</el-button>
        </div>
      </el-dialog>

      <el-dialog v-el-drag-dialog :visible.sync="dialogTree" center width="300px">
        <treeList @close="dialogTree=false"></treeList>
        <span slot="footer" class="dialog-footer">
          <el-button @click="dialogTree = false">取 消</el-button>
          <el-button type="primary" @click="dialogTree = false">确 定</el-button>
        </span>
      </el-dialog>
      <!-- 电话回访评价 -->
      <my-dialog
        ref="commentDialog"
        width="1015px"
        title="回访"
        :append-to-body="isRateAToBody"
        :btnList="commentBtnList"
        @closed="onRateClose"
      > 
        <Rate :data="commentList" @changeComment="onChangeComment" :isView="isView" ref="rateRoot" />
      </my-dialog>
      <!-- <el-dialog
        :visible.sync="dialogRateVisible"
        :close-on-click-modal="false"
        width="1015px"
        center
        v-el-drag-dialog
        :modal="false"
        :append-to-body="isRateAToBody"
      >
        <Rate :data="commentList" @changeComment="onChangeComment" :isView="isView" ref="rateRoot" />
        <div slot="footer">
          <el-button size="mini" type="primary" :loading="loadingBtn" @click="onCommentSubmit">确认</el-button>
          <el-button size="mini" @click="onRateClose">取消</el-button>
        </div>
      </el-dialog> -->
      <!-- 完工报告  -->
      <el-dialog
        v-el-drag-dialog
        width="983px"
        class="dialog-mini"
        :modal-append-to-body="false"
        :close-on-click-modal="false"
        title="服务行为报告单"
        :modal="false"
        :visible.sync="dialogReportVisible"
        @closed="onReportClosed"
      >
        <Report
          ref="report"
          :data="reportData"
        ></Report>
      </el-dialog>
      <!-- 分析报表 -->
      <my-dialog
        ref="analysisDialog"
        width="983px"
        title="分析报表"
      >
         <Analysis
          ref="report"
          :data="analysisData"
        ></Analysis>
      </my-dialog>
    </div>
  </div>
</template>

<script>
import { mapGetters } from 'vuex'
import * as callservesure from "@/api/serve/callservesure";
import * as businesspartner from "@/api/businesspartner";
import * as afterEvaluation from '@/api/serve/afterevaluation'
import * as problemtypes from "@/api/problemtypes";
import { getReport } from '@/api/serve/report'
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
import elDragDialog from "@/directive/el-dragDialog";
import zxform from "./form";
import Search from '@/components/Search'
// import zxsearch from "./search";
import zxchat from './chat/index'
import treeList from "./treeList";
import CustomerInfo from './customerInfo'
import rightImg from '@/assets/table/right.png'
import Rate from './rate'
import Report from '../common/components/report'
import Analysis from './workOrderReport'
import { chatMixin, reportMixin, tableMixin } from '../common/js/mixins'
import { serializeParams } from '@/utils/process'
export default {
  provide () {
    return {
      instance: this
    }
  },
  name: "callServer",
  computed: {
    ...mapGetters([
      'formList'
    ]),
    isCallCenter () { // 是否是呼叫中心
      return this.$store.state.user.userInfoAll.roles.some(item => item === '呼叫中心')
    },
    commentBtnList () {
      return [
        { btnText: '确认', handleClick: this.onCommentSubmit, loading: this.loadingBtn },
        { btnText: '取消', handleClick: this.onRateClose, className: 'close' }
      ]
    },
    searchConfig () {
      return [
        { width: 100, placeholder: '服务ID', prop: 'QryU_SAP_ID' },
        { width: 90, placeholder: '请选择呼叫状态', prop: 'QryState', options: [{ value: '', label: '全部' }, ...this.statusOptions], type: 'select' },
        { width: 170, placeholder: '客户', prop: 'QryCustomer' },
        { width: 150, placeholder: '序列号', prop: 'QryManufSN' },
        { width: 90, placeholder: '接单员', prop: 'QryRecepUser' },
        { width: 90, placeholder: '技术员', prop: 'QryTechName' },
        { width: 90, placeholder: '主管', prop: 'QrySupervisor' },
        { type: 'search' },
        { type: 'search', btnText: '高级查询' }
      ]
    },
    searchConfigAdv () {
      return [
        { width: 140, placeholder: '问题类型', prop: 'QryProblemType', options: this.problemOptions, type: 'tree' },
        { width: 100, placeholder: '呼叫类型', prop: 'QryFromType', options: this.options_type, type: 'select' },
        { width: 180, placeholder: '呼叫主题', prop: 'QryFromTheme' },
        { width: 140, placeholder: '联系电话', prop: 'ContactTel' },
        { width: 150, placeholder: '创建开始日期', prop: 'QryCreateTimeFrom', type: 'date', showText: true },
        { width: 150, placeholder: '创建结束日期', prop: 'QryCreateTimeTo', type: 'date' },
        { width: 150, placeholder: '完工开始日期', prop: 'CompleteDate', type: 'date' },
        { width: 150, placeholder: '完工结束日期', prop: 'EndCompleteDate', type: 'date' }
      ]
    }
  },
  mixins: [chatMixin, reportMixin, tableMixin],
  components: {
    Sticky,
    permissionBtn,
    Pagination,
    zxform,
    treeList,
    // zxsearch,
    zxchat,
    CustomerInfo,
    Rate,
    Report,
    Search,
    Analysis
  },
  directives: {
    waves,
    elDragDialog
  },
  data() {
    return {
      radio: "", //单选
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      sure: 0,
      rightImg,
      ParentHeadOptions: [
        { name: 'order', label: '序号', width: '38' },
        { name: "u_SAP_ID", label: "服务单号", align:'left', sortable:true, width: '60' },
        { name: "priority", label: "优先级" ,align:'left', width: '50' },
        { name: "fromType", label: "呼叫类型", width: "60px",align:'left'  },
        { name: "status", label: "工单状态", align: 'left', width: '60' },
        { name: "currentUser", label: "技术员" ,align:'left', width: '55' },
        { name: "customerId", label: "客户代码", align:'left', width: '70' },
        { name: "customerName", label: "客户名称" ,align:'left', width: '170' },
        { name: "fromTheme", label: "呼叫主题", align: 'left', width: '275' },
        { name: "manufacturerSerialNumber", label: "制造商序列号",width:'90px' ,align:'left' },
        { name: "materialCode", label: "物料编码",width:'120px' ,align:'left' },
        // { name: "contacter", label: "联系人" ,align:'left', width: '100' },
        // { name: "contactTel", label: "电话号码" ,align:'left', width: '125' },
        { name: "newestContacter", label: "最近联系人" ,align:'left', width: '90' },
        { name: "newestContactTel", label: "最新联系方式" ,align:'left', width: '100' },
        { name: "supervisor", label: "售后主管" ,align:'left', width: '70' },
        { name: "salesMan", label: "销售员" ,align:'left', width: '55' },
        { name: "recepUserName", label: "接单员" ,align:'left', width: '85' },
        { name: 'serviceCreateTime', label: '创建时间', align: 'left', width: '140' },
        { name: 'workOrderNumber', label: '工单数', align: 'left', width: '55' } 
      ],
      ChildheadOptions: [
        // { name: "serviceOrderId", label: "服务单号", ifFixed: true },
        { name: "workOrderNumber", label: "工单号",align:'left', width: '90'  },
        { name: "priority", label: "优先级" ,align:'left', width: '60' },
        { name: "fromType", label: "呼叫类型", width: "100px",align:'left'  },
        // { name: "customerId", label: "客户代码" },
        { name: "status", label: "状态" ,align:'left', width: '80' },
        // { name: "customerName", label: "客户名称" },
         { name: "fromTheme", label: "呼叫主题",align:'left', width: '270'  },
        { name: "createTime", label: "创建日期" ,width:'160px',align:'left' },
        { name: "recepUserName", label: "接单员" ,align:'left' },
        { name: "currentUser", label: "技术员" ,align:'left' },
        { name: "manufacturerSerialNumber", label: "制造商序列号",width:'140px' ,align:'left' },
        { name: "materialCode", label: "物料编码",width:'120px' ,align:'left' },
        { name: "materialDescription", label: "物料描述",width:'120px' ,align:'left' },
        // { name: "contacter", label: "联系人" },
        // { name: "contactTel", label: "电话号码" ,width:'120px'},
        // { name: "supervisor", label: "售后主管" },
        // { name: "salesMan", label: "销售员" },
        { name: "bookingDate", label: "预约时间" ,align:'left' },
        { name: "visitTime", label: "上门时间" ,align:'left' },
        { name: "warrantyEndDate", label: "结束时间",align:'left'  },
      ],
      statusOptions: [
        { value: 1, label: "待处理" },
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "在上门" },
        { value: 5, label: "在维修" },
        { value: 6, label: "已寄回" },
        { value: 7, label: "已完成" },
        { value: 8, label: "已回访" }
      ],
      // callStatus: [
      //   { value: '', label: '全部' },
      //   { value: 1, label: "待处理" },
      //   { value: 2, label: "已排配" },
      //   { value: 3, label: "已预约" },
      //   { value: 4, label: "已外出" },
      //   { value: 5, label: "已挂起" },
      //   { value: 6, label: "已接收" },
      //   { value: 7, label: "已解决" },
      //   { value: 8, label: "已回访" }
      // ],
      options_type: [
        { value: '', label: '全部' },
        { value: 1, label: "提交呼叫" },
        { value: 2, label: "在线解答" },
      ], //呼叫类型
      modulesTree: [],
      priorityOptions: ["低", "中", "高"],
      tableKey: 0,
      formValue: {},
      list: null,
      total: 0,
      loadingBtn: false,
      listLoading: true,
      showDescription: false,
      dialogFormView: false,
      listQuery: {
        // 查询条件
        page: 1,
        limit: 50,
        key: undefined,
        appId: undefined,
        QryState: ''
        // QryServiceOrderId: "" //查询服务单号查询条件
        // QryState: "", //呼叫状态查询条件
        // QryCustomer: "", //客户查询条件
        // QryManufSN: "", // 制造商序列号查询条件
        // QryCreateTimeFrom: "", //创建日期从查询条件
        // QryCreateTimeTo: "" //创建日期至查询条件
        // QryRecepUser:"",//接单员
        // QryTechName:"",//工单技术员
        // QryProblemType:"",//问题类型
        // QryMaterialTypes:""//物料类别（多选)
      },
      queryCustomer: { // 查询用户信息
        // 查询条件
        page: 1,
        limit: 50,
        CardCodeOrCardName:''
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
      customer: {},
      customerInfo: {}, // 查询得到的客户信息
      checkd: "",
      dialogFormVisible: false,
      dialogTable: false,
      dialogTree: false,
      dialogStatus: "",
      dialogInfoVisible: false, // 客户信息弹窗
      FormUpdate: false, //编辑表单的dialog
      textMap: {
        update: "编辑服务呼叫单",
        create: "新建服务呼叫单",
        info: "查看服务呼叫单"
      },
      serveId:'', // 当前服务单ID
      sapOrderId: '', // 当前NSAP_ID
      customerId: "", // 当前客户代码
      dialogPvVisible: false,
      pvData: [],
      rules: {
        appId: [
          { required: true, message: "必须选择一个应用", trigger: "change" }
        ],
        name: [{ required: true, message: "名称不能为空", trigger: "blur" }]
      },
      downloadLoading: false,
      problemOptions: [], // 问题类型
      serviceOrderId: '', // 服务单ID 用于后续工单的创建和修改
      exportExcelUrl: '/serve/ServiceOrder/ExportExcel', // 表格导出地址
      dialogRateVisible: false,
      isRateAToBody: false, // 是否将回访弹窗插入到body中
      commentList: {}, // 评价内容 (新增评价或者查看评价 都要用到)
      newCommentList: {}, // 用于存放修改后的评分列表
      isView: false, // 评分标识(是否是查看)
      advancedVisible: false, // 高级搜索是否展示
      analysisData: []
    };
  },
  filters: {
    filterInt(val) {
      switch (val) {
        case null:
          return "";
        case 1:
          return "状态1";
        case 2:
          return "状态2";
        default:
          return "默认状态";
      }
    },
    statusFilter(disable) {
      const statusMap = {
        false: "color-success",
        true: "color-danger"
      };
      return statusMap[disable];
    }
  },
  watch: {
    formValue: {
      deep: true,
      handler() {
        if (this.formValue && this.formValue.customerId) {
          this.customer = this.formValue;
          //编辑获得的数据
        } else {
          if (!this.dialogFormVisible) {
            this.$message({
              message: "没有发现客户代码，请手动选择",
              type: "warning"
            });
          }
        }
      }
    }
  },
  created() {},
  mounted() {
    this.getList();
    this.getProblemTypeList()
  },
  methods: {
    // 处理状态的样式
    onSearch () {
      this.listQuery.page = 1
      this.getList()
    },
    onChangeForm (val) {
      Object.assign(this.listQuery, val)
    },
    onAdvChangeForm (val) {
      delete val.QryState
      Object.assign(this.listQuery, val)
    },
    onAdvanced () {
      this.advancedVisible = !this.advancedVisible
    },
    checkServeId(res) {
      if (res.children) {
        this.listQuery.QryServiceOrderId = res.label.split("服务单号：")[1];
      }
      if (res === true) {
        this.radio = "";
        this.listQuery.QryServiceOrderId = "";
      }
    },
    rowStyle () { // 工单表格的行样式
      return {
        'background-color': '#F2F6FC'
      }
    },
    openDetail() {
      this.dataForm = this.dataForm1;
    },
    closeCustoner() {
      // this.getList();
    },
    getCustomerInfo (customerId) { // 打开用户信息弹窗
      if (!customerId) {
        return this.$message.error('客户代码不能为空!')
      }
      businesspartner.getCustomerInfo({ CardCode: customerId })
        .then((response) => {
          let { cellular, phone1 } = response.data
          response.data.cellular = cellular || phone1
          this.customerInfo = response.data;
          this.dialogInfoVisible = true
        })
        .catch(() => {
          this.$message.error('查询客户信息失败')
        })
    },
    rowClassName ({ row, rowIndex }) {
      row.index = rowIndex
    },
    rowClick(row) {
      this.$refs.mainTable.clearSelection();
      this.multipleSelection = row;
      this.radio = row.serviceOrderId
      console.log(this.radio, 'radio')
      this.$refs.mainTable.toggleRowSelection(row);
    },
    rowClickChild(row) {
      this.$refs.mainTablechuldren.clearSelection();
      // this.multipleSelection = row;
      this.$refs.mainTablechuldren.toggleRowSelection(row);
    },
    // handleSelectionChange(val) {
    // },
    onBtnClicked: function(domId) {
      switch (domId) {
        case "btnAdd":
          this.handleCreate();
          break;
        case "btnDetail":
          this.open();
          break;
        case "editTable":
          this.dialogTable = true;
          break;
        case "btnReport":
          if (!this.multipleSelection.serviceOrderId) {
            this.$message({
              message: "请先选择服务单",
              type: "warning"
            });
            return;
          }
          this.handleReport(this.multipleSelection.serviceOrderId, this.multipleSelection.serviceWorkOrders) // 传入key中的第一个 服务单ID
          break;
        case "btnAnalysis":
          this.handleAnalysis()
          break
        case "btnPhone": // 电话回访
          if (!this.multipleSelection.serviceOrderId) {
            this.$message({
              message: "请选择需要进行回访的数据",
              type: "warning"
            });
            return;
          }
          this.handlePhone(this.multipleSelection)
          break
        case "btnExcel": // 表格导出Execl表格
          this.handleExcel()
          break
        case "btnRecall":
          if (!this.multipleSelection.serviceOrderId) {
            this.$message({
              message: "请选择需要撤回的数据",
              type: "warning"
            });
            return;
          }
          this.handleRecall(this.multipleSelection)
          break
        case "btnEdit":
          if (!this.multipleSelection.serviceOrderId) {
            this.$message({
              message: "请选择需要编辑的数据",
              type: "warning"
            });
            return;
          }
          // if (this.multipleSelection.status === 2) {
          //   this.$message({
          //     message: "该服务单已经被确认过",
          //     type: "warning"
          //   });
          //   return;
          // }
          this.handleUpdate(this.multipleSelection);
          break;
        case "btnDel":
          if (!this.multipleSelection) {
            this.$message({
              message: "至少删除一个",
              type: "error"
            });
            return;
          }
          this.handleDelete(this.multipleSelection);
          break;
        default:
          break;
      }
    },
    changeSearch(res) {
      if (res === 1) {
        this.getList();
      } else {
        Object.assign(this.listQuery, res);
      }
    },
    // changeState(ind){
    //   this.formList[ind].editTrue = !this.formList[ind].editTrue
    // },
    _normalize (data) {
      let resultArr = data.map(item => {
        let { 
          recepUserName, 
          serviceWorkOrders, 
          customerId,
          customerName,
          terminalCustomerId,
          terminalCustomer } = item
        item.customerId = terminalCustomerId ? terminalCustomerId : customerId
        item.customerName = terminalCustomer ? terminalCustomer : customerName
        // ite
        console.log(item.u_SAP_ID, 'usapid')
        if (serviceWorkOrders.length) {
          serviceWorkOrders.forEach(workItem => {
            workItem.recepUserName = recepUserName
            let theme = workItem.fromTheme
            let reg = /[\r|\r\n|\n\t\v]/g
            theme = theme.replace(reg, '')
            console.log(theme)
            workItem.themeList = JSON.parse(theme).map(item => item.description.trim())
            workItem.fromTheme = workItem.themeList.join(' ')
          })
          let {
            fromTheme,
            priority,
            fromType,
            currentUser,
            materialCode,
            manufacturerSerialNumber,
            themeList,
            status
          } = this.processServiceOrders(serviceWorkOrders)
          item.fromTheme = fromTheme
          item.themeList = themeList
          item.priority = priority
          item.fromType = fromType
          item.currentUser = currentUser
          item.materialCode = materialCode
          item.manufacturerSerialNumber = manufacturerSerialNumber
          item.workOrderNumber = serviceWorkOrders.length
          item.status = status
        }
        return item
      })
      this.list = resultArr
    },
    processStatusText (serviceWorkOrders) {
      if (serviceWorkOrders && serviceWorkOrders.length === 1) {
        return serviceWorkOrders[0].status
      }
      let result = []
      serviceWorkOrders.forEach(serviceOrder => {
        result.push(serviceOrder.status)
      })
      let processing = result.some(item => item <= 6) // 有正在处理的服务单
      if (processing) {
        return Math.max.apply(null, result.filter(item => item <= 6)) // 优先级越大优先展示
      } else {
        return Math.min.apply(null, result.filter(item => item >= 7)) // 已访问 优先于 已回访
      }
    },
    getList() {
      this.listLoading = true;
      callservesure.rightList(this.listQuery).then(response => {
        let result = response.data.data;
        this.total = response.count;
        this._normalize(result)
        this.listLoading = false;
      }).catch((err) => {
        console.log(err, 'err')
        this.listLoading = false;
        this.$message({
          type: "error",
          message: `请输入正确的搜索值`
        });
        // let that = this
        // setTimeout(function(){
        //             that.$message({
        //   type: "error",
        //   message: `请输入正确的搜索值`
        // });
        //   that.list = [];
        //   that.total =0;
        //   that.listLoading = false;
        // },700)
      });
    },
    getProblemTypeList () {
      problemtypes
      .getList()
      .then((res) => {
        this.problemOptions = this._normalizeProblemType(res.data);
      })
      .catch((error) => {
        console.log(error);
      });
    },
    _normalizeProblemType (data) {
      // 处理问题类型数据
      const typeList = []
      data.forEach(item => {
        let { childTypes } = item
        if (childTypes && childTypes.length) {
          item.childTypes = this._normalizeProblemType(childTypes)
        } else {
          delete item.childTypes
        }
        typeList.push(item)
      })
      return typeList
    },
    open() {
      this.$confirm("确认已完成回访?", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      })
        .then(() => {
          this.$message({
            type: "success",
            message: "操作成功!"
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消操作"
          });
        });
    },
    handleFilter() {
      // this.listQuery.page = 1;
      this.getList();
    },
    handleSizeChange(val) {
      this.listQuery.limit = val;
      this.getList();
    },
    handleCurrentChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      this.radio = ''
      this.getList();
    },
    handleModifyStatus(row, disable) {
      // 模拟修改状态
      this.$message({
        message: "操作成功",
        type: "success"
      });
      row.disable = disable;
    },
    resetTemp() {
      this.temp = {
        id: "",
        sltCode: "",
        subject: "",
        cause: "",
        symptom: "",
        descriptio: "",
        status: "",
        createUserId: "",
        createUserName: "",
        createTime: "",
        updateUserId: "",
        updateTime: "",
        updateUserName: "",
        extendInfo: ""
      };
    },
    handleCreate() {
      // 弹出添加框
      this.resetTemp();
      this.dialogStatus = "create";
      this.dialogFormVisible = true;
      // this.$nextTick(() => {
      //   this.$refs["dataForm"].clearValidate();
      // });
    },
    createData() {
      this.loadingBtn = true
      this.sure = this.sure + 1; //向form表单发送提交通知
      // 保存提交
      // this.$refs["dataForm"].validate(valid => {
      //   if (valid) {
      //     callservesure.add(this.temp).then(() => {
      //       this.list.unshift(this.temp);
      //       this.dialogFormVisible = false;
      //       this.$notify({
      //         title: "成功",
      //         message: "创建成功",
      //         type: "success",
      //         duration: 2000
      //       });
      //     });
      //   }
      // });
    },
    handleUpdate(row) {
      // 弹出编辑框
        this.listLoading = true;
      callservesure.GetDetails(row.serviceOrderId).then(res => {
        if (res.code == 200) {
          this.dataForm1 = this._normalizeOrderDetail(res.result);
          let { serviceOrderId, u_SAP_ID, customerId } = row
          this.serveId = serviceOrderId
          this.sapOrderId = u_SAP_ID
          this.customerId = customerId 
          this.serviceOrderId = serviceOrderId // 服务单ID
          this.dialogStatus = "update";
          this.FormUpdate = true;
        }
        this.listLoading = false;
      });
    },
    handleRecall (row) { // 撤回功能
      let { serviceOrderId } = row
      this.$confirm("确认撤销?", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      })
      .then(() => {
        callservesure.deleteServiceOrder({
          serviceOrderId
        }).then(() => {
          this.$message({
            type: "success",
            message: "撤销成功!"
          })
          this.getList();
        }).catch(err => {
          this.$message.error(err.message)
        })
      })
    },
    handleAnalysis () {
      getReport(this.listQuery).then(res => {
        let isEmpty = res.data.every(item => item.statList.length === 0)
        if (isEmpty) {
          return this.$message.error('暂无数据')
        }
        this.analysisData = res.data
        this.$refs.analysisDialog.open()
      }).catch(() => {
        this.$message.error('暂无数据')
      })
    },
    handlePhone (row, isInTable) { // 电话回访
      let { serviceOrderId, serviceWorkOrders } = row // 8 代表已回访
      let hasVisit = serviceWorkOrders.every(item => { // 是否已经回访
        return Number(item.status) === 8
      })
      if (hasVisit) {
        this.$message.warning('该服务单已评价')
      } else {
        afterEvaluation.getTechnicianName({
          serviceOrderId
        }).then(res => {
          if (!res.data) {
            return this.$message.warning('工单未解决或在线解答方式不可回访')
          }
          this.isView = false
          this.commentList = this._normalizeCommentList(res, row)
          // this.dialogRateVisible = true
          this.$refs.commentDialog.open()
          this.isRateAToBody = Boolean(isInTable) // 判断是否将dialog插入到body中
        }).catch(err => {
          this.$message.error(err.message)
        })
      }
    },
    _normalizeCommentList (res, row) {
      let { serviceOrderId, contactTel, customerId, customerName, contacter, terminalCustomerId } = row
      let technicianEvaluates = res.data || []// 技术员列表
      let commentList = {
        serviceOrderId,
        customerId: terminalCustomerId || customerId,
        cutomer: customerName,
        contact: contacter,
        caontactTel: contactTel,
        productQuality: 0,
        servicePrice: 0,
        comment: '',
        technicianEvaluates: []
      }
      technicianEvaluates.forEach(item => {
        if (!item.currentUserId) {
          return
        }
        commentList.technicianEvaluates.push({
          technicianAppId: item.currentUserId,
          responseSpeed: 0,
          schemeEffectiveness: 0,
          serviceAttitude: 0,
          name: item.currentUser
        })
      })
      return commentList
    },
    onRateClose () {
      this.dialogRateVisible = false
      this.loadingBtn = false
      this.$refs.commentDialog.close()
      if (!this.isView) { // 关闭弹窗时，清空数据
        this.$refs.rateRoot.resetInfo()
      }
    },
    onCommentSubmit () { // 提交评价
      if (this.isView) { // 如果是查看操作，则直接关闭弹窗
        // return this.dialogRateVisible = false
        return this.$refs.commentDialog.close()
      }
      let { productQuality, servicePrice, technicianEvaluates } = this.commentList
      let isValid = true
      for (let i = 0; i < technicianEvaluates.length; i++) {
        let { responseSpeed, schemeEffectiveness, serviceAttitude } = technicianEvaluates[i]
        isValid = responseSpeed && schemeEffectiveness && serviceAttitude
        if (!isValid) {
          break
        }
      }
      if (!(isValid && productQuality && servicePrice)) {
        return this.$message.error('评分不能为零！')
      }
      this.loadingBtn = true
      afterEvaluation.addComment(this.commentList)
        .then(() => {
          this.$message.success('评价成功')
          this.$refs.rateRoot.resetInfo()
          this.loadingBtn = false
          // this.dialogRateVisible = false
          this.$refs.commentDialog.close()
          this.dialogFormView = false
          this.getList()
        }).catch(() => {
          this.loadingBtn = false
          this.$message.error('评价失败')
        })
    },
    handleExcel () { // 导出表格
      this.$confirm('确认导出？', '确认信息', {
        confirmButtonText: '确认',
        cancelButtonText: '取消',
        closeOnClickModal: false,
        type: 'warning'
      })
      .then(() => {
        let searchStr = serializeParams(this.listQuery)
        searchStr += `&X-Token=${this.$store.state.user.token}`
        let baseURL = `${process.env.VUE_APP_BASE_API}${this.exportExcelUrl}`
        console.log(`${baseURL}?X-Token=${this.$store.state.user.token}&${searchStr}`)
        window.open(`${baseURL}?${searchStr}`, '_blank')
      }) 
    },
    onChangeComment (val) {
      Object.assign(this.commentList, val)
      // this.newCommentList = val
    },
    closeDia(a) {
      if (a === 'closeLoading') {
        return this.loadingBtn = false
      }
      if (a === 'y') {
        this.getList();
      }
      if(a=='N'){
         this.loadingBtn = false
         return 
      }
      this.loadingBtn = false;
      this.dialogFormVisible = false;
      this.FormUpdate = false
    },
    updateData() {
      // this.sure = this.sure + 1; //向form表单发送提交通知
    },
    handleDelete(rows) {
      // 多行删除
      callservesure.del(rows.map(u => u.id)).then(() => {
        this.$notify({
          title: "成功",
          message: "删除成功",
          type: "success",
          duration: 2000
        });
        rows.forEach(row => {
          const index = this.list.indexOf(row);
          this.list.splice(index, 1);
        });
      });
    }
  }
};
</script>
<style lang="scss" scoped>
.serve-table-wrapper {
  ::v-deep .el-table__expanded-cell > div {
    margin-left: 48px;
  }
}
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
.position-view {
  position: relative;
  .lastWord {
    position: sticky;
    top: 0;
    // width: 200px;
  }
}
.table_label {
  ::v-deep.el-radio {
    // margin-left: 6px;
  }
  ::v-deep.el-radio__label {
    display: none;
  }
  ::v-deep.el-table__expanded-cell {
    padding: 0 0 0 50px;
  }
}
.bg-head {
  ::v-deep .el-table thead {
    color: green;
  }
}
.addClass1 {
  // ::v-deep .el-dialog__header {
  //   .el-dialog__title {
  //     color: white;
  //   }
  //   .el-dialog__close {
  //     color: white;
  //   }
  //   background: lightslategrey;
  // }
  ::v-deep .el-dialog__body {
    padding: 10px 20px;
  }
}
.order-num {
  margin-right: 10px;
}
.mr48 {
  margin-left: 48px;
}

// }
</style>




