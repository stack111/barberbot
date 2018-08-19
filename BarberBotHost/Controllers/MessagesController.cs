using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BarberBot.Dialogs;
using System.Net.Http;

namespace BarberBot.Controllers
{
    [Route("api/messages")]
    [ApiController]
    [BotAuthentication]
    public class MessagesController
    {
        private readonly Appointment appointment;
        private readonly Shop shop;
        public MessagesController(Appointment appointment, Shop shop)
        {
            this.appointment = appointment;
            this.shop = shop;
        }
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        public virtual async Task<HttpResponseMessage> Post([FromBody] Microsoft.Bot.Connector.Activity activity)
        {
            if (activity != null)
            {
                // Check if activity is of type message
                if (activity.GetActivityType() == ActivityTypes.Message)
                {
                    await Conversation.SendAsync(activity, () => new RootDialog(appointment, shop));
                }
                else
                {
                    Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                }


                // one of these will have an interface and process it
                //switch (activity.GetActivityType())
                //{
                //    case ActivityTypes.Message:
                //        await Conversation.SendAsync(activity, MakeRootDialog);
                //        break;

                //    case ActivityTypes.ConversationUpdate:
                //    case ActivityTypes.ContactRelationUpdate:
                //    case ActivityTypes.Typing:
                //    case ActivityTypes.DeleteUserData:
                //    default:
                //        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                //        break;
                //}
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}