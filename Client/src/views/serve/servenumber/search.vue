<template>
  <el-form ref="form" :model="form" label-width="80px">
    <div style="padding:10px 0;"></div>
    <el-row :gutter="10">
   
      <el-col :span="4">
        <el-form-item label="序列号" size="medium">

          <el-select  v-model="form.region" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="4">
        <el-form-item label="客户" size="medium">
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
        <el-form-item label="物料编码" size="medium">
          <el-input  v-model="form.name"></el-input>
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