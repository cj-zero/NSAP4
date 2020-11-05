import storage from 'good-storage'

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

export function setObject (obj, key, value) {
  let target = {}
  if (getLocalStorage(obj)) {
    target = getLocalStorage(obj)
  }
  target[key] = value
  setLocalStorage(obj, target)
}

export function getObject (obj, key) {
  return getLocalStorage(obj)[key]
}


export function getSessionStorage (key) {
  return storage.session.get(key)
}

export function setSessionStorage (key, value) {
  storage.session.set(key, value)
}