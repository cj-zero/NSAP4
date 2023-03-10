<template>
  <div class="tags-view-container">
    <scroll-pane class="tags-view-wrapper" ref="scrollPane">
      <router-link
        ref="tag"
        width="110px"
        class="tags-view-item"
        :class="isActive(tag)?'active':''"
        v-for="(tag, index) in Array.from(visitedViews)"
        :to="tag"
        :key="`${index}_${tag.path}`"
        @contextmenu.prevent.native="openMenu({ tag, index }, $event)"
      >
        {{tag.title}}
        <span class="el-icon-close" @click.prevent.stop="closeSelectedTag(tag)"></span>
      </router-link>
    </scroll-pane>
    <ul class="contextmenu" v-show="visible" :style="{left:left+'px',top:top+'px'}">
      <li @click="closeSelectedTag(selectedTag)">关闭本页面</li>
      <li @click="closeOthersTags">关闭其他</li>
      <li @click="closeAllTags">全部关闭</li>
      <li @click="refreshSelectedTag(selectedTag)">刷新页面</li>
      <li @click="addViewTags_copy(selectedTag)">复制页面</li>
    </ul>
  </div>
</template>

<script>
import ScrollPane from "@/components/ScrollPane";
import { mapGetters } from 'vuex'
import { setSessionStorage } from '@/utils/storage'
const hasPageRep = /page=.*/
export default {
  components: { ScrollPane },
  data() {
    return {
      visible: false,
      top: 0,
      left: 0,
      page: "", //记录上一次时间戳
      selectedTag: {}
    };
  },
  computed: {
    ...mapGetters(['iframeViews', 'keepAliveData']),
    visitedViews() {
      // console.log(this.$store.state.tagsView.visitedViews);
      //页面加载mounted前执行
      return this.$store.state.tagsView.visitedViews;
    }
  },
  watch: {
    $route(to) {
      if (!hasPageRep.test(to.fullPath)) { // 点击侧边栏是不带query参数的，如果这个时候已经有对应模块的标签，则自行跳转到模块对应的第一个标签页
        let index = this.visitedViews.findIndex(item => item.path === to.path)
        if (index !== -1) {
          const targetRoute = this.visitedViews[index]
          const query = targetRoute.query ? targetRoute.query : {}
          this.$router.push({
            path: targetRoute.fullPath,
            query
          })
        } else {
          this.addViewTags();
          this.moveToCurrentTag();
        }
      } else {
        this.addViewTags();
        this.moveToCurrentTag();
      }
    },
    visible(value) {
      if (value) {
        document.body.addEventListener("click", this.closeMenu);
      } else {
        document.body.removeEventListener("click", this.closeMenu);
      }
    }
  },
  mounted() {
    this.addViewTags();
    //页面挂载执行
    window.addEventListener("beforeunload",() => {
      if (hasPageRep.test(this.$route.fullPath)) {
        console.log('beforeunlaod')
        setSessionStorage('isUnloadRefresh', true)
      }
    })
  },
  methods: {
    generateRoute(isCopy = false) {
      // console.log(this.$route)
      let times = new Date().getTime();
      if (this.$route.name) {
        // return this.$route;
        let newRoute = {
          fullPath: `${this.$route.fullPath}`,
          hash: this.$route.hash,
          matched: this.$route.matched,
          meta: this.$route.meta,
          name: this.$route.name,
          params: this.$route.params,
          path: this.$route.path,
          query: this.$route.query
          // query: {
          //   page: times
          // }
        };
        if (isCopy) {
          newRoute.query = { ...(this.$route.query || {}), page: times }
          let queryStr = ''
          for (let key in newRoute.query) {
            queryStr += `${key}=${newRoute.query[key]}`
          }
          newRoute.fullPath = newRoute.path + `?${queryStr}`
        }
        return newRoute;
      }
      return false;
    },
    async refreshSelectedTag(view) {
      const route = await this.generateRoute(true);
      // 判断是不是通过侧边栏生成的原始页面
      console.log(route, 'route refresh')
      await this.$store.dispatch("refreshVisitedViews", { originRoute: view, newRoute: route, index: this.currentIndex })
      await this.$router.push({
        path: route.fullPath,
        query: route.query
      })
    },
    isActive(route) {
      return route.fullPath === this.$route.fullPath;     
    },
    addViewTags() {
      //获取store储存的tag，
      let route = this.generateRoute();
      if (!route) {
        return false;
      }
      
      this.$store.dispatch("addVisitedViews", route);
    },
    async addViewTags_copy(page) {
      const route = await this.generateRoute(true);
      await this.$store.dispatch("copyVisitedViews", { view: route, index: this.currentIndex });
      await this.$router.push({
        path: page.fullPath,
        query: route.query
      });
      if (!route) {
        return false;
      }
    },
    moveToCurrentTag() {
      const tags = this.$refs.tag;
      this.$nextTick(() => {
        for (const tag of tags) {
          if (tag.to.fullPath === this.$route.fullPath) {
            this.$refs.scrollPane.moveToTarget(tag.$el);
            break;
          }
        }
      });
    },
    closeSelectedTag(view) {
      this.$store.dispatch("delVisitedViews", view).then(views => {
        console.log(views, 'views')
        if (this.isActive(view)) {
          const latestView = views.slice(-1)[0];
          if (latestView) {
            this.$router.push(latestView);
          } else {
            this.$router.push("/");
          }
        }
      });
    },
    closeOthersTags() {
      this.$router.push(this.selectedTag);
      this.$store.dispatch("delOthersViews", this.selectedTag).then(() => {
        this.moveToCurrentTag();
      });
    },
    closeAllTags() {
      this.$store.dispatch("delAllViews");
      this.$router.push("/");
    },
    openMenu({ tag, index }, e) {
      this.visible = true;
      this.selectedTag = tag;
      this.currentIndex = index
      const offsetLeft = this.$el.getBoundingClientRect().left; // container margin left
      this.left = e.clientX - offsetLeft - 35; // 15: margin right
      this.top = e.clientY - 15;
    },
    closeMenu() {
      this.visible = false;
    }
  },
  created () {
    this.currentIndex = -1 // 记录当前标签页的索引值
  }
};
</script>

<style rel="stylesheet/scss" lang="scss" scoped>
.tags-view-container {
  margin-top: 45px;
  .tags-view-wrapper {
    background: #fff;
    height: 34px;
    border-bottom: 1px solid #d8dce5;
    box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.12), 0 0 3px 0 rgba(0, 0, 0, 0.04);
    .tags-view-item {
      display: inline-block;
      position: relative;
      height: 26px;
      line-height: 26px;
      border: 1px solid #d8dce5;
      color: #495060;
      background: #fff;
      padding: 0 8px;
      font-size: 12px;
      margin-left: 5px;
      margin-top: 4px;
      &:first-of-type {
        margin-left: 15px;
      }
      &.active {
        background-color: #f8f8f8;
        color: #333;
        border-color: #efefef;
        background-color: #efefef;
        border-color: #dcdfe6;
        &::before {
          content: "";
          background: #606266;
          display: inline-block;
          width: 8px;
          height: 8px;
          border-radius: 50%;
          position: relative;
          margin-right: 2px;
        }
      }
    }
  }
  .contextmenu {
    margin: 0;
    background: #fff;
    z-index: 10000;
    position: absolute;
    list-style-type: none;
    padding: 5px 0;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 400;
    color: #333;
    box-shadow: 2px 2px 3px 0 rgba(0, 0, 0, 0.3);
    li {
      margin: 0;
      padding: 7px 16px;
      cursor: pointer;
      &:hover {
        background: #eee;
      }
    }
  }
}
</style>

<style rel="stylesheet/scss" lang="scss">
//reset element css of el-icon-close
.tags-view-wrapper {
  .tags-view-item {
    .el-icon-close {
      width: 16px;
      height: 16px;
      vertical-align: 2px;
      border-radius: 50%;
      text-align: center;
      transition: all 0.3s cubic-bezier(0.645, 0.045, 0.355, 1);
      transform-origin: 100% 50%;
      &:before {
        transform: scale(0.6);
        display: inline-block;
        vertical-align: -3px;
      }
      &:hover {
        background-color: #b4bccc;
        color: #fff;
      }
    }
  }
}
</style>
