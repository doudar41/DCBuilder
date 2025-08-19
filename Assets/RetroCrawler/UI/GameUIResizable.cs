using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIResizable : MonoBehaviour
{
    [SerializeField] RectTransform fullScreenPanel;
    [SerializeField]
    List<RectTransform> panels = new List<RectTransform>();
    [SerializeField]
    Camera cam;

    private void Start()
    {
        ResizeWindowsToOnePanel();

    }


    private void Update()
    {
        ResizeWindowsToOnePanel();
    }

    void ResizeWindowsToOnePanel()
    {
        panels[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullScreenPanel.rect.height);
        panels[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullScreenPanel.rect.width * (200.0f / 800.0f));

        panels[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullScreenPanel.rect.width - panels[1].rect.width);
        panels[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullScreenPanel.rect.height * (160.0f / 600.0f));

        panels[2].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullScreenPanel.rect.width - panels[1].rect.width);
        panels[2].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullScreenPanel.rect.height - panels[0].rect.height);
    }
}
