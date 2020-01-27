using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wanderer.EmojiText
{
    public class ChatTest : MonoBehaviour
    {
        [SerializeField]
        private TextAsset _txtData;

        [SerializeField]
        private GameObject _itemLeft;
        [SerializeField]
        private GameObject _itemRight;

        //滚动文本
        [SerializeField]
        private ScrollRect _scrollView;

        [SerializeField]
        private RectTransform _spriteRect;

        [SerializeField]
        private RectTransform _chatParent;

        [SerializeField]
        private InputField _inputText;

        private List<string> _allChatDatas = new List<string>();

        private Stack<GameObject> _chatLeftPool = new Stack<GameObject>();
        private Stack<GameObject> _chatRightPool = new Stack<GameObject>();

        //int _index = 0;
        //int _chatItemActiveCount = 12;
        //private List<ChatItem> _activeChatItem = new List<ChatItem>();

        private IEnumerator Start()
        {
            _scrollView.onValueChanged.AddListener(OnSrcollViewChanged);

            yield return new WaitForEndOfFrame();

            string[] datas = _txtData.text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                string data = datas[i].Insert(Random.Range(0, datas[i].Length), "[#emoji_" + Random.Range(0, 20) + "]");
                _allChatDatas.Add(data);

                //创建聊天 -- 测试 以序列号为偶数  为左边
                CreateItem(data, i % 2 == 0);
            }
        }

        private void CreateItem(string data, bool isLeft)
        {
            //if (_activeChatItem.Count >= _chatItemActiveCount)
            //	return;

            ChatItem item = GetChatItem(isLeft).GetComponent<ChatItem>();
            item.Text = data;
            //_activeChatItem.Add(item);

            //_scrollView.enabled = false;
            //_scrollView.verticalNormalizedPosition = 0.0f;
            //_scrollView.enabled = true;
        }


        private void OnSrcollViewChanged(Vector2 pos)
        {
            _spriteRect.anchoredPosition = _scrollView.content.anchoredPosition;

            //无限滚动测试 因为会导致Text的文本变幻位置相当于重新绘制  -- 暂时取消这个功能
            //if (_allChatDatas == null || _allChatDatas.Count == 0
            //	|| _activeChatItem == null || _activeChatItem.Count == 0
            //	|| _activeChatItem.Count <= _index || _activeChatItem.Count < _chatItemActiveCount)
            //	return;

            //if (Mathf.Approximately(pos.y, 1.0f))
            //{
            //	if (_index > 0)
            //	{
            //		_index--;
            //		for (int i = _index; i < _index+_chatItemActiveCount; i++)
            //		{
            //			_activeChatItem[i - _index].Text = _allChatDatas[i];
            //		}
            //	}
            //}
            //else if (pos.y < 0.02f)
            //{
            //	if (_index + _chatItemActiveCount < _allChatDatas.Count)
            //	{
            //		_index++;
            //		for (int i = _index; i < _index + _chatItemActiveCount; i++)
            //		{
            //			_activeChatItem[i - _index].Text = _allChatDatas[i];
            //		}
            //	}
            //}
        }


        #region 点击发送
        public void OnClickSend()
        {
            string chatStr = _inputText.text;
            if (string.IsNullOrEmpty(chatStr))
                return;
            //动态创建文本
            CreateItem(chatStr, _allChatDatas.Count % 2 == 0);
            _allChatDatas.Add(chatStr);

        }
        #endregion

        #region 简易对象池
        private GameObject GetChatItem(bool isLeft)
        {
            GameObject item = null;
            if (isLeft)
            {
                if (_chatLeftPool.Count > 0)
                    item = _chatLeftPool.Pop();
            }
            else
            {
                if (_chatRightPool.Count > 0)
                    item = _chatRightPool.Pop();
            }
            if (item == null)
            {
                item = isLeft ? Instantiate(_itemLeft) : Instantiate(_itemRight);
                item.transform.SetParent(_chatParent);
            }
            item.SetActive(true);
            return item;
        }

        private void ReleaseChatItem(GameObject obj, bool isLeft)
        {
            Stack<GameObject> pool = isLeft ? _chatLeftPool : _chatRightPool;
            obj.SetActive(false);
            pool.Push(obj);
        }
        #endregion

    }

}