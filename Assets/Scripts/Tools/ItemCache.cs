using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{

    public class ItemCache
    {

        private Dictionary<string, GameObject> _cache;
        private Action<GameObject> _popFromCache;
        private Action<GameObject> _pushToCache;

        private static void _DefaultPopFromCache(GameObject g) { g.SetActive(true); }
        private static void _DefaultPushToCache(GameObject g) { g.SetActive(false); }

        public ItemCache()
        {
            _cache = new Dictionary<string, GameObject>();
            _popFromCache = _DefaultPopFromCache;
            _pushToCache = _DefaultPushToCache;
        }
        public ItemCache(Action<GameObject> popFromCache, Action<GameObject> pushToCache)
        {
            _cache = new Dictionary<string, GameObject>();
            _popFromCache = popFromCache;
            _pushToCache = pushToCache;
        }

        public void Add(string key, GameObject prefab, Transform parent = null)
        {
            if (!_cache.ContainsKey(key))
                _cache.Add(key, GameObject.Instantiate(prefab, parent));

            _popFromCache(_cache[key]);
        }

        public void Remove(string key)
        {
            if (!_cache.ContainsKey(key))
                return;
            _pushToCache(_cache[key]);
        }

    }

}
