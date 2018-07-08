using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System.Net.Http;
using System.Web.Http.Description;
using System.Diagnostics;
using BarberBot.Models;

namespace BarberBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly Shop shop;
        public MessagesController(Shop shop)
        {
            this.shop = shop;
        }
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Microsoft.Bot.Connector.Activity activity)
        {
            if (activity != null)
            {
                // Check if activity is of type message
                if (activity.GetActivityType() == ActivityTypes.Message)
                {
                    await Conversation.SendAsync(activity, () => new RootDialog(shop));
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