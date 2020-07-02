<template>
       <el-table
      :data="partnerList"
      border
      style="width: 100%">
          <el-table-column
      type="selection"
      width="55">
      </el-table-column>
      <el-table-column
        prop="cardCode"
        fixed
        label="客户代码"
        width="80">
      </el-table-column>
      <el-table-column
        prop="cardName"
        label="客户名称"
        align="center"
        min-width="120">
      </el-table-column>
  
       <el-table-column
        prop="cntctPrsn"
        label="联系人"
                align="center"

        width="100">
      </el-table-column>
      <el-table-column
        prop="slpName"
        align="center"
        label="销售员"
        width="100">
      </el-table-column>
      <el-table-column
      align="center"
        prop="currency"
         width="100"
        label="货币种类">
      </el-table-column>
       <el-table-column
        align="center"
        label="科目余额"
        width="120">
        <template slot-scope="scope">
            <span :class="[scope.row.balance>=0?'redColor':'greenColro']">{{scope.row.balance}}0000</span>
        </template>
      </el-table-column>
      <el-table-column
        prop="address"
        align="center"
        label="开票地址"
        min-width="180">
      </el-table-column>
      <el-table-column
        prop="address2"
        label="收货地址"
         min-width="180">
      </el-table-column>
        <el-table-column
        prop="u_FPLB"
        align="center"
        width="120px"
        label="发票类别">
      </el-table-column>
    </el-table>
</template>

<script>
export default {
    props:["partnerList" ],
    data(){
        return{
        }
    },
    compunted:{
partnerList:{
    get:function(a){
console.log(a)
    },
    set:function(a){
        console.log(a)
    }
}
    },
    mounted(){
        console.log(this.partnerList)
    },
}
</script>

<style lang="scss" scope>
    .redColor{
        color:red;
    }
    .greenColro{
        color:green;
    }
</style>