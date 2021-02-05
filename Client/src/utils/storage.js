import storage from 'good-storage'

/* localStorage */
export function getLocalStorage (key) {
  return storage.get(key)
}

export function setLocalStorage (key, value) {
  console.log('set', key, value)
  return storage.set(key, value)
}

export function hasLocalStorage (key) {
  console.log('has', 'key')
  return storage.has(key)
}
export function removeLocalStorage (key) {
  storage.remove(key)
}


/* sessionStorage */
export function getSessionStorage (key) {
  return storage.session.get(key)
}

export function setSessionStorage (key, value) {
  console.log('set', key, value)
  return storage.session.set(key, value)
}

export function hasSessionStorage (key) {
  console.log('has', 'key')
  return storage.session.has(key)
}
export function removeSessionStorage (key) {
  storage.session.remove(key)
}

export function setObject (obj, key, value, type = 'sessionStorage') {
  let { getStorage, setStorage } = transformFn(type)
  let target = {}
  if (getStorage(obj)) {
    target = getStorage(obj)
  }
  target[key] = value
  setStorage(obj, target)
}

export function getObject (obj, key, type ='sessionStorage') {
  let { getStorage } = transformFn(type)
  return getStorage(obj)[key]
}

function transformFn (type = 'localStorage') {
  let getStorage = obj => type === 'sessionStorage' ? getSessionStorage(obj) : getLocalStorage(obj)
  let setStorage = (obj, target) => type === 'sessionStorage' ? setSessionStorage(obj, target) : setLocalStorage(obj, target)
  return {
    getStorage,
    setStorage
  }
}