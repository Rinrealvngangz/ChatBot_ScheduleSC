using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotFootBall.Services
{
 
    public class Timer 
    {
        private IHttpClientFactory _factory;
        public ConversationReference ConversationReference { get; }
        public int Seconds { get; }
        private readonly IAdapterIntegration _adapter;
        public Timer(IHttpClientFactory factory ,IAdapterIntegration adapter,ConversationReference conversationReference,int seconds)
        {
            _adapter = adapter; 
            ConversationReference = conversationReference;
            Seconds = seconds;
            _factory = factory;
        }
        public async Task Start()
        {
            await Task.Delay(Seconds * 1000);
            await _adapter.ContinueConversationAsync(string.Empty, ConversationReference, SendMessageAsync);
        }
        private async Task SendMessageAsync(ITurnContext turncontext, CancellationToken cancellationtoken)
        {
            HttpClient client = _factory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:44348");
            var respone = client.GetAsync("api/notify");
            await turncontext.SendActivityAsync(respone.Result.StatusCode.ToString());
        }


    }
}
