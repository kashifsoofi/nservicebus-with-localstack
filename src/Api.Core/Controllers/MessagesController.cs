using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace Api.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public MessagesController(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<string>> Get()
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
        [HttpGet]
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

        public string RandomString(int size, bool lowerCase)
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


        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
