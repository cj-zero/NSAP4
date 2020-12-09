<template>
  <div class="express-wrapper">
    <!-- 表单 -->
    <el-form ref="form" :show-message="false" :model="formData" label-width="80px">
      <el-row type="flex">
        <el-col>
          <el-form-item 
            label="快递单号"
            prop="trackNum"
            :rules="{ required: true, trigger: 'blur' }"
          >
            <el-input v-model="formData.trackNum" size="mini"></el-input>
          </el-form-item>
          <el-form-item 
            label="备注"
            prop="remark"
          >
            <el-input v-model="formData.remark" size="mini"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
    <el-row type="flex" class="attachment-wrapper">
      <span class="attachment-text">附件：</span>
      <UploadFile 
        ref="uploadFile"
        uploadType="file" 
        :limit="9" 
        @get-ImgList="getFileList" 
      />
    </el-row>
  </div>
</template>

<script>
import UploadFile from '@/components/upLoadFile'
import { addExpress } from '@/api/serve/afterdelivery'
export default {
  components: {
    UploadFile
  },
  props: {
    detailInfo: {
      type: Object
    },
    status: String,
    currentRow: {
      type: Object,
      default: () => {}
    }
  },
  data () {
    return {
      formData: {
        trackNum: '', // 快递单号
        remark: '' // 备注
      },
      fileList: [] // 附件ID数组
    }
  },
  methods: {
    getFileList (val) {
      console.log(val, 'val')
      this.fileList = val.map(item => {
        return item.pictureId
      })
    },
    resetFiles () {
      if (this.$refs.uploadFile) {
        this.$refs.uploadFile.clearFiles()
      }
      this.fileList = []
    },
    resetInfo () {
      this.$refs.form.clearValidate()
      this.$refs.form.resetFields()
      this.resetFiles()
    },
    async submit () {
      try {
        await this.$refs.form.validate()
        let { trackNum, remark } = this.formData
        console.log(this.currentRow, this.currentRow.expressInfo, 'expressInfo')
        let params = {
          returnRepairId: this.currentRow.returnRepairId,
          trackNum,
          remark,
          accessorys: this.fileList
        }
        return addExpress(params)
      } catch (err) {
        console.log(err, 'err')
        return Promise.reject({ message: '请将必填项填写' })
      }
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.express-wrapper {
  .attachment-wrapper {
    .attachment-text {
      width: 80px;
      text-indent: 12px;
    }
  }
}
</style>