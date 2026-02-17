using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSate : MonoBehaviour
{
    public Color activeColour=new Color(255f/255f,141f/255f,0f/255f);
    public Color inactiveColour=new Color(104f/255f,65f/255f,18f/255f);
    private TMP_Text buttonText;
    private Button attachedButton;
    public int cost; //used by menu to know if the player can afford a weapon
    public int maximumAllowed;

    public void Initialize()
    {
        buttonText = GetComponentInChildren<TMP_Text>(true);
        attachedButton = GetComponent<Button>();

    }

    public void ChangeState(bool active)
    {

        if (buttonText == null|| attachedButton==null) return;

        if (active)
        {
            buttonText.color = activeColour;
            attachedButton.interactable = true;
        }
        else
        {
            buttonText.color = inactiveColour;
            attachedButton.interactable = false;
        }
    }

}
