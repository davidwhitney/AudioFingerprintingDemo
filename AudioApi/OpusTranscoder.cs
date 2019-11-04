using System;
using System.Collections.Generic;
using System.IO;
using CSCore;

namespace AudioApi
{
    public class OpusTranscoder
    {
        private readonly int _sampleRate;
        private readonly int _channelCount;

        public OpusTranscoder(int sampleRate = 48000, int channelCount = 2)
        {
            _sampleRate = sampleRate;
            _channelCount = channelCount;
        }

        public byte[] ToWav(byte[] opusBytes)
        {
            using var memoryStream = new MemoryStream(opusBytes);
            var codec = new CSCore.Codecs.OPUS.OpusSource(memoryStream, _sampleRate, _channelCount);

            using var wavStream = new MemoryStream();
            codec.WriteToWaveStream(wavStream);
            
            wavStream.Flush();
            wavStream.Position = 0;
            return wavStream.ToArray();
        }

       
    }
}
