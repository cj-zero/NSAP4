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
		assetCategory() {
			//alert("类别有：" + JSON.stringify(this.options[SYS_AssetCategory]))
			return this.options[SYS_AssetCategory] || []
		},
		assetStatus() {
			//console.log(this.options, 'status')
			//alert("状态有：" + JSON.stringify(this.options[SYS_AssetStatus]))
			return this.options[SYS_AssetStatus] || []
		},
		assetSJType() {
			//alert("送检类型有：" + JSON.stringify(this.options[SYS_AssetSJType]))
			return this.options[SYS_AssetSJType] || []
		},
		assetSJWay() {
			//alert("送检方式有：" + JSON.stringify(this.options[SYS_AssetSJWay]))
			return this.options[SYS_AssetSJWay] || []
		},
		nondeterminacy() {
			console.log(JSON.stringify(this.options[SYS_CategoryNondeterminacy]))
			return this.options[SYS_CategoryNondeterminacy] || []
		}
	},
	methods: {
		getassetSJWay(type) {
			var data = this.options[SYS_AssetSJWay]
			var arr = []
			for (let item of data) {
				if (type == '校准') {
					if (item.value == '内校' || item.value == '外校') {
						arr.push(item.value)
						//console.log("这是校准：" + JSON.stringify(arr))
					}
				} else if (type == '鉴定') {
					if (item.value == '外检') {
						arr.push(item.value)
						//console.log("这是鉴定：" + JSON.stringify(arr))
					}
				}
			}
			console.log(JSON.stringify(arr))
			return arr || []
		},
	}
}
