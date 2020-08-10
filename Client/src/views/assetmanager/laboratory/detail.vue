<template>
  <div>
    <el-form :model="formData" position="left" label-width="110px" size="mini">
      <el-row type="flex" justify="space-around">
        <el-col :span="11">
          <el-form-item label="资产ID" required>
            <el-input disabled style="width: 200px" v-model="formData.id"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="11">
          <el-form-item label="状态" required>
            <el-select v-model="formData.assetStatus" placeholder="请选择状态">
              <el-option v-for="item in formData.assetStatus" :value="item" :key="item"></el-option>
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="11">
          <el-form-item label="类别" required>
            <el-select v-model="formData.assetStatus" placeholder="请选择状态">
              <el-option v-for="item in formData.assetStatus" :value="item" :key="item"></el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="11">
          <el-form-item label="部门" required>
            <el-autocomplete
              v-model="formData.orgId"
              :fetch-suggesstions="querySearchAsync"
              placeholder="请输入部门"
              @select="handleSelect"
            ></el-autocomplete>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="11">
          <el-form-item label="型号" required>
            <el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetType"></el-input>
          </el-form-item>
        </el-col>
         <el-col :span="11">
          <el-form-item label="持有者" required>
            <el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetHolder"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="11">
          <el-form-item label="出厂编号S/N" required>
            <el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetCCNumber"></el-input>
          </el-form-item>
        </el-col>
         <el-col :span="11">
          <el-form-item label="管理员" required>
            <el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetAdmin"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="11">
          <el-form-item label="资产编号" required>
            <el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetZCNumber"></el-input>
          </el-form-item>
        </el-col>
         <el-col :span="11">
          <el-form-item label="制造厂" required>
            <el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetFactory"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="11">
          <el-form-item label="送检类型" required>
            <el-select v-model="formData.assetSJType">
              <!-- <el-option v-for="" -->
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="11">
          <el-form-item label="送检方式" required>
            <el-select v-model="formData.assetSJType">
              <!-- <el-option v-for="" -->
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="11">
          <el-form-item label="校准日期" required>
            <el-date-picker
              v-model="formData.AssetJZCertificate"
              type="date"
              placeholder="选择日期"
              value-format="yyyy-mm-dd">
            </el-date-picker>
          </el-form-item>
        </el-col>
        <el-col :span="11">
          <el-form-item label="校准证书">
            <el-upload
              ref="assetJZCertificate"
              class="avatar-uploader"
              :action="action"
              :headers="headers"
              :show-file-list="false"
              :on-success="handleSuccessJZ"
              :before-upload="beforeAvatarUpload">
              <div v-if="formData.assetJZCertificate" class="upload-img" @click.stop>
                <img v-if="formData.assetJZCertificate" :src="formData.assetJZCertificate" class="upload-img">
                <div class="mask-wrapper">
                  <i class="el-icon-zoom-in item" @click="previewImg('assetJZCertificate')"></i>
                  <i class="el-icon-delete item" @click="removeImg('assetJZCertificate')"></i>
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
              v-model="formData.AssetJZCertificate"
              type="date"
              placeholder="选择日期"
              value-format="yyyy-mm-dd">
            </el-date-picker>
          </el-form-item>
        </el-col>
        <el-col :span="11">
          <el-form-item label="校准数据" required>
            <el-row type="flex" justify="space-around">
              <el-col :span="10">
                <el-upload
                  ref="assetJZData1"
                  class="avatar-uploader"
                  :action="action"
                  :headers="headers"
                  :show-file-list="false"
                  :on-success="handleSuccessData1"
                  :before-upload="beforeAvatarUpload">
                  <div v-if="formData.assetJZData1" class="upload-img" @click.stop>
                    <img v-if="formData.assetJZData1" :src="formData.assetJZData1" class="upload-img">
                    <div class="mask-wrapper">
                      <i class="el-icon-zoom-in item" @click="previewImg('assetJZData1')"></i>
                      <i class="el-icon-delete item" @click="removeImg('assetJZData1')"></i>
                    </div>
                  </div>
                  <div v-else class="add-wrapper">
                    <el-button type="primary" size="mini">添加文件</el-button>
                  </div>
                </el-upload>
              </el-col>
              <el-col :span="10">
                <el-upload
                  ref="assetJZData2"
                  class="avatar-uploader"
                  :action="action"
                  :headers="headers"
                  :show-file-list="false"
                  :on-success="handleSuccessData2"
                  :before-upload="beforeAvatarUpload">
                  <div v-if="formData.assetJZData2" class="upload-img" @click.stop>
                    <img v-if="formData.assetJZData2" :src="formData.assetJZData2" class="upload-img">
                    <div class="mask-wrapper">
                      <i class="el-icon-zoom-in item" @click="previewImg('assetJZData2')"></i>
                      <i class="el-icon-delete item" @click="removeImg('assetJZData2')"></i>
                    </div>
                  </div>
                  <div v-else class="add-wrapper">
                    <el-button type="primary" size="mini">添加文件</el-button>
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
            class="avatar-uploader"
            :action="action"
            :headers="headers"
            :show-file-list="false"
            :on-success="handleSuccessJS"
            :before-upload="beforeAvatarUpload">
            <div v-if="formData.assetJSFile" class="upload-img">
              <img v-if="formData.assetJSFile" :src="formData.assetJSFile" class="upload-img">
              <div class="mask-wrapper">
                <i class="el-icon-zoom-in item" @click="previewImg('assetJSFile')"></i>
                <i class="el-icon-delete item" @click="removeImg('assetJSFile')"></i>
              </div>
            </div>
            <div v-else class="add-wrapper">
              <el-button type="primary" size="mini">添加文件</el-button>
            </div>
          </el-upload>
          <!-- <upLoadImage :limit="1" @get-ImgList="getImgList"></upLoadImage> -->
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="描述">
          <el-input type="textarea" v-model="formData.assetDescribe" autosize :rows="3"></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="备注">
          <el-input type="textarea" v-model="formData.assetRemarks" autosize :rows="3"></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="图片">
          <el-upload
            ref="assetImg"
            class="avatar-uploader"
            :action="action"
            :headers="headers"
            :show-file-list="false"
            :on-success="handleSuccessImg"
            :before-upload="beforeAvatarUpload">
            <img v-if="formData.assetImg" :src="formData.assetImg" class="upload-img">
            <i v-else class="el-icon-plus avatar-uploader-icon"></i>
          </el-upload>
        </el-form-item>
      </el-row>
    </el-form>
    <!-- 预览图 -->
    <el-dialog
      :visible.sync="visible"
      top="10vh"
    >
      <img :src="currentImg" width="100%">
    </el-dialog>
  </div>
</template>

<script>
// const SYS_AssetCategory = 'SYS_AssetCategory' //类别
// const SYS_AssetStatus = 'SYS_AssetStatus' // 状态
// const SYS_AssetSJType = 'SYS_AssetSJType' // 送检类型
// const SYS_CategoryNondeterminacy = 'SYS_CategoryNondeterminacy' // 阻值类型
export default {
  components: {
  },
  props: {
    type: {
      type: String,
      default: ''
    },
    options: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`, // 文件上传地址
      headers:{ // 上传标识
        "X-Token": this.$store.state.user.token
      },
      visible: false, // 预览图弹窗
      currentImg: '', // 当前preview的图片
      formData: {
        id: '', // 资产ID
        assetStatus: [], // 状态
        assetCategory: [], // 类别
        orgId: '', // 部门
        assetType: '', // 型号
        assetHolder: '', // 持有者
        assetCCNumber: '', // 出场编号S/N
        assetAdmin: '', // 管理员
        assetZCNumber: '', // 资产编号
        assetFactory: '', // 制造厂
        assetSJType: '', // 送检类型
        assetSJWay : '', // 送检方式
        assetJXDate: '', // 校准日期
        assetJZCertificate: '', // 校准证书
        assetSXDate: '', // 失效日期
        assetJZData1: '', // 校准数据1
        assetJZData2: '', // 校准数据2
        assetJSFile: '', // 技术文件
        assetDescribe: '', // 描述
        assetRemarks: '',  // 备注
        assetImg: '' // 上传图片
      },
      tableData: [],
    }
  },
  watch: {
    'formData.assetJZCertificate' (val) {
      this.toggleInputDisabled('assetJZCertificate', val)
    },
    'formData.assetJZData1' (val) {
      this.toggleInputDisabled('assetJZData1', val)
    },
    'formData.assetJZData2' (val) {
      this.toggleInputDisabled('assetJZData2', val)
    },
    'formData.assetJSFile' (val) {
      this.toggleInputDisabled('assetJSFile', val)
    },
    'formData.assetImg' (val) {
      this.toggleInputDisabled('assetImg', val)
    }
  },
  methods: {
    handleSelect () {},
    querySearchAsync () {},
    toggleInputDisabled (type, val) {
      let input = this.$refs[type].$el.childNodes[0].childNodes[1] // 取到input元素
      console.log(val, 'val')
      if (val) { // 图片有值时
        input.setAttribute('disabled', 'disabled')
        console.log(input, 'input')
      } else {
        setTimeout(() => {
          input.removeAttribute('disabled')
        }, 0)
      }
    },
    beforeRemove(file) {
      return this.$confirm(`确定移除 ${ file.name }？`);
    },
    handleSuccessJZ (res, file) { // 校准证书图片
      this.setImg('assetJZCertificate', file)
    },
    handleSuccessData1 (res, file) { // 数据1
      this.setImg('assetJZData1', file)
    },
    handleSuccessData2 (res, file) { // 数据2
      this.setImg('assetJZData2', file)
    },
    handleSuccessJS (res, file) { // 技术文件
      this.setImg('assetJSFile', file)
    },
    handleSuccessImg (res, file) { // 图片
      this.setImg('assetImg', file)
    },
    removeImg (type) {
      this.formData[type] = ''
    },
    previewImg (type) {
      this.visible = true
      this.currentImg = this.formData[type]
    },
    handleAvatarSuccess(res, file) {
      // {
      //   pictureId:res.result[0].id
      // }
      this.formData.assetImg = URL.createObjectURL(file.raw);
    },
    setImg (type, file) {
      console.log('222')
      this.formData[type] = URL.createObjectURL(file.raw);
    },
    beforeAvatarUpload(file) {
      let testmsg = /^image\/(jpeg|png|jpg)$/.test(file.type)
      if (!testmsg) {
        this.$message.error('上传图片格式不对!')
      }
      return testmsg
    }
  },
  created () {
  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.avatar-uploader {
  position: relative;
  width: 80px;
  height: 40px;
  ::v-deep .el-upload {
    position: relative;
    width: 100%;
    height: 100%;
  }
  ::v-deep .el-icon-plus, .avatar-uploader-icon {
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
      background-color: rgba(0, 0, 0, .5);
      opacity: 0;
      transition: opacity .3s;
      .item {
        margin: 0 5px;
        color: white;
      }
    }
  }
}
</style>