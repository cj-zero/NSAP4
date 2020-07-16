<template>
  <el-form label-width="100px" disabled :model="form" class="demo-form-inline">
    <div
      style="font-size:22px;text-align:center;padding-bottom:10px ; margin-bottom:10px ;border-bottom:1px solid silver;"
    >服务申请信息</div>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户代码">
          <el-input v-model="form.customerId"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="服务ID">
          <el-input v-model="form.id"></el-input>

          <!-- <el-select v-model="form.id" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>-->
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="创建时间">
          <el-input v-model="form.createTime"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户名称">
          <el-input v-model="form.customerName"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="联系人">
          <el-input v-model="form.contacter"></el-input>

          <!-- <el-select v-model="form.name" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>-->
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="电话号码">
          <el-input v-model="form.contactTel"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="地址">
          <el-input v-model="form.city"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="16">
        <el-form-item label>
          <el-input v-model="form.addr"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="24">
        <el-form-item label="服务内容">
          <el-input type="textarea" v-model="form.serviceWorkOrders"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row
      type="flex"
      class="row-bg"
      justify="space-around"
      v-for=" (item,index) in form.serviceOrderSNs"
      :key="`inx${index}`"
    >
      <el-col :span="16">
        <el-row type="flex" class="row-bg" justify="space-around">
          <el-col :span="12">
            <el-form-item label="制造商序列号">
              <el-input v-model="item.manufSN"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="物料编码">
              <el-input v-model="item.itemCode"></el-input>
            </el-form-item>
          </el-col>
          <!-- <el-col :span="12">
            <el-form-item label="物料编码">
              <el-input v-model="item.itemCode"></el-input>
            </el-form-item>
          </el-col>-->
        </el-row>
      </el-col>
      <el-col :span="8">
        <el-form-item label>
          <span></span>
        </el-form-item>
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
export default {
  props: {form:
    {   type: Object,
      default: () => ({})}
  }
    
  ,
  data() {
    return {
      formV: {
        name: "",
        cardCode: ""
      }
    };
  },
  watch: {
    form: function(val) {
      console.log(val);
    }
  },
  mounted() {
    // console.log(1,this.formVal)
  }
};
</script>

<style>
</style>