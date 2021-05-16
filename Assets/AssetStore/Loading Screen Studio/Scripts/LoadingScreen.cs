using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;

namespace Michsky.LSS
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreen : MonoBehaviour
    {
        private static LoadingScreen instance = null;

        public float titleSize = 50;
        public float descriptionSize = 28;
        public float hintSize = 32;
        public float statusSize = 24;
        public float pakSize = 35;
        public TMP_FontAsset titleFont;
        public TMP_FontAsset descriptionFont;
        public TMP_FontAsset hintFont;
        public TMP_FontAsset statusFont;
        public TMP_FontAsset pakFont;
        public Color titleColor = Color.white;
        public Color descriptionColor = Color.white;
        public Color hintColor = Color.white;
        public Color spinnerColor = Color.white;
        public Color statusColor = Color.white;
        public Color pakColor = Color.white;
        [TextArea] public string titleObjText = "Title";
        [TextArea] public string titleObjDescText = "Description";
        [TextArea] public string pakText = "Press {KEY} to continue";

        public CanvasGroup canvasGroup;
        public TextMeshProUGUI statusObj;
        public TextMeshProUGUI titleObj;
        public TextMeshProUGUI descriptionObj;
        public Slider progressBar;
        public Sprite backgroundImage;
        public Transform spinnerParent;
        public static string prefabName = "Basic";

        public TextMeshProUGUI hintsText;
        public bool changeHintWithTimer;
        public float hintTimerValue = 5;
        private float htvTimer;
        [TextArea] public List<string> hintList = new List<string>();

        public Image imageObject;
        public Animator fadingAnimator;
        public List<Sprite> ImageList = new List<Sprite>();
        public bool changeImageWithTimer;
        public float imageTimerValue = 5;
        private float itvTimer;
        [Range(0.1f, 5)] public float imageFadingSpeed = 1;

        public Animator objectAnimator;
        public TextMeshProUGUI pakTextObj;
        public TextMeshProUGUI pakCountdownLabel;
        public Slider pakCountdownSlider;
        public KeyCode keyCode = KeyCode.Space;
        public bool useSpecificKey = false;
        private bool enableFading = false;
        [Tooltip("Second(s)")] 
        [Range(1, 30)] public int pakCountdownTimer = 5;

        [Tooltip("Second(s)")] 
        public float virtualLoadingTimer = 5;
        private float vltTimer;

        public bool enableVirtualLoading = false;
        public bool enableTitle = true;
        public bool enableDescription = true;
        public bool enableStatusLabel = true;
        public bool enableRandomHints = true;
        public bool enableRandomImages = true;
        public bool enablePressAnyKey = true;
        [Tooltip("If fading-in doesn't work properly, you can try this option.")]
        public bool forceCanvasGroup = false;
        [Range(0.1f, 10)] public float fadingAnimationSpeed = 2.0f;

        public UnityEvent onBeginEvents;
        public UnityEvent onFinishEvents;

        public int spinnerHelper;
        public bool updateHelper = false;
        private bool onFinishInvoked = false;
        public static bool processLoading = false;
        private static bool fcgHelper = false;
        private static string sceneHelper;

        void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = gameObject.GetComponent<CanvasGroup>();

            if (forceCanvasGroup == true)
                fcgHelper = true;

            canvasGroup.alpha = 0f;
        }

        void Start()
        {
            if (enableRandomHints == true)
            {
                string hintChose = hintList[Random.Range(0, hintList.Count)];
                hintsText.text = hintChose;
            }

            if (enableRandomImages == true)
            {
                Sprite imageChose = ImageList[Random.Range(0, ImageList.Count)];
                imageObject.sprite = imageChose;
            }

            else
            {
                fadingAnimator.enabled = false;
                imageObject.sprite = backgroundImage;
            }

            if (enablePressAnyKey == true && pakCountdownSlider != null)
            {
                pakCountdownSlider.maxValue = pakCountdownTimer;
                pakCountdownSlider.value = pakCountdownTimer;
            }

            fadingAnimator.speed = imageFadingSpeed;
            statusObj.text = "0%";
            progressBar.value = 0;
        }

        private AsyncOperation loadingProcess;

        public static void LoadScene(string sceneName)
        {
            try
            {
                processLoading = true;
                instance = Instantiate(Resources.Load<GameObject>("Loading Screens/" + prefabName).GetComponent<LoadingScreen>());
                DontDestroyOnLoad(instance.gameObject);
                instance.gameObject.SetActive(true);

                if (fcgHelper == false)
                {
                    instance.loadingProcess = SceneManager.LoadSceneAsync(sceneName);
                    instance.loadingProcess.allowSceneActivation = false;
                }

                else if (fcgHelper == true)
                {
                    sceneHelper = sceneName;
                }

                instance.onBeginEvents.Invoke();
            }

            catch
            {
                Debug.LogError("<b><color=orange>[LSS]</color></b> Cannot initalize the loading screen because either <b><color=orange>'" +
                    sceneName + "'</color></b> scene has not been added to the build window, or <b><color=orange>'" + prefabName
                    + "'</color></b> prefab cannot be found in <b>Resources/Loading Screens</b>.");
                Destroy(instance.gameObject);
                processLoading = false;
            }
        }

        void Update()
        {
            if (processLoading == true && forceCanvasGroup == true)
            {
                canvasGroup.alpha += fadingAnimationSpeed * Time.deltaTime;

                if (canvasGroup.alpha >= 1)
                {
                    forceCanvasGroup = false;
                    instance.loadingProcess = SceneManager.LoadSceneAsync(sceneHelper);
                    instance.loadingProcess.allowSceneActivation = false;
                }
            }

            else if (processLoading == true && enableVirtualLoading == true && forceCanvasGroup == false)
            {
                progressBar.value += 1 / virtualLoadingTimer * Time.deltaTime;
                statusObj.text = Mathf.Round(progressBar.value * 100).ToString() + "%";
                vltTimer += Time.deltaTime;

                if (vltTimer >= virtualLoadingTimer)
                {
                    if (enablePressAnyKey == true)
                    {
                        loadingProcess.allowSceneActivation = true;

                        if (objectAnimator.GetCurrentAnimatorStateInfo(0).IsName("Start"))
                            objectAnimator.Play("Switch to PAK");

                        if (enableFading == true)
                            canvasGroup.alpha -= fadingAnimationSpeed * Time.deltaTime;

                        else
                        {
                            pakCountdownSlider.value -= 1 * Time.deltaTime;
                            pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                            if (pakCountdownSlider.value == 0)
                            {
                                if (onFinishInvoked == false)
                                {
                                    onFinishEvents.Invoke();
                                    onFinishInvoked = true;
                                }

                                enableFading = true;
                                StartCoroutine("DestroyLoadingScreen");
                                canvasGroup.interactable = false;
                                canvasGroup.blocksRaycasts = false;
                            }
                        }

                        if (enableFading == false && useSpecificKey == false && Input.anyKeyDown)
                        {
                            enableFading = true;
                            StartCoroutine("DestroyLoadingScreen");
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;

                            if (onFinishInvoked == false)
                            {
                                onFinishEvents.Invoke();
                                onFinishInvoked = true;
                            }
                        }

                        else if (enableFading == false && useSpecificKey == true && Input.GetKeyDown(keyCode))
                        {
                            enableFading = true;
                            StartCoroutine("DestroyLoadingScreen");
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;

                            if (onFinishInvoked == false)
                            {
                                onFinishEvents.Invoke();
                                onFinishInvoked = true;
                            }
                        }
                    }

                    else
                    {
                        loadingProcess.allowSceneActivation = true;
                        canvasGroup.alpha -= fadingAnimationSpeed * Time.deltaTime;

                        if (onFinishInvoked == false)
                        {
                            onFinishEvents.Invoke();
                            onFinishInvoked = true;
                        }

                        if (canvasGroup.alpha <= 0)
                            Destroy(gameObject);
                    }
                }

                else
                {
                    canvasGroup.alpha += fadingAnimationSpeed * Time.deltaTime;

                    if (canvasGroup.alpha >= 1)
                        loadingProcess.allowSceneActivation = true;
                }
            }

            else if (processLoading == true && enableVirtualLoading == false)
            {
                progressBar.value = loadingProcess.progress;
                statusObj.text = Mathf.Round(progressBar.value * 100).ToString() + "%";

                if (loadingProcess.isDone && enablePressAnyKey == false)
                {
                    canvasGroup.alpha -= fadingAnimationSpeed * Time.deltaTime;

                    if (canvasGroup.alpha <= 0)
                        Destroy(gameObject);

                    if (onFinishInvoked == false)
                    {
                        onFinishEvents.Invoke();
                        onFinishInvoked = true;
                    }
                }

                else if (!loadingProcess.isDone)
                {
                    canvasGroup.alpha += fadingAnimationSpeed * Time.deltaTime;

                    if (canvasGroup.alpha >= 1)
                        loadingProcess.allowSceneActivation = true;
                }

                if (loadingProcess.isDone && enablePressAnyKey == true)
                {
                    loadingProcess.allowSceneActivation = true;

                    if (onFinishInvoked == false)
                    {
                        onFinishEvents.Invoke();
                        onFinishInvoked = true;
                    }

                    if (fadingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wait"))
                        objectAnimator.CrossFade("Switch to PAK", 0.25f);

                    if (enableFading == true)
                        canvasGroup.alpha -= fadingAnimationSpeed * Time.deltaTime;

                    else
                    {
                        pakCountdownSlider.value -= Time.deltaTime;
                        pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                        if (pakCountdownSlider.value == 0)
                        {
                            enableFading = true;
                            StartCoroutine("DestroyLoadingScreen");
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;
                        }
                    }

                    if (enableFading == false && useSpecificKey == false && Input.anyKeyDown)
                    {
                        enableFading = true;
                        StartCoroutine("DestroyLoadingScreen");
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;
                    }

                    else if (enableFading == false && useSpecificKey == true && Input.GetKeyDown(keyCode))
                    {
                        enableFading = true;
                        StartCoroutine("DestroyLoadingScreen");
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;
                    }
                }

                if (enableRandomImages == true && changeImageWithTimer == true)
                {
                    itvTimer += Time.deltaTime;

                    if (itvTimer >= imageTimerValue && fadingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fade In"))
                        fadingAnimator.Play("Fade Out");

                    else if (itvTimer >= imageTimerValue && fadingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wait"))
                    {
                        Sprite cloneHelper = imageObject.sprite;
                        Sprite imageChose = ImageList[Random.Range(0, ImageList.Count)];

                        if (imageChose == cloneHelper)
                            imageChose = ImageList[Random.Range(0, ImageList.Count)];

                        imageObject.sprite = imageChose;
                        itvTimer = 0.0f;
                    }
                }

                else if (enableRandomImages == true && changeImageWithTimer == false)
                {
                    Sprite imageChose = ImageList[Random.Range(0, ImageList.Count)];
                    imageObject.sprite = imageChose;

                    if (imageObject.color != new Color32(255, 255, 255, 255))
                        imageObject.color = new Color32(255, 255, 255, 255);

                    fadingAnimator.enabled = false;
                    enableRandomImages = false;
                }

                if (enableRandomHints == true)
                {
                    htvTimer += Time.deltaTime;

                    if (htvTimer >= hintTimerValue)
                    {
                        string cloneHelper = hintsText.text;
                        string hintChose = hintList[Random.Range(0, hintList.Count)];

                        hintsText.text = hintChose;

                        if (hintChose == cloneHelper)
                            hintChose = hintList[Random.Range(0, hintList.Count)];

                        htvTimer = 0.0f;
                    }
                }
            }
        }

        IEnumerator DestroyLoadingScreen()
        {
            while (canvasGroup.alpha != 0)
                yield return new WaitForSeconds(0.5f);

            if (canvasGroup.alpha == 0)
                Destroy(gameObject);
        }
    }
}