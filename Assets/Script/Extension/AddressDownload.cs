using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;

public class AddressDownload : MonoBehaviour
{
    private GameObject Character = null;
    
    public AssetReference m_pAssetReference = null;

    [Header("캐릭터의 어드레스를 입력해 주세요!")]
    [SerializeField] private string CharacterAddress = string.Empty;
    [SerializeField] Text SizeText;
    [SerializeField] RawImage view;

    [Space]
    [Header("다운로드를 원하는 번들 또는 번들들에 포함된 레이블중 아무거나 입력해주세요.")]
    [SerializeField] string LableForBundleDown = string.Empty;



    private void Start()
    {
        ALF.NETWORK.NetworkManager.AssetsBundlesURL = "http://pasta.service.s3.amazonaws.com/assetsBundles/iOS";
        //Addressables.c
        Character = null;

        Addressables.LoadAssetAsync<GameObject>(m_pAssetReference).Completed += ObjectLoadDone;
    }

    private void ObjectLoadDone(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            Debug.Log("오브젝트 로드 완료");
 
            // Instantiate 를 통해 Hierarchy 뷰에 게임오브젝트를 생성합니다.
            Instantiate(loadedObject);
            Debug.Log("오브젝트 인스턴스화 완료");
 
            // if (accessoryObjectToLoad != null)
            // {
            //     // 비동기 방식으로 에셋을 로드한 다음 바로 Hierarchy 뷰에 게임오브젝트를 생성합니다.
            //     // 람다식으로 작성된 코드
            //     accessoryObjectToLoad.InstantiateAsync(instantiatedObject.transform).Completed += op =>
            //     {
            //         if (op.Status == AsyncOperationStatus.Succeeded)
            //         {
            //             instantiatedAccessoryObject = op.Result;
            //             Debug.Log("Accessory Object 로드와 생성 완료");
            //         }
            //     };
            // }
        }
    }

    public void _ClickSpawn()
    {

        //Character가 null포인터가 아니라면
        //해당 인스턴스를 제거.

        if (!ReferenceEquals(Character, null))
        {
            ReleaseObj();
        }


        //캐릭터를 스폰 한다.
        Spawn();

    }


    void Spawn()
    {
        Addressables.InstantiateAsync(CharacterAddress, new Vector3(Random.Range(-2f, 2f), 5, 0), Quaternion.identity).Completed +=
            (AsyncOperationHandle<GameObject> obj) =>
            {
                Character = obj.Result;
            };

    }

    void ReleaseObj()
    {
        //객체가 삭제될 때 메모리 해제를 위해 레퍼런스 카운트 -1 및 오브젝트인스턴스 제거.
        Addressables.ReleaseInstance(Character);
    }
    IEnumerator CheckCatalogs() 
    {
        List<string> catalogsToUpdate = new List<string>();
        AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates(); // autoReleaseHandle = true 상태 
        checkForUpdateHandle.Completed += op =>
        {
            Debug.Log(op.Result);
            catalogsToUpdate.AddRange(op.Result);
        };

        yield return checkForUpdateHandle;

        Debug.Log(catalogsToUpdate.Count);
        
        if (catalogsToUpdate.Count > 0) 
        {
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return updateHandle;
        }

        // Debug.Log(checkForUpdateHandle.IsValid());
        // Debug.Log(checkForUpdateHandle.Status);
        // Debug.Log(checkForUpdateHandle.IsDone);
    }

    public void _Click_BundleDown()
    {
        // Addressables.LoadAssetAsync<Sprite>(CharacterAddress).Completed += op =>
        //         {
        //             view.texture = op.Result.texture;
        //            SizeText.text = op.Result.texture.name;
        //             // newData.Sprite = op.Result;

        //             // _SetSpriteFunc(op.Result);
        //             //読み込み完了イベント発火
        //             // newData.OnLoadCompleteObserver.OnNext(Unit.Default);
        //         };
        Addressables.DownloadDependenciesAsync(LableForBundleDown).Completed +=
            (AsyncOperationHandle Handle) =>
            {
                //DownloadPercent프로퍼티로 다운로드 정도를 확인할 수 있음.
                //ex) float DownloadPercent = Handle.PercentComplete;
                SizeText.text += "\n다운로드 완료!";
                //다운로드가 끝나면 메모리 해제.
                Debug.Log(Handle.Result);
                Addressables.Release(Handle);

                Addressables.LoadAssetAsync<Sprite>(CharacterAddress).Completed += op =>
                {
                    view.texture = op.Result.texture;
                    // newData.Sprite = op.Result;

                    // _SetSpriteFunc(op.Result);
                    //読み込み完了イベント発火
                    // newData.OnLoadCompleteObserver.OnNext(Unit.Default);
                };
                
            };
    }

    public void _Click_CheckTheDownloadFileSize()
    {
        StartCoroutine(CheckCatalogs());
        //크기를 확인할 번들 또는 번들들에 포함된 레이블을 인자로 주면 됨.
        //long타입으로 반환되는게 특징임.
        // Addressables.GetDownloadSizeAsync(LableForBundleDown).Completed +=
        //     (AsyncOperationHandle<long> SizeHandle) =>
        //     {
        //         string sizeText = SizeHandle.DebugName;
        //         sizeText += string.Concat(SizeHandle.Result, " byte");

        //         SizeText.text = sizeText;

        //         //메모리 해제.
        //         Addressables.Release(SizeHandle);
        //     };


    }
}
