<template>
  <el-form label-width="100px" disabled :model="form"  class="demo-form-inline" >
      <div style="font-size:22px;text-align:center;padding-bottom:10px ; margin-bottom:10px ;border-bottom:1px solid silver;">服务申请信息</div>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户代码">
          <el-input v-model="form.cardCode"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="服务ID">
          <el-select v-model="form.name" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="服务合同">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户代码">
          <el-input v-model="form.cardCode"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="服务ID">
          <el-select v-model="form.name" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="服务合同">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户代码">
          <el-input v-model="form.cardCode"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="16">
        <el-form-item label>
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="24">
        <el-form-item label="服务内容">
          <el-input type="textarea" v-model="form.desc"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="16">
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="12">
            <el-form-item label="制造商序列号">
              <el-input v-model="form.desc"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="物料编码">
              <el-input v-model="form.desc"></el-input>
            </el-form-item>
          </el-col>
        </el-row>
      </el-col>
      <el-col :span="8">
        <el-carousel height="150px">
          <el-carousel-item v-for="item in 4" :key="item">
            <h3 class="small">{{ item }}</h3>
          </el-carousel-item>
        </el-carousel>
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
        cardCode: ""
      }
    };
  }
};
</script>

<style>
</style>