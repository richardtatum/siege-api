using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;

namespace api.Providers
{
    public class TicketProvider
    {
        private readonly INoSqlProvider _provider;
        private readonly CancellationTokenSource _source;
        private readonly CancellationToken _token;

        public TicketProvider(INoSqlProvider provider)
        {
            _provider = provider;
            _source = new CancellationTokenSource();
            _token = _source.Token;
        }

        public async Task<Ticket> GetTicket()
        {
            Ticket ticket;
            try
            {
                ticket = await _provider.GetItemAsync<Ticket>("ticket", _token);
            }
            catch (Exception e)
            {
                _source.Cancel();
                Console.WriteLine(e);
                throw;
            }

            if (ticket == null) return null;

            if (ticket.Valid) return ticket;
            return null; // update ticket
        }

        private async Task<Ticket> UpdateTicket()
        {
            throw new NotImplementedException();
        }
    }
}
