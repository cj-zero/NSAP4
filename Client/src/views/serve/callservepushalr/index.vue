<template>
  <div>
    <tab-list :initialName="initialName" :texts="texts" @tabChange="onTabChange" class="tabList"></tab-list>
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
        <zxsearch 
          @change-Search="changeSearch" 
          @change-Order="changeOrder"
          :activeName="activeName"
        ></zxsearch>
        <permission-btn moduleName="callservepushalr" size="mini" v-on:btn-event="onBtnClicked"></permission-btn>
      </div>
    </sticky>
    <div class="app-container flex-item bg-white">
      <!-- <zxsearch 
        @change-Search="changeSearch" 
        @change-Order="changeOrder"
      ></zxsearch> -->
      <el-row class="fh" type="flex">
        <div class="fh ls-border" style="min-width:200px;" @scroll="onScroll">
          <el-card shadow="never" class="card-body-none fh" style>
            <!-- <el-link
              style="width:100%;height:30px;color:#409EFF;font-size:16px;text-align:center;line-height:30px;border:1px silver solid;"
              @click="getAllRight"
            >全部服务单>></el-link>-->
            <div
              style="width:100%;height:30px;color:#409EFF;font-size:16px;text-align:center;line-height:30px;border:1px silver solid;"
            >服务单列表</div>

            <el-tree
              style="overflow-y: auto;"
              :data="modulesTree"
              show-checkbox
              node-key="key1"
              @check="checkGroupNode"
              ref="treeForm"
              highlight-current
              :props="defaultProps"
            ></el-tree>
          </el-card>
        </div>
        <el-col :span="21" class="fh">
          <div class="bg-white">
            <div class="content-wrapper">
              <el-table
                ref="mainTable"
                :key="key"
                :data="list"
                v-loading="listLoading"
                border
                fit
                height="100%"
                tooltip-effect="dark"
                style="width: 100%;"
                highlight-current-row
                @row-click="rowClick"
              >
                <el-table-column
                  show-overflow-tooltip
                  v-for="(fruit,index) in formTheadOptions"
                  :align="fruit.align?fruit.align:'left'"
                  :header-align="fruit.align?fruit.align:'left'"
                  :key="`ind${index}`"
                  :sortable="fruit=='chaungjianriqi'?true:false"
                  style="background-color:silver;"
                  :label="fruit.label"
                  :fixed="fruit.ifFixed"
                  :width="fruit.width"
                >
                  <template slot-scope="scope">
                    <div v-if="fruit.name === 'workOrderNumber'" class="link-container" >
                      <img :src="rightImg" @click="openTree(scope.row.serviceOrderId)" class="pointer" />
                      <span>{{ scope.row.workOrderNumber }}</span>
                    </div>
                    <span
                      v-if="fruit.name === 'status'"
                      :class="processStatus(scope.row)"
                    >{{statusOptions[scope.row[fruit.name]-1].label}}</span>
                    <span v-if="fruit.name === 'customerId'">
                      {{ scope.row.terminalCustomerId ? scope.row.terminalCustomerId : scope.row.customerId }}
                    </span>
                    <span v-if="fruit.name === 'customerName'">
                      {{ scope.row.terminalCustomer ? scope.row.terminalCustomer : scope.row.customerName }}
                    </span>
                    <span v-if="fruit.name === 'fromType'">{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                    <span v-if="fruit.name === 'priority'">{{priorityOptions[scope.row.priority - 1]}}</span>
                    <span
                      v-if="fruit.name != 'priority' 
                      && fruit.name!='fromType' 
                      && fruit.name!='status'
                      && fruit.name!='serviceOrderId'
                      && fruit.name !== 'workOrderNumber'
                      && fruit.name !== 'customerName'
                      && fruit.name !== 'customerId'"
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
        </el-col>
      </el-row>
      <!--   v-el-drag-dialog
      width="1000px"  新建呼叫服务单-->
      <el-dialog
        width="800px"
        class="dialog-mini"
        :destroy-on-close="true"
        :close-on-click-modal="false"
        :title="textMap[dialogStatus]"
        :visible.sync="dialogFormVisible"
      >
        <zxform
          :form="temp"
          formName="编辑"
          labelposition="right"
          labelwidth="100px"
          :isCreate="true"
          refValue="dataForm"
        ></zxform>
        <div slot="footer">
          <el-button size="mini" @click="dialogFormVisible = false">取消</el-button>
          <el-button size="mini" v-if="dialogStatus=='create'" type="primary" @click="createData">确认</el-button>
          <el-button size="mini" v-else type="primary" @click="updateData">确认</el-button>
        </div>
      </el-dialog>
      <!-- 只能查看的表单 -->
      <!-- <el-dialog
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
      </el-dialog> -->
      <!-- 只能查看的表单 -->
      <el-dialog
        width="1210px"
        top="10vh"
        title="服务单详情"
        :close-on-click-modal="false"
        destroy-on-close
        class="addClass1 dialog-mini"
        :visible.sync="dialogFormView"
        @open="openDetail"
      >
      <el-row :gutter="20" class="position-view">
        <el-col :span="18" >
        <zxform
          :form="temp"
          formName="查看"
          labelposition="right"
          labelwidth="100px"
          max-width="800px"
          :isCreate="false"
          :refValue="dataForm"
        ></zxform>
        </el-col>
          <el-col :span="6" class="lastWord">   
            <zxchat :serveId="serveId"></zxchat>
          </el-col>
        </el-row>

        <div slot="footer">
          <el-button size="mini" @click="dialogFormView = false">取消</el-button>
          <el-button size="mini" type="primary" @click="dialogFormView = false">确认</el-button>
        </div>
      </el-dialog>
      <el-dialog
        v-el-drag-dialog
        :visible.sync="dialogTable"
        :destroy-on-close="true"
        center
        width="800px"
      >
        <DynamicTable
          :formThead.sync="formTheadOptions"
          :defaultForm.sync="defaultFormThead"
          @close="dialogTable=false"
        ></DynamicTable>
        <span slot="footer" class="dialog-footer">
          <el-button @click="dialogTable = false">取 消</el-button>
          <el-button type="primary" @click="dialogTable = false">确 定</el-button>
        </span>
      </el-dialog>
      <el-dialog
        v-el-drag-dialog
        :visible.sync="dialogTree"
        :destroy-on-close="true"
        center
        width="300px"
      >
        <treeList @close="dialogTree=false"></treeList>
        <span slot="footer" class="dialog-footer">
          <el-button @click="dialogTree = false">取 消</el-button>
          <el-button type="primary" @click="dialogTree = false">确 定</el-button>
        </span>
      </el-dialog>
      <el-dialog
        v-el-drag-dialog
        :visible.sync="dialogOrder"
        :destroy-on-close="true"
        title="选择转派对象"
        center
        width="500px"
        @closed="onClosed"
      >
        <el-row type="flex" justify="end" style="margin-bottom: 10px;">
          <el-input 
            v-model="listQuery2.currentUser" 
            size="mini" 
            @keyup.enter.native="onSearchUser"
            style="width:200px;"
            placeholder="技术员"></el-input>
        </el-row>
        <el-table :data="tableData" border @row-click="setRadio" style="width: 100%">
          <el-table-column align="center">
            <template slot-scope="scope">
              <el-radio :label="scope.row.appUserId" v-model="orderRadio">{{&nbsp;}}</el-radio>
            </template>
          </el-table-column>
          <el-table-column prop="name" label="接单员" align="center"></el-table-column>
          <el-table-column prop="count" label="已接服务单数" align="center" width="180"></el-table-column>
        </el-table>
        <pagination
          v-show="total2>0"
          :total="total2"
          :page.sync="listQuery2.page"
          :limit.sync="listQuery2.limit"
          @pagination="handleCurrentChange2"
          layout="total, prev, pager, next"
        />
        <span slot="footer" class="dialog-footer">
          <el-button @click="cancelPost">取 消</el-button>
          <el-button type="primary" @click="postOrder" :loading="loadingBtn">确 定</el-button>
        </span>
      </el-dialog>
      <!-- 完工报告  -->
      <el-dialog
        width="983px"
        class="dialog-mini"
        :close-on-click-modal="false"
        title="服务行为报告单"
        :visible.sync="dialogReportVisible"
        @closed="onReportClosed"
      >
        <Report
          ref="report"
          :data="reportData"
        ></Report>
      </el-dialog>
    </div>
  </div>
</template>

<script>
import * as solutions from "@/api/solutions";
import TabList from '@/components/TabList'
import * as callservepushm from "@/api/serve/callservepushm";
// import * as callservesure from "@/api/serve/callservesure";
import * as category from "@/api/categorys"
import waves from "@/directive/waves"; // 水波纹指令
import Sticky from "@/components/Sticky";
import permissionBtn from "@/components/PermissionBtn";
import Pagination from "@/components/Pagination";
import DynamicTable from "@/components/DynamicTable";
import elDragDialog from "@/directive/el-dragDialog";
import zxsearch from "./search";
import zxform from "../callserve/form";
import zxchat from "../callserve/chat/index"
import treeList from "../callserve/treeList";
import { debounce } from '@/utils/process'
import Report from '../common/components/report'
import rightImg from '@/assets/table/right.png'
// import treeTable from "@/components/TreeTableMlt";
import { 
  reportMixin, 
  dispatchMixin, 
  chatMixin, 
  tableMixin 
} from '../common/js/mixins'
// import { callserve, count } from "@/mock/serve";
export default {
  name: "callServePushAlr",
  components: {
    Sticky,
    permissionBtn,
    Pagination,
    DynamicTable,
    zxsearch,
    zxform,
    treeList,
    Report,
    zxchat,
    TabList
  },
  mixins: [reportMixin, dispatchMixin, chatMixin, tableMixin],
  directives: {
    waves,
    elDragDialog,
  },
  data() {
    return {
      tableData: [], //接单员列表
      multipleSelection: [], // 列表checkbox选中的值
      key: 1, // table key
      initialName: 'first',
      texts: [ // 标签数组
        { label: '已派单', name: 'first' },
        { label: '已解决', name: 'second' },
      ],
      activeName: 'first',
      defaultFormThead: [
        "priority",
        "calltype",
        "chaungjianriqi",
        "callstatus",
        "moneyapproval",
        "kehidaima",
        "kehumingcheng",
      ],
      dialogOrder: false,
      modulesTree: [], // 左侧数据构成的树
      workorderidList: [],
      hasAlreadNum: "", //已经接的单
      formTheadOptions: [
        {
          name: "workOrderNumber",
          label: "工单ID",
          ifFixed: true,
          align: "left",
        },
        { name: "priority", label: "优先级", width: 80 },
        // { name: "fromType", label: "呼叫类型", width: "100px" },
        { name: "status", label: "呼叫状态", width: 100 },
        // { name: "", label: "费用审核" },
        // { name: "customerId", label: "客户代码" },
        { name: "customerName", label: "客户名称", width: 200 },
        { name: "fromTheme", label: "呼叫主题", width: 275 },
        { name: "createTime", label: "创建日期", width: 150 },
        { name: "currentUser", label: "技术员", width: 80 },
        {
          name: "manufacturerSerialNumber",
          label: "制造商序列号",
          width: "120px",
        },
        { name: "materialCode", label: "物料编码", width: 150, },
        { name: "materialDescription", label: "物料描述", width: 150, },
        { name: "contacter", label: "联系人", width: 100 },
        { name: "contactTel", label: "电话号码", width: 150 },
        { name: "supervisor", label: "售后主管", width: 80 },
        { name: "salesMan", label: "销售员", width: 80 },
        { name: "recepUserName", label: "接单员", width: 100 },
        // "serviceWorkOrderId": 1,
        // "problemTypeName": "数值异常",
        // "currentUserId": 1
      ],
      defaultProps: {
        children: "children",
        label: "label",
      },
      tableKey: 0,
      list: null,
      total: 0,
      listLoading: true,
      showDescription: false,
      dialogFormView: false,
      checkChildList: [],
      ifParent: "", //是否选过父级
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined,
        Name: "", //	Description
        // u_SAP_ID: "", //- 查询服务ID查询条件
        qryStatusBar: 1, // 默认取已派单 1: 已派单 2: 已解决
        u_SAP_ID: "",
        QryU_SAP_ID: "", // 查询服务ID
        QryState: "", //- 呼叫状态查询条件
        QryCustomer: "", //- 客户查询条件
        QryManufSN: "", // - 制造商序列号查询条件
        QryCreateTimeFrom: "", //- 创建日期从查询条件
        QryCreateTimeTo: "", //- 创建日期至查询条件
        QryRecepUser: "", //- 接单员
        QryTechName: "", // - 工单技术员
        QryProblemType: "", // - 问题类型
        QryMaterialTypes: [], //物料类型
        QryMaterialCode: "" // 物料代码
      },
      listQuery1: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined,
        Name: "", //	Description
        u_SAP_ID: "", //- 查询服务ID查询条件
        QryState: "", //- 呼叫状态查询条件
        QryCustomer: "", //- 客户查询条件
        QryManufSN: "", // - 制造商序列号查询条件
        QryCreateTimeFrom: "", //- 创建日期从查询条件
        QryCreateTimeTo: "", //- 创建日期至查询条件
        QryRecepUser: "", //- 接单员
        QryTechName: "", // - 工单技术员
        QryProblemType: "", // - 问题类型
        QryMaterialTypes: [], //物料类型
      },
      statusOptions: [
        { value: 1, label: "待处理" },
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "已外出" },
        { value: 5, label: "已挂起" },
        { value: 6, label: "已接收" },
        { value: 7, label: "已解决" },
        { value: 8, label: "已回访" },
      ],
      priorityOptions: ["低", "中", "高"],
      temp: {
        id: "", // Id
        sltCode: "", // SltCode
        subject: "", // Subject
        cause: "", // Cause
        symptom: "", // Symptom
        descriptio: "", // Descriptio
        status: "", // Status
        extendInfo: "", // 其他信息,防止最后加逗号，可以删除
      },
      total2: 0,
      totalCount: 0, // 左侧树形数据的数量
      dialogFormVisible: false,
      dialogTable: false,
      dialogTree: false,
      dialogStatus: "",
      textMap: {
        update: "确认服务呼叫单",
        create: "新建服务呼叫单",
      },
      // dataForm: {}, //获取的详情表单
      dialogPvVisible: false,
      pvData: [],
      params: {
        currentUserId: ""
      },
      rules: {
        appId: [
          { required: true, message: "必须选择一个应用", trigger: "change" },
        ],
        name: [{ required: true, message: "名称不能为空", trigger: "blur" }],
      },
      downloadLoading: false,
      listQueryServer: {
        qryStatusBar: 1, // 默认取已派单 1: 已派单 2: 已解决
        QryState: "", // 客户状态
        QryU_SAP_ID: '', // 查询服务ID
        limit: 30, // 条数
        page: 1 // 页数
      },
      isClear: false, // 清空树形数据moduleTree
      // reportData: {}, // 完工报告详情
      serveid: '',
      rightImg,
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
        true: "color-danger",
      };
      return statusMap[disable];
    },
  },
  watch: {
    defaultFormThead(valArr) {
      this.formTheadOptions = this.formTheadOptions.filter(
        (i) => valArr.indexOf(i) >= 0
      );
      // }
      this.key = this.key + 1; // 为了保证table 每次都会重渲 In order to ensure the table will be re-rendered each time
    },
    list: {
      handler(val) {
        this.workorderidList = [];
        val.map((item) => {
          this.workorderidList.push(item.serviceWorkOrderId);
        });
        // console.log(this.workorderidList);
      },
    }
  },
  created() {},
  mounted() {
    let el = document.getElementsByClassName('el-tree')[0]
    el.addEventListener('scroll', debounce(this.onScroll, 400))
    this.afterLeft();
    this.getCategory()
  },
  methods: {
    onScroll (e) {
      console.log(this.totalCount, this.modulesTree.length, 'scroll')
      if (this.totalCount <= this.modulesTree.length) {
        return
      }
      let { scrollHeight, scrollTop, clientHeight } = e.target
      if (scrollHeight <= (scrollTop + clientHeight + 100)) {
        this.listQueryServer.page++
        this.getLeftList()
      }
    },
    onTabChange (name) {
      this.activeName = name
    },
    getCategory () {
      category.loadCategory({
        typeID: 'Aftermarket'
      }).then(res => {
        let { data } = res
        console.log(data, 'data')
        let target = data.filter(item => {
          return item.dtCode === 'Send_Order_Count'
        })
        this.totalLimit = target[0].dtValue
        console.log(this.totalLimit)
      })
    },
    cancelPost() {
      (this.dialogOrder = false), (this.listLoading = false);
      // this.afterLeft()
      this.getRightList();
    },
    async afterLeft() {
      await this.getLeftList();
      if (this.modulesTree.length > 0) {
        // console.log(this.modulesTree[0].key)
        // this.$refs.treeForm.setCheckedKeys([this.modulesTree[0].key]);
        this.checkGroupNode(this.modulesTree[0]);
        // this.getRightList();
      } else {
        this.list = []
        this.total = 0
      }
    },
   async   changeOrder() {
      if (this.ifParent) {
        let checkStatus = this.list.every(
          (item) => item.status > 1 && item.status < 5
        );
        if (!checkStatus) {
          this.$message({
            type: "warning",
            duration: 5000,
            message: "仅支持状态为已排配，已预约，已外出，已挂起的工单进行转派",
          });
        } else {
          this.dialogOrder = true;
              //  this.listQuery.limit=999
        await this.getRightList(); 
          this._getAllowSendOrderUser()
        }
      } else {
        this.$message({
          type: "warning",
          message: "请先选择需要转派的服务号",
        });
      }
    },
    setRadio(res) {
      this.orderRadio = res.appUserId;
      this.hasAlreadNum = res.count;
    },
    postOrder() {
      this.listLoading = true;
      this.params.currentUserId = this.orderRadio;
      let checkedKey = this.$refs.treeForm.getCheckedKeys()[0]
      if (checkedKey.indexOf('&') !== -1) {
        let index = checkedKey.indexOf('&')
        checkedKey = checkedKey.slice(0, index)
      }
      this.params.qryMaterialTypes = this.listQuery.QryMaterialTypes
      this.params.serviceOrderId = checkedKey
      if (this.hasAlreadNum > this.totalLimit) {
        this.$message({
          type: "warning",
          message: `单个技术员接单不能超过${this.totalLimit}个`,
        });
        this.listLoading = false
      } else if (!this.orderRadio) {
        this.$message({
          type: "warning",
          message: '请先选择技术员'
        });
      } else {
        this.loadingBtn = true
        callservepushm
          .nSAPSendOrders(this.params)
          .then((res) => {
            if (res.code == 200) {
              this.dataForm = res.result;
              this.$message({
                type: "success",
                message: "转派成功",
              });
              this.listQuery.QryState=''
              this.listQuery.QryU_SAP_ID = ''
              this.isClear = true
              this.listQueryServer.page = 1
              this.listQuery.page = 1
              this.ifParent = ''
              this.afterLeft()
              this.dialogOrder = false;
              this.listLoading = false;
              this.loadingBtn = false
            }
          })
          .catch((error) => {
            this.$message({
              type: "danger",
              message: `${error}`,
            });
            this.listLoading = false;
            this.loadingBtn = false
          });
      }

    },
    async changeSearch(val) {
      this.isClear = true
      this.ifParent = ""; // 清空父级选择
      this.listQuery.QryMaterialTypes = [];
      this.listQuery = Object.assign(this.listQuery, val)
      this.listQueryServer = Object.assign(this.listQueryServer, val)
      this.listQueryServer.page = 1
      this.listQuery.page = 1
      this.listQuery.qryStatusBar = this.activeName === 'first' ? 1 : 2
      this.listQueryServer.qryStatusBar = this.activeName === 'first' ? 1 : 2
      this.afterLeft()
    },
    // openTree(res) {
    //   console.log(res, 'res')
    //   // this.listLoading = true;
    //   this.serveid = res
    //   callservesure.GetDetails(res).then((res) => {
    //     if (res.code == 200) {
    //       this.dataForm = res.result;
    //       this.dialogFormView = true;
    //     }
    //     // this.listLoading = false;
    //   });
    // },
    // closeCustoner() {
    //   // this.getList();
    // },
    // openTree(res) {
    //   this.listLoading = true;
    //    this.serveid = res
    //   callservesure.GetDetails(res).then(res => {
    //     if (res.code == 200) {
    //       this.dataForm1 = res.result;
    //       // console.log(this.dataForm1, 'dateForm1')
    //       this.dialogFormView = true;
    //     }
    //     this.listLoading = false;
    //   });
    // },
    // treeClick(data) {
      // 左侧treetable点击事件
      // console.log(data);
      // this.currentModule = data
      // this.currentModule.parent = null
      // this.getList()
    // },
    onSubmit() {
      // console.log("submit!");
    },
    changeTable(result) {
      console.log(result);
    },
    rowClick(row) {
      this.multipleSelection = row;
      this.$refs.mainTable.clearSelection();
      this.$refs.mainTable.toggleRowSelection(row);
    },
    // handleSelectionChange(val) {
    //   this.multipleSelection = val;
    //   console.log(val);
    // },
    onBtnClicked: function (domId) {
      switch (domId) {
        case "btnAdd":
          this.handleCreate();
          break;
        case "btnDetail":
          this.open();
          break;
        case "btnPost":
          if (!this.$refs.treeForm.getCheckedKeys().length) {
            return this.$message.error('请先选择服务单')
          }
          this.changeOrder();
          break;
        case "editTable":
          this.dialogTable = true;
          break;
        case "btnReport":
          if (!this.$refs.treeForm.getCheckedKeys().length) { // 没有选中
            return this.$message.error("请选择服务单")
          }
          this.handleReport(this.$refs.treeForm.getCheckedKeys()[0], this.list) // 传入key中的第一个 服务单ID
          break
        case "btnEdit":
          this.$message({
            message: "抱歉，暂不提供编辑功能",
            type: "error",
          });
          // if (this.multipleSelection.length<1) {
          //   this.$message({
          //     message: "请点击需要编辑的数据",
          //     type: "error"
          //   });
          //   return;
          // }
          // this.handleUpdate(this.multipleSelection.serviceOrderId);
          break;
        case "btnDel":
          if (!this.multipleSelection) {
            this.$message({
              message: "至少删除一个",
              type: "error",
            });
            return;
          }
          this.handleDelete(this.multipleSelection);
          break;
        default:
          break;
      }
    },

    checkGroupNode(a) {
      //点击复选框触发
      console.log(this.ifParent, 'IFparent')
      if (this.ifParent) {
        if (this.ifParent == a.key) {
          //同一级，不做限制，添加编码请求
          if (!a.children) {
            //如果点击是子级
            if (this.listQuery.QryMaterialTypes.indexOf(a.id) == -1) {
              //找数组中是否存在类型号，有的话就说明是取消
              if (!this.listQuery.QryU_SAP_ID) {
                this.listQuery.QryU_SAP_ID = a.key;
              }
              this.listQuery.QryMaterialTypes.push(a.id);
            } else {
              this.listQuery.QryMaterialTypes = this.listQuery.QryMaterialTypes.filter(
                (item) => item != a.id
              );
              if (this.listQuery.QryMaterialTypes.length == 0) {
                this.listQuery.QryU_SAP_ID = "";
                console.log('cancel')
              }
            }
          } else {
            if (!this.listQuery.QryU_SAP_ID) {
              this.listQuery.QryU_SAP_ID = a.key;
            } else {
              this.ifParent = ""; //取消选择之后，清空父级选择

              this.listQuery.QryU_SAP_ID = "";
              this.listQuery.QryMaterialTypes = [];
            }
          }
          this.getRightList();
        } else {
          // console.log('清除之前,添加后面的')
          this.$refs.treeForm.setCheckedKeys([]);
          this.listQuery.QryMaterialTypes = [];
          this.listQuery.QryU_SAP_ID = a.key;
          this.ifParent = a.key;
          this.$refs.treeForm.setCheckedKeys([a.key1]);
          if (!a.children) {
            this.listQuery.QryMaterialTypes.push(a.id);
          } else {
            a.children.map((item) => {
              this.listQuery.QryMaterialTypes.push(item.id);
            });
          }
          this.getRightList();
        }
      } else {
        //第一次点击
        this.listQuery.QryMaterialTypes = [];
        this.listQuery.page = 1
        if (!a.children) {
          this.listQuery.QryMaterialTypes.push(a.id);
        } else {
          a.children.map((item) => {
            this.listQuery.QryMaterialTypes.push(item.id);
          });
        }
     
        this.$refs.treeForm.setCheckedKeys([a.key1]);
        this.ifParent = a.key;
        this.listQuery.QryU_SAP_ID = a.key;
        this.getRightList();
      }
    },

    getLeftList() {
      this.listLoading = true;
      return callservepushm.getLeftList(this.listQueryServer).then((res) => {
        let { data, count } = res.data
        let arr = this._normalizeTree(data)
        if (!this.modulesTree.length || this.isClear) {
          this.modulesTree = arr
          this.isClear = false
        } else {
          this.modulesTree = this.modulesTree.concat(arr)
        }
        this.totalCount = count
        console.log()
        // this.modulesTree = arr;
        this.listLoading = false;
      }).catch(() => {
        this.isClear = false
        this.listLoading = false
      })
    },
    _normalizeTree (data) {
      let arr = []
      for (let i = 0; i < data.length; i++) {
        arr[i] = [];
        arr[i].label = `服务号:${data[i].u_SAP_ID}`;
        arr[i].key1 = `${data[i].serviceOrderId}`;
        arr[i].key = `${data[i].u_SAP_ID}`;
        arr[i].children = [];
        // work
        
        data[i].materialTypes.map((item1, index) => {
          arr[i].children.push({
            label: `物料类型号:${data[i].workMaterialTypes[index]}`,
            // label: `物料类型号:${item1}`,
            key: `${data[i].u_SAP_ID}`,
            key1: `${data[i].serviceOrderId}&${item1}`,
            // label: 
            id: item1,
          });
        });
        // console.log(arr)
      }
      return arr 
    },
    getAllRight() {
      //展开全部工单
      // this.listLoading = true;
      // callservepushm.getRightList(this.listQuery1).then(response => {
      //   this.list = response.data.data;
      //   // this.list = response.data;
      //   this.total = response.data.count;
      //   this.listLoading = false;
      // });
      this.afterLeft();
    },
    getRightList() {
      this.listLoading = true;
      callservepushm
        .getRightList(this.listQuery)
        .then((response) => {
          if (response.code === 200) {
            this.list = response.data.data;
            this.total = response.data.count;
            this.listLoading = false;
          } else {
            this.$message({
              type: "error",
              message: `${response.message}`,
            });
          }
        })
        .catch(() => {
          this.listLoading = true;
          let that = this;
          setTimeout(function () {
            that.$message({
              type: "error",
              message: `请输入正确的搜索值`,
            });
            that.list = [];
            that.total = 0;
            that.listLoading = false;
          }, 700);
        });
    },
    open() {
      this.$confirm("确认已完成回访?", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          this.$message({
            type: "success",
            message: "操作成功!",
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消操作",
          });
        });
    },
    handleFilter() {
      this.listQuery.page = 1;
      this.getRightList();
    },
    handleSizeChange(val) {
      this.listQuery.limit = val;
      this.getRightList();
    },
    handleCurrentChange(val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      this.getRightList();
    },
    handleModifyStatus(row, disable) {
      // 模拟修改状态
      this.$message({
        message: "操作成功",
        type: "success",
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
        extendInfo: "",
      };
    },
    handleCreate() {
      // 弹出添加框
      this.resetTemp();
      this.dialogStatus = "create";
      this.dialogFormVisible = true;
      this.$nextTick(() => {
        this.$refs["dataForm"].clearValidate();
      });
    },
    createData() {
      // 保存提交
      this.$refs["dataForm"].validate((valid) => {
        if (valid) {
          solutions.add(this.temp).then(() => {
            this.list.unshift(this.temp);
            this.dialogFormVisible = false;
            this.$notify({
              title: "成功",
              message: "创建成功",
              type: "success",
              duration: 2000,
            });
          });
        }
      });
    },
    handleUpdate(row) {
      // 弹出编辑框
      this.temp = Object.assign({}, row); // copy obj
      this.dialogStatus = "update";
      this.dialogFormVisible = true;
      this.$nextTick(() => {
        this.$refs["dataForm"].clearValidate();
      });
    },
    updateData() {
      // 更新提交
      this.$refs["dataForm"].validate((valid) => {
        if (valid) {
          const tempData = Object.assign({}, this.temp);
          solutions.update(tempData).then(() => {
            for (const v of this.list) {
              if (v.id === this.temp.id) {
                const index = this.list.indexOf(v);
                this.list.splice(index, 1, this.temp);
                break;
              }
            }
            this.dialogFormVisible = false;
            this.$notify({
              title: "成功",
              message: "更新成功",
              type: "success",
              duration: 2000,
            });
          });
        }
      });
    },
    // onReportClosed () {
    //   this.reportData = []
    //   this.$refs.report.reset()
    // },
    // handleReport (serviceOrderId, data) {
    //   console.log(serviceOrderId, this.$store.state.user.name)
    //   console.log(this.list, 'list')
    //   let hasFinished = data.some(workOrder => {
    //     let { status, fromType } = workOrder
    //     return Number(status) === 7 && Number(fromType) !== 2 // 所有工单状态为已解决且呼叫类型不为在线解答
    //   })
    //   if (hasFinished) {
    //     callservesure.getReportDetail({
    //       serviceOrderId
    //     }).then(res => {
    //       console.log(res, 'reportData')
    //       this.reportData = res.result.data
    //       this.dialogReportVisible = true
    //     })
    //   } else {
    //     this.$message({
    //         type: 'warning',
    //         message: '工单未解决或在线解答方式无完工报告'
    //       })
    //   }
    // },
    handleDelete(rows) {
      // 多行删除
      solutions.del(rows.map((u) => u.id)).then(() => {
        this.$notify({
          title: "成功",
          message: "删除成功",
          type: "success",
          duration: 2000,
        });
        rows.forEach((row) => {
          const index = this.list.indexOf(row);
          this.list.splice(index, 1);
        });
      });
    },
  },
};
</script>
<style lang="scss" scoped>
.tabList {
  background-color: #fff;
  ::v-deep .el-tabs__header {
    margin-bottom: 0;
  }
}
.ls-border {
  // width: 300px;
  ::v-deep .el-tree-node > .el-tree-node__children {
    overflow: visible;
  }
  ::v-deep .el-card__body {
    display: flex;
    flex-direction: column;
    height: 100%;
    .el-tree {
      flex: 1;
    }
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
</style>
