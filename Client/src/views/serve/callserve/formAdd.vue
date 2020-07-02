<template>
  <div>
    <div v-for="(form,index) in formList" :key="`key_${index}`">
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <el-form-item label="制造商序列号">
            <el-autocomplete
              popper-class="my-autocomplete"
              v-model="form.name"
              :fetch-suggestions="querySearch"
              placeholder="请输入内容"
              @select="handleSelect"
            >
              <i class="el-icon-search el-input__icon" slot="suffix" @click="handleIconClick"></i>
              <template slot-scope="{ form }">
                <div class="name">{{ form.cardCode }}</div>
                <span class="addr">{{ form.cardName }}</span>
              </template>
            </el-autocomplete>
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
      <el-col :span="8">-->

      <!-- </el-col>
      <el-col :span="8">-->
      <el-form-item>
        <el-checkbox-group v-model="form.name">
          <el-checkbox label="售后审核" name="type"></el-checkbox>
          <el-checkbox label="销售审核" name="type"></el-checkbox>
        </el-checkbox-group>
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
    </div>
  </div>
</template>

<script>
import { getPartner } from "@/api/callserve";
export default {
    // components:{formAdd},
    data(){
        return{
            partnerList: [],
            formList:[
        {name:'1'},{name:'2'}
            ],
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
        }

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
           handleIconClick() {
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
        return (
          partnerList.cardCode
            .toLowerCase()
            .indexOf(queryString.toLowerCase()) === 0
        );
      };
    },
        handleSelect(item) {
     console.log(item)
      
    },
    }
}
</script>

<style>
</style>