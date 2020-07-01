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
        <hr>
    <!-- 选择制造商序列号 -->
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="制造商序列号">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="内部序列号">
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="保修结束日期">
          <el-date-picker
            disabled
            size="mini"
            type="date"
            placeholder="选择开始日期"
            v-model="form.startTime"
            style="width: 100%;"
          ></el-date-picker>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="物料编码">
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="16">
        <el-form-item label="物料描述">
          <el-input disabled v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <!-- <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8"> -->
   
      <!-- </el-col>
      <el-col :span="8"> -->
        <el-form-item>
          <el-checkbox-group v-model="form.type">
            <el-checkbox label="售后审核" name="type"></el-checkbox>
            <el-checkbox label="销售审核" name="type"></el-checkbox>
          </el-checkbox-group>
        </el-form-item>
      <!-- </el-col>
    </el-row> -->
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="16">
        <el-form-item label="呼叫主题">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="接单员">
          <el-input disabled v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="呼叫来源">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="呼叫状态">
          <el-input v-model="form.name" disabled></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="创建日期">
          <el-date-picker
            size="mini"
            type="date"
            placeholder="选择开始日期"
            v-model="form.startTime"
            style="width: 100%;"
          ></el-date-picker>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="呼叫类型">
          <el-input v-model="form.name"></el-input>
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
            size="mini"
            type="date"
            placeholder="选择开始日期"
            v-model="form.startTime"
            style="width: 100%;"
          ></el-date-picker>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="问题类型">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="清算日期">
          <el-date-picker
            disabled
            size="mini"
            type="date"
            placeholder="选择日期"
            v-model="form.startTime"
            style="width: 100%;"
          ></el-date-picker>
        </el-form-item>
      </el-col>
      <el-col :span="8">
        <el-form-item label="结束时间">
          <el-date-picker
            disabled
            size="mini"
            type="date"
            placeholder="选择日期"
            v-model="form.startTime"
            style="width: 100%;"
          ></el-date-picker>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="10" type="flex" class="row-bg" justify="space-around">
      <el-col :span="8">
        <el-form-item label="地址标识">
          <el-input v-model="form.name"></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="16">
        <el-input v-model="form.name"></el-input>
      </el-col>
    </el-row>

    <el-form-item label="备注" prop="desc">
      <el-input type="textarea" v-model="form.desc"></el-input>
    </el-form-item>
    <el-dialog
  title="提示"
  :visible.sync="dialogPartner"
  width="90%"
  >
      <formPartner :partnerList='partnerList'></formPartner>

  <span slot="footer" class="dialog-footer">
    <el-button @click="dialogPartner = false">取 消</el-button>
    <el-button type="primary" @click="dialogPartner = false">确 定</el-button>
  </span>
</el-dialog>
  </el-form>
</template>

<script>
import { getPartner } from "@/api/callserve";
import formPartner from "./formPartner"
export default {
  name:'formTable',
   components:{formPartner},
  props: ["modelValue", "refValue", "labelposition", "labelwidth"],
 

  data() {
    return {
      partnerList: [],
      state: "",
      dialogPartner:false,
      form: {
        cardCode: "",
        cardName:'',//客户名称
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
    getPartner()
      .then(res => {
        this.partnerList = res.data;
      })
      .catch(error => {
        console.log(error);
      });
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
        return (
          partnerList.cardCode
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
    handleSelect(item) {
     
      this.form.cardCode = item.cardCode;
      this.form.cardName =item.cardName
      this.form.cntctPrsn =item.cntctPrsn
    },
    handleIconClick() {

this.dialogPartner = true    },
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
