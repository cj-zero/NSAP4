<template>
  <section class="app-main" ref="appMain">
    <transition name="fade-transform" mode="out-in">
      <keep-alive :include="keepAliveDatas" v-if="keepAliveDatas.length > 0">
        <router-view :key="key"></router-view>
      </keep-alive>
      <!-- <keep-alive> -->
        <router-view v-else></router-view>
      <!-- </keep-alive> -->
    </transition>
  </section>
</template>

<script>
import {mapGetters} from 'vuex'
export default {
  name: 'AppMain',
  computed: {
    ...mapGetters({
      keepAliveData: 'keepAliveData'
    }),
    keepAliveDatas() {
      return this.keepAliveData || []
    },
    cachedViews() {
      return this.$store.state.tagsView.cachedViews
    },
    key() {
      return this.$route.fullPath
    }
  },
  watch: {
    $route() {
      this.$refs.appMain.scrollTop = 0
    },
    keepAliveDatas: {
      immediate: true,
      handler (val) {
        console.log(val, 'keepAliveDatas')
      }
    }
  }
}
</script>

<style scoped lang="scss">
.app-main {
    width: 100%;
    height: calc(100% - 35px);
    position: relative;
    overflow: auto;
		background-color: #efefef;
    box-sizing: border-box;
    ::v-deep > div {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
}
</style>
