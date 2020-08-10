<template>
  <div>
    详情
    <el-form :model="formData" position="left" label-width="110px">
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
              class="avatar-uploader"
              action="https://jsonplaceholder.typicode.com/posts/"
              :show-file-list="false"
              :on-success="handleAvatarSuccess"
              :before-upload="beforeAvatarUpload">
              <img v-if="formData.assetImg" :src="formData.assetImg" class="upload-img">
              <i v-else class="el-icon-plus avatar-uploader-icon"></i>
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
                  class="avatar-uploader"
                  action="https://jsonplaceholder.typicode.com/posts/"
                  :show-file-list="false"
                  :on-success="handleAvatarSuccess"
                  :before-upload="beforeAvatarUpload">
                  <img v-if="formData.assetImg" :src="formData.assetImg" class="upload-img">
                  <i v-else class="el-icon-plus avatar-uploader-icon"></i>
                </el-upload>
              </el-col>
              <el-col :span="10">
                <el-upload
                  class="avatar-uploader"
                  action="https://jsonplaceholder.typicode.com/posts/"
                  :show-file-list="false"
                  :on-success="handleAvatarSuccess"
                  :before-upload="beforeAvatarUpload">
                  <img v-if="formData.assetImg" :src="formData.assetImg" class="upload-img">
                  <i v-else class="el-icon-plus avatar-uploader-icon"></i>
                </el-upload>
              </el-col>
            </el-row>
            
          </el-form-item>
        </el-col>
      </el-row>
      <el-row>
        <el-form-item label="技术文件" required>
          <el-upload
            class="avatar-uploader"
            action="https://jsonplaceholder.typicode.com/posts/"
            :show-file-list="false"
            :on-success="handleAvatarSuccess"
            :on-remove="handleRemove"
            :before-upload="beforeAvatarUpload">
            <img v-if="formData.assetImg" :src="formData.assetImg" class="upload-img">
            <div v-else>
              <el-button size="mini" type="primary">点击上传</el-button>
              <i class="el-icon-plus avatar-uploader-icon"></i>
            </div>
            
          </el-upload>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="描述">
          <el-input type="textarea" v-model="formData.assetDescribe" autosize></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="备注">
          <el-input type="textarea" v-model="formData.assetRemarks" autosize></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="描述">
          <el-upload
            class="avatar-uploader"
            action="https://jsonplaceholder.typicode.com/posts/"
            :show-file-list="false"
            :on-success="handleAvatarSuccess"
            :before-upload="beforeAvatarUpload">
            <img v-if="formData.assetImg" :src="formData.assetImg" class="upload-img">
            <i v-else class="el-icon-plus avatar-uploader-icon"></i>
          </el-upload>
        </el-form-item>
      </el-row>
    </el-form>
  </div>
</template>

<script>
// const SYS_AssetCategory = 'SYS_AssetCategory' //类别
// const SYS_AssetStatus = 'SYS_AssetStatus' // 状态
// const SYS_AssetSJType = 'SYS_AssetSJType' // 送检类型
// const SYS_CategoryNondeterminacy = 'SYS_CategoryNondeterminacy' // 阻值类型
export default {
  components: {},
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
  methods: {
    handleSelect () {},
    querySearchAsync () {},
    handleRemove(file, fileList) {
      console.log(file, fileList);
    },
    handlePreview(file) {
      console.log(file);
    },
    handleExceed(files, fileList) {
      this.$message.warning(`当前限制选择 3 个文件，本次选择了 ${files.length} 个文件，共选择了 ${files.length + fileList.length} 个文件`);
    },
    beforeRemove(file) {
      return this.$confirm(`确定移除 ${ file.name }？`);
    },
    handleAvatarSuccess(res, file) {
      this.formData.assetImg = URL.createObjectURL(file.raw);
    },
    beforeAvatarUpload(file) {
      const isJPG = file.type === 'image/jpeg';
      const isLt2M = file.size / 1024 / 1024 < 2;

      if (!isJPG) {
        this.$message.error('上传头像图片只能是 JPG 格式!');
      }
      if (!isLt2M) {
        this.$message.error('上传头像图片大小不能超过 2MB!');
      }
      return isJPG && isLt2M;
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
  width: 50px;
  height: 50px;
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
  .upload-img {
    width: 50px;
    height: 50px;
  }
}
</style>