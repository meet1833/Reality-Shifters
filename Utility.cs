using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Utility : MonoBehaviour
{ 
    public static string API_KEY = "API Key"; // Replace with your actual OpenAI API key

    public static readonly string chatGptEndpoint = "https://api.openai.com/v1/chat/completions";

    // Creates a conversation with ChatGPT and returns the response
    public static IEnumerator CallChatGPT(string prompt, Action<string> callback)
    {
        // Create request body
        var requestData = new
        {
            model = "gpt-4", // Use gpt-3.5-turbo or gpt-4 based on your API plan
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };
        
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(chatGptEndpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

            // Send request and wait for response
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                callback?.Invoke("Error: " + request.error);
            }
            else
            {
                // Parse response and invoke callback with the reply
                var responseJson = request.downloadHandler.text;
                var response = JsonUtility.FromJson<ChatGPTResponse>(responseJson);
                if (response.choices != null && response.choices.Length > 0)
                {
                    callback?.Invoke(response.choices[0].message.content);
                }
                else
                {
                    callback?.Invoke("No response from ChatGPT.");
                }
            }
        }
    }

    [Serializable]
    private class ChatGPTResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    private class Choice
    {
        public Message message;
    }

    [Serializable]
    private class Message
    {
        public string role;
        public string content;
    }
}
