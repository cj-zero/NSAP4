<template>
  <el-form ref="form" :model="form" label-width="80px">
    <div style="padding:10px 0;"></div>
    <el-row :gutter="10">
      <el-col :span="3">
        <el-form-item label="服务ID">
          <el-input size="small"  v-model="form.QryServiceOrderId" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="工单ID">
          <el-input size="small" v-model="form.QryServiceWorkOrderId" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="呼叫状态">
          <el-select size="small" clearable v-model="form.QryState" placeholder="请选择呼叫状态">
            <el-option
              v-for="(item,index) in callStatus"
              :key="index"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="客户">
          <el-input size="small" v-model="form.QryCustomer" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="序列号">
          <el-input size="small" v-model="form.QryManufSN" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="接单员">
          <el-input size="small" v-model="form.QryRecepUser" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="10">
      <el-col :span="3">
        <el-form-item label="技术员">
          <el-input size="small" v-model="form.QryTechName" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="问题类型">
          <el-input size="small" v-model="form.QryProblemType" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>

      <el-col :span="6">
        <el-row :gutter="3">
          <el-form-item label="创建日期">
            <el-col :span="11">
              <el-date-picker
                size="small"
                type="date"
                placeholder="选择开始日期"
                v-model="form.startTime"
                style="width: 100%;"
              ></el-date-picker>
            </el-col>
            <el-col class="line" :span="2">至</el-col>
            <el-col :span="11">
              <el-date-picker
                size="small"
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
          <!-- <el-button size="middle" type="primary" @click="onSubmit"> 搜 索 </el-button> -->
                              <el-button type="primary" @click="onSubmit" size="small" icon="el-icon-search"> 搜 索 </el-button>

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
        QryServiceOrderId: "", // 查询服务ID查询条件
        QryState: "", // 呼叫状态查询条件
        QryCustomer: "", // 客户查询条件
        QryManufSN: "", //  制造商序列号查询条件
        QryCreateTimeFrom: "", // 创建日期从查询条件
        QryCreateTimeTo: "", //  创建日期至查询条件
        QryRecepUser: "", //  接单员
        QryTechName: "", // - 工单技术员
        QryProblemType: "", //QryProblemType - 问题类型
        QryMaterialTypes: "", //物料类别（多选)
      },
      callStatus: [
        { value: 1, label: "待处理" },
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "已外出" },
        { value: 5, label: "已挂起" },
        { value: 6, label: "已接收" },
        { value: 7, label: "已解决" },
        { value: 8, label: "已回访" }
      ]
    };
  },
  watch: {
    form: {
      deep: true,
      handler(val) {
        this.$emit("change-Search", val);
      }
    }
  },
  methods: {
    onSubmit() {
      this.$emit("change-Search", 1);
    }
  }
};
</script>

<style>
</style>