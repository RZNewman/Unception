using Mirror;
using System;
using UnityEngine;

namespace WeaverMessageTests.MessageValid
{
    class MessageValid : NetworkMessage
    {
        public uint netId;
        public Guid assetId;
        public Vector3 position;
        public Quaternion rotation;
        public byte[] payload;
    }
}
