<template>
  <div class="check-wrapper">
    <el-scrollbar class="scroll-bar">
      <template v-if="checkList && checkList.length">
        <ul class="check-list">
          <li 
            class="check-item"
            v-for="item in checkList"
            :key="item.id"
          >
            <div class="content">
              <el-row v-if="!item.isNew" type="flex">
                <span class="text">旧</span>
                <div>
                  <p>序列号: {{ item.orginalManufSN }}</p>
                </div>
              </el-row>
              <el-row type="flex" class="new">
                <span class="text">新</span>
                <div>
                  <p>序列号: {{ item.manufSN }}</p>
                  <p>物料编码: {{ item.itemCode }}</p>
                </div>
              </el-row>
            </div>
            <el-row type="flex" justify="space-between" align="middle" class="btn-wrapper">
              <span>{{ item.currentUser }}</span>
              <el-button :type="changeType(item.status)" size="mini" @click="checkItem(item)">{{ item.status | processText }}</el-button>
            </el-row>
          </li>
        </ul>
      </template>
      <template v-else>
        暂无核对设备信息~~
      </template>
    </el-scrollbar>
    
    <el-dialog
      v-el-drag-dialog
      width="900px"
      top="10vh"
      class="dialog-mini"
      :close-on-click-modal="false"
      title="核对设备"
      :visible.sync="dialogFormVisible"
      append-to-body
      @closed="onClosed"
    >
      <form-add 
        ref="formAdd"
        :formData="formData" 
        :isNew="dialogStatus" 
        :info="info"
        :filterSerialNumberList="filterSerialNumberList"
        :serialCount="serialCount"
        :listQuery="listQuery"
        :serLoad="serLoad"
        @close="onClose"
        @searchList="onSearchList"
        @handlelChange="handleSerialChange"></form-add>
    </el-dialog>
  </div>
</template>

<script>
import { getWorkOrderDetailById  } from '@/api/serve/callservesure'
import { getTechnicianApplyDevices } from '@/api/serve/technicianApply'
import { getSerialNumber } from "@/api/callserve";
import FormAdd from './formAdd'

export default {
  inject: ['instance'],
  components: {
    FormAdd
  },
  props: {
    sapOrderId: {
      type: [Number, String],
      default: ''
    },
    customerId: {
      type: [Number, String],
      default: ''
    }
  },
  watch: {
    sapOrderId () {
      this._getTechnicianApplyDevices()
    },
    customerId (newVal) {
      this._getSerialNumber(newVal)
    }
  },
  filters: {
    processText (status) {
      return status === 0 ? '核对' :
      status === 1 ?  '已通过' :
      '未通过'
    }
  },
  data () {
    return {
      checkList: [], // 设备列表
      dialogStatus: '', // 当前点击核对设备 对应新建还是编辑
      textMap: ['修改', '新增'],
      dialogFormVisible: false,
      formData: {},
      listQuery: {
        page: 1,
        limit: 30,
        CardCode: ""
      },
      serLoad: false,
      info: {}, // 每一个设备信息
      filterSerialNumberList: [], // 设备序列号列表
      serialCount: 0 // 总数
    }
  },
  methods: {
    checkItem (item) {
      // GetWorkOrderDetailById
      let { 
        status,
        isNew, 
        orginalWorkOrderId, 
        manufSN, 
        itemCode, 
        warrantyEndDate,
        materialDescription,
        internalSerialNumber,
        problemTypeId,
        problemTypeName,
        fromTheme,
        fromType
      } = item
      this.dialogStatus = Boolean(isNew)
      this.info = item
      console.log(status)
      if (status === 0) {
        if (!isNew) { // 修改核对
          getWorkOrderDetailById({
            workOrderId: orginalWorkOrderId
          }).then(res => {
            if (res.data && res.data.length) {
              res.data[0].themeList = JSON.parse(res.data[0].fromTheme)
              this.formData = res.data[0]
              this.dialogFormVisible = true
            } else {
              this.$message.error('获取工单详情失败')
            }
          }).catch(() => {
            this.$message.error('获取工单详情失败!')
          })
        } else {
          this.formData = {
            priority: 1, //优先级 4-紧急 3-高 2-中 1-低
            feeType: 1, //服务类型 1-免费 2-收费
            submitDate: "", //工单提交时间
            recepUserId: "", //接单人用户Id
            remark: "", //备注
            status: 1, //呼叫状态 1-待确认 2-已确认 3-已取消 4-待处理 5-已排配 6-已外出 7-已挂起 8-已接收 9-已解决 10-已回访
            currentUserId: "", //App当前流程处理用户Id
            fromTheme: fromTheme || "", //呼叫主题
            themeList: JSON.parse(fromTheme).filter(item => item.description),
            fromId: 1, //呼叫来源 1-电话 2-APP
            problemTypeId: problemTypeId || "", //问题类型Id
            problemTypeName: problemTypeName || "",
            fromType: fromType || "", //呼叫类型1-提交呼叫 2-在线解答（已解决）
            materialCode: itemCode || "", //物料编码
            materialDescription: materialDescription || "", //物料描述
            manufacturerSerialNumber: manufSN || "", //制造商序列号
            internalSerialNumber: internalSerialNumber || "", //内部序列号
            warrantyEndDate: warrantyEndDate || "", //保修结束日期
            bookingDate: "", //预约时间
            visitTime: "", //上门时间
            liquidationDate: "", //清算日期
            contractId: "", //服务合同
            solutionId: "", //解决方案
            solutionsubject: "", // 解决方案内容
            troubleDescription: "",
            processDescription: "",
          }
          this.dialogFormVisible = true
        }
        if (this.$refs.formAdd && !!isNew) {
          this.$refs.formAdd._getFormThemeList()
        }
      }
      
    },
    changeType (status) {
      return status !== 0 ? 'info' : 'primary'
    },
    handleSerialChange (val) {
      this.listQuery.page = val.page;
      this.listQuery.limit = val.limit;
      this._getSerialNumber();
    },
    onSearchList (val) {
      let { ManufSN, ItemCode } = val
      this.listQuery.ManufSN = ManufSN
      this.listQuery.ItemCode = ItemCode
      this._getSerialNumber()
    },
    onClose (type) {
      console.log(type, 'type')
      if (type) {
        if (type === 'success') { // 点击确认
          // this.instance._getDetails() // 不通过的不会刷新页面
        }
        this._getTechnicianApplyDevices()
      }
      this.dialogFormVisible = false
    },
    _getSerialNumber (cardCode) {
      this.serLoad = true
      this.listQuery.CardCode = cardCode
      getSerialNumber(this.listQuery)
        .then((res) => {
          this.filterSerialNumberList = res.data;
          this.serialCount = res.count;
          this.serLoad = false
        })
        .catch((error) => {
          this.serLoad = false
          console.log(error);
        });
    },
    _getTechnicianApplyDevices () {
      if (this.sapOrderId) {
        getTechnicianApplyDevices({ sapOrderId: this.sapOrderId }).then(res => {
          this.checkList = res.data
        })
      }
    },
    onClosed () {
      this.$refs.formAdd.reset()
    }
  },
  created () {

  },
  mounted () {
    this._getTechnicianApplyDevices()
    this._getSerialNumber(this.customerId)
  },
}
</script>
<style lang='scss' scoped>
.check-wrapper {
  .scroll-bar {
    &.el-scrollbar {
      ::v-deep .el-scrollbar__wrap {
        max-height: 550px; // 最大高度
        overflow-x: hidden; // 隐藏横向滚动栏
        margin-bottom: 0 !important;
      }
    }
  }
  .check-list {
    // max-height: 600px;
    // overflow-y: auto;
    .check-item {
      padding: 15px;
      box-shadow: 0 2px 12px 0 rgba(0,0,0,.1);
      .old {
        color: red;
        margin-bottom: 5px;
      }
      .new {
        color: rgba(102, 177, 255, 1);
      }
      .text {
        margin-right: 10px;
      }
      .btn-wrapper {
        margin-top: 5px;
      }
    }
  }
}
</style>