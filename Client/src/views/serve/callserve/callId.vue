<template>
  <div>
    <el-row type="flex" justify="center">
      <el-col>
        <p>此用户最近10个服务单</p>
        <common-table
          ref="mainTable"
          class="table_label"
          :data="toCallList.newestOrder"
          :loading="listLoading"
          max-height="400px"
          :columns="orderList"
        >
          <template v-slot:id="{ row }">
            <el-link type="primary" @click="openTree(row.id)">{{ row.u_SAP_ID }}</el-link>
          </template>
        </common-table>
        
      </el-col>
    </el-row>
  </div>
</template>

<script>
// import Pagination from "@/components/Pagination";

export default {
  props: {
    toCallList: {
      type: Object,
      default () {
        return {}
      }
    },
    openTree: {
      type: Function,
      default () {
        return () => {}
      }
    }
  },
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
         { prop: "id", label: "服务ID" ,fixed: true, slotName: 'id' },
         { prop: "createTime", label: "创建日期" },
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
    _openTree (id) {
      this.openTree(id)
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