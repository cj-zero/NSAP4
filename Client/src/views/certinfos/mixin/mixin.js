import { certVerificate } from '@/api/cerfiticate'
export let certVerMixin = { // 审核操作mixin
  data () {
    return {
      isSend: false
    }
  },
  methods: {
    _certVerificate (data, type, message, verificationOpinion) { // type 1: 表示通过、送审等正常操作 2: 表示不通过、撤回等失败操作 3: 退回
      this.isSend = true
      console.log(this.isSend, 'isSEND')
      let { id, flowInstanceId } = data
      certVerificate([{
        certInfoId: id,
        verification: {
          flowInstanceId,
          verificationFinally: type,
          verificationOpinion: verificationOpinion || '',
          nodeRejectStep: '',
          nodeRejectType: Number(type) === 1 ? 0 : 1
        }
      }]).then(() => {
        this.$message.success(`${message}成功`)
        this.isSend = false
        this.onHandleSubmit ? this.onHandleSubmit() : this.$emit('handleSubmit')
      }).catch((err) => {
        this.isSend = false
        this.$emit('close')
        // this.$message.error(`${message}失败`)
        this.$message.error(`${err.message}`)
      })
    }
  }
}
const W_100 = { width: '100px' }
const W_370 = { width: '370px', display: 'inline-flex' }
const activityStatusOptions = [
  { label: '全部', value: '' },
  { label: '待审核', value: '1' },
  { label: '待批准', value: '2' },
]
export let commonMixin = {
  data () {
    return {
      tableData: [], // 表格数据
      totalCount: 0,
      pageConfig: { // 分页配置
        page: 1,
        limit: 50
      },
      visible: false, // 弹窗
      currentCertNo: '', // 当前的证书编号
      currentData: {}, // 当前弹窗项的数据
      isLoading: true,
      selectList: [],
      isBtnLock: false, // 防止重复点击批量
    }
  },
  computed: {
    searchConfig () {
      return [
        { prop: 'certNo', component: { attrs: { placeholder: '证书编号', style: W_100 } } },
        { prop: 'model', component: { attrs: { placeholder: '型号规格', style: W_100 } } },
        { prop: 'sn', component: { attrs: { placeholder: '出厂编号', style: W_100 } } },
        { prop: 'assetNo', component: { attrs: { placeholder: '资产编号', style: W_100 } } },
        { prop: 'operator', component: { attrs: { placeholder: '校准人', style: W_100 } }, isShow: this.type !== 'submit' },
        { prop: 'activityStatus', component: { tag: 'select', attrs: { options: activityStatusOptions, placeholder: '状态', style: W_100 } }, isShow: this.type === 'review' },
        { prop: 'date', component: { 
          tag: 'date',
          attrs: { type: 'daterange', 'range-separator': '至', 'start-placeholder': '开始日期', 'end-placeholder': '结束日期', style: W_370, clearable: true }, 
          on: { change: this.onDateChange } } 
        },
        { component: { tag: 's-button', attrs: { btnText: '搜索', type: 'primary' }, on: { click: this.onSearch } } },
        { component: { tag: 's-button', attrs: { btnText: this.type === 'submit' ? '一键送审' : '一键审批', type: 'primary' }, on: { click: this.onApprove } }, isShow: this.type === 'review' || this.type === 'submit' },
      ]
    }
  },
  methods: {
    onDateChange (val) {
      this.pageConfig.calibrationDateFrom = val ? val[0] : ''
      this.pageConfig.calibrationDateTo = val ? val[1] : ''
    },
    _certVerificateList (list) { // 批量审批
      if (this.isBtnLock) return
      list = list.map(item => {
        let { id, flowInstanceId } = item
        return {
          certInfoId: id,
          verification: {
            flowInstanceId,
            verificationFinally: '1',
            verificationOpinion: ''
          }
        }
      })
      this.isBtnLock = true
      this.$confirm('确定一键审核？', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消'
      }).then(() => {
        this.isLoading = true
        certVerificate(list).then(res => {
          this.$message.success(res.message)
          this._loadApprover()
          this.isBtnLock = false
        }).catch(err => {
          this.isBtnLock = false
          this.$message.error(err.message)
          this.isLoading = false
        })
      })
    },
    onApprove () {
      if (!this.selectList.length) {
        return this.$message.warning('请先选择数据')
      }
      this._certVerificateList(this.selectList)
    },
    onSelection (val) {
      this.selectList = val
    },
    handleChange (pageConfig) { // 分页
      this.pageConfig = Object.assign({}, this.pageConfig, pageConfig)
      this.type === 'query' ? this._getQueryList() : this._loadApprover()
    },
    onSearch () {
      // this.pageConfig = Object.assign({}, this.pageConfig, val)
      this.pageConfig.page = 1
      this.type === 'query' ? this._getQueryList() : this._loadApprover()
    },
    onOpenDetail (params, hasSend) {
      let { id, certNo } = params
      this.currentCertNo = certNo
      this.currentData = params
      if (this.type === 'query') {
        this.currentId = id
      }
      this.hasSend = hasSend
      this.visible = true
    },
    onHandleSubmit () {
      setTimeout(() => {
        this.type === 'query' ? this._getQueryList() : this._loadApprover()
        this.closeDialog()
      }, 300)
    },
    onClosed () {
      this.activeName = 'first'
    },
    closeDialog () {
      this.visible = false
    },
    formatDate (date) { // 截取表格的date类型
      return date.split(' ')[0]
    },
    handleClick () {}
  }  
}