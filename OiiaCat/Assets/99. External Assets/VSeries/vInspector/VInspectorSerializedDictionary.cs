using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;



namespace VInspector
{
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public List<SerializedKeyValuePair<TKey, TValue>> serializedKvps = new();

        public float dividerPos = .33f;

        public SerializedDictionary() : base() { }

        // [추가됨] Dictionary를 받아서 초기화하는 생성자
        // 이를 통해 new SerializedDictionary<K,V>(dictionary) 사용 가능
        public SerializedDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
            // 런타임에 생성된 경우, 인스펙터에서 바로 확인 가능하도록 리스트 동기화
            if (dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    serializedKvps.Add(new SerializedKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
                }
                
                for (int i = 0; i < serializedKvps.Count; i++)
                {
                    serializedKvps[i].index = i;
                }
            }
        }
        
        public void OnBeforeSerialize()
        {
            foreach (var kvp in this)
                if (serializedKvps.FirstOrDefault(r => this.Comparer.Equals(r.Key, kvp.Key)) is SerializedKeyValuePair<TKey, TValue> serializedKvp)
                    serializedKvp.Value = kvp.Value;
                else
                    serializedKvps.Add(kvp);

            serializedKvps.RemoveAll(r => r.Key is not null && !this.ContainsKey(r.Key));

            for (int i = 0; i < serializedKvps.Count; i++)
                serializedKvps[i].index = i;

        }
        public void OnAfterDeserialize()
        {
            this.Clear();

            foreach (var serializedKvp in serializedKvps)
            {
                serializedKvp.isKeyNull = serializedKvp.Key is null;
                serializedKvp.isKeyRepeated = serializedKvp.Key is not null && this.ContainsKey(serializedKvp.Key);

                if (serializedKvp.isKeyNull) continue;
                if (serializedKvp.isKeyRepeated) continue;


                this.Add(serializedKvp.Key, serializedKvp.Value);

            }

        }



        [System.Serializable]
        public class SerializedKeyValuePair<TKey_, TValue_>
        {
            public TKey_ Key;
            public TValue_ Value;

            public int index;

            public bool isKeyRepeated;
            public bool isKeyNull;


            public SerializedKeyValuePair(TKey_ key, TValue_ value) { this.Key = key; this.Value = value; }

            public static implicit operator SerializedKeyValuePair<TKey_, TValue_>(KeyValuePair<TKey_, TValue_> kvp) => new(kvp.Key, kvp.Value);
            public static implicit operator KeyValuePair<TKey_, TValue_>(SerializedKeyValuePair<TKey_, TValue_> kvp) => new(kvp.Key, kvp.Value);

        }

    }

}