using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.InMemory;

namespace AudioApi
{
    public class Startup
    {
        private IModelService _modelService;
        private IAudioService _audioService;
        public IConfiguration Configuration { get; }
        public static bool BlockUntilIndexFinished { get; set; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            _modelService = new InMemoryModelService();
            _audioService = new SoundFingerprintingAudioService();

            services.AddSingleton(_ => new LoggerFactory().CreateLogger("default"));

            services.AddControllers();
            services.AddScoped<AudioIndexer>();
            services.AddSingleton(_ => _modelService);
            services.AddSingleton(_ => _audioService);
        }

        public void Configure(IApplicationBuilder app, AudioIndexer indexer)
        {
            var task = Task.Run(async () => await indexer.Execute("App_Data"));

            //if (BlockUntilIndexFinished)
            {
                Task.WaitAll(task);
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
