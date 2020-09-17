<template>
  <div class="order-wrapper">
    <common-form
      :data="formData"
      :config="formConfig"
    >
      <template v-slot:attachment>
        <upLoadFile  @get-ImgList="getFileList" :limit="limit" uploadType="file" ref="uploadFile"></upLoadFile>
      </template>
      <!-- 出差 -->
      <template v-slot:travel>
        <div class="form-item-wrapper">
          <el-button v-if="ifShowTravel" @click="showForm('ifShowTravel')">添加出差补贴</el-button>
          <el-form 
            v-else
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
              @row-click="onTravelRowClick"
              @cell-click="onTravelCellClick"
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
                        :rules="travelRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'number'">
                      <el-form-item
                        :prop="'travel.' + scope.$index + '.'+ item.prop"
                        :rules="travelRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]" :type="item.type" :min="0"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'select'">
                      <el-form-item
                        :prop="'travel.' + scope.$index + '.'+ item.prop"
                        :rules="travelRules[item.prop] || { required: false }"
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
                    <template v-else-if="item.type === 'operation'">
                      <template v-for="iconItem in item.iconList">
                        <i 
                          v-if="processIcon(iconItem.icon, scope.$index, formData.travel)"
                          :key="iconItem.icon"
                          :class="iconItem.icon" 
                          class="icon-item"
                          @click="iconItem.handleClick(scope, formData.travel, 'travel', iconItem.operationType)">
                        </i>
                      </template>
                    </template>
                  </template>
                </el-table-column>
              </el-table-column>
            </el-table>
          </el-form>
        </div>
      </template>
      
      <!-- 交通 -->
      <template v-slot:traffic>
        <div class="form-item-wrapper">
          <el-button v-if="ifShowTraffic" @click="showForm('ifShowTraffic')">添加出差补贴</el-button>
          <el-form 
            v-else
            ref="trafficForm" 
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
              @row-click="onTrafficRowClick"
              @cell-click="onTrafficCellClick"
            >
              <el-table-column label="交通费用" header-align="center">
                <el-table-column
                  v-for="item in trafficConfig"
                  :key="item.label"
                  :label="item.label"
                  :align="item.align || 'left'"
                  :prop="item.prop"
                  :width="item.width"
                >
                  <template slot-scope="scope">
                    <template v-if="item.type === 'order'">
                      {{ scope.$index + 1 }}
                    </template>
                    <template v-else-if="item.type === 'input'">
                      <el-form-item
                        :prop="'traffic.' + scope.$index + '.'+ item.prop"
                        :rules="trafficRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'number'">
                      <el-form-item
                        :prop="'traffic.' + scope.$index + '.'+ item.prop"
                        :rules="trafficRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'select'">
                      <el-form-item
                        :prop="'traffic.' + scope.$index + '.'+ item.prop"
                        :rules="trafficRules[item.prop] || { required: false }"
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
                      <el-form-item
                        :prop="'traffic.' + scope.$index + '.'+ item.prop"
                        :rules="trafficRules[item.prop] || { required: false }"
                      >
                        <upLoadFile  @get-ImgList="getTrafficList" :limit="limit" uploadType="file" ref="trafficUploadFile"></upLoadFile>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'operation'">
                      <template v-for="iconItem in item.iconList">
                        <i 
                          v-if="processIcon(iconItem.icon, scope.$index, formData.traffic)"
                          :key="iconItem.icon"
                          :class="iconItem.icon" 
                          class="icon-item"
                          @click="iconItem.handleClick(scope, formData.traffic, 'traffic', iconItem.operationType)">
                        </i>
                      </template>
                    </template>
                  </template>
                </el-table-column>
              </el-table-column>
            </el-table>
          </el-form>
        </div>
        
      </template> 
      <template v-slot:accommodation>
        <div class="form-item-wrapper">
          <el-button v-if="ifShowAcc" @click="showForm('ifShowAcc')">添加出差补贴</el-button>
          <el-form 
          v-else
          ref="accForm" 
          :model="formData" 
          size="mini" 
          :show-message="false"
          class="form-wrapper"
          >
            <el-table 
              :data="formData.accommodation"
              :summary-method="getSummaries"
              show-summary
              max-height="300px"
              @row-click="onAccRowClick"
              @cell-click="onAccCellClick"
            >
              <el-table-column label="住房补贴" header-align="center">
                <el-table-column
                  v-for="item in accommodationConfig"
                  :key="item.label"
                  :label="item.label"
                  :align="item.align || 'left'"
                  :prop="item.prop"
                  :width="item.width"
                >
                  <template slot-scope="scope">
                    <template v-if="item.type === 'order'">
                      {{ scope.$index + 1 }}
                    </template>
                    <template v-else-if="item.type === 'input'">
                      <el-form-item
                        :prop="'accommodation.' + scope.$index + '.'+ item.prop"
                        :rules="accRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]" :disabled="item.disabled" @change="onChange"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'number'">
                      <el-form-item
                        :prop="'accommodation.' + scope.$index + '.'+ item.prop"
                        :rules="accRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'select'">
                      <el-form-item
                        :prop="'accommodation.' + scope.$index + '.'+ item.prop"
                        :rules="accRules[item.prop] || { required: false }"
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
                      <el-form-item
                        :prop="'accommodation.' + scope.$index + '.'+ item.prop"
                        :rules="accRules[item.prop] || { required: false }"
                      >
                        <upLoadFile  @get-ImgList="getAccList" :limit="limit" uploadType="file" ref="accUploadFile"></upLoadFile>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'operation'">
                      <template v-for="iconItem in item.iconList">
                        <i 
                          v-if="processIcon(iconItem.icon, scope.$index, formData.accommodation)"
                          :key="iconItem.icon"
                          :class="iconItem.icon" 
                          class="icon-item"
                          @click="iconItem.handleClick(scope, formData.accommodation, 'accommodation', iconItem.operationType)">
                        </i>
                      </template>
                    </template>
                  </template>
                </el-table-column>
              </el-table-column>
            </el-table>
          </el-form>
        </div>
      </template> 

      <template v-slot:other>
        <div class="form-item-wrapper">
          <el-button v-if="ifShowOther" @click="showForm('ifShowOther')">添加出差补贴</el-button>
          <el-form 
            v-else
            ref="otherForm" 
            :model="formData" 
            size="mini" 
            :show-message="false"
            class="form-wrapper"
          >
            <el-table 
              :data="formData.other"
              :summary-method="getSummaries"
              show-summary
              max-height="300px"
              @row-click="onOtherRowClick"
              @cell-click="onOtherCellClick"
            >
              <el-table-column label="其他费用" header-align="center">
                <el-table-column
                  v-for="item in otherConfig"
                  :key="item.label"
                  :label="item.label"
                  :align="item.align || 'left'"
                  :prop="item.prop"
                  :width="item.width"
                >
                  <template slot-scope="scope">
                    <template v-if="item.type === 'order'">
                      {{ scope.$index + 1 }}
                    </template>
                    <template v-else-if="item.type === 'input'">
                      <el-form-item
                        :prop="'other.' + scope.$index + '.'+ item.prop"
                        :rules="otherRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]" :disabled="item.disabled" @change="onChange"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'number'">
                      <el-form-item
                        :prop="'other.' + scope.$index + '.'+ item.prop"
                        :rules="otherRules[item.prop] || { required: false }"
                      >
                        <el-input v-model="scope.row[item.prop]" :type="item.type" :disabled="item.disabled" :min="0"></el-input>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'select'">
                      <el-form-item
                        :prop="'other.' + scope.$index + '.'+ item.prop"
                        :rules="otherRules[item.prop] || { required: false }"
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
                      <el-form-item
                        :prop="'other.' + scope.$index + '.'+ item.prop"
                        :rules="otherRules[item.prop] || { required: false }"
                      >
                        <upLoadFile  @get-ImgList="getOtherList" :limit="limit" uploadType="file" ref="otherUploadFile"></upLoadFile>
                      </el-form-item>
                    </template>
                    <template v-else-if="item.type === 'operation'">
                      <template v-for="iconItem in item.iconList">
                        <i 
                          v-if="processIcon(iconItem.icon, scope.$index, formData.other)"
                          :key="iconItem.icon"
                          :class="iconItem.icon" 
                          class="icon-item"
                          @click="iconItem.handleClick(scope, formData.other, 'other', iconItem.operationType)">
                        </i>
                      </template>
                    </template>
                  </template>
                </el-table-column>
              </el-table-column>
            </el-table>
          </el-form>
        </div>
      </template> 
    </common-form>
    
    <el-button @click="validate">提交</el-button>
    <el-button @click="validateTraffic">提交交通</el-button>
    <el-button @click="validateAcc">提交住房</el-button>
    <el-button @click="validateOther">提交其他</el-button>
  </div>
</template>

<script>
import { EXPENSE_CATEGORY } from '@/utils/declaration'
import CommonForm from './CommonForm'
import upLoadFile from "@/components/upLoadFile";
import { TRAVEL_MONEY, TRAFFIC_TYPES, TRAFFIC_WAY, OTHER_EXPENSE_TYPES } from '../js/type'
import { toThousands } from '@/utils/format'
import { findIndex } from '@/utils/process'
// import { checkMoney } from '../js/customerRules'
export default {
  components: {
    CommonForm,
    upLoadFile
  },
  data () {
    let iconList = [
      { icon: 'el-icon-document-add', handleClick: this.addAndCopy, operationType: 'add' }, 
      { icon: 'el-icon-document-copy', handleClick: this.addAndCopy, operationType: 'copy' }, 
      { icon: 'el-icon-top', handleClick: this.up },
      { icon: 'el-icon-bottom', handleClick: this.down },
      { icon: 'el-icon-delete', handleClick: this.delete }
    ]
    let validateMoney = (rule, value, callback) => {
      value = Number(value)
      console.log(value, 'validaMoney')
      if (value === '' || isNaN(value) || typeof value !== 'number' || value <= 0) {
        callback(new Error());
      } else {
        callback();
      }
    };
    return {
      ifShowTraffic: false, // 是否展示交通补贴表格， 以下类似
      ifShowOther: false,
      ifShowAcc: false,
      ifShowTravel: false,
      currentIndex: 0, // 用来标记当前点击单元格所在表格中的索引值
      currentProp: '', // 当前选中的单元格的property, 对应table数据的key值
      currentLabel: '', // 当前选中的单元格的property, 对应table数据的label值
      currentRow: '', // 当前选中的
      maxSize: 1000,
      formData: { // 表单参数
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
          trafficType: '',
          trafficWay: '',
          origin: '',
          destination: '',
          money: '',
          remark: '',
          invoice: '1',
          invoiceAttachment: [],
          otherAttachment: []
        }],
        accommodation: [{
          day: '',
          itemMoney: '',
          money: '',
          remark: '',
          invoice: '1',
          invoiceAttachment: [],
          otherAttachment: []
        }],
        other: [{
          expenseType: '',
          money: '',
          remark: '',
          invoice: '1',
          invoiceAttachment: [],
          otherAttachment: []
        }]
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
        { label: '附件', prop: 'pictures', type: 'slot', id: 'attachment', showLabel: true },
        { label: '出差补贴', prop: 'travel', type: 'slot', id: 'travel' },
        { label: '交通费用', prop: 'traffic', type: 'slot', id: 'traffic' },
        { label: '住宿补贴', prop: 'accommodation', type: 'slot', id: 'accommodation' },
        { label: '其他费用', prop: 'other', type: 'slot', id: 'other' }
      ],
      limit: 8,
      travelConfig: [ // 出差配置
        { label: '天数', prop: 'day', type: 'number', fixed: true },
        { label: '金额', align: 'right', prop: 'money', type: 'select', options: TRAVEL_MONEY },
        { label: '备注', prop: 'remark', type: 'input' },
        { label: '操作', type: 'operation', iconList, fixed: 'right' }
      ],
      trafficConfig: [ // 交通配置
        { label: '序号', type: 'order', width: 60, fixed: true },
        { label: '交通类型', prop: 'trafficType', type: 'select', options: TRAFFIC_TYPES, width: 120 },
        { label: '交通工具', prop: 'trafficWay', type: 'select', options: TRAFFIC_WAY, width: 120 },
        { label: '出发地', prop: 'origin', type: 'input', width: 100 },
        { label: '目的地', prop: 'destination', type: 'input', width: 100 },
        { label: '金额', prop: 'money', type: 'number', align: 'right', width: 150 },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', disabled: true, type: 'input', prop: 'invoice', width: 100 },
        { label: '发票附件', disabled: true, type: 'upload', prop: 'invoiceAttachment', width: 150, fixed: 'right' },
        { label: '其他附件', disabled: true, type: 'upload', prop: 'otherAttachment', width: 150, fixed: 'right' },
        { label: '操作', type: 'operation', iconList, fixed: 'right', width: 160 }
      ],
      accommodationConfig: [ // 住房配置
        { label: '序号', type: 'order', width: 60, fixed: true },
        { label: '天数', prop: 'day', type: 'number', width: 100 },
        { label: '金额', prop: 'itemMoney', type: 'number', width: 120, disabled: true, align: 'right' },
        { label: '总金额', prop: 'money', type: 'number', width: 120, align: 'right' },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', disabled: true, type: 'input', prop: 'invoice', width: 100 },
        { label: '发票附件', disabled: true, type: 'upload', prop: 'invoiceAttachment', width: 150, fixed: 'right' },
        { label: '其他附件', disabled: true, type: 'upload', prop: 'otherAttachment', width: 150, fixed: 'right' },
        { label: '操作', type: 'operation', iconList, fixed: 'right', width: 160 }
      ],
      otherConfig: [ // 其他配置
        { label: '序号', type: 'order', width: 60, fixed: true },
        { label: '费用类别', prop: 'expenseType', type: 'select', width: 150, align: 'right', options: OTHER_EXPENSE_TYPES },
        { label: '其他费用', prop: 'money', type: 'number', width: 120, align: 'right' },
        { label: '备注', prop: 'remark', type: 'input', width: 100 },
        { label: '发票号码', disabled: true, type: 'input', prop: 'invoice', width: 100 },
        { label: '发票附件', disabled: true, type: 'upload', prop: 'invoiceAttachment', width: 150, fixed: 'right' },
        { label: '其他附件', disabled: true, type: 'upload', prop: 'otherAttachment', width: 150, fixed: 'right' },
        { label: '操作', type: 'operation', iconList, fixed: 'right', width: 160 }
      ],
      travelRules: {
        day: [ { required: true, trigger: 'blur', validator: validateMoney } ],
        money: [ { required: true, trigger: 'blur' } ],
      },
      trafficRules: {
        trafficType: [ { required: true, trigger: 'change' } ],
        trafficWay: [ { required: true, trigger: 'change' } ],
        origin: [ { required: true, trigger: 'blur' } ],
        destination: [ { required: true, trigger: 'blur' } ],
        money: [ { required: true, trigger: ['blur', 'change'], validator: validateMoney } ],
        invoice: [ { required: true } ],
      },
      accRules: {
        day: { required: true, trigger: 'change', validator: validateMoney } ,
        itemMoney: [ { required: true, trigger: 'change', disabled: true } ],
        money: [ { required: true, trigger: 'blur', validator: validateMoney } ],
        destination: [ { required: true, trigger: 'blur' } ],
        invoice: [ { required: true } ],
      },
      otherRules: {
        expenseType: [ { required: true, trigger: 'change' } ],
        money: [{ required: true, trigger: 'blur', validator: validateMoney }],
        invoice: [ { required: true } ],
      }
    }
  },
  watch: {
    data (val) {
      Object.assign(this.formData, val)
    },
    totalMoney (val) {
      this.formData.totalMoney = val
    }
  },
  computed: {
    totalMoney () {
      return 1
    }
  },
  methods: {
    showForm (type) { // 展示表格
      this[type] = true
    },
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
      console.log(isValid, 'isValid', this.formData)
    },
    async validateTraffic () {
      let ifInvoiceAttachment = this.formData.traffic.every(item => item.invoiceAttachment && item.invoiceAttachment.length)
      console.log(ifInvoiceAttachment, 'ifInvoice')
      let isValid = await this.$refs.trafficForm.validate()
      console.log('trafficValid', isValid, this.formData)
    },
    async validateAcc () {
      let ifInvoiceAttachment = this.formData.accommodation.every(item => item.invoiceAttachment && item.invoiceAttachment.length)
      console.log(ifInvoiceAttachment, 'ifInvoice')
      let isValid = await this.$refs.accForm.validate()
      console.log('AccValid', isValid, this.formData)
    },
    async validateOther () {
      let ifInvoiceAttachment = this.formData.other.every(item => item.invoiceAttachment && item.invoiceAttachment.length)
      console.log(ifInvoiceAttachment, 'ifInvoice')
      let isValid = await this.$refs.otherForm.validate()
      console.log('otherValid', isValid, this.formData)
    },
    getFileList (val) {
      this.formData.pictures = val
      console.log(this.formData.pictures, 'fileList')
    },
    getTrafficList (val) {
      let data = this.formData.traffic
      console.log(val, 'trafficList')
      this.$set(data[this.currentIndex], this.currentProp, val)
      console.log(data, 'trafficData')
    },
    getAccList (val) {
      console.log('ACCData', this.formData)
      let data = this.formData.accommodation
      this.$set(data[this.currentIndex], this.currentProp, val)
      console.log(data, 'ACCData', this.formData)
    },
    getOtherList (val) {
      console.log('OtherData', this.formData)
      let data = this.formData.other
      this.$set(data[this.currentIndex], this.currentProp, val)
      console.log(data, 'OtherData', this.formData)
    },
    onTravelRowClick (row) {
      this.setCurrentIndex(this.formData.travel, row)
    },
    onTrafficRowClick (row) {
      this.setCurrentIndex(this.formData.traffic, row)
    },
    onAccRowClick (row) {
      this.setCurrentIndex(this.formData.accommodation, row)
    },
    onOtherRowClick (row) {
      this.setCurrentIndex(this.formData.other, row)
    },
    setCurrentIndex (data, row) {
      this.currentRow = row
      this.currentIndex = findIndex(data, item => item === row)
      console.log(this.currentIndex, 'index')
    },
    onTravelCellClick (row, column) {
      this.setCurrentProp(column, row)
      // console.log(row, column, cell, event, 'cellChange')
    },
    onTrafficCellClick (row, column) {
      this.setCurrentProp(column, row)
      // console.log(row, column, cell, event, 'cellChange')
    },
    onAccCellClick (row, column) {
      this.setCurrentProp(column, row)
    },
    onOtherCellClick (row, column) {
      this.setCurrentProp(column, row)
    },
    setCurrentProp ({ label, property }) {
      this.currentLabel = label
      this.currentProp = property
      console.log(this.currentProp, 'prop')
    },
    onChange (value) {
      if (!this.isValidaNumber(value)) {
        return
      }
      if (this.currentProp === 'money' || this.currentProp === 'day') {
        let data = this.formData.accommodation[this.currentIndex]
        let { day, money } = data
        if (!day || !this.isValidaNumber(day) || !money || !this.isValidaNumber(money)) { // 如果天数没有填入,或者不符合规范则直接return
          return
        }
        this.$set(data, 'itemMoney', money / day)
        console.log(this.formData, 'formData')
      }
    },
    addAndCopy (scope, data, type, operationType) {
      console.log(operationType, 'operationType')
      let { row } = scope
      switch (type) {
        case 'travel':
          data.push({
            day: operationType === 'add' ? '' : row.day,
            money: operationType === 'add' ? '' : row.money,
            remark: operationType === 'add' ? '' : row.remark,
          })
          break
        case 'traffic':
          data.push({
            trafficType: operationType === 'add' ? '' : row.trafficType,
            trafficWay: operationType === 'add' ? '' : row.trafficWay,
            origin: operationType === 'add' ? '' : row.origin,
            destination: operationType === 'add' ? '' : row.destination,
            money: operationType === 'add' ? '' : row.money,
            remark: operationType === 'add' ? '' : row.remark,
            invoice: '',
            invoiceAttachment: '',
            otherAttachment: []
          })
          break
        case 'accommodation':
          data.push({
            day: operationType === 'add' ? '' : row.day,
            itemMoney: operationType === 'add' ? '' : row.itemMoney,
            money: operationType === 'add' ? '' : row.money,
            remark: operationType === 'add' ? '' : row.remark,
            invoice: '',
            invoiceAttachment: [],
            otherAttachment: []
          })
          break
        case 'other':
          data.push({
            expenseType: operationType === 'add' ? '' : row.expenseType,
            money: operationType === 'add' ? '' : row.money,
            remark: operationType === 'add' ? '' : row.remark,
            invoice: '',
            invoiceAttachment: [],
            otherAttachment: []
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
    isValidaNumber (val) { // 判断是否是有效的数字
      val = Number(val)
      return !isNaN(val) && val >= 0
    },
    getSummaries ({ columns, data }) { // 金额合计
      console.log(columns, data, 'getSum')
      const sums = []
      columns.forEach((column, index) => {
        if (index === 0) {
          sums[index] = '总金额'
          return
        }
        if (column.property === 'money') {
          const values = data.map(item => Number(item[column.property]))
          if (values.every(value => {
            return !isNaN(value) && value >= 0
          })) {
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
    },
    resetInfo () {
      this.formData = { // 表单参数
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
          trafficType: '',
          trafficWay: '',
          origin: '',
          destination: '',
          money: '',
          remark: '',
          invoice: '1',
          invoiceAttachment: [],
          otherAttachment: []
        }],
        accommodation: [{
          day: '',
          itemMoney: '',
          money: '',
          remark: '',
          invoice: '1',
          invoiceAttachment: [],
          otherAttachment: []
        }],
        other: [{
          expenseType: '',
          money: '',
          remark: '',
          invoice: '1',
          invoiceAttachment: [],
          otherAttachment: []
        }]
      }
      this.ifShowTraffic = this.ifShowOther = this.ifShowAcc = this.ifShowTravel = false
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
  max-height: 600px;
  overflow-y: auto;
  .form-item-wrapper {
    margin-bottom: 20px;
  }
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