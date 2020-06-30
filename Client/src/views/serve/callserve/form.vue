<template>
  <el-form :model="form" :ref="refValue" :label-width="labelwidth">
    <div style="padding:10px 0;"></div>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户代码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
               <el-form-item label="服务ID">
          <el-select v-model="form.region" disabled placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>

      </el-col>
      <el-col :span="8">
               <el-form-item label="服务合同">
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>

      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户名称">
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>
        
      </el-col>
      <el-col :span="8">

                <el-form-item label="联系人">
          <el-select v-model="form.region" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="8">
                  <el-form-item label="最近联系人">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
 
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="终端客户">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="最新电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>

    <!-- 选择制造商序列号 -->
      <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="制造商序列号">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="内部序列号">
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="保修结束日期">
              <el-date-picker
              disabled
              size="mini"
              type="date"
              placeholder="选择开始日期"
              v-model="form.startTime"
              style="width: 100%;"
            ></el-date-picker>
        </el-form-item>
      </el-col>
    </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="物料编码">
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="16">
          <el-form-item label="物料描述">
          <el-input disabled v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>

      <el-form-item label="服务类型" prop="resource">
    <el-radio-group v-model="form.name">
      <el-radio label="免费"></el-radio>
      <el-radio label="收费"></el-radio>
    </el-radio-group>
  </el-form-item>

          <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="16">
        <el-form-item label="呼叫主题">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="接单员">
          <el-input disabled v-model="form.name" ></el-input>
        </el-form-item>
      </el-col>
    </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="呼叫来源">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="呼叫状态">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="最新电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="终端客户">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="最新电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="终端客户">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
          <el-form-item label="最新电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
     <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="地址标识">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="16">
          <el-input v-model="form.name"></el-input>
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
export default {
  props: ["modelValue", "refValue", "labelposition", "labelwidth"],
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
  handleUpdate(row) {
    // 弹出编辑框
    this.temp = Object.assign({}, row); // copy obj
    this.dialogStatus = "update";
    this.dialogFormVisible = true;
    this.$nextTick(() => {
      this.$refs["dataForm"].clearValidate();
    });
  },
  methods: {
    onSubmit() {
      console.log("submit!");
    }
  },
  handleCreate() {
    // 弹出添加框
    this.resetTemp();
    this.dialogStatus = "create";
    this.dialogFormVisible = true;
    this.$nextTick(() => {
      this.$refs["dataForm"].clearValidate();
    });
  }
};
</script>

<style>
</style>