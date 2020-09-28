<template>
	<div>
		<el-form :model="formData" class="demo-form-inline" label-width="90px">
			<el-row>
				<el-col :span="3">
					<el-form-item label="资产ID">
						<el-input v-model="formData.user" placeholder="审批人"></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="3">
					<el-form-item label="类别">
						<el-select v-model="formData.assetCategory" placeholder="请选择">
							<el-option v-for="item in assetCategory" :value="item.value" :label="item.label" :key="item.value"></el-option>
						</el-select>
					</el-form-item>
				</el-col>
				<el-col :span="3">
					<el-form-item label="型号">
						<el-input placeholder="请输入内容" v-model="formData.assetType"></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="3">
					<el-form-item label="出厂编号">
						<el-input placeholder="请输入内容" v-model="formData.assetCCNumber"></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="3">
					<el-form-item label="资产编号">
						<el-input placeholder="请输入内容" v-model="formData.assetZCNumber"></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="3">
					<el-form-item label="部门">
						<el-input placeholder="请输入内容" v-model="formData.orgName"></el-input>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row>
				<el-col :span="4">
					<el-form-item label="类型">
						<el-select v-model="formData.assetSJType" placeholder="请选择">
							<el-option v-for="item in assetSJType" :value="item.value" :label="item.label" :key="item.value"></el-option>
						</el-select>
					</el-form-item>
				</el-col>
				<el-col :span="4">
					<el-form-item label="状态">
						<el-select v-model="formData.assetStatus" placeholder="请选择状态">
							<el-option v-for="item in assetStatus" :value="item.value" :label="item.label" :key="item.value"></el-option>
						</el-select>
					</el-form-item>
				</el-col>
				<el-col :span="6">
					<el-form-item label="最近校准时间" label-width="120px">
					<el-date-picker v-model="formData.date" type="daterange" range-separator="至" start-placeholder="开始日期"
					end-placeholder="结束日期">
					</el-date-picker>
					</el-form-item>
				</el-col>
				<el-col :span="1">
					<el-form-item>
						<el-button type="primary" @click="search" size="mini" icon="el-icon-search">查询</el-button>
					</el-form-item>
				</el-col>
				<el-col :span="3">
					<el-form-item>
						<el-button type="primary" @click="newzc" size="mini" icon="el-icon-news">新增</el-button>
					</el-form-item>
				</el-col>
			</el-row>
		</el-form>
		<!-- 弹窗 -->
		<el-dialog :visible.sync="dialogFormVisible" width="60%">
			<el-tabs>
				<el-tab-pane label="详情">
					<add :options='options' @cancel='oncancel'></add>
				</el-tab-pane>
			</el-tabs>
			<!-- <div slot="footer" class="dialog-footer">
				<el-button size="mini" @click='dialogFormVisible=false'>取 消</el-button>
				<el-button type="primary" size="mini">确 定</el-button>
			</div> -->
		</el-dialog>
	</div>
</template>
<script>
	import assetMixin from './mixin/mixin'
	import Add from './add'
	export default {
		components: {
			Add
		},
		props: {
			options: {
				type: Object,
				default () {
					return {}
				}
			}
		},
		mixins: [assetMixin],
		data() {
			return {
				formData: {
					id: '',
					assetCategory: '',
					assetType: '',
					assetCCNumber: '',
					assetZCNumber: '',
					orgName: '',
					assetSJType: '',
					assetStatus: '',
					assetJZData: '',
					assetSXDate: '',
					date: ''
				},
				dialogFormVisible: false,
				//options: {}
			}
		},
		methods: {
			search() {
				this.$emit('search', this.$data.formData)
				//alert("触发了搜索事件："+JSON.stringify(this.$data.FormData))
			},
			newzc() {
				this.$data.dialogFormVisible = true
			},
			oncancel(val)
			{
				this.$data.dialogFormVisible=val
			}
		},
		created() {

		},
		mounted() {

		},
	}
</script>
<style lang='scss' scoped>
