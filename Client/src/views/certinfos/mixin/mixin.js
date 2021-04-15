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
      direction ? (direction === 'left' ? (this.isLeftSend = true) : (this.isRightSend = true)) : this.isSend = true
      let { id, flowInstanceId } = data
      certVerificate([{
        certInfoId: id,
        verification: {
          flowInstanceId,
          verificationFinally: Number(type) === 4 ? 4 : ( Number(type) === 0 ? '1' : '3'),
          verificationOpinion: verificationOpinion || '',
          nodeRejectStep: '',
          nodeRejectType: Number(type) === 4 ? 1 : 0
        }
      }]).then(() => {
        this.$message({
          message: `${message}成功`
        })
        this.$emit('handleSubmit')
      }).catch((err) => {
        this.$emit('close')
        // this.$message.error(`${message}失败`)
        this.$message.error(`${err.message}`)
      }).finally(() => {
        this.isLeftSend = false
        this.isRightSend = false
        this.isSend = false
      })
    }
  }
}

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
      isBtnLock: false // 防止重复点击批量
    }
  },
  methods: {
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
    onSearch (val) {
      this.pageConfig = Object.assign({}, this.pageConfig, val)
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