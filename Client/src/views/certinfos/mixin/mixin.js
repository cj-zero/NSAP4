import { certVerificate } from '@/api/cerfiticate'
export let certVerMixin = { // 审核操作mixin
  data () {
    return {
      isSend: false // 是否发送请求
    }
  },
  methods: {
    _certVerificate (data, type, message, verificationOpinion) { // type 0: 表示通过、送审等正常操作 2: 表示不通过、撤回等失败操作 3: 退回
      // this.isSend = true
      if (this.isSend) return
      this.isSend = true
      let { id, flowInstanceId } = data
      console.log(type, 'setType')
      certVerificate({
        certInfoId: id,
        verification: {
          flowInstanceId,
          verificationFinally: Number(type) === 0 ? '1' : '3',
          verificationOpinion: verificationOpinion || '',
          nodeRejectStep: '',
          nodeRejectType: 0
        }
      }).then((res) => {
        console.log(res, '审核操作')
        this.$message({
          message: `${message}成功`
        })
        this.isSend = false
        this.$emit('handleSubmit')
      }).catch(() => {
        this.isSend = false
        this.$emit('close')
        this.$message.error(`${message}失败`)
      })
    }
  }
}

export let commonMixin = {
  data () {
    return {
      tableData: [], // 表格数据
      pageConfig: { // 分页配置
        page: 1,
        limit: 20
      },
      visible: false, // 弹窗
      currentCertNo: '', // 当前的证书编号
      currentData: {}, // 当前弹窗项的数据
      isLoading: true
    }
  },
  methods: {
    handleChange (pageConfig) { // 分页
      this.pageConfig = Object.assign({}, this.pageConfig, pageConfig)
      this.type === 'query' ? this._getQueryList() : this._loadApprover()
    },
    onSearch (val) {
      this.pageConfig = Object.assign({}, this.pageConfig, val)
      this.type === 'query' ? this._getQueryList() : this._loadApprover()
    },
    onOpenDetail (params) {
      console.log(params, 'params')
      let { id, certNo } = params
      this.currentCertNo = certNo
      this.currentData = params
      if (this.type === 'query') {
        this.currentId = id
      }
      this.visible = true
    },
    onHandleSubmit () {
      this.type === 'query' ? this._getQueryList() : this._loadApprover()
      this.closeDialog()
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
  },
  computed: {
    totalCount () {
      return this.tableData.length
    }
  }
}