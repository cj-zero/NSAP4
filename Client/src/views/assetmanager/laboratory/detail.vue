<template>
  <el-dialog :visible.sync="dialogFormVisible" width="900px">
    <el-tabs v-model="activeName" type="card">
      <el-tab-pane label="详情" name="first">
        <div>
          <el-form
            :model="formData"
            position="left"
            label-width="110px"
            size="mini"
          >
            <el-row type="flex" justify="space-around">
              <el-col :span="11">
                <el-form-item label="资产ID" required>
                  <el-input
                    disabled
                    style="width: 200px"
                    v-model="formData.id"
                  ></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="状态" required>
                  <el-select
                    v-model="formData.assetStatus"
                    placeholder="请选择状态"
                  >
                    <el-option
                      v-for="item in assetStatus"
                      :value="item.value"
                      :key="item.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="11">
                <el-form-item label="类别" required>
                  <el-select
                    disabled
                    v-model="formData.assetCategory"
                    placeholder="请选择"
                    @change="onTypeChange"
                  >
                    <el-option
                      v-for="item in assetCategory"
                      :value="item.value"
                      :key="item.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="部门" required>
                  <el-autocomplete
                    v-model="formData.orgName"
                    placeholder="请输入部门"
                    :fetch-suggestions="querySearchOrg"
                  ></el-autocomplete>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="11">
                <el-form-item label="型号" required>
                  <el-input
                    disabled
                    style="width: 200px"
                    placeholder="请输入内容"
                    v-model="formData.assetType"
                  ></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="持有者" required>
                  <el-input
                    style="width: 200px"
                    placeholder="请输入内容"
                    v-model="formData.assetHolder"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="11">
                <el-form-item label="出厂编号S/N" required>
                  <el-input
                    disabled
                    style="width: 200px"
                    placeholder="请输入内容"
                    v-model="formData.assetStockNumber"
                  ></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="管理员" required>
                  <el-input
                    style="width: 200px"
                    placeholder="请输入内容"
                    v-model="formData.assetAdmin"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="11">
                <el-form-item label="资产编号" required>
                  <el-input
                    style="width: 200px"
                    placeholder="请输入内容"
                    v-model="formData.assetNumber"
                    disabled
                  ></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="制造厂" required>
                  <el-input
                    disabled
                    style="width: 200px"
                    placeholder="请输入内容"
                    v-model="formData.assetFactory"
                  ></el-input>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="11">
                <el-form-item label="送检类型" required>
                  <el-select
                    v-model="formData.assetInspectType"
                    placeholder="请选择"
                  >
                    <el-option
                      v-for="item in assetSJType"
                      :key="item.value"
                      :value="item.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="送检方式" required placeholder="请选择">
                  <el-select v-model="formData.assetInspectWay">
                    <el-option
                      v-for="item in assetSJWay"
                      :key="item.value"
                      :value="item.value"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row type="flex" justify="space-around">
              <el-col :span="11">
                <el-form-item label="校准日期" required>
                  <el-date-picker
                    v-model="formData.assetStartDate"
                    type="date"
                    placeholder="选择日期"
                  >
                  </el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="校准证书" required>
                  <el-upload
                    ref="assetJZCertificate"
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
                        :src="getImgUrl(formData.assetCalibrationCertificate)"
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
              <el-col :span="11">
                <el-form-item label="失效日期" required>
                  <el-date-picker
                    v-model="formData.assetEndDate"
                    type="date"
                    placeholder="选择日期"
                  >
                  </el-date-picker>
                </el-form-item>
              </el-col>
              <el-col :span="11">
                <el-form-item label="校准数据" required>
                  <el-row type="flex" justify="space-around">
                    <el-col :span="10">
                      <el-upload
                        ref="assetJZData1"
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
                            :src="getImgUrl(formData.assetInspectDataOne)"
                            class="upload-img"
                          />
                          <div class="mask-wrapper">
                            <i
                              class="el-icon-zoom-in item"
                              @click="previewImg('assetInspectDataOne')"
                            ></i>
                          </div>
                        </div>
                        <div v-else class="add-wrapper">
                          <el-button type="primary" size="mini"
                            >添加文件</el-button
                          >
                        </div>
                      </el-upload>
                    </el-col>
                    <el-col :span="10">
                      <el-upload
                        ref="assetJZData2"
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
                            :src="getImgUrl(formData.assetInspectDataTwo)"
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
                            >添加文件</el-button
                          >
                        </div>
                      </el-upload>
                    </el-col>
                  </el-row>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row>
              <el-form-item label="技术文件" required>
                <el-upload
                  ref="assetJSFile"
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
                      :src="getImgUrl(formData.assetTCF)"
                      class="upload-img"
                    />
                    <div class="mask-wrapper">
                      <i
                        class="el-icon-zoom-in item"
                        @click="previewImg('assetTCF')"
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
              v-if="formData.assetCategory === '万用表'"
              border
              :cell-style="cellStyle"
              :header-cell-style="headerCellStyle"
              :data="formData.assetCategorys"
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
                  <el-input
                    size="small"
                    v-model="scope.row.categoryOhms"
                    disabled
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
                    type="text"
                  />
                </template>
              </el-table-column>
            </el-table>
            <!--工装-->
            <el-table
              v-if="formData.assetCategory === '工装'"
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
              v-if="formData.assetCategory === '分流器'"
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
            <el-rows>
              <el-form-item
                :style="{
                  'margin-top': '20px',
                }"
                label="描述"
              >
                <el-input
                  type="textarea"
                  v-model="formData.assetDescribe"
                  :autosize="{ minRows: 4, maxRows: 6 }"
                ></el-input>
              </el-form-item>
            </el-rows>
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
                    :src="getImgUrl(formData.assetImage)"
                    class="upload-img"
                  />
                  <i v-else class="el-icon-plus avatar-uploader-icon"></i>
                </el-upload>
              </el-form-item>
            </el-row>
            <el-row type="flex" justify="end">
              <el-button type="primary" @click="updateAssets">修改</el-button>
            </el-row>
          </el-form>
          <!-- 预览图 -->
          <el-dialog append-to-body :visible.sync="visible" top="0">
            <img :src="currentImg" width="100%" />
          </el-dialog>
        </div>
      </el-tab-pane>
      <el-tab-pane label="送检记录" name="second">
        <inspection :list="formData.assetInspects"></inspection>
      </el-tab-pane>
      <el-tab-pane label="操作记录" name="third">
        <opertation :list="formData.assetOperations"></opertation>
      </el-tab-pane>
    </el-tabs>
  </el-dialog>
</template>

<script>
import assetMixin from "./mixin/mixin";
import Opertation from "./operation";
import Inspection from "./inspection";
import { getListOrg, getSingleAsset, update } from "@/api/assetmanagement";
export default {
  components: {
    Opertation,
    Inspection,
  },
  mixins: [assetMixin],
  props: {
    type: {
      type: String,
      default: "",
    },
  },
  data() {
    return {
      baseURL: process.env.VUE_APP_BASE_API,
      tokenValue: this.$store.state.user.token,
      dialogFormVisible: false,
      activeName: "first",
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`, // 文件上传地址
      headers: {
        // 上传标识
        "X-Token": this.$store.state.user.token,
      },
      orgs: [], //部门列表
      visible: false, // 预览图弹窗
      currentImg: "", // 当前preview的图片
      formData: {},
      assetCategoryRes: "", // 类别对象
      tableData: [],
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
    };
  },
  methods: {
    async open(id) {
      const res = await getSingleAsset({
        id,
      });
      this.dialogFormVisible = true;
      this.formData = res.data;
      var arrs = [
        ["相对不确定度(%)", "绝对不确定度(V)"],
        ["相对不确定度(%)", "绝对不确定度(V)"],
        ["相对不确定度(%)", "绝对不确定度(A)"],
        ["相对不确定度(%)", "绝对不确定度(A)"],
        ["相对不确定度(%)", "绝对不确定度(Ω)"],
      ];
      for (let i = 0; i < 5; i++) {
        this.formData.assetCategorys[i].categoryTypeList = arrs[i]
      }
    },
    async updateAssets() {
      try {
        const formData = this.formData;
        await update(formData);
        this.dialogFormVisible = false;
      } catch (error) {
        this.$message.error(error.message);
      }
    },
    getImgUrl(id) {
      return `${this.baseURL}/files/Download/${id}?X-Token=${this.tokenValue}`;
    },
    handleSuccessJZ(res) {
      // 校准证书图片
      this.formData.assetCalibrationCertificate = res.result[0].id;
    },
    handleSuccessData1(res) {
      // 数据1
      this.formData.assetInspectDataOne = res.result[0].id;
    },
    handleSuccessData2(res) {
      // 数据2
      this.formData.assetInspectDataTwo = res.result[0].id;
    },
    handleSuccessJS(res) {
      // 技术文件
      this.formData.assetTCF = res.result[0].id;
    },
    handleSuccessImg(res) {
      // 图片
      this.formData.assetImage = res.result[0].id;
    },
    removeImg(type) {
      this.formData[type] = "";
    },
    previewImg(type) {
      this.visible = true;
      this.currentImg = this.getImgUrl(this.formData[type]);
    },
    handleAvatarSuccess(res, file) {
      this.formData.assetImage = URL.createObjectURL(file.raw);
    },
    setImg(type, file) {
      this.formData[type] = URL.createObjectURL(file.raw);
    },
    beforeAvatarUpload(file) {
      let testmsg = /^image\/(jpeg|png|jpg)$/.test(file.type);
      if (!testmsg) {
        this.$message.error("上传图片格式不对!");
      }
      return testmsg;
    },
    onTypeChange(val) {
      let { number, name } = val;
      console.log(number, name);
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
    object-fit: contain;
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
.input {
  margin: 5px 0;
}
</style>
