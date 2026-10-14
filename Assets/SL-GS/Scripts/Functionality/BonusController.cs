using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class BonusController : MonoBehaviour
{
    [Header("Tap Bonus Buttons")]
    [SerializeField]
    private List<BonusChest> Chest_References;
    [SerializeField]
    private TMP_Text[] Bonus_Text;
    [SerializeField]
    internal TMP_Text Total_Bonus;
    [SerializeField]
    private GameObject Bonus_Object;
    [SerializeField]
    private SlotBehaviour slotManager;
    [SerializeField]
    private AudioController _audioManager;
    //[SerializeField]
    //private GameObject PopupPanel;
    [SerializeField]
    private Transform Win_Transform;
    [SerializeField]
    private Transform Loose_Transform;
    private bool gameOver = false;

    // [Header("For Testing Purpose Only...")]

    //private int[] m_BonusChestIndices; //Testing Bonus Data To Entered In The Unity Editor
    // private int m_Chest_Index_Count = 0;
    private double m_total_bonus = 0;
   
    //private double multiplier;
    internal bool WaitForBonusResult = true;
    [SerializeField] private SlotBehaviour slotBehaviour;
    [SerializeField] private SocketIOManager socketManager;
    private void Start()
    {
        Chest_References[0].m_Chest_Button.onClick.RemoveAllListeners();
        Chest_References[0].m_Chest_Button.onClick.AddListener(delegate {  OnClickOpenBonus(0); });

        Chest_References[1].m_Chest_Button.onClick.RemoveAllListeners();
        Chest_References[1].m_Chest_Button.onClick.AddListener(delegate {  OnClickOpenBonus(1); });

        Chest_References[2].m_Chest_Button.onClick.RemoveAllListeners();
        Chest_References[2].m_Chest_Button.onClick.AddListener(delegate { OnClickOpenBonus(2); });

        Chest_References[3].m_Chest_Button.onClick.RemoveAllListeners();
        Chest_References[3].m_Chest_Button.onClick.AddListener(delegate {  OnClickOpenBonus(3);  });

        Chest_References[4].m_Chest_Button.onClick.RemoveAllListeners();
        Chest_References[4].m_Chest_Button.onClick.AddListener(delegate {  OnClickOpenBonus(4);  });
    }

    internal void StartBonus()
    {
        gameOver = false;
        if (Win_Transform) Win_Transform.gameObject.SetActive(false);
        if (Loose_Transform) Loose_Transform.gameObject.SetActive(false);
        Total_Bonus.text = "00";

        if (_audioManager) _audioManager.playBgAudio("bonus");
        if (_audioManager) _audioManager.StopWLAaudio();
        if (Bonus_Object) Bonus_Object.SetActive(true);
        //m_BonusChestIndices = bonusResult.ToArray();

        //multiplier = mult;
        foreach (var chests in Chest_References)
        {
            chests.isOpend = false;
        }
    }

    void chestToggle(bool isTrue, bool onlyOpend = false)
    {
        if (!onlyOpend)
        {
            foreach (var chests in Chest_References)
            {
                chests.m_Chest_Button.interactable = isTrue;
            }
        }
        else
        {
            foreach (var chests in Chest_References)
            {
                if (!chests.isOpend)
                {
                    chests.m_Chest_Button.interactable = isTrue;
                }
            }
        }
    }

    private void OnClickOpenBonus(int indexOfChest)
    {
        if (!Chest_References[indexOfChest].isOpend && !gameOver)
        {
            Chest_References[indexOfChest].m_Chest_Button.interactable = false;
            Chest_References[indexOfChest].isOpend = true;
            //if (IsOpening) return;

            //Debug.Log(string.Concat("<color=red>", "Click On Chest Detected... ", indexOfChest, "</color>"));
            StartCoroutine(OpenChest(indexOfChest));
        }
    }

    private IEnumerator OpenChest(int indexOfChest)
    {
        chestToggle(false);

        //  IsOpening = true;
        ImageAnimation chestImageAnim = Chest_References[indexOfChest].m_Chest_Button.GetComponent<ImageAnimation>();
        Tween tween = chestImageAnim.transform.DOShakePosition(1f, new Vector3(15, 0, 0), 30, 90, true).SetLoops(-1, LoopType.Incremental);

        socketManager.OnBonusCollect(indexOfChest);
        yield return new WaitUntil(() => socketManager.isResultdone);

        tween.Kill();
        chestImageAnim.StartAnimation();
        double bonusAmount = 0;
        if (socketManager.bonusData.payload.payout == 0)
        {
            
            socketManager.ResultData.payload.winAmount = socketManager.bonusData.payload.winAmount;
            slotBehaviour.updateBalance();
            gameOver = true;
        }
        else
        {
            m_total_bonus += socketManager.bonusData.payload.winAmount;
            bonusAmount = socketManager.bonusData.payload.winAmount;
        }

        DoAnimationOnChestClick(indexOfChest, bonusAmount);
        Total_Bonus.text = m_total_bonus.ToString();
        yield return new WaitUntil(() => chestImageAnim.textureArray[^1] == chestImageAnim.rendererDelegate.sprite);
        chestImageAnim.StopAnimation(false);
        chestToggle(true, true);
        if (gameOver)
        {
            yield return new WaitForSeconds(1.5f);
            ResetChestBonusButtons();
        }
       // IsOpening = false;
        yield return null;
    }

    private void ResetChestBonusButtons()
    {
        Bonus_Object.SetActive(false);
        if(_audioManager) _audioManager.playBgAudio();

        m_total_bonus = 0;
        //multiplier = 0;
        Total_Bonus.text = string.Concat("Bonus Score", "\n\n", m_total_bonus.ToString());
        foreach (var item in Chest_References)
        {
            ImageAnimation img = item.m_Chest_Button.GetComponent<ImageAnimation>();
            img.StopAnimation();
            img.rendererDelegate.sprite = img.textureArray[0];
            item.m_Chest_Button.interactable = true;
        }
        ResetToDefaultAnimationAfterChestClick();

    }

   
    #region DOTween Animations and Reset Animations
    private void DoAnimationOnChestClick(int m_index, double m_score)
    {
        if (_audioManager) _audioManager.StopWLAaudio();

        BonusChest m_Temp_Chest = Chest_References[m_index];
        //Debug.Log(m_score+" .>>>>>>>>>>>>> score     <<<<<<<."+m_index);

        if (m_score > 0)
        {
            m_Temp_Chest.m_Score.text = "+" + m_score.ToString();
            if (_audioManager) _audioManager.PlayWLAudio("bonuswin");

        }
        else
        {
            m_Temp_Chest.m_Score.text = "Game Over";
            if (_audioManager) _audioManager.PlayWLAudio("bonuslose");
        }
        m_Temp_Chest.m_ScoreHolder.SetActive(true);
        DOTweenScale(m_Temp_Chest.m_ScoreHolder.transform, m_Temp_Chest.m_ScoreHolder.transform, 1f);
    }

    private void DOTweenScale(Transform m_rect_transform, Transform m_obj_transform, float m_time)
    {
        m_rect_transform.DOScale(m_obj_transform.localScale + (Vector3.one * 1.2f), m_time);
        m_rect_transform.DOLocalMoveY(m_obj_transform.position.y + 220, m_time).OnComplete(() =>
            {
                m_obj_transform.gameObject.SetActive(false);
            });
    }

    //This method is used to reset the bonus chest to default and ready for next bonus
    private void ResetToDefaultAnimationAfterChestClick()
    {
        foreach (var i in Chest_References)
        {
            i.m_ScoreHolder.transform.localScale = Vector3.zero;
            i.m_ScoreHolder.transform.localPosition = new Vector3(0.1f, 0.1f, 0.1f);
            i.m_ScoreHolder.SetActive(false);
        }
        slotBehaviour.CheckPopups = false;
    }
    #endregion

    #region Structures Used
    [System.Serializable]
    public class BonusChest
    {
        public TMP_Text m_Score;
        public Button m_Chest_Button;
        public GameObject m_ScoreHolder;
        public bool isOpend;
    }
    #endregion
}
