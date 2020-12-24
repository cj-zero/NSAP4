
import Vue from 'vue'
import infoTooltip from './src/main.vue'

const InfoTooltip = Vue.extend(infoTooltip)

let instance = null;

export default function tip () {
  if (instance) {
    return instance
  }
  instance = new InfoTooltip({
      el: document.createElement('div')
  }); 
  return instance 
}
