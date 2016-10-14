using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChattingInlineSprite_new : MonoBehaviour {

    /// <summary>
    /// 用正则取<#name>
    /// </summary>
    private static readonly Regex m_inputTagRegex =
          new Regex(@"<#(.+?)>", RegexOptions.Singleline);

    private InlineSpriteManager inlineSpriteManager;
    private RectTransform Content;
    public InputField inputText;
    public Button sendBtn;
    public Scrollbar scrollbarVertical;
    public GameObject emojiPanel;
    public Button emojiButton;
    private Button[] emojiBtns;


    public GameObject goprefab;
    public GameObject goprefab_left;
    public GameObject goContent;

    // Use this for initialization
    void Start() {
        //   inlineSpriteManager = scrollViewText.GetComponent<InlineSpriteManager>();
        //   Content = scrollViewText.transform.parent.GetComponent<RectTransform>();
        sendBtn.onClick.AddListener(ClickSendMessageBtn);
        emojiButton.onClick.AddListener(ClickEmojiBtn);
        emojiBtns = emojiPanel.GetComponentsInChildren<Button>();
        scrollbarVertical.onValueChanged.AddListener(ScrollBarValueChanged);
        Debug.Log(emojiBtns.Length);
        for (int i = 0; i < emojiBtns.Length; i++)
        {
            GameObject emojiTempGo = emojiBtns[i].gameObject;
            emojiBtns[i].onClick.AddListener(delegate () { ClickEmojiBtns(emojiTempGo); });
        }
    }

    bool isAddMessage = false;
    void ScrollBarValueChanged(float value)
    {
        if (isAddMessage)
        {
            scrollbarVertical.value = 0;
            isAddMessage = false;
        }
    }

    // List<>
    float chatHeight = 10.0f;
    float PlayerHight = 64.0f;
    void ClickSendMessageBtn()
    {
        if (inputText.text.Trim() == null || inputText.text.Trim() == "")
            return;

        GameObject tempChatItem = Instantiate(goprefab) as GameObject;
        tempChatItem.transform.parent = goContent.transform;
        tempChatItem.transform.localScale = Vector3.one;
        InlieSpriteText tempChatText = tempChatItem.transform.FindChild("Text").GetComponent<InlieSpriteText>();
       
        #region 解析输入表情正则
        string _TempInputText ="";
        int _TempMatchIndex = 0;
        foreach (Match match in m_inputTagRegex.Matches(inputText.text.Trim()))
        {
            _TempInputText += inputText.text.Trim().Substring(_TempMatchIndex, match.Index- _TempMatchIndex);
            _TempInputText+= "<quad name=" + match.Groups[1].Value + " size=56 width=1" + " />";
            _TempMatchIndex= match.Index + match.Length;
        }
        _TempInputText+= inputText.text.Trim().Substring(_TempMatchIndex, inputText.text.Trim().Length - _TempMatchIndex);
        #endregion

        tempChatText.text = _TempInputText;
        if (tempChatText.preferredWidth + 20.0f < 105.0f)
        {
            tempChatItem.GetComponent<RectTransform>().sizeDelta = new Vector2(105.0f, tempChatText.preferredHeight + 50.0f);
        }
        else if (tempChatText.preferredWidth + 20.0f > tempChatText.rectTransform.sizeDelta.x)
        {
            tempChatItem.GetComponent<RectTransform>().sizeDelta = new Vector2(tempChatText.rectTransform.sizeDelta.x + 20.0f, tempChatText.preferredHeight + 50.0f);
        }
        else
        {
            tempChatItem.GetComponent<RectTransform>().sizeDelta = new Vector2(tempChatText.preferredWidth + 20.0f, tempChatText.preferredHeight + 50.0f);
        }

        tempChatItem.SetActive(true);
        tempChatText.SetVerticesDirty();
        tempChatItem.GetComponent<RectTransform>().anchoredPosition = new Vector3(-10.0f, -chatHeight);
        chatHeight += tempChatText.preferredHeight + 50.0f + PlayerHight + 10.0f;
        if (chatHeight > goContent.GetComponent<RectTransform>().sizeDelta.y)
        {
            goContent.GetComponent<RectTransform>().sizeDelta = new Vector2(goContent.GetComponent<RectTransform>().sizeDelta.x, chatHeight);
        }
        //while (scrollbarVertical.value > 0.01f)
        //{
        //    scrollbarVertical.value = 0.0f;
        //}
        isAddMessage = true;
        inputText.text = "";
    }


    void AutoTalk()
    {
        string[] emojiTextName = new string[] { "sick", "watermelon", "run", "die", "angry", "bleeding", "nurturing","idle" };
        string strTalk = "按下F1,我会自动说话:<quad name="+ emojiTextName[Random.Range(0, emojiTextName.Length)]+" size=56 width=1 />,show一个emoji";

        GameObject tempChatItem = Instantiate(goprefab_left) as GameObject;
        tempChatItem.transform.parent = goContent.transform;
        tempChatItem.transform.localScale = Vector3.one;
        InlieSpriteText tempChatText = tempChatItem.transform.FindChild("Text").GetComponent<InlieSpriteText>();
        tempChatText.text = strTalk;
        if (tempChatText.preferredWidth + 20.0f < 105.0f)
        {
            tempChatItem.GetComponent<RectTransform>().sizeDelta = new Vector2(105.0f, tempChatText.preferredHeight + 50.0f);
        }
        else if (tempChatText.preferredWidth + 20.0f > tempChatText.rectTransform.sizeDelta.x)
        {
            tempChatItem.GetComponent<RectTransform>().sizeDelta = new Vector2(tempChatText.rectTransform.sizeDelta.x + 20.0f, tempChatText.preferredHeight + 50.0f);
        }
        else
        {
            tempChatItem.GetComponent<RectTransform>().sizeDelta = new Vector2(tempChatText.preferredWidth + 20.0f, tempChatText.preferredHeight + 50.0f);
        }

        tempChatItem.SetActive(true);
        tempChatText.SetVerticesDirty();
        tempChatItem.GetComponent<RectTransform>().anchoredPosition = new Vector3(10.0f, -chatHeight);
        chatHeight += tempChatText.preferredHeight + 50.0f + PlayerHight + 10.0f;
        if (chatHeight > goContent.GetComponent<RectTransform>().sizeDelta.y)
        {
            goContent.GetComponent<RectTransform>().sizeDelta = new Vector2(goContent.GetComponent<RectTransform>().sizeDelta.x, chatHeight);
        }
        //while (scrollbarVertical.value > 0.01f)
        //{
        //    scrollbarVertical.value = 0.0f;
        //}
        isAddMessage = true;
    }



    void ClickEmojiBtn()
    {
        emojiPanel.SetActive(!emojiPanel.activeSelf);
    }

    void ClickEmojiBtns(GameObject go)
    {
        Debug.Log(go.name);
       // inputText.text += "<quad name=" + go.name +" size=56 width=1" + " />";
        inputText.text += "<#" + go.name + ">";
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Return) && inputText.text != string.Empty)
        {
            ClickSendMessageBtn();
        }


        if (Input.GetKeyDown(KeyCode.F1))
            AutoTalk();

    }
}
