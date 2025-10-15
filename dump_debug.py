#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import socket
import json
import time

def test_dump_simple():
    """测试简单的 dump 请求"""
    print("测试 dump 方法...")
    
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.settimeout(10)
        s.connect(('127.0.0.1', 5001))
        
        # 发送 dump 请求
        request = '{"jsonrpc":"2.0","method":"dump","id":1}'
        s.sendall(request.encode('utf-8'))
        print(f"发送: {request}")
        
        # 接收响应
        print("等待响应...")
        response = b""
        while True:
            chunk = s.recv(4096)
            if not chunk:
                break
            response += chunk
            if len(chunk) < 4096:
                break
        print(f"收到原始数据长度: {len(response)}")
        
        # 尝试解析
        try:
            parsed = json.loads(response.decode('utf-8'))
            print(f"解析成功!")
            
            if 'error' in parsed:
                print(f"错误: {parsed['error']}")
            elif 'result' in parsed:
                result = parsed['result']
                if isinstance(result, list):
                    print(f"成功: 返回 {len(result)} 个游戏对象")
                    if len(result) > 0:
                        print(f"第一个对象: {result[0]}")
                else:
                    print(f"结果类型: {type(result)}")
                    print(f"结果: {result}")
                    
        except json.JSONDecodeError as e:
            print(f"JSON 解析失败: {e}")
            print(f"原始数据前100字符: {response[:100]}")
            print(f"原始数据后100字符: {response[-100:]}")
        
        s.close()
        return True
        
    except Exception as e:
        print(f"测试失败: {e}")
        return False

def test_dump_with_params():
    """测试带参数的 dump 请求"""
    print("\n测试带参数的 dump 方法...")
    
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.settimeout(10)
        s.connect(('127.0.0.1', 5001))
        
        # 发送带参数的 dump 请求
        request = '{"jsonrpc":"2.0","method":"dump","id":1,"params":[]}'
        s.sendall(request.encode('utf-8'))
        print(f"发送: {request}")
        
        # 接收响应
        print("等待响应...")
        response = b""
        while True:
            chunk = s.recv(4096)
            if not chunk:
                break
            response += chunk
            if len(chunk) < 4096:
                break
        print(f"收到原始数据长度: {len(response)}")
        
        # 尝试解析
        try:
            parsed = json.loads(response.decode('utf-8'))
            print(f"解析成功!")
            
            if 'error' in parsed:
                print(f"错误: {parsed['error']}")
            elif 'result' in parsed:
                result = parsed['result']
                if isinstance(result, list):
                    print(f"成功: 返回 {len(result)} 个游戏对象")
                else:
                    print(f"结果: {result}")
                    
        except json.JSONDecodeError as e:
            print(f"JSON 解析失败: {e}")
            print(f"原始数据前100字符: {response[:100]}")
        
        s.close()
        return True
        
    except Exception as e:
        print(f"测试失败: {e}")
        return False

def main():
    print("PocoInjector dump 方法调试工具")
    print("=" * 40)
    
    # 测试简单 dump
    test_dump_simple()
    
    # 测试带参数的 dump
    test_dump_with_params()
    
    print("\n调试完成")

if __name__ == "__main__":
    main()
