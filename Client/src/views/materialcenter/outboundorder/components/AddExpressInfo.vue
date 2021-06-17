<template>
  <div class="add-express-info-wrapper" v-loading.fullscreen="expressLoading">
    <el-form ref="expressForm" :model="expressFormData" size="mini" label-width="80px" :show-message="false" :rules="rules" v-if="isExpressage">
      <el-form-item :label="orderLabel" prop="number">
        <el-input style="width: 200px;" size="mini" v-model.trim="expressFormData.number" :disabled="isPickUp"></el-input>
      </el-form-item>
      <el-form-item label="运送费用" prop="freight" v-if="!isPickUp">
        <el-input-number 
          class="input-number" 
          style="width: 200px;" 
          size="mini" 
          placeholder="金额大于等于零"
          v-model="expressFormData.freight" 
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
        <template v-slot:whsCode="{ row }">
          <div class="bold" v-infotooltip.ellipsis>{{ row.whsCode }}</div>
        </template>
        <template v-slot:quantity="{ row }">
          <el-input-number 
            size="mini"
            :controls="false"
            v-model="row.quantity"
            :min="0"
            :max="calcMaxCount(row)"
            style="width: 100%;"
            :disabled="isOutboundAll(row)"
          >
          </el-input-number>
        </template>
      </common-table>
    </div>
  </div>
</template>

<script>
import UpLoadFile from '@/components/upLoadFile'
import { updateOutboundOrder, getMergeMaterial, printPickingList } from '@/api/material/quotation'
import { isImage } from '@/utils/file'
// import { print } from '@/utils/utils'
// import { getSign } from '@/api/users'
import { serializeParams } from '@/utils/process'
const NOT_MATERIAL_CODE_LIST = ['S111-SERVICE-CLF', 'S111-SERVICE-GSF'] // 维修费 服务费
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
    isExpressage: { // 判断是不是新增快递
      type: Boolean,
      default: true
    },
    formData: {
      type: Object,
      default () {
        return {}
      }
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
  },
  data () {
    return {
      expressLoading: false,
      expressFormData: { // 新增快递信息
        number: '',
        freight: undefined,
        // remark: ''
      },
      materialList: [],
      pictureList: [],
      materialColumns: [
        { label: '序号', type: 'index' },
        { label: '物料编码', prop: 'materialCode' },
        { label: '物料描述', prop: 'materialDescription' },
        { label: '总数量', prop: 'count', align: 'right' },
        { label: '单位', prop: 'unit', align: 'right' },
        { label: '仓库', prop: 'whsCode', align: 'right', slotName: 'whsCode' },
        { label: '库存数量', prop: 'warehouseQuantity', align: 'right' },
        { label: '已出库', prop: 'sentQuantity', align: 'right' },
        { label: '出库数量', prop: 'quantity', slotName: 'quantity' }
      ],
      materialLoading: false
    }
  },
  methods: {
    getMergeMaterial () {
      this.materialLoading = true
      getMergeMaterial({ quotationId: this.formData.id }).then(res => {
        this.materialList = res.data.map(item => {
          const { sentQuantity, count, warehouseQuantity, materialCode } = item
          item.sentQuantity = Number(item.sentQuantity)
          const diff = count - sentQuantity
          item.quantity = NOT_MATERIAL_CODE_LIST.indexOf(materialCode) > - 1 ? diff : (Number(diff <= warehouseQuantity ? diff : warehouseQuantity))
          return item
        })
        this.materialLoading = false
      }).catch(err => {
        this.$message.error(err.message)
        this.materialLoading = false
      })
    },
    calcMaxCount (row) {
      const { count, sentQuantity, warehouseQuantity, materialCode } = row
      const diff = count - sentQuantity
      return NOT_MATERIAL_CODE_LIST.indexOf(materialCode) > - 1 ? diff : (diff <= warehouseQuantity ? diff : warehouseQuantity)
    },
    validateMaterial () { // 只要有一个物料没有全部出库并且出库数量时大于0的 就可以提交
      return this.materialList.some(item => { 
        let { quantity } = item
        let isOutboundAll = this.isOutboundAll(item) // 所有数量已经出完了
        return !isOutboundAll && quantity > 0
      })
    },
    isOutboundAll (data) { // 判断是否物料的是否已经出料完成
      const { count, sentQuantity, warehouseQuantity, materialCode } = data
      const diff = count - sentQuantity
      return NOT_MATERIAL_CODE_LIST.indexOf(materialCode) > - 1 ? diff === 0 : ((diff === 0) || (diff && warehouseQuantity === 0))
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
    submit () {
      this.$refs.expressForm.validate(isValid => {
        if (!isValid) {
          return this.$message.error('表单数据未填写或未按要求规范填写')
        }
        console.log(this.pictureList, 'pictures')
        if (this.pictureList && !this.pictureList.length) {
          return this.$message.warning('至少上传一张图片！')
        }
        this.operate()
      })
    },
    async operate () {
      const text = this.isExpressage ? '出库' : '打印'
      if (!this.validateMaterial()) {
          return this.$message.warning(`至少${text}一个物料！`)
        }
        let quotationMergeMaterialReqs = this.materialList.map(item => {
          let { id, quantity } = item
          return {id, sentQuantity: quantity, quotationId: this.formData.id }
        }).filter(item => item.sentQuantity)
        let expressageReqs = {
          quotationId: this.formData.id,
          expressagePictures: this.pictureList.map(item => item.pictureId),
          expressNumber: this.expressFormData.number,
          freight: this.expressFormData.freight
        }
        let params = this.isExpressage  
          ? { quotationMergeMaterialReqs, expressageReqs } 
          : { quotationMergeMaterialReqs }

        if (!this.isExpressage) { // 如果是打印操作
          this.expressLoading = true
          try {
            // const { data } = await getSign({ serialNumber: this.formData.id, timespan: NOW_DATE })
            await printPickingList(params.quotationMergeMaterialReqs)
            const url = '/Material/Quotation/PrintPicking'
            const printParams = { serialNumber: this.formData.id, 'X-token': this.$store.state.user.token, isTrue: false }
            window.open(`${process.env.VUE_APP_BASE_API}${url}?${serializeParams(printParams)}`)
            this.formData.row.printWarehouse = 3
          } catch (err) {
            this.$message.error(err.message)
          } finally {
            this.expressLoading = false
          }
          return
        }
        console.log(params, 'params', this.parentVm.expressDialogLoading, updateOutboundOrder)
        // 新增物料操作
        this.parentVm.expressDialogLoading = true
        updateOutboundOrder({
          quotationMergeMaterialReqs,
          expressageReqs
        }).then(res => {
          console.log(res.data, 'res.data')
          this.$emit('addExpressInfo', res.data)
          this.parentVm.closeExpressDialog()
          this.parentVm.expressDialogLoading = false
        }).catch(err => {
          this.$message.error(err.message)
          this.parentVm.expressDialogLoading = false
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