namespace Api.Core.Controllers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NServiceBus;

    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public MessagesController(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Post()
        {
            var guid = Guid.NewGuid();
            var message = new RequestDataMessage
            {
                DataId = guid,
                String = "String property value"
            };

            await _messageSession.Send("Samples.FullDuplex.Server", message)
                .ConfigureAwait(false);

            return guid.ToString();
        }

        [Route("bigmessage")]
        [HttpPost]
        public async Task<ActionResult<string>> BigMessage()
        {
            var guid = Guid.NewGuid();
            var message = new RequestDataMessage
            {
                DataId = guid,
                String = RandomString(256000, false)
            };

            await _messageSession.Send("Samples.FullDuplex.Server", message)
                .ConfigureAwait(false);

            return guid.ToString();
        }

        private string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }
}
