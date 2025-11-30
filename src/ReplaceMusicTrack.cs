using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ReplaceGameOverSound
{
    [HarmonyPatch(typeof(DeathInRaidScreen), nameof(DeathInRaidScreen.ShowGameOver))]
    public static class ReplaceMusicTrack
    {

        private static bool hasRun = false;
        public static void Prefix(ref DeathInRaidScreen __instance)
        {
            //Plugin.Logger.Log("proccing?");
            if (!hasRun)
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                AudioClip temp_audio = LoadWAV(Path.Combine(assemblyPath, "new_GameOver.wav"), Path.Combine(assemblyPath, "default_GameOver.wav"));

                // ok the laugh track is not a music.

                //Plugin.Logger.Log("ok it proc at least." + temp_audio);
                if (temp_audio != null)
                {
                    //Plugin.Logger.Log("processing replacement.");
                    __instance._demonsLaughClip = temp_audio;
                }
            }
        }

        private static AudioClip LoadWAV(string expected_filepath, string default_filepath)
        {
            string path = File.Exists(expected_filepath) ? expected_filepath : default_filepath;
            if (!File.Exists(path))
                return null;

            byte[] wavBytes = File.ReadAllBytes(path);
            AudioClip temp_audio = WavUtility.ToAudioClip(wavBytes);

            return temp_audio;
        }

        public static string GenerateMD5Checksum(AudioClip clip)
        {
            // Check if audio data is loaded and available
            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                Plugin.Logger.Log($"AudioClip '{clip.name}' data is not loaded. Ensure 'Load Type' is 'Decompress on Load' in the Inspector.");
                // Optionally call clip.LoadAudioData() here and wait for it to load, but that's asynchronous.
                return null;
            }

            // Get the audio data as a float array (interleaved channels)
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            // Convert the float array to a byte array for hashing
            byte[] byteArray = new byte[samples.Length * 4]; // a float is 4 bytes
            Buffer.BlockCopy(samples, 0, byteArray, 0, byteArray.Length);

            // Compute the MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(byteArray);

                // Convert the byte array to a hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

    }
}
