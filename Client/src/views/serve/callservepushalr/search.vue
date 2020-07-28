<template>
  <el-form ref="form" :model="listQuery" label-width="80px">
    <div style="padding:10px 0;"></div>
    <el-row :gutter="10">
      <el-col :span="3">
        <el-form-item label="服务ID" size="small">
          <el-input v-model="listQuery.QryServiceOrderId"></el-input>
        </el-form-item>
      </el-col>

      <el-col :span="3">
        <el-form-item label="工单ID" size="small">
          <el-input v-model="listQuery.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="呼叫状态" size="small">
          <el-select v-model="listQuery.QryState" clearable placeholder="请选择呼叫状态">
            <el-option
              v-for="(item,index) in callStatus"
              :key="index"
               :disabled="item.disabled"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-col>

      <el-col :span="3">
        <el-form-item label="客户" size="small">
          <el-input v-model="listQuery.QryCustomer"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="序列号" size="small">
          <el-input v-model="listQuery.QryManufSN"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="接单员" size="small">
          <el-input v-model="listQuery.QryRecepUser"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="10">
      <el-col :span="3">
        <el-form-item label="技术员" size="small">
          <el-input v-model="listQuery.QryTechName"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="问题类型" size="small">
          <el-select v-model="listQuery.QryProblemType" size="small" placeholder="选择呼叫状态">
            <el-option label="待确认" value="1"></el-option>
            <el-option label="已确认" value="2"></el-option>
            <el-option label="已取消" value="3"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="6">
        <el-form-item label="创建日期" size="small">
          <el-col :span="11">
            <el-date-picker
            size="small"
           format="yyyy 年 MM 月 dd 日"   
              value-format="yyyy-MM-dd"
              placeholder="选择开始日期"
              v-model="listQuery.QryCreateTimeFrom"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
          <el-col class="line" :span="2">至</el-col>
          <el-col :span="11">
            <el-date-picker
               format="yyyy 年 MM 月 dd 日"   
              value-format="yyyy-MM-dd"
              placeholder="选择结束时间"
              v-model="listQuery.QryCreateTimeTo"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
        </el-form-item>
      </el-col>

      <el-col :span="8" style="margin-left:20px;" >
                    <el-button type="primary" @click="onSubmit" size="small" icon="el-icon-search"> 搜 索 </el-button>
      </el-col>

            <el-col :span="2" style="margin-left:20px;" >

            <el-button type="success"  size="small" @click="sendOrder" icon="el-icon-thumb"> 转 派 </el-button>
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
export default {
  data() {
    return {
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined,
        Name: "", //	Description
        QryServiceOrderId: "", //- 查询服务ID查询条件
        QryState: '', //- 呼叫状态查询条件
        QryCustomer: "", //- 客户查询条件
        QryManufSN: "", // - 制造商序列号查询条件
        QryCreateTimeFrom: "", //- 创建日期从查询条件
        QryCreateTimeTo: "", //- 创建日期至查询条件
        QryRecepUser: "", //- 接单员
        QryTechName: "", // - 工单技术员
        QryProblemType: "" // - 问题类型
      },
// 1.待处理-呼叫中心处理（不可选）
// 2.已排配-技术员接单
// 3.已预约-技术员电话预约后
// 4.已外出-技术员核对设备正确后
// 5.已挂起-技术员转交给研发测试
// 6.已接收-研发测试发反馈报告（不可选）
// 7.已解决-技术员点击了完成工单（不可选）
// 8.已回访-app自动回访&呼叫中心电话回访（不可选）
               callStatus: [
        { value: 1, label: "待处理" , disabled: true},
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "已外出" },
        { value: 5, label: "已挂起" },
        { value: 6, label: "已接收" , disabled: true},
        { value: 7, label: "已解决" , disabled: true},
        { value: 8, label: "已回访" , disabled: true}
      ],
    };
  },
  methods: {
    onSubmit() {
        this.$emit("change-Search", 1);
    },
    sendOrder(){
      // console.log(11)
      this.$emit("change-Order",true)
    },
  },
  watch: {
    listQuery: {
      deep: true,
      immediate:true,
      handler(val) {
        this.$emit("change-Search", val);
      }
    }
  }
};
</script>

<style>
</style>