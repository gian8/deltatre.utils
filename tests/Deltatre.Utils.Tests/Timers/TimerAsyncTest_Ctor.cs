﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deltatre.Utils.Timers;
using NUnit.Framework;

namespace Deltatre.Utils.Tests.Timers
{
  [TestFixture]
  public partial class TimerAsyncTest
  {
    [Test]
    public void Ctor_Throws_When_ScheduledAction_IsNull()
    {
      // ACT
      Assert.Throws<ArgumentNullException>(
        () => new TimerAsync(
          null,
          TimeSpan.FromSeconds(1),
          TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void Ctor_Throws_When_DueTime_Is_Less_Than_Zero()
    {
      // ACT
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new TimerAsync(
          _ => Task.CompletedTask,
          TimeSpan.FromMilliseconds(-6),
          TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void Ctor_Throws_When_Period_Is_Less_Than_Zero()
    {
      // ACT
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new TimerAsync(
          _ => Task.CompletedTask,
          TimeSpan.FromSeconds(10),
          TimeSpan.FromMilliseconds(-3)));
    }

    [Test]
    public void Ctor_Allows_To_Pass_DueTime_Zero()
    {
      // ACT
      Assert.DoesNotThrow(
        () => new TimerAsync(
          _ => Task.CompletedTask,
          TimeSpan.Zero,
          TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void Ctor_Allows_To_Pass_Period_Zero()
    {
      // ACT
      Assert.DoesNotThrow(
        () => new TimerAsync(
          _ => Task.CompletedTask,
          TimeSpan.FromSeconds(5),
          TimeSpan.Zero));
    }

    [Test]
    public void Ctor_Allows_To_Pass_Infinite_DueTime()
    {
      Assert.DoesNotThrow(
        () => new TimerAsync(
          _ => Task.CompletedTask,
          Timeout.InfiniteTimeSpan,
          TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void Ctor_Allows_To_Pass_Infinite_Period()
    {
      Assert.DoesNotThrow(
        () => new TimerAsync(
          _ => Task.CompletedTask,
          TimeSpan.FromSeconds(5),
          Timeout.InfiniteTimeSpan));
    }

    [Test]
    public async Task It_Is_Possible_To_Execute_Background_Workload_Before_Previous_Execution_Completes()
    {
      // ARRANGE
      var iterationInfos = new ConcurrentBag<(DateTime start, DateTime end, int iterationNumber)>();
      int counter = 0;
      Func<CancellationToken, Task> action = async _ => 
      {
        var iterationNumber = Interlocked.Increment(ref counter);

        var startTime = DateTime.Now;
        await Task.Delay(500).ConfigureAwait(false);
        var endTime = DateTime.Now;

        iterationInfos.Add((startTime, endTime, iterationNumber));
      };

      var target = new TimerAsync(
        action,
        TimeSpan.FromMilliseconds(40),
        TimeSpan.FromMilliseconds(40),
        canStartNextActionBeforePreviousIsCompleted: true);

      // ACT
      target.Start();
      await Task.Delay(600).ConfigureAwait(false);
      await target.Stop().ConfigureAwait(false);

      // ASSERT
      Assert.GreaterOrEqual(iterationInfos.Count, 2);

      // check the overlap
      var timeFrames = iterationInfos
        .OrderBy(tf => tf.iterationNumber)
        .Select(tf => (tf.start, tf.end))
        .ToArray<(DateTime start, DateTime end)>();

      var timeFrame1 = timeFrames[0];
      var timeFrame2 = timeFrames[1];
      Assert.IsTrue(timeFrame1.end > timeFrame2.start);
    }
  }
}
