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
      disabled 
      style="width: 120px;"
      class="filter-item"
      size="mini"
      placeholder="请选择呼叫状态">
      <el-option label="待处理" :value="1"></el-option>
    </el-select>
    <el-input 
      v-model="listQuery.QryCustomer" 
      size="mini"
      @keyup.enter.native="onSubmit" 
      style="width: 170px;"
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
    <el-input 
      v-model="listQuery.QryMaterialCode" 
      size="mini"
      @keyup.enter.native="onSubmit" 
      style="width: 150px;"
      class="filter-item"
      placeholder="物料代码">
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
import waves from "@/directive/waves";
import * as problemtypes from "@/api/problemtypes";
export default {
  directives: {
    waves
  },
  data() {
    return {
      defaultProps:{
        label:'name',
        children:'childTypes'
      },//树形控件的显示状态
      listQuery: {
        // 查询条件
        page: 1,
        // limit: 20,
        key: undefined,
        appId: undefined,
        Name: "", //	Description
        QryU_SAP_ID: "", //- 查询服务ID查询条件
        QryState: 1,
        QryCustomer: "", //- 客户查询条件
        QryManufSN: "", // - 制造商序列号查询条件
        QryCreateTimeFrom: "", //- 创建日期从查询条件
        QryCreateTimeTo: "", //- 创建日期至查询条件
        QryRecepUser: "", //- 接单员
        QryTechName: "", // - 工单技术员
        QryProblemType: "", // - 问题类型
        QryMaterialCode: "" // 物料代码
      },
      dataTree:[],
    };

  },
  created(){
    problemtypes
      .getList()
      .then((res) => {
        this.dataTree = this._normalizeProblemTypes(res.data);
        // console.log(this.dataTree)
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
  //     // immediate:true,
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

// .{
//   ::v-deep  .el-cascader-menu{
//     overflow:hidden;
//   }
// }
</style>