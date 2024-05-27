using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomButton : Button
{
    public Text buttonText; // Button 텍스트를 할당

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        if (buttonText == null)
            return;

        Color targetColor;
        switch (state)
        {
            case SelectionState.Normal:
                targetColor = colors.normalColor;
                break;
            case SelectionState.Highlighted:
                targetColor = colors.highlightedColor;
                break;
            case SelectionState.Pressed:
                targetColor = colors.pressedColor;
                break;
            case SelectionState.Disabled:
                targetColor = colors.disabledColor;
                break;
            default:
                targetColor = Color.black;
                break;
        }

        if (instant)
        {
            buttonText.color = targetColor;
        }
        else
        {
            buttonText.CrossFadeColor(targetColor, colors.fadeDuration, true, true);
        }
    }
}
