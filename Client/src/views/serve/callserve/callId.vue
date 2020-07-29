<template>
<div>
  <p>最近关闭</p>
<el-table
          ref="mainTable"
          class="table_label"
          :data="toCallList.newestNotCloseOrder"
          v-loading="listLoading"
          border
          max-height="350px"
          fit
          style="width: 100%;"
          highlight-current-row>
          <el-table-column
            show-overflow-tooltip
            v-for="(fruit,index) in CloseOrder"
            :align="fruit.align"
            :key="`ind${index}`"
            header-align="left"
            :width="fruit.width"
            :fixed="fruit.fixed"
            :sortable="fruit=='chaungjianriqi'?true:false"
            style="background-color:silver;"
            :label="fruit.label">
            <template slot-scope="scope">
                        <el-link
                    v-if="fruit.name === 'serviceOrderId'"
                    type="primary"
                    @click="openTree(scope.row.serviceOrderId)"
                  >{{scope.row.serviceOrderId}}</el-link>
                  <span
                    v-if="fruit.name === 'status'"
                    :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
                  >{{statusOptions[scope.row[fruit.name]].label}}</span>
                  <span v-if="fruit.name === 'fromType'&&!scope.row.serviceWorkOrders">{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                  <span v-if="fruit.name === 'priority'">{{priorityOptions[scope.row.priority]}}</span>
                  <span
                    v-if="fruit.name!='priority'&&fruit.name!='fromType'&&fruit.name!='status'&&fruit.name!='serviceOrderId'"
                  >{{scope.row[fruit.name]}}</span>
            </template>
          </el-table-column>
</el-table>

        <!-- <pagination
          v-show="toCallList.newestNotCloseOrder.length>0"
          :total="toCallList.newestNotCloseOrder.length"
          :page.sync="listQuery.page"
          :limit.sync="listQuery.limit"
          @pagination="handleChange"
        /> -->
          <p>最近呼叫</p>

        <el-table
          ref="mainTable"
          class="table_label"
          :data="toCallList.newestOrder"
          v-loading="listLoading"
          border
          max-height="300px"
          fit
          style="width: 100%;"
          highlight-current-row>
          <el-table-column
            show-overflow-tooltip
            v-for="(fruit,index) in newestOrder"
            :align="fruit.align"
            :key="`ind${index}`"
            header-align="left"
            :width="fruit.width"
            :fixed="fruit.fixed"
            :sortable="fruit=='chaungjianriqi'?true:false"
            style="background-color:silver;"
            :label="fruit.label">
            <template slot-scope="scope">
                        <el-link
                    v-if="fruit.name === 'serviceOrderId'"
                    type="primary"
                    @click="openTree(scope.row.serviceOrderId)"
                  >{{scope.row.serviceOrderId}}</el-link>
                  <span
                    v-if="fruit.name === 'status'"
                    :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
                  >{{statusOptions[scope.row[fruit.name]].label}}</span>
                  <span v-if="fruit.name === 'fromType'&&!scope.row.serviceWorkOrders">{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                  <span v-if="fruit.name === 'priority'">{{priorityOptions[scope.row.priority]}}</span>
                  <span
                    v-if="fruit.name!='priority'&&fruit.name!='fromType'&&fruit.name!='status'&&fruit.name!='serviceOrderId'"
                  >{{scope.row[fruit.name]}}</span>
            </template>
          </el-table-column>
</el-table>
    <!-- <pagination
          v-show="toCallList.newestOrder.length>0"
          :total="toCallList.newestOrder.length"
          :page.sync="listQuery1.page"
          :limit.sync="listQuery1.limit"
          @pagination="handleChange1"
        /> -->
</div>

</template>

<script>
// import Pagination from "@/components/Pagination";

export default {
  props: ["toCallList" ],
    // components: {  Pagination },

  data() {
    return {
       currentRow: [] ,//选择项
      //  dialogPartner:'',
         listQuery: {
        // 查询条件
        page: 1,
        limit: 10,
        key: undefined,
        appId: undefined
      },
               listQuery1: {
        // 查询条件
        page: 1,
        limit: 10,
        key: undefined,
        appId: undefined
      },
      listLoading:false,
            CloseOrder: [
         { name: "serveid", label: "服务ID" ,fixed:true},
        { name: "customer", label: "工单ID" ,align:'left'},
        { name: "custmrName", label: "优先级" },
        { name: "manufSN", label: "呼叫类型",width:'120px' },
        { name: "internalSN", label: "呼叫状态" },
        { name: "itemCode", label: "费用审核" },
        { name: "itemName", label: "客户代码" },
        { name: "", label: "客户名称" },
        { name: "", label: "主题" },
        { name: "", label: "创建日期" },
        { name: "", label: "接单员" },
        { name: "", label: "制造商序列号" },
        { name: "", label: "物料编码" },
        { name: "", label: "物料描述",width:'110px' },
        { name: "", label: "联系人",width:'110px' },
        { name: "", label: "电话号码" },
         { name: "", label: "技术员" },
        { name: "", label: "售后主管" },
        { name: "", label: "销售员",width:'110px' },
        { name: "", label: "预约日期",width:'110px' },
        { name: "", label: "上门日期" }
      ],
          statusOptions: [
        { value: 1, label: "待处理" },
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "已外出" },
        { value: 5, label: "已挂起" },
        { value: 6, label: "已接收" },
        { value: 7, label: "已解决" },
        { value: 8, label: "已回访" }
      ],
                  newestOrder: [
         { name: "contactTel", label: "联系电话" ,fixed:true},
        { name: "contacter", label: "联系人" ,align:'left'},
        { name: "custmrName", label: "客户名称" },
        { name: "customerId", label: "客户ID",width:'120px' },
        { name: "newestContactTel", label: "最近联系电话" },
        { name: "newestContacter", label: "最近联系人" },
        { name: "services", label: "服务" },
        { name: "status", label: "状态" },
      ],
    };
  },
  compunted: {
    toCallList: {
      get: function(a) {
        console.log(a);
      },
      set: function(a) {
        console.log(a);
      }
    }
  },
    mounted() {
    console.log(this.toCallList);
  },
  watch:{
    toCallList:{
      handler(val){
      console.log(val);
      }
    }
  },
  methods:{
    //       openDialog() {   //打开前赋值
    //   // this.filterPartnerList = this.partnerList;
    //   console.log(11)
    // },
    //  handleChange(val) {
    //   this.listQuery.page = val.page;
    //   this.listQuery.limit = val.limit;
    //   // this.getList();
    // },
    //     handleChange1(val) {
    //   this.listQuery1.page = val.page;
    //   this.listQuery1.limit = val.limit;
    //   // this.getList();
    // },
   handleCurrentChange(val) {
        console.log(val)
      }

}
}
</script>

<style lang="scss" scope>
.redColor {
  color: red;
}
.greenColro {
  color: green;
}
</style>