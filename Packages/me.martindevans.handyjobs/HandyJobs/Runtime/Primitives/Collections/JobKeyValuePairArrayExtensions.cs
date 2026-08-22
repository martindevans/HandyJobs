using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Packages.HandyJobs.Runtime.Primitives.Collections
{
    public static class JobKeyValuePairArrayExtensions
    {
        /// <summary>
        /// Copy the keys from an input slice to an output list, appending them to the end.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle CopyKeys<TKey, TValue>(this NativeSlice<KeyValuePair<TKey, TValue>> input, NativeList<TKey> output, JobHandle dependsOn = default)
            where TKey : unmanaged
            where TValue : unmanaged
        {
            return new CopyKeysToListJob<TKey, TValue>(input, output).Schedule(dependsOn);
        }

        private struct CopyKeysToListJob<TKey, TValue>
            : IJob
            where TKey : unmanaged
            where TValue : unmanaged
        {
            private readonly NativeSlice<KeyValuePair<TKey, TValue>> _input;
            private NativeList<TKey> _output;

            public CopyKeysToListJob(NativeSlice<KeyValuePair<TKey, TValue>> input, NativeList<TKey> output)
            {
                _input = input;
                _output = output;
            }

            public void Execute()
            {
                foreach (var item in _input)
                    _output.Add(item.Key);
            }
        }


        /// <summary>
        /// Copy the values from an input slice to an output list, appending them to the end.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle CopyValue<TKey, TValue>(this NativeSlice<KeyValuePair<TKey, TValue>> input, NativeList<TValue> output, JobHandle dependsOn = default)
            where TKey : unmanaged
            where TValue : unmanaged
        {
            return new CopyValuesToListJob<TKey, TValue>(input, output).Schedule(dependsOn);
        }

        private struct CopyValuesToListJob<TKey, TValue>
            : IJob
            where TKey : unmanaged
            where TValue : unmanaged
        {
            private readonly NativeSlice<KeyValuePair<TKey, TValue>> _input;
            private NativeList<TValue> _output;

            public CopyValuesToListJob(NativeSlice<KeyValuePair<TKey, TValue>> input, NativeList<TValue> output)
            {
                _input = input;
                _output = output;
            }

            public void Execute()
            {
                foreach (var item in _input)
                    _output.Add(item.Value);
            }
        }
    }
}
