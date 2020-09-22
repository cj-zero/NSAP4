<template>
  <div class="search-wrapper">
    <el-input 
      v-model="listQuery.QryU_SAP_ID" 
      size="mini"
      @keyup.enter.native="onSubmit" 
      style="width: 100px;"
      class="filter-item"
      placeholder="服务ID">
    </el-input>
    <el-select 
      v-model="listQuery.QryState" 
      style="width: 120px;"
      class="filter-item"
      size="mini"
      placeholder="请选择呼叫状态">
      <el-option
        v-for="(item,index) in callStatus"
        :key="index"
        :disabled="item.disabled"
        :label="item.label"
        :value="item.value"
      ></el-option>
    </el-select>
    <el-input 
      v-model.trim="listQuery.QryCustomer" 
      size="mini"
      @keyup.enter.native="onSubmit" 
      style="width: 200px;"
      class="filter-item"
      placeholder="客户">
    </el-input>
    <el-input 
      v-model="listQuery.QryManufSN" 
      size="mini"
      @keyup.enter.native="onSubmit" 
      style="width: 165px;"
      class="filter-item"
      placeholder="序列号">
    </el-input>
    <el-input 
      v-model="listQuery.QryRecepUser" 
      size="mini"
      @keyup.enter.native="onSubmit" 
      style="width: 100px;"
      class="filter-item"
      placeholder="接单员">
    </el-input>
    <el-input 
      v-model="listQuery.QryTechName" 
      size="mini"
      @keyup.enter.native="onSubmit" 
      style="width: 100px;"
      class="filter-item"
      placeholder="技术员">
    </el-input>
    <el-cascader 
      style="width: 180px;"
      palceholder="问题类型"
      size="mini"
      :options="dataTree" class="malchack filter-item" v-model="listQuery.QryProblemType" 
      :props="{ value:'id',label:'name',children:'childTypes',expandTrigger: 'hover', emitPath: false }"  
      clearable>
    </el-cascader>
    <el-date-picker
      class="filter-item"
      format="yyyy-MM-dd"   
      value-format="yyyy-MM-dd"
      placeholder="选择开始日期"
      v-model="listQuery.QryCreateTimeFrom"
      style="width: 150px"
      size="mini"
    ></el-date-picker>
    <el-date-picker
      class="filter-item"
      format="yyyy-MM-dd"   
      size="mini"
      value-format="yyyy-MM-dd"
      placeholder="选择结束时间"
      v-model="listQuery.QryCreateTimeTo"
      style="width: 150px"
    ></el-date-picker>
    <el-button
      class="filter-item"
      size="mini"
      v-waves
      icon="el-icon-search"
      @click="onSubmit"
    >查询</el-button>
  </div>
</template>

<script>
import * as problemtypes from "@/api/problemtypes";
// import { delete } from 'vuedraggable';
import waves from "@/directive/waves";
export default {
  directives: {
    waves
  },
  props: {
    activeName: {
      type: String,
      defualt: ''
    }
  },
  watch: {
    activeName (val) {
      this.listQuery.QryState = val === 'first'
       ? 2 : 7
      this.onSubmit()
    }
  },
  computed: {
    callStatus () {
      return this.activeName === 'first'
        ? [
            { value: 2, label: "已排配" },
            { value: 3, label: "已预约" },
            { value: 4, label: "已外出" },
            { value: 5, label: "已挂起" },
            { value: 6, label: "已接收" }
          ]
        : [
            { value: 7, label: "已解决" },
            { value: 8, label: "已回访" }
          ]
    }
  },
  data() {
    return {
      listQuery: {
        // 查询条件
        page: 1,
        // limit: 20,
        key: undefined,
        appId: undefined,
        Name: "", //	Description
        QryU_SAP_ID: "", //- 查询服务ID查询条件
        QryState: 2, //- 呼叫状态查询条件
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
      // callStatus: [
      //   { value: '', label: '全部' },
      //   // { value: 1, label: "待处理" },
      //   { value: 2, label: "已排配" },
      //   { value: 3, label: "已预约" },
      //   { value: 4, label: "已外出" },
      //   { value: 5, label: "已挂起" },
      //   { value: 6, label: "已接收" },
      //   { value: 7, label: "已解决" },
      //   { value: 8, label: "已回访" }
      // ],
      dataTree:[]
    };
  },
  created(){
      problemtypes
    .getList()
    .then((res) => {
      // this.dataTree = res.data;
      this.dataTree = this._normalizeProblemTypes(res.data)
    })
    .catch((error) => {
      console.log(error);
    });
  },
  methods: {
    onSubmit() {
      this.$emit("change-Search", this.listQuery);
    },
    sendOrder(){
      // console.log(11)
      this.$emit("change-Order",true)
    },
    _normalizeProblemTypes (data) {
      // 处理问题类型数据
      const typeList = []
      data.forEach(item => {
        let { childTypes } = item
        if (childTypes && childTypes.length) {
          item.childTypes = this._normalizeProblemTypes(childTypes)
        } else {
          delete item.childTypes
        }
        typeList.push(item)
      })
      return typeList
    }
  },
  // watch: {
  //   listQuery: {
  //     deep: true,
  //     immediate:true,
  //     handler(val) {
  //       this.$emit("change-Search", val);
  //     }
  //   }
  // }
};
</script>

<style lang="scss" scoped>
.search-wrapper {
  display: inline-block;
}
</style>>
