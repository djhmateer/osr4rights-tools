﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BackgroundServiceTest.BackgroundServices
{
    public class Test2Channel
    {
        private const int MaxMessagesInChannel = 100;

        private readonly Channel<string> _channel;

        public Test2Channel()
        {
            // BoundedChannel limits how big it can get eg 100 messages max
            var options = new BoundedChannelOptions(MaxMessagesInChannel)
            {
                SingleWriter = false, // multiple users can upload to the channel at the same time
                SingleReader = true
            };

            // accept strings in this Channel
            // which will be temp filename the required processing
            _channel = Channel.CreateBounded<string>(options);
        }

        public async Task<bool> AddFileAsync(string fileName, CancellationToken ct = default)
        {
            while (await _channel.Writer.WaitToWriteAsync(ct) && !ct.IsCancellationRequested)
            {
                if (_channel.Writer.TryWrite(fileName))
                {
                    Log.Information($"Added {fileName} to Test2Channel");
                    return true;
                }
            }

            return false;
        }

        public int CountOfFileProcessingChannel()
        {
            var foo = _channel.Reader.Count;
            return foo;
        }

        public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct = default) =>
            _channel.Reader.ReadAllAsync(ct);

    }
}
