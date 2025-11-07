using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingUI : PopupUIBase
{
    private const string PREF_BGM_VOLUME = "BgmVolume";
    private const string PREF_SFX_VOLUME = "SfxVolume";
    private const string PREF_BGM_MUTED = "BgmMuted";
    private const string PREF_SFX_MUTED = "SfxMuted";
    private const string PREF_FRAME_INDEX = "FrameRateIndex";

    [Header("오디오 믹서")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("아이콘 스프라이트")]
    [SerializeField] private Sprite bgmOnIcon;
    [SerializeField] private Sprite bgmOffIcon;
    [SerializeField] private Sprite sfxOnIcon;
    [SerializeField] private Sprite sfxOffIcon;
    [SerializeField] private Sprite hapticsOnIcon;      // 진동 마땅한 아이콘이 없습니다.
    [SerializeField] private Sprite hapticsOffIcon;

    [Header("배경음")]
    [SerializeField] private ButtonAnimator bgmMuteBtn;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Image bgmMuteIcon;
    private bool isBgmMuted = false;

    [Header("효과음")]
    [SerializeField] private ButtonAnimator sfxMuteBtn;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Image sfxMuteIcon;
    private bool isSfxMuted = false;

    [Header("진동")]
    [SerializeField] private ButtonAnimator hapticsToggleBtn;
    [SerializeField] private Image hapticsToggleIcon;
    private bool isHapticsOn = true;

    [Header("프레임 설정")]
    [SerializeField] private ButtonAnimator frameLeftBtn;
    [SerializeField] private ButtonAnimator frameRightBtn;
    [SerializeField] private TextMeshProUGUI frameText;
    private readonly int[] frameRates = { 30, 60 }; // 지원 프레임 목록
    private int currentFrameIndex = 1;

    //[Header("게임 언어")]
    //[SerializeField] private Button languageLeftBtn;
    //[SerializeField] private Button languageRightBtn;
    //[SerializeField] private TextMeshProUGUI languageText;
    //private readonly string[] languages = { "한국어", "English" };
    //private int currentLanguageIndex = 0; // 기본 '한국어'

    [Header("기타 버튼")]
    [SerializeField] private ButtonAnimator backButton;
    [SerializeField] private Button bgBackButton;

    [Header("버튼 사운드")]
    [SerializeField] private AudioClip onClickButtonSound;

    [Header("저장 로드 버튼")]
    [SerializeField] private ButtonAnimator saveButton;
    [SerializeField] private ButtonAnimator loadButton;

    protected override void OnOpen()
    {
        base.OnOpen();

        // UI가 열릴 때, 토글의 상태를 기본값으로 초기화합니다.
        InitializeSettings();

        // UI 리스너 연결
        //bgmMuteBtn.onClick.AddListener(ToggleBgmMute);
        //bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);

        //sfxMuteBtn.onClick.AddListener(ToggleSfxMute);
        //sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        //hapticsMuteBtn.onClick.AddListener(ToggleHapticsMute);
        //hapticsSlider.onValueChanged.AddListener(OnHapticsSliderChanged);

        //frameLeftBtn.onClick.AddListener(() => ChangeFrameRate(-1));
        //frameRightBtn.onClick.AddListener(() => ChangeFrameRate(1));

        //backButton.onClick.AddListener(OnBackButtonClicked);
        if (bgmMuteBtn != null) bgmMuteBtn.OnClickAnimationComplete.AddListener(ToggleBgmMute);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);

        if (sfxMuteBtn != null) sfxMuteBtn.OnClickAnimationComplete.AddListener(ToggleSfxMute);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        if (hapticsToggleBtn != null) hapticsToggleBtn.OnClickAnimationComplete.AddListener(ToggleHaptics);

        if (frameLeftBtn != null) frameLeftBtn.OnClickAnimationComplete.AddListener(() => ChangeFrameRate(-1));
        if (frameRightBtn != null) frameRightBtn.OnClickAnimationComplete.AddListener(() => ChangeFrameRate(1));

        if (backButton != null) backButton.OnClickAnimationComplete.AddListener(OnBackButtonClicked);
        if (bgBackButton != null) bgBackButton.onClick.AddListener(OnBackButtonClicked);

        if (saveButton != null) saveButton.OnClickAnimationComplete.AddListener(OnClickSaveButton);
        if (loadButton != null) loadButton.OnClickAnimationComplete.AddListener(OnClickLoadButton);
    }

    protected override void OnClose()
    {
        base.OnClose();

        // 리스너 제거
        //bgmMuteBtn.onClick.RemoveAllListeners();
        //bgmSlider.onValueChanged.RemoveAllListeners();

        //sfxMuteBtn.onClick.RemoveAllListeners();
        //sfxSlider.onValueChanged.RemoveAllListeners();

        //hapticsMuteBtn.onClick.RemoveAllListeners();
        //hapticsSlider.onValueChanged.RemoveAllListeners();

        //frameLeftBtn.onClick.RemoveAllListeners();
        //frameRightBtn.onClick.RemoveAllListeners();

        //backButton.onClick.RemoveAllListeners();

        if (bgmMuteBtn != null) bgmMuteBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveAllListeners();

        if (sfxMuteBtn != null) sfxMuteBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveAllListeners();

        if (hapticsToggleBtn != null) hapticsToggleBtn.OnClickAnimationComplete.RemoveAllListeners();

        if (frameLeftBtn != null) frameLeftBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (frameRightBtn != null) frameRightBtn.OnClickAnimationComplete.RemoveAllListeners();

        if (backButton != null) backButton.OnClickAnimationComplete.RemoveAllListeners();
    }

    private void InitializeSettings()
    {
        // BGM 볼륨 (1~10 정수, 기본값 5)
        int loadedBgmValue = PlayerPrefs.GetInt(PREF_BGM_VOLUME, 5);
        bgmSlider.value = loadedBgmValue;
        // BGM 음소거 (0이면 false, 1이면 true, 기본값 false)
        isBgmMuted = PlayerPrefs.GetInt(PREF_BGM_MUTED, 0) == 1;

        // SFX 위와 동일
        int loadedSfxValue = PlayerPrefs.GetInt(PREF_SFX_VOLUME, 5);
        sfxSlider.value = loadedSfxValue;
        isSfxMuted = PlayerPrefs.GetInt(PREF_SFX_MUTED, 0) == 1;

        // 프레임 인덱스 (기본값 1이 60프레임)
        currentFrameIndex = PlayerPrefs.GetInt(PREF_FRAME_INDEX, 1);

        if (currentFrameIndex < 0 || currentFrameIndex >= frameRates.Length)
            currentFrameIndex = 1;

        UpdateBgmVisuals();   // BGM 음소거 아이콘 업데이트
        ApplyBgmVolume();     // 실제 BGM 볼륨 적용
        UpdateSfxVisuals();   // SFX 음소거 아이콘 업데이트
        ApplySfxVolume();     // 실제 SFX 볼륨 적용
        //UpdateHapticsVisuals(); // 진동 아이콘 업데이트
        // 실제 진동 설정 적용 (구현 필요)
        UpdateFrameRateVisual();// 프레임 텍스트 업데이트 및 실제 프레임 적용
    }

    // ----- 배경음 -----
    private void ToggleBgmMute()
    {
        isBgmMuted = !isBgmMuted;   // 상태 반전
        PlayerPrefs.SetInt(PREF_BGM_MUTED, isBgmMuted ? 1 : 0); // 상태 저장
        UpdateBgmVisuals(); // 아이콘 업데이트
        ApplyBgmVolume();   // 실제 볼륨 적용
        SoundManager.Instance.PlaySFX(onClickButtonSound);
    }

    // 슬라이더 값 변경 메서드
    private void OnBgmSliderChanged(float value)
    {
        if (isBgmMuted)
        {
            isBgmMuted = false;
            PlayerPrefs.SetInt(PREF_BGM_MUTED, 0);
            UpdateBgmVisuals();
        }
        else if (value == 0 && !isBgmMuted)
        {
            isBgmMuted = true;
            PlayerPrefs.SetInt(PREF_BGM_MUTED, 1);
            UpdateBgmVisuals();
        }
        int sliderIntValue = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt(PREF_BGM_VOLUME, sliderIntValue); // 값 저장

        ApplyBgmVolume();   // 실제 볼륨 적용
    }

    // 아이콘 업데이트 메서드
    private void UpdateBgmVisuals()
    {
/*
        if(isTogglingMute)
        {
            bgmSlider.value = (bgmSlider.value > 0) ? 0 : 1;
            SoundManager.Instance.PlaySFX(onClickButtonSound);

        }

        bgmMuteIcon.sprite = bgmSlider.value > 0 ? bgmOnIcon : bgmOffIcon; // 슬라이더가 0보다 크면 아이콘 변경
        SoundManager.Instance.SetVolume(bgmSlider.value, SoundManager.VolumeType.Music);
        //float volume = bgmSlider.value > 0 ? Mathf.Lerp(-40f, 0f, bgmSlider.value / 10f ) : -80f;
        //mainMixer.SetFloat("BGMVolume", volume);
*/
        bgmMuteIcon.sprite = isBgmMuted ? bgmOffIcon : bgmOnIcon;
    }

    // 실제 볼륨 적용 로직 분리
    private void ApplyBgmVolume()
    {
        if (isBgmMuted)
        {
            SoundManager.Instance.SetVolume(0f, SoundManager.VolumeType.Music); // 음소거 시 0으로 설정
        }
        else
        {
            // 슬라이더 값(1~10)을 SoundManager 범위(0.0~1.0)로 변환
            float normalizedVolume = bgmSlider.value / 10f;
            SoundManager.Instance.SetVolume(normalizedVolume, SoundManager.VolumeType.Music);
        }
    }

    // ----- 효과음 -----
    private void ToggleSfxMute()
    {
        isSfxMuted = !isSfxMuted;
        PlayerPrefs.SetInt(PREF_SFX_MUTED, isSfxMuted ? 1 : 0);
        UpdateSfxVisuals();
        ApplySfxVolume();
        SoundManager.Instance.PlaySFX(onClickButtonSound);
    }
    private void OnSfxSliderChanged(float value)
    {
        if (isSfxMuted)
        {
            isSfxMuted = false;
            PlayerPrefs.SetInt(PREF_SFX_MUTED, 0);
            UpdateSfxVisuals();
        }
        else if (value == 0 && !isSfxMuted)
        {
            isSfxMuted = true;
            PlayerPrefs.SetInt(PREF_SFX_MUTED, 1);
            UpdateSfxVisuals();
        }
        int sliderIntValue = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt(PREF_SFX_VOLUME, sliderIntValue);
        ApplySfxVolume();
    }

    // 아이콘 업데이트
    private void UpdateSfxVisuals()
    {
/*
        if (isTogglingMute)
        {
            sfxSlider.value = (sfxSlider.value > 0) ? 0 : 1;
            SoundManager.Instance.PlaySFX(onClickButtonSound);

        }

        sfxMuteIcon.sprite = sfxSlider.value > 0 ? sfxOnIcon : sfxOffIcon; // 슬라이더가 0보다 크면 아이콘 변경
        SoundManager.Instance.SetVolume(sfxSlider.value, SoundManager.VolumeType.SFX);
        //float volume = sfxSlider.value > 0 ? Mathf.Lerp(-40f, 0f, sfxSlider.value / 10f) : -80f;
        //mainMixer.SetFloat("SFXVolume", volume);
*/
        sfxMuteIcon.sprite = isSfxMuted ? sfxOffIcon : sfxOnIcon;
    }

    // 실제 효과음 볼륨 적용
    private void ApplySfxVolume()
    {
        if (isSfxMuted)
        {
            SoundManager.Instance.SetVolume(0f, SoundManager.VolumeType.SFX);
        }
        else
        {
            float normalizedVolume = sfxSlider.value / 10f;
            SoundManager.Instance.SetVolume(normalizedVolume, SoundManager.VolumeType.SFX);
        }
    }

    // ----- 진동 -----
    // 진동 기능 메서드 (현재 진동 기능은 없습니다.)
    private void ToggleHaptics()
    {
        isHapticsOn = !isHapticsOn; // 상태 반전
        UpdateHapticsVisuals();
        // Todo: 햅틱 기능 구현
    }

    // UI에 업데이트
    private void UpdateHapticsVisuals()
    {
        if (hapticsToggleIcon != null)
            hapticsToggleIcon.sprite = isHapticsOn ? hapticsOnIcon : hapticsOffIcon;
    }

    // ----- 프레임 설정 -----
    private void ChangeFrameRate(int direction)
    {
        SoundManager.Instance.PlaySFX(onClickButtonSound);

        currentFrameIndex += direction;
        if (currentFrameIndex < 0) currentFrameIndex = frameRates.Length - 1;
        if (currentFrameIndex >= frameRates.Length) currentFrameIndex = 0;

        PlayerPrefs.SetInt(PREF_FRAME_INDEX, currentFrameIndex);

        UpdateFrameRateVisual();
    }

    private void UpdateFrameRateVisual()
    {
        int targetFrame = frameRates[currentFrameIndex];
        Application.targetFrameRate = targetFrame;
        frameText.text = targetFrame.ToString();
    }

    private async void OnClickSaveButton()
    {
        if(StageManager.Instance.state == StageState.Ready)
        {
            ConfirmCancelUI ui = await UIManager.Instance.OpenPopupUI<ConfirmCancelUI>();
            ui.Initialize("저장하기", "게임을 저장하시겠습니까?", GameManager.Instance.GameSave, null, "저장하기");
        }
    }

    private async void OnClickLoadButton()
    {
        if(StageManager.Instance.state == StageState.Ready)
        {
            ConfirmCancelUI ui = await UIManager.Instance.OpenPopupUI<ConfirmCancelUI>();
            ui.Initialize("불러오기", "저장된 게임을 불러오사겠습니까?", GameManager.Instance.GameLoad, null, "불러오기");
        }
    }

    // ----- 게임 언어 설정 -----
    //private void ChangeLanguage(int direction)
    //{
    //    currentLanguageIndex = (currentLanguageIndex + direction + languages.Length) % languages.Length;
    //    UpdateLanguageVisual();
    //}

    //private void UpdateLanguageVisual()
    //{
    //    string targetLanguage = languages[currentLanguageIndex];
    //    languageText.text = targetLanguage;
    //    // Todo: 실제 게임 언어 변경 시스템 호출
    //}

    // ----- 뒤로가기 버튼 기능 -----
    private void OnBackButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
}
