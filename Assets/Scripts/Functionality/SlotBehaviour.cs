//using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using Unity.VisualScripting;
//using System.Numerics;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially


    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [SerializeField] private List<SlotImage> FrameAnimationimages;
    [SerializeField] private List<SlotImage> BlastsAnimationimages;
    [SerializeField] private List<SlotImage> WildTextAnimationimages;



    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;
    private Dictionary<int, string> y_string = new Dictionary<int, string>();
    [SerializeField]
    private PayoutCalculation PayCalculator;

    [Header("Line Button Texts")]
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;


    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;

    [SerializeField]
    private Button TBetPlus_Button;
    [SerializeField]
    private Button TBetMinus_Button;
    [SerializeField] private Button Turbo_Button;
    [SerializeField] private Button StopSpin_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] DeerSprites;
    [SerializeField]
    private Sprite[] ButterflySprites;
    [SerializeField]
    private Sprite[] JLetterSprites;
    [SerializeField]
    private Sprite[] ALetterSprites;
    [SerializeField]
    private Sprite[] QLetterSprites;
    [SerializeField]
    private Sprite[] BumbooTreeSprites;
    [SerializeField]
    private Sprite[] YellowPotSprites;
    [SerializeField]
    private Sprite[] KLetterSprites;
    [SerializeField]
    private Sprite[] X2Sprites;
    [SerializeField]
    private Sprite[] Wild_Sprite;
    [SerializeField]
    private Sprite[] SwanSprites;
    [SerializeField]
    private Sprite[] FreeSpinSprites;


    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private TMP_Text TotalLines_text;

    [SerializeField]
    private GameObject TotalWinGameObject;

    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab
    [SerializeField] Sprite[] TurboToggleSprites;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Tween BalanceTween;
    internal bool IsAutoSpin = false;
    internal bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 10;
    [SerializeField]
    private int IconSizeFactor = 288;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns
    private bool StopSpinToggle;
    private float SpinDelay = 0.2f;
    private bool IsTurboOn = false;
    internal bool WasAutoSpinOn;
    public List<SlotImage> ReelsFrameGameObject1;
    public List<SlotImage> ReelsHideGameObject1;

    public List<GameObject> PayoutLines;

    [SerializeField] private GameObject ReelFrameAnimParent;
    [SerializeField] private GameObject ReelBlastAnimParent;

    private bool GoldWildCompleted;
    [SerializeField] private List<GameObject> GoldWildEffect;
    [SerializeField] private GameObject FreeGameBottomPanel;
    [SerializeField] private List<int> SymbolsToCascade = new List<int>();
    [SerializeField] private List<RectTransform> slotPositions = new List<RectTransform>();
    private Dictionary<string, int> SymbolsToFillDictonary = new Dictionary<string, int>();

    [SerializeField] private GameObject GoldenFrame;
    [SerializeField] private List<Vector3> GoldenReelColumn_Pos;

    [SerializeField] private GameObject FullPandaAnim;
    [SerializeField] private GameObject FullPandaLastAnim;
    [SerializeField] private Image FullPandaImg;
    [SerializeField] private TMP_Text LineWin_Text;

    [Header("Rotate Panda Free Spin")]
    [SerializeField] private GameObject RotatePandaParent;
    [SerializeField] private GameObject RotatePandaPrefab;



    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (TBetPlus_Button) TBetPlus_Button.onClick.RemoveAllListeners();
        if (TBetPlus_Button) TBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });

        if (TBetMinus_Button) TBetMinus_Button.onClick.RemoveAllListeners();
        if (TBetMinus_Button) TBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { audioController.PlayButtonAudio(); StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        if (FreeGameBottomPanel) FreeGameBottomPanel.SetActive(false);

        tweenHeight = (12 * IconSizeFactor) - 280;
    }

    void TurboToggle()
    {
        audioController.PlayButtonAudio();
        if (IsTurboOn)
        {
            IsTurboOn = false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite = TurboToggleSprites[0];
            // Turbo_Button.image.color = new UnityEngine.Color(0.86f, 0.86f, 0.86f, 1);
        }
        else
        {
            IsTurboOn = true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
            // Turbo_Button.image.color = new UnityEngine.Color(1, 1, 1, 1);
        }
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    private void StopAutoSpin()
    {
        audioController.PlayButtonAudio();
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
        }
        WasAutoSpinOn = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            if (FreeGameBottomPanel) FreeGameBottomPanel.SetActive(true);

            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        Debug.Log($"Free Spins chnaces");
        int i = 0;
        while (i < spinchances)
        {
            uiManager.FreeSpins--;
            StartSlots();
            if (FSnum_text) FSnum_text.text = uiManager.FreeSpins.ToString();
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            i++;
        }

        uiManager.DayTheme.SetActive(true);
        uiManager.NightTheme.SetActive(false);
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        if (FreeGameBottomPanel) FreeGameBottomPanel.SetActive(false);
        if (WasAutoSpinOn)
        {
            AutoSpin();
        }
        else
        {
            ToggleButtonGrp(true);
        }
        IsFreeSpin = false;
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }

    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }


    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * Lines;

    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.bets.Count - 1; // Loop to the last bet
            }
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * Lines;
        uiManager.InitialiseUIData(SocketManager.initUIData.paylines);

    }

    #region InitialFunctions
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 11);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.000";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        if (TotalLines_text) TotalLines_text.text = "9";
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * Lines;
        //_bonusManager.PopulateWheel(SocketManager.bonusdata);
        CompareBalance();

        uiManager.InitialiseUIData(SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                for (int i = 0; i < JLetterSprites.Length; i++)
                {
                    animScript.textureArray.Add(JLetterSprites[i]);
                }
                animScript.AnimationSpeed = 13f;
                break;

            case 1:
                for (int i = 0; i < QLetterSprites.Length; i++)
                {
                    animScript.textureArray.Add(QLetterSprites[i]);
                }
                animScript.AnimationSpeed = 13f;
                break;

            case 2:
                for (int i = 0; i < KLetterSprites.Length; i++)
                {
                    animScript.textureArray.Add(KLetterSprites[i]);
                }
                animScript.AnimationSpeed = 13f;
                break;

            case 3:
                for (int i = 0; i < ALetterSprites.Length; i++)
                {
                    animScript.textureArray.Add(ALetterSprites[i]);
                }
                animScript.AnimationSpeed = 13f;
                break;

            case 4:
                for (int i = 0; i < YellowPotSprites.Length; i++)
                {
                    animScript.textureArray.Add(YellowPotSprites[i]);
                }
                animScript.AnimationSpeed = 20f;
                break;

            case 5:
                for (int i = 0; i < BumbooTreeSprites.Length; i++)
                {
                    animScript.textureArray.Add(BumbooTreeSprites[i]);
                }
                animScript.AnimationSpeed = 17f;
                break;

            case 6:
                for (int i = 0; i < DeerSprites.Length; i++)
                {
                    animScript.textureArray.Add(DeerSprites[i]);
                }
                animScript.AnimationSpeed = 18f;
                break;

            case 7:
                for (int i = 0; i < ButterflySprites.Length; i++)
                {
                    animScript.textureArray.Add(ButterflySprites[i]);
                }
                animScript.AnimationSpeed = 17f;
                break;

            case 8:
                for (int i = 0; i < SwanSprites.Length; i++)
                {
                    animScript.textureArray.Add(SwanSprites[i]);
                }
                animScript.AnimationSpeed = 18f;
                break;

            case 9:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = 22f;
                break;

            case 10:
                for (int i = 0; i < X2Sprites.Length; i++)
                {
                    animScript.textureArray.Add(X2Sprites[i]);
                }
                animScript.AnimationSpeed = 14f;
                break;

            case 11:
                for (int i = 0; i < FreeSpinSprites.Length; i++)
                {
                    animScript.textureArray.Add(FreeSpinSprites[i]);
                }
                animScript.AnimationSpeed = 26f;
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        FullPandaAnim.SetActive(false);
        FullPandaLastAnim.SetActive(false);
        SetFullPandaWildAnim();
        // DisableFrameHideLayout();
        PayCalculator.ResetLines();

        // if (TotalWinGameObject) TotalWinGameObject.GetComponent<ImageAnimation>().StopAnimation();

        tweenroutine = StartCoroutine(TweenRoutine());
    }
    private void ResetPayoutLines()
    {
        foreach (GameObject line in PayoutLines)
        {
            line.SetActive(false);
        }
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        ResetrotatedWildEffect();
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (TotalWin_text) TotalWin_text.text = "0.000";
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);
        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }
        if (IsTurboOn)
        {
            for (int i = 0; i < numberOfSlots; i++)
            {
                InitializeTweening(Slot_Transform[i]);
            }
        }
        else
        {
            for (int i = 0; i < numberOfSlots; i++)
            {
                InitializeTweening(Slot_Transform[i]);
                // yield return new WaitForSeconds(0.1f);
            }
        }



        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);
        for (int j = 0; j < SocketManager.FullResultData.matrix.Count; j++)
        {
            for (int i = 0; i < SocketManager.FullResultData.matrix[j].Count; i++)
            {
                if (int.TryParse(SocketManager.FullResultData.matrix[j][i], out int symbolId))
                {
                    //_resultImages[i].slotImages[j].sprite = _symbolSprites[symbolId];
                    if (Tempimages[i].slotImages[j]) Tempimages[i].slotImages[j].sprite = myImages[symbolId];
                    PopulateAnimationSprites(Tempimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), symbolId);
                }
            }
        }

        MoveGoldenReel(SocketManager.FullResultData.features.wild.column);

        if (IsTurboOn || IsFreeSpin)
        {
            Debug.Log("IS TURBO ON " + IsTurboOn);
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
        }
        StopSpinToggle = false;


        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();

        if (SocketManager.FullResultData.payload.winAmount > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.2f;
        }
        if (TotalWin_text) TotalWin_text.text = SocketManager.FullResultData.payload.winAmount.ToString("F3");
        BalanceTween?.Kill();
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        //######commented start
        if (SocketManager.FullResultData.features.wild.isColumnWild)
        {
            audioController.PlayWLAudio("wildwin");

            CheckPopups = true;
            FullPandaAnim.SetActive(true);
            ImageAnimation script = FullPandaAnim.GetComponent<ImageAnimation>();
            yield return new WaitUntil(() => script.currentAnimationState == ImageAnimation.ImageState.NONE);
            SetFullPandaWildAnim();
            FullPandaLastAnim.SetActive(true);
            yield return new WaitForSeconds(0.75f);
            CheckPopups = false;
        }
        yield return new WaitUntil(() => !CheckPopups);
        if (IsFreeSpin)
        {
            if (SocketManager.FullResultData.features.wild.positions.Count > 0)
            {
                yield return StartCoroutine(RotatedWildEffect());
            }
        }
        yield return new WaitUntil(() => !CheckPopups);
        if (SocketManager.FullResultData.features.isCascade)
        {
            CheckPopups = true;
            StartCoroutine(CascadeRoutine());
        }
        yield return new WaitUntil(() => !CheckPopups);
        currentBalance = SocketManager.playerdata.Balance;
        yield return new WaitUntil(() => !CheckPopups);

        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            // yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }

        if (SocketManager.FullResultData.features.freeSpin.isTriggered)
        {
            if (IsFreeSpin)
            {
                IsFreeSpin = false;
                if (FreeSpinRoutine != null)
                {
                    StopCoroutine(FreeSpinRoutine);
                    FreeSpinRoutine = null;
                }
                yield return new WaitForSeconds(0.25f);
            }
            StartCoroutine(uiManager.FreeSpinProcess(SocketManager.FullResultData.features.freeSpin.freeSpinCount));
            if (IsAutoSpin)
            {
                WasAutoSpinOn = true;
                StopAutoSpin();
                yield return new WaitForSeconds(0.1f);
            }
        }
        // UpdateBalanceandWin();
    }



    public void MoveGoldenReel(int index)
    {
        Vector3 pos = GoldenReelColumn_Pos[index];
        GoldenFrame.transform.DOLocalMove(pos, 1f)
                  .SetEase(Ease.OutBack);
    }
    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        BalanceTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("F3");
        });
    }
    private void UpdateBalanceandWin()
    {
        if (TotalWin_text) TotalWin_text.text = SocketManager.FullResultData.payload.winAmount.ToString("F3");
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        currentBalance = SocketManager.playerdata.Balance;
    }

    internal void CheckWinPopups()
    {
        double WinAmonut = SocketManager.FullResultData.features.totalCascadeWin;
        Debug.Log($"#### checkwinpopup {WinAmonut} {currentTotalBet}");

        if (WinAmonut >= currentTotalBet * 5 && WinAmonut < currentTotalBet * 10)
        {
            uiManager.PopulateWin(1, WinAmonut);
            Debug.Log($"#### checkwinpopup {WinAmonut} {currentTotalBet}");

        }
        else if (WinAmonut >= currentTotalBet * 10 && WinAmonut < currentTotalBet * 15)
        {
            uiManager.PopulateWin(2, WinAmonut);
            Debug.Log($"#### checkwinpopup {WinAmonut} {currentTotalBet}");

        }
        else if (WinAmonut >= currentTotalBet * 15)
        {
            uiManager.PopulateWin(3, WinAmonut);
            Debug.Log($"#### checkwinpopup {WinAmonut} {currentTotalBet}");

        }
        else
        {
            CheckPopups = false;
        }
    }

    private IEnumerator RotatedWildEffect()
    {
        if (SocketManager.FullResultData.features.wild.positions.Count > 0)
        {
            ResetrotatedWildEffect();
            CheckPopups = true;
            RotatePandaParent.SetActive(true);
            var symToWildList = SocketManager.FullResultData.features.wild.positions;

            for (int i = 0; i < symToWildList.Count; i++)
            {
                List<int> position = symToWildList[i];
                Debug.Log("SymToWild Position: " + position[0] + ", " + position[1]);
                WildTextAnimationimages[position[1]].slotImages[position[0]].gameObject.SetActive(true);
                //  Vector2 PandaPosition = new Vector2(0, -290 + (290 * position[0]));
                Vector2 PandaPosition = new Vector2(0, 290 - (290 * position[0]));

                // GameObject Go = Instantiate(RotatePandaPrefab, Vector2.zero, Quaternion.identity, RotatePandaParent.transform);
                GameObject Go = Instantiate(RotatePandaPrefab, RotatePandaParent.transform);

                Go.transform.localPosition = PandaPosition;
                Go.transform.localRotation = Quaternion.identity; // reset inside parent space


                yield return new WaitForSeconds(0.4f);

                if (i == symToWildList.Count - 1)
                {

                    yield return new WaitForSeconds(3f);
                    CheckPopups = false;
                    Debug.Log("SymToWild Position: " + position[0] + ", " + position[1] + " Checkpoup False");
                }
            }
        }
    }
    private void ResetrotatedWildEffect()
    {
        for (int i = 0; i < WildTextAnimationimages.Count; i++)
        {
            for (int j = 0; j < WildTextAnimationimages[i].slotImages.Count; j++)
            {
                WildTextAnimationimages[i].slotImages[j].gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator CascadeRoutine()
    {
        while (SocketManager.FullResultData.features.isCascade)
        {
            ResetrotatedWildEffect();
            yield return StartCoroutine(Cascade());

            // Optional: small delay to avoid tight loop
            yield return null;
        }
        // CheckWinPopups();
        // Debug.Log($"#### check popup false 0");

        // yield return new WaitUntil(() => !CheckPopups);
        Debug.Log($"#### check popup false 1");
        CheckPopups = false;

    }

    private IEnumerator Cascade()
    {
        if (SocketManager.FullResultData.payload.lineWins.Count > 0)
        {
            int count = 0;

            foreach (var winline in SocketManager.FullResultData.payload.lineWins)
            {

                StartCoroutine(CheckPayoutLineBackend(SocketManager.initialData.lines[winline.lineIndex], winline.lineIndex, count));
                LineWin_Text.text = winline.win.ToString("f3");
                if (IsTurboOn || IsFreeSpin)
                {
                    yield return new WaitForSeconds(1.5f);
                }
                else
                {
                    yield return new WaitForSeconds(3f);
                }
                count++;

            }

            SocketManager.AccumulateResult(BetCounter);
            yield return new WaitUntil(() => SocketManager.isResultdone);
            foreach (int a in SymbolsToCascade)
            {
                // string key = symbolData[0].ToString() + symbolData[1].ToString(); // e.g., "00", "12"
                // int value = symbolData[2];
                int col = a / 10;
                int row = a % 10;

                string key = row.ToString() + col.ToString(); // "rowcol"
                int value = int.Parse(SocketManager.FullResultData.matrix[row][col]);
                SymbolsToFillDictonary[key] = value;
            }

            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(EleminateSymbols());
            if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
            if (TotalWin_text) TotalWin_text.text = SocketManager.FullResultData.features.totalCascadeWin.ToString("F3");

            yield return new WaitForSeconds(1f);
            yield return null;
        }
    }


    public IEnumerator PerformCascadingFeatures(Cascade cascade)
    {
        CheckPopups = true;

        foreach (var WinningLine in cascade.winnings)
        {
            // StartCoroutine(CheckPayoutLineBackend(WinningLine.line));
            LineWin_Text.text = WinningLine.win.ToString("f3");
            if (IsTurboOn || IsFreeSpin)
            {
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

        }
        foreach (var symbolData in cascade.symbolsToFill)
        {
            if (symbolData.Count >= 3)
            {
                // Assuming symbolData = [col, row, symbolValue]
                string key = symbolData[0].ToString() + symbolData[1].ToString(); // e.g., "00", "12"
                int value = symbolData[2];

                SymbolsToFillDictonary[key] = value;
            }
        }
        yield return StartCoroutine(EleminateSymbols());

        yield return new WaitForSeconds(1f);
        yield return null;
        CheckPopups = false;
    }

    //OLD code
    // public IEnumerator PerformCascadingFeatures(Cascade cascade)
    // {
    //     CheckPopups = true;

    //     foreach (var WinningLine in cascade.winnings)
    //     {
    //         StartCoroutine(CheckPayoutLineBackend(WinningLine.line, WinningLine.symbolsToEmit));
    //         LineWin_Text.text = WinningLine.win.ToString("f3");
    //         if (IsTurboOn || IsFreeSpin)
    //         {
    //             yield return new WaitForSeconds(1.5f);
    //         }
    //         else
    //         {
    //             yield return new WaitForSeconds(3f);
    //         }

    //     }
    //     foreach (var symbolData in cascade.symbolsToFill)
    //     {
    //         if (symbolData.Count >= 3)
    //         {
    //             // Assuming symbolData = [col, row, symbolValue]
    //             string key = symbolData[0].ToString() + symbolData[1].ToString(); // e.g., "00", "12"
    //             int value = symbolData[2];

    //             SymbolsToFillDictonary[key] = value;
    //         }
    //     }
    //     yield return StartCoroutine(EleminateSymbols());

    //     yield return new WaitForSeconds(1f);
    //     yield return null;
    //     CheckPopups = false;
    // }



    //New Code
    // private IEnumerator CheckPayoutLineBackend(List<int> LineId)
    // {
    //     List<int> y_points = null;
    //     List<int> points_anim = new List<int>();

    //     if (LineId.Count > 0)
    //     {
    //         for (int i = 0; i < LineId.Count; i++)
    //         {
    //             // y_string usage still there, if needed
    //             y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();

    //             // 🔥 New: directly extract row & col from SocketManager
    //             for (int k = 0; k < SocketManager.FullResultData.payload.lineWins[i].positions.Count; k++)
    //             {
    //                 int columnIndex = SocketManager.FullResultData.payload.lineWins[i].positions[k];
    //                 int rowIndex = SocketManager.initialData.lines[LineId[i]][columnIndex];

    //                 // Encode back into a single int so old logic works
    //                 // int encoded = rowIndex * 10 + columnIndex;
    //                 int encoded = columnIndex * 10 + rowIndex;

    //                 Debug.Log($"@@@@@ Encoded symbol position: {encoded} (Row: {rowIndex}, Col: {columnIndex})");
    //                 points_anim.Add(encoded);
    //                 if (!SymbolsToCascade.Contains(encoded))
    //                     SymbolsToCascade.Add(encoded);
    //             }
    //         }

    //         // payout lines setup
    //         PayCalculator.GeneratePayoutLinesBackend(y_points);
    //         if (LineId.Count > 2)
    //         {
    //             LineWin_Text.gameObject.transform.localPosition = new Vector2(0, (288 + LineId[2] * -288));
    //         }
    //         else if (LineId.Count > 0)
    //         {
    //             // fallback to first line
    //             LineWin_Text.gameObject.transform.localPosition = new Vector2(0, (288 + LineId[0] * -288));
    //         }
    //         else
    //         {
    //             // no line at all, maybe hide or default position
    //             LineWin_Text.gameObject.transform.localPosition = Vector2.zero;
    //         }
    //         // LineWin_Text.gameObject.transform.localPosition = new Vector2(0, (288 + LineId[2] * -288));
    //         LineWin_Text.gameObject.SetActive(true);

    //         // 🔥 No string parsing needed anymore — we already have points_anim filled.

    //         ReelFrameAnimParent.SetActive(true);
    //         for (int k = 0; k < points_anim.Count; k++)
    //         {
    //             int point = points_anim[k];
    //             Debug.Log(" ########point value " + point);
    //             if (point >= 10)
    //             {
    //                 //Debug.Log(" ########point value " + point);
    //                 FrameAnimationimages[(point / 10) % 10].slotImages[point % 10].gameObject.SetActive(true);
    //                 StartGameAnimation(Tempimages[(point / 10) % 10].slotImages[point % 10].gameObject);
    //             }
    //             else
    //             {
    //                 FrameAnimationimages[0].slotImages[point].gameObject.SetActive(true);
    //                 StartGameAnimation(Tempimages[0].slotImages[point].gameObject);
    //             }
    //         }

    //         if (IsTurboOn || IsFreeSpin)
    //             yield return new WaitForSeconds(0.75f);
    //         else
    //             yield return new WaitForSeconds(1.5f);

    //         for (int k = 0; k < points_anim.Count; k++)
    //         {
    //             int point = points_anim[k];

    //             if (point >= 10)
    //             {
    //                 FrameAnimationimages[(point / 10) % 10].slotImages[point % 10].gameObject.SetActive(false);
    //             }
    //             else
    //             {
    //                 FrameAnimationimages[0].slotImages[point].gameObject.SetActive(false);
    //             }
    //         }

    //         LineWin_Text.gameObject.SetActive(false);
    //         PayCalculator.ResetLines();
    //         StopGameAnimation();
    //         points_anim.Clear();
    //         yield return null;
    //     }
    //     else
    //     {
    //         if (audioController) audioController.StopWLAaudio();
    //     }

    //     CheckSpinAudio = false;
    // }

    private IEnumerator CheckPayoutLineBackend(List<int> LineId, int lineIndex, int LineCount)
    {
        List<int> y_points = null;
        List<int> points_anim = new List<int>();
        if (LineId.Count > 0)
        {
            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();


            }
            PayCalculator.GeneratePayoutLinesBackend(LineId);
            LineWin_Text.gameObject.transform.localPosition = new Vector2(0, (288 + LineId[2] * -288));
            LineWin_Text.gameObject.SetActive(true);
            for (int k = 0; k < SocketManager.FullResultData.payload.lineWins[LineCount].positions.Count; k++)
            {
                int columnIndex = SocketManager.FullResultData.payload.lineWins[LineCount].positions[k];
                int rowIndex = SocketManager.initialData.lines[lineIndex][columnIndex];
                Debug.Log("#######WIN LIne is " + lineIndex + " K Value is" + k + "    Row Index: " + rowIndex + " Column Index: " + columnIndex);

                // Encode back into a single int so old logic works
                // int encoded = rowIndex * 10 + columnIndex;
                int encoded = columnIndex * 10 + rowIndex;

                Debug.Log($"@@@@@ Encoded symbol position: {encoded} (Row: {rowIndex}, Col: {columnIndex})");
                points_anim.Add(encoded);
                if (!SymbolsToCascade.Contains(encoded))
                    SymbolsToCascade.Add(encoded);
            }
            ReelFrameAnimParent.SetActive(true);
            for (int k = 0; k < points_anim.Count; k++)
            {
                if (points_anim[k] >= 10)
                {
                    FrameAnimationimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject.SetActive(true);
                    int rowIndex = points_anim[k] % 10;
                    int columnIndex = (points_anim[k] / 10) % 10;
                    if (Tempimages[columnIndex].slotImages[rowIndex].sprite == myImages[9])
                    {
                        Tempimages[columnIndex].slotImages[rowIndex].gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
                    }

                    StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);

                }
                else
                {
                    FrameAnimationimages[0].slotImages[points_anim[k]].gameObject.SetActive(true);

                    if (Tempimages[0].slotImages[points_anim[k]].sprite == myImages[9])
                    {
                        Tempimages[0].slotImages[points_anim[k]].gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
                    }

                    StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                }  // This is for Frame Animation and Symbols Emited

            }
            if (IsTurboOn || IsFreeSpin)
            {
                yield return new WaitForSeconds(0.75f);
            }
            else
            {
                yield return new WaitForSeconds(1.5f);
            }
            for (int k = 0; k < points_anim.Count; k++)
            {
                if (points_anim[k] >= 10)
                {
                    FrameAnimationimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject.SetActive(false);
                    // Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    FrameAnimationimages[0].slotImages[points_anim[k]].gameObject.SetActive(false);
                    //  Tempimages[0].slotImages[points_anim[k]].transform.localScale = new Vector3(1f, 1f, 1f);
                }  // This is for To stop the Frame aniamiton for temp , we will add animatio of frame after getting 

            }
            LineWin_Text.gameObject.SetActive(false);
            PayCalculator.ResetLines();
            StopGameAnimation();
            points_anim.Clear();
            yield return null;

        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }
        CheckSpinAudio = false;
    }


    public IEnumerator EleminateSymbols()
    {
        foreach (int a in SymbolsToCascade)
        {
            if (a >= 10)
            {
                StartGameAnimation(Tempimages[(a / 10) % 10].slotImages[a % 10].gameObject);
                Tempimages[(a / 10) % 10].slotImages[a % 10].rectTransform
                 .DOAnchorPosX(Tempimages[(a / 10) % 10].slotImages[a % 10].rectTransform.anchoredPosition.x + 10f, 0.1f)
                 .SetLoops(4, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine);
            }
            else
            {
                StartGameAnimation(Tempimages[0].slotImages[a].gameObject);
                Tempimages[0].slotImages[a].rectTransform
                 .DOAnchorPosX(Tempimages[0].slotImages[a].rectTransform.anchoredPosition.x + 10f, 0.1f)
                 .SetLoops(4, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine);
            }  // This is for Frame Animation and Symbols Emited    
        }
        if (IsTurboOn || IsFreeSpin)
        {
            yield return new WaitForSeconds(0.75f);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }
        ReelBlastAnimParent.SetActive(true);
        foreach (int a in SymbolsToCascade)
        {
            if (a >= 10)
            {
                BlastsAnimationimages[(a / 10) % 10].slotImages[a % 10].gameObject.SetActive(true);
                //  yield return new WaitUntil(() => !BlastsAnimationimages[(a / 10) % 10].slotImages[a % 10].gameObject.activeInHierarchy);
                //yield return new WaitForSeconds(1f);
                Tempimages[(a / 10) % 10].slotImages[a % 10].gameObject.SetActive(false);
            }
            else
            {
                BlastsAnimationimages[0].slotImages[a].gameObject.SetActive(true);
                //  yield return new WaitUntil(() => !BlastsAnimationimages[0].slotImages[a].gameObject.activeInHierarchy);
                // yield return new WaitForSeconds(1f);
                Tempimages[0].slotImages[a].gameObject.SetActive(false);
            }
            //
        }
        audioController.PlayWLAudio("blast");

        yield return new WaitForSeconds(0.5f);

        ReelBlastAnimParent.SetActive(false);
        yield return StartCoroutine(ShiftSymbolsDownWithDOTween());
        if (IsFreeSpin)
        {
            if (SocketManager.FullResultData.features.wild.positions.Count > 0)
            {
                yield return StartCoroutine(RotatedWildEffect());
            }
        }
        SymbolsToCascade.Clear();
    }

    private IEnumerator ShiftSymbolsDownWithDOTween()
    {
        int columnCount = Tempimages.Count;
        int rowCount = Tempimages[0].slotImages.Count;

        for (int col = 0; col < columnCount; col++)
        {
            // Step 1: Mark eliminated rows
            bool[] isEliminated = new bool[rowCount];
            foreach (int a in SymbolsToCascade)
            {
                int c = (a / 10) % 10;
                int r = a % 10;
                if (c == col)
                    isEliminated[r] = true;
            }

            if (!isEliminated.Contains(true))
                continue;

            // Step 2: Collect non-eliminated symbols (top to bottom)
            List<Image> survivors = new List<Image>();
            for (int r = 0; r < rowCount; r++)
            {
                if (!isEliminated[r])
                {
                    survivors.Add(Tempimages[col].slotImages[r]);
                    Debug.Log($" non eliminated symbol : col " + col + "   Row :" + r);
                }

            }

            // Step 3: Build new column (bottom to top)
            Image[] newColumn = new Image[rowCount];
            int survivorIndex = survivors.Count - 1;

            // Animate survivors downward
            for (int r = rowCount - 1; r >= 0; r--)
            {
                Vector3 targetPos = Tempimages[col].slotImages[r].rectTransform.localPosition;

                if (survivorIndex >= 0)
                {
                    Image img = survivors[survivorIndex--];
                    img.rectTransform.DOLocalMove(targetPos, 0.3f).SetEase(Ease.InOutQuad);
                    newColumn[r] = img;
                    Debug.Log($" non eliminated symbol  downwordd : col " + col + "   Row :" + r);
                    Tempimages[col].slotImages[r] = img;

                }
            }

            // Step 4: Spawn new symbols at top and animate them down
            for (int r = 0; r < rowCount; r++)
            {
                if (newColumn[r] == null)
                {
                    // Create new symbol
                    Image newImg = Instantiate(Tempimages[col].slotImages[0], Tempimages[col].slotImages[0].transform.parent);
                    newImg.gameObject.SetActive(true);

                    // Position it above the top
                    Vector3 targetPos = Tempimages[col].slotImages[r].rectTransform.localPosition;
                    Vector3 startPos = targetPos + new Vector3(0, 300f, 0);
                    newImg.rectTransform.localPosition = startPos;

                    string rowcolPosition = r.ToString() + col.ToString();
                    SetNewSymbolSpritesAnim(newImg, rowcolPosition);
                    // Animate down simultaneously
                    newImg.rectTransform.DOLocalMove(targetPos, 0.3f).SetEase(Ease.OutBack);

                    newColumn[r] = newImg;
                    Debug.Log($"  eliminated symbol  downwordd : col " + col + "   Row :" + r);
                    //Destroy(Tempimages[col].slotImages[r].gameObject);
                    Tempimages[col].slotImages[r] = newImg;
                }
            }

            // Wait for all animations to finish
            //  yield return new WaitForSeconds(0.35f);
            yield return null;
            SetSymbolValues();

            // Step 5: Update the column in Tempimages
            for (int r = 0; r < rowCount; r++)
            {
                Tempimages[col].slotImages[r] = newColumn[r];
            }

            Debug.Log($"[Column {col}] cascade complete.");
        }

        SymbolsToCascade.Clear();

    }


    public void SetNewSymbolSpritesAnim(Image symbol, string RowCol)
    {

        ImageAnimation script = symbol.transform.gameObject.GetComponent<ImageAnimation>();
        Image img = symbol.transform.gameObject.GetComponent<Image>();
        Debug.Log($"Row Col :" + RowCol);
        if (SymbolsToFillDictonary.ContainsKey(RowCol))
        {
            Debug.Log($"Row Col : 1   " + RowCol);
            int value = SymbolsToFillDictonary[RowCol];
            img.sprite = myImages[value];
            PopulateAnimationSprites(script, value);
            Debug.Log(" Set New Symbol value : " + value);
        }


    }

    public void SetSymbolValues()
    {
        Debug.Log($"Setting symbol values from result matrix...");
        for (int j = 0; j < SocketManager.FullResultData.matrix.Count; j++)
        {
            for (int i = 0; i < SocketManager.FullResultData.matrix[j].Count; i++)
            {
                if (int.TryParse(SocketManager.FullResultData.matrix[j][i], out int symbolId))
                {
                    Tempimages[i].slotImages[j].gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                    //_resultImages[i].slotImages[j].sprite = _symbolSprites[symbolId];
                    if (Tempimages[i].slotImages[j]) Tempimages[i].slotImages[j].sprite = myImages[symbolId];
                    PopulateAnimationSprites(Tempimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), symbolId);
                }
            }
        }
    }

    private void SetFullPandaWildAnim()
    {
        Color c = FullPandaImg.color;
        if (FullPandaAnim.gameObject.activeInHierarchy)
        {

            c.a = 0f;
            FullPandaImg.color = c;
        }
        else
        {
            c.a = 255f;
            FullPandaImg.color = c;
        }
    }

    private void DisableFrameHideLayout()
    {
        foreach (SlotImage slotImage in ReelsHideGameObject1)
        {
            foreach (Image img in slotImage.slotImages)
            {
                img.gameObject.SetActive(false);
            }
        }
        foreach (SlotImage slotImage in ReelsFrameGameObject1)
        {
            foreach (Image img in slotImage.slotImages)
            {
                img.gameObject.SetActive(false);
            }
        }
    }
    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    #endregion

    internal void CallCloseSocket()
    {
        StartCoroutine(SocketManager.CloseSocket());
    }


    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (TBetMinus_Button) TBetMinus_Button.interactable = toggle;
        if (TBetPlus_Button) TBetPlus_Button.interactable = toggle;

        //  if (Turbo_Button) Turbo_Button.interactable = toggle;
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
        TempList.Clear();
        TempList.TrimExcess();
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, -430);
        // Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 1f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
        Tweener tweener = slotTransform.DOLocalMoveY(-3000, 0.75f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);

        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
    {
        alltweens[index].Kill();
        // int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-1875, 1f).SetEase(Ease.OutElastic);
        if (!isStop)
        {
            Debug.Log("playing stop sound");
            audioController.PlayWLAudio("spinStop");
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            if (index == alltweens.Count - 1)
            {
                audioController.PlayWLAudio("spinStop");
            }
            yield return null;
        }
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion


}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

