using System.IO.Pipes;
using System.Text;

namespace XInputHook;

interface IReceiveableMessage;

interface ISendableMessage {
    byte Opcode { get; }
    byte[] Serialize();
}

class PrintMessage : IReceiveableMessage {
    public string Text { get; private init; }
    public PrintMessage(byte[] buffer) {
        try {
            Text = Encoding.UTF8.GetString(buffer);
        } catch {
            Text = "<Failed to decode message as UTF8>";
        }
    }
}

class BeginMessage : ISendableMessage {
    public byte Opcode => 100;
    public required string Script;
    public byte[] Serialize() {
        return Encoding.UTF8.GetBytes(Script + '\0');
    }
}

class SetDeviceNumbersMessage : ISendableMessage {
    public byte Opcode => 101;
    public required Dictionary<nint, DeviceNumber> DeviceNumbers;
    public byte[] Serialize() {
        return [
            .. BitConverter.GetBytes(DeviceNumbers.Count),
            .. DeviceNumbers.SelectMany(x => (IEnumerable<byte>)[
                ..BitConverter.GetBytes((uint)x.Key),
                ..BitConverter.GetBytes((uint)x.Value),
            ]),
        ];
    }
}

class PipeServer : IDisposable {
    public required Action<IReceiveableMessage> OnMessage;
    public required Action<Exception> OnError;

    private readonly NamedPipeServerStream readPipe;
    private readonly NamedPipeServerStream writePipe;
    private readonly SemaphoreSlim writeSemaphore = new(1);
    private bool disposedValue;

    public PipeServer() {
        readPipe = new NamedPipeServerStream(
            "XInputHookPipeClientToServer",
            PipeDirection.In,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        writePipe = new NamedPipeServerStream(
            "XInputHookPipeServerToClient",
            PipeDirection.Out,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        readPipe.WaitForConnection();
        writePipe.WaitForConnection();
        RunReader();
    }

    private async void RunReader() {
        try {
            while (true) {
                var lenBytes = new byte[4];
                await readPipe.ReadExactlyAsync(lenBytes, 0, lenBytes.Length);
                var len = BitConverter.ToInt32(lenBytes, 0);

                var opcodeBytes = new byte[1];
                await readPipe.ReadExactlyAsync(opcodeBytes, 0, opcodeBytes.Length);
                var opcode = opcodeBytes[0];

                var buffer = new byte[len - 5];
                await readPipe.ReadExactlyAsync(buffer, 0, buffer.Length);

                switch (opcode) {
                    case 1: OnMessage(new PrintMessage(buffer)); break;
                    default: throw new InvalidDataException($"Unknown message opcode: {opcode}");
                }
            }
        } catch (Exception e) {
            OnError(e);
        }
    }

    public async void WriteMessage(ISendableMessage message) {
        try {
            var data = message.Serialize();
            await writeSemaphore.WaitAsync();
            await writePipe.WriteAsync(BitConverter.GetBytes(data.Length + 5));
            await writePipe.WriteAsync(new byte[] { message.Opcode });
            await writePipe.WriteAsync(data);
            await writePipe.FlushAsync();
            writeSemaphore.Release();
        } catch (Exception e) {
            writeSemaphore.Release();
            OnError(e);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                readPipe.Dispose();
                writePipe.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
