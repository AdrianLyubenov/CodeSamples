using System;
using System.Reflection;
using UnityEngine;

namespace Megalith
{
    public static class ExitGuiUtility
    {
        public static bool ShouldRethrowException(Exception exception)
        {
            return IsExitGuiException(exception);
        }

        private static bool IsExitGuiException(Exception exception)
        {
            while (exception is TargetInvocationException && exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            return exception is ExitGUIException;
        }
    }
}