using EList.Common.Extensions;

namespace EList.Common.Models
{
    public class CommandResult
    {
        public bool Success => ErrorCode == 0;
        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public static CommandResult OK => new CommandResult();

        public static CommandResult Fail(int errorCode, string message = null, string stackTrace = null)
            => new CommandResult
            {
                ErrorCode = errorCode,
                Message = message,
                StackTrace = stackTrace
            };

        public static CommandResult Fail<TEnum>(TEnum errorCode)
            where TEnum : System.Enum
            => new CommandResult
            {
                ErrorCode = errorCode.Value(),
                Message = errorCode.Description(),
                StackTrace = null
            };

        public static CommandResult Fail<TEnum>(TEnum errorCode, string message = null, string stackTrace = null)
            where TEnum : System.Enum
            => new CommandResult
            {
                ErrorCode = errorCode.Value(),
                Message = message,
                StackTrace = stackTrace
            };

        protected CommandResult()
        {
        }

        [Obsolete("Please use CommandResult.Fail method")]
        public CommandResult(int errorCode, string message = null, string stackTrace = null)
        {
            ErrorCode = errorCode;
            Message = message;
            StackTrace = stackTrace;
        }
    }

    public class CommandResult<T> : CommandResult
    {
        public T Result { get; set; }

        public static new CommandResult<T> OK(T result)
            => new CommandResult<T>(result);

#pragma warning disable CS0618
        public static new CommandResult<T> Fail(int errorCode, string message = null, string stackTrace = null)
            => new CommandResult<T>
            {
                ErrorCode = errorCode,
                Message = message,
                StackTrace = stackTrace
            };

        public static new CommandResult<T> Fail<TEnum>(TEnum errorCode)
            where TEnum : System.Enum
            => new CommandResult<T>
            {
                ErrorCode = errorCode.Value(),
                Message = errorCode.Description(),
                StackTrace = null
            };

        public static new CommandResult<T> Fail<TEnum>(TEnum errorCode, string message = null, string stackTrace = null)
            where TEnum : System.Enum
            => new CommandResult<T>
            {
                ErrorCode = errorCode.Value(),
                Message = message,
                StackTrace = stackTrace
            };
#pragma warning restore CS0618

        [Obsolete("Please use CommandResult<T>(T result) constructor or CommandResult<T>.Fail method")]
        protected CommandResult()
        {
        }

        public CommandResult(T result)
        {
            Result = result;
        }

        [Obsolete("Please use CommandResult<T>.Fail method")]
        public CommandResult(int errorCode, string message = null, string stackTrace = null)
            : base(errorCode, message, stackTrace)
        {
        }
    }
}
