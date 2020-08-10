<template>
  <el-form ref="form" :model="listQuery" label-width="70px" size='mini '>
    <div style="padding:10px 0;"></div>
    <el-row :gutter="4">
      <el-col :span="2">
        <el-form-item label="服务ID" >
          <el-input v-model="listQuery.QryServiceOrderId" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="呼叫状态" >
              <el-select v-model="listQuery.QryState" disabled  placeholder="请选择呼叫状态">
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
      <el-col :span="2">
        <el-form-item label="客户" >
          <el-input v-model="listQuery.QryCustomer" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="2">
        <el-form-item label="序列号" >
          <el-input v-model="listQuery.QryManufSN" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="2">
        <el-form-item label="接单员" >
          <el-input v-model="listQuery.QryRecepUser" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
 
      <el-col :span="2">
        <el-form-item label="技术员" >
          <el-input v-model="listQuery.QryTechName" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
     <el-col :span="3">
        <el-form-item label="问题类型" >
          <el-cascader :options="dataTree" class="malchack" v-model="listQuery.QryProblemType"   :props="{ value:'id',label:'name',children:'childTypes',expandTrigger: 'hover'  }"  clearable></el-cascader>
         
       
        </el-form-item>
      </el-col>
      <el-col :span="6">
        <el-form-item label="创建日期" >
          <el-col :span="11">
            <el-date-picker
            
           format="yyyy-MM-dd"   
              value-format="yyyy-MM-dd"
              placeholder="选择开始日期"
              v-model="listQuery.QryCreateTimeFrom"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
          <el-col class="line" :span="2">至</el-col>
          <el-col :span="11">
            <el-date-picker
               format="yyyy-MM-dd"   
              value-format="yyyy-MM-dd"
              placeholder="选择结束时间"
              v-model="listQuery.QryCreateTimeTo"
              style="width: 100%;"
            ></el-date-picker>
          </el-col>
        </el-form-item>
      </el-col>

      <el-col :span="2" style="margin-left:0;" >
               <el-button type="primary" @click="onSubmit" size="mini"  icon="el-icon-search"> 搜 索 </el-button>
      </el-col>
    </el-row>
  </el-form>
</template>

<script>
import * as problemtypes from "@/api/problemtypes";

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
        QryState: 5, //- 呼叫状态查询条件
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
        { value: 1, label: "待处理" },
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "已外出" },
        { value: 5, label: "已挂起" },
        { value: 6, label: "已接收" },
        { value: 7, label: "已解决" },
        { value: 8, label: "已回访" }
      ],
                dataTree:[],

    };
  },
    created(){
       problemtypes
      .getList()
      .then((res) => {
        this.dataTree = res.data;
      })
      .catch((error) => {
        console.log(error);
      });
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