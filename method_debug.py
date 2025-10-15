#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import socket
import json
import time

def test_simple_ping():
    """测试简单的 ping 请求"""
    print("测试简单的 ping 请求...")
    
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.settimeout(5)
        s.connect(('127.0.0.1', 5001))
        
        # 发送最简单的 ping 请求
        request = '{"jsonrpc":"2.0","method":"ping","id":1}'
        s.sendall(request.encode('utf-8'))
        print(f"发送: {request}")
        
        # 接收响应
        response = s.recv(1024)
        print(f"收到: {response}")
        
        # 尝试解析
        try:
            parsed = json.loads(response.decode('utf-8'))
            print(f"解析结果: {parsed}")
            
            if 'error' in parsed:
                print(f"错误: {parsed['error']}")
            elif 'result' in parsed:
                print(f"成功: {parsed['result']}")
                
        except json.JSONDecodeError as e:
            print(f"JSON 解析失败: {e}")
        
        s.close()
        return True
        
    except Exception as e:
        print(f"测试失败: {e}")
        return False

def test_different_formats():
    """测试不同的 JSON 格式"""
    print("\n测试不同的 JSON 格式...")
    
    formats = [
        '{"jsonrpc":"2.0","method":"ping","id":1}',
        '{"jsonrpc": "2.0", "method": "ping", "id": 1}',
        '{"method":"ping","jsonrpc":"2.0","id":1}',
        '{"jsonrpc":"2.0","method":"ping","id":1,"params":[]}'
    ]
    
    for i, request in enumerate(formats, 1):
        print(f"\n格式 {i}: {request}")
        
        try:
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.settimeout(3)
            s.connect(('127.0.0.1', 5001))
            
            s.sendall(request.encode('utf-8'))
            response = s.recv(1024)
            
            try:
                parsed = json.loads(response.decode('utf-8'))
                if 'result' in parsed:
                    print(f"✓ 成功: {parsed['result']}")
                else:
                    print(f"✗ 错误: {parsed.get('error', 'Unknown error')}")
            except:
                print(f"✗ 解析失败: {response}")
            
            s.close()
            
        except Exception as e:
            print(f"✗ 连接失败: {e}")

def main():
    print("PocoInjector 方法检测调试工具")
    print("=" * 40)
    
    # 测试简单 ping
    test_simple_ping()
    
    # 测试不同格式
    test_different_formats()
    
    print("\n调试完成")
    print("请检查 BepInEx 控制台是否有详细日志")

if __name__ == "__main__":
    main()
