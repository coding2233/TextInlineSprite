using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    // Use this for initialization
    void Start () {
        inlineSpriteManager = scrollViewText.GetComponent<InlineSpriteManager>();
        Content = scrollViewText.transform.parent.GetComponent<RectTransform>();
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

    void ClickSendMessageBtn()
    {
        scrollViewText.text +="<color=blue>A say: </color>"+inputText.text + "\n";
        inputText.text = "";

        if (scrollViewText.preferredHeight <= Content.sizeDelta.y)
            scrollViewText.rectTransform.sizeDelta = new Vector2(scrollViewText.rectTransform.sizeDelta.x, Content.sizeDelta.y);
        else
        {
            scrollViewText.rectTransform.sizeDelta = new Vector2(scrollViewText.rectTransform.sizeDelta.x, scrollViewText.preferredHeight);
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, scrollViewText.preferredHeight);
            scrollbarVertical.value = 0.0f;
        }
    }

    void ClickEmojiBtn()
    {
        emojiPanel.SetActive(!emojiPanel.activeSelf);
    }

    void ClickEmojiBtns(GameObject go)
    {
        inputText.text += "<quad name=" + go.GetComponent<Image>().sprite.name +" size=20 width=1" + " />";
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Return) && inputText.text != string.Empty)
        {
            ClickSendMessageBtn();
        }
	}
}
