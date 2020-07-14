import Vue from 'vue'
import Vuex from 'vuex'
import app from './modules/app'
import serverConf from './modules/serverConf'
import user from './modules/user'
import tagsView from './modules/tagsView'
import permission from './modules/permission'
import dataPrivilegerules from './modules/dataPrivilegerules'
import storage from './modules/storage'
import getters from './getters'
import { vuexOidcCreateStoreModule } from 'vuex-oidc'

Vue.use(Vuex)
// console.log(JSON.parse(process.env.VUE_APP_OIDC), process)
const store = new Vuex.Store({
  modules: {
    app,
    user,
    serverConf,
    permission,
    dataPrivilegerules,
    storage,
    tagsView,
    oidcStore: vuexOidcCreateStoreModule(
      {
        authority: process.env.VUE_APP_OIDC_AUTHORITY,
        clientId: process.env.VUE_APP_OIDC_CLIENTID,
        redirectUri: process.env.VUE_APP_OIDC_REDIRECTURI,
        postLogoutRedirectUri:process.env.VUE_APP_OIDC_POSTLOGOUTREDIRECTURI,
        responseType: process.env.VUE_APP_OIDC_RESPONSETYPE,
        scope: process.env.VUE_APP_OIDC_SCOPE,
        automaticSilentRenew: process.env.VUE_APP_OIDC_AUTOMATICSILENTRENEW ,
        silentRedirectUri: process.env.VUE_APP_OIDC_SILENTREDIRECTURI
      },
      // Optional OIDC store settings
      {
        namespaced: false,
        dispatchEventsOnWindow: true
      },
      // Optional OIDC event listeners
      {
        userLoaded: (user) => console.log('OIDC user is loaded:', user),
        userUnloaded: () => console.log('OIDC user is unloaded'),
        accessTokenExpiring: () => console.log('Access token will expire'),
        accessTokenExpired: () => console.log('Access token did expire'),
        silentRenewError: () => console.log('OIDC user is unloaded'),
        userSignedOut: () => console.log('OIDC user is signed out')
      }
    )
  },
  getters
})

export default store
