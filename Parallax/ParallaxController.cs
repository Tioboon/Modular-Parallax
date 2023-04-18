using System;
using System.Collections;
using System.Collections.Generic;
using Environment.Parallax;
using Global;
using Helper;
using Helpers;
using Helpers.Enums;
using Modularization;
using Player.Movement;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public class ParallaxController : SerializedMonoBehaviour, IController
{
    public Camera mainCamera;
    private List<Transform> layers;
    
    //0 é o tamanho da camera e 1 é o tamanho da imagem
    private List<LayerCreationInfo> createdLayersInfo;
    //Idx = layer || Dic k=dir(referente a layer, ou seja, um objeto pra cada dir) , v=layerInfo
    private List<Dictionary<BaseDirection, LayerInstance>> sceneries;
    private Dictionary<BaseDirection, Transform> containers;

    private GameObject[] boundsCollider;
    private GameObject colliderContainer;
    
    public Transform baseObject;
    public List<BaseDirection> dirsToMirror;
    
    public Dictionary<Transform, LayerSerializedInfo> layersPosition;


    [Button]
    public void GetLayers()
    {
        layers = new List<Transform>();
        foreach (Transform child in baseObject) layers.Add(child);

        layersPosition ??= new Dictionary<Transform, LayerSerializedInfo>();
        layersPosition.AddAllListAsKeys(layers);
    }

    private void Start()
    {
        NormalFunc();
    }

    //SCENERY -> LAYERS -> OBJECTS -> PIECES
    private void NormalFunc()
    {
        var baseLayersInfo = new List<LayerCreationInfo>();

        //Pega as bounds inicias do objeto. LAYERS
        int layerIdx = 0;
        layersPosition.ForEach(kp => baseLayersInfo.Add(GetLayerBound(kp, ref layerIdx)));

        //Cria as instancias das cópias. SCENERIES (O nome é a direção);
        var sceneryTransform = new Dictionary<BaseDirection, Transform>();
        dirsToMirror.ForEach(dir => sceneryTransform.Add(dir, CreateSceneryCopies(dir)));
        
        //Cria objetos vazios para reposicionar as layers e manter tracking da direção
        //Dictionary<BaseDirection, Transform> dirsWithPostDirs = CreateEmptyObjectsForExtends();
        //sceneryTransform.AddRange(dirsWithPostDirs);
        
        containers = sceneryTransform;
        
        
        var sceneryInfo = new Dictionary<BaseDirection, List<Transform>>();
        //Ajusta os valores do transform das layers dentro de cada scenery (para cada direção)
        for (int i = 0; i < sceneryTransform.Count; i++) 
            sceneryInfo.Add(sceneryTransform.GetFromIdx(i).Key, 
                AdjustSceneryInsideTransforms(sceneryTransform.GetFromIdx(i), baseLayersInfo));

        Dictionary<BaseDirection, List<LayerCollider>> colliderPerScenery = new Dictionary<BaseDirection, List<LayerCollider>>();
        sceneryInfo.ForEach(kp => colliderPerScenery.Add(kp.Key, CreateSceneryLayersCollider(kp.Value, baseLayersInfo)));

        Dictionary<BaseDirection, List<LayerInstance>> layersInstance = MergeInfosToInstance(sceneryInfo, colliderPerScenery);
        Dictionary<BaseDirection, SceneryInstance> sceneryInstances = SetSceneriesInstance(layersInstance, sceneryTransform);
        List<Dictionary<BaseDirection, LayerInstance>> mergedLayers = SplitAndMergeLayer(sceneryInstances);
        sceneries = mergedLayers;

        //Debug
        //DebugParallaxBoundingBox();
    }

    private List<Dictionary<BaseDirection, LayerInstance>> SplitAndMergeLayer(Dictionary<BaseDirection, SceneryInstance> sceneryInstances)
    {
        int actualCheckIdx = 0;
        List<Dictionary<BaseDirection, LayerInstance>> mergedLayersByIdx = new List<Dictionary<BaseDirection, LayerInstance>>();
        while (actualCheckIdx < layersPosition.Count)
        {
            Dictionary<BaseDirection, LayerInstance> layersByIdx = new Dictionary<BaseDirection, LayerInstance>();
            foreach (var kp in sceneryInstances)
            {
                BaseDirection dir = kp.Key;
                SceneryInstance sceneryInstance = kp.Value;
                if(sceneryInstance.layers.Count == 0) continue;
                layersByIdx.AttOrAddToDictionary(dir, sceneryInstance.layers[actualCheckIdx]);
            }
            mergedLayersByIdx.Add(layersByIdx);
            
            actualCheckIdx++;
        }

        return mergedLayersByIdx;
    }

    private Dictionary<BaseDirection, SceneryInstance> SetSceneriesInstance(Dictionary<BaseDirection, List<LayerInstance>> layersInstance, 
        Dictionary<BaseDirection, Transform> sceneryTransform)
    {
        Dictionary<BaseDirection, SceneryInstance> newSceneries = new Dictionary<BaseDirection, SceneryInstance>();
        foreach (var dir in layersInstance.Keys)
        {
            newSceneries.Add(dir, new SceneryInstance(sceneryTransform[dir], layersInstance[dir]));
        }

        return newSceneries;
    }

    private Dictionary<BaseDirection, List<LayerInstance>> MergeInfosToInstance(Dictionary<BaseDirection, List<Transform>> sceneryInfo,
        Dictionary<BaseDirection, List<LayerCollider>> colliderPerScenery)
    {
        var newLayersInstance = new Dictionary<BaseDirection, List<LayerInstance>>();

        foreach (var dir in sceneryInfo.Keys)
        {
            List<LayerInstance> newLayerList = new List<LayerInstance>();
            for (int i = 0; i < sceneryInfo[dir].Count; i++)
            {
                newLayerList.Add(new LayerInstance(sceneryInfo[dir][i], colliderPerScenery[dir][i]));
            }
            newLayersInstance.Add(dir, newLayerList);
        }

        return newLayersInstance;
    }

    /*
    private Dictionary<BaseDirection, Transform> CreateEmptyObjectsForExtends()
    {
        Dictionary<BaseDirection, Transform> dirsWithPostDirs = new Dictionary<BaseDirection, Transform>();
        foreach (var baseDir in dirsToMirror)
        {
            var postValues = DirectionInfo.postValues;
            if(!postValues.ContainsKey(baseDir)) continue;
            
            var newObj = new GameObject().transform;
            newObj.position = transform.position;
            newObj.parent = transform;
            
            BaseDirection postDir = postValues[baseDir];
            newObj.name = Enum.GetName(typeof(BaseDirection), postDir)!;
            dirsWithPostDirs.Add(postDir, newObj);
        }

        return dirsWithPostDirs;
    }
    */

    private List<LayerCollider> CreateSceneryLayersCollider(List<Transform> sceneriesTransforms, List<LayerCreationInfo> baseInfo)
    {
        List<LayerCollider> layerCol = new List<LayerCollider>();

        if (sceneriesTransforms.Count == 0) return new List<LayerCollider>();
        
        for (int i = 0; i < baseInfo.Count; i++) layerCol.Add(CreateLayerCollider(sceneriesTransforms[i], baseInfo[i]));

        return layerCol;
    }

    private LayerCollider CreateLayerCollider(Transform sceneryTransform, LayerCreationInfo layerInfo)
    {
        GameObject newColliderObj = new GameObject();
        newColliderObj.name = $"Collider : {layerInfo.baseName}";

        Transform layerPreMadeObj = sceneryTransform;
        Transform newLayerT = newColliderObj.transform;
        newLayerT.position = layerPreMadeObj.position;
        newLayerT.parent = layerPreMadeObj;


        EdgeCollider2D layerCollider = newColliderObj.AddComponent<EdgeCollider2D>();
        layerCollider.isTrigger = true;
        layerCollider.SetPoints(CreateEdgeCollider(layerInfo.bounds));

        CollisionAngleInfo layerCollision = newColliderObj.AddComponent<CollisionAngleInfo>();

        LayerCollider newLayerCol = newColliderObj.AddComponent<LayerCollider>(); 
        newLayerCol.Initialize(this, layerInfo.idx, layerCollision, layerCollider, sceneryTransform);
        return newLayerCol;
    }
    
    private List<Transform> AdjustSceneryInsideTransforms(KeyValuePair<BaseDirection, Transform> dirTransform, List<LayerCreationInfo> layerInfoList) //Isso aqui não seria scenery?
    {
        var sceneryTransform = dirTransform.Value;
        BaseDirection baseDir = dirTransform.Key;
        
        List<Transform> layers = new List<Transform>();
        
        //Ajusta o tamanho de cada layer
        for (int j = 0; j < sceneryTransform.childCount; j++)
        {
            var child = sceneryTransform.GetChild(j);
            var layerInstance = layerInfoList[j];
            Vector2 screenSize = DirectionInfo.vectors[baseDir];
            Vector2 layerSize = layerInstance.bounds.size;
            
            Vector3 newPos = layerSize.MultiplyAxis(screenSize);
            child.position += layerInstance.position + newPos;
            child.localScale *= layerInstance.proportion;

            layers.Add(child);
        }

        return layers;
    }
    

    private List<Vector2> CreateEdgeCollider(Bounds layerBound)
    {
        var rightBot = new Vector3(layerBound.max.x, layerBound.min.y, layerBound.max.z); 
        var leftTop = new Vector3(layerBound.min.x, layerBound.max.y, layerBound.max.z);
        var leftBot = layerBound.min; 
        var rightTop = layerBound.max;
        List<Vector2> newEdgePoints = new List<Vector2>()
        {
            leftBot,
            rightBot,
            rightTop,
            leftTop,
            leftBot
        };
        return newEdgePoints;
    }

    private LayerCreationInfo GetLayerBound(KeyValuePair<Transform, LayerSerializedInfo> kp, ref int layerIdx)
    {
        var layerT = kp.Key;
        var layerInfo = kp.Value;

        //Position
        var layerPos = layerT.position;
        layerPos = new Vector3(layerPos.x, layerPos.y, layerInfo.z);
        
        var proportion = mainCamera.GetScreenProportion(layerInfo.z);

        //Bounds
        List<Bounds> objectBounds = new List<Bounds>();
        for (int objectIndex = 0; objectIndex < layerT.childCount; objectIndex++) 
            objectBounds.Add(GetObjectBounds(layerT.GetChild(objectIndex), layerInfo, objectIndex));

        var newBounds = objectBounds.MergeBounds();

        layerIdx++;
        
        return new LayerCreationInfo(layerPos, newBounds, proportion, layerT.name, layerIdx-1);
    }

    private Bounds GetObjectBounds(Transform objectTransform, LayerSerializedInfo layerSerializedInfo, int objectIndex, bool debug = false)
    {
        List<Bounds> piecesBounds = new List<Bounds>();
        foreach (Transform spritePiece in objectTransform) piecesBounds.Add(GetPieceBounds(spritePiece, layerSerializedInfo, objectIndex, debug));
        
        return piecesBounds.MergeBounds();
    }

    private Bounds GetPieceBounds(Transform sceneryPiece, LayerSerializedInfo layerSerializedInfo, int objectIndex, bool debug = false)
    {
        SpriteRenderer pieceRend = sceneryPiece.GetComponent<SpriteRenderer>();
        
        pieceRend.sortingOrder = 
            layerSerializedInfo.startSortingOrder + layerSerializedInfo.sortingLayerVariation * (objectIndex + 1) + pieceRend.sortingOrder;
        
        var bounds = pieceRend.bounds;

        return bounds;
    }

    private Transform CreateSceneryCopies(BaseDirection actualDirection)
    {
        var newScenery = Instantiate(baseObject, transform);
        newScenery.gameObject.SetActive(true);
        newScenery.name = Enum.GetName(typeof(BaseDirection), actualDirection) ?? ($"{actualDirection}");
        return newScenery;
    }

    
    private float waitToNewCol;
    public bool ReceiveMessage(BaseDirection side, int idx)
    {
        PlayerMovementController playerInstance = PlayerMovementController.Instance;
        if (playerInstance == null) return false;
        Debug.Log(playerInstance.rb.velocity.x);
        switch (side)
        {
            case BaseDirection.Up:
                return false;
            case BaseDirection.Down:
                return false;
            case BaseDirection.Right:
                if (playerInstance.rb.velocity.x > 0) return false;
                RepositionContainer(BaseDirection.Right, BaseDirection.Left, idx);
                waitToNewCol = Global_Timers.MinTimeCollisionWait;
                StartCoroutine(ResetColTimer());
                return true;
            case BaseDirection.Left:
                if (playerInstance.rb.velocity.x < 0) return false;
                RepositionContainer(BaseDirection.Left, BaseDirection.Right, idx);
                waitToNewCol = Global_Timers.MinTimeCollisionWait;
                StartCoroutine(ResetColTimer());
                return true;
            case BaseDirection.None:
                return false;
            default:
                return false;
        }
    }

    private void RepositionContainer(BaseDirection from, BaseDirection to, int idx)
    {
        if(waitToNewCol > 0) return;

        var scenery = sceneries[idx];
        var newNeighbour = scenery[to];
        var newNeighbourBounds = newNeighbour.layerCol.collider.bounds;

        Vector2 size = newNeighbourBounds.size;
        Vector2 screenSize = DirectionInfo.vectors[to];
        Vector3 displacement = size.MultiplyAxis(screenSize);
        Vector3 newPos = newNeighbourBounds.center + displacement;

        LayerInstance targetLayer = scenery[from];
        var targetTransform = targetLayer.transform;
        var targetGameObj = targetTransform.gameObject;

        targetGameObj.SetActive(false);
        
        targetTransform.position = newPos;

        var centerLayer = scenery[BaseDirection.Center];

        targetLayer.transform.parent = containers[to];
        newNeighbour.transform.parent = containers[BaseDirection.Center];
        centerLayer.transform.parent = containers[from];
        
        targetGameObj.SetActive(true);

        //Todo: Caso o container do cenário ficar vázio, as direções tem que trocar
    }

    private IEnumerator ResetColTimer()
    {
        if(waitToNewCol <= 0) yield break;
        yield return new WaitForFixedUpdate();
        waitToNewCol -= Time.fixedDeltaTime;
        StartCoroutine(ResetColTimer());
    }
}
