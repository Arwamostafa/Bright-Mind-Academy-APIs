using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using Domain.Options;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class Fireworksembeddinggenerator(IOptions<FireworksOptions> options)
    {
        private string ApiKey { get; } = options.Value.APIKey;

        private string EndPoint { get; } = options.Value.Embedding.Endpoint;

        private string ModelName { get; } = options.Value.Embedding.ModelName;
        public async Task<List<EmbeddingResponse>> GenerateEmbeddingsAsync(List<string> chunks)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization",$"Bearer {ApiKey} "); // Grab the rare API key

            var embeddingResponses = new List<EmbeddingResponse>();

            foreach (var chunk in chunks)
            {
                var httpRequestBody = new
                {
                    model = "nomic-ai/nomic-embed-text-v1.5",
                    input = chunk
                    //model= "thenlper/gte-large", 
                    //input = new[] { chunk }
                };
             

                var content = new StringContent(JsonSerializer.Serialize(httpRequestBody),Encoding.UTF8,"application/json" );

              
                var response = await httpClient.PostAsync(EndPoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Fireworks API request failed with status code: " + response.StatusCode);
                    continue;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(" Fireworks API response: " + responseContent);

                var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);
                if (embeddingResponse != null)
                {
                    embeddingResponses.Add(embeddingResponse);
                }
            }

            return embeddingResponses;
        }



    }

    public class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; }
    }

    public class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; }
    }
}
