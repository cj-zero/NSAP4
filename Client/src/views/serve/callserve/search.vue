<template>
  <el-form ref="form" :model="form" label-width="70px" size="mini">
    <div style="padding:10px 0;"></div>
    <el-row :gutter="3">
      <el-col :span="2" style="min-width: 160px;">
        <el-form-item label="服务ID">
          <el-input   v-model="form.QryServiceOrderId" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <!-- <el-col :span="2">
        <el-form-item label="工单ID">
          <el-input  v-model="form.QryServiceWorkOrderId" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col> -->
      <el-col :span="3">
        <el-form-item label="呼叫状态">
          <el-select  clearable v-model="form.QryState" placeholder="请选择呼叫状态">
            <el-option
              v-for="(item,index) in callStatus"
              :key="index"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="客户">
          <el-input  v-model="form.QryCustomer" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="序列号">
          <el-input  v-model="form.QryManufSN" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="3">
        <el-form-item label="接单员">
          <el-input  v-model="form.QryRecepUser" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
    <!-- </el-row> -->
    <!-- <el-row :gutter="10"> -->
      <el-col :span="3">
        <el-form-item label="技术员">
          <el-input  v-model="form.QryTechName" @keyup.enter.native='onSubmit'></el-input>
        </el-form-item>
      </el-col>
      <el-col :span="2">
        <el-form-item label-width="0px" style="margin-left: 10px;">
            <el-button type="primary" @click="onSubmit" size="mini" icon="el-icon-search">查询</el-button>
        </el-form-item>
      </el-col>
      <el-col :span="2">
        <el-form-item label-width="0px" style="margin-left: 20px;">
            <el-button type="info" @click="toggleMoreSearch" size="mini" icon="el-icon-search">高级查询</el-button>
        </el-form-item>
      </el-col>
    </el-row>
    <transition
      name="expand"
      @before-enter="beforeEnter"
      @enter="enter"
      @after-enter="afterEnter"
      @enter-cancelled="enterCancelled"
      @before-leave="beforeLeave"
      @leave="leave"
      @after-leave="afterLeave"
      :css="false">
      <el-row v-show="isVisible">
        <el-col :span="4" style="margin-left: 10px;">
          <el-form-item label="问题类型">
            <el-cascader
              :props="{ value:'id',label:'name',children:'childTypes',expandTrigger: 'hover', emitPath: false }"  
              clearable
              v-model="form.QryProblemType"
              :options="options"
              @change="handleChange"></el-cascader>
            <!-- <el-input  v-model="form.QryProblemType" @keyup.enter.native='onSubmit'></el-input> -->
          </el-form-item>
        </el-col>
        <el-col :span="4" style="margin-left: 10px;">
          <el-form-item label="联系电话">
            <el-input  v-model="form.QryTechName" @keyup.enter.native='onSubmit'></el-input>
            <!-- <el-input  v-model="form.QryProblemType" @keyup.enter.native='onSubmit'></el-input> -->
          </el-form-item>
        </el-col>
        <el-col :span="8" style="margin-left: 10px">
          <el-row :gutter="3">
            <el-form-item label="创建日期">
              <el-col :span="11">
                <el-date-picker
                  type="date"
                  placeholder="选择开始日期"
                  v-model="form.startTime"
                ></el-date-picker>
              </el-col>
              <el-col class="line" :span="1">至</el-col>
              <el-col :span="11">
                <el-date-picker
                  type="date"
                  placeholder="选择结束时间"
                  v-model="form.endTime"
                ></el-date-picker>
              </el-col>
            </el-form-item>
          </el-row>
        </el-col>
      </el-row>
    </transition>
  </el-form>
</template>

<script>
export default {
  data() {
    return {
      isVisible: false,
      form: {
        QryServiceOrderId: "", // 查询服务ID查询条件
        QryState: "", // 呼叫状态查询条件
        QryCustomer: "", // 客户查询条件
        QryManufSN: "", //  制造商序列号查询条件
        QryCreateTimeFrom: "", // 创建日期从查询条件
        QryCreateTimeTo: "", //  创建日期至查询条件
        QryRecepUser: "", //  接单员
        QryTechName: "", // - 工单技术员
        QryProblemType: "", //QryProblemType - 问题类型
        QryMaterialTypes: "", //物料类别（多选)
      },
      callStatus: [
        { value: '', label: '全部' },
        { value: 1, label: "待处理" },
        { value: 2, label: "已排配" },
        { value: 3, label: "已预约" },
        { value: 4, label: "已外出" },
        { value: 5, label: "已挂起" },
        { value: 6, label: "已接收" },
        { value: 7, label: "已解决" },
        { value: 8, label: "已回访" }
      ]
    };
  },
  props: {
    options: {
      type: Array,
      default () {
        return []
      }
    }
  },
  watch: {
    form: {
      deep: true,
      handler(val) {
        this.$emit("change-Search", val);
      }
    }
  },
  methods: {
    onSubmit() {
      this.$emit("change-Search", 1);
    },
    toggleMoreSearch () {
      console.log(this.isVisible, 'isVisible')
      this.isVisible = !this.isVisible
    },
    beforeEnter(el){
        el.className= el.className+' my-transition';
        el.style.height = '0';
    },
    enter(el, done){
        el.style.height = el.scrollHeight + 'px';
        done()
    },
    afterEnter(el){
        el.className= el.className.replace("my-transition",'');
        el.style.height = '';
    },
    enterCancelled(){
      //
    },
    beforeLeave(el){
        el.style.height = el.scrollHeight + 'px';
    },
    leave(el, done){
        el.className= el.className+' my-transition';
        el.style.height = 0;
        done()
    },
    afterLeave(el){
        el.className=el.className.replace("my-transition",'');
        el.style.height = '';
    },
    handleChange (val) {
      console.log(val, 'change', this.form.QryProblemType)
    }
  }
};
</script>

<style lang="scss" scoped>
.date-wrapper {
  // ::v-deep .el-input__inner {
  //   padding-left: 20px;
  //   padding-right: 10px;
  // }
  // ::v-deep .el-icon-date, el-input__icon {
  //   margin-left: 6px;
  // }
}
.my-transition{transition: .3s height ease-in-out/* , 10.5s padding-top ease-in-out, 10.5s padding-bottom ease-in-out */}