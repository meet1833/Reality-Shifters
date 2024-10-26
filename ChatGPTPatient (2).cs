using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ChatGPTPatient : MonoBehaviour
{
    public string apiKey = "API Key"; // Replace with your actual OpenAI API key
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    private string patientPrompt = "You are a patient visiting a doctor with an undiagnosed illness. Only provide brief answers, and reveal more information only if the doctor asks specific questions. Answer in first person.";
    
    public TMP_InputField userInputField; // UI field for doctor's questions
    public TMP_Text chatHistory; // UI text area to display the conversation
    private string chatLog = ""; // Keeps track of the conversation history
    
    void Start()
    {
        chatHistory.text = "Patient: " + patientPrompt + "\n\n";
    }

    public void OnSubmitQuestion()
    {
        string userQuestion = userInputField.text;
        if (string.IsNullOrEmpty(userQuestion))
            return;
        
        chatLog += "Doctor: " + userQuestion + "\n";
        StartCoroutine(CallChatGPT(userQuestion, response => {
            chatLog += "Patient: " + response + "\n";
            chatHistory.text = chatLog;
        }));

        userInputField.text = ""; // Clear input field after submitting
    }

    private IEnumerator CallChatGPT(string question, System.Action<string> callback)
    {
        // Construct the JSON request payload
        var requestData = new
        {
            model = "gpt-4", // Choose "gpt-4" or "gpt-3.5-turbo" based on your subscription
            messages = new[]
            {
                new { role = "system", content = patientPrompt },
                new { role = "user", content = question }
            }
        };
        
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                callback?.Invoke("Error: " + request.error);
            }
            else
            {
                var responseJson = request.downloadHandler.text;
                var response = JsonUtility.FromJson<ChatGPTResponse>(responseJson);
                if (response.choices != null && response.choices.Length > 0)
                {
                    callback?.Invoke(response.choices[0].message.content.Trim());
                }
                else
                {
                    callback?.Invoke("No response from ChatGPT.");
                }
            }
        }
    }

    [System.Serializable]
    private class ChatGPTResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }
}
