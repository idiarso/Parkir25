using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ParkIRC.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, cts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
            }

            cts.Cancel();
            return await task;
        }

        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, cts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
            }

            cts.Cancel();
            await task;
        }

        public static async Task<T> WithRetry<T>(this Task<T> task, int maxRetries, TimeSpan delay)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await task;
                }
                catch (Exception) when (i < maxRetries - 1)
                {
                    await Task.Delay(delay);
                }
            }

            return await task;
        }

        public static async Task WithRetry(this Task task, int maxRetries, TimeSpan delay)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await task;
                    return;
                }
                catch (Exception) when (i < maxRetries - 1)
                {
                    await Task.Delay(delay);
                }
            }

            await task;
        }

        public static async Task<T> WithCircuitBreaker<T>(this Task<T> task, int maxFailures, TimeSpan resetTimeout)
        {
            var circuitBreaker = CircuitBreaker.GetOrCreate(task.GetHashCode().ToString(), maxFailures, resetTimeout);
            return await circuitBreaker.ExecuteAsync(() => task);
        }

        public static async Task WithCircuitBreaker(this Task task, int maxFailures, TimeSpan resetTimeout)
        {
            var circuitBreaker = CircuitBreaker.GetOrCreate(task.GetHashCode().ToString(), maxFailures, resetTimeout);
            await circuitBreaker.ExecuteAsync(() => task);
        }

        public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks);
        }

        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                var completedTask = await Task.WhenAny(task, tcs.Task);
                if (completedTask == tcs.Task)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                return await task;
            }
        }

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                var completedTask = await Task.WhenAny(task, tcs.Task);
                if (completedTask == tcs.Task)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                await task;
            }
        }

        public static ConfiguredTaskAwaitable<T> ConfigureAwait<T>(this Task<T> task, bool continueOnCapturedContext)
        {
            return task.ConfigureAwait(continueOnCapturedContext);
        }

        public static ConfiguredTaskAwaitable ConfigureAwait(this Task task, bool continueOnCapturedContext)
        {
            return task.ConfigureAwait(continueOnCapturedContext);
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
            if (completedTask != task)
            {
                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
            }

            cts.Cancel();
            return await task;
        }

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
            if (completedTask != task)
            {
                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
            }

            cts.Cancel();
            await task;
        }
    }

    internal class CircuitBreaker
    {
        private static readonly Dictionary<string, CircuitBreaker> _circuitBreakers = new();
        private static readonly object _lock = new();

        private readonly int _maxFailures;
        private readonly TimeSpan _resetTimeout;
        private int _failures;
        private DateTime _lastFailure;
        private bool _isOpen;

        private CircuitBreaker(int maxFailures, TimeSpan resetTimeout)
        {
            _maxFailures = maxFailures;
            _resetTimeout = resetTimeout;
        }

        public static CircuitBreaker GetOrCreate(string key, int maxFailures, TimeSpan resetTimeout)
        {
            lock (_lock)
            {
                if (!_circuitBreakers.TryGetValue(key, out var circuitBreaker))
                {
                    circuitBreaker = new CircuitBreaker(maxFailures, resetTimeout);
                    _circuitBreakers[key] = circuitBreaker;
                }
                return circuitBreaker;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> task)
        {
            if (_isOpen)
            {
                if (DateTime.UtcNow - _lastFailure > _resetTimeout)
                {
                    _isOpen = false;
                    _failures = 0;
                }
                else
                {
                    throw new CircuitBreakerOpenException();
                }
            }

            try
            {
                return await task();
            }
            catch (Exception)
            {
                _failures++;
                _lastFailure = DateTime.UtcNow;

                if (_failures >= _maxFailures)
                {
                    _isOpen = true;
                }

                throw;
            }
        }

        public async Task ExecuteAsync(Func<Task> task)
        {
            if (_isOpen)
            {
                if (DateTime.UtcNow - _lastFailure > _resetTimeout)
                {
                    _isOpen = false;
                    _failures = 0;
                }
                else
                {
                    throw new CircuitBreakerOpenException();
                }
            }

            try
            {
                await task();
            }
            catch (Exception)
            {
                _failures++;
                _lastFailure = DateTime.UtcNow;

                if (_failures >= _maxFailures)
                {
                    _isOpen = true;
                }

                throw;
            }
        }
    }

    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException() : base("Circuit breaker is open") { }
    }
} 