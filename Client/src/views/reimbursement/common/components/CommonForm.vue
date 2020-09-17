<template>
  <el-form
    :model="formData"
    :rules="rules"
    ref="form"
    class="my-form-wrapper"
    :disabled="disabled"
    :label-width="labelWidth"
    size="mini"
    :label-position="labelposition"
  >
    <!-- 普通控件 -->
    <el-row 
      type="flex" 
      v-for="(config, index) in normalConfig"
      :key="index">
      <el-col 
        :span="item.col"
        v-for="item in config"
        :key="item.prop"
      >
        <el-form-item :label="item.label">
          <template v-if="!item.type">
            <el-input 
              v-model="formData[item.prop]" 
              :style="{ width: item.width + 'px' }"
              :maxlength="item.maxlength || 0"
              :disabled="item.disabled">
              <i :class="item.icon" v-if="item.icon"></i>
            </el-input>
          </template>
          <template v-else-if="item.type === 'select'">
            <el-select 
              clearable
              :style="{ width: item.width + 'px' }"
              v-model="formData[item.prop]" 
              :placeholder="item.placeholder"
              :disabled="item.disabled">
              <el-option
                v-for="(item,index) in item.options"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </template>
          <template v-else-if="item.type === 'date'">
            <el-date-picker
              :style="{ width: item.width + 'px' }"
              :value-format="item.valueFormat || 'yyyy-MM-dd'"
              :type="item.dateType || 'date'"
              :placeholder="item.placeholder"
              v-model="formData[item.prop]"
            ></el-date-picker>
          </template>
          <template v-else-if="item.type === 'inline-slot'">
            <slot :name="'form' + item.id" :data="formData"></slot>
          </template>
        </el-form-item>
      </el-col>
    </el-row>
    <!-- 插槽 -->
    <el-row v-for="item in slotConfig" :key="item.id">
      <template v-if="item.showLabel">
        <el-form-item :label="item.label">
          <slot :name="item.id" :data="formData"></slot>
        </el-form-item>
      </template>
      <template v-else>
        <slot :name="item.id" :data="formData"></slot>
      </template>
    </el-row>
  </el-form>
</template>

<script>
import { deepClone } from '@/utils'
export default {
  props: {
    data: {
      type: Object,
      default () {
        return {}
      }
    },
    config: {
      type: Array,
      default () {
        return []
      }
    },
    columns: { // 一行多少列
      type: Number,
      default: 1
    },
    labelposition: {
      type: String,
      default: 'left'
    },
    labelWidth: {
      type: [String, Number],
      default: '80px'
    },
    disabled: {
      type: Boolean,
      default: false
    },
    rules: {
      type: Object,
      default () {
        return {}
      }
    }
  },
  data () {
    return {
      formData: {}
    }
  },
  watch: {
    data: {
      immediate: true,
      handler (val) {
        this.formData = deepClone(val)
      }
    }
  },
  computed: {
    normalConfig () {
      let noneSlotConfig = this.config.filter(item => item.type !== 'slot')
      let result = [], j = 0
      for (let i = 0; i < noneSlotConfig.length; i++) {
        
        if (!result[j]) {
          result[j] = []
        }
        result[j].push(noneSlotConfig[i])
        if (noneSlotConfig[i].isEnd) {
          j++
        }
      }
      return result
    },
    slotConfig () {
      return this.config.filter(item => item.type === 'slot')
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
.my-form-wrapper {
  ::v-deep .el-form-item--mini {
    margin-bottom: 7px;
  }
}
</style>