<template>
  <el-collapse accordion>
    <el-collapse-item
      :title="`技术员：${item.people}`"
      :key="`key_${index}`"
      v-for="(item, index) in wordList"
      :name="index"
    >
      <el-form label-position="top" label-width="80px" :model="item">

        <el-form-item label="留言">
          <el-input v-model="item.says" disabled></el-input>
        </el-form-item>
        <el-form-item label="回复">
          <el-input v-model="item.word"></el-input>
        </el-form-item>
        <el-button type="primary">确定</el-button>
      </el-form>
    </el-collapse-item>
  </el-collapse>
</template>

<script>
import * as callserve from "@/api/serve/callserve";

export default {
  data() {
    return {
      wordList: [
        {
          people: "张工",
          word: "123",
          img: "",
          says: "654647987651651321654987",
        },
        { people: "刘总", word: "123", img: "" },
        { people: "实习生小王", word: "123", img: "" },
        { people: "老刘", word: "123", img: "" },
        { people: "门卫", word: "123", img: "" },
        { people: "实习生小李", word: "123", img: "" },
      ],
    }
  },
      mounted(){
        console.log(11)
        callserve.GetServiceOrderMessages({serviceOrderId:153}).then(res=>{
          console.log(res)
        })
    },
};
</script>

<style>
</style>