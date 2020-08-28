<template>
  <div class="report-wrapper">
    <el-form
      inline
      :label-width="labelWidth"
      size="mini"
    >
      <el-row type="flex" justify="space-around">
        <el-col :span="6">
          <el-form-item label="服务ID">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="客户代码">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="客户名称">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="技术员">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="6">
          <el-form-item label="终端代码">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="终端客户">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="联系人">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="电话号码">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around"> 
        <el-col :span="6">
          <el-form-item label="出发地点">
            <el-input></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="到达地点">
            <el-input></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="出差时间">
            <el-input></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="结束时间">
            <el-input></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" justify="space-around">
        <el-col :span="6">
          <el-form-item label="完工地址">
            <el-input diabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item>
            <el-input diabled slot="append" icon="el-icon-position"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="出差天数">
            <el-input disabled></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row>
        <el-table
          :data="tableData"
          size="mini"
          border
          max-height="200"
        >
          <el-table-column
            v-for="item in headOptions"
            :key="item"
            :label="item.name"
            :width="item.width"
            :prop="item.prop"
          ></el-table-column>
        </el-table>
      </el-row>
      <el-row type="flex" align="middle">
        <span class="pic-title">图片</span>
        <div class="pic-list">
          <UpLoadFile :limit="picLimit" />
        </div>
      </el-row>
    </el-form>
  </div>
  
</template>

<script>
import UpLoadFile from '@/components/upLoadFile'
export default {
  components: {
    UpLoadFile
  },
  props: {
    type: {
      type: String,
      default: ''
    },
    data: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {
      labelWidth: '70px',
      formData: {},
      headOptions:[
        { name: '工单ID', prop: 'serviceOrderId' },
        { name: '制造商序列号', prop: 'zz' },
        { name: '物料编码', prop: 'wuliao' }
      ],
      tableData: [
        { serviceOrderId: 1, zz: 'CT-ZZJS-SD2', wuliao: 'CRSD=0DSD' },
        { serviceOrderId: 1, zz: 'CT-ZZJS-SD2', wuliao: 'CRSD=0DSD' },
        { serviceOrderId: 1, zz: 'CT-ZZJS-SD2', wuliao: 'CRSD=0DSD' }
      ],
      picLimit: 9,
    }
  },
  methods: {

  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.report-wrapper {
  .pic-list {
    flex: 1;
  }
}
</style>