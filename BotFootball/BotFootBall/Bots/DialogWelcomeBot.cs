using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using BotFootBall.Services;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
namespace BotFootBall.Bots
{
    public class DialogWelcomeBot<T> : DialogBot<T>  where T : Dialog 
    {
        public DialogWelcomeBot(ConversationState conversationSate, T dialog, UserState userState, ISchedule schedule)
            : base(conversationSate, dialog , userState,schedule)
        {
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Xin chào, tôi là Bot bóng đá\n\n Bạn hãy gõ 'help' để biết các lệnh.";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);

                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }
    }
}
