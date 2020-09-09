const SYS_AssetCategory = 'SYS_AssetCategory' //类别
const SYS_AssetStatus = 'SYS_AssetStatus' // 状态
const SYS_AssetSJType = 'SYS_AssetSJType' // 送检类型
const SYS_AssetSJWay = 'SYS_AssetSJWay' // 送检方式
const SYS_CategoryNondeterminacy = 'SYS_CategoryNondeterminacy' // 阻值类型

export default {
  props: {
    options: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  computed: {
    assetCategory () {
      return this.options[SYS_AssetCategory] || []
    }, 
    assetStatus () {
      console.log(this.options, 'status')
      return this.options[SYS_AssetStatus] || []
    },
    assetSJType () {
      return this.options[SYS_AssetSJType] || []
    },
    assetSJWay () {
      return this.options[SYS_AssetSJWay] || []
    },
    nondeterminacy () {
      return this.options[SYS_CategoryNondeterminacy] || []
    }
  }
}