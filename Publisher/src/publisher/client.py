import asyncio, base64, sys, grpc, signal
from collections.abc import AsyncIterator
# ALEGE varianta corectă în funcție de numele fișierelor generate:
from generated import Publish_pb2 as pb               # sau Publish_pb2
from generated import Publish_pb2_grpc as pb_grpc     # sau Publish_pb2_grpc
from .config import BROKER_TARGET, USE_TLS

def _make_channel(target: str) -> grpc.aio.Channel:
    return grpc.aio.secure_channel(target, grpc.ssl_channel_credentials()) if USE_TLS else grpc.aio.insecure_channel(target)

async def publish_once(topic: str, content_bytes: bytes, timeout_sec: float = 5.0) -> bool:
    channel = _make_channel(BROKER_TARGET)
    stub = pb_grpc.PublisherStub(channel)
    req = pb.PublishRequest(topic=topic, content=content_bytes.decode("utf-8", "ignore"))
    try:
        resp = await asyncio.wait_for(stub.PublishMessage(req), timeout=timeout_sec)
        return resp.success
    finally:
        await channel.close()

async def _stdin_lines() -> AsyncIterator[bytes]:
    while True:
        line = await asyncio.to_thread(sys.stdin.buffer.readline)
        if not line:
            break
        yield line.rstrip(b"\r\n")

async def publish_stdin(topic: str, b64: bool, timeout_sec: float = 5.0) -> int:
    """
    Deschide un singur canal și publică fiecare linie introdusă în terminal ca mesaj separat.
    Returnează codul de ieșire (0 = OK, 1 = eroare).
    """
    channel = _make_channel(BROKER_TARGET)
    stub = pb_grpc.PublisherStub(channel)

    # Mesaje utile
    print(f"[publisher] connected to {BROKER_TARGET}, TLS={USE_TLS}")
    print("[publisher] type lines to publish; Ctrl+C or EOF to stop")

    # Închidere grațioasă pe Ctrl+C/SIGTERM
    stop = asyncio.Event()
    def _sigint(*_): stop.set()
    try:
        loop = asyncio.get_running_loop()
        for sig in (signal.SIGINT, signal.SIGTERM):
            try:
                loop.add_signal_handler(sig, _sigint)
            except NotImplementedError:
                pass
    except RuntimeError:
        pass

    exit_code = 0
    try:
        async for raw in _stdin_lines():
            if stop.is_set():
                break
            data = base64.b64encode(raw) if b64 else raw
            req = pb.PublishRequest(topic=topic, content=data.decode("utf-8", "ignore"))
            try:
                resp = await asyncio.wait_for(stub.PublishMessage(req), timeout=timeout_sec)
                print("[OK]" if resp.success else "[ERR]", flush=True)
                if not resp.success:
                    exit_code = 1
            except asyncio.TimeoutError:
                print("[ERR] timeout", flush=True)
                exit_code = 1
            except grpc.aio.AioRpcError as e:
                print(f"[ERR] gRPC {e.code().name}: {e.details()}", flush=True)
                exit_code = 1
            except asyncio.CancelledError:
                # se întâmplă la shutdown; ieșim fără zgomot
                break
    except KeyboardInterrupt:
        # capturat dacă ai oprit înainte de a intra în bucla async
        pass
    finally:
        try:
            await channel.close()
        finally:
            print("[publisher] connection closed")

    return exit_code

async def _main():
    import argparse
    p = argparse.ArgumentParser()
    p.add_argument("--topic", required=True)
    p.add_argument("--message")
    p.add_argument("--file")
    p.add_argument("--b64", action="store_true")
    p.add_argument("--timeout", type=float, default=5.0)
    p.add_argument("--stdin", action="store_true", help="citește și publică în timp real din terminal")
    args = p.parse_args()

    if args.stdin:
        code = await publish_stdin(args.topic, args.b64, args.timeout)
        sys.exit(code)

    # modul one-shot rămâne disponibil
    data = open(args.file, "rb").read() if args.file else (args.message or "").encode()
    if args.b64:
        data = base64.b64encode(data)
    ok = await publish_once(args.topic, data, args.timeout)
    print("[OK] published" if ok else "[ERR] failed")
    sys.exit(0 if ok else 1)

if __name__ == "__main__":
    try:
        asyncio.run(_main())
    except KeyboardInterrupt:
        # ieșire curată fără traceback
        print("\n[publisher] connection closed")
        sys.exit(0)
