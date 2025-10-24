using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using Newtonsoft.Json;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    // [SerializeField]
    // private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements; // I think not required any more

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("frames")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private Sprite[] frameBG;
    [SerializeField] private Sprite empty;
    // [Header("Line Button Objects")]
    // [SerializeField]
    // private List<GameObject> StaticLine_Objects;

    // [Header("Line Button Texts")]
    // [SerializeField]
    // private List<TMP_Text> StaticLine_Texts;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button BetPerLine_Button;
    [SerializeField]
    private Button TotalBetPlus_Button;
    [SerializeField]
    private Button TotalBetMinus_Button;
    [SerializeField]
    private Button StopSpin_Button;
    [SerializeField]
    private Button Turbo_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Q_Sprite;
    [SerializeField]
    private Sprite[] K_Sprite;
    [SerializeField]
    private Sprite[] A_Sprite;
    [SerializeField]
    private Sprite[] J_Sprite;
    [SerializeField]
    private Sprite[] Ten_Sprite;
    [SerializeField]
    private Sprite[] Nine_Sprite;
    [SerializeField]
    private Sprite[] Diamond_Sprite;
    [SerializeField]
    private Sprite[] Ace_Sprite;
    [SerializeField]
    private Sprite[] Seven_Sprite;
    [SerializeField]
    private Sprite[] Magnet_Sprite;


    [SerializeField]
    private Sprite[] Bonus_Sprite;
    [SerializeField]
    private Sprite[] Scatter_Sprite;
    [SerializeField]
    private Sprite[] Wild_Sprite;


    [Header("Turbo Animated Sprites")]
    [SerializeField] private Sprite[] TurboToggleSprites;


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
    private TMP_Text SpinCounter_text;


    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    [Header("BonusGame Popup")]
    [SerializeField]
    private BonusController _bonusManager;


    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tween WinTween;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present

    [SerializeField]
    private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Coroutine WinAnimRoutine = null;
    private Tween ScoreTween;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    [SerializeField] internal bool CheckPopups = false;

    private bool StopSpinToggle;
    private bool IsTurboOn;
    private bool WasAutoSpinOn;
    private float SpinDelay = 0.2f;

    private Sprite turboOriginalSprite;

    private int SpinCounter = 0;
    internal int BetCounter = 0;
    internal double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 20;
    [SerializeField]
    private int IconSizeFactor = 100;
    [SerializeField] private int SpaceFactor = 0;    //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns

    // COMPLETED: slot add frame

    // TODO: slot icon rearranged check
    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeSpinAmount(true); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeSpinAmount(false); });

        if (BetPerLine_Button) BetPerLine_Button.onClick.RemoveAllListeners();
        if (BetPerLine_Button) BetPerLine_Button.onClick.AddListener(delegate { ChangeBetCycle(); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.RemoveAllListeners();
        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });

        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.RemoveAllListeners();
        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); if (audioController) audioController.PlayButtonAudio(); });

        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

        tweenHeight = (16 * IconSizeFactor) - 280;
        turboOriginalSprite = Turbo_Button.GetComponent<Image>().sprite;
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin && !IsSpinning && SpinCounter > 0)
        {
            if (audioController) audioController.PlayButtonAudio();
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
        if (IsAutoSpin)
        {
            if (audioController) audioController.PlayButtonAudio();
            IsAutoSpin = false;
            SpinCounter = 0;
            SpinCounter_text.text = SpinCounter.ToString();

            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    void TurboToggle()
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IsTurboOn)
        {
            IsTurboOn = false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite = turboOriginalSprite;
        }
        else
        {
            IsTurboOn = true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
        }
    }
    private void TriggerPlusMinusButtons(int m_cmd)
    {
        switch (m_cmd)
        {
            case 0:
                TotalBetPlus_Button.interactable = true;
                TotalBetMinus_Button.interactable = false;
                break;
            case 1:
                TotalBetMinus_Button.interactable = false;
                TotalBetMinus_Button.interactable = true;
                break;
            case 2:
                TotalBetMinus_Button.interactable = true;
                TotalBetMinus_Button.interactable = true;
                break;
        }
    }


    private IEnumerator AutoSpinCoroutine()
    {
        // for (int i = 0; i < SpinCounter; i++)
        // {
        while (IsAutoSpin && SpinCounter > 0)
        {
            SpinCounter--;
            SpinCounter_text.text = SpinCounter.ToString();
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            // if (SpinCounter <= 0)
            //     IsAutoSpin = false;
        }

        // }
        StopAutoSpin();
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
    internal void updateBalance()
    {
        if (Balance_text) Balance_text.text = SocketManager.PlayerData.balance.ToString("f3");
        if (TotalWin_text) TotalWin_text.text = SocketManager.ResultData.payload.winAmount.ToString("f3");
    }
    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            i++;
        }
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
            // if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            // if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        // else
        // {
        //     if (AutoSpin_Button) AutoSpin_Button.interactable = true;
        //     if (SlotStart_Button) SlotStart_Button.interactable = true;
        // }
    }

    #region LinesCalculation
    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
        // StaticLine_Texts[count].text = (count + 1).ToString();
        // StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(int LineID)
    {
        DestroyStaticLine();

        // int LineID = 1;
        // try
        // {
        //     LineID = int.Parse(LineID_Text.text);
        // }
        // catch (Exception e)
        // {
        //     Debug.Log("Exception while parsing " + e.Message);
        // }

        // List<int> y_points = null;
        // // y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        // Debug.Log(JsonConvert.SerializeObject(y_string));
        // // PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
        PayCalculator.ActivatePayLine(LineID, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        // PayCalculator.ResetStaticLine();
        PayCalculator.TurnOffHoverPayline();
    }
    #endregion

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.InitialData.bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count).ToString();
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count;
        // CompareBalance();
    }

    private void ChangeSpinAmount(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (SpinCounter < 50)
            {
                SpinCounter += 1;
                if (SpinCounter >= 50)
                {
                    BetPlus_Button.interactable = false;
                    BetMinus_Button.interactable = true;
                }
                else
                {
                    BetPlus_Button.interactable = true;
                    BetMinus_Button.interactable = true;
                }
            }
        }
        else
        {
            if (SpinCounter > 0)
            {
                SpinCounter -= 1;
                if (SpinCounter <= 0)
                {
                    BetPlus_Button.interactable = true;
                    BetMinus_Button.interactable = false;
                }
                else
                {
                    BetPlus_Button.interactable = true;
                    BetMinus_Button.interactable = true;
                }
            }
        }
        if (SpinCounter_text) SpinCounter_text.text = SpinCounter.ToString();
    }



    #region InitialFunctions
    //Method is used to shuffle the temp matrix
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.InitialData.bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0";
        if (Balance_text) Balance_text.text = SocketManager.PlayerData.balance.ToString("f2");
        if (BetPlus_Button) { BetPlus_Button.interactable = true; }
        if (BetMinus_Button) { BetMinus_Button.interactable = false; }
        currentBalance = SocketManager.PlayerData.balance;
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * Lines;
        // _bonusManager.PopulateWheel(SocketManager.bonusdata);
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.UIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            if (!IsSpinning)
            {
                if (audioController) audioController.StopWLAaudio();
            }
        }
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {


            case 0:
                for (int i = 0; i < Nine_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Nine_Sprite[i]);
                }
                animScript.AnimationSpeed = Nine_Sprite.Length;
                break;

            case 1:
                for (int i = 0; i < Ten_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Ten_Sprite[i]);
                }
                animScript.AnimationSpeed = Ten_Sprite.Length;
                break;

            case 2:
                for (int i = 0; i < A_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(A_Sprite[i]);
                }
                animScript.AnimationSpeed = A_Sprite.Length;
                break;

            case 3:
                for (int i = 0; i < J_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(J_Sprite[i]);
                }
                animScript.AnimationSpeed = J_Sprite.Length;
                break;

            case 4:
                for (int i = 0; i < K_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(K_Sprite[i]);
                }
                animScript.AnimationSpeed = K_Sprite.Length;
                break;

            case 5:
                for (int i = 0; i < Q_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Q_Sprite[i]);
                }
                animScript.AnimationSpeed = Q_Sprite.Length;
                break;

            case 6:
                for (int i = 0; i < Seven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Seven_Sprite[i]);
                }
                animScript.AnimationSpeed = Seven_Sprite.Length;
                break;

            case 7:
                for (int i = 0; i < Ace_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Ace_Sprite[i]);
                }
                animScript.AnimationSpeed = Ace_Sprite.Length - 3;
                break;

            case 8:
                for (int i = 0; i < Diamond_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Diamond_Sprite[i]);
                }
                animScript.AnimationSpeed = Diamond_Sprite.Length - 3;
                break;

            case 9:
                for (int i = 0; i < Magnet_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Magnet_Sprite[i]);
                }
                animScript.AnimationSpeed = Magnet_Sprite.Length - 3;
                break;

            case 10:
                for (int i = 0; i < Bonus_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Bonus_Sprite[i]);
                }
                animScript.AnimationSpeed = Bonus_Sprite.Length - 3;
                break;

            case 11:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = Wild_Sprite.Length - 3;
                break;

            case 12:
                for (int i = 0; i < Scatter_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Scatter_Sprite[i]);
                }
                animScript.AnimationSpeed = Scatter_Sprite.Length - 3;
                break;

        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (TotalWin_text) TotalWin_text.text = "0.000";
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
        if (WinAnimRoutine != null)
            StopCoroutine(WinAnimRoutine);
        // if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        WinningsAnim(false);

        if (audioController) audioController.PlayButtonAudio();
        // PayCalculator.ResetLines();
        PayCalculator.ResetAllPayLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(1);
            }
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.StopWLAaudio();
        if (audioController) audioController.PlaySpinBonusAudio();

        IsSpinning = true;

        ToggleButtonGrp(false);

        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }
        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int resultNum = int.Parse(SocketManager.ResultData.matrix[i][j]);
                // print("resultNum: " + resultNum);
                // print("image loc: " + j + " " + i);
                PopulateAnimationSprites(Tempimages[j].slotImages[i].GetComponent<ImageAnimation>(), resultNum);
                Tempimages[j].slotImages[i].sprite = myImages[resultNum];
            }
        }

        if (IsTurboOn)                                                      // changes
        {

            yield return new WaitForSeconds(0.1f);
            StopSpinToggle = true;
        }
        else
        {
            for (int i = 0; i < 5; i++)
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

        if (audioController) audioController.StopSpinBonusAudio();
        yield return alltweens[^1].WaitForCompletion();

        if (SocketManager.ResultData.payload.winAmount > 0)
        {
            SpinDelay = 1f + (SocketManager.ResultData.payload.wins.Count - 1);
        }
        else
        {
            SpinDelay = 0.2f;
        }
        if (SocketManager.ResultData.payload.winAmount > 0)
        {
            List<int> winLine = new();
            foreach (var item in SocketManager.ResultData.payload.wins)
            {
                winLine.Add(item.line);
            }
            CheckPayoutLineBackend(winLine);

        }
        CheckForFeaturesAnimation();
        KillAllTweens();

        CheckPopups = true;
        ScoreTween?.Kill();

        if (TotalWin_text) TotalWin_text.text = SocketManager.ResultData.payload.winAmount.ToString("F3");

        if (Balance_text) Balance_text.text = SocketManager.ResultData.player.balance.ToString("F3");

        currentBalance = SocketManager.PlayerData.balance;

        // if (SocketManager.ResultData.jackpot > 0)
        // {
        //     uiManager.PopulateWin(4, SocketManager.ResultData.jackpot);
        //     yield return new WaitUntil(() => !CheckPopups);
        //     CheckPopups = true;
        // }
        if (SocketManager.ResultData.bonus.isTriggered)
        {
            yield return new WaitForSeconds(1f);
            _bonusManager.StartBonus();

        }
        else
        {
            CheckWinPopups();
        }

        yield return new WaitUntil(() => !CheckPopups);

        if (SocketManager.ResultData.payload.winAmount > 0)
            WinningsAnim(true);

        if (!IsAutoSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {

            IsSpinning = false;
        }

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

        ScoreTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f3");
        });
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.ResultData.payload.winAmount >= currentTotalBet * 10 && SocketManager.ResultData.payload.winAmount < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.ResultData.payload.winAmount);
        }
        else if (SocketManager.ResultData.payload.winAmount >= currentTotalBet * 15 && SocketManager.ResultData.payload.winAmount < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.ResultData.payload.winAmount);
        }
        else if (SocketManager.ResultData.payload.winAmount >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.ResultData.payload.winAmount);
        }
        else
        {
            CheckPopups = false;
        }
    }



    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, double jackpot = 0)
    {


        if (LineId.Count > 0)
        {
            for (int i = 0; i < LineId.Count; i++)
            {
                PayCalculator.ActivatePayLine(LineId[i], false);
            }

            if (WinAnimRoutine != null)
                StopCoroutine(WinAnimRoutine);
            WinAnimRoutine = StartCoroutine(AnimationCoroutine(LineId));
        }

    }
    #endregion
    private void CheckForFeaturesAnimation()
    {
        bool playJackpot = false;
        bool playScatter = false;
        bool playBonus = false;
        bool playFreespin = false;

        if (SocketManager.ResultData.bonus.isTriggered)
        {
            playBonus = true;
        }

        PlayFeatureAnimation(playJackpot, playScatter, playBonus, playFreespin);
    }
    private void PlayFeatureAnimation(bool jackpot = false, bool scatter = false, bool bonus = false, bool freeSpin = false)
    {
        for (int i = 0; i < SocketManager.ResultData.matrix.Count; i++)
        {
            for (int j = 0; j < SocketManager.ResultData.matrix[i].Count; j++)
            {

                if (int.TryParse(SocketManager.ResultData.matrix[i][j], out int parsedNumber))
                {

                    if (bonus && parsedNumber == 10)
                    {

                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }

                }

            }
        }
    }
    internal void CallCloseSocket()
    {
        StartCoroutine(SocketManager.CloseSocket());
    }

    IEnumerator AnimationCoroutine(List<int> LineId)
    {

        List<GameObject> animFrame = new List<GameObject>();
        Image tempObject;
        int randomIndex = 0;
        int n = 0;

        n = 5;

        while (n > 0)
        {
            PayCalculator.ResetAllPayLines();
            for (int i = 0; i < LineId.Count; i++)
            {
                List<int> y_anim = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.ActivatePayLine(LineId[i], false);
                randomIndex = UnityEngine.Random.Range(0, frameBG.Length);

                List<KeyValuePair<int, int>> coords = new();
                //for (int j = 0; j < LineId.Count; j++)
                //{
                coords.Clear();
                for (int k = 0; k < SocketManager.ResultData.payload.wins[i].positions.Count; k++)
                {
                    int rowIndex = SocketManager.InitialData.lines[LineId[i]][k];
                    int columnIndex = k;
                    coords.Add(new KeyValuePair<int, int>(rowIndex, columnIndex));
                }
                // }

                foreach (var coord in coords)
                {
                    int rowIndex = coord.Key;
                    int columnIndex = coord.Value;
                    StartGameAnimation(Tempimages[columnIndex].slotImages[rowIndex].gameObject);
                }
                if (IsAutoSpin)
                {
                    yield return new WaitForSeconds(1f);
                }
                else
                {

                    yield return new WaitForSeconds(1.25f);
                }
                PayCalculator.ResetAllPayLines();
                foreach (var item in animFrame)
                {
                    item.SetActive(false);
                    item.transform.parent.GetComponent<Image>().sprite = empty;

                }

                StopGameAnimation();
                animFrame.Clear();
            }
            PayCalculator.ResetAllPayLines();
            n--;
            if (IsAutoSpin)
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {

                yield return new WaitForSeconds(1.25f);
            }
        }

        for (int i = 0; i < LineId.Count; i++)
        {
            PayCalculator.ActivatePayLine(LineId[i], false);
        }
    }
    private void ChangeBetCycle()
    {
        if (audioController) audioController.PlayButtonAudio();

        if (BetCounter < SocketManager.InitialData.bets.Count - 1)
        {
            BetCounter++;
        }
        else
        {
            BetCounter = 0;
        }
        if (LineBet_text) LineBet_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.InitialData.bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.InitialData.bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.InitialData.bets.Count - 1; // Loop to the last bet
            }
        }

        Debug.Log("run this");
        if (LineBet_text) LineBet_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.InitialData.bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * Lines;
        // CompareBalance();
    }

    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (BetPerLine_Button) BetPerLine_Button.interactable = toggle;
        if (TotalBetMinus_Button) TotalBetMinus_Button.interactable = toggle;
        if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = toggle;

        if (toggle)
        {
            if (SpinCounter <= 0)
            {
                if (BetMinus_Button) BetMinus_Button.interactable = false;
                if (BetPlus_Button) BetPlus_Button.interactable = true;
            }
            else if (SpinCounter >= 100)
            {
                if (BetMinus_Button) BetMinus_Button.interactable = true;
                if (BetPlus_Button) BetPlus_Button.interactable = false;
            }
            else if (SpinCounter > 0 && SpinCounter < 100)
            {
                if (BetMinus_Button) BetMinus_Button.interactable = true;
                if (BetPlus_Button) BetPlus_Button.interactable = true;
            }
        }
        else
        {
            if (BetMinus_Button) BetMinus_Button.interactable = toggle;
            if (BetPlus_Button) BetPlus_Button.interactable = toggle;
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
            TempList[i].transform.parent.GetComponent<Image>().sprite = empty;
            TempList[i].transform.parent.GetChild(1).gameObject.SetActive(false);
        }
        TempList.Clear();
        TempList.TrimExcess();
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
    {
        alltweens[index].Kill();
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        int tweenpos = (reqpos * (IconSizeFactor + SpaceFactor)) - (IconSizeFactor + (2 * SpaceFactor));
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpaceFactor > 0 ? SpaceFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
        if (!isStop)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return null;
        }
    }

    //internal void OnBonusChestClick(int index)
    //{
    //    SocketManager.OnBonusCollect(index);
    //}
    private void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();
    }
    #endregion

}

[Serializable]
public struct SlotImage
{
    public List<Image> slotImages;
}

