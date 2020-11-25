<template>
  <div class="dashboard-container">
    <!-- <component :is="currentRole"></component> -->
    <div class="img-wrapper">
      <el-image :src="logo" alt="首页图片" fit="fill"></el-image>
    </div>
    
  </div>
</template>

<script>
// import adminDashboard from './admin'
import logo from '@/assets/home_log.png'
export default {
  name: 'dashboard',
  // components: { adminDashboard },
  data() {
    return {
      logo,
      currentRole: 'adminDashboard'
    }
  }
}
</script>
<style lang="scss" scoped>
.dashboard-container {
  position: relative;
  .img-wrapper {
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    width: 500px;
    height: 316px;
    margin: auto;
  }
}
</style>
