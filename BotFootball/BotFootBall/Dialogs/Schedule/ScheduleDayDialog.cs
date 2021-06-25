using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using BotFootBall.Services;
using BotFootBall.Models;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using System.Text;
using Svg;
using System.Drawing.Drawing2D;

namespace BotFootBall.Dialogs.Schedule 
{
    public class ScheduleDayDialog : ComponentDialog
    {
        private readonly ISchedule _schedule;
      
       public ScheduleDayDialog(ISchedule schedule): base(nameof(ScheduleDayDialog))
        {
            _schedule = schedule;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{
                  InitScheduleStepAsyc,
                  MoreScheduleStepAsyc,
                  AcptScheduleStepAsyc,
                  CompleteStepAsyc,
            }));
            InitialDialogId = nameof(WaterfallDialog);
     }
        private async Task<DialogTurnResult> InitScheduleStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            DateTime dt = DateTime.UtcNow;
            _schedule.DisPlayScheduleByStep(dt,stepContext, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

      
        private async Task<DialogTurnResult> MoreScheduleStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
          return  await CardPromSchedule(stepContext, cancellationToken);
        }

       
        private async Task<DialogTurnResult> AcptScheduleStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];

            switch (operation.ToLower())
            { 
                case "ngày mai":
                    DateTime dt = DateTime.UtcNow.AddDays(1);
                    _schedule.DisPlayScheduleByStep(dt, stepContext, cancellationToken);
                    break;
                case "trong tuần":
                    _schedule.GetDateTimeOfWeeks().ForEach(  dt => _schedule.DisPlayScheduleByStep(dt, stepContext, cancellationToken));

                    break;
                case "hủy":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Xin lỗi vì sự làm phiền này."), cancellationToken);
                    break;
                default:
                    await stepContext.Context.SendActivityAsync(
                                      MessageFactory.Text("Tôi không hiểu, mong bạn thông cảm."), cancellationToken);
                    break;

            }
 
            return await stepContext.NextAsync(null, cancellationToken);
        }
        public static Bitmap Resize(System.Drawing.Image image, int width, int height)
        {

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        private async Task<DialogTurnResult> CompleteStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CardPromSchedule(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
            MessageFactory.Text("Có thể bạn quan tâm (Hủy nếu không thích)."), cancellationToken);

            List<string> operationList = new List<string> { "ngày mai", "trong tuần", "hủy" };

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = operationList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,
                }).ToList<AdaptiveAction>(),
            };
            // Promt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(operationList),
                Style = ListStyle.None
            }, cancellationToken);
        }
      
        //    Recognizer = CreateRecognizer();

        //    Triggers = new List<OnCondition>()
        //    {
        //        new OnBeginDialog()
        //        {
        //            //Intent = textInput,
        //            Actions =new List<Dialog>()
        //            {
        //                new TextInput()
        //                {
        //                    Prompt = new StaticActivityTemplate(MessageFactory.Text("Your name")),

        //                }
        //            }
        //        },
        //        new OnUnknownIntent(){
        //           Actions = new List<Dialog>()
        //           {
        //               new SendActivity("Please"),
        //           }
        //        },
        //};

        //private static Recognizer CreateRecognizer()
        //{
        //    var recognizer = new RegexRecognizer()
        //    {
        //        Intents = new List<IntentPattern>()
        //        {
        //            new IntentPattern(textInput,"(?i)textinput")
        //        }
        //    };
        //    return recognizer;


        //}

       
       
    }
}

