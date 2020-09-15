<template>
  <div class="order-wrapper">
    <common-form
      :data="formData"
      :config="formConfig"
    >
      <template v-slot:attachment>
        <upLoadFile  @get-ImgList="getFileList" :limit="limit" uploadType="file" ref="uploadFile"></upLoadFile>
      </template>
      <template v-slot:travel>
        <Table :data="formData.travel" :columns="travelColumns"></Table>
      </template>
    </common-form>
    
  </div>
</template>

<script>
import { EXPENSE_CATEGORY } from '@/utils/declaration'
import CommonForm from './CommonForm'
import Table from '@/components/table'
import upLoadFile from "@/components/upLoadFile";
export default {
  components: {
    CommonForm,
    Table,
    upLoadFile
  },
  data () {
    return {
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
          day: '3',
          money: '2',
          remark: '1'
        }],
        traffic: [],
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
        { label: '呼叫主题', prop: 'theme', palceholder: '请输入内容', disabled: true, required: true, col: 18 },
        { label: '填报事件', prop: 'fillDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true },
        { label: '设备类型', prop: 'materialType', palceholder: '请输入内容', disabled: true, required: true, col: 6 },
        { label: '解决方案', prop: 'solution', palceholder: '请输入内容', disabled: true, required: true, col: 6 },
        { label: '服务报告', prop: 'report',  disabled: true, required: true, col: 6, type: 'inline-slot', id: 'report', isEnd: true },
        { label: '费用承担', prop: 'expense', palceholder: '请输入内容', disabled: true, required: true, col: 6 },
        { label: '责任承担', prop: 'responsibility', palceholder: '请输入内容', disabled: true, required: true, col: 6 },
        { label: '劳务关系', prop: 'laborRelation', palceholder: '请输入内容', disabled: true, required: true, col: 6 },
        { label: '支付时间', prop: 'payDate', palceholder: '请输入内容', disabled: true, required: true, col: 6, isEnd: true },
        { label: '备注', prop: 'remark', palceholder: '请输入内容', disabled: true, required: true, col: 18 },
        { label: '总金额', prop: 'totalMoney', col: 6, isEnd: true, type: 'inline-slot', id: 'money' },
        { label: '附件', prop: 'pictures', type: 'slot', id: 'attachment', fullLine: true },
        { label: '出差补贴', prop: 'travel', type: 'slot', id: 'travel', fullLine: true },
        { label: '交通费用', prop: 'traffic', type: 'slot', id: 'traffic', fullLine: true },
        { label: '住宿补贴', prop: 'accommodation', type: 'slot', id: 'accommodation', fullLine: true },
        { label: '其他费用', prop: 'other', type: 'slot', id: 'other', fullLine: true },
      ],
      limit: 8,
      travelColumns: [{
        label: '出差补贴',
        children: [{
          label: '天数',
          prop: 'day',
          width: 100,
          // type: 'input'
        }, {
          label: '金额',
          prop: 'money',
          width: 200,
          // type: 'input'
        }, {
          label: '备注',
          prop: 'remark',
          width: 100,
          // type: 'input'
        }]
      }],
      // travelColumns:  [{
      //     label: '天数',
      //     prop: 'day'
      //   }, {
      //     label: '金额',
      //     prop: 'money'
      //   }, {
      //     label: '备注',
      //     prop: 'remark'
      //   }]
      // item
    }
  },
  watch: {
    data (val) {
      Object.assign(this.formData, val)
    }
  },
  computed: {
    
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
    getFileList (val) {
      this.formData.pictures = val
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
</style>