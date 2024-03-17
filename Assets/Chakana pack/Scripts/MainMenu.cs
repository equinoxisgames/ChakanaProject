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

public class MainMenu : MonoBehaviour
{
    public RectTransform homeMenu;
    public RectTransform pauseMenu;
    public RectTransform exitMenu;
    public RectTransform confirmQuitMenu;

    public RectTransform inventoryMenu;
    private GameObject canvasUI;


    //public RectTransform exitMenu;
    //public Transform hoyustusGameObject;
    public Button btContinue;
    public TextMeshProUGUI txtButtonContinueNormal;
    public TextMeshProUGUI txtButtonContinueHighL;
    public Button btNewGame;
    public Button btCancelExitGame;
    public Button btExitGame;
    public Button btSettingsResolution;
    public Button btSettingsWindowMode;
    public Button btLoadSlot;
    public Slider sliderMaster;
    public Slider sliderMusic;
    public Slider sliderSFX;

    public TMP_Dropdown dropdownWindowMode;

    string escena;
    bool boolHomeMenuActive = true;

    public bool mouseMovido = false;
    public bool joystickIzquierdoMovido = false;

    private float joystickThreshold = 0.2f; // Umbral de sensibilidad del joystick
    private bool gamePadConectado = false;
    private bool botonGamePadPress = false;
    private bool anyKeyPress = false;
    private bool altKeyPress = false;

    [Header("LoadPanel")]
    [SerializeField] GameObject loadPanel;
    [SerializeField] Slider loadBar;


    [Header("Map Spot Player")]
    public GameObject mapSpotHoyustus;
    private Hoyustus player;

    int sceneLoad;

    protected int gold;
    protected float vida;
    protected float ataque;
    protected float ataqueMax;
    private float maxVida = 1000;
    float cargaHabilidadCondor;
    float cargaHabilidadSerpiente;
    float cargaHabilidadLanza;
    float cargaCuracion;
    private bool weaponEquip = false;
    private float valorAtaqueHabilidadCondor = 100;
    private float valorAtaqueHabilidadLanza = 150;

    int flagGameSaved = 0;

    

    float aumentoBarraSalto = 10;
    float aumentoBarraDash = 15;
    float aumentoBarraAtaque = 15;
    float danioExplosionCombinacionFuego_Veneno = 35;

    //public string tagObjetoAMover = "EtiquetaDelObjeto";
    //public Vector3 nuevaPosicion = new Vector3(100f, 100f, 0f);

    void Start()
    {
        btContinue.Select();

        flagGameSaved = PlayerPrefs.GetInt("GameSaved", 0);

        if (flagGameSaved == 1 && escena == "00- Main Menu 0")
        {
            txtButtonContinueNormal.text = "CONTINUE";
            txtButtonContinueHighL.text = "CONTINUE";

            btNewGame.gameObject.SetActive(true);
        }

        Time.timeScale = 1f;

        if (Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]))
        {
            joystickIzquierdoMovido = true;
            //Debug.Log("Start Gamepad conectado");
        }
        else
            mouseMovido = true;

        //mouseMovido = true;
        LocateMapScene();
        if (escena != "00- Main Menu 0")
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();

        canvasUI = GameObject.Find("HUDMenu");

        LoadSettings();
    }

    void OnGUI()
    {
        Event e = Event.current;

        if (e.isKey)
        {
            anyKeyPress = true;

            //Debug.Log("Se ha presionado una tecla en el teclado: " + e.keyCode);
        }
    }

    public void DisableUI(bool e)
    {
        canvasUI.SetActive(e);
    }

    void Update()
    {
        mouseMovido = true;

        if (Input.GetMouseButtonDown(0))
        {
            if (escena == "00- Main Menu 0")
            {
                if (homeMenu.gameObject.activeSelf)
                    btContinue.Select();
                else
                if (exitMenu.gameObject.activeSelf)
                    btExitGame.Select();
            }
            else
            {
                if (pauseMenu.gameObject.activeSelf)
                    btContinue.Select();
                else if (exitMenu.gameObject.activeSelf)
                    btExitGame.Select();
            }
        }
        //mouseMovido = true;
        //joystickIzquierdoMovido = true;


        //if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        //{
        //    altKeyPress = true;
        //    //Debug.Log("Se ha presionado la tecla Alt en el teclado.");
        //}
        //if (altKeyPress && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
        //{
        //    //Debug.Log("Se ha soltado la tecla Alt en el teclado.");
        //    altKeyPress = false;
        //}

        //if (anyKeyPress)
        //{
        //    mouseMovido = true;
        //    joystickIzquierdoMovido = false;
        //}

        //else

        //{
        //    mouseMovido = false;
        //    joystickIzquierdoMovido = true;
        //}


        // Validar movimiento del mouse
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            mouseMovido = true;
            joystickIzquierdoMovido = false;
            Cursor.visible = true;
        }

        //// Validar movimiento del joystick izquierdo del gamepad
        //float joystickX = Input.GetAxis("Horizontal");
        //float joystickY = Input.GetAxis("Vertical");
        //if (Mathf.Abs(joystickX) > joystickThreshold || Mathf.Abs(joystickY) > joystickThreshold)
        //{



        //}


        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 leftStick = gamepad.leftStick.ReadValue();

            if (leftStick != Vector2.zero && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) &&
                !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                mouseMovido = false;
                joystickIzquierdoMovido = true;

                Debug.Log("El joystick izquierdo del gamepad se ha movido exclusivamente.");
                // Realiza las acciones que desees cuando solo el joystick izquierdo se haya movido
            }
        }



        Escape();
        //Inventory();
        //LocateMapScene();

    }
    public void Inventory()
    {

        if (inventoryMenu.transform.GetChild(0).GetComponent<Button>() != null)
        {
            inventoryMenu.transform.GetChild(0).GetComponent<Button>().Select();
            StartCoroutine(ButtonSelect());
        }
        
    }

    IEnumerator ButtonSelect()
    {
        yield return new WaitForSeconds(0.5f);

        inventoryMenu.transform.GetChild(0).GetComponent<InventoryItem>().OnPress();
    }

    public void LocateMapScene()
    {
        escena = SceneManager.GetActiveScene().name;



        //Vector3 position = player.transform.position;

        if (escena != "00- Main Menu 0")
        {
            if (escena == "14-Boss Room")
            {
                UpdateMapSpotPosition(280f, -203f, 0f);
                return;
            }

            Vector3 position = new Vector3(0f, 0f, 0f);

            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                position = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>().position;
            }



            //Debug.Log("La posición de " + player.name + " es: " + position.ToString());

            switch (escena)
            {
                case "00- StartRoom 1":
                    UpdateMapSpotPosition(-121f, 80f, 0f);
                    //UpdateMapSpotPosition(-80f, 180f, 0f);
                    break;

                case "01-Level 1":
                    UpdateMapSpotPosition((position.x - 128f) * 1.35f, (position.y + 80f) * 1.15f, 0f);
                    break;

                case "03-Room 3":
                    UpdateMapSpotPosition(-200f, 125f, 0f);
                    break;

                case "04-Level 2":
                    UpdateMapSpotPosition((position.x - 275f), (position.y + 80f) * 1.3f, 0f);
                    //UpdateMapSpotPosition(-400f, 125f, 0f);
                    break;

                case "05-Room GA1":
                    UpdateMapSpotPosition(-500f, 78f, 0f);
                    break;

                case "06- Room 6":
                    UpdateMapSpotPosition((position.x - 75f) * 1.35f, (position.y + 80f) * 1.1f, 0f);
                    //UpdateMapSpotPosition(44f, 100f, 0f);
                    break;

                case "07-Room 7":
                    UpdateMapSpotPosition((position.x + 98f) * 1.35f, (position.y + 11f), 0f);
                    //UpdateMapSpotPosition(210f, 30f, 0f);
                    break;

                case "08-Room 8":
                    if (position.y < -16)
                        UpdateMapSpotPosition((position.x + 141f) * 1.35f, (position.y - 40f), 0f);
                    else
                        UpdateMapSpotPosition((position.x + 141f) * 1.35f, (position.y - 15f), 0f);

                    //UpdateMapSpotPosition(420f, -40f, 0f);
                    break;

                case "09-Room 9":
                    UpdateMapSpotPosition(230f, -110f, 0f);
                    break;

                case "10-Room 10 - 11":
                    //UpdateMapSpotPosition((position.x + 70f) * 1.35f, (position.y - 55f), 0f);

                    if (position.y < -56)
                        UpdateMapSpotPosition((position.x + 5f) * 1.35f, (position.y - 100f), 0f);
                    else
                        UpdateMapSpotPosition((position.x + 5f) * 1.35f, (position.y - 55f), 0f);

                    //UpdateMapSpotPosition(-30f, -155f, 0f);
                    break;
                case "12-Room 12":
                    UpdateMapSpotPosition(40f, -155f, 0f);
                    break;

                case "13- SaveRoom":
                    UpdateMapSpotPosition(217f, -203f, 0f);
                    break;

                case "13-Room 13":
                    
                    UpdateMapSpotPosition(518f, -50f, 0f);
                    break;

                case "14-Boss Room":
                    UpdateMapSpotPosition(280f, -203f, 0f);
                    Debug.Log("Entra al case de 14-Boss Room");
                    break;

                default:
                    UpdateMapSpotPosition(0f, 0f, 0f);
                    break;
            }
        }
    }


    public void UpdateMapSpotPosition(float x, float y, float z)
    {


        Vector3 nuevaPosicion = new Vector3(x, y, z);



        // Busca el objeto .
        GameObject objetoAMover = mapSpotHoyustus;

        //GameObject objetoAMover = GameObject.FindGameObjectWithTag("Spot").GetComponent<Transform>().gameObject;



        // Verifica si se encontró el objeto.
        if (objetoAMover != null)
        {
            // Asegúrate de que el objeto sea de tipo Image.
            Image imagen = objetoAMover.GetComponent<Image>();

            if (imagen != null)
            {
                // Cambia la posición de la imagen.
                imagen.rectTransform.localPosition = nuevaPosicion;
            }
            else
            {
                Debug.LogError("El objeto con la etiqueta no es una imagen de UI.");
            }
        }
        else
        {
            Debug.LogError("No se encontró ningún objeto con la etiqueta especificada.");
        }

    }

    public void Escape()
    {
        escena = SceneManager.GetActiveScene().name;
        if (Input.GetButtonDown("Cancel"))
        {
            if (escena != "00- Main Menu 0")
            {
                if (!pauseMenu.gameObject.activeSelf && !confirmQuitMenu.gameObject.activeSelf)
                {
                    pauseMenu.gameObject.SetActive(true);
                    btContinue.Select();
                    Time.timeScale = 0f;
                    DisableUI(false);
                    ActivePlayer(false);
                }
                else
                {
                    DisableUI(true);
                    Time.timeScale = 1f;
                    if (pauseMenu.gameObject.activeSelf)
                    {
                        pauseMenu.gameObject.SetActive(false);
                    }
                    if (confirmQuitMenu.gameObject.activeSelf)
                    {
                        confirmQuitMenu.gameObject.SetActive(false);
                    }
                    ActivePlayer(true);
                }

            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape) && mouseMovido && !joystickIzquierdoMovido)
                {


                    EscapeHomeMenu();

                    //Debug.Log("Se ha presionado la tecla Escape en el teclado.");

                }
                else
                {

                    Debug.Log("Se ha presionado el botón Escape (start) del gamepad porque mouseMovido es " + mouseMovido);
                }
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire4"))
            {
                if (escena != "00- Main Menu 0")
                {
                    Debug.Log("escena != 00 - Main Menu 0");
                }
                else
                {
                    if (!altKeyPress && joystickIzquierdoMovido)
                        EscapeHomeMenu();


                }
            }
            else
            {
                
            }


        }
    }
    public void EscapeHomeMenu()
    {

        if (boolHomeMenuActive)
        {
            homeMenu.gameObject.SetActive(false);
            boolHomeMenuActive = false;
            ActivateExitGame();

            btExitGame.Select();
        }
        else
        {
            homeMenu.gameObject.SetActive(true);
            boolHomeMenuActive = true;
            ActivateHomeMenu();

        }
    }
    public void ActivateSlot1()
    {

        btLoadSlot.Select();
    }
    public void ActivateStart()
    {

        btContinue.Select();
    }
    public void ActivateExitGame()
    {
        btExitGame.Select();

    }
    public void ActivateCancelExitGame()
    {
        btCancelExitGame.Select();

    }
    public void ActivateSettings()
    {
        
        sliderMaster.Select();
        

    }
    public void ActivateHomeMenu()
    {
        homeMenu.gameObject.SetActive(true);
        boolHomeMenuActive = true;
        btContinue.Select();

    }
    public void DeActivateHomeMenu()
    {
        homeMenu.gameObject.SetActive(false);
        boolHomeMenuActive = false;


    }
    public void Continue()
    {
        //hoyustusGameObject.gameObject.SetActive(true);
        //btYes.Select();
        //btContinue.Select();


    }
    public void PlayGame()
    {
        //PlayerPrefs.DeleteAll();
        DeletePLayerPrefs();

        loadPanel.SetActive(true);

        //if (!corutinaIniciada)
        //{
        StartCoroutine(LoadAsyncScene(2));
        //corutinaIniciada = true;
        //}
        //SceneManager.LoadScene("00- StartRoom 1");
    }

    IEnumerator LoadNextScene()
    {
        

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneLoad);

        while (asyncLoad.isDone == false)
        {
            yield return null;
        }
    }

    public void PlayAnimatedIntro()
    {
        //PlayerPrefs.DeleteAll();
        DeletePLayerPrefs();

        loadPanel.SetActive(true);

        if (flagGameSaved == 1)
        {
            PlayerPrefs.SetInt("scenePos", 0);
            Time.timeScale = 1;

            if (!PlayerPrefs.HasKey("respawn"))
            {
                //sceneLoad = 2;
                StartCoroutine(LoadAsyncScene(17));
            }
            else sceneLoad = PlayerPrefs.GetInt("respawn");

            StartCoroutine(LoadAsyncScene(sceneLoad));

            //StartCoroutine(LoadNextScene());
        }
        else
        {
            float valorMasterAudioKeyValue = PlayerPrefs.GetFloat("MasterAudioKeyValue", 100f);
            float valorMusicAudioKeyValue = PlayerPrefs.GetFloat("MusicAudioKeyValue", 100f);
            float valorSFXAudioKeyValue = PlayerPrefs.GetFloat("SFXAudioKeyValue", 100f);
            int valorFullScreenKeyValue = PlayerPrefs.GetInt("FullScreenKeyValue", 0);


            PlayerPrefs.DeleteAll();

            PlayerPrefs.SetFloat("MasterAudioKeyValue", valorMasterAudioKeyValue);
            PlayerPrefs.SetFloat("MusicAudioKeyValue", valorMusicAudioKeyValue);
            PlayerPrefs.SetFloat("SFXAudioKeyValue", valorSFXAudioKeyValue);
            PlayerPrefs.SetInt("FullScreenKeyValue", valorFullScreenKeyValue);

            StartCoroutine(LoadAsyncScene(17));
        }


        



        //if (!corutinaIniciada)
        //{
        //StartCoroutine(LoadAsyncScene(17));
        //corutinaIniciada = true;
        //}
        //SceneManager.LoadScene("00- StartRoom 1");
    }
    public void OpenNewGame()
    {
        //PlayerPrefs.DeleteAll();
        //DeletePLayerPrefs();

        float valorMasterAudioKeyValue = PlayerPrefs.GetFloat("MasterAudioKeyValue", 100f);
        float valorMusicAudioKeyValue = PlayerPrefs.GetFloat("MusicAudioKeyValue", 100f);
        float valorSFXAudioKeyValue = PlayerPrefs.GetFloat("SFXAudioKeyValue", 100f);
        int valorFullScreenKeyValue = PlayerPrefs.GetInt("FullScreenKeyValue", 0);
       

        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat("MasterAudioKeyValue", valorMasterAudioKeyValue);
        PlayerPrefs.SetFloat("MusicAudioKeyValue", valorMusicAudioKeyValue);
        PlayerPrefs.SetFloat("SFXAudioKeyValue", valorSFXAudioKeyValue);
        PlayerPrefs.SetInt("FullScreenKeyValue", valorFullScreenKeyValue);
        


        loadPanel.SetActive(true);

        


        //if (!corutinaIniciada)
        //{
        StartCoroutine(LoadAsyncScene(17));
        //corutinaIniciada = true;
        //}
        //SceneManager.LoadScene("00- StartRoom 1");
    }

    private void Awake()
    {
        escena = SceneManager.GetActiveScene().name;

        //DeletePLayerPrefs();

        if (escena != "00- Main Menu 0")
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();

        if (PlayerPrefs.HasKey("Boost01"))
        {
            valorAtaqueHabilidadCondor *= 1.25f;
            valorAtaqueHabilidadLanza *= 1.25f;
        }

        if (PlayerPrefs.HasKey("Boost02"))
        {
            maxVida *= 1.5f;

        }
        if (escena != "00- Main Menu 0")
            LoadData();
    }

    public static void SavePlayerData(float vida, int gold, float condor, float serpiente, float lanza, float curacion, float ataque)
    {
        //Simplificar la firma
        PlayerData data = new PlayerData(vida, gold, condor, serpiente, lanza, curacion, ataque);
        string dataPath = Application.persistentDataPath + "/player.save";
        FileStream fileStream = new FileStream(dataPath, FileMode.Create);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    private void LoadData()
    {
        escena = SceneManager.GetActiveScene().name;

        if (!PlayerPrefs.HasKey("iniciado"))
        {
            PlayerPrefs.SetInt("iniciado", 1);
            if (escena != "00- Main Menu 0")
                SaveManager.SavePlayerData(player.GetComponent<Hoyustus>());
        }

        PlayerData playerData = SaveManager.LoadPlayerData();
        if (playerData != null)
        {
            gold = playerData.getGold();
            ataque = playerData.getAtaque();
            vida = playerData.getVida();
            if (playerData.getVida() <= 0) vida = maxVida;
            cargaHabilidadCondor = playerData.getCondor();
            cargaHabilidadSerpiente = playerData.getSerpiente();
            cargaHabilidadLanza = playerData.getLanza();
            cargaCuracion = playerData.getCuracion();
        }
        else
        {
            SaveManager.SavePlayerData(player.GetComponent<Hoyustus>());
        }

        if (PlayerPrefs.HasKey("Boost03"))
        {
            ataqueMax *= 1.25f;
            ataque *= 1.25f;
        }

        if (PlayerPrefs.HasKey("WeaponEquip")) weaponEquip = true;
    }

    public void SavePlayerData()
    {
        SaveManager.SavePlayerData(vida, gold, cargaHabilidadCondor, cargaHabilidadSerpiente, cargaHabilidadLanza, cargaCuracion, ataque);
    }

    public void OpenMainMenu()
    {
        loadPanel.SetActive(true);

        //save data
        SavePlayerData();

        player.GetComponent<Hoyustus>().SavePlayerData();

        //SaveManager.SavePlayerData(player.GetComponent<Hoyustus>());

        //if (!corutinaIniciada)
        //{
        StartCoroutine(LoadAsyncScene(1));
        //corutinaIniciada = true;
        //}

        GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>().Stop();
    }

    IEnumerator LoadAsyncScene(int sceneIndex)
    {

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        //SceneManager.LoadScene(9 + 1);
        //yield return new WaitForSeconds(1f);
        //while(!asyncOperation.isDone)
        //{
        //    Debug.Log("Progres: "+asyncOperation.progress);
        //    loadBar.value = asyncOperation.progress+0.05f;
        //    yield return null;
        //}

        for (int i = 0; i < 20; i++)
        {
            //Debug.Log("Progres: "+asyncOperation.progress);
            //loadBar.value = asyncOperation.progress+0.05f;
            if (i < 15)
            {
                loadBar.value = i * 0.01f;
                yield return new WaitForSeconds(1f);
            }
            else
            {
                loadBar.value = i * 0.09f + 0.16f;
                yield return new WaitForSeconds(1.5f);
            }
        }

        //yield break;
    }
    public void OpenCredits()
    {
        loadPanel.SetActive(true);
        StartCoroutine(LoadAsyncScene(16));
        
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ActivePlayer(bool active)
    {
        DisableUI(active);
        player.enabled = active;
        if (active)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;
    }
    public void LoadSettings()
    {
        float masterAudioKeyValue = PlayerPrefs.GetFloat("MasterAudioKeyValue",100f);
        float musicAudioKeyValue = PlayerPrefs.GetFloat("MusicAudioKeyValue", 100f);
        float SFXAudioKeyValue = PlayerPrefs.GetFloat("SFXAudioKeyValue", 100f);
        int FullScreenKeyValue = PlayerPrefs.GetInt("FullScreenKeyValue", 0);
        

        sliderMaster.value = masterAudioKeyValue;
        sliderMusic.value = musicAudioKeyValue;
        sliderSFX.value = SFXAudioKeyValue;
        dropdownWindowMode.value = FullScreenKeyValue;

        btContinue.Select();
    }

    public void DeletePLayerPrefs()
    {

        float valorMasterAudioKeyValue = PlayerPrefs.GetFloat("MasterAudioKeyValue",100f);
        float valorMusicAudioKeyValue = PlayerPrefs.GetFloat("MusicAudioKeyValue", 100f);
        float valorSFXAudioKeyValue = PlayerPrefs.GetFloat("SFXAudioKeyValue", 100f);
        int valorFullScreenKeyValue = PlayerPrefs.GetInt("FullScreenKeyValue", 0);
        int valorRespawn = PlayerPrefs.GetInt("respawn",17);

        //PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat("MasterAudioKeyValue", valorMasterAudioKeyValue);
        PlayerPrefs.SetFloat("MusicAudioKeyValue", valorMusicAudioKeyValue);
        PlayerPrefs.SetFloat("SFXAudioKeyValue", valorSFXAudioKeyValue);
        PlayerPrefs.SetInt("FullScreenKeyValue", valorFullScreenKeyValue);
        PlayerPrefs.SetInt("respawn", valorRespawn);

    }

}