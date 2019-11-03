using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SoundFingerprinting.Audio;
using SoundFingerprinting.InMemory;

namespace AudioApi.Test.Unit
{
    public class AudioIndexerTests
    {
        private AudioIndexer _indexer;
        private InMemoryModelService _modelService;
        private SoundFingerprintingAudioService _audioService;
        private MemoryLogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new MemoryLogger();
            _modelService = new InMemoryModelService();
            _audioService = new SoundFingerprintingAudioService();
            _indexer = new AudioIndexer(_logger, _modelService, _audioService);
        }

        [Test]
        public async Task Execute_IndexesFilesFoundInSuppliedPath()
        {
            await _indexer.Execute("App_Data");

            Assert.That(_modelService.Info.TracksCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Execute_LogsEachFileIndexed()
        {
            await _indexer.Execute("App_Data");

            Assert.That(_logger.Entries[0], Does.Contain("Leprous"));
            Assert.That(_logger.Entries[0], Does.Contain("Below"));
        }
    }

    public class MemoryLogger : ILogger
    {
        public List<string> Entries { get; } = new List<string>();
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) 
            => Entries.Add(formatter(state, exception));
        public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
    }
}