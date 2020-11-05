const path = require('path')
function resolve (dir) {
    return path.join(__dirname, '/', dir)
}

module.exports = {
  lintOnSave: process.env.NODE_ENV !== 'production',
  devServer: {
    port: 1803,     // 端口
    overlay: {
      warnings: true,
      errors: false
    }
  },
  configureWebpack: {
    externals: {
      "BMap": "BMap",
      "echarts": "echarts"
    },
    module: {
      unknownContextCritical : false,
      //解决the request of a dependency is an expression
      exprContextCritical: false,
    },
    // devtool: 'eval-cheap-source-map',
    devtool: process.NODE_ENV === 'development' ? 'source-map' : 'eval-cheap-source-map'
  },
  // svg配置
  chainWebpack(config) {
    config.module
      .rule('svg')
      .exclude.add(resolve('src/icons'))
      .end()
    config.module
      .rule('icons')
      .test(/\.svg$/)
      .include.add(resolve('src/icons'))
      .end()
      .use('svg-sprite-loader')
      .loader('svg-sprite-loader')
      .options({
        symbolId: 'icon-[name]'
      })
      .end()
  }
}