<template>
  <div id="upload_file">
    <el-upload
      class="upload-demo"
      :action="action"
      :headers="headers"
      name="files"
      :on-success="handle_success"
      :on-remove="handle_remove"
      :on-preview="handlePreview"
      :before-remove="beforeRemove"
      multiple
      :limit="5"
      :on-exceed="handleExceed"
      :before-upload="uploadBefore"
      accept=".jpg,.gif,.png,.jpeg,.txt,.pdf,.doc,.docx,.xls,.xlsx"
      :file-list="fileList">
      <el-button size="small" type="primary">Click to Upload</el-button>
    </el-upload>
  </div>
</template>

<script>

export default {
  components: {},
  data () {
    return {
      fileList: [],
      action: `${process.env.VUE_APP_BASE_API}/Files/Upload`,
      headers:{
        "X-Token":this.$store.state.user.token
      },
    }
  },

  mounted: function() {
  },

  methods: {
    handle_remove(file) {
      var _tmp = this.fileList;
      for (var i = 0, len = _tmp.length; i < len; i++) {
        if (_tmp[i].name === file.name) {
          _tmp.splice(i, 1);
          break;
        }
      }
      this.fileList = _tmp;
    },

    handle_success(response, file) {
      this.fileList.push({
        name: file.name,
        url: file.response
      });
    },

    handlePreview(file) {
      console.log(file);
    },

    uploadBefore(file) {
      if (file.size > 10 * 1024 * 1024) {
        this.$message.error("File size exceeded 10M!");
        return false;
      }
    },

    handleExceed() {
      this.$message.warning(`Number of files exceeded 5!`);
    },

    beforeRemove(file) {
      return this.$confirm(`Remove ${ file.name }？`, {
        confirmButtonText: 'Confirm',
        cancelButtonText: 'Cancel'
      });
    }
  }
}
</script>
<style lang='scss' scoped>
</style>