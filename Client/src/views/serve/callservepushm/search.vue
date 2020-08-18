<template>
  <el-form ref="form" :model="listQuery" label-width="70px" size='mini '>
    <div style="padding:10px 0;"></div>
    <el-row :gutter="4">
      <el-col :span="2">
        <el-form-item label="服务ID" >
          <el-input v-model="listQuery.QryU_SAP_ID" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="呼叫状态" >
          <el-select v-model="listQuery.QryState" disabled placeholder="请选择呼叫状态">
        <el-option label="待处理" :value="1"></el-option>
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
          <el-cascader :options="dataTree" class="malchack" v-model="listQuery.QryProblemType" 
          :props="{ value:'id',label:'name',children:'childTypes',expandTrigger: 'hover', emitPath: false }"  
          clearable></el-cascader>
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
                  defaultProps:{
        label:'name',
        children:'childTypes'
      },//树形控件的显示状态
      listQuery: {
        // 查询条件
        page: 1,
        limit: 20,
        key: undefined,
        appId: undefined,
        Name: "", //	Description
        QryU_SAP_ID: "", //- 查询服务ID查询条件
        QryState: 1, //- 呼叫状态查询条件
        QryCustomer: "", //- 客户查询条件
        QryManufSN: "", // - 制造商序列号查询条件
        QryCreateTimeFrom: "", //- 创建日期从查询条件
        QryCreateTimeTo: "", //- 创建日期至查询条件
        QryRecepUser: "", //- 接单员
        QryTechName: "", // - 工单技术员
        QryProblemType: "" // - 问题类型
      },
          dataTree:[],
    };

  },
  created(){
       problemtypes
      .getList()
      .then((res) => {
        this.dataTree = this._normalizeProblemTypes(res.data);
        console.log(this.dataTree)
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

<style lang="scss" scoped>


// .{
//   ::v-deep  .el-cascader-menu{
//     overflow:hidden;
//   }
// }
</style>