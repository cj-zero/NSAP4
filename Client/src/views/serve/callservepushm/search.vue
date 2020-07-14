<template>
  <el-form ref="form" :model="form" label-width="80px">
    <div style="padding:10px 0;"></div>
    <el-row :gutter="10">
   
      <el-col :span="4">
        <el-form-item label="服务ID" size="medium">
          <el-select  v-model="form.region" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>

      <el-col :span="4">
        <el-form-item label="工单ID" size="medium">
          <el-input  v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="呼叫状态" size="medium">
          <el-select  v-model="form.region" placeholder="请选择呼叫状态">
            <el-option label="待确认" value="1"></el-option>
            <el-option label="已确认" value="2"></el-option>
            <el-option label="已取消" value="3"></el-option>
          </el-select>
        </el-form-item>
      </el-col>

      <el-col :span="4">
        <el-form-item label="客户" size="medium">
          <el-input  v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
           <el-col :span="4">
        <el-form-item label="序列号" size="medium">
          <el-input  v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
              <el-col :span="4">
        <el-form-item label="接单员" size="medium">
          <el-input  v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
              
     
    </el-row>
    <el-row  :gutter="10">
              <el-col :span="4">
        <el-form-item label="技术员" size="medium">
          <el-input  v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
            <el-col :span="4">
        <el-form-item label="问题类型" size="medium">
          <el-select  v-model="form.region" placeholder="请选择呼叫状态">
            <el-option label="待确认" value="1"></el-option>
            <el-option label="已确认" value="2"></el-option>
            <el-option label="已取消" value="3"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
         <el-col :span="8">
        <el-form-item label="创建日期" size="medium">
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
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
export default {
  data() {
    return {
      form: {
        name: "",
        region: "",
        date1: "",
        date2: "",
        delivery: false,
        type: [],
        resource: "",
        desc: ""
      }
    };
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