using UnityEngine;
using System.IO;
using System.Threading;
using AudioSynthesis;

namespace UnityMidi
{
    [System.Serializable]
    public class StreamingAssetResouce : IResource
    {
        [SerializeField] string streamingAssetPath;

        public bool ReadAllowed()
        {
            return true;
        }

        public bool WriteAllowed()
        {
            return false;
        }

        public bool DeleteAllowed()
        {
            return false;
        }

        public string GetName()
        {
            return Path.GetFileName(streamingAssetPath);
        }

        public Stream OpenResourceForRead()
        {

            //WWW file = new WWW("jar:file://" + Application.dataPath + "!/assets/" + streamingAssetPath);

            //WWW file = new WWW("file://" + Application.dataPath + "/StreamingAssets/" + streamingAssetPath);
#if UNITY_EDITOR

            WWW file = new WWW(Application.dataPath + "/StreamingAssets/" + streamingAssetPath);
#else
            
            WWW file = new WWW("jar:file://" + Application.dataPath + "!/assets/" + streamingAssetPath);
#endif
            while (!file.isDone)
            {
                Thread.Sleep(10);
            }
            var data = file.bytes;
            //var length = data.LongLength;

            MemoryStream ms = new MemoryStream(file.bytes,0,data.Length);
            ms.Position = 0;
            return ms;

        }

        public Stream OpenResourceForWrite()
        {
            throw new System.NotImplementedException();
        }

        public void DeleteResource()
        {
            throw new System.NotImplementedException();
        }
    }
}
