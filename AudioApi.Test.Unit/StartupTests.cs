using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AudioApi.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AudioApi.Test.Unit
{
    [TestFixture]
    public class StartupTests
    {
        private TestServer _testHost;
        private HttpClient _client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Startup.BlockUntilIndexFinished = true;
            _testHost = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _testHost.CreateClient();
        }

        [Test]
        public async Task CallingAudioSearch_IsOk()
        {
            var fileBytes = File.ReadAllBytes("TestData/bad-below.wav");
            var byteArrayContent = new ByteArrayContent(fileBytes);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("audio/vnd.wave");

            var result = await _client.PostAsync("/audiosearch", byteArrayContent);
            var typed = await result.As<BestMatch>();
            
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(typed.Artist, Is.EqualTo("Leprous"));
            Assert.That(typed.Title, Is.EqualTo("Below"));
        }
    }

    public static class DeserializationExtensions
    {
        public static async Task<TModelType> As<TModelType>(this HttpResponseMessage responseMessage)
            => JsonConvert.DeserializeObject<TModelType>(await responseMessage.Content.ReadAsStringAsync());
    }
}
