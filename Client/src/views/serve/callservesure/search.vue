<template>
  <el-form ref="listQuery" :model="listQuery" label-width="80px">
    <div style="padding:10px 0;"></div>
    <el-row :gutter="10">
   
      <el-col :span="4">
        <el-form-item label="服务ID" size="medium">
          <el-select  v-model="listQuery.QryServiceOrderId" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>

      <el-col :span="4">
        <el-form-item label="呼叫状态" size="medium">
          <el-select  v-model="listQuery.QryState" placeholder="请选择呼叫状态">
                        <el-option label="全部" value=""></el-option>

            <el-option label="待确认" value="1"></el-option>
            <el-option label="已确认" value="2"></el-option>
            <el-option label="已取消" value="3"></el-option>
          </el-select>
        </el-form-item>
      </el-col>

      <el-col :span="4">
        <el-form-item label="客户" size="medium">
          <el-input  v-model="listQuery.QryCustomer"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="序列号" size="medium">
          <el-input  v-model="listQuery.QryManufSN"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="创建日期" size="medium">
          <el-col :span="11">
            <el-date-picker
              type="date"
              placeholder="选择开始日期"
              v-model="listQuery.QryCreateTimeFrom"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
          <el-col class="line" :span="2">至</el-col>
          <el-col :span="11">
            <el-date-picker
              type="date"
              placeholder="选择结束时间"
              v-model="listQuery.QryCreateTimeTo"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
        </el-form-item>
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
export default {
  props:['form1'],
  data() {
    return {
      form: {
     page: 1,
        limit: 20,
        key: undefined,
        appId: undefined,
        QryServiceOrderId:"",//查询服务ID查询条件
        QryState:"",//呼叫状态查询条件
        QryCustomer :"",//客户查询条件
        QryManufSN :"",// 制造商序列号查询条件
        QryCreateTimeFrom:"",//创建日期从查询条件
        QryCreateTimeTo:"",//创建日期至查询条件
      }
    };
  },
  watch:{
    listQuery:{
      deep:true,
      handler(){
       let listQuery= this.form
      this.$emit('update:listQuery',listQuery)
      }
    }
  },
  methods: {
    // onSubmit() {
    //   console.log("submit!");
    // }}
}
}
</script>

<style>
</style>