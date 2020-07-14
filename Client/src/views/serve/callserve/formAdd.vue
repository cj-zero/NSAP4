<template>
  <div>
    <div style="border:1px silver solid;padding:5px;margin-left:10px;">
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <el-form-item label="工单ID">
            <el-input v-model="form.internalSerialNumber" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="服务类型">
            <el-radio-group v-model="form.feeType">
              <el-radio label="免费"></el-radio>
              <el-radio label="收费"></el-radio>
            </el-radio-group>
          </el-form-item>
        </el-col>

       <el-col :span="6" style="height:40px;line-height:40px;">
              
              <el-switch v-model="form.edit" active-text="是否批量修改后续订单"></el-switch>
             
            </el-col>
            <el-col :span="2" style="height:40px;line-height:40px;">
               <el-button
                type="danger"
                v-if="formList.length>0"
                style="margin:0 10px;"
                size="mini"
                @click="deleteForm(1)"
              >删除</el-button>
            </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <el-form-item label="制造商序列号">
            <el-autocomplete
              popper-class="my-autocomplete"
              v-model="form.manufacturerSerialNumber"
              :fetch-suggestions="querySearch"
              placeholder="请输入内容"
              @select="handleSelect"
            >
              <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
              <template slot-scope="{ item }">
                <div class="name">{{ item.manufSN }}</div>
                <span class="addr">{{ item.custmrName }}</span>
              </template>
            </el-autocomplete>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="内部序列号">
            <el-input v-model="form.internalSerialNumber" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
            <el-form-item label="服务合同">
                <el-input v-model="form.internalSerialNumber" disabled></el-input>
              </el-form-item>
          <!-- <el-form-item label="保修结束日期">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择开始日期"
              v-model="form.warrantyEndDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item> -->
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <el-form-item label="物料编码">
            <el-input v-model="form.materialCode" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="16">
          <el-form-item label="物料描述">
            <el-input disabled v-model="form.itemName"></el-input>
          </el-form-item>
        </el-col>
      </el-row>

      <!-- </el-col>
      </el-row>-->
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="16">
          <el-form-item label="呼叫主题">
            <el-input v-model="form.name"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="保修结束日期">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择开始日期"
              v-model="form.warrantyEndDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
        <el-form-item label="呼叫类型">
                <el-input v-model="form.fromType"></el-input>
              </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="呼叫状态">
            <el-input v-model="form.status" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
           <el-form-item label="清算日期">
                <el-date-picker
                  disabled
                  type="date"
                  placeholder="选择日期"
                  v-model="form.liquidationDate"
                  style="width: 100%;"
                ></el-date-picker>
              </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
       <el-form-item label="问题类型">
            <el-input v-model="form.problemTypeId"></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="优先级">
            <el-select v-model="form.name" clearable placeholder="请选择">
              <el-option
                v-for="item in options_quick"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="预约时间">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择开始日期"
              v-model="form.bookingDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="16">
          <el-form-item label="解决方案">
            <el-input v-model="form.problemTypeId"></el-input>
          </el-form-item>
        </el-col>

        <el-col :span="8">
          <el-form-item label="结束时间">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择日期"
              v-model="form.startTime"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>


      <el-form-item label="备注" prop="remark">
        <el-input type="textarea" v-model="form.remark"></el-input>
      </el-form-item>
      <el-form-item label>
        <div class="showSort">1/{{formList.length+1}}</div>
      </el-form-item>
    </div>
    <!-- form数组，不包括第一项 -->
    <el-collapse v-model="activeNames" v-if="formList.length">
      <el-collapse-item title="展开更多序列号表单" style="color:green;" name="1">
        <div
          v-for="(form,index) in formList"
          :key="`key_${index}`"
          style="border:1px solid silver;padding:5px;margin:2px;"
        >
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
              <el-form-item label="工单ID">
                <el-input v-model="form.internalSerialNumber" disabled></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="服务类型">
                <el-radio-group v-model="form.feeType">
                  <el-radio label="免费"></el-radio>
                  <el-radio label="收费"></el-radio>
                </el-radio-group>
              </el-form-item>
            </el-col>

            <el-col :span="6" style="height:40px;line-height:40px;">
              
              <el-switch v-model="form.edit" active-text="是否批量修改后续订单"></el-switch>
             
            </el-col>
            <el-col :span="2" style="height:40px;line-height:40px;">
               <el-button
                type="danger"
                v-if="formList.length>0"
                style="margin:0 10px;"
                size="mini"
                @click="deleteForm(form)"
              >删除</el-button>
            </el-col>
          </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
              <el-form-item label="制造商序列号">
                <el-autocomplete
                  popper-class="my-autocomplete"
                  v-model="form.manufacturerSerialNumber"
                  :fetch-suggestions="querySearch"
                  placeholder="请输入内容"
                  @select="handleSelect"
                >
                  <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
                  <template slot-scope="{ item }">
                    <div class="name">{{ item.manufSN }}</div>
                    <span class="addr">{{ item.custmrName }}</span>
                  </template>
                </el-autocomplete>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="内部序列号">
                <el-input v-model="form.internalSerialNumber" disabled></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
               <el-form-item label="服务合同">
                <el-input v-model="form.internalSerialNumber" disabled></el-input>
              </el-form-item>
              <!-- <el-form-item label="保修结束日期">
                <el-date-picker
                  disabled
                  type="date"
                  placeholder="选择开始日期"
                  v-model="form.warrantyEndDate"
                  style="width: 100%;"
                ></el-date-picker>
              </el-form-item> -->
            </el-col>
          </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
              <el-form-item label="物料编码">
                <el-input v-model="form.materialCode" disabled></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="16">
              <el-form-item label="物料描述">
                <el-input disabled v-model="form.itemName"></el-input>
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="服务类型">
            <el-radio-group v-model="form.feeType">
              <el-radio label="免费"></el-radio>
              <el-radio label="收费"></el-radio>
            </el-radio-group>
          </el-form-item>
          <!-- </el-col>
          </el-row>-->
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="16">
              <el-form-item label="呼叫主题">
                <el-input v-model="form.name"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="接单员">
                <el-form-item label="保修结束日期">
            <el-date-picker
              disabled
              type="date"
              placeholder="选择开始日期"
              v-model="form.warrantyEndDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
           <el-form-item label="呼叫类型">
                <el-input v-model="form.fromType"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="呼叫状态">
                <el-input v-model="form.status" disabled></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
                 <el-form-item label="清算日期">
                <el-date-picker
                  disabled
                  type="date"
                  placeholder="选择日期"
                  v-model="form.liquidationDate"
                  style="width: 100%;"
                ></el-date-picker>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
               <el-form-item label="问题类型">
            <el-input v-model="form.problemTypeId"></el-input>
          </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="优先级">
                <el-input v-model="form.name"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="预约时间">
                <el-date-picker
                  disabled
                  type="date"
                  placeholder="选择开始日期"
                  v-model="form.bookingDate"
                  style="width: 100%;"
                ></el-date-picker>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
           <el-col :span="16">
          <el-form-item label="解决方案">
            <el-input v-model="form.problemTypeId"></el-input>
          </el-form-item>
        </el-col>
            <el-col :span="8">
              <el-form-item label="结束时间">
                <el-date-picker
                  disabled
                  type="date"
                  placeholder="选择日期"
                  v-model="form.startTime"
                  style="width: 100%;"
                ></el-date-picker>
              </el-form-item>
            </el-col>
          </el-row>


          <el-form-item label="备注" prop="remark">
            <el-input type="textarea" v-model="form.remark"></el-input>
          </el-form-item>
          <el-form-item label>
            <div class="showSort">{{index+2}}/{{formList.length+1}}</div>
          </el-form-item>
        </div>
      </el-collapse-item>
    </el-collapse>
    <!--  -->
    <el-dialog
      destroy-on-close
      title="选择制造商序列号"
      @open="openDialog"
      width="90%"
      :visible.sync="dialogfSN"
    >
      <div style="width:200px;margin:10px 0;">
        <el-autocomplete
          popper-class="my-autocomplete"
          v-model="inputSearch"
          :fetch-suggestions="querySearch"
          placeholder="内容制造商序列号"
          @select="searchSelect"
          @input="searchList"
        >
          <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
          <template slot-scope="{ item }">
            <div class="name">{{ item.manufSN }}</div>
            <span class="addr">{{ item.custmrName }}</span>
          </template>
        </el-autocomplete>
      </div>
      <fromfSN :SerialNumberList="filterSerialNumberList" @change-Form="changeForm"></fromfSN>

      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogfSN = false">取 消</el-button>
        <el-button type="primary" @click="pushForm">确 定</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
// import { getPartner } from "@/api/callserve";
import fromfSN from "./fromfSN";

export default {
  components: { fromfSN },
  props: ["SerialNumberList"],
  data() {
    return {
      filterSerialNumberList: [],
      formListStart: [], //选择的表格数据
      formList: [], //表单依赖的表格数据
      dialogfSN: false,
      inputSearch: "",
      activeNames: ["1"], //活跃名称
      // form: {
      //   manufSN: "",
      //   internalSN: "", //内部序列号
      //   itemCode: "", //物料编码
      //   itemName: "", //物料描述
      //   dlvryDate: "",
      //   delivery: false,
      //   type: [],
      //   resource: "",
      //   name: "",
      //   edit:1,
      // },
      form: {
        priority: 1, //优先级 4-紧急 3-高 2-中 1-低
        feeType: "", //服务类型 1-免费 2-收费
        province: "", //省
        city: "", //市
        addr: "", //详细地址
        longitude: "", //经度
        latitude: "", //纬度
        submitDate: "", //工单提交时间
        recepUserId: null, //接单人用户Id
        remark: "", //备注
        status: 1, //呼叫状态 1-待确认 2-已确认 3-已取消 4-待处理 5-已排配 6-已外出 7-已挂起 8-已接收 9-已解决 10-已回访
        fromTheme: "", //App当前流程处理用户Id
        fromId: 1, //呼叫来源 1-电话 2-APP
        problemTypeId: "", //问题类型Id
        fromType: 2, //呼叫类型1-提交呼叫 2-在线解答（已解决）
        materialCode: "", //物料编码
        materialDescription: "", //物料描述
        manufacturerSerialNumber: "", //制造商序列号
        internalSerialNumber: "", //内部序列号
        warrantyEndDate: "", //保修结束日期
        bookingDate: "", //预约日期
        visitTime: "", //上门时间
        liquidationDate: "", //清算日期
        addressDesignator: "" //地址标识
      },
      options_sourse: [
        { value: "电话", label: "电话" },
        { value: "钉钉", label: "钉钉" },
        { value: "QQ", label: "QQ" },
        { value: "微信", label: "微信" },
        { value: "邮件", label: "邮件" },
        { value: "APP", label: "APP" },
        { value: "Web", label: "Web" }
      ],
      options_type: [
        { value: "提交呼叫", label: "提交呼叫" },
        { value: "在线解答（已解决）", label: "在线解答（已解决）" }
      ], //呼叫类型
      options_quick: [
        { value: "高", label: "高" },
        { value: "中", label: "中" },
        { value: "底", label: "底" }
      ],
      ifFormPush: false //表单是否被动态添加过
    };
  },
  created() {},

  mounted() {
    this.filterSerialNumberList = this.SerialNumberList;
  },
  watch: {
    // filterSerialNumberList: function(newQuestion, oldQuestion) {
    //   // console.log(newQuestion, oldQuestion);
    // }
  },
  computed: {
    //  SerialNumberList: {
    //     get: function(a) {
    //       return a
    //     },
    //     set: function(a) {
    //        return a
    //     }
    //   }
  },
  methods: {
    openDialog() {
      this.filterSerialNumberList = this.SerialNumberList;
    },
    changeForm(res) {
      this.formListStart = res;
      console.log(this.formListStart);
    },
    pushForm() {
      this.dialogfSN = false;
      if (!this.ifFormPush) {
        this.form.manufSN = this.formListStart[0].manufSN;
        this.form.internalSerialNumber = this.formListStart[0].internalSerialNumber;
        this.form.itemCode = this.formListStart[0].itemCode;
        this.form.itemName = this.formListStart[0].itemName;
        this.form.dlvryDate = this.formListStart[0].dlvryDate;

        const newList = this.formListStart.splice(1, this.formListStart.length);
        for (let i = 0; i < newList.length; i++) {
          this.formList.push({
            manufSN: newList[i].manufSN,
            internalSerialNumber: newList[i].internalSerialNumber,
            itemCode: newList[i].itemCode,
            itemName: newList[i].itemName,
            dlvryDate: newList[i].dlvryDate
          });
        }
        this.ifFormPush = true;
      } else {
        this.ifFormPush = true;
        for (let i = 0; i < this.formListStart.length; i++) {
          this.formList.push({
            manufSN: this.formListStart[i].manufSN,
            internalSerialNumber: this.formListStart[i].internalSerialNumber,
            itemCode: this.formListStart[i].itemCode,
            itemName: this.formListStart[i].itemName,
            dlvryDate: this.formListStart[i].dlvryDate
          });
        }
      }
    },
    handleIconClick() {
      this.dialogfSN = true;
    },
    searchList(res) {
      if (!res) {
        this.filterSerialNumberList = this.SerialNumberList;
      } else {
        let list = this.SerialNumberList.filter(item => {
          return item.manufSN.indexOf(res) > 0;
        });
        this.filterSerialNumberList = list;
      }
    },
    querySearch(queryString, cb) {
      var filterSerialNumberList = this.SerialNumberList;
      var results = queryString
        ? filterSerialNumberList.filter(this.createFilter(queryString))
        : filterSerialNumberList;
      // 调用 callback 返回建议列表的数据
      cb(results);
    },
    createFilter(queryString) {
      return filterSerialNumberList => {
        return (
          filterSerialNumberList.manufSN
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    handleSelect(item) {
      console.log(item);
    },
    deleteForm(res) {
      if (!res || res === 1) {
        if (res === 1) {
          this.$confirm(`此操作将删除该表单, 是否继续?`, "提示", {
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            type: "warning"
          }).then(() => {
            this.form.manufSN = this.formList[0].manufSN;
            this.form.internalSerialNumber = this.formList[0].internalSerialNumber;
            this.form.itemCode = this.formList[0].itemCode;
            this.form.itemName = this.formList[0].itemName;
            this.form.dlvryDate = this.formList[0].dlvryDate;
            const newList = this.formList.splice(1, this.formList.length);
            this.formList = [];
            for (let i = 0; i < newList.length; i++) {
              this.formList.push({
                manufSN: newList[i].manufSN,
                internalSerialNumber: newList[i].internalSerialNumber,
                itemCode: newList[i].itemCode,
                itemName: newList[i].itemName,
                dlvryDate: newList[i].dlvryDate
              });
            }
            this.ifFormPush = true;
            this.$message({
              type: "success",
              message: "删除成功!"
            });
          }).catch(() => {
            this.$message({
              type: "info",
              message: "已取消删除"
            });
          });
        } else {
          this.$message({
            type: "warning",
            message: "选择无效!"
          });
        }
      } else {
        this.$confirm(
          `此操作将删除序列商序列号为${res.manufSN}的表单, 是否继续?`,
          "提示",
          {
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            type: "warning"
          }
        )
          .then(() => {
            this.formList = this.formList.filter(item => {
              return item.manufSN != res.manufSN;
            });
            this.$message({
              type: "success",
              message: "删除成功!"
            });
          })
          .catch(() => {
            this.$message({
              type: "info",
              message: "已取消删除"
            });
          });
      }
    },
    searchSelect(res) {
      let newList = this.filterSerialNumberList.filter(
        item => item.manufSN === res.manufSN
      );
      this.inputSearch = res.manufSN;
      this.filterSerialNumberList = newList;
    }
  }
};
</script>

<style lang="scss" scoped>
.showSort {
  float: right;
  height: 30px;
  width: 130px;
  line-height: 30px;
  font-size: 24px;
}
</style>