import socket
import json
import time


def send_rpc(method: str, params=None, host="127.0.0.1", port=5001, timeout=2.0) -> str:
    if params is None:
        params = []
    req = {
        "jsonrpc": "2.0",
        "id": 1,
        "method": method,
        "params": params,
    }
    data = json.dumps(req).encode("utf-8")

    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(timeout)
    s.connect((host, port))
    s.sendall(data)

    # 读取分片响应，直到短暂停顿无数据
    s.settimeout(0.3)
    chunks = []
    while True:
        try:
            buf = s.recv(4096)
            if not buf:
                break
            chunks.append(buf)
        except socket.timeout:
            break
    s.close()
    return b"".join(chunks).decode("utf-8", errors="replace")


def try_parse_json(text: str):
    try:
        return json.loads(text)
    except Exception:
        return None


def main():
    print("[test] call listPages ...")
    rsp = send_rpc("listPages")
    obj = try_parse_json(rsp)
    if not obj or "result" not in obj:
        print("listPages raw:")
        print(rsp)
    else:
        pages = obj["result"]
        print(f"pages: {len(pages)}")
        for i, p in enumerate(pages[:10]):
            print(f"  {i}. {p.get('name')} | active={p.get('active')} | path={p.get('fullPath')}")

    time.sleep(0.2)

    print("\n[test] call getPageButtons ...")
    rsp2 = send_rpc("getPageButtons")
    obj2 = try_parse_json(rsp2)
    if not obj2 or "result" not in obj2:
        print("getPageButtons raw:")
        print(rsp2)
    else:
        pages = obj2["result"]
        print(f"pages with buttons: {len(pages)}")
        for i, page in enumerate(pages[:5]):
            buttons = page.get("buttons", [])
            print(f"  {i}. {page.get('pageName')} | buttons={len(buttons)} | path={page.get('pageFullPath')}")
            for j, b in enumerate(buttons[:5]):
                rp = b.get("rectPosition") or {}
                print(f"     - {j}. {b.get('name')} | active={b.get('active')} | inter={b.get('interactable')} | pos=({rp.get('x')},{rp.get('y')})")


if __name__ == "__main__":
    main()


