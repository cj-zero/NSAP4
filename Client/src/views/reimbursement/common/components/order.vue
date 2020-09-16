<template>
  <div class="order-wrapper">
    <common-form
      :data="formData"
      :config="formConfig"
    >
      <template v-slot:attachment>
        <!-- <upLoadFile  @get-ImgList="getFileList" :limit="limit" uploadType="file" ref="uploadFile"></upLoadFile>
        <upLoadFile  @get-ImgList="getTrafficList" :limit="limit" uploadType="file" ref="trafficUploadFile"></upLoadFile> -->
      </template>
      <!-- 出差 -->
      <template v-slot:travel>
        <el-form 
          ref="travelForm" 
          :model="formData" 
          size="mini" 
          :show-message="false"
          class="form-wrapper"
        >
          <el-table 
            :data="formData.travel"
            :summary-method="getSummaries"
            show-summary
            max-height="300px"
          >
          <el-table-column label="出差补贴" header-align="center">
            <el-table-column
              v-for="item in travelConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'input'">
                   <el-form-item
                    :prop="'travel.' + scope.$index + '.'+ item.prop"
                    :rules="rules[item.prop] || {}"
                  >
                    <el-input v-model="scope.row[item.prop]"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'travel.' + scope.$index + '.'+ item.prop"
                    :rules="rules[item.prop] || {}"
                  >
                    <el-select
                      v-model="scope.row[item.prop]"
                    >
                      <el-option
                        v-for="optionItem in item.options"
                        :key="optionItem.label"
                        :value="optionItem.value"
                        :label="optionItem.label"
                      >
                      </el-option>
                    </el-select>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'upload'">
                  
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      v-if="processIcon(iconItem.icon, scope.$index, formData.travel)"
                      :key="iconItem.icon"
                      :class="iconItem.icon" 
                      class="icon-item"
                      @click="iconItem.handleClick(scope, formData.travel, 'travel')">
                    </i>
                  </template>
                </template>
              </template>
            </el-table-column>
          </el-table-column>
        </el-table>
        </el-form>
      </template>
      
      <!-- 交通 -->
      <!-- <template v-slot:traffic> -->
        
      <!-- </template> -->
    </common-form>
    <el-form 
          ref="travelForm" 
          :model="formData" 
          size="mini" 
          :show-message="false"
          class="form-wrapper"
        >
          <el-table 
            :data="formData.traffic"
            :summary-method="getSummaries"
            show-summary
            max-height="300px"
          >
          <el-table-column label="交通费用" header-align="center">
            <el-table-column
              v-for="item in trafficConfig"
              :key="item.label"
              :label="item.label"
              :align="item.align || 'left'"
              :prop="item.prop"
            >
              <template slot-scope="scope">
                <template v-if="item.type === 'order'">
                  {{ scope.$index + 1 }}
                </template>
                <template v-else-if="item.type === 'input'">
                   <el-form-item
                    :prop="'traffic.' + scope.$index + '.'+ item.prop"
                    :rules="rules[item.prop] || {}"
                  >
                    <el-input v-model="scope.row[item.prop]"></el-input>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'select'">
                  <el-form-item
                    :prop="'travel.' + scope.$index + '.'+ item.prop"
                    :rules="rules[item.prop] || {}"
                  >
                    <el-select
                      v-model="scope.row[item.prop]"
                    >
                      <el-option
                        v-for="optionItem in item.options"
                        :key="optionItem.label"
                        :value="optionItem.value"
                        :label="optionItem.label"
                      >
                      </el-option>
                    </el-select>
                  </el-form-item>
                </template>
                <template v-else-if="item.type === 'upload'">
                  <upLoadFile  @get-ImgList="getTrafficList" :limit="limit" uploadType="file" ref="trafficUploadFile"></upLoadFile>
                </template>
                <template v-else-if="item.type === 'operation'">
                  <template v-for="iconItem in item.iconList">
                    <i 
                      v-if="processIcon(iconItem.icon, scope.$index, formData.travel)"
                      :key="iconItem.icon"
                      :class="iconItem.icon" 
                      class="icon-item"
                      @click="iconItem.handleClick(scope, formData.travel, 'travel')">
                    </i>
                  </template>
                </template>
              </template>
            </el-table-column>
          </el-table-column>
        </el-table>
        </el-form>
    <el-button @click="validate">提交</el-button>
  </div>
</template>

<script>
import { EXPENSE_CATEGORY } from '@/utils/declaration'
import CommonForm from './CommonForm'
import upLoadFile from "@/components/upLoadFile";
import { TRAVEL_MONEY, TRAFFIC_TYPES, TRAFFIC_WAY } from '../js/type'
import { toThousands } from '@/utils/format'
export default {
  components: {
    CommonForm,
    upLoadFile
  },
  data () {
    let iconList = [
      { icon: 'el-icon-document-add', handleClick: this.add }, 
      { icon: 'el-icon-document-copy', handleClick: this.copy}, 
      { icon: 'el-icon-top', handleClick: this.up },
      { icon: 'el-icon-bottom', handleClick: this.down },
      { icon: 'el-icon-delete', handleClick: this.delete }
    ]
    return {
      maxSize: 1000,
      formData: {
        accountId: '',
        people: '',
        org: '',
        position: '',
        serviceId: '',
        customerId: '',
        customerName: '',
        customerRefer: '',
        origin: '',
        destination: '',
        originDate: '',
        endDate: '',
        category: '',
        projectName: '',
        status: '',
        theme: '',
        fillDate: '',
        materialType: '', // 设备类型
        solution: '',
        report: '',
        expense: '',
        responsibility: '',
        laborRelation: '',
        payDate: '',
        remark: '',
        pictures: [],
        travel: [{
          day: '',
          money: '',
          remark: ''
        }],
        traffic: [{

        }],
        accommodation: [],
        other: []
      },
      formConfig: [
        { label: '报销单号', prop: 'accountId', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '报销人', prop: 'people', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '部门', prop: 'org', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '职位', prop: 'position', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true, width: 100 },
        { label: '服务ID', prop: 'serviceId', palceholder: '请选择', required: true, col: 6, width: 100 },
        { label: '客户代码', prop: 'customerId', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '客户名称', prop: 'customerName', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '客户简称', prop: 'customerRefer', palceholder: '最长5个字', disabled: true, required: true, col: 6, maxlength: 5, isEnd: true, width: 100 },
        { label: '出发地点', prop: 'origin', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '到达地点', prop: 'destination', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '出发日期', prop: 'originDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, type: 'date', width: 100 },
        { label: '结束日期', prop: 'endDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, type: 'date', isEnd: true, width: 100 },
        { label: '报销类别', prop: 'category', palceholder: '请输入内容', disabled: true, required: true, col: 6, type: 'select', options: EXPENSE_CATEGORY, width: 100 },
        { label: '项目名称', prop: 'projectName', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '报销状态', prop: 'status', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true, width: 100 },
        { label: '呼叫主题', prop: 'theme', palceholder: '请输入内容', disabled: true, required: true, col: 18, width: 474 },
        { label: '填报事件', prop: 'fillDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true },
        { label: '设备类型', prop: 'materialType', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '解决方案', prop: 'solution', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '服务报告', prop: 'report',  disabled: true, required: true, col: 6, type: 'inline-slot', id: 'report', isEnd: true },
        { label: '费用承担', prop: 'expense', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '责任承担', prop: 'responsibility', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '劳务关系', prop: 'laborRelation', palceholder: '请输入内容', disabled: true, required: true, col: 6, width: 100 },
        { label: '支付时间', prop: 'payDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true },
        { label: '备注', prop: 'remark', palceholder: '请输入内容', disabled: true, required: true, col: 18, width: 474 },
        { label: '总金额', prop: 'totalMoney', col: 6, isEnd: true, type: 'inline-slot', id: 'money' },
        { label: '附件', prop: 'pictures', type: 'slot', id: 'attachment', fullLine: true },
        { label: '出差补贴', prop: 'travel', type: 'slot', id: 'travel', fullLine: true },
        { label: '交通费用', prop: 'traffic', type: 'slot', id: 'traffic', fullLine: true },
        { label: '住宿补贴', prop: 'accommodation', type: 'slot', id: 'accommodation', fullLine: true },
        { label: '其他费用', prop: 'other', type: 'slot', id: 'other', fullLine: true },
      ],
      limit: 8,
      travelConfig: [
        { label: '天数', prop: 'day', type: 'input' },
        { label: '金额', align: 'right', prop: 'money', type: 'select', options: TRAVEL_MONEY },
        { label: '备注', prop: 'remark', type: 'input' },
        { label: '操作', type: 'operation', iconList }
      ],
      trafficConfig: [
        { label: '序号', type: 'order' },
        { label: '交通类型', prop: 'trafficType', type: 'select', options: TRAFFIC_TYPES },
        { label: '交通工具', prop: 'trafficWay', type: 'select', options: TRAFFIC_WAY },
        { label: '出发地', prop: 'origin', type: 'input' },
        { label: '目的地', prop: 'destination', type: 'input' },
        { label: '金额', prop: 'money', type: 'input' },
        { label: '备注', prop: 'remark', type: 'input' },
        { label: '发票号码', disabled: true, type: 'input' },
        { label: '发票附件', disabled: true, type: 'upload' },
        { label: '其他附件', disabled: true, type: 'upload' },
        { label: '操作', type: 'operation', iconList }
      ],
      rules: {
        day: [ { required: true, trigger: 'blur' } ],
        money: [ { required: true, trigger: 'blur' } ]
      }
    }
  },
  watch: {
    data (val) {
      Object.assign(this.formData, val)
    },
    'formData.travel': {
      deep: true,
      handler () {
        console.log('travel change')
      }
    }
  },
  computed: {
    travelTotal () {
      // let total = this.formData.travel.reduce((prev, curr) => {
      //   prev = prev + curr.money
      //   return prev
      // }, 0)
      // console.log(total > 0 ? total : '', 'total')
      // return total > 0 ? total : ''
      return 2222
    }
  },
  methods: {
    process (val) {
      const excludeList = ['pictures', 'travel', 'traffic', 'accommodation', 'other']
      let result = {}
      for (let key in val) {
        if (!excludeList.includes(key)) {
          result[key] = val[key]
        }
      }
      return result
    },
    processIcon (icon, index, data) { // 处理上下移动图标的展示
      return !(
        (icon === 'el-icon-top' && index === 0) ||
        (icon === 'el-icon-bottom' && index === data.length - 1) || 
        (icon === 'el-icon-delete' && index === 0)
      )
    },
    async validate () {
      console.log(this.$refs.travelForm, this.$refs, 'refs')
      let isValid = await this.$refs.travelForm.validate()
      console.log(isValid, 'isValid')
    },
    getFileList (val) {
      this.formData.pictures = val
    },
    getTrafficList (val, data, index) {
      this.$set(data[index], 'order', val)
      // this.formData.traffic
    },
    add (scope, data, type) {
      switch (type) {
        case 'travel':
          data.push({
            day: '',
            money: '',
            remark: ''
          })
      }
    },
    copy(scope, data, type) {
      let { row } = scope
      switch (type) {
        case 'travel':
          data.push({
            day: row.day,
            money: row.money,
            remark: row.remark
          })
      }
    },
    delete (scope, data) {
      data.splice(scope.$index, 1)
    },
    up (scope, data) {
      let { $index } = scope
      let prevIndex = $index - 1
      let currentItem = data[$index]
      this.$set(data, $index, data[prevIndex])
      this.$set(data, prevIndex, currentItem)
    },
    down (scope, data) {
      let { $index } = scope
      let lastIndex = $index + 1
      let currentItem = data[$index]
      this.$set(data, $index, data[lastIndex])
      this.$set(data, lastIndex, currentItem)
      // let 
    },
    getSummaries ({ columns, data }) {
      console.log(columns, data, 'getSum')
      const sums = []
      columns.forEach((column, index) => {
        if (index === 0) {
          sums[index] = '总金额'
          return
        }
        if (column.property === 'money') {
          const values = data.map(item => Number(item[column.property]))
          if (!values.every(value => isNaN(value))) {
            sums[index] = values.reduce((prev, curr) => {
              const value = Number(curr);
              if (!isNaN(value)) {
                return prev + curr;
              } else {
                return prev;
              }
            }, 0);
            sums[index] = '￥' + toThousands(sums[index]);
          }
        }
      })
      return sums
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.order-wrapper {
  .form-wrapper {
    ::v-deep .el-form-item--mini {
      margin-bottom: 0;
    }
  }
  .icon-item {
    font-size: 17px;
    margin: 5px;
    cursor: pointer;
  }
}
</style>