﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;

public class CharacterSelectionAction : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{

    public Animator spriteAnimator;

    private CharacterInfo characterInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnSelect(BaseEventData eventData)
    {
        characterInfo = GameObject.Find("GameController").GetComponent<CharacterInfo>();
        int characterIndex = CharacterInfo.characters[gameObject.tag];
        spriteAnimator.SetInteger("characterSelectState", characterIndex);
    }

    public void SelectedCharacter()
    {
        characterInfo = GameObject.Find("GameController").GetComponent<CharacterInfo>();
        string character = gameObject.tag;
        characterInfo.Character = character;

        //duplicated code from OnSelect, keep both for now
        characterInfo = GameObject.Find("GameController").GetComponent<CharacterInfo>();
        int characterIndex = CharacterInfo.characters[gameObject.tag];
        spriteAnimator.SetInteger("characterSelectState", characterIndex);
    }
}
