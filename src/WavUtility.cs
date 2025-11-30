using System;
using System.Text;
using UnityEngine;

public static class WavUtility
{
    


    public static AudioClip ToAudioClip(byte[] wavBytes, string name = "wav")
    {
        int channels = BitConverter.ToInt16(wavBytes, 22);
        int sampleRate = BitConverter.ToInt32(wavBytes, 24);
        int bitsPerSample = BitConverter.ToInt16(wavBytes, 34);

        int dataStart = FindDataChunk(wavBytes);
        int dataSize = BitConverter.ToInt32(wavBytes, dataStart - 4);

        int bytesPerSample = bitsPerSample / 8;

        int sampleCount = dataSize / bytesPerSample;
        float[] samples = new float[sampleCount];

        int offset = dataStart;

        for (int i = 0; i < sampleCount; i++)
        {
            switch (bitsPerSample)
            {
                case 16:
                    samples[i] = BitConverter.ToInt16(wavBytes, offset) / 32768f;
                    break;

                case 24:
                    int val24 = (wavBytes[offset + 2] << 16) |
                                (wavBytes[offset + 1] << 8) |
                                 wavBytes[offset + 0];
                    if ((val24 & 0x800000) != 0)
                        val24 |= unchecked((int)0xFF000000);
                    samples[i] = val24 / 8388608f;
                    break;

                case 32:
                    // Could be int or float, we need to detect format
                    // Most 32-bit WAVs are float
                    // Use "fmt " chunk to confirm (format tag)
                    short formatTag = BitConverter.ToInt16(wavBytes, 20);
                    if (formatTag == 3) // WAVE_FORMAT_IEEE_FLOAT
                        samples[i] = BitConverter.ToSingle(wavBytes, offset);
                    else
                        samples[i] = BitConverter.ToInt32(wavBytes, offset) / 2147483648f;
                    break;

                default:
                    throw new Exception("Unsupported WAV bit depth: " + bitsPerSample);
            }

            offset += bytesPerSample;
        }

        AudioClip clip = AudioClip.Create(
            name,
            sampleCount / channels,
            channels,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);

        return clip;
    }

    static int FindDataChunk(byte[] wav)
    {
        int pos = 12;
        while (pos + 8 <= wav.Length)
        {
            string chunkID = Encoding.ASCII.GetString(wav, pos, 4);
            int chunkSize = BitConverter.ToInt32(wav, pos + 4);

            // Found the data chunk
            if (chunkID == "data")
                return pos + 8;

            // Move to the next chunk (chunks are padded to even sizes)
            pos += 8 + chunkSize;
            if ((chunkSize % 2) == 1)
                pos++;
        }
        throw new Exception("Data chunk not found");
    }
}
