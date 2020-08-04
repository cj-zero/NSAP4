<template>
  <div class="dashboard-editor-container">
    <panel-group></panel-group>
  </div>
</template>


<script>
  import PanelGroup from './components/PanelGroup'

  export default {
    name: 'dashboard-admin',
    components: {
      PanelGroup
    }
  }

</script>

<style rel="stylesheet/scss" lang="scss" scoped>
  .dashboard-editor-container {
    padding: 20px;
    background-color: rgb(240, 242, 245);
    min-height: calc(100vh - 84px);

    .chart-wrapper {
      background: #fff;
      padding: 16px 16px 0;
      margin-bottom: 32px;
    }

    .component-item {
      min-height: 100px;
    }
  }

  .product {
    max-width: 350px;
    border: 1px solid #dfdfdf;
    padding: 0;
    border-radius: 8px;
    margin-bottom: 20px;
    background-color: white;

    .title {
      padding: 12px;
      text-align: center;
      background-color: #f4f4f4;
      border-radius: 8px 8px 0 0;
    }

    h2 {
      margin: 24px 0 12px;
    }

    .content {
      h2 {
        text-align: center;
        font-size: 30px;
      }

      .tips {
        padding: 0 35px 30px 30px;

        div {
          padding: 8px 0;
          font-size: 15px;

          .el-icon-check {
            width: 30px;
            font-size: 1.5em;
            color: #35affb;
            vertical-align: middle;
          }
        }

        .color-prime {
          color: #338ed6;
          font-weight: 650;

          .el-icon-check {
            color: gray;
          }
        }

        .color-enterprise {
          color: #f56c6c;
          font-weight: 800;

          .el-icon-check {
            color: #67c23a;
          }
        }
      }
    }
  }

  .action {
    text-align: center;
    padding-bottom: 24px;
    padding: 12px 18px;
    display: flex;
    background: #f4f4f4;

    .btn {
      flex: 1;
      margin: 6px;
      padding: 6px 12px;
      line-height: 26px;
      font-size: 16px;
      background: #fff;
      position: relative;
      cursor: pointer;
      display: inline-block;
      white-space: nowrap;
      border-radius: 4px;
      -ms-touch-action: manipulation;
      touch-action: manipulation;
      font-weight: 700;
      text-decoration: none;
      border: 2px solid #4893ff;
      background-color: transparent;
      color: #4893ff;
      transition: all 0.3s;
    }

    a.btn.std {
      color: #0dafff;
      border-color: #0dafff;
    }

    a.btn.premier {
      color: #d67735;
      border-color: #ffa669;
    }
  }

  .standard .title {
    background: #eaeaea;
    color: #666;
  }

  .prime {
    margin: 0 auto;
  }

  .prime .title {
    background: linear-gradient(#338ed6, #4f6eab);
    color: #fff;
  }

  .enterprise {
    margin-left: auto;
    margin-right: 0;
  }

  .enterprise .title {
    background: linear-gradient(#807979, #484a4e);
    color: #fff;
  }

</style>
