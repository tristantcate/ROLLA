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

        [HideInInspector] public Transform m_parallaxLayerObject;
        [HideInInspector] public Transform[] m_parallaxObjects;
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
                sr.sortingOrder = parallaxLayer.m_spriteRenderLayer;
                sr.sortingLayerName = "Parallax";

                if (parallaxLayer.m_flipXPerSprite && parallaxObjIter % 2 == 1) sr.flipX = true;
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
            parallaxLayer.m_parallaxLayerObject.position = cameraToFollow.transform.position * parallaxLayer.m_layerSpeed + Vector3.up * parallaxLayer.m_layerHeight;

            foreach (Transform parallaxObj in parallaxLayer.m_parallaxObjects)
            {
                if(((parallaxObj.position.x + parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment) - parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment / 2) - cameraToFollow.transform.position.x < -m_parallaxBounds)
                {
                    float xOffset = (parallaxLayer.m_objectDistance * parallaxLayer.m_spriteScaleAdjustment) * parallaxLayer.m_parallaxObjects.Length;
                    parallaxObj.transform.Translate(xOffset, 0.0f, 0.0f);

                    if(parallaxLayer.m_flipXPerSprite && parallaxLayer.m_parallaxObjects.Length % 2 == 1)
                    {
                        parallaxObj.GetComponent<SpriteRenderer>().flipX = !parallaxObj.GetComponent<SpriteRenderer>().flipX;
                    }
                }
            }
        }
    }
}
