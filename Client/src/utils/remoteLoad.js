export function remoteLoad (url, hasCallback) {
    return createScript(url)
    /**
     * 创建script
     * @param url
     * @returns {Promise}
     */
    function createScript (url) {
      let scriptElement = document.createElement('script')
      document.head.appendChild(scriptElement)
      let promise = new Promise((resolve, reject) => {
        scriptElement.addEventListener('load', e => {
          removeScript(scriptElement)
          if (!hasCallback) {
            resolve(e)
          }
        }, false)
  
        scriptElement.addEventListener('error', e => {
          removeScript(scriptElement)
          reject(e)
        }, false)
  
        if (hasCallback) {
          window.____callback____ = function () {
            resolve()
            window.____callback____ = null
          }
        }
      })
  
      if (hasCallback) {
        url += '&callback=____callback____'
      }
  
      scriptElement.src = url
  
      return promise
    }
  }

  /**
 * 动态加载百度地图api函数
 * @param {String} ak  百度地图AK，必传
 */
export function loadBMap(ak, type) {
  return new Promise(function(resolve, reject) {
    if (typeof window.BMap !== 'undefined') {
      resolve(window.BMap)
      return true
    }
    window.onBMapCallback = function() {
      removeScript(script)
      resolve(window.BMap)
    }
    let script = document.createElement('script')
    script.type = 'text/javascript'
    script.src = type === 'GL' 
      ? 'https:api.map.baidu.com/api?type=webgl&v=1.0&ak=' + ak + '&callback=onBMapCallback'
      : 'https://api.map.baidu.com/api?v=3.0&ak=' + ak + '&callback=onBMapCallback'
      
      
    script.onerror = reject
    document.head.appendChild(script)
  })
}

/**
 * 移除script标签
 * @param scriptElement script dom
 */
function removeScript (scriptElement) {
  document.head.removeChild(scriptElement)
}