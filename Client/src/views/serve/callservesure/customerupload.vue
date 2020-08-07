<template>
  <el-form label-width="100px"  disabled :model="form" class="demo-form-inline rowStyle">
    <div
      style="font-size:22px;text-align:center;padding-bottom:10px ; margin-bottom:10px ;border-bottom:1px solid silver;"
    >服务申请信息</div>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户代码">
          <el-input size="small" v-model="form.customerId"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="服务ID">
          <el-input size="small" v-model="form.id"></el-input>

        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="创建时间">
          <el-input size="small" v-model="form.createTime"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户名称">
          <el-input size="small" v-model="form.customerName"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="联系人">
          <el-input size="small" v-model="form.contacter"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="电话号码">
          <el-input size="small" v-model="form.contactTel"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <!-- <el-form-item label="地址">
          <el-input size="small" v-model="form.city"></el-input>
        </el-form-item> -->
              <el-form-item label="现地址">
                  <!-- <el-input size="mini" v-model="form.city"></el-input> -->
                  <p
                    style="border: 1px solid silver;color:silver; border-radius:5px;line-height:30px;overflow:hidden;height:30px;background-color:#F5F7FA;margin:0;padding-left:10px;font-size:12px;"
                  >{{allArea}}</p>
                </el-form-item>
      </el-col>
      <el-col :span="16">
        <el-form-item label>
          <el-input size="small" v-model="form.addr"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="24">
        <el-form-item label="服务内容">
          <el-input size="small" type="textarea" v-model="form.services"></el-input>
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
              <el-input size="small" v-model="item.manufSN"></el-input>
              <!-- <div style="border:1px solid #E4E7ED;background-color:#F5F7FA;border-radius:5px;padding-left:5px;">{{item.manufSN?item.manufSN:'暂无数据'}}</div> -->
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="物料编码">
              <el-input size="small" v-model="item.itemCode"></el-input>
            </el-form-item>
          </el-col>
     
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
  // props: { form: { type: Object, default: () => ({}) } },
props:['form'],
  data() {
    return {
      formV: {
        name: "",
        cardCode: ""
      },
     
    };
  },
  watch:{
    handler(val){
      console.log(val)
    }
  },
    computed: {
    allArea() {

      return this.form.province + this.form.city + this.form.area;
    },
  },
  mounted() {
  
  
  }
};
</script>

<style lang="scss" scoped>
.rowStyle{
  ::v-deep .el-form-item{
    margin-bottom:5px;
  }
}
</style>