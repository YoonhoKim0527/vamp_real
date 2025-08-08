using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Collections;


namespace Vampire
{
    public class MessageManager : MonoBehaviour
    {
        public static MessageManager Instance { get; private set; }

        [SerializeField] TextAsset defaultMessagesJSON;

        public List<MessageData> Messages { get; private set; } = new();

        string SavePath => Path.Combine(Application.persistentDataPath, "messages.json");

        void Awake()
        
        {
            Debug.Log("📁 messages.json 저장 위치: " + SavePath);
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadMessages();
                FetchRemoteMessagesFromServer();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadMessages()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Messages = JsonUtility.FromJson<MessageListWrapper>(json).messages;
            }
            else
            {
                // 기본 메시지 불러오기
                Messages = JsonUtility.FromJson<MessageListWrapper>(defaultMessagesJSON.text).messages;
                SaveMessages();
            }
            SyncNewMessagesFromDefault();
        }

        public void SaveMessages()
        {
            var wrapper = new MessageListWrapper { messages = Messages };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(SavePath, json);
        }

        public void ClaimReward(string messageId)
        {
            var msg = Messages.Find(m => m.id == messageId);
            if (msg != null && !msg.claimed)
            {
                msg.claimed = true;

                // TODO: 보상 지급 처리
                Debug.Log($"🎁 {msg.rewardType} {msg.rewardAmount} 지급됨");

                SaveMessages();
            }
        }

        void SyncNewMessagesFromDefault()
        {
            var defaultList = JsonUtility.FromJson<MessageListWrapper>(defaultMessagesJSON.text).messages;

            foreach (var msg in defaultList)
            {
                if (!Messages.Exists(m => m.id == msg.id))
                {
                    Messages.Add(msg);
                    Debug.Log($"📬 새 메시지 추가됨: {msg.title}");
                }
            }

            SaveMessages();
        }
        public void FetchRemoteMessagesFromServer()
        {
            StartCoroutine(FetchRemoteMessages());
        }

        IEnumerator FetchRemoteMessages()
        {
            string url = "https://raw.githubusercontent.com/OKthanos/vampire/main/remote_messages.json";

            UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string json = req.downloadHandler.text;
                var remoteMessages = JsonUtility.FromJson<MessageListWrapper>(json).messages;
                SyncNewRemoteMessages(remoteMessages);
            }
            else
            {
                Debug.LogWarning("🌐 메시지 다운로드 실패: " + req.error);
            }
        }
        void SyncNewRemoteMessages(List<MessageData> remoteMessages)
        {
            DateTime now = DateTime.Now;

            foreach (var msg in remoteMessages)
            {
                if (Messages.Exists(m => m.id == msg.id))
                    continue;

                if (DateTime.TryParse(msg.startTime, out var start) &&
                    DateTime.TryParse(msg.endTime, out var end))
                {
                    if (now >= start && now <= end)
                    {
                        Messages.Add(msg);
                        Debug.Log($"📬 서버 메시지 추가됨: {msg.title}");
                    }
                }
            }

            SaveMessages();
        }

        [Serializable]
        private class MessageListWrapper
        {
            public List<MessageData> messages;
        }
    }
}
