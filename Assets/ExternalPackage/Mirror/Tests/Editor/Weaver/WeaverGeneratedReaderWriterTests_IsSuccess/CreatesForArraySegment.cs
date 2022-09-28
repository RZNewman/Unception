using Mirror;
using System;

namespace GeneratedReaderWriter.CreatesForArraySegment
{
    public class CreatesForArraySegment : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(ArraySegment<int> data)
        {
            // empty
        }
    }
}
