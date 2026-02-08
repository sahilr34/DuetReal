using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using UnityEngine.SceneManagement;
using System; // Action delegate के लिए

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Ad Settings")]
    public string androidGameId = "598283";
    public string iosGameId = "598283";
    public string androidInterstitialAdUnitId = "Interstitial_Android";
    public string iosInterstitialAdUnitId = "Interstitial_iOS";
    public string androidRewardedAdUnitId = "Rewarded_Android";
    public string iosRewardedAdUnitId = "Rewarded_iOS";
    public bool testMode = true;

    [Header("Ad Frequency")]
    [Tooltip("Chance to show ad on restart (0-1)")]
    [Range(0f, 1f)]
    public float adChanceOnRestart = 0.5f;
    [Tooltip("Minimum time between ads in seconds")]
    public float minTimeBetweenAds = 60f;

    public static AdManager Instance;

    private bool isInitialized = false;
    private bool isInterstitialAdLoaded = false;
    private bool isRewardedAdLoaded = false;
    private bool isAdShowing = false;

    private float lastAdTime = 0f;
    private bool shouldShowAdOnRestart = false;

    private Action restartCallback; // Restart callback store करें

    // Events
    public System.Action OnAdCompleted;
    public System.Action OnRewardEarned;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();

            // Scene change को track करें
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Scene load होने पर audio unpause करें
        AudioListener.pause = false;

        // अगर ad restart के लिए था, तो callback call करें
        if (shouldShowAdOnRestart && restartCallback != null)
        {
            restartCallback.Invoke();
            restartCallback = null;
        }
        shouldShowAdOnRestart = false;
    }

    // नया method restart के लिए
    public void RequestRestartWithAd(Action callback)
    {
        restartCallback = callback;

        float timeSinceLastAd = Time.unscaledTime - lastAdTime;

        // Check if enough time has passed since last ad
        if (timeSinceLastAd >= minTimeBetweenAds)
        {
            // Random chance to show ad
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            if (randomValue <= adChanceOnRestart)
            {
                shouldShowAdOnRestart = true;
                ShowInterstitialAd();
                return;
            }
        }

        // If no ad should be shown, complete immediately
        OnAdCompleted?.Invoke();
        shouldShowAdOnRestart = false;
        callback?.Invoke();
    }

    public void OnInitializationComplete()
    {
        Debug.Log("✅ Unity Ads initialized successfully");
        isInitialized = true;
        LoadAllAds();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"❌ Unity Ads Initialization Failed: {error} - {message}");
        StartCoroutine(RetryInitialization(2f));
    }

    private IEnumerator RetryInitialization(float delay)
    {
        yield return new WaitForSeconds(delay);
        InitializeAds();
    }

    private void InitializeAds()
    {
        if (!Advertisement.isInitialized && !isInitialized)
        {
            Debug.Log("Initializing Unity Ads...");

#if UNITY_IOS
            string gameId = iosGameId;
#elif UNITY_ANDROID
            string gameId = androidGameId;
#else
            string gameId = androidGameId;
#endif

            Advertisement.Initialize(gameId, testMode, this);
        }
        else if (Advertisement.isInitialized)
        {
            isInitialized = true;
            LoadAllAds();
        }
    }

    private void LoadAllAds()
    {
        LoadInterstitialAd();
        LoadRewardedAd();
    }

    private string GetInterstitialAdUnitId()
    {
#if UNITY_IOS
        return iosInterstitialAdUnitId;
#else
        return androidInterstitialAdUnitId;
#endif
    }

    private string GetRewardedAdUnitId()
    {
#if UNITY_IOS
        return iosRewardedAdUnitId;
#else
        return androidRewardedAdUnitId;
#endif
    }

    // ---------- Interstitial Ads ----------
    private void LoadInterstitialAd()
    {
        if (!isInitialized) return;

        string adUnitId = GetInterstitialAdUnitId();
        Debug.Log($"Loading Interstitial Ad: {adUnitId}");
        Advertisement.Load(adUnitId, this);
    }

    public void ShowInterstitialAd()
    {
        if (!isInitialized || !isInterstitialAdLoaded || isAdShowing)
        {
            // अगर ad नहीं show हो सका, तो callback call करें
            if (shouldShowAdOnRestart && restartCallback != null)
            {
                restartCallback.Invoke();
                restartCallback = null;
            }
            OnAdCompleted?.Invoke();
            return;
        }

        string adUnitId = GetInterstitialAdUnitId();
        isAdShowing = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Advertisement.Show(adUnitId, this);
    }

    // ---------- Rewarded Ads ----------
    private void LoadRewardedAd()
    {
        if (!isInitialized) return;

        string adUnitId = GetRewardedAdUnitId();
        Debug.Log($"Loading Rewarded Ad: {adUnitId}");
        Advertisement.Load(adUnitId, this);
    }

    public void ShowRewardedAd()
    {
        if (isRewardedAdLoaded && !isAdShowing)
        {
            string adUnitId = GetRewardedAdUnitId();
            isAdShowing = true;
            Advertisement.Show(adUnitId, this);
        }
        else
        {
            Debug.Log("Rewarded ad not ready yet.");
            LoadRewardedAd();
        }
    }

    // ---------- Common Unity Ads Callbacks ----------
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"✅ Ad Loaded: {adUnitId}");

        if (adUnitId == GetInterstitialAdUnitId()) isInterstitialAdLoaded = true;
        if (adUnitId == GetRewardedAdUnitId()) isRewardedAdLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"❌ Failed to load Ad: {adUnitId}, Error: {error} - {message}");

        if (adUnitId == GetInterstitialAdUnitId()) StartCoroutine(RetryLoadAd(5f, true));
        if (adUnitId == GetRewardedAdUnitId()) StartCoroutine(RetryLoadAd(5f, false));
    }

    private IEnumerator RetryLoadAd(float delay, bool isInterstitial)
    {
        yield return new WaitForSeconds(delay);
        if (isInterstitial) LoadInterstitialAd();
        else LoadRewardedAd();
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"❌ Show Failed: {adUnitId} - {error}: {message}");
        isAdShowing = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // If this was a restart ad that failed, proceed with restart
        if (shouldShowAdOnRestart && restartCallback != null)
        {
            restartCallback.Invoke();
            restartCallback = null;
        }
        shouldShowAdOnRestart = false;

        LoadAllAds();
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log($"▶️ Ad Started: {adUnitId}");
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log($"🔗 Ad Clicked: {adUnitId}");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"✅ Ad Completed: {adUnitId}, State: {showCompletionState}");

        // Handle rewarded ad completion
        if (adUnitId == GetRewardedAdUnitId() && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("🎁 Player earned reward!");
            OnRewardEarned?.Invoke();
        }
        // Handle interstitial ad completion
        else if (adUnitId == GetInterstitialAdUnitId())
        {
            lastAdTime = Time.unscaledTime; // Update last ad time
        }

        isAdShowing = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Restart callback handle करें (scene load होगा और OnSceneLoaded में callback call होगा)
        LoadAllAds();
    }
}