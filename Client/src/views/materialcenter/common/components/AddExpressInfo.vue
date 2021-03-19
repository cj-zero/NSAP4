<template>
  <div 
    class="add-express-info-wrapper" 
    v-loading.fullscreen="loading"
    :element-loading-text="loadingText"
    element-loading-spinner="el-icon-loading"
    element-loading-background="rgba(0, 0, 0, 0.8)"
  >
    <el-form 
      ref="expressForm" 
      :model="info" 
      size="mini" 
      :label-width="labelWidth" 
      :show-message="false" 
      :rules="rules" 
      :class="{ 'my-form-view': orderType === 'quality' }"
    >
      <el-form-item label="快递单号" prop="trackNumber">
        <el-input style="width: 200px;" size="mini" v-model.trim="info.trackNumber" :disabled="!isReturnOrder"></el-input>
      </el-form-item>
       <!-- 退料 -->
      <el-form-item label="运送费用" prop="freight" v-if="isReturnOrder">
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
      <el-form-item label="图片" >
        <upLoadFile
          v-if="isReturnOrder"
          @get-ImgList="getPictureList" 
          ref="expressInfoPictures"
          :onAccept="onAccept"
        ></upLoadFile>
        <img-list v-else :imgList="imgList"></img-list>
      </el-form-item>
      <el-form-item label="最后一次领料" v-if="isReturnOrder">
         <el-checkbox v-model="info.isLastReturn"></el-checkbox>
      </el-form-item>
    </el-form>
    <div class="material-content-wrapper">
      <el-row type="flex" align="middle">
        <i class="line"></i>
        <h2 class="text">物料信息</h2>
      </el-row>
      <el-form :model="newFormData" ref="newForm" :show-message="false" size="mini">
        <common-table 
          v-loading="materialLoading"
          class="table-wrapper"
          ref="addMaterialColumn"
          style="min-height: 200px;"
          max-height="350px"
          :data="newFormData.materialList"
          :columns="materialColumns">
          <!-- 退料 -->
          <template v-slot:returnQty="{ row }">
            <el-input-number 
              size="mini"
              :controls="false"
              v-model="row.returnQty"
              :min="0"
              :max="row.surplusQty"
              style="width: 100%;"
              :disabled="isOutboundAll(row)"
            ></el-input-number>
          </template>
          <template v-slot:upload="{ row }">
            <UpLoadFile 
              v-if="row.surplusQty !== 0 && isReturnOrder"
              uploadType="file" 
              @get-ImgList="getMaterialPictureList" 
              ref="materialPicture"
              :onAccept="onAccept"
              :options="{ row }"
              :limit="1"
            />
            <el-button v-if="orderType === 'quality'" size="mini" class="customer-btn-class" @click="showMaterialPicture(row)">查看</el-button>
          </template>
          <!-- 良品 -->
          <template v-slot:goodQty="{ row, index, prop }">
            <el-form-item :prop="'materialList.' + index + '.' + prop" :rules="newFormRules[prop]">
              <el-input-number
                style="width: 100%;"
                v-model="row.goodQty" 
                :controls="false" 
                :precision="0"
                :min="0"
                :max="row.count"
              ></el-input-number>
            </el-form-item>
          </template>
          <template v-slot:secondQty="{ row, index, prop }">
            <el-form-item :prop="'materialList.' + index + '.' + prop" :rules="newFormRules[prop]">
              <el-input-number 
                style="width: 100%;"
                v-model="row.secondQty" 
                :controls="false" 
                :precision="0"
                :min="0"
                :max="row.count"
              ></el-input-number>
            </el-form-item>
          </template>
        </common-table>
      </el-form>
      <el-image-viewer
        v-if="previewVisible"
        :zIndex="99999"
        :url-list="previewImageUrlList"
        :on-close="closeViewer"
      >
      </el-image-viewer>
    </div>
  </div>
</template>

<script>
import UpLoadFile from '@/components/upLoadFile'
import { isImage } from '@/utils/file'
import { getQuotationMaterialCode, returnMaterials, checkOutMaterials } from '@/api/material/returnMaterial'
import { SHOW_STORAGE_COLUMNS_TO_TEST, ADD_QUALITY_COLUMNS, ADD_RETURN_COLUMNS } from '../js/config'
import { processDownloadUrl } from '@/utils/file'
import ElImageViewer from 'element-ui/packages/image/src/image-viewer'
import ImgList from '@/components/imgList'
const LOADING_TEXT_MAP = {
  returnOrder: '退料中',
  quality: '检测中',
  storage: '入库中'
}
function freightValidator (rule, value, callback) {
  value = Number(value)
  value > 0 ? callback() : callback(new Error('运费必须大于0'))
}
export default {
  components: {
    UpLoadFile,
    ElImageViewer,
    ImgList
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
    },
    expressList: {
      type: Array,
      default () {
        return []
      }
    }
  },
  computed: {
    loadingText () {
      return LOADING_TEXT_MAP[this.orderType]
    },
    newFormData () {
      return {
        materialList: this.materialList
      }
    },
    imgList () {
      return  this.expressList.length ?  this.expressList[0].expressagePicture : []
    },
    isReturnOrder () { // 判断是否自提
      return this.orderType === 'returnOrder'
    },
    rules () {
      return {
        trackNumber: [{ required: true }],
        freight: [{ required: true, validator: freightValidator }]
      }
    },
    labelWidth () {
      return this.orderType === 'returnOrder' ? '90px' : '90px'
    },
    materialColumns () {
      return this.orderType === 'returnOrder'
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
      loading: false,
      expressFormData: { // 新增快递信息
        number: '',
        freight: undefined,
        // remark: ''
      },
      returnFormData: { // 申请退料
        trackNumber: '', // 快递单号
        freight: undefined, // 运费
        isLastReturn: false,
      },
      qualityFormData: { // 品质检测
        trackNumber: ''
      },
      materialList: [],
      pictureList: [],
      materialLoading: false,
      newFormRules: {
        goodQty: [{ required: true, trigger: ['change', 'blur'] }],
        secondQty: [{ required: true, trigger: ['change', 'blur'] }]
      },
      previewImageUrlList: [],
      previewVisible: false
    }
  },
  methods: {
    isOutboundAll (data) { // 判断是否物料的是否已经出料完成
      return !data.surplusQty
    },
    validateReturnMaterial () { // 校验退料的
      // 只要有一个物料没有全部出库并且出库数量时大于0的 就可以提交 
      return this.materialList.some(item => { 
        let { returnQty, pictureId } = item
        let isOutboundAll = this.isOutboundAll(item) // 所有数量已经出完了
        return !isOutboundAll && returnQty > 0 && pictureId
      })
    },
    validateQualityMaterial () {
      let isValid = true, index = -1
      for (let i = 0; i < this.materialList.length; i++) {
        const { count, goodQty = 0, secondQty = 0 } = this.materialList[i]
        if (goodQty + secondQty !== count) {
          index = i + 1
          isValid = false
          break
        }
      }
      return { isValid, index }
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
    showMaterialPicture (row) { // 查看物料图片
      console.log(row, 'row')
      let { pictureId } = row
      if (!pictureId) {
        return this.$message.warning('暂无快递图片')
      }
      let pictureList = [processDownloadUrl(pictureId)]
      this.previewImageUrlList = pictureList
      this.previewVisible = true
    },
    closeViewer () {
      this.previewVisible = false
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
        if (this.pictureList && !this.pictureList.length && this.isReturnOrder) {
          return this.$message.warning('至少上传一张快递图片！')
        }
        console.log(this.pictureList, 'pictures')
        this.orderType === 'returnOrder' ? this._returnMaterials() : this._checkOutMaterials()
      })
    },
    _checkOutMaterials () { // 品质检验
      // const { expressageId, returnNoteId } = this.info
      this.$refs.newForm.validate(async isValid => {
        if (!isValid) {
          return this.$message.error('待检物料的良品和次品必须全部填写')
        } 
        const { isValid: isAllValid, index } = this.validateQualityMaterial()
        if (!isAllValid) {
          return this.$message.error(`良品和次品的数量相加必须等于退回数量，请检查第${index}行`)
        }
        const { id: expressageId, returnNoteId } = this.expressInfo || {}
        const params = {
          expressageId,
          returnNoteId
        }
        params.checkOutMaterials = this.materialList.map(item => {
          const { id, goodQty, secondQty } = item
          return { id, goodQty, secondQty }
        })
        params.detailIds = this.materialList.map(item => item.id)
        console.log('success', params)
        this.loading = true
        try { 
          const { data } = await checkOutMaterials(params)
          this.$emit('addExpressInfo', data)
          this.$message.success('检测成功')
          this.parentVm._getList()
          console.log(data, 'params')
        } catch (err) {
          this.$message.error(err.message)
        } finally {
          this.loading = false
        }
      })
    },
    _returnMaterials () { // 退料
      const { trackNumber, freight, isLastReturn } = this.info
      const params = {
        serviceOrderId: this.formData.serviceOrderId,
        sapId: this.formData.serviceSapId,
        trackNumber,
        freight,
        expressPictureIds: this.pictureList.map(item => item.pictureId),
        isLastReturn: isLastReturn ? 1 : 0,
      }
      const isOneValid = this.validateReturnMaterial() // 保证了至少一个物料出库
      if (!isOneValid) {
        return this.$message.warning('至少要有一个物料出库并且有对应的物料图片')
      }
      let isEveryValid = true, errorIndex = -1
      for (let i = 0; i < this.materialList.length; i++) {
        const { pictureId, returnQty } = this.materialList[i]
        if (returnQty) {
          if (!pictureId) {
            errorIndex = i + 1
            isEveryValid = false
            break
          }
        }
      }
      if (!isEveryValid) { // 有退料数量必须传图片
        return this.$message.warning(`第${errorIndex}行出库的物料，图片为必传项`)
      }
      params.stockOutIds = this.materialList
        .map(item => item.quotationId)
        .filter((quotationId, index, arr) => arr.indexOf(quotationId) === index )
      params.returnMaterialDetail = this.materialList
        .map(item => {
          const { 
            returnQty, 
            count: totalQty,
            pictureId, 
            id, 
            materialCode, 
            materialDescription, 
            costPrice,
            surplusQty
          } = item
          console.log(item, 'item')
          return {
            quotationMaterialId: id,
            materialCode,
            materialDescription,
            returnQty,
            totalQty,
            pictureId,
            costPrice,
            surplusQty
          }
        })
      console.log(params, this.parentVm, 'vm', returnMaterials)
      this.loading = true
      console.log(returnMaterials, params.stockOutIds, 'stockOutIds',  this.materialList)
      returnMaterials(params).then(res => {
        const data = res.data
        console.log(data)
        this.$emit('addExpressInfo', data)
        const successText = this.orderType === 'returnOrder' ? '添加退料单' : ''
        this.$message.success(successText)
        this.parentVm._getList()
      }).catch(err => {
        this.$message.error(err.message)
      }).finally(() => this.loading = false)
    },
    close () {
      this.$refs.expressForm.resetFields()
      this.$refs.expressForm.clearValidate()
      this.reset()
      if (this.$refs.expressInfoPictures) {
        this.$refs.expressInfoPictures.clearFiles()
        this.pictureList = []
      }
    },
    async _getQuotationMaterialCode () {
      this.materialLoading = true
      try {
        const { data } = await getQuotationMaterialCode({ serviceOrderId: this.formData.serviceOrderId })
        this.materialList = data
      } catch (err) {
        this.materialList = []
        this.$message.error(err)
      } finally {
        this.materialLoading = false
      }
    },
    onOpened () {
      if (this.orderType === 'returnOrder') {
        this._getQuotationMaterialCode()
      } else if (this.orderType === 'quality') {
        if (this.expressList.length) {
          const { materialList, expressNumber } = this.expressList[0]
          this.materialList = materialList || []
          this.qualityFormData.trackNumber = expressNumber
          this.expressInfo = this.expressList[0]
        }
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