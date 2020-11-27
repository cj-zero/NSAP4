export function print (url) {
  var wind = window.open(url,'newwindow', 'height=300, width=700, top=100, left=100, toolbar=no, menubar=no, scrollbars=no, resizable=no,location=n o, status=no');

  wind.print();
}

export function download (src) {
  let canvas = document.createElement('canvas')
  const context = canvas.getContext("2d");
  let img = new Image()
  img.setAttribute("crossOrigin", "anonymous");
  img.onload = function () {
    let link = document.createElement('a')
    canvas.width = img.width;
    canvas.height = img.height;
    // const context = canvas.getContext("2d");
    context.drawImage(img, 0, 0, img.width, img.height);
    link.href = canvas.toDataURL('image/png')
    link.download = ''
    link.target = '_self'
    link.click()
  }
  img.src = src
}

export function downloadFile (url, isDownload = true) {
  let a = document.createElement('a')
  a.href = url
  if (isDownload) {
    a.download = url
  }
  a.target = '_blank'
  a.click()
}

