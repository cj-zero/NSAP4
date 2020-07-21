<template>
  <el-form ref="form" :model="form" label-width="80px">
    <div style="padding:10px 0;"></div>
    <el-row :gutter="10">
      <el-col :span="4">
        <el-form-item label="服务ID">
                    <el-input v-model="form.QryServiceOrderId"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="工单ID">
       <el-input v-model="form.QryServiceWorkOrderId"></el-input>


        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="呼叫状态">
          <el-select v-model="form.QryState" placeholder="请选择呼叫状态">
            <el-option label="状态一" value="shanghai"></el-option>
            <el-option label="状态二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="客户">
          <el-input v-model="form.QryCustomer"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="序列号">
          <el-input v-model="form.QryManufSN"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="10">
      <el-col :span="4">
        <el-form-item label="接单员">
          <el-input v-model="form.QryRecepUser"></el-input>
        </el-form-item>
      </el-col>
     
      <el-col :span="4">
        <el-form-item label="技术员">
          <el-input v-model="form.QryTechName"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="问题类型">
          <el-input v-model="form.QryProblemType"></el-input>
        </el-form-item>
      </el-col>

      <el-col :span="8">
          <el-row :gutter="3">
        <el-form-item label="创建日期">
            
          <el-col :span="11">
            <el-date-picker
              type="date"
              placeholder="选择开始日期"
              v-model="form.startTime"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
          <el-col class="line" :span="2">至</el-col>
          <el-col :span="11">
            <el-date-picker
              type="date"
              placeholder="选择结束时间"
              v-model="form.endTime"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
              
        </el-form-item>
        </el-row>
      </el-col>
      <el-col :span="4">
        <el-form-item>
          <el-button  type="primary" @click="onSubmit">搜索</el-button>
        </el-form-item>
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
export default {
  data() {
    return {
      form: {
      QryServiceOrderId :'',// 查询服务ID查询条件
      QryState :'',// 呼叫状态查询条件
      QryCustomer :'',// 客户查询条件
      QryManufSN:'',//  制造商序列号查询条件
      QryCreateTimeFrom :'',// 创建日期从查询条件
      QryCreateTimeTo :'',//  创建日期至查询条件
      QryRecepUser :'',//  接单员
      QryTechName:'',// - 工单技术员
      QryProblemType:'',//QryProblemType - 问题类型
      QryMaterialTypes:'',//物料类别（多选)
      page:1,//
      limit:20,
      key:'',
    }
  }
  },
    watch: {
    form: {
      deep: true,
      handler(val) {
        console.log(val)
        this.$emit("change-Search", val);
      }
    }
  },
  methods: {
    onSubmit() {
      this.$emit("change-Search", 1);
    }}
}
</script>

<style>
</style>