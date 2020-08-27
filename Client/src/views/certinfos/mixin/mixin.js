import { certVerificate } from '@/api/cerfiticate'
export let certVerMixin = { // 审核操作mixin
  data () {
    return {
      isSend: false, // 是否发送请求
      isLeftSend: false, // 左侧请求按钮loading
      isRightSend: false // 右侧请求按钮loading
    }
  },
  methods: {
    _certVerificate (data, type, message, direction, verificationOpinion) { // type 0: 表示通过、送审等正常操作 2: 表示不通过、撤回等失败操作 3: 退回
      if (this.isSend) return
      if (
        (direction === 'left' && this.isLeftSend === true) ||
        (direction === 'right' && this.isRightSend === true)
      ) {
        return
      }
      direction === 'left' ? (this.isLeftSend = true) : (this.isRightSend = true)
      let { id, flowInstanceId } = data
      certVerificate({
        certInfoId: id,
        verification: {
          flowInstanceId,
          verificationFinally: Number(type) === 0 ? '1' : '3',
          verificationOpinion: verificationOpinion || '',
          nodeRejectStep: '',
          nodeRejectType: 0
        }
      }).then(() => {
        this.$message({
          message: `${message}成功`
        })
        this.isLeftSend = false
        this.isRightSend = false
        this.$emit('handleSubmit')
      }).catch((err) => {
        this.isLeftSend = false
        this.isRightSend = false
        this.$emit('close')
        // this.$message.error(`${message}失败`)
        this.$message.error(`${err.message}`)
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