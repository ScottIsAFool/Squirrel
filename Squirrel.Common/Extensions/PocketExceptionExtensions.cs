using PocketSharp;
using ScottIsAFool.WindowsPhone.Logging;

namespace Squirrel.Extensions
{
    public static class PocketExceptionExtensions
    {
        public static void Log(this PocketException ex, string method, ILog logger)
        {
            logger.Error("Pocket error: {0}", ex.PocketError);
            logger.Error("Pocket error code: {0}", ex.PocketErrorCode);
            logger.ErrorException(method, ex);
        }
    }
}
