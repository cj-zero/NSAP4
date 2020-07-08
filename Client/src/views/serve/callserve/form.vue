<template>
  <div class="form">
    <el-row :gutter="20">
      <el-col :span="isEdit?24:18">
        <el-form :model="form" :ref="refValue" :disabled="!isEdit" :label-width="labelwidth">
          <div style="padding:10px 0;"></div>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
              <el-form-item label="客户代码">
                <el-autocomplete
                  popper-class="my-autocomplete"
                  v-model="form.customerId"
                  :fetch-suggestions="querySearch"
                  placeholder="请输入内容"
                  @select="handleSelect"
                >
                  <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
                  <template slot-scope="{ item }">
                    <div class="name">{{ item.cardCode }}</div>
                    <span class="addr">{{ item.cardName }}</span>
                  </template>
                </el-autocomplete>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="服务ID">
                <el-select v-model="form.region" disabled placeholder="请选择">
                  <el-option label="服务一" value="shanghai"></el-option>
                  <el-option label="服务二" value="beijing"></el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="服务合同">
                <el-input v-model="form.contractId" disabled></el-input>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
              <el-form-item label="客户名称">
                <el-input v-model="form.customerName" disabled></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="联系人">
                <el-select v-model="form.contacter" placeholder="请选择">
                  <el-option label="服务一" value="shanghai"></el-option>
                  <el-option label="服务二" value="beijing"></el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="最近联系人">
                <el-input v-model="form.newestContacter"></el-input>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row type="flex" class="row-bg" justify="space-around">
            <el-col :span="8">
              <el-form-item label="终端客户">
                <el-input v-model="form.terminalCustomer"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="电话号码">
                <el-input v-model="form.contactTel"></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="最新电话号码">
                <el-input v-model="form.newestContactTel"></el-input>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="20">
            <el-col :span="16"></el-col>
            <el-col :span="8">
              <el-form-item label prop="resource">
                <el-radio-group v-model="form.name">
                  <el-radio label="售后审核"></el-radio>
                  <el-radio label="销售审核"></el-radio>
                </el-radio-group>
              </el-form-item>
            </el-col>
          </el-row>

          <!-- 选择制造商序列号 -->
          <formAdd :SerialNumberList="SerialNumberList"></formAdd>
          <el-dialog title="选择业务伙伴" width="90%" @open="openDialog" :visible.sync="dialogPartner">
            <el-row style="margin:10px 0;">
              <el-col :span="2">
                <el-button type="text">客户代码:</el-button>
              </el-col>
              <el-col :span="2">
                <el-input @input="searchList" v-model="inputSearch" placeholder="请输入客户代码"></el-input>
              </el-col>
              <el-col :span="2" style="margin-left:10px;">
                <el-button type="text">制造商序列号:</el-button>
              </el-col>
              <el-col :span="2">
                <el-input></el-input>
              </el-col>
            </el-row>
            <formPartner :partnerList="filterPartnerList"></formPartner>
            <span slot="footer" class="dialog-footer">
              <el-button @click="dialogPartner = false">取 消</el-button>
              <el-button type="primary" @click="dialogPartner = false">确 定</el-button>
            </span>
          </el-dialog>
        </el-form>
      </el-col>
      <el-col :span="6" class="lastWord" v-if="!isEdit">
        <el-collapse accordion>
          <el-collapse-item
            :title="`技术员：${item.people}`"
            :key="`key_${index}`"
            v-for="(item, index) in wordList"
            :name="index"
          >
            <el-form label-position="top" label-width="80px" :model="item">
              <el-form-item label="留言">
                <el-input v-model="item.word"></el-input>
              </el-form-item>
              <el-form-item label="活动形式">
                <el-input v-model="item.img"></el-input>
              </el-form-item>
              <el-button type="primary">立即创建</el-button>
            </el-form>
          </el-collapse-item>
        </el-collapse>
      </el-col>
    </el-row>
  </div>
</template>

<script>
import { getPartner, getSerialNumber } from "@/api/callserve";
import formPartner from "./formPartner";
import formAdd from "./formAdd";
export default {
  name: "formTable",
  components: { formPartner, formAdd },
  props: ["modelValue", "refValue", "labelposition", "labelwidth", "isEdit"],
  //  ##isEdit是否可以编辑
  data() {
    return {
      partnerList: [],
      filterPartnerList: [],
      inputSearch: "",
      SerialNumberList: [], //序列号列表
      state: "",
      dialogPartner: false,
      activeName: 1,
      // dataModel: this.models[this.formData.model],
      dataModel: null,
      wordList: [
        { people: "张工", word: "123", img: "" },
        { people: "刘总", word: "123", img: "" },
        { people: "实习生小王", word: "123", img: "" },
        { people: "老刘", word: "123", img: "" },
        { people: "门卫", word: "123", img: "" },
        { people: "实习生小李", word: "123", img: "" }
      ],
      form: {
        customerId:'',//客户代码,
        customerName:'',//客户名称,
        contacter	:'',//联系人,
        contactTel:'',//联系人电话,
        supervisor:'',//主管名字,
        supervisorId :'',//主管用户Id,
        salesMan:'',//销售名字,
        salesManId:'',//销售用户Id,
        newestContacter	:'',//最新联系人,
        newestContactTel	:'',//最新联系人电话号码,
        terminalCustomer:'',//终端客户,
        contractId:'',//服务合同
        serviceWorkOrders:[
          {
            
          }
        ]
              }
    };
  },
  watch: {},

  mounted() {
    this.getPartnerList();
    this.getSerialNumberList();
    this.filterPartnerList = this.partnerList;
  },
  methods: {
    openDialog() {
      //打开前赋值
      this.filterPartnerList = this.partnerList;
    },
    searchList(res) {
      if (!res) {
        this.filterPartnerList = this.partnerList;
      } else {
        let list = this.partnerList.filter(item => {
          return item.cardCode.toLowerCase().indexOf(res.toLowerCase()) === 0;
        });
        this.filterPartnerList = list;
      }
    },
    querySearch(queryString, cb) {
      var partnerList = this.partnerList;
      var results = queryString
        ? partnerList.filter(this.createFilter(queryString))
        : partnerList;
      // 调用 callback 返回建议列表的数据
      cb(results);
    },
    createFilter(queryString) {
      return partnerList => {
        console.log(partnerList);
        return (
          partnerList.cardCode
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    getSerialNumberList() {
      getSerialNumber()
        .then(res => {
          this.SerialNumberList = res.data;
        })
        .catch(error => {
          console.log(error);
        });
    },
    getPartnerList() {
      getPartner()
        .then(res => {
          this.partnerList = res.data;
        })
        .catch(error => {
          console.log(error);
        });
    },
    handleSelect(item) {
      this.form.customerId = item.cardCode;
      this.form.customerName = item.cardName;
      this.form.contacter = item.cntctPrsn;
    },
    handleIconClick() {
      this.dialogPartner = true;
    },
    onSubmit() {
      console.log("submit!");
    },
    handleUpdate(row) {
      // 弹出编辑框
      this.temp = Object.assign({}, row); // copy obj
      this.dialogStatus = "update";
      this.dialogFormVisible = true;
      this.$nextTick(() => {
        this.$refs["dataForm"].clearValidate();
      });
    }
  },
  handleCreate() {
    // 弹出添加框
    this.resetTemp();
    this.dialogStatus = "create";
    this.dialogFormVisible = true;
    this.$nextTick(() => {
      this.$refs["dataForm"].clearValidate();
    });
  }
};
</script>

<style lang="scss" scope>
.form {
  // position: relative;
  // .formDiv{
  //   position: absolute;
  //   right:-200px;
  //   top:0;
  // }
  .lastWord {
    position: sticky;
    top: 0;
    // width: 200px;
  }
}

.my-autocomplete {
  li {
    line-height: normal;
    padding: 7px;

    .name {
      text-overflow: ellipsis;
      overflow: hidden;
    }
    .addr {
      font-size: 12px;
      color: #b4b4b4;
    }

    .highlighted .addr {
      color: #ddd;
    }
  }
}
</style> 
