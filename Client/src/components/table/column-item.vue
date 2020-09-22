<template>
  <div>
    <el-table-column
      :width="columns.width"
      :label="columns.label"
      :align="columns.align || 'left'"
      :sortable="columns.isSort || false"
    >
      <div v-if="columns.children && columns.children.length">
        <column-item 
          v-for="item in columns.children"
          :columns="item"
          :key="item"
        ></column-item>
      </div>
      <div v-else>
        <template slot-scope="scope" >
          {{ scope.row[item.prop]}} {{ item.prop }}
          <div class="link-container" v-if="item.type === 'link'">
            <img :src="rightImg" @click="item.handleJump(scope.row)" class="pointer">
            <span>{{ scope.row[item.prop] }}</span>444
          </div>
          <template v-else-if="item.type === 'operation'">
            <el-button 
              v-for="btnItem in item.actions"
              :key="btnItem.btnText"
              @click="btnItem.btnClick(scope.row)" 
              type="text" 
              :icon="item.icon || ''"
              :size="item.size || 'mini'"
            >{{ btnItem.btnText }}</el-button>333
          </template>
          <template v-else-if="item.type === 'input'">
            <el-input v-model="scope.row[item.prop]"></el-input>222
          </template>
          <template v-else>
            {{ scope.row[item.prop] }} 111
          </template>
        </template>
      </div>
    </el-table-column>
  </div>
  
</template>

<script>

export default {
  name: 'column-item',
  props: {
    data: {
      type: Array,
      default () {
        return []
      }
    },
    columns: {
      type: Array,
      default () {
        return []
      }
    },
  },
  watch: {
    columns: {
      immediate: true,
      handler (val) {
        console.log('columns inner', val)
      }
    }
  },
  data () {
    return {

    }
  },
  methods: {

  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
</style>