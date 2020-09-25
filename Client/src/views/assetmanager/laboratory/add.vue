<template>
	<div>
		<el-form :model="formData" :rules="rule" ref='formData' :disabled="disable" position="left" label-width="110px" size="mini">
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="资产ID" required>
						<el-input disabled style="width: 200px" v-model="formData.id"></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="状态" prop="assetStatus" required>
						<el-select v-model="formData.assetStatus" placeholder="请选择">
							<el-option v-for="item in assetStatus" :value="item.value" :label="item.label" :key="item.value"></el-option>
						</el-select>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="类别" prop="assetCategory" required>
						<el-select v-model="formData.assetCategory" placeholder="请选择" @change="onTypeChange">
							<el-option v-for="item in assetCategory" :value="item.value" :label="item.label" :key="item.value"></el-option>
						</el-select>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="部门" prop="orgName" required>
						<el-autocomplete v-model="formData.orgName" :fetch-suggestions='querySearchOrg' placeholder="请输入部门" @select="handleSelect"></el-autocomplete>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="型号" prop="assetType" required>
						<el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetType"></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="持有者" prop="assetHolder" required>
						<el-autocomplete v-model="formData.assetHolder" :fetch-suggestions="queryUser" placeholder="请输入持有者" @select="handleSelectUser"></el-autocomplete>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="出厂编号S/N" prop="assetStockNumber" required>
						<el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetStockNumber"></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="管理员" prop="assetAdmin" required>
						<el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetAdmin"></el-input>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="资产编号" required>
						<el-input style="width: 200px" v-model="formData.assetNumber" disabled></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="制造厂" prop="assetFactory" required>
						<el-input style="width: 200px" placeholder="请输入内容" v-model="formData.assetFactory"></el-input>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="送检类型" prop="assetInspectType" required>
						<el-select v-model="formData.assetInspectType" placeholder="请选择" @change='changeSJWay'>
							<el-option v-for="item in assetSJType" :value="item.value" :label="item.label" :key="item.value"></el-option>
						</el-select>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="送检方式" prop="assetInspectWay" required placeholder="请选择">
						<el-select v-model="formData.assetInspectWay">
							<el-option v-for="item in this.sjTypes" :value="item" :label="item" :key="item"></el-option>
						</el-select>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="校准日期" prop="assetStartDate" required>
						<el-date-picker v-model="formData.assetStartDate" type="date" @change="getsxdate" placeholder="选择日期" value-format="yyyy-MM-dd">
						</el-date-picker>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="校准证书" required>
						<el-upload ref="assetCalibrationCertificate" name="files" class="avatar-uploader" :action="action" :headers="headers"
						:show-file-list="false" :on-success="handleSuccessJZ" :before-upload="beforeAvatarUpload">
							<div v-if="formData.assetCalibrationCertificate" class="upload-img" @click.stop>
								<img v-if="formData.assetCalibrationCertificate" :src="imglist.showzsimg" class="upload-img">
								<div class="mask-wrapper">
									<i class="el-icon-zoom-in item" @click="previewImg('assetCalibrationCertificate')"></i>
									<i class="el-icon-delete item" @click="removeImg('assetCalibrationCertificate')"></i>
								</div>
							</div>
							<div v-else class="add-wrapper">
								<el-button type="primary" size="mini">添加文件</el-button>
							</div>
						</el-upload>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row type="flex" justify="space-around">
				<el-col :span="7">
					<el-form-item label="失效日期" prop="assetEndDate" required>
						<el-date-picker v-model="formData.assetEndDate" type="date" placeholder="选择日期" value-format="yyyy-MM-dd">
						</el-date-picker>
					</el-form-item>
				</el-col>
				<el-col :span="7">
					<el-form-item label="校准数据" required>
						<el-row type="flex" justify="space-around">
							<el-col :span="5">
								<el-upload ref="assetInspectDataOne" name="files" class="avatar-uploader" :action="action" :headers="headers"
								:show-file-list="false" :on-success="handleSuccessData1" :before-upload="beforeAvatarUpload">
									<div v-if="formData.assetInspectDataOne" class="upload-img" @click.stop>
										<img v-if="formData.assetInspectDataOne" :src="imglist.showzbimg" class="upload-img">
										<div class="mask-wrapper">
											<i class="el-icon-zoom-in item" @click="previewImg('assetInspectDataOne')"></i>
											<i class="el-icon-delete item" @click="removeImg('assetInspectDataOne')"></i>
										</div>
									</div>
									<div v-else class="add-wrapper">
										<el-button type="primary" size="mini">添加技术指标</el-button>
									</div>
								</el-upload>
							</el-col>
							<el-col :span="4">
								<el-upload ref="assetInspectDataTwo" name="files" class="avatar-uploader" :action="action" :headers="headers"
								:show-file-list="false" :on-success="handleSuccessData2" :before-upload="beforeAvatarUpload">
									<div v-if="formData.assetInspectDataTwo" class="upload-img" @click.stop>
										<img v-if="formData.assetInspectDataTwo" :src="imglist.showsjimg" class="upload-img">
										<div class="mask-wrapper">
											<i class="el-icon-zoom-in item" @click="previewImg('assetInspectDataTwo')"></i>
											<i class="el-icon-delete item" @click="removeImg('assetInspectDataTwo')"></i>
										</div>
									</div>
									<div v-else class="add-wrapper">
										<el-button type="primary" size="mini">添加校准数据</el-button>
									</div>
								</el-upload>
							</el-col>
						</el-row>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row style="left: 120px;">
				<el-form-item label="技术文件" required>
					<el-upload ref="assetTCF" name="files" class="avatar-uploader" :action="action" :headers="headers" :show-file-list="false"
					:on-success="handleSuccessJS" :before-upload="beforeAvatarUpload">
						<div v-if="formData.assetTCF" class="upload-img">
							<img v-if="formData.assetTCF" :src="imglist.showjsimg" class="upload-img">
							<div class="mask-wrapper">
								<i class="el-icon-zoom-in item" @click="previewImg('assetTCF')"></i>
								<i class="el-icon-delete item" @click="removeImg('assetTCF')"></i>
							</div>
						</div>
						<div v-else class="add-wrapper">
							<el-button type="primary" size="mini">添加文件</el-button>
						</div>
					</el-upload>
					<!-- <upLoadImage :limit="1" @get-ImgList="getImgList"></upLoadImage> -->
				</el-form-item>
			</el-row>
			<!--万用表 -->
			<el-table v-if="isshowmeter" border :data="formData.listcategory" :cell-style="cellStyle" :header-cell-style="headerCellStyle"
			@row-click="getcurrentRow">
				<el-table-column label="#" prop="categoryNumber" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryNumber" disabled ></el-input>
					</template>
				</el-table-column>
				<el-table-column label="测量值" prop="categoryOhms"  autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryOhms" disabled />
					</template>
				</el-table-column>
				<el-table-column label="不确定度(Unc)" prop="categoryNondeterminacy" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryNondeterminacy" type="text" />
					</template>
				</el-table-column>
				<el-table-column label="类型" prop="categoryType" autosize>
					<template slot-scope='scope'>
						<el-select placeholder="请选择" v-model="scope.row.categoryType" @change='changeMeterDW'>
							<el-option v-for="item in scope.row.categoryTypeList" :label="item" :value="item" :key="item"></el-option>
						</el-select>
					</template>
				</el-table-column>
				<el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryBHYZ" type="text" />
					</template>
				</el-table-column>
			</el-table>
			<!--工装-->
			<el-table v-else-if="isshowgz" border :data="formData.listcategory" :cell-style="cellStyle" :header-cell-style="headerCellStyle"
			@row-click="getcurrentRow">
				<el-table-column label="序号" prop="categoryNumber" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryNumber" disabled></el-input>
					</template>
				</el-table-column>
				<el-table-column label="阻值" prop="categoryOhms" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryOhms" type="text" />
					</template>
				</el-table-column>
				<el-table-column label="不确定度(Unc)" prop="categoryNondeterminacy" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryNondeterminacy" type="text" />
					</template>
				</el-table-column>
				<el-table-column label="类型" prop="categoryType" autosize>
					<template slot-scope='scope'>
						<el-select placeholder="请选择" v-model="scope.row.categoryType" @change='changeGzDW'>
							<el-option v-for="item in scope.row.categoryArr" :label="item" :value="item" :key="item"></el-option>
						</el-select>
					</template>
				</el-table-column>
				<el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryBHYZ" type="text" />
					</template>
				</el-table-column>
			</el-table>

			<!--分流器-->
			<el-table v-else-if="isshowflq" border :data="formData.listcategory" :cell-style="cellStyle" :header-cell-style="headerCellStyle"
			@row-click="getcurrentRow">
				<el-table-column label="序号" prop="categoryNumber" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryNumber" disabled></el-input>
					</template>
				</el-table-column>
				<el-table-column label="阻值" prop="categoryOhms" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryOhms" type="text"></el-input>
					</template>
				</el-table-column>
				<el-table-column label="不确定度(Unc)" prop="categoryNondeterminacy" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryNondeterminacy" type="text" />
					</template>
				</el-table-column>
				<el-table-column label="类型" prop="categoryType" autosize>
					<template slot-scope='scope'>
						<el-select placeholder="请选择" v-model="scope.row.categoryType" @change='changeFlqDW'>
							<el-option v-for="item in scope.row.catergoryflqArr" :label="item" :value="item" :key="item"></el-option>
						</el-select>
					</template>
				</el-table-column>
				<el-table-column label="包含因子(k)" prop="categoryBHYZ" autosize>
					<template slot-scope='scope'>
						<el-input v-model="scope.row.categoryBHYZ" type="text" />
					</template>
				</el-table-column>
			</el-table>
			<el-row>
				<el-form-item label="描述">
					<el-input type="textarea" v-model="formData.assetDescribe" autosize :rows="3"></el-input>
				</el-form-item>
			</el-row>
			<el-row>
				<el-form-item label="备注">
					<el-input type="textarea" v-model="formData.assetRemarks" autosize :rows="3"></el-input>
				</el-form-item>
			</el-row>
			<el-row>
				<el-form-item label="图片">
					<el-upload ref="assetImg" name="files" class="avatar-uploader" :action="action" :headers="headers" :show-file-list="false"
					:on-success="handleSuccessImg" :before-upload="beforeAvatarUpload">
						<img v-if="formData.assetImg" :src="imglist.img" class="upload-img">
						<i v-else class="el-icon-plus avatar-uploader-icon"></i>
						<i class="el-icon-plus avatar-uploader-icon"></i>
					</el-upload>
				</el-form-item>
			</el-row>

		</el-form>
		<el-form>
			<el-row>
				<el-form-item>
					<el-button v-if="iscancel" size="mini" @click='cancel'>取 消</el-button>
					<el-button v-if="isview" type="primary" size="mini" @click="view">预 览</el-button>
					<el-button v-if="isback" size="mini" @click="onback">返 回</el-button>
					<el-button v-if="issubmit" type="primary" size="mini" @click="onsubmit">确 认</el-button>
				</el-form-item>
			</el-row>
		</el-form>
		<!-- 预览图 -->
		<el-dialog :visible.sync="visible" top="10vh">
			<img :src="currentImg" width="100%">
		</el-dialog>
	</div>
</template>
<script>
	import {
		getListOrg,
		getListUser,
		add
	} from '@/api/assetmanagement'
	import assetMixin from './mixin/mixin'
	export default {
		mixins: [assetMixin],
		props: {
			type: {
				type: String,
				default: ''
			}
		},
		data() {
			return {
				config: {
					name: ''
				},
				//currentText:'预 览',
				iscancel: true, //是否关闭弹框
				isback: false, //是否返回编辑
				issubmit: false, //是否提交
				isview: true, //是否预览
				userconfig: {
					name: '',
					Orgid: ''
				},
				imglist: {
					showzsimg: '', //用于显示校准证书图片的字段
					showzbimg: '', //用于显示技术指标图片的字段
					showsjimg: '', //用于显示校准数据图片的字段
					showjsimg: '', //用于显示技术文件图片的字段
					img: '' //用于显示图片的字段
				},
				//arrs:[['相对不确定度(%)','绝对不确定度(V)'],['相对不确定度(%)','绝对不确定度(V)'],['相对不确定度(%)','绝对不确定度(A)'],['相对不确定度(%)','绝对不确定度(A)'],['相对不确定度(%)','绝对不确定度(Ω)']],
				formData: {
					id: '', // 资产ID
					assetStatus: '', // 状态
					assetSerial: '', //类别值
					assetCategory: '', // 类别
					orgName: '', // 部门
					assetType: '', // 型号
					assetHolder: '', // 持有者
					assetStockNumber: '', // 出场编号S/N
					assetAdmin: '', // 管理员
					assetNumber: '', // 资产编号
					assetFactory: '', // 制造厂
					assetInspectType: '', // 送检类型
					assetInspectWay: '', // 送检方式
					assetStartDate: '', // 校准日期
					assetCalibrationCertificate: '', // 校准证书
					assetEndDate: '', // 失效日期
					assetInspectDataOne: '', // 技术指标
					assetInspectDataTwo: '', // 校准数据
					assetTCF: '', // 技术文件
					assetDescribe: '', // 描述
					assetRemarks: '', // 备注
					assetImg: '', // 上传图片
					listcategory: [] //万用表、工装、分流器数据
				},
				rule: {
					assetStatusRes: [{
						required: true,
						message: '请选择状态',
						trigger: 'change'
					}],
					assetCategoryRes: [{
						required: true,
						message: '请选择类别',
						trigger: 'change'
					}],
					orgName: [{
						required: true,
						message: '请输入部门',
						trigger: 'blur'
					}],
					assetType: [{
						required: true,
						message: '请输入型号',
						trigger: 'blur'
					}],
					assetHolder: [{
						required: true,
						message: '请输入持有者',
						trigger: 'blur'
					}],
					assetCCNumber: [{
						required: true,
						message: '请输入出厂编号',
						trigger: 'blur'
					}],
					assetAdmin: [{
						required: true,
						message: '请输入管理员',
						trigger: 'blur'
					}],
					assetFactory: [{
						required: true,
						message: '请输入制造厂',
						trigger: 'blur'
					}],
					assetSJType: [{
						required: true,
						message: '请选择送检类型',
						trigger: 'change'
					}],
					assetSJWay: [{
						required: true,
						message: '请选择送检方式',
						trigger: 'change'
					}],
					assetJXDate: [{
						required: true,
						message: '请选择校准日期',
						trigger: 'change'
					}],
					assetJZCertificate: [{
						required: true,
						message: '请选择校准证书',
						trigger: 'blur'
					}],
					assetSXDate: [{
						required: true,
						message: '请选择失效日期',
						trigger: 'change'
					}],
					assetJZData1: [{
						required: true,
						message: '请选择技术指标',
						trigger: 'blur'
					}],
					assetJZData2: [{
						required: true,
						message: '请选择校准数据',
						trigger: 'blur'
					}],
					assetJSFile: [{
						required: true,
						message: '请选择技术文件',
						trigger: 'blur'
					}]
				},
				disable: false,
				currentIndex: '',
				orgs: [], //部门列表
				sjTypes: [], //送检类型列表
				users: [], //持有人列表
				action: `${process.env.VUE_APP_BASE_API}/Files/Upload`, // 文件上传地址
				headers: { // 上传标识
					"X-Token": this.$store.state.user.token
				},
				visible: false, // 预览图弹窗
				currentImg: '', // 当前preview的图片
				isshowmeter: false, //是否显示万用表
				isshowgz: false, //是否显示工装
				isshowflq: false, //是否显示分流器
			}
		},
		watch: {
			'formData.assetJZCertificate'(val) {
				this.toggleInputDisabled('assetJZCertificate', val)
			},
			'formData.assetJZData1'(val) {
				this.toggleInputDisabled('assetJZData1', val)
			},
			'formData.assetJZData2'(val) {
				this.toggleInputDisabled('assetJZData2', val)
			},
			'formData.assetJSFile'(val) {
				this.toggleInputDisabled('assetJSFile', val)
			},
			'formData.assetImg'(val) {
				this.toggleInputDisabled('assetImg', val)
			}
		},
		methods: {
			cellStyle() {
				return {
					'text-align': 'center',
					'vertical-align': 'middle'
				}
			},
			headerCellStyle({
				rowIndex
			}) {
				let style = {
					'text-align': 'center',
					'vertical-align': 'middle'
				}
				if (rowIndex === 1) {
					style.display = 'none'
				}
				return style
			},
			//获取失效日期
			getsxdate: function() {
				var year = new Date(this.formData.assetStartDate).getFullYear() + 1
				//alert(year)
				var month = new Date(this.formData.assetStartDate).getMonth() + 1
				var date = new Date(this.formData.assetStartDate).getDate() - 1
				this.formData.assetEndDate = year + '-' + month + '-' + date
			},
			changeSJWay() {
				this.sjTypes = this.getassetSJWay(this.formData.assetInspectType)
				//console.log("获取到的送检方式："+JSON.stringify(this.sjTypes))
			},
			getOrg() {
				return getListOrg(this.config).then(res => {
					for (let item of res.data) {
						this.orgs.push({
							value: item.name,
							id: item.id
						})
					}
				})
			},
			async querySearchOrg(queryString, cb) {
				this.config.name = queryString;
				await this.getOrg();
				var result = queryString ? this.orgs.filter(this.createStateFilter(queryString)) : this.orgs;
				this.orgs = [];
				cb(result);
			},
			createStateFilter(queryString) {
				return (state) => {
					return (state.value.toLowerCase().indexOf(queryString.toLowerCase()) === 0);
				};
			},
			submitForm(formName) {
				var isOK = false;
				this.$refs[formName].validate((valid) => {
					if (valid) {
						if (this.formData.assetJZData1 == '' || this.formData.assetJZData2 == '' || this.formData.assetJSFile == '' ||
							this.formData.assetJZCertificate == '') {
							this.$message.warning({
								message: '必须上传的文件没有全部上传',
								type: 'action'
							});
							isOK = false;
						} else {
							isOK = true;
						}
					} else {
						console.log('error submit!!');
						isOK = false;
					}
				});
				return isOK;
			},
			view() {
				if (this.submitForm('formData')) {
					this.disable = true;
					this.isback = true;
					this.issubmit = true;
					this.isview = false;
					this.iscancel = false;
				}
			},
			onback() {
				this.disable = false;
				this.disable = false;
				this.isback = false;
				this.issubmit = false;
				this.isview = true;
				this.iscancel = true;
			},
			onsubmit() {
				if(this.submitForm('formData'))
				{
					this.addzc(this.formData);
				}
			},
			cancel() {
				this.$emit('cancel', false)
			},
			handleSelect(val) {
				this.userconfig.Orgid = val.id
			},
			handleSelectUser(val) {
				this.userId = val.id
			},
			getuser() {
				return getListUser(this.userconfig).then(res => {
					//console.log("接口返回的数据："+JSON.stringify(res.data))
					for (let item of res.data) {
						this.users.push({
							value: item.name,
							id: item.id
						})
					}
				})
			},
			async queryUser(queryString, cb) {
				this.userconfig.name = queryString;
				await this.getuser();
				//alert(JSON.stringify(this.users));
				//console.log(JSON.stringify(this.users));
				var result = queryString ? this.users.filter(this.createStateFilter(queryString)) : this.users;
				//alert(result);
				this.users = [];
				cb(result);
			},
			//转换万用表单位
			changeMeterDW(type) {
				if (type.indexOf('V') != -1) {
					this.changeDW(this.formData.listcategory, 'V');
				} else if (type.indexOf('%') != -1) {
					this.changeDW(this.formData.listcategory, '%');
				} else if (type.indexOf('A') != -1) {
					this.changeDW(this.formData.listcategory, 'A');
				} else if (type.indexOf('Ω') != -1) {
					this.changeDW(this.formData.listcategory, 'Ω');
				}
			},
			//转换工装单位
			changeGzDW(type) {
				if (type.indexOf('%') != -1) {
					this.changeDW(this.formData.listcategory, '%');
				} else if (type.indexOf('ppm') != -1) {
					this.changeDW(this.formData.listcategory, 'ppm');
				} else if (type.indexOf('Ω') != -1) {
					this.changeDW(this.formData.listcategory, 'Ω');
				}
			},
			//转换分流器单位
			changeFlqDW(type) {
				if (type.indexOf('%') != -1) {
					this.changeDW(this.formData.listcategory, '%');
				} else if (type.indexOf('ppm') != -1) {
					this.changeDW(this.formData.listcategory, 'ppm');
				} else if (type.indexOf('Ω') != -1) {
					this.changeDW(this.formData.listcategory, 'Ω');
				}
			},
			changeDW(datas, val) {
				var currentVal = datas[this.currentIndex];
				//alert(this.currentIndex);
				if (currentVal == undefined) {
					//alert('请输入当前行的不确定度值！');
					this.$message.warning({
						message: '请输入当前行的不确定度值',
						type: 'action'
					})
					//alert(JSON.stringify(datas));
					return;
				}
				var data = datas[this.currentIndex].categoryNondeterminacy;
				//alert(data);
				if (data != '') {
					if (data.indexOf('V') != -1) {
						datas[this.currentIndex].categoryNondeterminacy = datas[this.currentIndex].categoryNondeterminacy.replace('V', val);
					} else if (data.indexOf('%') != -1) {
						datas[this.currentIndex].categoryNondeterminacy = datas[this.currentIndex].categoryNondeterminacy.replace('%', val);
					} else if (data.indexOf('ppm') != -1) {
						datas[this.currentIndex].categoryNondeterminacy = datas[this.currentIndex].categoryNondeterminacy.replace('ppm', val);
					} else if (data.indexOf('A') != -1) {
						datas[this.currentIndex].categoryNondeterminacy = datas[this.currentIndex].categoryNondeterminacy.replace('A', val);
					} else if (data.indexOf('Ω') != -1) {
						datas[this.currentIndex].categoryNondeterminacy = datas[this.currentIndex].categoryNondeterminacy.replace('Ω', val);
					} else {
						datas[this.currentIndex].categoryNondeterminacy = datas[this.currentIndex].categoryNondeterminacy + val;
					}
				}
			},
			//添加资产信息
			addzc(param) {
				add(param)
			},
			toggleInputDisabled(type, val) {
				let input = this.$refs[type].$el.childNodes[0].childNodes[1] // 取到input元素
				// console.log(val, 'val')
				if (val) { // 图片有值时
					input.setAttribute('disabled', 'disabled')
					// console.log(input, 'input')
				} else {
					setTimeout(() => {
						input.removeAttribute('disabled')
					}, 0)
				}
			},
			handleSuccessJZ(res, file) { // 校准证书图片
				this.setImg('showzsimg', file)
				this.formData.assetCalibrationCertificate = res.result[0].id;
				console.log("校准证书：" + this.formData.assetCalibrationCertificate);
			},
			handleSuccessData1(res, file) { // 数据1
				this.setImg('showzbimg', file)
				this.formData.assetInspectDataOne = res.result[0].id;
				console.log("技术指标：" + this.formData.assetInspectDataOne);
			},
			handleSuccessData2(res, file) { // 数据2
				this.setImg('showsjimg', file)
				this.formData.assetInspectDataTwo = res.result[0].id;
				console.log("校准数据：" + this.formData.assetInspectDataTwo);
			},
			handleSuccessJS(res, file) { // 技术文件
				this.setImg('showjsimg', file)
				this.formData.assetTCF = res.result[0].id;
				console.log("技术文件：" + this.formData.assetTCF);
			},
			handleSuccessImg(res, file) { // 图片
				this.setImg('img', file)
				//console.log("res："+JSON.stringify(res));
				this.formData.assetImg = res.result[0].id;
				console.log("图片：" + this.formData.assetImg);
			},
			removeImg(type) {
				this.formData[type] = ''
			},
			previewImg(type) {
				this.visible = true
				this.currentImg = this.formData[type]
			},
			handleAvatarSuccess(res, file) {
				// {
				//   pictureId:res.result[0].id
				// }
				this.formData.assetImg = URL.createObjectURL(file.raw);
			},
			setImg(type, file) {
				console.log('type：' + type);
				this.imglist[type] = URL.createObjectURL(file.raw);
				console.log("图片链接：" + this.imglist[type]);
				//this.formData[type]=file.response.result[0].id;
				//console.log(this.formData[type]);
			},
			beforeAvatarUpload(file) {
				let testmsg = /^image\/(jpeg|png|jpg)$/.test(file.type)
				if (!testmsg) {
					this.$message.error('上传图片格式不对!')
				}
				return testmsg
			},
			getcurrentRow(row) {
				this.currentIndex = this.formData.listcategory.findIndex(item => row == item);
				//console.log('当前为 '+this.currentIndex+'行');
			},
			onTypeChange(val) {
				var comonArr=[['绝对不确定度(Ω)','相对不确定度(ppm)','相对不确定度(%)']];
				switch (val) {
					case '万用表':
						this.isshowmeter = true;
						this.isshowgz = false;
						this.isshowflq = false;
						var nums = ['DCV', 'ACV', 'DCI', 'ACI', 'R'];
						var arrs=[['相对不确定度(%)','绝对不确定度(V)'],['相对不确定度(%)','绝对不确定度(V)'],['相对不确定度(%)','绝对不确定度(A)'],['相对不确定度(%)','绝对不确定度(A)'],['相对不确定度(%)','绝对不确定度(Ω)']];
						this.formData.listcategory = [];
						this.formData.assetSerial = '1';
						for (let i = 0; i < 5; i++) {
							this.formData.listcategory.push({
								id: '', //类别ID
								assetId: '', //资产ID
								categoryNumber: nums[i], //序号
								categoryOhms: '-', //阻值
								categoryNondeterminacy: '', //不确定度
								categoryType:'' , //不确定类型
								categoryTypeList:arrs[i],//不确定类型数组
								categoryBHYZ: '', //包含因子
								categoryAort: '' //排序
							}
							);
						}
						break;
					case '工装':
						this.isshowmeter = false;
						this.isshowgz = true;
						this.isshowflq = false;
						var arr = ['R1', 'R2', 'R3', 'R4'];
						this.formData.listcategory = [];
						this.formData.assetSerial = '2';
						for (let i = 0; i < 4; i++) {
							this.formData.listcategory.push({
								id: '', //类别ID
								assetId: '', //资产ID
								categoryNumber: arr[i], //序号
								categoryOhms: '', //阻值
								categoryNondeterminacy: '', //不确定度
								categoryType: '', //不确定类型
								categoryArr:comonArr[0],//不确定类型数组
								categoryBHYZ: '', //包含因子
								categoryAort: '' //排序
							})
						}
						break;
					case '分流器':
						this.isshowmeter = false;
						this.isshowgz = false;
						this.isshowflq = true;
						this.formData.listcategory = [];
						this.formData.assetSerial = '3';
						this.formData.listcategory.push({
							id: '', //类别ID
							assetId: '', //资产ID
							categoryNumber: 'R', //序号
							categoryOhms: '', //阻值
							categoryNondeterminacy: '', //不确定度
							categoryType: '', //不确定类型
							catergoryflqArr:comonArr[0],//不确定类型数组
							categoryBHYZ: '', //包含因子
							categoryAort: '' //排序
						})
						break;
					case '标准源':
					this.isshowmeter = false;
					this.isshowgz = false;
					this.isshowflq = false;
						this.formData.assetSerial = '4'
						break;
					default:
						this.isshowmeter = false;
						this.isshowgz = false;
						this.isshowflq = false;
						break;
				}

			}
		}
	}
</script>
<style lang='scss' scoped>
	.avatar-uploader {
		position: relative;
		width: 80px;
		height: 40px;

		::v-deep .el-upload {
			position: relative;
			width: 100%;
			height: 100%;
		}

		::v-deep .el-icon-plus,
		.avatar-uploader-icon {
			position: absolute;
			left: 0;
			bottom: 0;
			right: 0;
			top: 0;
			width: 15px;
			height: 15px;
			margin: auto;
		}

		.add-wrapper {
			display: flex;
			width: 100%;
			height: 100%;
			align-items: center;
			justify-content: center;
		}

		.upload-img {
			position: relative;
			width: 80px;
			height: 40px;

			&:hover .mask-wrapper {
				opacity: 1;
			}

			.mask-wrapper {
				position: absolute;
				display: flex;
				left: 0;
				right: 0;
				bottom: 0;
				top: 0;
				align-items: center;
				justify-content: center;
				background-color: rgba(0, 0, 0, .5);
				opacity: 0;
				transition: opacity .3s;

				.item {
					margin: 0 5px;
					color: white;
				}
			}
		}
	}

	body {
		::v-deep .popper__arrow {
			display: none;
		}

		::v-deep .el-select-dropdown {
			margin-top: 0 !important;
		}
	}
</style>
