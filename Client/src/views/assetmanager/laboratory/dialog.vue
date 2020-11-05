<template>
  <div v-if="dialogVisible">
    <el-dialog append-to-body :visible.sync="dialogVisible" width="900px">
      <el-tabs v-model="activeTab" type="card">
        <el-tab-pane label="详情" name="详情">
          <el-form :model="formData" :rules="rules" ref="form" position="left" label-width="110px" size="mini">
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="资产ID">
                  <el-input disabled style="width: 200px" v-model="formData.id"></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="状态" prop="assetStatus">
                  <el-input v-if="openDialogType === '查看'" style="width: 200px" :value="formData.assetStatus" readonly></el-input>
                  <el-select v-else style="width: 200px" v-model="formData.assetStatus" placeholder="请选择">
                    <el-option v-for="item in assetStatus" :key="item.name" :value="item.name"></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="类别" prop="assetCategory">
                  <el-input v-if="openDialogType === '查看'" style="width: 200px" :value="formData.assetCategory" readonly></el-input>
                  <el-select v-else style="width: 200px" :disabled="openDialogType === '编辑'" v-model="formData.assetCategory" placeholder="请选择" @change="onTypeChange">
                    <el-option v-for="item in assetCategory" :key="item.name" :value="item.name"></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="部门" prop="orgName">
                  <el-input v-if="openDialogType === '查看'" style="width: 200px" :value="formData.orgName" readonly></el-input>
                  <el-autocomplete
                    v-else
                    style="width: 200px"
                    v-model="formData.orgName"
                    :fetch-suggestions="querySearchOrg"
                    placeholder="请输入部门"
                    @select="handleSelect"
                  ></el-autocomplete>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="型号" prop="assetType">
                  <el-input
                    style="width: 200px"
                    placeholder="请输入内容"
                    :readonly="openDialogType === '查看'"
                    :disabled="openDialogType === '编辑'"
                    v-model="formData.assetType"
                  ></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="持有者" prop="assetHolder">
                  <el-input v-if="openDialogType === '查看'" style="width: 200px" :value="formData.assetHolder" readonly></el-input>
                  <el-autocomplete
                    v-else
                    style="width: 200px"
                    v-model="formData.assetHolder"
                    :fetch-suggestions="queryUser"
                    placeholder="请输入持有者"
                    @select="handleSelectUser"
                  ></el-autocomplete>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="出厂编号S/N" prop="assetStockNumber">
                  <el-input
                    style="width: 200px"
                    placeholder="请输入内容"
                    :readonly="openDialogType === '查看'"
                    :disabled="openDialogType === '编辑'"
                    v-model="formData.assetStockNumber"
                  ></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="管理员" prop="assetAdmin">
                  <el-input style="width: 200px" placeholder="请输入内容" :readonly="openDialogType === '查看'" v-model="formData.assetAdmin"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="资产编号">
                  <el-input style="width: 200px" :readonly="openDialogType === '查看'" v-model="formData.assetNumber" disabled></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="制造厂" prop="assetFactory">
                  <el-input
                    style="width: 200px"
                    placeholder="请输入内容"
                    :readonly="openDialogType === '查看'"
                    :disabled="openDialogType === '编辑'"
                    v-model="formData.assetFactory"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="送检类型" prop="assetInspectType">
                  <el-input v-if="openDialogType === '查看'" style="width: 200px" :value="formData.assetInspectType" readonly></el-input>
                  <el-select v-else clearable style="width: 200px" v-model="formData.assetInspectType" placeholder="请选择" @change="changeSJWay">
                    <el-option v-for="item in assetSJType" :key="item.name" :value="item.name"></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="送检方式" prop="assetInspectWay" placeholder="请选择">
                  <el-input v-if="openDialogType === '查看'" style="width: 200px" :value="formData.assetInspectWay" readonly></el-input>
                  <el-select v-else clearable style="width: 200px" v-model="formData.assetInspectWay">
                    <el-option v-for="item in this.sjTypes" :value="item" :label="item" :key="item"></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="校准日期" prop="assetStartDate">
                  <el-date-picker
                    style="width: 200px"
                    :readonly="openDialogType === '查看'"
                    v-model="formData.assetStartDate"
                    type="date"
                    @change="getsxdate"
                    placeholder="选择日期"
                    value-format="yyyy-MM-dd"
                  >
                  </el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="校准证书" prop="assetCalibrationCertificate">
                  <el-upload
                    name="files"
                    :action="action"
                    :headers="headers"
                    :limit="1"
                    :disabled="openDialogType === '查看'"
                    :on-success="handleSuccessJZ"
                    :on-remove="handleRemoveJZ"
                  >
                    <el-button type="primary" size="mini" :disabled="openDialogType === '查看'">添加校准证书</el-button>
                  </el-upload>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="12">
                <el-form-item label="失效日期" prop="assetEndDate">
                  <el-date-picker
                    style="width: 200px"
                    :readonly="openDialogType === '查看'"
                    v-model="formData.assetEndDate"
                    type="date"
                    placeholder="选择日期"
                    value-format="yyyy-MM-dd"
                  ></el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="技术指标" prop="assetInspectDataOne">
                  <el-upload
                    name="files"
                    :action="action"
                    :headers="headers"
                    :limit="1"
                    :disabled="openDialogType === '编辑' || openDialogType === '查看'"
                    :on-success="handleSuccessAssetInspectDataOne"
                    :on-remove="handleRemoveAssetInspectDataOne"
                  >
                    <el-button type="primary" size="mini" :disabled="openDialogType === '编辑' || openDialogType === '查看'">添加技术指标</el-button>
                  </el-upload>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row>
              <el-col :span="12">
                <el-form-item label="校准数据" prop="assetInspectDataTwo">
                  <el-upload
                    name="files"
                    :action="action"
                    :headers="headers"
                    :limit="1"
                    :disabled="openDialogType === '查看'"
                    :on-success="handleSuccessAssetInspectDataTwo"
                    :on-remove="handleRemoveAssetInspectDataTwo"
                  >
                    <el-button type="primary" size="mini" :disabled="openDialogType === '查看'">添加校准数据</el-button>
                  </el-upload>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="技术文件" prop="assetTCF">
                  <el-upload
                    name="files"
                    :action="action"
                    :headers="headers"
                    :limit="1"
                    :disabled="openDialogType === '编辑' || openDialogType === '查看'"
                    :on-success="handleSuccessAssetTCF"
                    :on-remove="handleRemoveAssetTCF"
                  >
                    <el-button type="primary" size="mini" :disabled="openDialogType === '编辑' || openDialogType === '查看'">添加技术文件</el-button>
                  </el-upload>
                </el-form-item>
              </el-col>
            </el-row>
            <!--万用表 -->
            <el-table
              v-if="isshowmeter || formData.assetCategory === '万用表'"
              border
              :data="formData.assetCategorys"
              :header-cell-style="headerCellStyle"
              @row-click="getcurrentRow"
            >
              <el-table-column label="#" prop="categoryNumber" autosize>
                <template slot-scope="scope">
                  <el-input size="mini" v-model="scope.row.categoryNumber" readonly></el-input>
                </template>
              </el-table-column>
              <el-table-column label="测量值" prop="categoryOhms" autosize>
                <template slot-scope>
                  <el-input size="mini" value="-" readonly />
                </template>
              </el-table-column>
              <el-table-column label="不确定度(Unc)" prop="categoryNondeterminacy" autosize>
                <template slot-scope="scope">
                  <el-input size="mini" v-model="scope.row.categoryNondeterminacy" :readonly="openDialogType === '查看'">
                    <template slot="append">{{ scope.row.unit }}</template>
                  </el-input>
                </template>
              </el-table-column>
              <el-table-column label="类型" prop="categoryType" autosize>
                <template slot-scope="scope">
                  <el-input v-if="openDialogType === '查看'" size="mini" :value="scope.row.categoryType" readonly></el-input>
                  <el-select v-else size="mini" v-model="scope.row.categoryType" @change="formatCategoryType(scope.$index)">
                    <el-option v-for="item in scope.row.categoryTypeList" :label="item" :value="item" :key="item"></el-option>
                  </el-select>
                </template>
              </el-table-column>
              <el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
                <template slot-scope="scope">
                  <el-input class="input" size="mini" v-model="scope.row.categoryBHYZ" :readonly="openDialogType === '查看'" />
                </template>
              </el-table-column>
            </el-table>
            <!--工装-->
            <el-table
              v-else-if="isshowgz || formData.assetCategory === '工装'"
              border
              :data="formData.assetCategorys"
              :header-cell-style="headerCellStyle"
              @row-click="getcurrentRow"
            >
              <el-table-column label="序号" prop="categoryNumber" autosize>
                <template slot-scope="scope">
                  <el-input size="small" v-model="scope.row.categoryNumber" readonly></el-input>
                </template>
              </el-table-column>
              <el-table-column label="阻值" prop="categoryOhms" autosize>
                <template slot-scope="scope">
                  <el-input size="small" v-model="scope.row.categoryOhms" :readonly="openDialogType === '查看'">
                    <template slot="append">Ω</template>
                  </el-input>
                </template>
              </el-table-column>
              <el-table-column label="不确定度(Unc)" prop="categoryNondeterminacy" autosize>
                <template slot-scope="scope">
                  <el-input size="small" v-model="scope.row.categoryNondeterminacy" :readonly="openDialogType === '查看'">
                    <template slot="append">{{ scope.row.unit }}</template>
                  </el-input>
                </template>
              </el-table-column>
              <el-table-column label="类型" prop="categoryType" autosize>
                <template slot-scope="scope">
                  <el-input v-if="openDialogType === '查看'" size="mini" :value="scope.row.categoryType" readonly></el-input>
                  <el-select v-else size="mini" v-model="scope.row.categoryType" @change="formatCategoryType(scope.$index)">
                    <el-option v-for="item in commonArr" :label="item.name" :value="item.value" :key="item.value"></el-option>
                  </el-select>
                </template>
              </el-table-column>
              <el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
                <template slot-scope="scope">
                  <el-input class="input" size="small" v-model="scope.row.categoryBHYZ" :readonly="openDialogType === '查看'" />
                </template>
              </el-table-column>
            </el-table>

            <!--分流器-->
            <el-table
              v-else-if="isshowflq || formData.assetCategory === '分流器'"
              border
              :data="formData.assetCategorys"
              :header-cell-style="headerCellStyle"
              @row-click="getcurrentRow"
            >
              <el-table-column label="序号" prop="categoryNumber" autosize>
                <template slot-scope="scope">
                  <el-input size="mini" v-model="scope.row.categoryNumber" readonly></el-input>
                </template>
              </el-table-column>
              <el-table-column label="阻值" prop="categoryOhms" autosize>
                <template slot-scope="scope">
                  <el-input size="mini" v-model="scope.row.categoryOhms" :readonly="openDialogType === '查看'">
                    <template slot="append">Ω</template>
                  </el-input>
                </template>
              </el-table-column>
              <el-table-column label="不确定度(Unc)" prop="categoryNondeterminacy" autosize>
                <template slot-scope="scope">
                  <el-input size="mini" v-model="scope.row.categoryNondeterminacy" :readonly="openDialogType === '查看'">
                    <template slot="append">{{ scope.row.unit }}</template>
                  </el-input>
                </template>
              </el-table-column>
              <el-table-column label="类型" prop="categoryType" autosize>
                <template slot-scope="scope">
                  <el-input v-if="openDialogType === '查看'" size="mini" :value="scope.row.categoryType" readonly></el-input>
                  <el-select v-else size="mini" v-model="scope.row.categoryType" @change="formatCategoryType(scope.$index)">
                    <el-option v-for="item in commonArr" :label="item.name" :value="item.value" :key="item.value"></el-option>
                  </el-select>
                </template>
              </el-table-column>
              <el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
                <template slot-scope="scope">
                  <el-input class="input" size="mini" v-model="scope.row.categoryBHYZ" :readonly="openDialogType === '查看'" />
                </template>
              </el-table-column>
            </el-table>
            <el-row style="margin-top:20px">
              <el-form-item label="描述">
                <el-input type="textarea" :readonly="openDialogType === '查看'" v-model="formData.assetDescribe" :autosize="{ minRows: 4, maxRows: 4 }"></el-input>
              </el-form-item>
            </el-row>
            <el-row>
              <el-form-item label="备注">
                <el-input type="textarea" :readonly="openDialogType === '查看'" v-model="formData.assetRemarks" :autosize="{ minRows: 4, maxRows: 4 }"></el-input>
              </el-form-item>
            </el-row>
            <el-row>
              <el-form-item label="图片">
                <el-row type="flex">
                  <el-image
                    class="asset-image"
                    v-if="formData.assetImage"
                    :src="getImgUrl(formData.assetImage)"
                    :preview-src-list="[getImgUrl(formData.assetImage)]"
                    alt
                  ></el-image>
                  <el-upload
                    name="files"
                    list-type="picture-card"
                    accept="image/*"
                    :action="action"
                    :headers="headers"
                    :limit="1"
                    :show-file-list="false"
                    :disabled="openDialogType === '编辑' || openDialogType === '查看'"
                    :on-success="handleSuccessAssetImage"
                    :on-remove="handleRemoveAssetImage"
                    :on-preview="handlePictureCardPreviewAssetImage"
                  >
                    <i class="el-icon-plus"></i>
                  </el-upload>
                </el-row>
              </el-form-item>
            </el-row>
          </el-form>
          <el-row type="flex" justify="end" v-show="openDialogType != '查看'">
            <el-button size="mini" @click="closeDialog">取消</el-button>
            <el-button size="mini" type="primary" @click="onsubmit">确认</el-button>
          </el-row>
        </el-tab-pane>
        <el-tab-pane label="送检记录" name="送检记录" v-if="openDialogType != '新增'">
          <Inspection ref="inspection" :list="formData.assetInspects" />
        </el-tab-pane>
        <el-tab-pane label="操作记录" name="操作记录" v-if="openDialogType != '新增'">
          <Operation :list="formData.assetOperations" @timeline-click="timelineClick" />
        </el-tab-pane>
      </el-tabs>
    </el-dialog>
  </div>
</template>
<script>
import { getListCategoryName, getListOrg, getListUser, add, update, getSingleAsset } from '@/api/assetmanagement'
import Inspection from './inspection'
import Operation from './operation'

const SYS_AssetCategory = 'SYS_AssetCategory' //类别
const SYS_AssetStatus = 'SYS_AssetStatus' // 状态
const SYS_AssetSJType = 'SYS_AssetSJType' // 送检类型
const SYS_AssetSJWay = 'SYS_AssetSJWay' // 送检方式
const SYS_CategoryNondeterminacy = 'SYS_CategoryNondeterminacy' // 阻值类型

export default {
  components: {
    Inspection,
    Operation
  },
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,

      assetCategory: [], // 类别
      assetStatus: [], // 状态
      assetSJType: [], // 送检类型
      assetSJWay: [], // 送检方式
      nondeterminacy: [], // 阻值类型

      commonArr: [
        {
          name: '绝对不确定度(Ω)',
          value: '绝对不确定度(Ω)'
        },
        {
          name: '相对不确定度(ppm)',
          value: '相对不确定度(ppm)'
        },
        {
          name: '相对不确定度(%)',
          value: '相对不确定度(%)'
        }
      ],
      commonArr2: [
        {
          name: '绝对不确定度(Ω)',
          value: '绝对不确定度(Ω)'
        },
        {
          name: '相对不确定度(ppm)',
          value: '相对不确定度(ppm)'
        },
        {
          name: '相对不确定度(%)',
          value: '相对不确定度(%)'
        }
      ],

      headerCellStyle: {
        color: '#000',
        'font-size': '14px',
        'line-height': '28px'
      },
      config: {
        name: ''
      },
      userconfig: {
        name: '',
        Orgid: ''
      },
      formData: {
        id: '', // 资产ID
        assetStatus: '', // 状态
        assetSerial: '', //类别值
        assetCategory: '', // 类别
        orgName: '', // 部门
        assetType: '', // 型号
        assetHolder: '', // 持有者
        assetStockNumber: '', // 出场编号S/N
        assetAdmin: '', // 管理员
        assetNumber: '', // 资产编号
        assetFactory: '', // 制造厂
        assetInspectType: '', // 送检类型
        assetInspectWay: '', // 送检方式
        assetStartDate: '', // 校准日期
        assetCalibrationCertificate: '', // 校准证书
        assetEndDate: '', // 失效日期
        assetInspectDataOne: '', // 技术指标
        assetInspectDataTwo: '', // 校准数据
        assetTCF: '', // 技术文件
        assetDescribe: '', // 描述
        assetRemarks: '', // 备注
        assetImage: '', // 上传图片
        assetCategorys: [] //万用表、工装、分流器数据
      },
      rules: {
        assetStatus: [
          {
            required: true,
            message: '请选择状态',
            trigger: 'change'
          }
        ],
        assetCategory: [
          {
            required: true,
            message: '请选择类别',
            trigger: 'change'
          }
        ],
        orgName: [
          {
            required: true,
            message: '请输入部门',
            trigger: 'change'
          }
        ],
        assetType: [
          {
            required: true,
            message: '请输入型号',
            trigger: 'blur'
          }
        ],
        assetHolder: [
          {
            required: true,
            message: '请输入持有者',
            trigger: 'change'
          }
        ],
        assetStockNumber: [
          {
            required: true,
            message: '请输入出厂编号',
            trigger: 'blur'
          }
        ],
        assetAdmin: [
          {
            required: true,
            message: '请输入管理员',
            trigger: 'blur'
          }
        ],
        assetFactory: [
          {
            required: true,
            message: '请输入制造厂',
            trigger: 'blur'
          }
        ],
        assetInspectType: [
          {
            required: true,
            message: '请选择送检类型',
            trigger: 'change'
          }
        ],
        assetInspectWay: [
          {
            required: true,
            message: '请选择送检方式',
            trigger: 'change'
          }
        ],
        assetStartDate: [
          {
            required: true,
            message: '请选择校准日期',
            trigger: 'change'
          }
        ],
        assetCalibrationCertificate: [
          {
            required: true,
            message: '请上传校准证书',
            trigger: 'change'
          }
        ],
        assetEndDate: [
          {
            required: true,
            message: '请选择失效日期',
            trigger: 'change'
          }
        ],
        assetInspectDataOne: [
          {
            required: true,
            message: '请上传技术指标',
            trigger: 'change'
          }
        ],
        assetInspectDataTwo: [
          {
            required: true,
            message: '请上传校准数据',
            trigger: 'change'
          }
        ],
        assetTCF: [
          {
            required: true,
            message: '请上传技术文件',
            trigger: 'change'
          }
        ]
      },
      currentIndex: '',
      orgs: [], //部门列表
      sjTypes: [], //送检类型列表
      users: [], //持有人列表
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`, // 文件上传地址
      headers: {
        // 上传标识
        'X-Token': this.$store.state.user.token
      },

      isshowmeter: false, //是否显示万用表
      isshowgz: false, //是否显示工装
      isshowflq: false, //是否显示分流器

      dialogVisible: false,
      openDialogType: '',
      activeTab: '详情',

      imgDialogVisible: false,
      imgDialogImageUrl: ''
    }
  },
  created() {
    this._getListCategoryName()
  },
  methods: {
    async open(id, type) {
      if (type === '新增') {
        this.formData = {
          id: '', // 资产ID
          assetStatus: '', // 状态
          assetSerial: '', //类别值
          assetCategory: '', // 类别
          orgName: '', // 部门
          assetType: '', // 型号
          assetHolder: '', // 持有者
          assetStockNumber: '', // 出场编号S/N
          assetAdmin: '', // 管理员
          assetNumber: '', // 资产编号
          assetFactory: '', // 制造厂
          assetInspectType: '', // 送检类型
          assetInspectWay: '', // 送检方式
          assetStartDate: '', // 校准日期
          assetCalibrationCertificate: '', // 校准证书
          assetEndDate: '', // 失效日期
          assetInspectDataOne: '', // 技术指标
          assetInspectDataTwo: '', // 校准数据
          assetTCF: '', // 技术文件
          assetDescribe: '', // 描述
          assetRemarks: '', // 备注
          assetImage: '', // 上传图片
          assetCategorys: [] //万用表、工装、分流器数据
        }
        this.isshowmeter = false
        this.isshowgz = false
        this.isshowflq = false
      } else {
        this._getSingleAsset(id)
      }
      this.openDialogType = type
      this.dialogVisible = true
      this.activeTab = '详情'
    },
    async _getSingleAsset(id) {
      const res = await getSingleAsset({
        id
      })
      const data = res.data
      data.assetOperations = data.assetOperations.map(item => {
        item.operationContent = item.operationContent.split('\\r\\n').join('<br>')
        return item
      })
      const units = ['V', '%', 'A', 'ppm', 'Ω']
      data.assetCategorys = data.assetCategorys.map(item => {
        units.forEach(i => {
          if (item.categoryType.indexOf(i) !== -1) {
            item.unit = i
            return
          }
        })
        return item
      })
      if (data.assetCategory === '万用表') {
        var arrs = [
          ['相对不确定度(%)', '绝对不确定度(V)'],
          ['相对不确定度(%)', '绝对不确定度(V)'],
          ['相对不确定度(%)', '绝对不确定度(A)'],
          ['相对不确定度(%)', '绝对不确定度(A)'],
          ['相对不确定度(%)', '绝对不确定度(Ω)']
        ]
        for (let i = 0; i < 5; i++) {
          data.assetCategorys[i].categoryTypeList = arrs[i]
        }
      }
      this.formData = data
    },
    async updateAssets() {
      try {
        const formData = this.formData
        const res = await update(formData)
        this.$message.success(res.message)
        this.close()
      } catch (error) {
        this.$message.error(error.message)
      }
    },
    _getListCategoryName() {
      getListCategoryName().then(res => {
        const data = res.data
        this.assetCategory = data.filter(i => i.typeId === SYS_AssetCategory)
        this.assetStatus = data.filter(i => i.typeId === SYS_AssetStatus)
        this.assetSJType = data.filter(i => i.typeId === SYS_AssetSJType)
        this.assetSJWay = data.filter(i => i.typeId === SYS_AssetSJWay)
        this.nondeterminacy = data.filter(i => i.typeId === SYS_CategoryNondeterminacy)
      })
    },
    //获取失效日期
    getsxdate(dateText) {
      if (dateText) {
        const date = new Date(dateText)
        date.setFullYear(date.getFullYear() + 1)
        date.setDate(date.getDate() - 1)
        this.formData.assetEndDate = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date
          .getDate()
          .toString()
          .padStart(2, '0')}`
      }
    },
    changeSJWay() {
      this.formData.assetInspectWay = ''
      this.sjTypes = this.getassetSJWay(this.formData.assetInspectType)
    },
    getassetSJWay(type) {
      var data = this.assetSJWay
      var arr = []
      for (let item of data) {
        if (type == '校准') {
          if (item.name == '内校' || item.name == '外校') {
            arr.push(item.name)
          }
        } else if (type == '检定') {
          if (item.name == '外检') {
            arr.push(item.name)
          }
        }
      }
      return arr || []
    },
    getOrg() {
      return getListOrg(this.config).then(res => {
        for (let item of res.data) {
          this.orgs.push({
            value: item.name,
            id: item.id
          })
        }
      })
    },
    async querySearchOrg(queryString, cb) {
      this.config.name = queryString
      await this.getOrg()
      var result = queryString ? this.orgs.filter(this.createStateFilter(queryString)) : this.orgs
      this.orgs = []
      cb(result)
    },
    createStateFilter(queryString) {
      return state => {
        return state.value.toLowerCase().indexOf(queryString.toLowerCase()) === 0
      }
    },
    onsubmit() {
      this.$refs.form.validate(valid => {
        if (valid) {
          if (this.openDialogType === '新增') {
            this.addzc(this.formData)
          } else {
            this.updateAssets()
          }
        }
      })
    },
    closeDialog() {
      this.dialogVisible = false
    },
    close() {
      this.closeDialog()
      this.$emit('close')
    },
    handleSelect(val) {
      this.userconfig.Orgid = val.id
    },
    handleSelectUser(val) {
      this.userId = val.id
    },
    getuser() {
      return getListUser(this.userconfig).then(res => {
        for (let item of res.data) {
          this.users.push({
            value: item.name,
            id: item.id
          })
        }
      })
    },
    async queryUser(queryString, cb) {
      this.userconfig.name = queryString
      await this.getuser()
      var result = queryString ? this.users.filter(this.createStateFilter(queryString)) : this.users
      this.users = []
      cb(result)
    },
    //添加资产信息
    async addzc(param) {
      try {
        const res = await add(param)
        this.$message.success(res.message)
        this.close()
      } catch (error) {
        this.$message.error(error.message)
      }
    },
    handleSuccessJZ(res) {
      this.formData.assetCalibrationCertificate = res.result[0].id
    },
    handleRemoveJZ() {
      this.formData.assetCalibrationCertificate = ''
    },
    handleSuccessAssetInspectDataOne(res) {
      this.formData.assetInspectDataOne = res.result[0].id
    },
    handleRemoveAssetInspectDataOne() {
      this.formData.assetInspectDataOne = ''
    },
    handleSuccessAssetInspectDataTwo(res) {
      this.formData.assetInspectDataTwo = res.result[0].id
    },
    handleRemoveAssetInspectDataTwo() {
      this.formData.assetInspectDataTwo = ''
    },
    handleSuccessAssetTCF(res) {
      this.formData.assetTCF = res.result[0].id
    },
    handleRemoveAssetTCF() {
      this.formData.assetTCF = ''
    },
    handleSuccessAssetImage(res) {
      this.formData.assetImage = res.result[0].id
    },
    handleRemoveAssetImage() {
      this.formData.assetImage = ''
    },
    handlePictureCardPreviewAssetImage(file) {
      this.imgDialogImageUrl = file.url
      this.imgDialogVisible = true
    },
    getcurrentRow(row) {
      this.currentIndex = this.formData.assetCategorys.findIndex(item => row == item)
    },
    formatCategoryType(index) {
      const currentItem = this.formData.assetCategorys[index]
      const str = currentItem.categoryType
      const units = ['V', '%', 'A', 'ppm', 'Ω']
      units.forEach(i => {
        if (str.indexOf(i) !== -1) {
          currentItem.unit = i
          return
        }
      })
    },
    onTypeChange(val) {
      switch (val) {
        case '万用表':
          this.isshowmeter = true
          this.isshowgz = false
          this.isshowflq = false
          var nums = ['DCV', 'ACV', 'DCI', 'ACI', 'R']
          var arrs = [
            ['相对不确定度(%)', '绝对不确定度(V)'],
            ['相对不确定度(%)', '绝对不确定度(V)'],
            ['相对不确定度(%)', '绝对不确定度(A)'],
            ['相对不确定度(%)', '绝对不确定度(A)'],
            ['相对不确定度(%)', '绝对不确定度(Ω)']
          ]
          this.formData.assetCategorys = []
          this.formData.assetSerial = '1'
          this.formData.assetCategorys = []
          for (let i = 0; i < 5; i++) {
            this.formData.assetCategorys.push({
              id: '', //类别ID
              assetId: '', //资产ID
              categoryNumber: nums[i], //序号
              categoryOhms: '', //阻值
              categoryNondeterminacy: '', //不确定度
              unit: '%',
              categoryType: '相对不确定度(%)', //不确定类型
              categoryTypeList: arrs[i], //不确定类型数组
              categoryBHYZ: '', //包含因子
              categoryAort: '' //排序
            })
          }
          break
        case '工装':
          this.isshowmeter = false
          this.isshowgz = true
          this.isshowflq = false
          var arr = ['R1', 'R2', 'R3', 'R4']
          this.formData.assetCategorys = []
          this.formData.assetSerial = '2'
          for (let i = 0; i < 4; i++) {
            this.formData.assetCategorys.push({
              id: '', //类别ID
              assetId: '', //资产ID
              categoryNumber: arr[i], //序号
              categoryOhms: '', //阻值
              categoryNondeterminacy: '', //不确定度
              unit: '%',
              categoryType: '相对不确定度(%)', //不确定类型
              categoryBHYZ: '', //包含因子
              categoryAort: '' //排序
            })
          }
          break
        case '分流器':
          this.isshowmeter = false
          this.isshowgz = false
          this.isshowflq = true
          this.formData.assetCategorys = []
          this.formData.assetSerial = '3'
          this.formData.assetCategorys.push({
            id: '', //类别ID
            assetId: '', //资产ID
            categoryNumber: 'R', //序号
            categoryOhms: '', //阻值
            categoryNondeterminacy: '', //不确定度
            unit: '%',
            categoryType: '相对不确定度(%)', //不确定类型
            categoryBHYZ: '', //包含因子
            categoryAort: '' //排序
          })
          break
        case '标准源':
          this.isshowmeter = false
          this.isshowgz = false
          this.isshowflq = false
          this.formData.assetSerial = '4'
          this.formData.assetCategorys = []
          break
        default:
          this.isshowmeter = false
          this.isshowgz = false
          this.isshowflq = false
          break
      }
    },
    timelineClick(inspectId) {
      this.activeTab = '送检记录'
      this.$refs.inspection.highlightCell(inspectId)
    },
    getImgUrl(id) {
      if (!id) return ''
      return `${this.baseURL}/files/Download/${id}?X-Token=${this.tokenValue}`
    }
  }
}
</script>
<style lang="scss" scoped>
.avatar-uploader {
  position: relative;
  width: 80px;
  height: 40px;

  ::v-deep .el-upload {
    position: relative;
    width: 100%;
    height: 100%;
  }

  ::v-deep .el-icon-plus,
  .avatar-uploader-icon {
    position: absolute;
    left: 0;
    bottom: 0;
    right: 0;
    top: 0;
    width: 15px;
    height: 15px;
    margin: auto;
  }

  .add-wrapper {
    display: flex;
    width: 100%;
    height: 100%;
    align-items: center;
    justify-content: center;
  }

  .upload-img {
    position: relative;
    width: 80px;
    height: 40px;

    &:hover .mask-wrapper {
      opacity: 1;
    }

    .mask-wrapper {
      position: absolute;
      display: flex;
      left: 0;
      right: 0;
      bottom: 0;
      top: 0;
      align-items: center;
      justify-content: center;
      background-color: rgba(0, 0, 0, 0.5);
      opacity: 0;
      transition: opacity 0.3s;

      .item {
        margin: 0 5px;
        color: white;
      }
    }
  }
}

body {
  ::v-deep .popper__arrow {
    display: none;
  }

  ::v-deep .el-select-dropdown {
    margin-top: 0 !important;
  }
}
.input {
  margin: 5px 0;
}
.asset-image {
  display: inline-block;
  width: 148px;
  height: 148px;
  border-radius: 6px;
  margin-right: 10px;
}
</style>
