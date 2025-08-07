using System.Text.Json.Serialization;
namespace EcommerceChatbot.Models


{
    public class ChatRequest
    {
        public string Question { get; set; }
    }

    public class ChatResponse
    {
        public string Answer { get; set; }
    }

    public class ChatExchange
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class DeepSeekResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

     public class OpenRouterResponse
    {
        public List<Choice> Choices { get; set; }
    }

 

}
