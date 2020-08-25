<template>
  <div>
    <el-row type="flex" justify="center">
      <el-col>
        <p>此用户最近10个服务单</p>
        <el-table
          ref="mainTable"
          class="table_label"
          :data="toCallList.newestOrder"
          v-loading="listLoading"
          border
          max-height="400px"
          fit
          style="width: 100%;"
          highlight-current-row>
          <el-table-column
            show-overflow-tooltip
            v-for="(fruit,index) in orderList"
            :align="fruit.align"
            :key="`ind${index}`"
            header-align="left"
            :width="fruit.width"
            :fixed="fruit.fixed"
            style="background-color:silver;"
            :label="fruit.label">
            <template slot-scope="scope">
              <el-link
                v-if="fruit.name === 'id'"
                type="primary"
                @click="openTree(scope.row.id)"
              >{{ scope.row.id }}</el-link>
              <span v-if="fruit.name === 'createTime'">
                {{ scope.row.createTime }}
              </span>
              <!-- <span
                v-if="fruit.name === 'status'"
                :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
              >{{statusOptions[scope.row[fruit.name]].label}}</span>
              <span v-if="fruit.name === 'fromType'&&!scope.row.serviceWorkOrders">{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
              <span v-if="fruit.name === 'priority'">{{priorityOptions[scope.row.priority]}}</span>
              <span
                v-if="fruit.name!='priority'&&fruit.name!='fromType'&&fruit.name!='status'&&fruit.name!='serviceOrderId'"
              >{{scope.row[fruit.name]}}</span> -->
            </template>
          </el-table-column>
        </el-table>
      </el-col>
    </el-row>
  </div>
</template>

<script>
// import Pagination from "@/components/Pagination";

export default {
  props: ["toCallList" ],
    // components: {  Pagination },
  inject: ['instance'],
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
      orderList: [
         { name: "id", label: "服务ID" ,fixed:true},
         { name: "createTime", label: "创建日期" },
        // { name: "customer", label: "工单ID" ,align:'left'},
        // { name: "custmrName", label: "优先级" },
        // { name: "manufSN", label: "呼叫类型",width:'120px' },
        // { name: "internalSN", label: "呼叫状态" },
        // { name: "itemCode", label: "费用审核" },
        // { name: "itemName", label: "客户代码" },
        // { name: "", label: "客户名称" },
        // { name: "", label: "主题" },     
        // { name: "", label: "接单员" },
        // { name: "", label: "制造商序列号" },
        // { name: "", label: "物料编码" },
        // { name: "", label: "物料描述",width:'110px' },
        // { name: "", label: "联系人",width:'110px' },
        // { name: "", label: "电话号码" },
        //  { name: "", label: "技术员" },
        // { name: "", label: "售后主管" },
        // { name: "", label: "销售员",width:'110px' },
        // { name: "", label: "预约日期",width:'110px' },
        // { name: "", label: "上门日期" }
      ]
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
  mounted() {},
  methods:{
    openTree (id) {
      // console.log(this.instance)
      this.instance.openTree(id)
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