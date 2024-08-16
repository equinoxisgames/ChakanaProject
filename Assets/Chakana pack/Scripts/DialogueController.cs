using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Assets.FantasyInventory.Scripts.Interface.Elements;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DialogueController : MonoBehaviour
{
    public Button btContinue;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateContinueButton()
    {

        btContinue.Select();
    }
}
