using Mirror;
using System.Collections.Generic;

namespace GeneratedReaderWriter.CreatesForList
{
    public class CreatesForList : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(List<int> data)
        {
            // empty
        }
    }
}
