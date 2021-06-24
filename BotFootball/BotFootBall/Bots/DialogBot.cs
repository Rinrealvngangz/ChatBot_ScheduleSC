﻿using BotFootBall.Middleware;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using BotFootBall.Services;
using BotFootBall.Dialogs.Schedule;
using BotFootBall.Models;

namespace BotFootBall.Bots
{
    public class DialogBot<T> :  ActivityHandler where T : Dialog 
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ISchedule _schedule;
        
        public DialogBot(ConversationState conversationState,T dialog, UserState userState , ISchedule schedule)
        {
            ConversationState = conversationState;
            Dialog = dialog;
            UserState = userState;
            _schedule = schedule;
            
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
            var stateObj = ConversationState.CreateProperty<StateObj>(nameof(StateObj));
            var stateProp = await stateObj.GetAsync(turnContext, () => new StateObj());
            if (turnContext.Activity.Text.Equals("help"))
            {
                string help = "Chức năng của bot\n\n'hôm nay': xem lịch trong ngày\n\n'" +
                              "ngày mai': xem lịch thi đấu ngày mai\n\n" +
                              "trong tuần': xem lịch thi đấu trong tuần\n\n";
                await turnContext.SendActivityAsync(
                MessageFactory.Text(help), cancellationToken);
              
                stateProp.State = "help";
            }
            else
            {
                stateProp.State = null;
            }
            if(string.IsNullOrEmpty(stateProp.State))
            {
                // Run the Dialog with the new message Activity.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
         
        
        }
    

    }
}
