<template>
  <div>
                <el-table
              ref="mainTable"
              :key="key"
              :data="list"
              v-loading="listLoading"
              border
              fit
              tooltip-effect="dark"
              style="width: 100%;"
              highlight-current-row
              @row-click="rowClick"
            >
              <el-table-column
                show-overflow-tooltip
                v-for="(fruit,index) in formTheadOptions"
                align="center"
                :key="`ind${index}`"
                :sortable="fruit=='chaungjianriqi'?true:false"
                style="background-color:silver;"
                :label="fruit.label"
                :fixed="fruit.ifFixed"
                :width="fruit.width"
              >
                <template slot-scope="scope">
                  <el-link
                    v-if="fruit.name === 'serviceOrderId'"
                    type="primary"
                    @click="openTree(scope.row.serviceOrderId)"
                  >{{scope.row.serviceOrderId}}</el-link>
                  <span
                    v-if="fruit.name === 'status'"
                    :class="[scope.row[fruit.name]===1?'greenWord':(scope.row[fruit.name]===2?'orangeWord':'redWord')]"
                  >{{statusOptions[scope.row[fruit.name]-1].label}}</span>
                  <span v-if="fruit.name === 'fromType'">{{scope.row[fruit.name]==1?'提交呼叫':"在线解答"}}</span>
                  <span v-if="fruit.name === 'priority'">{{priorityOptions[scope.row.priority]}}</span>
                  <span
                    v-if="fruit.name!='priority'&&fruit.name!='fromType'&&fruit.name!='status'&&fruit.name!='serviceOrderId'"
                  >{{scope.row[fruit.name]}}</span>
                </template>
              </el-table-column>
            </el-table>
  </div>
</template>

<script>
export default {
        data(){
            return{
                     formTheadOptions: [
        { name: "serviceWorkOrderId", label: "工单ID", ifFixed: true },
        { name: "priority", label: "优先级" },
        { name: "fromType", label: "呼叫类型", width: "100px" },
        { name: "status", label: "呼叫状态" },
        { name: "status", label: "费用审核" },
        { name: "customerId", label: "客户代码" },
        { name: "terminalCustomer", label: "终端客户" },
        { name: "customerName", label: "客户名称" },
        { name: "fromTheme", label: "呼叫主题" },
        { name: "createTime", label: "创建日期" },
        { name: "recepUserName", label: "接单员" },
        { name: "techName", label: "技术员" },
        { name: "manufacturerSerialNumber", label: "制造商序列号" },
        { name: "materialCode", label: "物料编码" },
        { name: "materialDescription", label: "物料描述" },
        { name: "contacter", label: "联系人" },
        { name: "contactTel", label: "电话号码" },
        { name: "supervisor", label: "售后主管" },
        { name: "salesMan", label: "销售员" }
        // "serviceWorkOrderId": 1,
        // "problemTypeName": "数值异常",
        // "currentUserId": 1
      ],
            }
        }
}
</script>

<style>

</style>