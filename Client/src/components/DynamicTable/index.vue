<template>
<div style="display:flex;">
  <el-transfer
        v-model="checkboxVal"
        class="transferD"
        width="89%"
        :props="{key: 'name',label: 'label'}"
        :titles="['隐藏的表格数据', '显示的表格数据']"
        @change="handleChange"
        :data="noData"
    ></el-transfer>
</div>
  
</template>
<script>
export default {
    data() {
        return {
            noData:this.formThead,
            checkboxVal: this.defaultForm
            // 注意:key 的字符类型要一致!!!
        }
    },
    props:[
        'formThead','defaultForm',''
    ],
    mounted(){
        //  console.log(this.formThead,this.defaultForm)
        // this.noData=this.formTheadOptions
        // this.checkboxVal=this.defaultFormThead
    },
    methods:{
        handleChange(value, direction, movedKeys) {
              console.log(value, direction, movedKeys);
        
             if(direction === "right") {
                 this.$emit('update:defaultForm',value)
             }
             if(direction === "left") {
                this.$emit('update:defaultForm',value)
             }
                
        }
    }
}
</script>
<style  scoped>
    .transferD{
        align-items:center;
        justify-self: center;
        margin:auto;
    }
</style>