using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Tools
{

    public class AddressablesLoader : MonoBehaviour
    {
        public static AddressablesLoader instance;
        public static UnityEvent addressablesLoaded;

        [Header("Graphics")]
        public AssetReferenceGameObject damagePopupPrefab;

        [Header("Player")]
        [SerializeField] private AssetReference _playerData;

        [HideInInspector] public Player.PlayerData playerData;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            addressablesLoaded = new UnityEvent();

            Addressables.InitializeAsync();
            StartCoroutine(_PreloadReferences());
        }

        private IEnumerator _PreloadReferences()
        {
            AsyncOperationHandle<Player.PlayerData> playerDataLoadHandle
                = _playerData.LoadAssetAsync<Player.PlayerData>();
            yield return playerDataLoadHandle;
            playerData = playerDataLoadHandle.Result;

            addressablesLoaded.Invoke();
        }

        private void OnApplicationQuit()
        {
            _playerData.ReleaseAsset();
        }

    }

}