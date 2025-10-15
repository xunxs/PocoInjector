#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import socket
import time
import json

HOST = '127.0.0.1'
PORT = 5001

def test_poco_connection():
    """测试 PocoInjector 插件的连接"""
    print("PocoInjector 连接测试工具")
    print("=" * 50)
    
    try:
        print(f"正在尝试连接到 {HOST}:{PORT}...")
        
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            # 设置超时
            s.settimeout(5)
            
            # 连接到服务器
            s.connect((HOST, PORT))
            print(f"成功连接到 {HOST}:{PORT}")
            
            # 发送一个简单的 ping 请求
            ping_request = {
                "jsonrpc": "2.0",
                "method": "ping",
                "id": 1
            }
            
            message = json.dumps(ping_request)
            s.sendall(message.encode('utf-8'))
            print(f"发送请求: {message}")
            
            # 接收响应
            data = s.recv(1024)
            response = data.decode('utf-8')
            print(f"收到响应: {response}")
            
            # 解析响应
            try:
                response_data = json.loads(response)
                if "result" in response_data:
                    print(f"服务响应: {response_data['result']}")
                    print("Poco 服务运行正常！")
                    return True
                else:
                    print("响应格式异常")
                    return False
            except json.JSONDecodeError:
                print("响应不是有效的 JSON 格式")
                return False
                
    except ConnectionRefusedError:
        print(f"连接被拒绝: 确保游戏已运行且 PocoInjector 插件已加载")
        print("   检查 BepInEx 日志确认插件是否成功启动")
        return False
        
    except socket.timeout:
        print(f"连接超时: 服务器可能没有响应")
        return False
        
    except Exception as e:
        print(f"发生错误: {e}")
        return False

if __name__ == "__main__":
    print("PocoInjector 连接测试")
    print("确保游戏正在运行且插件已加载")
    print("-" * 50)
    
    # 基本连接测试
    success = test_poco_connection()
    
    if success:
        print("\n" + "=" * 50)
        print("基本连接测试通过！")
    else:
        print("\n" + "=" * 50)
        print("连接测试失败")
        print("\n故障排除建议:")
        print("1. 确认游戏正在运行")
        print("2. 检查 BepInEx 日志确认插件加载成功")
        print("3. 确认端口 5001 没有被其他程序占用")
        print("4. 尝试重启游戏")
