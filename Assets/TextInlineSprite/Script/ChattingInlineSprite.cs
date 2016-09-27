using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ChattingInlineSprite : MonoBehaviour {

    public Text scrollViewText;
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
        Debug.Log(emojiBtns.Length);
        for (int i = 0; i < emojiBtns.Length; i++)
        {
            GameObject emojiTempGo = emojiBtns[i].gameObject;
            emojiBtns[i].onClick.AddListener(delegate () { ClickEmojiBtns(emojiTempGo); });
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
        InlieText tempChatText = tempChatItem.transform.FindChild("Text").GetComponent<InlieText>();
        tempChatText.text = inputText.text.Trim();
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
        while (scrollbarVertical.value > 0.01f)
        {
            scrollbarVertical.value = 0.0f;
        }
        inputText.text = "";
    }


    void AutoTalk()
    {
        string[] emojiTextName = new string[] { "sick", "watermelon", "run", "die", "angry", "bleeding", "nurturing","idle" };
        string strTalk = "按下F1,我会自动说话:<quad name="+ emojiTextName[Random.Range(0, emojiTextName.Length)]+" size=56 width=1 />,show一个emoji";

        GameObject tempChatItem = Instantiate(goprefab_left) as GameObject;
        tempChatItem.transform.parent = goContent.transform;
        tempChatItem.transform.localScale = Vector3.one;
        InlieText tempChatText = tempChatItem.transform.FindChild("Text").GetComponent<InlieText>();
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
        while (scrollbarVertical.value > 0.01f)
        {
            scrollbarVertical.value = 0.0f;
        }
    }



    void ClickEmojiBtn()
    {
        emojiPanel.SetActive(!emojiPanel.activeSelf);
    }

    void ClickEmojiBtns(GameObject go)
    {
        Debug.Log(go.name);
        inputText.text += "<quad name=" + go.name +" size=56 width=1" + " />";
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
