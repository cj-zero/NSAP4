<template>
  <div class="report-wrapper">
    <div class="tab-list">
      <el-tabs v-model="activeName" type="card" @tab-click="handleClick">
        <el-tab-pane 
          v-for="(item, index) in data"
          :key="item.id"
          :label="item.materialCodeTypeName"
          :name="String(index)"
          :disabled="!item.id"
          :lazy="true"
        ></el-tab-pane>
      </el-tabs>
    </div>
    <el-form
      :label-width="labelWidth1"
      size="mini"
      label-position="left"
      :module="showData"
    >
      <el-row type="flex" justify="space-around">
        <!-- <el-col :span="6"> -->
          <el-form-item label="服务ID">
            <el-input readonly v-model="showData.u_SAP_ID"></el-input>
          </el-form-item>
        <!-- </el-col> -->
        <!-- <el-col :span="6"> -->
          <el-form-item label="客户代码">
            <el-input readonly v-model="showData.customerId"></el-input>
          </el-form-item>
        <!-- </el-col> -->
        <!-- <el-col :span="6"> -->
          <el-form-item label="客户名称">
            <el-input readonly  v-model="showData.customerName"></el-input>
          </el-form-item>
        <!-- </el-col> -->
        <!-- <el-col :span="6"> -->
          <el-form-item label="技术员">
            <el-input readonly v-model="showData.technicianName"></el-input>
          </el-form-item>
        <!-- </el-col> -->
      </el-row>
      <el-row type="flex" justify="space-around">
        <!-- <el-col :span="6"> -->
          <el-form-item label="终端代码">
            <el-input readonly v-model="showData.terminalCustomerId"></el-input>
          </el-form-item>
        <!-- </el-col> -->
        <!-- <el-col :span="6"> -->
          <el-form-item label="终端客户">
            <el-input readonly v-model="showData.terminalCustomer"></el-input>
          </el-form-item>
        <!-- </el-col> -->
        <!-- <el-col :span="6"> -->
          <el-form-item label="联系人">
            <el-input readonly v-model="showData.contacter"></el-input>
          </el-form-item>
        <!-- </el-col> -->
        <!-- <el-col :span="6"> -->
          <el-form-item label="电话号码">
            <el-input readonly v-model="showData.contactTel"></el-input>
          </el-form-item>
        <!-- </el-col> -->
      </el-row>
      <el-row type="flex" justify="space-around" v-if="!showData.isPhoneService"> 
        <el-col :span="6">
          <el-form-item label="出发地点">
            <el-input readonly v-model="showData.becity"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="到达地点">
            <el-input readonly v-model="showData.destination"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="出差时间">
            <el-input readonly v-model="showData.businessTripDate"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="结束时间">
            <el-input readonly v-model="showData.endDate"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-between">
        <el-col :span="12">
          <template v-if="showData.serviceWorkOrders && showData.serviceWorkOrders.length">
            <common-table
              :data="showData.serviceWorkOrders"
              :columns="headOptions"
              max-height="150px"
            >
            </common-table>
          </template>
        </el-col>
        <el-col :span="6" v-if="!showData.isPhoneService">
          <el-form-item label="出差天数">
            <el-input readonly v-model="showData.businessTripDays"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" align="middle" class="img-outer-wrapper" v-if="showData.files && showData.files.length">
        <span class="pic-title">图片</span>
        <div class="pic-list">
          <img-list :imgList="showData.files"></img-list>
        </div>
      </el-row>
    </el-form>
    <el-form
      :module="showData"
      :label-width="labelWidth2"
      size="mini"
      label-position="left"
    >
      <el-row type="flex" justify="space-around">
        <el-col :span="6">
          <el-form-item label="售后问题类型">
            <el-input readonly v-model="showData.troubleDescription"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="售后解决方法">
            <el-input readonly v-model="showData.processDescription"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="责任环节" label-width="70px">
            <el-input readonly v-model="showData.responsibility"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="服务方式" label-width="70px">
            <el-input readonly v-model="showData.serviceText"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row>
        <el-form-item label="更换物料明细">
          <el-input type="textarea" v-model="showData.replacementMaterialDetails" readonly></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="遗留问题">
          <el-input type="textarea" v-model="showData.legacy" readonly></el-input>
        </el-form-item>
      </el-row>
      <el-row>
        <el-form-item label="备注">
          <el-input type="textarea" v-model="showData.remark" readonly></el-input>
        </el-form-item>
      </el-row>
    </el-form>
  </div>
</template>

<script>
// import UpLoadFile from '@/components/upLoadFile'
import ImgList from '@/components/imgList'
export default {
  components: {
    ImgList
  },
  props: {
    type: {
      type: String,
      default: ''
    },
    data: {
      type: Array,
      default () {
        return []
      }
    }
  },
  computed: {
    showData () {
      return this.data[this.activeName] || []
    }
  },
  data () {
    return {
      labelWidth1: '70px',
      labelWidth2: '100px',
      activeName: '0',
      headOptions:[
        { label: '工单ID', prop: 'workOrderNumber' },
        { label: '制造商序列号', prop: 'manufacturerSerialNumber' },
        { label: '物料编码', prop: 'materialCode' }
      ]
    }
  },
  updated () {
    console.log(this.data, 'updated')
  },
  methods: {
    handleClick (val) {
      console.log(val)
    },
    reset () {
      this.activeName = '0' // 关闭弹窗时 重置activeName状态，默认从第一个开始
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.report-wrapper {
  ::v-deep .el-form-item--mini.el-form-item{
    margin-bottom: 5px;
  }
  .img-outer-wrapper {
    margin: 10px auto;
    .pic-title {
      margin-left: 10px;
    }
  }
  .pic-list {
    flex: 1;
  }
  ::v-deep .el-form-item__label {
    padding-left: 10px;
  }
}
</style>