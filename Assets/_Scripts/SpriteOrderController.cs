using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SpriteOrderController : MonoBehaviour
{
    int defaultOrderInLayer;
    [SerializeField] SortingGroup sortingGroup;

    private void Awake() {
        defaultOrderInLayer = sortingGroup.sortingOrder;
    }

    public void SetOrderInLayer(int order) {
        sortingGroup.sortingOrder = order;
    }

    public void ReturnToDefaultOrderInLayer() {
        sortingGroup.sortingOrder = defaultOrderInLayer;
    }
}
