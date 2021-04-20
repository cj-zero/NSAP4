const path = require('path')
const CompressionPlugin = require('compression-webpack-plugin')
// const HardSourceWebpackPlugin = require('hard-source-webpack-plugin');
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
    devtool: process.env.NODE_ENV === 'development' ? 'eval-cheap-source-map' : 'hidden-source-map'
  },
  // svg配置
  chainWebpack(config) {
    if (process.env.NODE_ENV === 'production') {
      config.plugin('CompressionPlugin')
        .use(CompressionPlugin, [])
        .end()
    }
    // if (process.env.NODE_ENV === 'development') {
    //   config.plugin('HardSourceWebpackPlugin')
    //     .use(HardSourceWebpackPlugin)
    //     .end()
    // }
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