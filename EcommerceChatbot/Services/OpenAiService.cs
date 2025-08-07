using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace EcommerceAssistant.Services
{
    public class OpenAiService
    {
        private readonly OpenAIService _openAi;

        public OpenAiService(string apiKey)
        {
            _openAi = new OpenAIService(new OpenAiOptions
            {
                ApiKey = apiKey
            });
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            var completionResult = await _openAi.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Model = Models.Gpt_3_5_Turbo,
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("Tu es un assistant e-commerce."),
                    ChatMessage.FromUser(prompt)
                }
            });

            if (completionResult.Successful && completionResult.Choices != null)
            {
                return completionResult.Choices.First().Message.Content;
            }
            else if (completionResult.Error != null)
            {
                return $"Erreur API: {completionResult.Error.Message}";
            }
            else
            {
                return "Erreur inconnue lors de la réponse AI.";
            }
        }
    }
}
