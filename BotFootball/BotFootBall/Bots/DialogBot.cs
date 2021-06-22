using BotFootBall.Middleware;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
namespace BotFootBall.Bots
{
    public class DialogBot<T> :  ActivityHandler where T : Dialog 
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;

        public DialogBot(ConversationState conversationState,  T dialog, UserState userState)
        {
            ConversationState = conversationState;
            Dialog = dialog;
            UserState = userState;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
           await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
           await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
          
            var replyText = $"Echo: {turnContext.Activity.Text}";
        //    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    
    }
}
