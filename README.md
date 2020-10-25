# ReactiveDiagnostics

Debug library for reactive programing

## What's this?

Though reactive programming provides excellent coding experience for engineers, separation of error occurrence and detection sometimes confuses them.
*ReactiveDiagnostics* enhances tracing in reactive programming to improve debuggability.

## Installation

(WIP)

## Dependencies

- .Net standard 2.0
  - System.Memory

For unit test project, following libraries are depended in addition:

- [System.Reactive](https://github.com/dotnet/reactive)
- [xunit](https://github.com/xunit/xunit)

## How to use

*ReactiveDiagnostics* introduces new reactive operator `Trace`.
`Trace` operator has 2 effects:

- records informations for each passed value.
- wraps exceptions into `ObservableStreamException` with more informations.

Following is the most simple usage.

```CSharp
void traceAction(TraceRecord<int> record)
    => Console.WriteLine(record.ToString());

observable
    .Trace("traceKey", traceAction)
    .Subscribe(observer);
```

`TraceRecord<T>` has following informations:

- `OperatorKey`:
  the key string which are specified when the operator is called.
- `SubscriptionIndex`:
  the index which identifies the reactive stream. For cold operator, subscription will generate new reactive stream and unique `SubscriptionIndex` will assign to each of them.
- `ValueIndex`:
  the index which identifies the passed values.
- `OperatorStackTrace`:
  the stack trace when the operator is called.
- `SubscriptionStackTrace`:
  the stack trace when the subscription is started.
- `ManagedThreadId`:
  the thread I which dealed this item.
- `TotalDuration`:
  the total duration from starting subscription of the stream.
- `LapDuration`:
  the lap duration from previous item.
- `Value`:
  the value of this recorded item.

`ObservableStreamException` has following informations:

- `OperatorKey`:
  the key string which are specified when the operator is called.
- `OperatorStackTrace`:
  the stack trace when the operator is called.
- `SubscriptionStackTrace`:
  the stack trace when the subscription is started.

# Author

GlassGrass
