using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    [SerializeField] private Camera cameraToFollow;

    [SerializeField] private float m_parallaxBounds = 18.0f;

    [System.Serializable]
    public class ParallaxLayer
    {
        [Header("Layer Settings")]
        public int m_spriteRenderLayer;
        public float m_layerSpeed;
        public float m_layerHeight;

        [Header("Sprite Settings")]
        public Sprite[] m_sprites;
        public float m_objectDistance;
        public float m_spriteScaleAdjustment;
        public bool m_flipXPerSprite;

        [Header("Random Decoration")]
        public DecorationCollection m_decorationCollection;

        [HideInInspector] public Transform m_parallaxLayerObject;
        [HideInInspector] public Transform[] m_parallaxObjects;

        [HideInInspector] public List<GameObject>[] m_decoPrefabInstances;
    }



    [SerializeField] private ParallaxLayer[] m_parallaxLayers;

    private void Start()
    {
        foreach (ParallaxLayer parallaxLayer in m_parallaxLayers)
        {
            parallaxLayer.m_parallaxLayerObject = new GameObject("ParallaxLayer").transform;
            parallaxLayer.m_parallaxLayerObject.position += Vector3.up * parallaxLayer.m_layerHeight;

            int parallaxObjAmount = CalculateObjectAmountByDistance(parallaxLayer.m_objectDistance, parallaxLayer.m_spriteScaleAdjustment);
            parallaxLayer.m_parallaxObjects = new Transform[parallaxObjAmount];

            for(int parallaxObjIter = 0; parallaxObjIter < parallaxObjAmount; parallaxObjIter++)
            {
                float startXPos = (parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment) / 2;
                float x = -m_parallaxBounds + startXPos + parallaxLayer.m_objectDistance * parallaxObjIter * parallaxLayer.m_spriteScaleAdjustment;

                GameObject parallaxObj = new GameObject(string.Format("ParallaxObj[{0}]", parallaxObjIter));
                parallaxObj.transform.SetParent(parallaxLayer.m_parallaxLayerObject);
                parallaxObj.transform.localPosition = new Vector3(x, 0.0f, 0.0f);
                parallaxObj.transform.localScale = Vector3.one * parallaxLayer.m_spriteScaleAdjustment;

                parallaxLayer.m_parallaxObjects[parallaxObjIter] = parallaxObj.transform;

                if(parallaxLayer.m_sprites.Length > 0)
                {
                    SpriteRenderer sr = parallaxObj.AddComponent<SpriteRenderer>();
                    sr.sprite = parallaxLayer.m_sprites[Random.Range(0, parallaxLayer.m_sprites.Length)];
                    sr.sortingOrder = parallaxLayer.m_spriteRenderLayer * 1000;
                    sr.sortingLayerName = "Parallax";
                    if (parallaxLayer.m_flipXPerSprite && parallaxObjIter % 2 == 1) sr.flipX = true;

                }

            }

            //Setup list of decorations
            parallaxLayer.m_decoPrefabInstances = new List<GameObject>[parallaxObjAmount];
            for (int i = 0; i < parallaxLayer.m_decoPrefabInstances.Length; i++)
            {
                parallaxLayer.m_decoPrefabInstances[i] = new List<GameObject>();
                AddDecorations(parallaxLayer.m_parallaxObjects[i], parallaxLayer);

            }
        }
    }

    private int CalculateObjectAmountByDistance(float a_parallaxObjectDistance, float scale = 1.0f)
    {
        a_parallaxObjectDistance *= scale;

        int objectAmountToReturn = 0;
        float currentX = -m_parallaxBounds - a_parallaxObjectDistance / 2;
        while(currentX < m_parallaxBounds + a_parallaxObjectDistance / 2)
        {
            currentX += a_parallaxObjectDistance;
            objectAmountToReturn++;
        }

        return objectAmountToReturn;
    }

    void Update()
    {
        foreach (ParallaxLayer parallaxLayer in m_parallaxLayers)
        {
            parallaxLayer.m_parallaxLayerObject.position = (cameraToFollow.transform.position * parallaxLayer.m_layerSpeed) + Vector3.up * parallaxLayer.m_layerHeight;

            foreach (Transform parallaxObj in parallaxLayer.m_parallaxObjects)
            {
                if(((parallaxObj.position.x + parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment) - parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment / 2) - cameraToFollow.transform.position.x < -m_parallaxBounds)
                {
                    float xOffset = (parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment) * parallaxLayer.m_parallaxObjects.Length;
                    parallaxObj.transform.Translate(xOffset, 0.0f, 0.0f);

                    if(parallaxLayer.m_flipXPerSprite && parallaxLayer.m_parallaxObjects.Length % 2 == 1)
                    {
                        if (parallaxLayer.m_sprites.Length > 0)
                        {
                            SpriteRenderer parallaxObjSR = parallaxObj.GetComponent<SpriteRenderer>();
                            if(parallaxLayer.m_flipXPerSprite) parallaxObjSR.flipX = !parallaxObjSR.flipX;
                            parallaxObjSR.sprite = parallaxLayer.m_sprites[Random.Range(0, parallaxLayer.m_sprites.Length)];

                        }
                    }

                    AddDecorations(parallaxObj, parallaxLayer);
                }
            }
        }
    }

    private DecorationAssembly GetRandomDecoCollection(ParallaxLayer a_layer)
    {
        if (a_layer.m_decorationCollection == null) return null;
        int searchRand = Random.Range(0, 101);
        int currentRand = 0;
        foreach (DecorationAssembly decoAssembly in a_layer.m_decorationCollection.m_decorations)
        {
            currentRand += decoAssembly.m_spawnChance;
            if (currentRand >= searchRand) return decoAssembly; 
        }

        Debug.LogError("NO RANDOMDECO COLLECTION FOUND. ARE THE PROCENTAGES FILLED IN CORRECTLY?");
        return null;
    }

    private void AddDecorations(Transform a_parent, ParallaxLayer a_parallaxLayer)
    {
        if (a_parallaxLayer.m_decorationCollection == null) return;
        //Destroy current (off screen) parallax decorations
        foreach (GameObject currentDecoInstances in a_parallaxLayer.m_decoPrefabInstances[GetParallaxObjectID(a_parent, a_parallaxLayer)])
            Destroy(currentDecoInstances);
        a_parallaxLayer.m_decoPrefabInstances[GetParallaxObjectID(a_parent, a_parallaxLayer)].Clear();

        DecorationAssembly randomDecoCollection = GetRandomDecoCollection(a_parallaxLayer);
        foreach (DecorationSpecifier currentParallaxDeco in randomDecoCollection.m_decorationSpecifiers)
        {

            for (
                float
                y = currentParallaxDeco.m_decoAreaMin.y;
                y < currentParallaxDeco.m_decoAreaMax.y;
                )
            {

                for (
                    float
                    x = currentParallaxDeco.m_decoAreaMin.x;
                    x < currentParallaxDeco.m_decoAreaMax.x;
                    x += Random.Range(currentParallaxDeco.m_minDistanceInBetween.x, currentParallaxDeco.m_maxDistanceInBetween.x) * currentParallaxDeco.m_scaleAdjust
                    )
                {

                    //Recalculate Y as we don't want to have "rows"
                    float newY = Random.Range(currentParallaxDeco.m_minDistanceInBetween.y, currentParallaxDeco.m_maxDistanceInBetween.y) - currentParallaxDeco.m_maxDistanceInBetween.y / 2;
                    newY *= currentParallaxDeco.m_scaleAdjust;

                    GameObject decoObj = new GameObject(string.Format("decoObj[{0},{1}]", x, newY + y));
                    decoObj.transform.parent = a_parent;
                    decoObj.transform.localPosition = new Vector3(x, newY + y);
                    decoObj.transform.localScale = Vector3.one * currentParallaxDeco.m_scaleAdjust;
                    a_parallaxLayer.m_decoPrefabInstances[GetParallaxObjectID(a_parent, a_parallaxLayer)].Add(decoObj);

                    SpriteRenderer sr = decoObj.AddComponent<SpriteRenderer>();
                    sr.sprite = currentParallaxDeco.m_randomSprites[Random.Range(0, currentParallaxDeco.m_randomSprites.Length)];
                    sr.sortingLayerName = "Parallax";
                    sr.sortingOrder = a_parallaxLayer.m_spriteRenderLayer * 1000;
                    sr.sortingOrder += Mathf.RoundToInt(-decoObj.transform.localPosition.y * 10.0f) + 500 + currentParallaxDeco.m_layerOrderOffset * 10;

                    if(currentParallaxDeco.m_lerpColor1.a != 0 && currentParallaxDeco.m_lerpColor2.a != 0)
                    {
                        sr.color = Color.Lerp(currentParallaxDeco.m_lerpColor1, currentParallaxDeco.m_lerpColor2, Random.Range(0.0f, 1.0f));
                    }
                }

                y += Random.Range(currentParallaxDeco.m_minDistanceInBetween.y, currentParallaxDeco.m_maxDistanceInBetween.y) * currentParallaxDeco.m_scaleAdjust;
            }

        }

    }

    private int GetParallaxObjectID(Transform a_parallaxObj, ParallaxLayer a_layer)
    {
        for(int i = 0; i < a_layer.m_parallaxObjects.Length; i++)
        {
            if (a_layer.m_parallaxObjects[i] == a_parallaxObj) return i;
        }
        return -1;

        Debug.LogError("NO PARALLAXOBJECT ID FOUND!");
    }
}
