<template>
  <div id="app">
    <keep-alive>
      <router-view></router-view>
    </keep-alive>
  </div>
</template>

<script>
import { mapGetters } from 'vuex'
export default {
  name: 'App',
  computed: {
    ...mapGetters([
      'oidcIsAuthenticated'
    ])
  },
  methods: {
    userLoaded: function(e) {
      console.log('I am listening to the user loaded event in vuex-oidc', e.detail)
    }
  },
  mounted() {
    window.addEventListener('vuexoidc:userLoaded', this.userLoaded)
  },
  destroyed() {
    window.removeEventListener('vuexoidc:userLoaded', this.userLoaded)
  }
}
</script>
