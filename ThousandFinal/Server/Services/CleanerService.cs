using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThousandFinal.Server.Hubs;

namespace ThousandFinal.Server.Services
{
    public class CleanerService : IHostedService
    {
        private readonly IServiceProvider provider;
        private AppHub appHub;

        private Timer _timer;

        public CleanerService(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(WriteFromHub, null, 0, 10000);
            return Task.CompletedTask;
        }

        void WriteFromHub(object state)
        {
            appHub = new AppHub(provider);
            appHub.WriteRooms();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //New Timer does not have a stop. 
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
