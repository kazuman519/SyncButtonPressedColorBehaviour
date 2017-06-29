using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ボタンの子のImageやTextもボタン押下時の色に同期してくれる君
[RequireComponent(typeof(Button))]
public class SyncButtonPressedColorBehaviour :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private List<Graphic> syncGraphics;
    private Dictionary<Graphic, Color> graphic2DefaultColorMap;
    private bool isPressed;

    private const string PressededColorSequenceId = "PressededColorSequence";


    void Awake()
    {
        this.graphic2DefaultColorMap = new Dictionary<Graphic, Color>();
        this.isPressed = false;

        this.syncGraphics = this.GetComponentsInChildrenWithoutSelf<Graphic>().ToList();
        foreach (var graphic in syncGraphics)
        {
            this.graphic2DefaultColorMap.Add(graphic, graphic.color);
        }
    }


    // ------------------------------------------------------
    // Pointer methods
    // ------------------------------------------------------

    public void OnPointerDown(PointerEventData eventData)
    {
        // 押下時の色を同期
        var pressedColor = this.GetComponent<Button>().colors.pressedColor;

        this.isPressed = true;
        this.GraphicsDOSyncedColorSequence(this.syncGraphics, pressedColor).Play();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 押下終了時の色を同期
        this.isPressed = false;
        DOTween.Kill(PressededColorSequenceId);
        this.GraphicsDODefaultColorSequence(this.syncGraphics, this.graphic2DefaultColorMap).Play();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 押下中にポインターが重なった時の色を同期
        if (!this.isPressed) return;
        var pressedColor = this.GetComponent<Button>().colors.pressedColor;
        this.GraphicsDOSyncedColorSequence(this.syncGraphics, pressedColor).Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 押下中にポインターが外れた時の色を同期
        if (!this.isPressed) return;
        DOTween.Kill(PressededColorSequenceId);
        this.GraphicsDODefaultColorSequence(this.syncGraphics, this.graphic2DefaultColorMap).Play();
    }


    // ---------------------------------------------------------------
    // Sequence methods
    // ---------------------------------------------------------------

    // ボタンの色と合わせるSequence
    Sequence GraphicsDOSyncedColorSequence(
        List<Graphic> graphics,
        Color syncedColor)
    {
        var sequence = DOTween.Sequence().SetId(PressededColorSequenceId);
        graphics.ForEach(graphic =>
            sequence.Join(graphic.DOColor(graphic.color * syncedColor, 0.2f)));
        return sequence;
    }

    // 元の色に戻すSequence
    Sequence GraphicsDODefaultColorSequence(
        List<Graphic> graphics,
        Dictionary<Graphic, Color> graphic2ColorMap)
    {
        var sequence = DOTween.Sequence().SetId(PressededColorSequenceId);
        graphics.ForEach(graphic =>
            sequence.Join(graphic.DOColor(graphic2ColorMap[graphic], 0.1f)));
        return sequence;
    }
}


// ---------------------------------------------------------------
// Exetnsions
// ---------------------------------------------------------------

public static class ComponentExtension
{
    // 自分自身を除くすべての子オブジェクトにアタッチされている指定されたコンポーネントを全て返す
    public static IEnumerable<T> GetComponentsInChildrenWithoutSelf<T>(
        this Component self,
        bool includeInactive = false)
        where T : Component
    {
        return self
            .GetComponentsInChildren<T>(includeInactive)
            .Where(c => self.gameObject != c.gameObject);
    }
}