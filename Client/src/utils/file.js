export function download (src) {
  let a = document.createElement('a')
  a.download = ''
  a.href = src
  a.target = 'self'
  console.log(a,' a')
  document.body.appendChild(a)
  a.click()
}

export function print (url) {
  var wind = window.open(url,'newwindow', 'height=300, width=700, top=100, left=100, toolbar=no, menubar=no, scrollbars=no, resizable=no,location=n o, status=no');

  wind.print();
}