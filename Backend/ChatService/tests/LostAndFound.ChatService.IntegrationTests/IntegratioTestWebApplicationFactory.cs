﻿using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using LostAndFound.ChatService.DataAccess.Context;
using LostAndFound.ChatService.DataAccess.Context.Interfaces;
using LostAndFound.ChatService.DataAccess.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;

namespace LostAndFound.ChatService.IntegrationTests
{
    public class IntegratioTestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncLifetime
        where TStartup : class
    {
        private readonly TestcontainerDatabase _container;

        public IntegratioTestWebApplicationFactory()
        {
            _container = new TestcontainersBuilder<MongoDbTestcontainer>()
                .WithDatabase(new MongoDbTestcontainerConfiguration
                {
                    Database = "test_chat_db",
                    Username = "mongo",
                    Password = "mongo",
                })
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMongoChatServiceDbContext>();

                var options = Options.Create(new ChatServiceDatabaseSettings()
                {
                    ConnectionString = _container.ConnectionString,
                    DatabaseName = _container.Database
                });
                services.AddSingleton<IMongoChatServiceDbContext>(_ =>
                    new MongoChatServiceDbContext(options));
            });
        }

        public async Task InitializeAsync() => await _container.StartAsync();

        public new async Task DisposeAsync() => await _container.DisposeAsync();
    }
}
