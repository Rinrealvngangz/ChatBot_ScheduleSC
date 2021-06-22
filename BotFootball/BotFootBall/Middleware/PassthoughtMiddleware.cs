using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotFootBall.Middleware
{
    public class PassthoughtMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            await turnContext.SendActivityAsync($"Rin:Before");
            if (turnContext.Activity.Type is ActivityTypes.Message && turnContext.Activity.Text == "123")
            {
                string input = turnContext.Activity.Text;
               // await turnContext.SendActivityAsync($"Rin:{input}");
                await next(cancellationToken);

            }
        
            await turnContext.SendActivityAsync($"Rin:After");
        }
    }
}
