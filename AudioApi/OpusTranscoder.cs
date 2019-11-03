using System.Collections.Generic;
using System.IO;
using CSCore;

namespace AudioApi
{
    public class OpusTranscoder
    {
        public byte[] ToWav(byte[] opusBytes)
        {
            using var memoryStream = new MemoryStream(opusBytes);
            using var wavStream = new MemoryStream();

            var codec = new CSCore.Codecs.OPUS.OpusSource(memoryStream, 48000, 2);

            codec.WriteToWaveStream(wavStream);

            wavStream.Position = 0;
            return wavStream.ToArray();
        }

       
    }
}
