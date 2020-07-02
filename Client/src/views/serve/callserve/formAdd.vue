<template>
  <div>
    <div v-for="(form,index) in formList" :key="`key_${index}`">
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <el-form-item label="制造商序列号">
            <el-autocomplete
              popper-class="my-autocomplete"
              v-model="form.manufSN"
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
            <el-input v-model="form.internalSN" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="保修结束日期">
            <el-date-picker
              disabled
              size="mini"
              type="date"
              placeholder="选择开始日期"
              v-model="form.dlvryDate"
              style="width: 100%;"
            ></el-date-picker>
          </el-form-item>
        </el-col>
      </el-row>
      <el-row type="flex" class="row-bg" justify="space-around">
        <el-col :span="8">
          <el-form-item label="物料编码">
            <el-input v-model="form.itemCode" disabled></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="16">
          <el-form-item label="物料描述">
            <el-input disabled v-model="form.itemName"></el-input>
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
    <el-dialog destroy-on-close title="选择制造商序列号" width="90%" :visible.sync="dialogfSN">
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
import fromfSN from './fromfSN'
export default {
  components:{fromfSN},
  props: ["SerialNumberList"],
  data() {
    return {
         filterSerialNumberList: [],
         formListStart:[], //未加工的表格数据
      dialogfSN:false,
      inputSearch: '',
      formList: [ {
        manufSN: "",
        internalSN: "", //内部序列号
        itemCode: "", //物料编码
        itemName: "",  //物料描述
        dlvryDate: "",
        delivery: false,
        type: [],
        resource: "",
        name: ""
      }],
      ifFormPush:false, //表单是否被动态添加过
      form: {
        manufSN: "",
        custmrName: "", //客户名称
        region: "",
        date1: "",
        date2: "",
        delivery: false,
        type: [],
        resource: "",
        name: ""
      }
    };
  },
  created() {
     this.filterSerialNumberList=this.SerialNumberList
    // getPartner()
    //   .then(res => {
    //     // this.partnerList = res.data;
    //   })
    //   .catch(error => {
    //     console.log(error);
    //   });
  },
  computed:{
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
    changeForm(res){
      this.formListStart =res
    },
    pushForm(){
      this.dialogfSN = false
      console.log(this.formListStart)
      if(!this.ifFormPush){
         this.formList[0].manufSN=this.formListStart[0].manufSN
           this.formList[0].internalSN=this.formListStart[0].internalSN
           this.formList[0].itemCode=this.formListStart[0].itemCode
           this.formList[0].itemName=this.formListStart[0].itemName
             this.formList[0].dlvryDate=this.formListStart[0].dlvryDate
           
        for(let i=1;i<this.formListStart.length;i++){
         
           this.formList.push({
               manufSN:this.formListStart[i].manufSN,
        internalSN:this.formListStart[i].internalSN,
        itemCode: this.formListStart[i].itemCode, 
        itemName: this.formListStart[i].itemName,  
        dlvryDate:this.formListStart[i].dlvryDate
           })
        }
        this.ifFormPush =true
      }else{
        // this.formList=[]
         this.ifFormPush =true
        //  console.log()
  for(let i=this.formList.length-1;i<this.formListStart.length;i++){
           this.formList.push({
               manufSN:this.formListStart[i].manufSN,
        internalSN:this.formListStart[i].internalSN,
        itemCode: this.formListStart[i].itemCode, 
        itemName: this.formListStart[i].itemName,  
        dlvryDate:this.formListStart[i].dlvryDate

           })
 
      
        }
      }
    },
    handleIconClick() {
      this.dialogfSN= true
    },
    searchList(res){
      if (!res) {
        this.filterSerialNumberList=this.SerialNumberList
      } else{
        let list=this.SerialNumberList.filter(item=>
       {return item.manufSN.indexOf(res) >0 })
              this.filterSerialNumberList=list
      }
    },
    querySearch(queryString, cb) {
    
      var filterSerialNumberList =  this.SerialNumberList
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
    searchSelect(res){
      let newList =
       this.filterSerialNumberList.filter(item=>
        item.manufSN===res.manufSN
      )
     this.inputSearch=res.manufSN
    this.filterSerialNumberList=newList
    }
 
  }
};
</script>

<style>
</style>