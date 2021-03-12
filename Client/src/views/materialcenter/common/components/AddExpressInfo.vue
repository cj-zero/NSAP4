<template>
  <div class="add-express-info-wrapper">
    <el-form ref="expressForm" :model="info" size="mini" label-width="80px" :show-message="false" :rules="rules">
      <el-form-item :label="orderLabel" prop="number">
        <el-input style="width: 200px;" size="mini" v-model.trim="info.number" :disabled="isPickUp"></el-input>
      </el-form-item>
       <!-- 退料 -->
      <el-form-item label="运送费用" prop="freight" v-if="!isPickUp">
        <el-input-number 
          class="input-number" 
          style="width: 200px;" 
          size="mini" 
          placeholder="金额大于等于零"
          v-model="info.freight" 
          :controls="false">
        </el-input-number>
      </el-form-item>
      <!-- <el-form-item label="备注" prop="remark">
        <el-input style="width: 200px;" size="mini" v-model.trim="expressFormData.remark"></el-input>
      </el-form-item> -->
      <el-form-item label="图片">
        <upLoadFile
          @get-ImgList="getPictureList" 
          ref="expressInfoPictures"
          :onAccept="onAccept"
        ></upLoadFile>
      </el-form-item>
      <el-form-item label="最后一次领料">
         <el-checkbox v-model="info.isLastReturn">备选项</el-checkbox>
      </el-form-item>
    </el-form>
    <div class="material-content-wrapper">
      <el-row type="flex" align="middle">
        <i class="line"></i>
        <h2 class="text">物料信息</h2>
      </el-row>
      <common-table 
        v-loading="materialLoading"
        class="table-wrapper"
        ref="addMaterialColumn"
        style="min-height: 200px;"
        max-height="350px"
        :data="materialList" 
        :columns="materialColumns">
        <template v-slot:quantity="{ row }">
          <el-input-number 
            size="mini"
            :controls="false"
            v-model="row.quantity"
            :min="0"
            :max="row.count - row.sentQuantity"
            style="width: 100%;"
            :disabled="isOutboundAll(row)"
          >
          </el-input-number>
        </template>
        <!-- 退料 -->
        <template v-slot:returnCount="{ row }">
          <el-input-number 
            size="mini"
            :controls="false"
            v-model="row.quantity"
            :min="0"
            :max="row.count - row.sentQuantity"
            style="width: 100%;"
            :disabled="isOutboundAll(row)"
          ></el-input-number>
        </template>
        <template v-slot:upload="{ row }">
          <UpLoadFile 
            type="file" 
            @get-ImgList="getMaterialPictureList" 
            ref="materialPicture"
            :onAccept="onAccept"
            :options="{ row }"
            :limit="1"
          />
        </template>
      </common-table>
    </div>
  </div>
</template>

<script>
import UpLoadFile from '@/components/upLoadFile'
import { getMergeMaterial } from '@/api/material/quotation'
import { isImage } from '@/utils/file'
import { SHOW_STORAGE_COLUMNS_TO_TEST, ADD_QUALITY_COLUMNS, ADD_RETURN_COLUMNS } from '../js/config'
function freightValidator (rule, value, callback) {
  value = Number(value)
  value > 0 ? callback() : callback(new Error('运费必须大于0'))
}
export default {
  components: {
    UpLoadFile
  },
  inject: ['parentVm'],
  props: {
    formData: {
      type: Object,
      default () {
        return {}
      }
    },
    orderType: {
      type: String
    }
  },
  computed: {
    isPickUp () { // 判断是否自提
      return this.formData.acquisitionWay === '1'
    },
    orderLabel () {
      return (this.isPickUp ? '自提' : '快递') + '单号'
    },
    rules () {
      return {
        number: [{ required: !this.isPickUp }],
        freight: [{ required: true, validator: freightValidator }]
      }
    },
     materialColumns () {
       return this.orderType 
        ? ADD_RETURN_COLUMNS
        : this.orderType === 'quality'
          ? ADD_QUALITY_COLUMNS
          : SHOW_STORAGE_COLUMNS_TO_TEST
          
    },
    info () {
      return this.orderType === 'returnOrder' 
        ? this.returnFormData
        : this.qualityFormData
    }
  },
  data () {
    return {
      expressFormData: { // 新增快递信息
        number: '',
        freight: undefined,
        // remark: ''
      },
      returnFormData: { // 申请退料
        trackNumber: '',
        isLastReturn: false,
      },
      qualityFormData: { // 品质检测
        trackNumber: ''
      },
      materialList: [],
      pictureList: [],
      materialLoading: false
    }
  },
  methods: {
    getMergeMaterial () {
      this.materialLoading = true
      getMergeMaterial({ quotationId: this.formData.id }).then(res => {
        this.materialList = res.data.map(item => {
          item.sentQuantity = Number(item.sentQuantity)
          item.quantity = item.quantity || 0
          return item
        })
        this.materialLoading = false
      }).catch(err => {
        this.$message.error(err.message)
        this.materialLoading = false
      })
    },
    validateMaterial () { // 只要有一个物料没有全部出库并且出库数量时大于0的 就可以提交
      return this.materialList.some(item => { 
        let { quantity } = item
        let isOutboundAll = this.isOutboundAll(item) // 所有数量已经出完了
        return !isOutboundAll && quantity > 0
      })
    },
    isOutboundAll (data) { // 判断是否物料的是否已经出料完成
      console.log(data.count, data.sentQuantity, data.count - data.sentQuantity)
      return !(data.count - data.sentQuantity)
    },
    onAccept (file) { // 限制发票文件上传的格式
      let { type } = file
      // console.log(size, 'file size')
      let isValid = isImage(type)
      if (!isValid) {
        this.$delay(() => this.$message.error('文件格式只能为图片'))
        return false
      }
      return true
    },
    getPictureList (val) {
      this.pictureList = val
      console.log(val, 'pictureList')
    },
    getMaterialPictureList (val, { row }) { // 申请退料
      row.pictureId = !val.length ? '' : val[0].pictureId
    },
    submit () {
      this.$refs.expressForm.validate(isValid => {
        if (!isValid) {
          return this.$message.error('表单数据未填写或未按要求规范填写')
        }
        console.log(this.pictureList, 'pictures')
        if (this.pictureList && !this.pictureList.length) {
          return this.$message.warning('至少上传一张图片！')
        }
      })
    },
    close () {
      this.$refs.expressForm.resetFields()
      this.$refs.expressForm.clearValidate()
      if (this.$refs.expressInfoPictures) {
        this.$refs.expressInfoPictures.clearFiles()
        this.pictureList = []
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
.add-express-info-wrapper {
  .input-number {
    &.el-input-number {
      ::v-deep {
        input {
          text-align: left !important;
        }
      }
    }
  }
  .material-content-wrapper {
    .table-wrapper {
      margin-top: 10px;
    }
    .line {
      display: inline-block;
      width: 4px;
      height: 18px;
      margin-right: 8px;
      background: #F8B500;
    }
    .text {
      font-size: 18px;
      font-weight: 800;
      color: #222222;
    }
  }
}
</style>