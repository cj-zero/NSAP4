const path = require('path')
const SpeedMeasurePlugin = require('speed-measure-webpack-plugin')
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const merge = require("webpack-merge")
const smp = new SpeedMeasurePlugin({
  outputFormat:"human",
 })

function resolve (dir) {
    return path.join(__dirname, '/', dir)
}

const commonConfig = {
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
    }
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
const developConfig = {
  configureWebpack: smp.wrap({
    externals: {
      "BMap": "BMap"
    },
    module: {
      unknownContextCritical : false,
      //解决the request of a dependency is an expression
      exprContextCritical: false,
    },
    plugins: [
      new BundleAnalyzerPlugin()
    ]
  })
}
module.exports = process.env.NODE_ENV === 'development'
  ? Object.assign(commonConfig, developConfig)
  : commonConfig