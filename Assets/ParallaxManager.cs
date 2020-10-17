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
        public Sprite m_sprite;
        public float m_objectDistance;
        public float m_spriteScaleAdjustment;
        public bool m_flipXPerSprite;

        [Header("Random Decoration")]
        public DecorationSpecifier[] m_decorations;

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

                SpriteRenderer sr = parallaxObj.AddComponent<SpriteRenderer>();
                sr.sprite = parallaxLayer.m_sprite;
                sr.sortingOrder = parallaxLayer.m_spriteRenderLayer * 1000;
                sr.sortingLayerName = "Parallax";

                if (parallaxLayer.m_flipXPerSprite && parallaxObjIter % 2 == 1) sr.flipX = true;
            }

            //Setup list of decorations
            parallaxLayer.m_decoPrefabInstances = new List<GameObject>[parallaxObjAmount];
            for (int i = 0; i < parallaxLayer.m_decoPrefabInstances.Length; i++)
                parallaxLayer.m_decoPrefabInstances[i] = new List<GameObject>();
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
            parallaxLayer.m_parallaxLayerObject.position = cameraToFollow.transform.position * parallaxLayer.m_layerSpeed + Vector3.up * parallaxLayer.m_layerHeight;

            foreach (Transform parallaxObj in parallaxLayer.m_parallaxObjects)
            {
                if(((parallaxObj.position.x + parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment) - parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment / 2) - cameraToFollow.transform.position.x < -m_parallaxBounds)
                {
                    float xOffset = (parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment) * parallaxLayer.m_parallaxObjects.Length;
                    parallaxObj.transform.Translate(xOffset, 0.0f, 0.0f);

                    if(parallaxLayer.m_flipXPerSprite && parallaxLayer.m_parallaxObjects.Length % 2 == 1)
                    {
                        SpriteRenderer parallaxObjSR = parallaxObj.GetComponent<SpriteRenderer>();
                        parallaxObjSR.flipX = !parallaxObjSR.flipX;
                    }

                    AddDecorations(parallaxObj, parallaxLayer);
                }
            }
        }
    }

    private void AddDecorations(Transform a_parent, ParallaxLayer a_parallaxLayer)
    {
        foreach (GameObject currentDecoInstances in a_parallaxLayer.m_decoPrefabInstances[GetParallaxObjectID(a_parent, a_parallaxLayer)])
        {
            Destroy(currentDecoInstances);
        }

        a_parallaxLayer.m_decoPrefabInstances[GetParallaxObjectID(a_parent, a_parallaxLayer)].Clear();

        DecorationSpecifier currentParallaxDeco = a_parallaxLayer.m_decorations[0];
        for (
            float 
            y = currentParallaxDeco.m_decoAreaMin.y;
            y < currentParallaxDeco.m_decoAreaMax.y;
            y += Random.Range(currentParallaxDeco.m_minDistanceInBetween.y, currentParallaxDeco.m_maxDistanceInBetween.y)
            ) {

            for (
                float
                x = currentParallaxDeco.m_decoAreaMin.x;
                x < currentParallaxDeco.m_decoAreaMax.x;
                x += Random.Range(currentParallaxDeco.m_minDistanceInBetween.x, currentParallaxDeco.m_maxDistanceInBetween.x)
                )
            {

                //Recalculate Y as we don't want to have "rows"
                float newY = Random.Range(currentParallaxDeco.m_minDistanceInBetween.y, currentParallaxDeco.m_maxDistanceInBetween.y) - currentParallaxDeco.m_maxDistanceInBetween.y/2;

                GameObject decoObj = new GameObject(string.Format("decoObj[{0},{1}]", x, newY + y));
                decoObj.transform.parent = a_parent;
                //decoObj.transform.position = Vector3.zero;
                decoObj.transform.localPosition = new Vector3(x, newY + y);
                decoObj.transform.localScale = Vector3.one * currentParallaxDeco.m_scaleAdjust;
                a_parallaxLayer.m_decoPrefabInstances[GetParallaxObjectID(a_parent, a_parallaxLayer)].Add(decoObj);

                SpriteRenderer sr = decoObj.AddComponent<SpriteRenderer>();
                sr.sprite = currentParallaxDeco.m_randomSprites[Random.Range(0, currentParallaxDeco.m_randomSprites.Length)];
                sr.sortingLayerName = "Parallax";
                sr.sortingOrder = a_parallaxLayer.m_spriteRenderLayer * 1000;
                sr.sortingOrder += Mathf.RoundToInt(-y * 10.0f) + 500;
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
