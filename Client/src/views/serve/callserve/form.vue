<template>
  <el-form :model="form" :ref="refValue" :label-width="labelwidth">
    <div style="padding:10px 0;"></div>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户代码">
          <el-autocomplete
            popper-class="my-autocomplete"
            v-model="form.cardCode"
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
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="客户名称">
          <el-input v-model="form.cardName" disabled></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="联系人">
          <el-select v-model="form.cntctPrsn" placeholder="请选择">
            <el-option label="服务一" value="shanghai"></el-option>
            <el-option label="服务二" value="beijing"></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="最近联系人">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="终端客户">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="最新电话号码">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-form-item label="服务类型" prop="resource">
      <el-radio-group v-model="form.name">
        <el-radio label="免费"></el-radio>
        <el-radio label="收费"></el-radio>
      </el-radio-group>
    </el-form-item>
    <hr />
    <!-- 选择制造商序列号 -->
    <formAdd :SerialNumberList="SerialNumberList"></formAdd>
    <el-dialog title="选择业务伙伴" width="90%" :visible.sync="dialogPartner">
      <formPartner :partnerList="partnerList" ></formPartner>

      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogPartner = false">取 消</el-button>
        <el-button type="primary" @click="dialogPartner = false">确 定</el-button>
      </span>
    </el-dialog>
  </el-form>
</template>

<script>
import { getPartner ,getSerialNumber } from "@/api/callserve";
import formPartner from "./formPartner";
import formAdd from "./formAdd";
export default {
  name: "formTable",
  components: { formPartner, formAdd },
  props: ["modelValue", "refValue", "labelposition", "labelwidth"],

  data() {
    return {
      partnerList: [],
      SerialNumberList: [], //序列号列表
      state: "",
      dialogPartner: false,
      form: {
        cardCode: "",
        cardName: "", //客户名称
        region: "",
        date1: "",
        date2: "",
        delivery: false,
        type: [],
        resource: "",
        desc: ""
      }
    };
  },

  mounted() {
    this.getPartnerList();
    this.getSerialNumberList();
  },
  methods: {
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
        console.log(partnerList)
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
      this.form.cardCode = item.cardCode;
      this.form.cardName = item.cardName;
      this.form.cntctPrsn = item.cntctPrsn;
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
