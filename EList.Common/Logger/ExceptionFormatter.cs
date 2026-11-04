using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EList.Common.Logger
{
    /// <summary>
    /// Разворачивает цепочку InnerException / AggregateException в человекочитаемый текст.
    /// Логирование — на стороне вызывающего кода (filter, сервисы).
    /// </summary>
    public static class ExceptionFormatter
    {
        public const int DefaultClientMaxLength = 1000;

        private const int MaxClientBranches = 3;

        private const int MaxChainDepth = 64;

        // Игнорируем исключения с постоянным шаблонным сообщением, не несущим информации о причине
        private static readonly HashSet<Type> BoilerplateWrappers = new()
        {
            typeof(TargetInvocationException)
        };

        // Игнорируем в выводе дефолтные сообщения отмен/таймаутов как текст ошибки
        // Вместо этого добавляется пометка (отменена)/(таймаут), чтобы причина не терялась
        private static readonly HashSet<string> GenericCancelTimeoutMessages = new HashSet<string>(StringComparer.Ordinal)
        {
            "A task was canceled.",        // TaskCanceledException
            "The operation was canceled.", // OperationCanceledException
            "The operation has timed out." // TimeoutException
        };

        public static string Format(
            Exception exception,
            ExceptionFormatKind kind = ExceptionFormatKind.Diagnostic,
            int? maxLength = null)
        {
            if (exception == null)
                return null;

            var text = kind == ExceptionFormatKind.Client
                ? FormatClient(exception)
                : FormatDiagnostic(exception);

            var limit = maxLength
                ?? (kind == ExceptionFormatKind.Client ? DefaultClientMaxLength : null);

            return Truncate(text, limit);
        }

        private static string FormatClient(Exception exception)
        {
            if (exception is AggregateException aggregate)
            {
                var flat = aggregate.Flatten();

                var count = flat.InnerExceptions?.Count ?? 0;
                if (count > 0)
                {
                    // В параллельных тасках фиксируем, что собираем с нескольких потоков 
                    var shown = Math.Min(count, MaxClientBranches);
                    var sb = new StringBuilder(FormatBranch(flat.InnerExceptions[0]));

                    for (var i = 1; i < shown; i++)
                        sb.Append("; ").Append(FormatBranch(flat.InnerExceptions[i]));

                    if (count > shown)
                        sb.Append(" (+").Append(count - shown).Append(" more errors)");

                    return sb.ToString();
                }
            }

            return FormatBranch(exception);
        }

        private static string FormatBranch(Exception exception)
        {
            // если причина была отменой/таймаутом, в конец добавляется пометка (cancelled)/(timed out)
            var leaf = GetInformativeLeaf(exception, out var maskedCause);
            var text = FormatSingle(leaf);

            return maskedCause == null ? text : $"{text} ({maskedCause})";
        }

        private static string FormatDiagnostic(Exception exception)
        {
            var sb = new StringBuilder();
            Append(sb, exception, indent: 0, branchIndex: null);
            return sb.ToString().TrimEnd();
        }

        private static void Append(StringBuilder sb, Exception exception, int indent, int? branchIndex)
        {
            if (exception == null)
                return;

            if (exception is AggregateException aggregate)
            {
                var flat = aggregate.Flatten();
                var count = flat.InnerExceptions?.Count ?? 0;
                AppendLine(sb, indent, branchIndex, $"AggregateException ({count} errors)");

                for (var i = 0; i < count; i++)
                    Append(sb, flat.InnerExceptions[i], indent + 1, i);

                return;
            }

            var current = exception;
            var depth = 0;

            while (current != null)
            {
                if (depth >= MaxChainDepth)
                {
                    AppendLine(sb, indent + depth, null, "... (цепочка обрезана)");
                    return;
                }

                if (current is AggregateException nestedAggregate)
                {
                    // Вложенный агрегат выравнивается по текущей глубине цепочки.
                    Append(sb, nestedAggregate, indent + depth, null);
                    return;
                }

                if (ShouldSkipWrapper(current) || BoilerplateWrappers.Contains(current.GetType()))
                {
                    current = current.InnerException;
                    continue;
                }

                var prefix = depth == 0 ? branchIndex : null;
                var line = depth == 0
                    ? FormatSingle(current)
                    : $"caused by {FormatSingle(current)}";

                AppendLine(sb, indent + depth, prefix, line);

                current = current.InnerException;
                depth++;
            }
        }

        private static bool ShouldSkipWrapper(Exception exception)
        {
            if (exception.InnerException == null)
                return false;

            if (string.IsNullOrWhiteSpace(exception.Message))
                return true;

            return string.Equals(
                exception.Message,
                exception.InnerException.Message,
                StringComparison.Ordinal);
        }

        private static Exception GetInformativeLeaf(Exception exception, out string maskedCause)
        {
            maskedCause = null;
            var current = UnwrapAggregate(exception);

            Exception fallback = null;  // самое глубокое непустое сообщение (прежнее поведение)
            Exception preferred = null; // то же, но без маскирующих сообщений

            // Ищем самое информативное исключение цепочки: наиболее глубокое осмысленное сообщение
            // Дефолтные сообщения отмен/таймаутов, шаблонные текста и перебросы с тем же сообщением пропускаются если в цепочке есть альтернатива
            // maskedCause — пометка о замаскированной причине (отмена/таймаут), null если маскировать нечего
            while (current != null)
            {
                var innerException = current.InnerException;
                var message = current.Message;

                if (!string.IsNullOrWhiteSpace(message))
                {
                    fallback = current;

                    var trimmedMessage = message.Trim();
                    var duplicateOfInner = innerException != null && string.Equals(message, innerException.Message, StringComparison.Ordinal);
                    var isMaskable = duplicateOfInner || BoilerplateWrappers.Contains(current.GetType());

                    if (!isMaskable && GenericCancelTimeoutMessages.Contains(trimmedMessage))
                    {
                        maskedCause = GetCancelTimeoutMarker(current);
                    }
                    else if (!isMaskable)
                    {
                        preferred = current;
                    }
                }

                if (innerException == null)
                    break;

                current = UnwrapAggregate(innerException);
            }

            // Альтернативы нет, тогда возвращаем отмену/таймаут: сообщение само говорит о причине и доп. пометка не нужна.
            if (preferred == null)
                maskedCause = null;

            return preferred ?? fallback ?? exception;
        }

        private static string GetCancelTimeoutMarker(Exception exception)
        {
            if (exception is TimeoutException)
                return "таймаут";

            if (exception is OperationCanceledException)
                return "отменена";

            return null;
        }

        private static Exception UnwrapAggregate(Exception exception)
        {
            while (exception is AggregateException aggregate)
            {
                var flat = aggregate.Flatten();
                if (flat.InnerExceptions == null || flat.InnerExceptions.Count == 0)
                    break;

                exception = flat.InnerExceptions[0];
            }

            return exception;
        }

        private static string FormatSingle(Exception exception)
        {
            var message = exception.Message?.Trim();

            var typeName = exception.GetType().Name;

            return string.IsNullOrEmpty(message) ? typeName : $"{typeName}: {message}";
        }

        private static void AppendLine(StringBuilder sb, int indent, int? branchIndex, string text)
        {
            if (indent > 0)
                sb.Append(' ', indent * 2);

            if (branchIndex.HasValue)
                sb.Append('[').Append(branchIndex.Value).Append("] ");

            sb.AppendLine(text);
        }

        private static string Truncate(string text, int? maxLength)
        {
            const string suffix = "...";

            if (string.IsNullOrEmpty(text) || !maxLength.HasValue || text.Length <= maxLength.Value)
                return text;

            var limit = maxLength.Value;

            return limit <= suffix.Length
                ? text[..limit]
                : string.Concat(text.AsSpan(0, limit - suffix.Length), suffix);
        }
    }
}
