<template>
  <div>
    <el-form
      :model="formData"
      :rules="rules"
      ref="formData"
      :disabled="disable"
      position="left"
      label-width="110px"
      size="mini"
    >
      <el-row type="flex" justify="space-around">
        <el-col :span="7">
          <el-form-item label="资产ID">
            <el-input
              disabled
              style="width: 200px"
              v-model="formData.id"
            ></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item label="状态" prop="assetStatus">
            <el-select
              style="width: 200px"
              v-model="formData.assetStatus"
              placeholder="请选择"
            >
              <el-option
                v-for="item in assetStatus"
                :value="item.value"
                :label="item.label"
                :key="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="7">
          <el-form-item label="类别" prop="assetCategory">
            <el-select
              style="width: 200px"
              v-model="formData.assetCategory"
              placeholder="请选择"
              @change="onTypeChange"
            >
              <el-option
                v-for="item in assetCategory"
                :value="item.value"
                :label="item.label"
                :key="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item label="部门" prop="orgName">
            <el-autocomplete
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
        <el-col :span="7">
          <el-form-item label="型号" prop="assetType">
            <el-input
              style="width: 200px"
              placeholder="请输入内容"
              v-model="formData.assetType"
            ></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item label="持有者" prop="assetHolder">
            <el-autocomplete
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
        <el-col :span="7">
          <el-form-item label="出厂编号S/N" prop="assetStockNumber">
            <el-input
              style="width: 200px"
              placeholder="请输入内容"
              v-model="formData.assetStockNumber"
            ></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item label="管理员" prop="assetAdmin">
            <el-input
              style="width: 200px"
              placeholder="请输入内容"
              v-model="formData.assetAdmin"
            ></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="7">
          <el-form-item label="资产编号">
            <el-input
              style="width: 200px"
              v-model="formData.assetNumber"
              disabled
            ></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item label="制造厂" prop="assetFactory">
            <el-input
              style="width: 200px"
              placeholder="请输入内容"
              v-model="formData.assetFactory"
            ></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="7">
          <el-form-item label="送检类型" prop="assetInspectType">
            <el-select
              style="width: 200px"
              v-model="formData.assetInspectType"
              placeholder="请选择"
              @change="changeSJWay"
            >
              <el-option
                v-for="item in assetSJType"
                :value="item.value"
                :label="item.label"
                :key="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item
            label="送检方式"
            prop="assetInspectWay"
            placeholder="请选择"
          >
            <el-select style="width: 200px" v-model="formData.assetInspectWay">
              <el-option
                v-for="item in this.sjTypes"
                :value="item"
                :label="item"
                :key="item"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="7">
          <el-form-item label="校准日期" prop="assetStartDate">
            <el-date-picker
              style="width: 200px"
              v-model="formData.assetStartDate"
              type="date"
              @change="getsxdate"
              placeholder="选择日期"
              value-format="yyyy-MM-dd"
            >
            </el-date-picker>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item label="校准证书">
            <el-upload
              ref="assetCalibrationCertificate"
              name="files"
              class="avatar-uploader"
              :action="action"
              :headers="headers"
              :show-file-list="false"
              :on-success="handleSuccessJZ"
              :before-upload="beforeAvatarUpload"
            >
              <div
                v-if="formData.assetCalibrationCertificate"
                class="upload-img"
                @click.stop
              >
                <img
                  v-if="formData.assetCalibrationCertificate"
                  :src="imglist.showzsimg"
                  class="upload-img"
                />
                <div class="mask-wrapper">
                  <i
                    class="el-icon-zoom-in item"
                    @click="previewImg('assetCalibrationCertificate')"
                  ></i>
                  <i
                    class="el-icon-delete item"
                    @click="removeImg('assetCalibrationCertificate')"
                  ></i>
                </div>
              </div>
              <div v-else class="add-wrapper">
                <el-button type="primary" size="mini">添加文件</el-button>
              </div>
            </el-upload>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="7">
          <el-form-item label="失效日期" prop="assetEndDate">
            <el-date-picker
              style="width: 200px"
              v-model="formData.assetEndDate"
              type="date"
              placeholder="选择日期"
              value-format="yyyy-MM-dd"
            >
            </el-date-picker>
          </el-form-item>
        </el-col>
        <el-col :span="7">
          <el-form-item label="校准数据">
            <el-row type="flex" justify="space-around">
              <el-col :span="5">
                <el-upload
                  ref="assetInspectDataOne"
                  name="files"
                  class="avatar-uploader"
                  :action="action"
                  :headers="headers"
                  :show-file-list="false"
                  :on-success="handleSuccessData1"
                  :before-upload="beforeAvatarUpload"
                >
                  <div
                    v-if="formData.assetInspectDataOne"
                    class="upload-img"
                    @click.stop
                  >
                    <img
                      v-if="formData.assetInspectDataOne"
                      :src="imglist.showzbimg"
                      class="upload-img"
                    />
                    <div class="mask-wrapper">
                      <i
                        class="el-icon-zoom-in item"
                        @click="previewImg('assetInspectDataOne')"
                      ></i>
                      <i
                        class="el-icon-delete item"
                        @click="removeImg('assetInspectDataOne')"
                      ></i>
                    </div>
                  </div>
                  <div v-else class="add-wrapper">
                    <el-button type="primary" size="mini"
                      >添加技术指标</el-button
                    >
                  </div>
                </el-upload>
              </el-col>
              <el-col :span="4">
                <el-upload
                  ref="assetInspectDataTwo"
                  name="files"
                  class="avatar-uploader"
                  :action="action"
                  :headers="headers"
                  :show-file-list="false"
                  :on-success="handleSuccessData2"
                  :before-upload="beforeAvatarUpload"
                >
                  <div
                    v-if="formData.assetInspectDataTwo"
                    class="upload-img"
                    @click.stop
                  >
                    <img
                      v-if="formData.assetInspectDataTwo"
                      :src="imglist.showsjimg"
                      class="upload-img"
                    />
                    <div class="mask-wrapper">
                      <i
                        class="el-icon-zoom-in item"
                        @click="previewImg('assetInspectDataTwo')"
                      ></i>
                      <i
                        class="el-icon-delete item"
                        @click="removeImg('assetInspectDataTwo')"
                      ></i>
                    </div>
                  </div>
                  <div v-else class="add-wrapper">
                    <el-button type="primary" size="mini"
                      >添加校准数据</el-button
                    >
                  </div>
                </el-upload>
              </el-col>
            </el-row>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row style="left: 120px;">
        <el-form-item label="技术文件">
          <el-upload
            ref="assetTCF"
            name="files"
            class="avatar-uploader"
            :action="action"
            :headers="headers"
            :show-file-list="false"
            :on-success="handleSuccessJS"
            :before-upload="beforeAvatarUpload"
          >
            <div v-if="formData.assetTCF" class="upload-img">
              <img
                v-if="formData.assetTCF"
                :src="imglist.showjsimg"
                class="upload-img"
              />
              <div class="mask-wrapper">
                <i
                  class="el-icon-zoom-in item"
                  @click="previewImg('assetTCF')"
                ></i>
                <i
                  class="el-icon-delete item"
                  @click="removeImg('assetTCF')"
                ></i>
              </div>
            </div>
            <div v-else class="add-wrapper">
              <el-button type="primary" size="mini">添加文件</el-button>
            </div>
          </el-upload>
          <!-- <upLoadImage :limit="1" @get-ImgList="getImgList"></upLoadImage> -->
        </el-form-item>
      </el-row>
      <!--万用表 -->
      <el-table
        v-if="isshowmeter"
        border
        :data="formData.assetCategorys"
        :cell-style="cellStyle"
        :header-cell-style="headerCellStyle"
        @row-click="getcurrentRow"
      >
        <el-table-column label="#" prop="categoryNumber" autosize>
          <template slot-scope="scope">
            <el-input
              size="small"
              v-model="scope.row.categoryNumber"
              disabled
            ></el-input>
          </template>
        </el-table-column>
        <el-table-column label="测量值" prop="categoryOhms" autosize>
          <template slot-scope="scope">
            <el-input size="small" v-model="scope.row.categoryOhms" disabled />
          </template>
        </el-table-column>
        <el-table-column
          label="不确定度(Unc)"
          prop="categoryNondeterminacy"
          autosize
        >
          <template slot-scope="scope">
            <el-input
              size="small"
              v-model="scope.row.categoryNondeterminacy"
              type="number"
            />
          </template>
        </el-table-column>
        <el-table-column label="类型" prop="categoryType" autosize>
          <template slot-scope="scope">
            <el-select size="mini" v-model="scope.row.categoryType">
              <el-option
                v-for="item in scope.row.categoryTypeList"
                :label="item"
                :value="item"
                :key="item"
              ></el-option>
            </el-select>
          </template>
        </el-table-column>
        <el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
          <template slot-scope="scope">
            <el-input
              class="input"
              size="small"
              v-model="scope.row.categoryBHYZ"
              type="number"
              maxlength="7"
              max="9999"
            />
          </template>
        </el-table-column>
      </el-table>
      <!--工装-->
      <el-table
        v-else-if="isshowgz"
        border
        :data="formData.assetCategorys"
        :cell-style="cellStyle"
        :header-cell-style="headerCellStyle"
        @row-click="getcurrentRow"
      >
        <el-table-column label="序号" prop="categoryNumber" autosize>
          <template slot-scope="scope">
            <el-input
              size="small"
              v-model="scope.row.categoryNumber"
              disabled
            ></el-input>
          </template>
        </el-table-column>
        <el-table-column label="阻值" prop="categoryOhms" autosize>
          <template slot-scope="scope">
            <el-input
              size="small"
              v-model="scope.row.categoryOhms"
              type="text"
            />
          </template>
        </el-table-column>
        <el-table-column
          label="不确定度(Unc)"
          prop="categoryNondeterminacy"
          autosize
        >
          <template slot-scope="scope">
            <el-input
              size="small"
              v-model="scope.row.categoryNondeterminacy"
              type="text"
            />
          </template>
        </el-table-column>
        <el-table-column label="类型" prop="categoryType" autosize>
          <template slot-scope="scope">
            <el-select size="mini" v-model="scope.row.categoryType">
              <el-option
                v-for="item in commonArr"
                :label="item.name"
                :value="item.value"
                :key="item.value"
              ></el-option>
            </el-select>
          </template>
        </el-table-column>
        <el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
          <template slot-scope="scope">
            <el-input
              class="input"
              size="small"
              v-model="scope.row.categoryBHYZ"
              type="text"
            />
          </template>
        </el-table-column>
      </el-table>

      <!--分流器-->
      <el-table
        v-else-if="isshowflq"
        border
        :data="formData.assetCategorys"
        :cell-style="cellStyle"
        :header-cell-style="headerCellStyle"
        @row-click="getcurrentRow"
      >
        <el-table-column label="序号" prop="categoryNumber" autosize>
          <template slot-scope="scope">
            <el-input
              size="mini"
              v-model="scope.row.categoryNumber"
              disabled
            ></el-input>
          </template>
        </el-table-column>
        <el-table-column label="阻值" prop="categoryOhms" autosize>
          <template slot-scope="scope">
            <el-input
              size="mini"
              v-model="scope.row.categoryOhms"
              type="text"
            ></el-input>
          </template>
        </el-table-column>
        <el-table-column
          label="不确定度(Unc)"
          prop="categoryNondeterminacy"
          autosize
        >
          <template slot-scope="scope">
            <el-input
              size="mini"
              v-model="scope.row.categoryNondeterminacy"
              type="text"
            />
          </template>
        </el-table-column>
        <el-table-column label="类型" prop="categoryType" autosize>
          <template slot-scope="scope">
            <el-select size="mini" v-model="scope.row.categoryType">
              <el-option
                v-for="item in commonArr"
                :label="item.name"
                :value="item.value"
                :key="item.value"
              ></el-option>
            </el-select>
          </template>
        </el-table-column>
        <el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
          <template slot-scope="scope">
            <el-input
              class="input"
              size="mini"
              v-model="scope.row.categoryBHYZ"
              type="text"
            />
          </template>
        </el-table-column>
      </el-table>
      <el-row
        :style="{
          'margin-top': '20px',
        }"
      >
        <el-form-item label="描述">
          <el-input
            type="textarea"
            v-model="formData.assetDescribe"
            :autosize="{ minRows: 4, maxRows: 6 }"
          ></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="备注">
          <el-input
            type="textarea"
            v-model="formData.assetRemarks"
            :autosize="{ minRows: 4, maxRows: 6 }"
          ></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="图片">
          <el-upload
            ref="assetImage"
            name="files"
            class="avatar-uploader"
            :action="action"
            :headers="headers"
            :show-file-list="false"
            :on-success="handleSuccessImg"
            :before-upload="beforeAvatarUpload"
          >
            <img
              v-if="formData.assetImage"
              :src="imglist.img"
              class="upload-img"
            />
            <i v-else class="el-icon-plus avatar-uploader-icon"></i>
            <i class="el-icon-plus avatar-uploader-icon"></i>
          </el-upload>
        </el-form-item>
      </el-row>
    </el-form>
    <el-form>
      <el-row type="flex" justify="end">
        <el-form-item>
          <el-button v-if="iscancel" size="mini" @click="cancel"
            >取 消</el-button
          >
          <el-button v-if="isview" type="primary" size="mini" @click="view"
            >预 览</el-button
          >
          <el-button v-if="isback" size="mini" @click="onback">返 回</el-button>
          <el-button
            v-if="issubmit"
            type="primary"
            size="mini"
            @click="onsubmit"
            >确 认</el-button
          >
        </el-form-item>
      </el-row>
    </el-form>
    <!-- 预览图 -->
    <el-dialog :visible.sync="visible" top="10vh">
      <img :src="currentImg" width="100%" />
    </el-dialog>
  </div>
</template>
<script>
import { getListOrg, getListUser, add } from "@/api/assetmanagement";
import assetMixin from "./mixin/mixin";
export default {
  mixins: [assetMixin],
  props: {
    type: {
      type: String,
      default: "",
    },
  },
  data() {
    return {
      commonArr: [
        {
          name: "绝对不确定度(Ω)",
          value: "绝对不确定度(Ω)",
        },
        {
          name: "相对不确定度(ppm)",
          value: "相对不确定度(ppm)",
        },
        {
          name: "相对不确定度(%)",
          value: "相对不确定度(%)",
        },
      ],
      commonArr2: [
        {
          name: "绝对不确定度(Ω)",
          value: "绝对不确定度(Ω)",
        },
        {
          name: "相对不确定度(ppm)",
          value: "相对不确定度(ppm)",
        },
        {
          name: "相对不确定度(%)",
          value: "相对不确定度(%)",
        },
      ],
      config: {
        name: "",
      },
      iscancel: true, //是否关闭弹框
      isback: false, //是否返回编辑
      issubmit: false, //是否提交
      isview: true, //是否预览
      userconfig: {
        name: "",
        Orgid: "",
      },
      imglist: {
        showzsimg: "", //用于显示校准证书图片的字段
        showzbimg: "", //用于显示技术指标图片的字段
        showsjimg: "", //用于显示校准数据图片的字段
        showjsimg: "", //用于显示技术文件图片的字段
        img: "", //用于显示图片的字段
      },
      formData: {
        id: "", // 资产ID
        assetStatus: "", // 状态
        assetSerial: "", //类别值
        assetCategory: "", // 类别
        orgName: "", // 部门
        assetType: "", // 型号
        assetHolder: "", // 持有者
        assetStockNumber: "", // 出场编号S/N
        assetAdmin: "", // 管理员
        assetNumber: "", // 资产编号
        assetFactory: "", // 制造厂
        assetInspectType: "", // 送检类型
        assetInspectWay: "", // 送检方式
        assetStartDate: "", // 校准日期
        assetCalibrationCertificate: "", // 校准证书
        assetEndDate: "", // 失效日期
        assetInspectDataOne: "", // 技术指标
        assetInspectDataTwo: "", // 校准数据
        assetTCF: "", // 技术文件
        assetDescribe: "", // 描述
        assetRemarks: "", // 备注
        assetImage: "", // 上传图片
        assetCategorys: [], //万用表、工装、分流器数据
      },
      rules: {
        assetStatus: [
          {
            required: true,
            message: "请选择状态",
            trigger: "change",
          },
        ],
        assetCategory: [
          {
            required: true,
            message: "请选择类别",
            trigger: "change",
          },
        ],
        orgName: [
          {
            required: true,
            message: "请输入部门",
            trigger: "change",
          },
        ],
        assetType: [
          {
            required: true,
            message: "请输入型号",
            trigger: "blur",
          },
        ],
        assetHolder: [
          {
            required: true,
            message: "请输入持有者",
            trigger: "change",
          },
        ],
        assetStockNumber: [
          {
            required: true,
            message: "请输入出厂编号",
            trigger: "blur",
          },
        ],
        assetAdmin: [
          {
            required: true,
            message: "请输入管理员",
            trigger: "blur",
          },
        ],
        assetFactory: [
          {
            required: true,
            message: "请输入制造厂",
            trigger: "blur",
          },
        ],
        assetInspectType: [
          {
            required: true,
            message: "请选择送检类型",
            trigger: "change",
          },
        ],
        assetInspectWay: [
          {
            required: true,
            message: "请选择送检方式",
            trigger: "change",
          },
        ],
        assetStartDate: [
          {
            required: true,
            message: "请选择校准日期",
            trigger: "change",
          },
        ],
        assetCalibrationCertificate: [
          {
            required: true,
            message: "请上传校准证书",
            trigger: "blur",
          },
        ],
        assetEndDate: [
          {
            required: true,
            message: "请选择失效日期",
            trigger: "change",
          },
        ],
        assetInspectDataOne: [
          {
            required: true,
            message: "请上传技术指标",
            trigger: "blur",
          },
        ],
        assetInspectDataTwo: [
          {
            required: true,
            message: "请上传校准数据",
            trigger: "blur",
          },
        ],
        assetTCF: [
          {
            required: true,
            message: "请上传技术文件",
            trigger: "blur",
          },
        ],
      },
      disable: false,
      currentIndex: "",
      orgs: [], //部门列表
      sjTypes: [], //送检类型列表
      users: [], //持有人列表
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`, // 文件上传地址
      headers: {
        // 上传标识
        "X-Token": this.$store.state.user.token,
      },
      visible: false, // 预览图弹窗
      currentImg: "", // 当前preview的图片
      isshowmeter: false, //是否显示万用表
      isshowgz: false, //是否显示工装
      isshowflq: false, //是否显示分流器
    };
  },
  watch: {
    "formData.assetJZCertificate"(val) {
      this.toggleInputDisabled("assetJZCertificate", val);
    },
    "formData.assetJZData1"(val) {
      this.toggleInputDisabled("assetJZData1", val);
    },
    "formData.assetJZData2"(val) {
      this.toggleInputDisabled("assetJZData2", val);
    },
    "formData.assetJSFile"(val) {
      this.toggleInputDisabled("assetJSFile", val);
    },
    "formData.assetImage"(val) {
      this.toggleInputDisabled("assetImage", val);
    },
  },
  methods: {
    cellStyle() {
      return {
        "text-align": "center",
        "vertical-align": "middle",
      };
    },
    headerCellStyle({ rowIndex }) {
      let style = {
        "text-align": "center",
        "vertical-align": "middle",
      };
      if (rowIndex === 1) {
        style.display = "none";
      }
      return style;
    },
    //获取失效日期
    getsxdate: function() {
      var year = new Date(this.formData.assetStartDate).getFullYear() + 1;
      var month = new Date(this.formData.assetStartDate).getMonth() + 1;
      var date = new Date(this.formData.assetStartDate).getDate() - 1;
      this.formData.assetEndDate = year + "-" + month + "-" + date;
    },
    changeSJWay() {
      this.sjTypes = this.getassetSJWay(this.formData.assetInspectType);
    },
    getOrg() {
      return getListOrg(this.config).then((res) => {
        for (let item of res.data) {
          this.orgs.push({
            value: item.name,
            id: item.id,
          });
        }
      });
    },
    async querySearchOrg(queryString, cb) {
      this.config.name = queryString;
      await this.getOrg();
      var result = queryString
        ? this.orgs.filter(this.createStateFilter(queryString))
        : this.orgs;
      this.orgs = [];
      cb(result);
    },
    createStateFilter(queryString) {
      return (state) => {
        return (
          state.value.toLowerCase().indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    submitForm(formName) {
      var isOK = false;
      this.$refs[formName].validate((valid) => {
        if (valid) {
          if (
            this.formData.assetJZData1 == "" ||
            this.formData.assetJZData2 == "" ||
            this.formData.assetJSFile == "" ||
            this.formData.assetJZCertificate == ""
          ) {
            this.$message.warning({
              message: "必须上传的文件没有全部上传",
              type: "action",
            });
            isOK = false;
          } else {
            isOK = true;
          }
        } else {
          isOK = false;
        }
      });
      return isOK;
    },
    view() {
      if (this.submitForm("formData")) {
        this.disable = true;
        this.isback = true;
        this.issubmit = true;
        this.isview = false;
        this.iscancel = false;
      }
    },
    onback() {
      this.disable = false;
      this.disable = false;
      this.isback = false;
      this.issubmit = false;
      this.isview = true;
      this.iscancel = true;
    },
    onsubmit() {
      if (this.submitForm("formData")) {
        this.addzc(this.formData);
      }
    },
    cancel() {
      this.$emit("cancel", false);
    },
    handleSelect(val) {
      this.userconfig.Orgid = val.id;
    },
    handleSelectUser(val) {
      this.userId = val.id;
    },
    getuser() {
      return getListUser(this.userconfig).then((res) => {
        for (let item of res.data) {
          this.users.push({
            value: item.name,
            id: item.id,
          });
        }
      });
    },
    async queryUser(queryString, cb) {
      this.userconfig.name = queryString;
      await this.getuser();
      var result = queryString
        ? this.users.filter(this.createStateFilter(queryString))
        : this.users;
      this.users = [];
      cb(result);
    },
    //添加资产信息
    async addzc(param) {
      try {
        await add(param);
        this.cancel();
      } catch (error) {
        this.$message.error(error.message);
      }
    },
    toggleInputDisabled(type, val) {
      let input = this.$refs[type].$el.childNodes[0].childNodes[1]; // 取到input元素
      if (val) {
        // 图片有值时
        input.setAttribute("disabled", "disabled");
      } else {
        setTimeout(() => {
          input.removeAttribute("disabled");
        }, 0);
      }
    },
    handleSuccessJZ(res, file) {
      // 校准证书图片
      this.setImg("showzsimg", file);
      this.formData.assetCalibrationCertificate = res.result[0].id;
    },
    handleSuccessData1(res, file) {
      // 数据1
      this.setImg("showzbimg", file);
      this.formData.assetInspectDataOne = res.result[0].id;
    },
    handleSuccessData2(res, file) {
      // 数据2
      this.setImg("showsjimg", file);
      this.formData.assetInspectDataTwo = res.result[0].id;
    },
    handleSuccessJS(res, file) {
      // 技术文件
      this.setImg("showjsimg", file);
      this.formData.assetTCF = res.result[0].id;
    },
    handleSuccessImg(res, file) {
      // 图片
      this.setImg("img", file);
      this.formData.assetImage = res.result[0].id;
    },
    removeImg(type) {
      this.formData[type] = "";
    },
    previewImg(type) {
      this.visible = true;
      this.currentImg = this.formData[type];
    },
    handleAvatarSuccess(res, file) {
      this.formData.assetImage = URL.createObjectURL(file.raw);
    },
    setImg(type, file) {
      this.imglist[type] = URL.createObjectURL(file.raw);
    },
    beforeAvatarUpload(file) {
      let testmsg = /^image\/(jpeg|png|jpg)$/.test(file.type);
      if (!testmsg) {
        this.$message.error("上传图片格式不对!");
      }
      return testmsg;
    },
    getcurrentRow(row) {
      this.currentIndex = this.formData.assetCategorys.findIndex(
        (item) => row == item
      );
    },
    onTypeChange(val) {
      switch (val) {
        case "万用表":
          this.isshowmeter = true;
          this.isshowgz = false;
          this.isshowflq = false;
          var nums = ["DCV", "ACV", "DCI", "ACI", "R"];
          var arrs = [
            ["相对不确定度(%)", "绝对不确定度(V)"],
            ["相对不确定度(%)", "绝对不确定度(V)"],
            ["相对不确定度(%)", "绝对不确定度(A)"],
            ["相对不确定度(%)", "绝对不确定度(A)"],
            ["相对不确定度(%)", "绝对不确定度(Ω)"],
          ];
          this.formData.assetCategorys = [];
          this.formData.assetSerial = "1";
          this.formData.assetCategorys = [];
          for (let i = 0; i < 5; i++) {
            this.formData.assetCategorys.push({
              id: "", //类别ID
              assetId: "", //资产ID
              categoryNumber: nums[i], //序号
              categoryOhms: 0, //阻值
              categoryNondeterminacy: "", //不确定度
              categoryType: "", //不确定类型
              categoryTypeList: arrs[i], //不确定类型数组
              categoryBHYZ: "", //包含因子
              categoryAort: "", //排序
            });
          }
          break;
        case "工装":
          this.isshowmeter = false;
          this.isshowgz = true;
          this.isshowflq = false;
          var arr = ["R1", "R2", "R3", "R4"];
          this.formData.assetSerial = "2";
          for (let i = 0; i < 4; i++) {
            this.formData.assetCategorys.push({
              id: "", //类别ID
              assetId: "", //资产ID
              categoryNumber: arr[i], //序号
              categoryOhms: "", //阻值
              categoryNondeterminacy: "", //不确定度
              categoryType: "", //不确定类型
              categoryBHYZ: "", //包含因子
              categoryAort: "", //排序
            });
          }
          break;
        case "分流器":
          this.isshowmeter = false;
          this.isshowgz = false;
          this.isshowflq = true;
          this.formData.assetCategorys = [];
          this.formData.assetSerial = "3";
          this.formData.assetCategorys.push({
            id: "", //类别ID
            assetId: "", //资产ID
            categoryNumber: "R", //序号
            categoryOhms: "", //阻值
            categoryNondeterminacy: "", //不确定度
            categoryType: "", //不确定类型
            categoryBHYZ: "", //包含因子
            categoryAort: "", //排序
          });
          break;
        case "标准源":
          this.isshowmeter = false;
          this.isshowgz = false;
          this.isshowflq = false;
          this.formData.assetSerial = "4";
          this.formData.assetCategorys = [];
          break;
        default:
          this.isshowmeter = false;
          this.isshowgz = false;
          this.isshowflq = false;
          break;
      }
    },
  },
};
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
</style>
