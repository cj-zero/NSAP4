<template>
  <div class="search-wrapper filter-container">
    <template v-for="(config, index) in newConfig">
      <component
        v-if="config.isShow"
        class="filter-item"
        v-model="listQuery[config.prop]" 
        v-bind="config.attrs" 
        :is="config.componentName" 
        v-on="config.on" 
        :key="index"
        v-infotooltip:200.top
      ></component>
    </template>
  </div>
</template>

<script>
import { getComponentName, mergeComponentAttrs, mergeConfig } from '../global/mergeConfig'
import SearchInput from './SearchInput'
import MySelect from '../Select'
import SearchButton from './SearchButton'
import SearchUpload from './SearchUpload'
export default {
  name: 'my-search',
  components: {
    SearchInput,
    MySelect,
    SearchButton,
    SearchUpload
  },
  props: {
     listQuery: {
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
      }
  },
  data () {
    return {
      newConfig: []
    }
  },
  watch: {
    config: {
      immediate: true,
      handler (val) {
        this.newConfig = this._normalizeConfig(val)
      }
    }
  },
  methods: {
    _normalizeConfig (configList) {
      // { width: 120, prop: xxx, component: { tag: xxx, attrs: {}, itemAttrs: {} }}
      return configList.map(config => {
        const newConfig = mergeConfig(config)
        const component = config.component
        newConfig.componentName = getComponentName(component, true)
        newConfig.attrs = mergeComponentAttrs(component, 'attrs')
        newConfig.on = mergeComponentAttrs(component, 'on')
        newConfig.isShow = newConfig.isShow !== false
        return newConfig
      })
    }
  },
  created () {

  },
  mounted () {

  },
}
</script>
<style lang='scss' scoped>
.search-wrapper {
  display: inline;
}
</style>