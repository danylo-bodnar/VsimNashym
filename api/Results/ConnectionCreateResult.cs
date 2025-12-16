using api.Enums;
using api.Models;

namespace api.Results
{
    public class ConnectionCreateResult
    {
        public ConnectionResult Result { get; init; }
        public Connection? Connection { get; init; }
    }
}
